using System.Text.Json;
using InterviewPrep.Domain.Grading;

namespace InterviewPrep.CodeExecution;

// Parses the JSON array emitted by the shim's HarnessReport.ToJson() into domain
// TestCaseResult records. We parse with System.Text.Json here in the GRADER's own
// (default) load context — the input is just a string, so nothing from the sandbox
// crosses over. Kept separate from the runner for focused unit testing.
internal static class HarnessJson
{
    public static IReadOnlyList<TestCaseResult> Parse(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var results = new List<TestCaseResult>();

        foreach (var el in doc.RootElement.EnumerateArray())
        {
            results.Add(new TestCaseResult(
                Name: GetString(el, "name") ?? "(unnamed)",
                Passed: el.GetProperty("passed").GetBoolean(),
                Expected: GetString(el, "expected"),
                Actual: GetString(el, "actual"),
                ExceptionType: GetString(el, "exceptionType"),
                ExceptionMessage: GetString(el, "exceptionMessage"),
                Stdout: GetString(el, "stdout"),
                ElapsedMs: el.TryGetProperty("elapsedMs", out var ms) ? ms.GetInt64() : 0));
        }

        return results;
    }

    // Reads a string property, treating JSON null (and absence) as C# null.
    private static string? GetString(JsonElement el, string name) =>
        el.TryGetProperty(name, out var p) && p.ValueKind == JsonValueKind.String
            ? p.GetString()
            : null;
}
