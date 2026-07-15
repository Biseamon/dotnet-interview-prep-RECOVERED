using System.Text.Json;
using InterviewPrep.Application.Content;
using InterviewPrep.Domain;
using InterviewPrep.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InterviewPrep.Infrastructure.Data;

// EF Core-backed implementation of the content repository. Read queries use
// AsNoTracking() (we're only reading, so skip change-tracking overhead) and
// Include() to eager-load the navigation properties each caller needs.
public sealed class EfContentRepository(AppDbContext db) : IContentRepository
{
    public async Task<IReadOnlyList<Topic>> GetTopicsAsync(CancellationToken ct) =>
        await db.Topics
            .AsNoTracking()
            .Include(t => t.Lessons.OrderBy(l => l.Order)) // ordered nav for stable UI
                .ThenInclude(l => l.Exercises)              // exercises → per-topic progress totals
            .OrderBy(t => t.Order)
            .ToListAsync(ct);

    public async Task<Topic?> GetTopicBySlugAsync(string slug, CancellationToken ct) =>
        await db.Topics
            .AsNoTracking()
            .Include(t => t.Lessons.OrderBy(l => l.Order))
                .ThenInclude(l => l.Exercises.OrderBy(x => x.Difficulty))
            .FirstOrDefaultAsync(t => t.Slug == slug, ct);

    public async Task<Lesson?> GetLessonBySlugAsync(string slug, CancellationToken ct) =>
        await db.Lessons
            .AsNoTracking()
            .Include(l => l.Exercises.OrderBy(x => x.Difficulty))
            .FirstOrDefaultAsync(l => l.Slug == slug, ct);

    public async Task<Exercise?> GetExerciseBySlugAsync(string slug, CancellationToken ct) =>
        await db.Exercises
            .AsNoTracking()
            .Include(x => x.Hints.OrderBy(h => h.Order))
            .Include(x => x.TestCases.OrderBy(tc => tc.Order))
            .Include(x => x.Lesson!).ThenInclude(l => l.Topic) // for the owning topic slug (navigation)
            .FirstOrDefaultAsync(x => x.Slug == slug, ct);

    public async Task AddAttemptAsync(Attempt attempt, CancellationToken ct)
    {
        db.Attempts.Add(attempt);      // stage the insert
        await db.SaveChangesAsync(ct); // flush to SQLite
    }

    public async Task<IReadOnlyList<Attempt>> GetAttemptsAsync(string exerciseSlug, int limit, CancellationToken ct) =>
        await db.Attempts
            .AsNoTracking()
            .Where(a => a.Exercise!.Slug == exerciseSlug) // join to Exercise via nav
            .OrderByDescending(a => a.CreatedAtUtc)       // newest first
            .Take(limit)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<string>> GetSolvedExerciseSlugsAsync(CancellationToken ct) =>
        await db.Attempts
            .AsNoTracking()
            .Where(a => a.Status == GradeStatus.Passed) // only fully-passing runs count as solved
            .Select(a => a.Exercise!.Slug)
            .Distinct()
            .ToListAsync(ct);

    // ---- Gamification: XP / level / belt / streak / weekly XP / quests ----

    private const int XpPerLevel = 100;
    private const int MaxHearts = 5;
    private const int RefillMinutes = 30;

    // XP per difficulty — matches the redesign (Easy 10 / Medium 15 / Hard 20).
    private static int XpFor(Difficulty d) => d switch
    {
        Difficulty.Easy => 10,
        Difficulty.Medium => 15,
        Difficulty.Hard => 20,
        _ => 10,
    };

    // Martial-arts belt for a level (Lv7 → Purple, Lv8 → Brown, matching the design).
    private static string BeltFor(int level) => level switch
    {
        1 => "White belt",
        2 => "Yellow belt",
        3 => "Orange belt",
        4 => "Green belt",
        5 => "Blue belt",
        6 or 7 => "Purple belt",
        8 => "Brown belt",
        9 => "Red belt",
        _ => "Black belt",
    };

    public Task<GamificationStats> GetGamificationStatsAsync(CancellationToken ct) => ComputeStatsAsync(ct);

