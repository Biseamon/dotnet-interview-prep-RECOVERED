using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace InterviewPrep.Tests;

// Integration tests: boot the REAL API in-memory with WebApplicationFactory and
// exercise it over HTTP end-to-end (routing → DI → EF Core/Postgres → Roslyn grader).
// Requires the Postgres container to be running (docker compose up -d).
public class GradingApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public GradingApiTests(WebApplicationFactory<Program> factory)
    {
        // The factory starts the app using its normal configuration (Development),
        // which points at the local Postgres and seeds content on startup.
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_returns_ok()
    {
        var response = await _client.GetAsync("/api/health");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Topics_include_seeded_async_topic()
    {
        var json = await _client.GetStringAsync("/api/topics");
        Assert.Contains("\"slug\":\"async\"", json);
    }

    [Fact]
    public async Task Exercise_detail_does_not_leak_solution_or_harness()
    {
        // The trimmed DTO must never carry the answer or the hidden test harness.
        // (Note: the PROMPT legitimately mentions Task.Yield as a teaching hint, so we
        // assert on the hidden field NAMES / harness marker, not on incidental tokens.)
        var json = await _client.GetStringAsync("/api/exercises/sum-async");
        Assert.DoesNotContain("__Harness", json);       // harness marker never leaks
        Assert.DoesNotContain("referenceSolution", json, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("harnessCode", json, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Correct_submission_grades_as_passed()
    {
        var body = new { source =
            "using System.Threading.Tasks; public static class Solution { " +
            "public static async Task<int> SumAsync(int a, int b){ await Task.Yield(); return a+b; } }" };

        var response = await _client.PostAsJsonAsync("/api/exercises/sum-async/grade", body);
        response.EnsureSuccessStatusCode();

        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.Equal("Passed", doc.RootElement.GetProperty("status").GetString());
    }

    [Fact]
    public async Task Buggy_submission_grades_as_failed_with_diff()
    {
        var body = new { source =
            "using System.Threading.Tasks; public static class Solution { " +
            "public static async Task<int> SumAsync(int a, int b){ await Task.Yield(); return a-b; } }" };

        var response = await _client.PostAsJsonAsync("/api/exercises/sum-async/grade", body);
        var doc = JsonDocument.Parse(await response.Content.ReadAsStringAsync());

        Assert.Equal("Failed", doc.RootElement.GetProperty("status").GetString());
        // First failing case should carry expected vs actual for the results panel.
        var first = doc.RootElement.GetProperty("testResults")[0];
        Assert.Equal("5", first.GetProperty("expected").GetString());
    }

    [Fact]
    public async Task Grading_unknown_exercise_returns_404()
    {
        var response = await _client.PostAsJsonAsync("/api/exercises/nope/grade", new { source = "x" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
