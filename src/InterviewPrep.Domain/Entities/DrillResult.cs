namespace InterviewPrep.Domain.Entities;

// A completed interview-drill session. Persisting these means a drill counts as
// activity for the day (keeping the streak alive) and its XP feeds the weekly chart,
// alongside solved-exercise XP.
public class DrillResult
{
    public int Id { get; set; }

    // When the drill was completed (UTC). Drives streak/weekly-XP attribution.
    public DateTime CreatedAtUtc { get; set; }

    public int CorrectCount { get; set; }
    public int Total { get; set; }

    // XP awarded for this drill (correctCount * 5 + 5, matching the prototype).
    public int XpEarned { get; set; }
}
