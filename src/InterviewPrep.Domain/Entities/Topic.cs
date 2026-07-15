namespace InterviewPrep.Domain.Entities;

// A top-level subject area, e.g. "Async & Multithreading", "Design Patterns",
// "Data Structures & Algorithms", "Garbage Collection".
// Topic → Lesson → Exercise is the content hierarchy the whole app is built on.
public class Topic
{
    public int Id { get; set; }

    // URL-friendly identifier used in routes (e.g. /api/topics/async). Unique.
    // We navigate by Slug rather than Id so URLs are readable and stable.
    public required string Slug { get; set; }

    public required string Name { get; set; }

    // Short blurb shown on the topic dashboard card.
    public required string Description { get; set; }

    // Display order on the dashboard (lower = earlier). Lets us curate a
    // learning path instead of relying on insertion/Id order.
    public int Order { get; set; }

    // Fingerprint of this topic's authored content (exercises + explanations). The
    // seeder compares it against the freshly-computed hash to decide whether to
    // refresh the topic — so content EDITS propagate, not just added/removed exercises.
    public string? ContentHash { get; set; }

    // Navigation property: EF Core populates this with the topic's lessons when
    // we Include() them. Initialised to an empty list to avoid null checks.
    public List<Lesson> Lessons { get; set; } = [];
}
