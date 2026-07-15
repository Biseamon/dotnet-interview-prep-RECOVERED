namespace InterviewPrep.Domain.Entities;

// Records that a given achievement (by its stable Code) has been earned, and when.
// Achievement eligibility is evaluated from the event log (attempts + drills); once an
// achievement is earned it is persisted here so it stays earned even if the underlying
// metric later dips (e.g. a streak breaks after you already unlocked "Week Warrior").
public class UnlockedAchievement
{
    public int Id { get; set; }

    // Stable identifier (e.g. "week-warrior", "topic-master"). Unique.
    public required string Code { get; set; }

    public DateTime UnlockedAtUtc { get; set; }
}
