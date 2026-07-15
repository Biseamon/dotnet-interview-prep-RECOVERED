using InterviewPrep.Application.Grading;
using InterviewPrep.CodeExecution;
using InterviewPrep.Domain;
using InterviewPrep.Domain.Grading;
using Xunit;

namespace InterviewPrep.Tests;

// Unit tests for the grading engine — the riskiest, most important component.
// Each test drives the real Roslyn runner end-to-end (compile → run → map).
public class RoslynCodeRunnerTests
{
    private readonly RoslynCodeRunner _runner = new();

    // A harness that checks Solution.Add(2,3) == 5 — reused across tests.
    private const string AddHarness =
        """
        public static class __Harness
        {
            public static string Run()
            {
                var r = new HarnessReport();
                r.Check("Add(2,3) == 5", () => Assert.Equal(5, Solution.Add(2, 3)));
                return r.ToJson();
            }
        }
        """;

    [Fact]
    public void Correct_solution_passes_all_tests()
    {
        var user = "public static class Solution { public static int Add(int a, int b) => a + b; }";

        var result = _runner.Run(new RunRequest(user, AddHarness, TimeoutSeconds: 5));

        Assert.Equal(GradeStatus.Passed, result.Status);
        Assert.Equal(1, result.PassedCount);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public void Wrong_answer_fails_with_expected_and_actual()
    {
        var user = "public static class Solution { public static int Add(int a, int b) => a * b; }"; // bug: multiplies

        var result = _runner.Run(new RunRequest(user, AddHarness, TimeoutSeconds: 5));

        Assert.Equal(GradeStatus.Failed, result.Status);
        var testCase = Assert.Single(result.TestResults);
        Assert.False(testCase.Passed);
        Assert.Equal("5", testCase.Expected); // Add(2,3) expected 5
        Assert.Equal("6", testCase.Actual);   // 2*3 = 6
    }

    [Fact]
    public void Syntax_error_maps_to_compile_error_with_position()
    {
        // Missing closing brace / bad token on line 1.
        var user = "public static class Solution { public static int Add(int a, int b) => a + ; }";

        var result = _runner.Run(new RunRequest(user, AddHarness, TimeoutSeconds: 5));

        Assert.Equal(GradeStatus.CompileError, result.Status);
        Assert.NotEmpty(result.CompileErrors);
        var err = result.CompileErrors[0];
        Assert.StartsWith("CS", err.Id);      // a real C# diagnostic id
        Assert.True(err.Line >= 1);           // 1-based position (Monaco-ready)
        Assert.True(err.Column >= 1);
    }

    [Fact]
    public void Missing_method_surfaces_as_signature_mismatch()
    {
        // Compiles on its own, but lacks Add → harness reference fails to compile.
        var user = "public static class Solution { }";

        var result = _runner.Run(new RunRequest(user, AddHarness, TimeoutSeconds: 5));

        Assert.Equal(GradeStatus.CompileError, result.Status);
        Assert.Contains(result.CompileErrors, e => e.Message.Contains("doesn't match"));
    }

    [Fact]
    public void Infinite_loop_is_bounded_by_timeout()
    {
        var user = "public static class Solution { public static int Add(int a, int b) { while(true){} } }";

        var result = _runner.Run(new RunRequest(user, AddHarness, TimeoutSeconds: 1));

        Assert.Equal(GradeStatus.Timeout, result.Status);
    }

    [Fact]
    public void Uncaught_exception_in_solution_is_reported_as_failed_case()
    {
        var user = "public static class Solution { public static int Add(int a, int b) => throw new System.Exception(\"boom\"); }";

        var result = _runner.Run(new RunRequest(user, AddHarness, TimeoutSeconds: 5));

        // The harness's Check() catches it → the case fails (not a whole-run crash).
        Assert.Equal(GradeStatus.Failed, result.Status);
        var testCase = Assert.Single(result.TestResults);
        Assert.Equal("Exception", testCase.ExceptionType);
        Assert.Equal("boom", testCase.ExceptionMessage);
    }

    [Fact]
    public void Repeated_runs_do_not_leak_or_crash()
    {
        // Exercises the collectible-ALC unload path many times: if assemblies leaked
        // or the context failed to unload, this would balloon memory / eventually fail.
        var user = "public static class Solution { public static int Add(int a, int b) => a + b; }";
        for (int i = 0; i < 25; i++)
        {
            var result = _runner.Run(new RunRequest(user, AddHarness, TimeoutSeconds: 5));
            Assert.Equal(GradeStatus.Passed, result.Status);
        }
    }
}