    // The single source of truth for player stats, computed from the persisted event
    // log: distinct solved exercises (XP by difficulty, attributed to first-pass day)
    // plus completed drills (their XP + activity day).
    private async Task<GamificationStats> ComputeStatsAsync(CancellationToken ct)
    {
        // Passing attempts flattened, then grouped in memory (single-user volume is tiny).
        var passRows = await db.Attempts.AsNoTracking()
            .Where(a => a.Status == GradeStatus.Passed)
            .Select(a => new { a.Exercise!.Slug, a.Exercise.Difficulty, a.CreatedAtUtc })
            .ToListAsync(ct);

        var solved = passRows
            .GroupBy(r => r.Slug)
            .Select(g => new { g.First().Difficulty, FirstUtc = g.Min(r => r.CreatedAtUtc) })
            .ToList();

        var drills = await db.DrillResults.AsNoTracking()
            .Select(d => new { d.CreatedAtUtc, d.XpEarned })
            .ToListAsync(ct);

        var xp = solved.Sum(s => XpFor(s.Difficulty)) + drills.Sum(d => d.XpEarned);
        var level = xp / XpPerLevel + 1;
        var xpIntoLevel = xp % XpPerLevel;

        // Active days (a passing exercise OR a drill) → streak.
        var days = solved.Select(s => DateOnly.FromDateTime(s.FirstUtc.ToUniversalTime()))
            .Concat(drills.Select(d => DateOnly.FromDateTime(d.CreatedAtUtc.ToUniversalTime())))
            .Distinct().OrderBy(d => d).ToList();
        var (current, longest) = Streaks(days);

        // Weekly XP + activity, Monday..Sunday of the current week.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var mondayIndex = ((int)today.DayOfWeek + 6) % 7; // Mon=0 .. Sun=6
        var monday = today.AddDays(-mondayIndex);
        var weeklyXp = new int[7];
        var weekActivity = new bool[7];
        void Attribute(DateOnly d, int amount)
        {
            var idx = d.DayNumber - monday.DayNumber;
            if (idx is >= 0 and < 7) { weeklyXp[idx] += amount; weekActivity[idx] = true; }
        }
        foreach (var s in solved) Attribute(DateOnly.FromDateTime(s.FirstUtc.ToUniversalTime()), XpFor(s.Difficulty));
        foreach (var d in drills) Attribute(DateOnly.FromDateTime(d.CreatedAtUtc.ToUniversalTime()), d.XpEarned);
        var dailyXp = weeklyXp[mondayIndex];

        // "3 in a row" quest: trailing run of consecutive Passed exercise runs today.
        var startOfToday = new DateTime(today.Year, today.Month, today.Day, 0, 0, 0, DateTimeKind.Utc);
        var todaysStatuses = await db.Attempts.AsNoTracking()
            .Where(a => a.CreatedAtUtc >= startOfToday)
            .OrderBy(a => a.CreatedAtUtc)
            .Select(a => a.Status)
            .ToListAsync(ct);
        var correctInARow = 0;
        foreach (var st in todaysStatuses) correctInARow = st == GradeStatus.Passed ? correctInARow + 1 : 0;

        var quests = new List<QuestProgress>
        {
            new("earn-xp", "⚡", "Earn 30 XP", Math.Min(dailyXp, 30), 30),
            new("in-a-row", "🎯", "Get 3 exercises right in a row", Math.Min(correctInARow, 3), 3),
        };

        var badges = new List<string>();
        if (solved.Count >= 1) badges.Add("first-solve");
        if (solved.Count >= 10) badges.Add("ten-solved");
        if (solved.Count >= 50) badges.Add("fifty-solved");
        if (longest >= 7) badges.Add("streak-7");

        return new GamificationStats(
            xp, level, BeltFor(level), xpIntoLevel, XpPerLevel,
            solved.Count, current, longest, dailyXp, correctInARow,
            weeklyXp, weekActivity, quests, badges);
    }

