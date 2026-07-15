namespace InterviewPrep.Infrastructure.Data.Seeding.Patterns;

// The "Design Patterns (GoF)" topic, split by category (Creational / Structural /
// Behavioral). Each exercise gives the learner some PROVIDED types (interfaces or
// base classes, marked in the starter) and asks them to implement the pattern's
// key participants. The hidden harness then exercises the behaviour.
internal static partial class DesignPatternsContent
{
    public static TopicSeed Topic => new()
    {
        Slug = "design-patterns",
        Name = "Design Patterns (GoF)",
        Description = "The Gang-of-Four patterns interviewers ask about — creational, structural, and behavioral.",
        Order = 3,
        Lessons =
        [
            CreationalLesson,
            StructuralLesson,
            BehavioralLesson,
        ],
    };
}
