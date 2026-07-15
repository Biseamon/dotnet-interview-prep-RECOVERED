using InterviewPrep.Domain.Entities;

namespace InterviewPrep.Application.Content;

// Read/write access to content + attempts, abstracted away from EF Core. Keeps
// the DbContext (Infrastructure) out of the Application/API layers. The concrete
// EfContentRepository lives in Infrastructure.
public interface IContentRepository
{
    // Topics with their lessons AND exercises (for the dashboard's per-topic progress),
    // ordered for display.
    Task<IReadOnlyList<Topic>> GetTopicsAsync(CancellationToken ct);

    // A single topic by slug, including its lessons and their exercises. Null if missing.
    Task<Topic?> GetTopicBySlugAsync(string slug, CancellationToken ct);

    // A single lesson by slug, including its exercises. Null if missing.
    Task<Lesson?> GetLessonBySlugAsync(string slug, CancellationToken ct);

    // A single exercise by slug, including hints + test cases. Null if missing.
    // NOTE: returns the FULL entity (incl. hidden harness/solution). The API layer
    // is responsible for mapping to a trimmed DTO before sending to the client.
    Task<Exercise?> GetExerciseBySlugAsync(string slug, CancellationToken ct);

    // Persist a grading attempt (progress/history).
    Task AddAttemptAsync(Attempt attempt, CancellationToken ct);

    // Most recent attempts for an exercise, newest first.
    Task<IReadOnlyList<Attempt>> GetAttemptsAsync(string exerciseSlug, int limit, CancellationToken ct);

    // Delete all attempts for an exercise — powers the "practice again" reset so the
    // learner can start the exercise fresh. Returns how many were removed.
    Task<int> ClearAttemptsAsync(string exerciseSlug, CancellationToken ct);

    // Slugs of every exercise that has at least one PASSED attempt — i.e. "solved".
    // Powers the checkmarks/progress indicators in the UI.
    Task<IReadOnlyList<string>> GetSolvedExerciseSlugsAsync(CancellationToken ct);

    // Player stats (XP, level, streak, weekly XP, quests, badges) derived from the
    // attempt + drill history. Powers the gamification header/Progress screen.
    Task<GamificationStats> GetGamificationStatsAsync(CancellationToken ct);

    // --- Hearts (server-persisted, single implicit player) ---
    Task<PlayerDto> GetPlayerAsync(CancellationToken ct);
    Task<PlayerDto> LoseHeartAsync(CancellationToken ct);
    Task<PlayerDto> RefillHeartsAsync(CancellationToken ct);

    // --- Interview drill (MCQ quiz) ---
    Task<IReadOnlyList<DrillQuestionDto>> GetDrillQuestionsAsync(int count, CancellationToken ct);
    Task<DrillCompleteResult> RecordDrillResultAsync(int correctCount, int total, CancellationToken ct);

    // --- Achievements (evaluated from the event log, then persisted once earned) ---
    Task<IReadOnlyList<AchievementDto>> GetAchievementsAsync(CancellationToken ct);
}
