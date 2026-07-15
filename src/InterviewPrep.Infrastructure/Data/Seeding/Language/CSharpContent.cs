namespace InterviewPrep.Infrastructure.Data.Seeding.Language;

// The "C# Language & Runtime" topic — the language features interviewers probe:
// LINQ (and deferred execution), pattern matching, generics, and Span<T>/memory.
internal static partial class CSharpContent
{
    public static TopicSeed Topic => new()
    {
        Slug = "csharp-language",
        Name = "C# Language & Runtime",
        Description = "LINQ, pattern matching, records, generics, and Span<T> — the modern-C# fundamentals.",
        Order = 6,
        Lessons =
        [
            LinqLesson,
            PatternMatchingLesson,
            GenericsLesson,
            SpansLesson,
        ],
    };
}
