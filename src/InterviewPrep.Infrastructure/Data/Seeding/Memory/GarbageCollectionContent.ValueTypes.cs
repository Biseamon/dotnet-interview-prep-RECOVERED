namespace InterviewPrep.Infrastructure.Data.Seeding.Memory;

// Lesson 2 for Memory & GC — value vs reference semantics and stack allocation.
internal static partial class GarbageCollectionContent
{
    private static LessonSeed ValueTypesLesson => new()
    {
        Slug = "value-types",
        Title = "Value Types & the Stack",
        Order = 2,
        MarkdownContent =
            """
            ## Value Types & the Stack

            - **Structs** are value types — assigning or passing one **copies** it. Two
              variables never share a struct's storage (unless you pass by `ref`).
            - **Classes** are reference types — variables point at the same heap object, so a
              mutation through one is visible through the other.
            - **`stackalloc`** allocates a small buffer on the stack (no GC allocation),
              usable as a `Span<T>` for short-lived scratch space.
            """,
        Exercises =
        [
            ValueSemantics,
            StackAllocSum,
        ],
    };

    private static ExerciseSeed ValueSemantics => new()
    {
        Slug = "value-semantics",
        Title = "Struct vs Class Semantics",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Show the copy difference. Given the provided `PointStruct` (a struct) and
            `PointClass` (a class), implement `Demo()` returning `(structX, classX)` after:
            copy each into a second variable, set the **copy's** X to 99, then read the
            **original's** X. The struct original stays unchanged; the class original changes.
            Expected result: `(1, 99)`.
            """,
        StarterCode =
            """
            // PROVIDED — do not modify:
            public struct PointStruct { public int X; }
            public class PointClass { public int X; }

            public static class Solution
            {
                // TODO: copy each (starting X = 1), mutate the copy's X to 99, return originals' X.
                public static (int structX, int classX) Demo()
                {
                    return (0, 0);
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
                    r.Check("struct copies, class shares -> (1, 99)", () =>
                    {
                        var (s, c) = Solution.Demo();
                        Assert.Equal(1, s);   // struct original untouched
                        Assert.Equal(99, c);  // class original mutated via the shared reference
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public struct PointStruct { public int X; }
            public class PointClass { public int X; }

            public static class Solution
            {
                public static (int structX, int classX) Demo()
                {
                    var s1 = new PointStruct { X = 1 };
                    var s2 = s1;   // COPY — independent value
                    s2.X = 99;     // does not affect s1

                    var c1 = new PointClass { X = 1 };
                    var c2 = c1;   // same reference
                    c2.X = 99;     // mutates the one shared object

                    return (s1.X, c1.X); // (1, 99)
                }
            }
            """,
        Hints =
        [
            "Assigning a struct copies its fields; the two variables are independent.",
            "Assigning a class copies the reference; both point at the same object.",
            "So mutating the struct copy leaves the original at 1, but the class original becomes 99.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "struct copies, class shares -> (1, 99)", IsHidden = false },
        ],
    };

    private static ExerciseSeed StackAllocSum => new()
    {
        Slug = "stackalloc-sum",
        Title = "Scratch Buffer with stackalloc",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Implement `SumOfSquares(n)` that fills a **stack-allocated** `Span<int>` of size
            `n` with `1², 2², …, n²` and returns their sum — no heap array, no GC allocation.
            (Assume `n` is small, e.g. ≤ 64.)
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: use `stackalloc int[n]` as a Span<int>, fill with squares, sum them.
                public static int SumOfSquares(int n)
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
                    r.Check("n=3 -> 1+4+9 = 14", () => Assert.Equal(14, Solution.SumOfSquares(3)));
                    r.Check("n=5 -> 55", () => Assert.Equal(55, Solution.SumOfSquares(5)));
                    r.Check("n=0 -> 0", () => Assert.Equal(0, Solution.SumOfSquares(0)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public static class Solution
            {
                public static int SumOfSquares(int n)
                {
                    Span<int> buffer = stackalloc int[n]; // on the stack — no GC allocation
                    for (int i = 0; i < n; i++)
                        buffer[i] = (i + 1) * (i + 1);

                    int total = 0;
                    foreach (var v in buffer) total += v;
                    return total;
                }
            }
            """,
        Hints =
        [
            "`Span<int> buf = stackalloc int[n];` gives a stack buffer with no allocation.",
            "Fill index i with (i+1) squared.",
            "Sum the span and return it.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "n=3 -> 1+4+9 = 14", IsHidden = false },
            new TestCaseSeed { Name = "n=5 -> 55", IsHidden = false },
            new TestCaseSeed { Name = "n=0 -> 0", IsHidden = true },
        ],
    };
}
