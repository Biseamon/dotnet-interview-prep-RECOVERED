namespace InterviewPrep.Infrastructure.Data.Seeding.Algorithms;

// The "Data Structures & Algorithms" topic. Split across partial-class files, one
// per lesson (Arrays, TwoPointers, Stack, LinkedList, Trees, BinarySearch), so each
// file stays readable. This file just composes them into the Topic.
//
// SHARED HARNESS CONTRACT (same as every topic):
//   public static class __Harness { public static string Run() { var r = new HarnessReport(); ...; return r.ToJson(); } }
//   User code compiles as a class named `Solution` in the same assembly.
//   For data-structure problems (linked lists / trees) the NODE TYPES are defined
//   inside the harness (so they always exist); the starter code references them.
internal static partial class AlgorithmsContent
{
    public static TopicSeed Topic => new()
    {
        Slug = "algorithms",
        Name = "Data Structures & Algorithms",
        Description = "The coding-round staples: arrays, hashing, two pointers, stacks, linked lists, trees, and search.",
        Order = 2,
        Lessons =
        [
            ArraysAndHashingLesson,
            TwoPointersLesson,
            StackLesson,
            LinkedListLesson,
            TreesLesson,
            BinarySearchLesson,
            DynamicProgrammingLesson,
            BacktrackingLesson,
            IntervalsLesson,
            GraphsLesson,
            BitManipulationLesson,
            ArraysMatrixLesson,
        ],
    };
}
