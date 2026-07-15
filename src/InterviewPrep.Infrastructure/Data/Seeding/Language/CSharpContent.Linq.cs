namespace InterviewPrep.Infrastructure.Data.Seeding.Language;

// Lesson — LINQ. Declarative queries over sequences, plus the interview favourite:
// deferred (lazy) execution and IEnumerable vs IQueryable.
internal static partial class CSharpContent
{
    private static LessonSeed LinqLesson => new()
    {
        Slug = "linq",
        Title = "LINQ",
        Order = 1,
        MarkdownContent =
            """
            ## LINQ

            LINQ expresses transformations declaratively: `Where`, `Select`, `GroupBy`,
            `SelectMany`, `Aggregate`, `OrderBy`.

            Two things interviewers love:
            - **Deferred execution** — a query built from `Where`/`Select` doesn't run until
              you enumerate it (`foreach`, `ToList`, `Count`…). Building ≠ executing.
            - **`IEnumerable` vs `IQueryable`** — `IEnumerable` runs in-process (LINQ to
              Objects); `IQueryable` builds an expression tree a provider (e.g. EF Core)
              translates to SQL and runs on the server.
            """,
        Exercises =
        [
            Flatten,
            WordFrequency,
            DistinctSorted,
        ],
    };

    private static ExerciseSeed Flatten => new()
    {
        Slug = "linq-flatten",
        Title = "Flatten Nested Arrays",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Flatten a jagged array `int[][]` into a single `int[]`, preserving order.
            `SelectMany` projects each inner sequence and concatenates them.
            """,
        StarterCode =
            """
            using System.Linq;

            public static class Solution
            {
                // TODO: flatten with SelectMany.
                public static int[] Flatten(int[][] arrays)
                {
                    return new int[0];
                }
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("[[1,2],[3],[4,5]] -> 1,2,3,4,5", () =>
                        Assert.Equal("1,2,3,4,5", string.Join(",", Solution.Flatten(new[]{ new[]{1,2}, new[]{3}, new[]{4,5} }))));
                    r.Check("[[]] -> ''", () =>
                        Assert.Equal("", string.Join(",", Solution.Flatten(new[]{ new int[0] }))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Linq;

            public static class Solution
            {
                // SelectMany flattens: each inner array is projected, then concatenated.
                public static int[] Flatten(int[][] arrays) => arrays.SelectMany(a => a).ToArray();
            }
            """,
        Hints =
        [
            "`Select` would give you a sequence of arrays — you want them merged.",
            "`SelectMany(a => a)` concatenates all inner sequences into one.",
            "Finish with `.ToArray()`.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[[1,2],[3],[4,5]] -> 1,2,3,4,5", IsHidden = false },
            new TestCaseSeed { Name = "[[]] -> ''", IsHidden = true },
        ],
    };

    private static ExerciseSeed WordFrequency => new()
    {
        Slug = "linq-word-frequency",
        Title = "Word Frequency with GroupBy",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Given an array of words, return how many times the given `word` appears,
            case-sensitively. Use LINQ (`Count`, or `GroupBy` into a dictionary). This
            mirrors the kind of aggregation EF Core would translate to SQL `GROUP BY`.
            """,
        StarterCode =
            """
            using System.Linq;

            public static class Solution
            {
                // TODO: count occurrences of `word` in `words`.
                public static int CountOf(string[] words, string word)
                {
                    return 0;
                }
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    var words = new[]{"a","b","a","c","a","b"};
                    r.Check("count 'a' -> 3", () => Assert.Equal(3, Solution.CountOf(words, "a")));
                    r.Check("count 'b' -> 2", () => Assert.Equal(2, Solution.CountOf(words, "b")));
                    r.Check("count 'z' -> 0", () => Assert.Equal(0, Solution.CountOf(words, "z")));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Linq;

            public static class Solution
            {
                // Count(predicate) is the concise form; GroupBy would also work.
                public static int CountOf(string[] words, string word) => words.Count(w => w == word);
            }
            """,
        Hints =
        [
            "`Where(...).Count()` or `Count(predicate)` both work.",
            "Compare each word to the target with `==`.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "count 'a' -> 3", IsHidden = false },
            new TestCaseSeed { Name = "count 'b' -> 2", IsHidden = false },
        ],
    };

    private static ExerciseSeed DistinctSorted => new()
    {
        Slug = "linq-distinct-sorted",
        Title = "Distinct, Sorted",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Return the unique values of `nums`, sorted ascending. Chain `Distinct()` and
            `OrderBy(...)`. (Note: the query is lazy until `ToArray` forces it.)
            """,
        StarterCode =
            """
            using System.Linq;

            public static class Solution
            {
                // TODO: distinct + sorted ascending.
                public static int[] DistinctSorted(int[] nums)
                {
                    return new int[0];
                }
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("[3,1,2,3,1] -> 1,2,3", () =>
                        Assert.Equal("1,2,3", string.Join(",", Solution.DistinctSorted(new[]{3,1,2,3,1}))));
                    r.Check("[] -> ''", () =>
                        Assert.Equal("", string.Join(",", Solution.DistinctSorted(new int[0]))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Linq;

            public static class Solution
            {
                public static int[] DistinctSorted(int[] nums) =>
                    nums.Distinct().OrderBy(x => x).ToArray();
            }
            """,
        Hints =
        [
            "`Distinct()` removes duplicates.",
            "`OrderBy(x => x)` sorts ascending.",
            "`ToArray()` executes the (until now deferred) query.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[3,1,2,3,1] -> 1,2,3", IsHidden = false },
            new TestCaseSeed { Name = "[] -> ''", IsHidden = true },
        ],
    };
}
