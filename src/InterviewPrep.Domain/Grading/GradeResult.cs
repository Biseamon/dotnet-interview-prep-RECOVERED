namespace InterviewPrep.Domain.Grading;

// Result types returned by the grader. These are DOMAIN concepts (the language of
// "grading a submission"), independent of Roslyn or EF — so they live in Domain and
// flow all the way out to the API response. All are immutable records of primitives
// only: nothing here references a compiled Type/assembly, which is what lets the
// grader's AssemblyLoadContext unload cleanly after each run.

// One compiler diagnostic, positioned so the frontend can draw a Monaco marker.
// Line/Column are 1-BASED (Roslyn is 0-based; the grader adds 1 during mapping).
public record CompileError(
    string Severity,   // "Error" / "Warning"
    string Id,         // e.g. "CS0103" — links to C# docs
    string Message,
    int Line,
    int Column,
    int EndLine,
    int EndColumn);

// The outcome of a single test case within a submission run.
public record TestCaseResult(
    string Name,
    bool Passed,
    string? Expected,        // stringified expected value (null if not applicable)
    string? Actual,          // stringified actual value
    string? ExceptionType,   // set if the case threw
    string? ExceptionMessage,
    string? Stdout,          // anything the user's code wrote to Console during this case
    long ElapsedMs);

// The full grading response — one object the API returns and the UI branches on.
public record GradeResult(
    GradeStatus Status,
    IReadOnlyList<CompileError> CompileErrors,
    IReadOnlyList<TestCaseResult> TestResults,
    int PassedCount,
    int TotalCount)
{
    // Convenience factories keep the grader/service call-sites readable.

    public static GradeResult FromCompileErrors(IReadOnlyList<CompileError> errors) =>
        new(GradeStatus.CompileError, errors, [], 0, 0);

    public static GradeResult Timeout() =>
        new(GradeStatus.Timeout, [], [], 0, 0);

    // Runtime failure that isn't tied to a specific test case (e.g. the harness
    // itself threw, or user code crashed the whole run).
    public static GradeResult RuntimeError(string message) =>
        new(GradeStatus.RuntimeError, [],
            [new TestCaseResult("run", false, null, null, "Exception", message, null, 0)],
            0, 0);

    // Build the final result from per-case results, inferring Passed vs Failed.
    public static GradeResult FromTestResults(IReadOnlyList<TestCaseResult> results)
    {
        var passed = results.Count(r => r.Passed);
        var status = passed == results.Count && results.Count > 0
            ? GradeStatus.Passed
            : GradeStatus.Failed;
        return new(status, [], results, passed, results.Count);
    }
}
