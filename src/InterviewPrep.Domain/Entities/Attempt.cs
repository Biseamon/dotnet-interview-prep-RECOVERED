namespace InterviewPrep.Domain.Entities;

// A record of one grading run for an exercise. Persisting attempts gives us
// progress tracking ("solved" once any attempt Passed) and an attempt history —
// and it's good EF Core practice (writes, queries, ordering by time).
public class Attempt
{
    public int Id { get; set; }

    public int ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }

    // The code the learner submitted — lets us show/restore past attempts.
    public required string SubmittedCode { get; set; }

    // Final status of this run (Passed/Failed/CompileError/...).
    public GradeStatus Status { get; set; }

    // How many hidden+visible tests passed out of the total, for a quick summary.
    public int PassedCount { get; set; }
    public int TotalCount { get; set; }

    // UTC timestamp of the run. Set by the service at grade time (not a DB default)
    // so the value is explicit and testable.
    public DateTime CreatedAtUtc { get; set; }
}
