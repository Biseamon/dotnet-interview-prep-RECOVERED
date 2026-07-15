using InterviewPrep.Api.Contracts;
using InterviewPrep.Application.Content;
using InterviewPrep.Application.Grading;
using InterviewPrep.Domain.Grading;

namespace InterviewPrep.Api.Endpoints;

// All content + grading HTTP endpoints, grouped under /api. Handlers are thin:
// they call the repository/grading service and map to DTOs. Keeping them here
// (not inline in Program.cs) keeps the composition root readable.
public static class ContentEndpoints
{
    public static IEndpointRouteBuilder MapContentEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api");

        // --- Topics dashboard ---
        api.MapGet("/topics", async (IContentRepository repo, CancellationToken ct) =>
        {
            var topics = await repo.GetTopicsAsync(ct);
            return Results.Ok(topics.Select(t => t.ToDto()));
        });

        // --- Topic detail (lessons + their exercises) for the topic page ---
        api.MapGet("/topics/{slug}", async (string slug, IContentRepository repo, CancellationToken ct) =>
        {
            var topic = await repo.GetTopicBySlugAsync(slug, ct);
            return topic is null ? Results.NotFound() : Results.Ok(topic.ToDetailDto());
        });

        // --- Lesson (with its exercises) ---
        api.MapGet("/lessons/{slug}", async (string slug, IContentRepository repo, CancellationToken ct) =>
        {
            var lesson = await repo.GetLessonBySlugAsync(slug, ct);
            return lesson is null ? Results.NotFound() : Results.Ok(lesson.ToDto());
        });

        // --- Exercise detail (trimmed: no harness/solution) ---
        api.MapGet("/exercises/{slug}", async (string slug, IContentRepository repo, CancellationToken ct) =>
        {
            var exercise = await repo.GetExerciseBySlugAsync(slug, ct);
            return exercise is null ? Results.NotFound() : Results.Ok(exercise.ToDto());
        });

        // --- Grade a submission (the hot path) ---
        api.MapPost("/exercises/{slug}/grade",
            async (string slug, GradeRequest body, IGradingService grading, CancellationToken ct) =>
            {
                var result = await grading.GradeAsync(slug, body.Source, ct);
                return result is null ? Results.NotFound() : Results.Ok(result);
            });

        // --- Progressive hint reveal: /hints/1, /hints/2, ... (1-based) ---
        api.MapGet("/exercises/{slug}/hints/{n:int}",
            async (string slug, int n, IContentRepository repo, CancellationToken ct) =>
            {
                var exercise = await repo.GetExerciseBySlugAsync(slug, ct);
                if (exercise is null) return Results.NotFound();

                var hint = exercise.Hints.FirstOrDefault(h => h.Order == n);
                return hint is null
                    ? Results.NotFound()
                    : Results.Ok(new { order = hint.Order, text = hint.Text, total = exercise.Hints.Count });
            });

        // --- Reference solution: gated. Client only calls this after pass/give-up. ---
        api.MapGet("/exercises/{slug}/solution",
            async (string slug, IContentRepository repo, CancellationToken ct) =>
            {
                var exercise = await repo.GetExerciseBySlugAsync(slug, ct);
                return exercise is null
                    ? Results.NotFound()
                    : Results.Ok(new { solution = exercise.ReferenceSolution });
            });

        // --- Attempt history (newest first) ---
        api.MapGet("/exercises/{slug}/attempts",
            async (string slug, IContentRepository repo, CancellationToken ct) =>
            {
                var attempts = await repo.GetAttemptsAsync(slug, limit: 20, ct);
                return Results.Ok(attempts.Select(a => new
                {
                    a.Status,
                    a.PassedCount,
                    a.TotalCount,
                    a.CreatedAtUtc,
                }));
            });

        // --- Progress: which exercises are solved (have a passing attempt) ---
        api.MapGet("/progress", async (IContentRepository repo, CancellationToken ct) =>
        {
            var solved = await repo.GetSolvedExerciseSlugsAsync(ct);
            return Results.Ok(new { solved });
        });

        // --- Gamification: XP, level, belt, streak, weekly XP, quests, badges ---
        api.MapGet("/gamification", async (IContentRepository repo, CancellationToken ct) =>
            Results.Ok(await repo.GetGamificationStatsAsync(ct)));

        // --- Hearts (server-persisted single player) ---
        api.MapGet("/player", async (IContentRepository repo, CancellationToken ct) =>
            Results.Ok(await repo.GetPlayerAsync(ct)));
        api.MapPost("/player/lose-heart", async (IContentRepository repo, CancellationToken ct) =>
            Results.Ok(await repo.LoseHeartAsync(ct)));
        api.MapPost("/player/refill", async (IContentRepository repo, CancellationToken ct) =>
            Results.Ok(await repo.RefillHeartsAsync(ct)));

        // --- Interview drill (MCQ quiz) ---
        api.MapGet("/drill/questions", async (int? count, IContentRepository repo, CancellationToken ct) =>
            Results.Ok(await repo.GetDrillQuestionsAsync(count is > 0 and <= 20 ? count.Value : 5, ct)));
        api.MapPost("/drill/complete", async (DrillCompleteRequest body, IContentRepository repo, CancellationToken ct) =>
            Results.Ok(await repo.RecordDrillResultAsync(body.CorrectCount, body.Total, ct)));

        // --- Achievements ---
        api.MapGet("/achievements", async (IContentRepository repo, CancellationToken ct) =>
            Results.Ok(await repo.GetAchievementsAsync(ct)));

        // --- Reset / practice again: clear this exercise's attempt history ---
        api.MapDelete("/exercises/{slug}/attempts",
            async (string slug, IContentRepository repo, CancellationToken ct) =>
            {
                var removed = await repo.ClearAttemptsAsync(slug, ct);
                return Results.Ok(new { cleared = removed });
            });

        return app;
    }
}
