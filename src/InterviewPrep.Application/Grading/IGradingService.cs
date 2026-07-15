using InterviewPrep.Domain.Grading;

namespace InterviewPrep.Application.Grading;

// Orchestrates a grading run at the use-case level: load the exercise, hand its
// harness + the user's code to the ICodeRunner, persist an Attempt, return the
// result. Controllers depend on THIS, not on the runner or the DbContext.
public interface IGradingService
{
    // Grades a submission for the exercise identified by slug.
    // Returns null if no such exercise exists (API maps that to 404).
    Task<GradeResult?> GradeAsync(string exerciseSlug, string userSource, CancellationToken ct);
}
