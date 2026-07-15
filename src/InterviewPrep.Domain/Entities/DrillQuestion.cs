namespace InterviewPrep.Domain.Entities;

// One multiple-choice question in the "interview drill" quiz bank. Seeded from
// DrillCatalog (authored in code, like the exercise content) and served in random
// batches. Options are stored as a JSON array of strings to keep it a single column.
public class DrillQuestion
{
    public int Id { get; set; }

    // Category label shown on the drill card (e.g. "ASYNC", "ALGORITHMS").
    public required string Tag { get; set; }

    // The question prompt.
    public required string Text { get; set; }

    // JSON array of the answer options, e.g. ["O(1)","O(log n)","O(n)","O(n log n)"].
    public required string OptionsJson { get; set; }

    // Index (0-based) of the correct option within OptionsJson.
    public int CorrectIndex { get; set; }

    // One-line explanation revealed after the learner answers.
    public required string Explanation { get; set; }

    // Stable authoring order (used only for deterministic seeding).
    public int Order { get; set; }
}
