namespace InterviewPrep.Domain.Entities;

// The single implicit player's mutable, non-derivable state. This app has no user
// accounts, so there is exactly ONE row (Id = 1). Everything that CAN be derived from
// the attempt/drill history (XP, streak, level) is computed on read; only "hearts"
// (lives) live here, because they are spent/refilled over wall-clock time and so can't
// be reconstructed from the event log alone.
public class PlayerState
{
    public int Id { get; set; } // always 1 — the singleton row

    // Remaining hearts (0..MaxHearts). A wrong run/answer costs one; they refill over
    // time. The refill is applied lazily on read against HeartsUpdatedUtc.
    public int Hearts { get; set; }

    // When Hearts last changed — the anchor for the time-based refill calculation.
    public DateTime HeartsUpdatedUtc { get; set; }
}
