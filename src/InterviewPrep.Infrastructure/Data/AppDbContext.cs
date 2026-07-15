using InterviewPrep.Domain;
using InterviewPrep.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InterviewPrep.Infrastructure.Data;

// The EF Core unit-of-work + query root. One DbSet per aggregate we persist.
// Configuration is done in OnModelCreating (Fluent API) rather than attributes,
// keeping the Domain entities free of persistence concerns.
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Topic> Topics => Set<Topic>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Exercise> Exercises => Set<Exercise>();
    public DbSet<Hint> Hints => Set<Hint>();
    public DbSet<TestCase> TestCases => Set<TestCase>();
    public DbSet<Attempt> Attempts => Set<Attempt>();

    // Gamification (Duolingo-style redesign): the single-player hearts row, the
    // seeded drill-quiz bank, completed drill sessions, and earned achievements.
    public DbSet<PlayerState> PlayerState => Set<PlayerState>();
    public DbSet<DrillQuestion> DrillQuestions => Set<DrillQuestion>();
    public DbSet<DrillResult> DrillResults => Set<DrillResult>();
    public DbSet<UnlockedAchievement> UnlockedAchievements => Set<UnlockedAchievement>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // --- Topic ---
        b.Entity<Topic>(e =>
        {
            e.HasIndex(t => t.Slug).IsUnique(); // slugs are our public lookup key
            // One Topic has many Lessons; deleting a Topic cascades to its Lessons.
            e.HasMany(t => t.Lessons)
             .WithOne(l => l.Topic!)
             .HasForeignKey(l => l.TopicId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Lesson ---
        b.Entity<Lesson>(e =>
        {
            e.HasIndex(l => l.Slug).IsUnique();
            e.HasMany(l => l.Exercises)
             .WithOne(x => x.Lesson!)
             .HasForeignKey(x => x.LessonId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Exercise ---
        b.Entity<Exercise>(e =>
        {
            e.HasIndex(x => x.Slug).IsUnique();

            // Store enums AS STRINGS (not ints). Rows stay readable and reordering
            // the enum won't silently remap existing data. This is a common, clean
            // EF Core pattern worth being able to explain in an interview.
            e.Property(x => x.Difficulty).HasConversion<string>();
            e.Property(x => x.Kind).HasConversion<string>();
            e.Property(x => x.Language).HasConversion<string>();

            e.HasMany(x => x.Hints)
             .WithOne(h => h.Exercise!)
             .HasForeignKey(h => h.ExerciseId)
             .OnDelete(DeleteBehavior.Cascade);

            e.HasMany(x => x.TestCases)
             .WithOne(tc => tc.Exercise!)
             .HasForeignKey(tc => tc.ExerciseId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // --- Attempt ---
        b.Entity<Attempt>(e =>
        {
            e.Property(a => a.Status).HasConversion<string>();
            // Index by (ExerciseId, CreatedAtUtc) so "latest attempts for exercise"
            // is an efficient, index-backed query.
            e.HasIndex(a => new { a.ExerciseId, a.CreatedAtUtc });
        });

        // --- DrillResult --- (queried by day for streak/weekly-XP)
        b.Entity<DrillResult>(e => e.HasIndex(d => d.CreatedAtUtc));

        // --- UnlockedAchievement --- (one row per achievement code)
        b.Entity<UnlockedAchievement>(e => e.HasIndex(a => a.Code).IsUnique());
    }
}
