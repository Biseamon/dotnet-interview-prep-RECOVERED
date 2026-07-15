using InterviewPrep.Application.Grading;
using InterviewPrep.CodeExecution;
using InterviewPrep.Domain;
using InterviewPrep.Infrastructure.Data.Seeding;
using Xunit;

namespace InterviewPrep.Tests;

// Guards every authored exercise: its OWN reference solution, run against its OWN
// hidden harness, must pass all tests. If a harness is buggy or a reference solution
// is wrong, this fails immediately — so authoring content stays safe and fast.
public class ContentIntegrityTests
{
    // One runner per language, matching the app's DI registration.
    private readonly IReadOnlyDictionary<ExerciseLanguage, IExerciseRunner> _runners =
        new IExerciseRunner[] { new RoslynCodeRunner(), new SqlRunner(), new RuleRunner() }
            .ToDictionary(r => r.Language);

    // xUnit MemberData source: one row per (topicSlug, exerciseSlug) across the catalog.
    public static IEnumerable<object[]> AllExercises()
    {
        foreach (var topic in SeedCatalog.All)
            foreach (var lesson in topic.Lessons)
                foreach (var ex in lesson.Exercises)
                    yield return [topic.Slug, ex.Slug];
    }

    // Guards that every authored exercise has an ELI5 explanation — so none of the
    // learning-material blurbs were missed as content grows.
    [Fact]
    public void Every_exercise_has_an_explanation()
    {
        var missing = SeedCatalog.All
            .SelectMany(t => t.Lessons)
            .SelectMany(l => l.Exercises)
            .Where(e => string.IsNullOrWhiteSpace(Explanations.For(e.Slug)))
            .Select(e => e.Slug)
            .ToList();

        Assert.True(missing.Count == 0, "Exercises missing an explanation: " + string.Join(", ", missing));
    }

    // Guards against a subtle authoring trap: the STARTER code (what the learner begins
    // from) omitting a `using` that the intended solution needs. The reference-solution
    // test above never catches this because the reference carries its own usings — but a
    // learner following the hints would hit a CS0246/CS1061 (e.g. `.Select` without
    // `using System.Linq;`). Invariant: the starter's usings ⊇ the reference's usings.
    [Fact]
    public void Starter_code_has_every_using_the_reference_needs()
    {
        static HashSet<string> Usings(string code) => code
            .Split('\n')
            .Select(l => l.Trim())
            .Where(l => l.StartsWith("using ") && l.EndsWith(";") && !l.Contains('='))
            .ToHashSet();

        var violations = new List<string>();
        foreach (var ex in SeedCatalog.All.SelectMany(t => t.Lessons).SelectMany(l => l.Exercises))
        {
            if (!ex.Language.Equals("CSharp", StringComparison.OrdinalIgnoreCase)) continue;
            var missing = Usings(ex.ReferenceSolution).Except(Usings(ex.StarterCode)).ToList();
            if (missing.Count > 0)
                violations.Add($"{ex.Slug}: starter is missing [{string.Join(", ", missing)}]");
        }

        Assert.True(violations.Count == 0,
            "Starter code missing usings the reference needs:\n" + string.Join("\n", violations));
    }

    [Theory]
    [MemberData(nameof(AllExercises))]
    public void Reference_solution_passes_its_own_harness(string topicSlug, string exerciseSlug)
    {
        // Locate the exercise from the catalog.
        var exercise = SeedCatalog.All
            .First(t => t.Slug == topicSlug)
            .Lessons.SelectMany(l => l.Exercises)
            .First(e => e.Slug == exerciseSlug);

        var language = Enum.Parse<ExerciseLanguage>(exercise.Language, ignoreCase: true);
        var result = _runners[language].Run(new RunRequest(
            UserSource: exercise.ReferenceSolution,
            HarnessCode: exercise.HarnessCode,
            TimeoutSeconds: exercise.TimeoutSeconds ?? 5));

        Assert.True(
            result.Status == GradeStatus.Passed,
            $"'{exerciseSlug}' reference solution did not pass: status={result.Status}, " +
            $"{result.PassedCount}/{result.TotalCount}. " +
            string.Join(" | ", result.TestResults
                .Where(r => !r.Passed)
                .Select(r => $"{r.Name}: exp={r.Expected} act={r.Actual} ex={r.ExceptionMessage}")) +
            string.Join(" | ", result.CompileErrors.Select(e => $"{e.Id} {e.Message}")));
    }
}
