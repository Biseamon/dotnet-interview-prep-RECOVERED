namespace InterviewPrep.Domain.Entities;

// A single teaching unit inside a Topic. Holds the explanatory prose (Markdown)
// the learner reads before attempting the Topic's exercises.
public class Lesson
{
    public int Id { get; set; }

    // Foreign key back to the owning Topic. EF Core infers the relationship from
    // this pair (TopicId + the Topic navigation property below).
    public int TopicId { get; set; }
    public Topic? Topic { get; set; }

    public required string Slug { get; set; }
    public required string Title { get; set; }

    // The lesson body in Markdown — rendered on the frontend. This is where the
    // richly-commented "what + why" teaching content lives.
    public required string MarkdownContent { get; set; }

    public int Order { get; set; }

    // The exercises that belong to this lesson.
    public List<Exercise> Exercises { get; set; } = [];
}