    // Current streak = consecutive active days ending today or yesterday (a day off
    // resets it, but the current day not yet done doesn't). Longest = best run ever.
    private static (int current, int longest) Streaks(List<DateOnly> days)
    {
        if (days.Count == 0) return (0, 0);

        int longest = 1, run = 1;
        for (int i = 1; i < days.Count; i++)
        {
            run = days[i].DayNumber - days[i - 1].DayNumber == 1 ? run + 1 : 1;
            if (run > longest) longest = run;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (today.DayNumber - days[^1].DayNumber > 1) return (0, longest); // broken

        int current = 1;
        for (int i = days.Count - 1; i > 0; i--)
        {
            if (days[i].DayNumber - days[i - 1].DayNumber == 1) current++;
            else break;
        }
        return (current, longest);
    }

    // ---- Hearts (server-persisted singleton PlayerState) ----

    private async Task<PlayerState> LoadPlayerAsync(CancellationToken ct)
    {
        var p = await db.PlayerState.FirstOrDefaultAsync(x => x.Id == 1, ct);
        if (p is null)
        {
            p = new PlayerState { Id = 1, Hearts = MaxHearts, HeartsUpdatedUtc = DateTime.UtcNow };
            db.PlayerState.Add(p);
            await db.SaveChangesAsync(ct);
        }
        return p;
    }

    // Apply any hearts earned back since the anchor. Returns the new (hearts, anchor);
    // when full the anchor is left untouched to avoid pointless writes.
    private static (int hearts, DateTime updated) ApplyRefill(int hearts, DateTime updated)
    {
        if (hearts >= MaxHearts) return (hearts, updated);
        var earned = (int)((DateTime.UtcNow - updated).TotalMinutes / RefillMinutes);
        if (earned <= 0) return (hearts, updated);
        var newHearts = Math.Min(MaxHearts, hearts + earned);
        var newUpdated = newHearts >= MaxHearts ? DateTime.UtcNow : updated.AddMinutes(earned * RefillMinutes);
        return (newHearts, newUpdated);
    }

    public async Task<PlayerDto> GetPlayerAsync(CancellationToken ct)
    {
        var p = await LoadPlayerAsync(ct);
        var (hearts, updated) = ApplyRefill(p.Hearts, p.HeartsUpdatedUtc);
        if (hearts != p.Hearts || updated != p.HeartsUpdatedUtc)
        {
            p.Hearts = hearts;
            p.HeartsUpdatedUtc = updated;
            await db.SaveChangesAsync(ct);
        }
        return ToPlayerDto(p);
    }

    public async Task<PlayerDto> LoseHeartAsync(CancellationToken ct)
    {
        var p = await LoadPlayerAsync(ct);
        var (hearts, _) = ApplyRefill(p.Hearts, p.HeartsUpdatedUtc);
        p.Hearts = Math.Max(0, hearts - 1);
        p.HeartsUpdatedUtc = DateTime.UtcNow; // spending resets the refill clock
        await db.SaveChangesAsync(ct);
        return ToPlayerDto(p);
    }

    public async Task<PlayerDto> RefillHeartsAsync(CancellationToken ct)
    {
        var p = await LoadPlayerAsync(ct);
        p.Hearts = MaxHearts;
        p.HeartsUpdatedUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return ToPlayerDto(p);
    }

    private static PlayerDto ToPlayerDto(PlayerState p)
    {
        int? minutesToNext = p.Hearts >= MaxHearts
            ? null
            : Math.Max(1, RefillMinutes - (int)((DateTime.UtcNow - p.HeartsUpdatedUtc).TotalMinutes));
        return new PlayerDto(p.Hearts, MaxHearts, minutesToNext);
    }

    // ---- Interview drill (MCQ quiz) ----

    public async Task<IReadOnlyList<DrillQuestionDto>> GetDrillQuestionsAsync(int count, CancellationToken ct)
    {
        // Bank is small; pull it and pick a random N in memory.
        var all = await db.DrillQuestions.AsNoTracking().ToListAsync(ct);
        return all
            .OrderBy(_ => Guid.NewGuid())
            .Take(count)
            .Select(q => new DrillQuestionDto(
                q.Id, q.Tag, q.Text,
                JsonSerializer.Deserialize<List<string>>(q.OptionsJson) ?? [],
                q.CorrectIndex, q.Explanation))
            .ToList();
    }

    public async Task<DrillCompleteResult> RecordDrillResultAsync(int correctCount, int total, CancellationToken ct)
    {
        var xp = correctCount * 5 + 5; // matches the prototype's scoring
        db.DrillResults.Add(new DrillResult
        {
            CreatedAtUtc = DateTime.UtcNow,
            CorrectCount = correctCount,
            Total = total,
            XpEarned = xp,
        });
        await db.SaveChangesAsync(ct);
        await EvaluateAchievementsAsync(ct);
        return new DrillCompleteResult(xp, await ComputeStatsAsync(ct));
    }

    // ---- Achievements ----

    private static readonly (string Code, string Emoji, string Title, string Desc)[] AchievementDefs =
    [
        ("week-warrior", "🔥", "Week Warrior", "7-day streak"),
        ("xp-1000", "⚡", "1,000 XP Club", "Earn 1,000 XP"),
        ("flawless", "🎯", "Flawless", "Lesson with no fails"),
        ("topic-master", "🏆", "Topic Master", "Finish any topic 100%"),
        ("legend-30", "👑", "30-Day Legend", "30-day streak"),
        ("drill-sergeant", "🧠", "Drill Sergeant", "10 perfect drills"),
    ];

    public async Task<IReadOnlyList<AchievementDto>> GetAchievementsAsync(CancellationToken ct)
    {
        await EvaluateAchievementsAsync(ct);
        var unlocked = await db.UnlockedAchievements.AsNoTracking()
            .ToDictionaryAsync(a => a.Code, a => a.UnlockedAtUtc, ct);
        return AchievementDefs
            .Select(d => new AchievementDto(d.Code, d.Emoji, d.Title, d.Desc,
                unlocked.ContainsKey(d.Code),
                unlocked.TryGetValue(d.Code, out var t) ? t : null))
            .ToList();
    }

    // Evaluate which achievements are currently earned and persist any newly-earned
    // ones (they stay earned even if the metric later dips).
    private async Task EvaluateAchievementsAsync(CancellationToken ct)
    {
        var stats = await ComputeStatsAsync(ct);
        var earned = new HashSet<string>();
        if (stats.LongestStreak >= 7) earned.Add("week-warrior");
        if (stats.LongestStreak >= 30) earned.Add("legend-30");
        if (stats.Xp >= 1000) earned.Add("xp-1000");

        if (await db.DrillResults.CountAsync(d => d.Total > 0 && d.CorrectCount == d.Total, ct) >= 10)
            earned.Add("drill-sergeant");

        // Topic completion + a flawless lesson need the content structure + attempt order.
        var topics = await db.Topics.AsNoTracking()
            .Include(t => t.Lessons).ThenInclude(l => l.Exercises)
            .ToListAsync(ct);
        var solvedSlugs = (await db.Attempts.AsNoTracking()
            .Where(a => a.Status == GradeStatus.Passed)
            .Select(a => a.Exercise!.Slug).Distinct().ToListAsync(ct)).ToHashSet();

        if (topics.Any(t =>
        {
            var slugs = t.Lessons.SelectMany(l => l.Exercises).Select(e => e.Slug).ToList();
            return slugs.Count > 0 && slugs.All(solvedSlugs.Contains);
        }))
            earned.Add("topic-master");

        // Flawless = a lesson where every exercise's FIRST attempt passed.
        var attemptRows = await db.Attempts.AsNoTracking()
            .Select(a => new { a.Exercise!.Slug, a.Status, a.CreatedAtUtc })
            .ToListAsync(ct);
        var firstPass = attemptRows
            .GroupBy(a => a.Slug)
            .ToDictionary(g => g.Key, g => g.OrderBy(a => a.CreatedAtUtc).First().Status == GradeStatus.Passed);
        if (topics.SelectMany(t => t.Lessons).Any(l =>
        {
            var exs = l.Exercises.Select(e => e.Slug).ToList();
            return exs.Count > 0 && exs.All(s => firstPass.TryGetValue(s, out var fp) && fp);
        }))
            earned.Add("flawless");

        var already = (await db.UnlockedAchievements.Select(a => a.Code).ToListAsync(ct)).ToHashSet();
        var toAdd = earned.Except(already)
            .Select(code => new UnlockedAchievement { Code = code, UnlockedAtUtc = DateTime.UtcNow })
            .ToList();
        if (toAdd.Count > 0)
        {
            db.UnlockedAchievements.AddRange(toAdd);
            await db.SaveChangesAsync(ct);
        }
    }

    public async Task<int> ClearAttemptsAsync(string exerciseSlug, CancellationToken ct) =>
        // ExecuteDeleteAsync issues a single DELETE ... WHERE in SQL — no loading
        // entities into memory first. Efficient bulk delete (EF Core 7+).
        await db.Attempts
            .Where(a => a.Exercise!.Slug == exerciseSlug)
            .ExecuteDeleteAsync(ct);
}
