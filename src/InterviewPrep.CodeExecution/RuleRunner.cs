using System.Text.Json;
using System.Text.RegularExpressions;
using InterviewPrep.Application.Grading;
using InterviewPrep.Domain;
using InterviewPrep.Domain.Grading;

namespace InterviewPrep.CodeExecution;

// Grades "config" exercises (Dockerfile, YAML, k8s manifests, docker-compose) by
// checking the learner's text against a set of structural RULES — each a regular
// expression that must match. This can't "run" infrastructure, but it teaches the
// required structure and catches missing/mis-ordered directives.
//
//   HarnessCode JSON: { "rules": [ { "name": "uses a .NET base image",
//                                    "pattern": "FROM\\s+.*dotnet",
//                                    "hidden": false } ] }
public sealed class RuleRunner : IExerciseRunner
{
    public ExerciseLanguage Language => ExerciseLanguage.Config;

    private sealed record Rule(string Name, string Pattern, bool Hidden);
    private sealed record Spec(List<Rule> Rules);

    private static readonly TimeSpan RegexTimeout = TimeSpan.FromSeconds(2);

    public GradeResult Run(RunRequest request)
    {
        var spec = JsonSerializer.Deserialize<Spec>(request.HarnessCode,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Invalid config harness spec.");

        var results = new List<TestCaseResult>();
        foreach (var rule in spec.Rules)
        {
            bool matched;
            try
            {
                // Case-insensitive + multiline so ^/$ work per line; timeout guards bad patterns.
                matched = Regex.IsMatch(request.UserSource, rule.Pattern,
                    RegexOptions.IgnoreCase | RegexOptions.Multiline, RegexTimeout);
            }
            catch (RegexMatchTimeoutException)
            {
                matched = false;
            }

            results.Add(new TestCaseResult(
                Name: rule.Name,
                Passed: matched,
                Expected: matched ? null : "present",
                Actual: matched ? null : "missing",
                ExceptionType: null,
                ExceptionMessage: null,
                Stdout: null,
                ElapsedMs: 0));
        }

        return GradeResult.FromTestResults(results);
    }
}
