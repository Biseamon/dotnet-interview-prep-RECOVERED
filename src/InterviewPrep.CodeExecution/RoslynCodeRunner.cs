using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using Basic.Reference.Assemblies;
using InterviewPrep.Application.Grading;
using InterviewPrep.Domain;
using InterviewPrep.Domain.Grading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace InterviewPrep.CodeExecution;

// The in-process grader. Compiles [user code + exercise harness + assert shim] into
// one in-memory assembly, runs the harness in a collectible load context under a
// timeout, and maps the outcome to a GradeResult.
//
// SECURITY NON-GOAL (documented): this is NOT a security sandbox. Arbitrary C# runs
// in-process and could touch the filesystem, call Environment.Exit, etc. That is
// acceptable for a LOCAL, single-user study app. The ICodeRunner seam lets us swap
// in an out-of-process (kill-able) runner later without touching other layers.
public sealed class RoslynCodeRunner : IExerciseRunner
{
    public ExerciseLanguage Language => ExerciseLanguage.CSharp;

    // Distinct file paths let us attribute each compiler diagnostic to its source.
    private const string UserPath = "Submission.cs";
    private const string HarnessPath = "Harness.cs";
    private const string ShimPath = "Shim.cs";

    private static readonly CSharpParseOptions ParseOptions =
        new(LanguageVersion.CSharp13);

    // Clean net9.0 reference set (from Basic.Reference.Assemblies) — NOT the host's
    // loaded assemblies, so compilation behaves consistently everywhere.
    private static readonly IReadOnlyList<MetadataReference> References =
        Net90.References.All;

    public GradeResult Run(RunRequest request)
    {
        // 1) Parse the three sources as separate trees (distinct paths for mapping).
        var trees = new[]
        {
            CSharpSyntaxTree.ParseText(request.UserSource, ParseOptions, path: UserPath),
            CSharpSyntaxTree.ParseText(request.HarnessCode, ParseOptions, path: HarnessPath),
            CSharpSyntaxTree.ParseText(GraderShim.Source, ParseOptions, path: ShimPath),
        };

        // 2) Compile to an in-memory DLL (Release, nullable enabled, no unsafe).
        var compilation = CSharpCompilation.Create(
            assemblyName: "Submission_" + Guid.NewGuid().ToString("N"),
            syntaxTrees: trees,
            references: References,
            options: new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                optimizationLevel: OptimizationLevel.Release,
                allowUnsafe: false,
                nullableContextOptions: NullableContextOptions.Enable));

        using var peStream = new MemoryStream();
        var emit = compilation.Emit(peStream);

        if (!emit.Success)
            return MapCompileFailure(emit.Diagnostics);

        peStream.Seek(0, SeekOrigin.Begin);

