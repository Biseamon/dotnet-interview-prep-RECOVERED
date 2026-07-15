using InterviewPrep.Domain;
using InterviewPrep.Domain.Grading;

namespace InterviewPrep.Application.Grading;

// A grader for one exercise LANGUAGE. Multiple implementations are registered (C#
// via Roslyn, SQL via SQLite, Config via rule checks); GradingService picks the one
// whose Language matches the exercise. Swappable for out-of-process later without
// touching other layers.
public interface IExerciseRunner
{
    // Which language this runner grades.
    ExerciseLanguage Language { get; }

    // Grade a submission. Never throws for user errors (those become
    // CompileError/Failed/Timeout results); only throws if the exercise's own harness
    // is broken (a server/authoring bug).
    GradeResult Run(RunRequest request);
}

// Everything a runner needs, with no dependency on EF entities. HarnessCode is
// interpreted per-language: C# harness source, or a JSON blob for SQL/Config.
public record RunRequest(
    string UserSource,   // the learner's submission (C#, SQL, or config text)
    string HarnessCode,  // the exercise's hidden grading spec
    int TimeoutSeconds);
