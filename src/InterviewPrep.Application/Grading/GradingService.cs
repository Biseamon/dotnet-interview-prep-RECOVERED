using InterviewPrep.Application.Content;
using InterviewPrep.Domain.Entities;
using InterviewPrep.Domain.Grading;

namespace InterviewPrep.Application.Grading;

// Concrete orchestration of a grading run. Deliberately thin: it loads the exercise,
// dispatches to the runner matching the exercise's LANGUAGE (C# / SQL / Config), and
// records history. All the hard work lives in the runners; persistence in the repo.
public sealed class GradingService(
    IContentRepository repository,
    IEnumerable<IExerciseRunner> runners,
    TimeProvider timeProvider) : IGradingService
{
    private const int DefaultTimeoutSeconds = 5;

    // Index the available runners by the language they grade.
    private readonly IReadOnlyDictionary<Domain.ExerciseLanguage, IExerciseRunner> _runners =
        runners.ToDictionary(r => r.Language);

    public async Task<GradeResult?> GradeAsync(string exerciseSlug, string userSource, CancellationToken ct)
    {
        var exercise = await repository.GetExerciseBySlugAsync(exerciseSlug, ct);
        if (exercise is null)
            return null;

        if (!_runners.TryGetValue(exercise.Language, out var runner))
            throw new InvalidOperationException($"No runner registered for language {exercise.Language}.");

        var request = new RunRequest(
            UserSource: userSource,
            HarnessCode: exercise.HarnessCode,
            TimeoutSeconds: exercise.TimeoutSeconds ?? DefaultTimeoutSeconds);

        var result = runner.Run(request);

        // Record the attempt for progress/history. TimeProvider keeps this testable.
        await repository.AddAttemptAsync(new Attempt
        {
            ExerciseId = exercise.Id,
            SubmittedCode = userSource,
            Status = result.Status,
            PassedCount = result.PassedCount,
            TotalCount = result.TotalCount,
            CreatedAtUtc = timeProvider.GetUtcNow().UtcDateTime,
        }, ct);

        return result;
    }
}