        // 3) Load + run under a collectible context with a hard timeout.
        return LoadAndRun(peStream, request.TimeoutSeconds);
    }

    // Turn a failed compilation into either user-facing compile errors or (if only
    // the shim broke — our bug) an exception.
    private static GradeResult MapCompileFailure(IEnumerable<Diagnostic> diagnostics)
    {
        var errors = diagnostics
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        var mapped = new List<CompileError>();
        foreach (var d in errors)
        {
            var path = d.Location.SourceTree?.FilePath;

            if (path == UserPath)
            {
                // A real error in the learner's code — map with precise position.
                var span = d.Location.GetLineSpan();
                mapped.Add(new CompileError(
                    Severity: "Error",
                    Id: d.Id,
                    Message: d.GetMessage(),
                    // Roslyn positions are 0-based; Monaco markers are 1-based → +1.
                    Line: span.StartLinePosition.Line + 1,
                    Column: span.StartLinePosition.Character + 1,
                    EndLine: span.EndLinePosition.Line + 1,
                    EndColumn: span.EndLinePosition.Character + 1));
            }
            else if (path == HarnessPath)
            {
                // Error in the harness usually means the learner's PUBLIC API doesn't
                // match what's expected (wrong/missing signature). Surface it without a
                // precise editor location, with a clarifying prefix.
                mapped.Add(new CompileError(
                    Severity: "Error",
                    Id: d.Id,
                    Message: "Your code's shape doesn't match what's expected: " + d.GetMessage(),
                    Line: 1, Column: 1, EndLine: 1, EndColumn: 1));
            }
        }

        if (mapped.Count > 0)
            return GradeResult.FromCompileErrors(mapped);

        // Only the shim failed → this is a server/authoring bug, not the user's fault.
        var detail = string.Join("; ", errors.Select(e => e.GetMessage()));
        throw new InvalidOperationException("Grader shim failed to compile: " + detail);
    }

    private static GradeResult LoadAndRun(MemoryStream peStream, int timeoutSeconds)
    {
        // Collectible context so the compiled assembly can be UNLOADED after the run,
        // preventing an assembly leak per submission. Tracked via WeakReference to
        // verify it actually collects.
        var alc = new CollectibleLoadContext();
        var alcRef = new WeakReference(alc);

        string? json = null;
        Exception? runError = null;
        try
        {
            var assembly = alc.LoadFromStream(peStream);

            // Run the harness on a dedicated BACKGROUND thread. Why not a Task +
            // CancellationToken? A runaway `while(true)` never checks a token — only a
            // thread we can abandon (Thread.Abort no longer exists) bounds the wait.
            var thread = new Thread(() =>
            {
                try
                {
                    var harnessType = assembly.GetType("__Harness")
                        ?? throw new InvalidOperationException("Harness type '__Harness' not found.");
                    var run = harnessType.GetMethod("Run", BindingFlags.Public | BindingFlags.Static)
                        ?? throw new InvalidOperationException("'__Harness.Run' not found.");

                    // Returns a JSON string (primitives only — safe to cross the ALC boundary).
                    json = (string?)run.Invoke(null, null);
                }
                catch (Exception ex)
                {
                    // Unwrap reflection's TargetInvocationException to the real cause.
                    runError = ex is TargetInvocationException tie && tie.InnerException is not null
                        ? tie.InnerException
                        : ex;
                }
            })
            {
                IsBackground = true, // won't keep the process alive if abandoned on timeout
                Name = "grader-run",
            };

            thread.Start();
            if (!thread.Join(TimeSpan.FromSeconds(timeoutSeconds)))
                return GradeResult.Timeout(); // abandon the runaway thread; it dies with the process

            if (runError is not null)
                return GradeResult.RuntimeError(runError.Message);

            if (string.IsNullOrEmpty(json))
                return GradeResult.RuntimeError("Harness produced no result.");

            var results = HarnessJson.Parse(json);
            return GradeResult.FromTestResults(results);
        }
        finally
        {
            // Request unload, then force GC so the collectible ALC is actually reclaimed.
            // (Everything ALC-bound — assembly/type/method — was local and is now out of
            // scope; only the `json` string, a core-lib type, survives.)
            alc.Unload();
            for (int i = 0; i < 8 && alcRef.IsAlive; i++)
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }
    }

    // A collectible load context. Returning null from Load() means "fall back to the
    // default context" for framework assemblies — we only host the one submission DLL.
    private sealed class CollectibleLoadContext : AssemblyLoadContext
    {
        public CollectibleLoadContext() : base(isCollectible: true) { }
        protected override Assembly? Load(AssemblyName assemblyName) => null;
    }

    // Warms up Roslyn (JIT + first-compile cost ~1-2s) so the learner's first real
    // submission isn't slow. Called once at startup.
    [MethodImpl(MethodImplOptions.NoInlining)]
    public static void Warmup()
    {
        var runner = new RoslynCodeRunner();
        runner.Run(new RunRequest(
            UserSource: "public static class Solution { }",
            HarnessCode: "public static class __Harness { public static string Run() => new HarnessReport().ToJson(); }",
            TimeoutSeconds: 5));
    }
}
