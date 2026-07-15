namespace InterviewPrep.Infrastructure.Data.Seeding;

// Plain DTOs that mirror the shape of the content/*.json files. Kept separate
// from the EF entities so the on-disk authoring format can evolve independently
// of the database schema, and so JSON deserialization targets simple objects.
// (System.Text.Json binds these by property name, case-insensitively.)

public sealed class TopicSeed
{
    public string Slug { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int Order { get; set; }
    public List<LessonSeed> Lessons { get; set; } = [];
}

public sealed class LessonSeed
{
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string MarkdownContent { get; set; } = "";
    public int Order { get; set; }
    public List<ExerciseSeed> Exercises { get; set; } = [];
}

public sealed class ExerciseSeed
{
    public string Slug { get; set; } = "";
    public string Title { get; set; } = "";
    public string Prompt { get; set; } = "";
    public string? Explanation { get; set; }          // ELI5 idea, attached from Explanations map
    public string Difficulty { get; set; } = "Easy"; // parsed to the Difficulty enum
    public string Kind { get; set; } = "Function";    // parsed to the ExerciseKind enum
    public string Language { get; set; } = "CSharp";   // CSharp | Sql | Config
    public int? TimeoutSeconds { get; set; }
    public string StarterCode { get; set; } = "";
    public string HarnessCode { get; set; } = "";
    public string ReferenceSolution { get; set; } = "";
    public List<string> Hints { get; set; } = [];
    public List<TestCaseSeed> TestCases { get; set; } = [];
}

public sealed class TestCaseSeed
{
    public string Name { get; set; } = "";
    public bool IsHidden { get; set; }
    public string? InputJson { get; set; }
    public string? ExpectedJson { get; set; }
}
