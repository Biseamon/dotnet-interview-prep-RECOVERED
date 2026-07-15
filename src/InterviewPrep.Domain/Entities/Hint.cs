namespace InterviewPrep.Domain.Entities;

// A single progressive hint for an Exercise. The frontend reveals these one at a
// time (in Order), so a stuck learner gets nudged without being handed the answer.
public class Hint
{
    public int Id { get; set; }

    public int ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }

    // 1-based reveal order. Hint 1 is the gentlest nudge; later hints get specific.
    public int Order { get; set; }

    public required string Text { get; set; }
}
