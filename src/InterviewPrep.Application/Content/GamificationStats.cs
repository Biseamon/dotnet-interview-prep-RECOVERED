namespace InterviewPrep.Application.Content;

// Player stats derived from the event log (Attempt + DrillResult rows). XP is earned
// per SOLVED exercise (weighted by difficulty) plus completed drills; level and belt
// are functions of XP; the streak counts consecutive UTC days with activity (a passing
// attempt OR a drill). All computed on read, so it always reflects current progress.
public sealed record GamificationStats(
    int Xp,
    int Level,
    string Belt,          // martial-arts belt name for the current level
    int XpIntoLevel,      // XP progress within the current level (0..XpForNextLevel)
    int XpForNextLevel,
    int SolvedCount,
    int StreakDays,
    int LongestStreak,
    int DailyXp,          // XP earned today (feeds the "Earn 30 XP" quest)
    int CorrectInARow,    // current consecutive correct exercise runs today
    IReadOnlyList<int> WeeklyXp,       // Mon..Sun XP for the current week (length 7)
    IReadOnlyList<bool> WeekActivity,  // Mon..Sun: was there activity that day (length 7)
    IReadOnlyList<QuestProgress> Quests,
    IReadOnlyList<string> Badges);

// A daily quest's live progress (rendered as a labelled gold progress bar + 🎁).
public sealed record QuestProgress(string Id, string Emoji, string Label, int Current, int Target);

// The player's spendable lives. Hearts refill over wall-clock time.
public sealed record PlayerDto(int Hearts, int MaxHearts, int? MinutesToNext);

// A drill question as served to the client. CorrectIndex + Explanation are included:
// this is a low-stakes self-check quiz (the prototype reveals the answer on "CHECK"),
// not a graded credential, so client-side evaluation is acceptable.
public sealed record DrillQuestionDto(int Id, string Tag, string Text, IReadOnlyList<string> Options, int CorrectIndex, string Explanation);

// Result of recording a completed drill: XP awarded + the refreshed stats.
public sealed record DrillCompleteResult(int XpEarned, GamificationStats Stats);

// An achievement definition + whether/when it has been earned.
public sealed record AchievementDto(string Code, string Emoji, string Title, string Description, bool Earned, DateTime? UnlockedAtUtc);
