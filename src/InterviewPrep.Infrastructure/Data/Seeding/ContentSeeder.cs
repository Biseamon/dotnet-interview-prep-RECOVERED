using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InterviewPrep.Domain;
using InterviewPrep.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InterviewPrep.Infrastructure.Data.Seeding;

// Loads authored content (TopicSeed objects from the SeedCatalog) into the DB.
// RECONCILING BY CONTENT HASH: content is authored in code (the source of truth). On
// startup we compute a fingerprint of each topic's authored content and compare it to
// the stored ContentHash. New or CHANGED topics (added exercises OR edited prompts/
// explanations/etc.) are (re)seeded; unchanged topics are left alone, preserving their
// attempt history. A refreshed topic is deleted (cascade) and re-inserted.
public sealed class ContentSeeder(AppDbContext db, ILogger<ContentSeeder> logger)
{
    public async Task SeedAsync(IReadOnlyList<TopicSeed> topics, CancellationToken ct = default)
    {
        var existing = await db.Topics.ToListAsync(ct);
        var existingBySlug = existing.ToDictionary(t => t.Slug);

        foreach (var seed in topics)
        {
            // Attach the ELI5 explanation for each exercise from the central map, so it
            // becomes part of both the seeded entity AND the content hash.
            foreach (var ex in seed.Lessons.SelectMany(l => l.Exercises))
                ex.Explanation = Explanations.For(ex.Slug);

            var hash = ComputeHash(seed);

            if (existingBySlug.TryGetValue(seed.Slug, out var current))
            {
                if (current.ContentHash == hash)
                    continue; // unchanged — leave it (and its attempts) alone

                db.Topics.Remove(current); // changed → cascade-delete, then re-insert fresh
                logger.LogInformation("Refreshing changed topic '{Slug}'", seed.Slug);
            }
            else
            {
                logger.LogInformation("Seeded new topic '{Slug}'", seed.Slug);
            }

            db.Topics.Add(MapTopic(seed, hash));
        }

        await db.SaveChangesAsync(ct);
    }

    // Seed the interview-drill question bank (idempotent by count). If the authored
    // catalog size changes, the whole bank is refreshed so edits take effect.
    public async Task SeedDrillsAsync(CancellationToken ct = default)
    {
        var existingCount = await db.DrillQuestions.CountAsync(ct);
        if (existingCount == DrillCatalog.All.Count) return; // unchanged

        if (existingCount > 0)
            await db.DrillQuestions.ExecuteDeleteAsync(ct);

        var rows = DrillCatalog.All.Select((q, i) => new DrillQuestion
        {
            Tag = q.Tag,
            Text = q.Text,
            OptionsJson = JsonSerializer.Serialize(q.Options),
            CorrectIndex = q.Correct,
            Explanation = q.Explanation,
            Order = i + 1,
        });
        db.DrillQuestions.AddRange(rows);
        await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} drill questions", DrillCatalog.All.Count);
    }

    // Stable SHA-256 fingerprint over everything that affects what a learner sees or is
    // graded on. Any edit to these fields changes the hash → triggers a refresh.
    private static string ComputeHash(TopicSeed s)
    {
        var sb = new StringBuilder();
        sb.Append(s.Slug).Append('|').Append(s.Name).Append('|').Append(s.Description).Append('|').Append(s.Order);
        foreach (var l in s.Lessons)
        {
            sb.Append("#L").Append(l.Slug).Append(l.Title).Append(l.Order).Append(l.MarkdownContent);
            foreach (var e in l.Exercises)
            {
                sb.Append("#E").Append(e.Slug).Append(e.Title).Append(e.Prompt).Append(e.Explanation)
                  .Append(e.Difficulty).Append(e.Kind).Append(e.Language).Append(e.StarterCode).Append(e.HarnessCode)
                  .Append(e.ReferenceSolution);
                foreach (var h in e.Hints) sb.Append("#H").Append(h);
                foreach (var tc in e.TestCases) sb.Append("#T").Append(tc.Name).Append(tc.IsHidden);
            }
        }
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString()));
        return Convert.ToHexString(bytes);
    }

    // --- Mapping from seed DTOs to EF entities (parsing enums along the way) ---

    private static Topic MapTopic(TopicSeed s, string contentHash) => new()
    {
        Slug = s.Slug,
        Name = s.Name,
        Description = s.Description,
        Order = s.Order,
        ContentHash = contentHash,
        Lessons = s.Lessons.Select(MapLesson).ToList(),
    };

    private static Lesson MapLesson(LessonSeed s) => new()
    {
        Slug = s.Slug,
        Title = s.Title,
        MarkdownContent = s.MarkdownContent,
        Order = s.Order,
        Exercises = s.Exercises.Select(MapExercise).ToList(),
    };

    private static Exercise MapExercise(ExerciseSeed s) => new()
    {
        Slug = s.Slug,
        Title = s.Title,
        Prompt = s.Prompt,
        Explanation = s.Explanation,
        // Enum.Parse turns the JSON string ("Medium") into the enum; case-insensitive.
        Difficulty = Enum.Parse<Difficulty>(s.Difficulty, ignoreCase: true),
        Kind = Enum.Parse<ExerciseKind>(s.Kind, ignoreCase: true),
        Language = Enum.Parse<ExerciseLanguage>(s.Language, ignoreCase: true),
        TimeoutSeconds = s.TimeoutSeconds,
        StarterCode = s.StarterCode,
        HarnessCode = s.HarnessCode,
        ReferenceSolution = s.ReferenceSolution,
        // Hints authored as a plain string array → numbered Hint rows (1-based Order).
        Hints = s.Hints.Select((text, i) => new Hint { Order = i + 1, Text = text }).ToList(),
        TestCases = s.TestCases.Select((tc, i) => new TestCase
        {
            Name = tc.Name,
            IsHidden = tc.IsHidden,
            InputJson = tc.InputJson,
            ExpectedJson = tc.ExpectedJson,
            Order = i + 1,
        }).ToList(),
    };
}
