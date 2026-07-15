namespace InterviewPrep.Infrastructure.Data.Seeding.Language;

// Lessons — Pattern Matching, Generics, and Span<T>. The modern-C# features that
// show up constantly in interviews and idiomatic code.
internal static partial class CSharpContent
{
    // =========================================================================
    // Pattern Matching
    // =========================================================================
    private static LessonSeed PatternMatchingLesson => new()
    {
        Slug = "pattern-matching",
        Title = "Pattern Matching",
        Order = 2,
        MarkdownContent =
            """
            ## Pattern Matching

            `switch` **expressions** and patterns replace long if/else chains with concise,
            exhaustive branching:
            - **Constant / relational** patterns: `< 0`, `0`, `> 0`.
            - **Type** patterns: `Circle c => ...`.
            - **Property** patterns: `{ Length: 0 }`.
            The compiler warns on non-exhaustive switches — a real safety win.
            """,
        Exercises =
        [
            ClassifyNumber,
            ShapeArea,
        ],
    };

    private static ExerciseSeed ClassifyNumber => new()
    {
        Slug = "classify-number",
        Title = "Classify with a switch expression",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Return `"negative"`, `"zero"`, or `"positive"` for an int, using a **switch
            expression** with relational patterns (`< 0`, `0`, `_`).
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: use a switch expression with relational patterns.
                public static string Classify(int n)
                {
                    return "";
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
                    r.Check("-5 -> negative", () => Assert.Equal("negative", Solution.Classify(-5)));
                    r.Check("0 -> zero", () => Assert.Equal("zero", Solution.Classify(0)));
                    r.Check("7 -> positive", () => Assert.Equal("positive", Solution.Classify(7)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static string Classify(int n) => n switch
                {
                    < 0 => "negative",   // relational pattern
                    0   => "zero",       // constant pattern
                    _   => "positive",   // discard (catch-all)
                };
            }
            """,
        Hints =
        [
            "Use `n switch { ... }` as an expression.",
            "Relational patterns like `< 0` are allowed as switch arms.",
            "`_` is the catch-all for everything else (here, positive).",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "-5 -> negative", IsHidden = false },
            new TestCaseSeed { Name = "0 -> zero", IsHidden = false },
            new TestCaseSeed { Name = "7 -> positive", IsHidden = true },
        ],
    };

    private static ExerciseSeed ShapeArea => new()
    {
        Slug = "shape-area-pattern",
        Title = "Type Patterns over Records",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Given the provided `Shape` record hierarchy, return each shape's area using a
            **switch expression with type/property patterns**. `Circle(r)` → πr²,
            `Rectangle(w,h)` → w·h.
            """,
        StarterCode =
            """
            using System;

            // PROVIDED — do not modify:
            public abstract record Shape;
            public record Circle(double Radius) : Shape;
            public record Rectangle(double Width, double Height) : Shape;

            public static class Solution
            {
                // TODO: switch on the concrete record type to compute area.
                public static double Area(Shape shape)
                {
                    return 0;
                }
            }
            """,
        HarnessCode =
            """
            using System;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("Circle(2) -> 4π", () =>
                        Assert.True(Math.Abs(Solution.Area(new Circle(2)) - Math.PI * 4) < 1e-9));
                    r.Check("Rectangle(3,4) -> 12", () =>
                        Assert.True(Math.Abs(Solution.Area(new Rectangle(3, 4)) - 12) < 1e-9));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public abstract record Shape;
            public record Circle(double Radius) : Shape;
            public record Rectangle(double Width, double Height) : Shape;

            public static class Solution
            {
                public static double Area(Shape shape) => shape switch
                {
                    // Positional patterns deconstruct the record directly.
                    Circle(var radius)         => Math.PI * radius * radius,
                    Rectangle(var w, var h)    => w * h,
                    _ => throw new ArgumentException("unknown shape"),
                };
            }
            """,
        Hints =
        [
            "Switch on `shape` and match each concrete record type.",
            "Records support positional patterns: `Circle(var radius) => ...`.",
            "Include a `_` arm to keep the switch exhaustive.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "Circle(2) -> 4π", IsHidden = false },
            new TestCaseSeed { Name = "Rectangle(3,4) -> 12", IsHidden = false },
        ],
    };

    // =========================================================================
    // Generics
    // =========================================================================
    private static LessonSeed GenericsLesson => new()
    {
        Slug = "generics",
        Title = "Generics",
        Order = 3,
        MarkdownContent =
            """
            ## Generics

            Generics give **type-safe reuse** without boxing. Constraints (`where T : …`)
            let you call methods on the type parameter — e.g. `where T : IComparable<T>`
            enables `CompareTo`. Generic code is JIT-specialized per value type, so it's
            both safe and fast.
            """,
        Exercises =
        [
            GenericMax,
            GenericFrequency,
        ],
    };

    private static ExerciseSeed GenericMax => new()
    {
        Slug = "generic-max",
        Title = "Generic Max with a Constraint",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Implement `Max<T>(T a, T b)` returning the larger, constrained to
            `IComparable<T>` so you can call `CompareTo`. Works for ints, strings, etc.
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: add a constraint so CompareTo is available, then return the larger.
                public static T Max<T>(T a, T b)
                {
                    throw new NotImplementedException();
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
                    r.Check("Max(3,7) -> 7", () => Assert.Equal(7, Solution.Max(3, 7)));
                    r.Check("Max('apple','banana') -> banana", () => Assert.Equal("banana", Solution.Max("apple", "banana")));
                    r.Check("Max(5,5) -> 5", () => Assert.Equal(5, Solution.Max(5, 5)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public static class Solution
            {
                // The constraint unlocks CompareTo on T.
                public static T Max<T>(T a, T b) where T : IComparable<T>
                    => a.CompareTo(b) >= 0 ? a : b;
            }
            """,
        Hints =
        [
            "Without a constraint you can't compare two `T` values.",
            "Add `where T : IComparable<T>`.",
            "`a.CompareTo(b) >= 0` means a is greater-or-equal.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "Max(3,7) -> 7", IsHidden = false },
            new TestCaseSeed { Name = "Max('apple','banana') -> banana", IsHidden = false },
        ],
    };

    private static ExerciseSeed GenericFrequency => new()
    {
        Slug = "generic-frequency",
        Title = "Generic Frequency Counter",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Implement `MostCommon<T>(T[] items)` returning the element that appears most
            often (any one, on ties). Works for any `T` via a `Dictionary<T,int>`.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: count with a Dictionary<T,int>, return the most frequent key.
                public static T MostCommon<T>(T[] items) where T : notnull
                {
                    throw new System.NotImplementedException();
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
                    r.Check("[1,2,2,3,2] -> 2", () => Assert.Equal(2, Solution.MostCommon(new[]{1,2,2,3,2})));
                    r.Check("['a','b','a'] -> a", () => Assert.Equal("a", Solution.MostCommon(new[]{"a","b","a"})));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;
            using System.Linq;

            public static class Solution
            {
                public static T MostCommon<T>(T[] items) where T : notnull
                {
                    var counts = new Dictionary<T, int>();
                    foreach (var item in items)
                        counts[item] = counts.GetValueOrDefault(item) + 1;
                    return counts.OrderByDescending(kv => kv.Value).First().Key;
                }
            }
            """,
        Hints =
        [
            "A `Dictionary<T,int>` counts occurrences generically.",
            "`GetValueOrDefault` simplifies the increment.",
            "Return the key with the largest count.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[1,2,2,3,2] -> 2", IsHidden = false },
            new TestCaseSeed { Name = "['a','b','a'] -> a", IsHidden = false },
        ],
    };

    // =========================================================================
    // Span<T>
    // =========================================================================
    private static LessonSeed SpansLesson => new()
    {
        Slug = "spans",
        Title = "Span<T> & Memory",
        Order = 4,
        MarkdownContent =
            """
            ## Span<T> & Memory

            `Span<T>` is a **stack-only view** over contiguous memory (arrays, slices,
            `stackalloc`) that enables slicing and in-place work **without allocating**.
            It's a `ref struct` — can't be boxed, stored on the heap, or used across
            `await` — which is exactly what makes it safe and fast for hot paths.
            """,
        Exercises =
        [
            SumSpan,
            ReverseSpan,
        ],
    };

    private static ExerciseSeed SumSpan => new()
    {
        Slug = "sum-span",
        Title = "Sum a ReadOnlySpan",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Sum a `ReadOnlySpan<int>` and return the total. Spans let callers pass an
            array, a slice, or stack memory with zero allocation.
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: iterate the span and sum it.
                public static int Sum(ReadOnlySpan<int> values)
                {
                    return 0;
                }
            }
            """,
        HarnessCode =
            """
            using System;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("[1,2,3,4] -> 10", () => Assert.Equal(10, Solution.Sum(new[]{1,2,3,4})));
                    r.Check("slice [2..] of [1,2,3,4] -> 7", () =>
                        Assert.Equal(7, Solution.Sum(new[]{1,2,3,4}.AsSpan(2))));
                    r.Check("empty -> 0", () => Assert.Equal(0, Solution.Sum(ReadOnlySpan<int>.Empty)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public static class Solution
            {
                public static int Sum(ReadOnlySpan<int> values)
                {
                    int total = 0;
                    foreach (var v in values) total += v; // no allocation, no bounds surprises
                    return total;
                }
            }
            """,
        Hints =
        [
            "A span is iterable with a normal `foreach`.",
            "Accumulate into a local and return it.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[1,2,3,4] -> 10", IsHidden = false },
            new TestCaseSeed { Name = "slice [2..] of [1,2,3,4] -> 7", IsHidden = false },
            new TestCaseSeed { Name = "empty -> 0", IsHidden = true },
        ],
    };

    private static ExerciseSeed ReverseSpan => new()
    {
        Slug = "reverse-span",
        Title = "Reverse In Place with a Span",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Reverse a `Span<int>` **in place** (mutating the caller's memory) using two
            pointers. No new array — that's the point of `Span<T>`.
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: reverse the span in place with two indices.
                public static void Reverse(Span<int> values)
                {
                }
            }
            """,
        HarnessCode =
            """
            using System;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("[1,2,3,4,5] -> 5,4,3,2,1", () =>
                    {
                        var arr = new[]{1,2,3,4,5};
                        Solution.Reverse(arr);
                        Assert.Equal("5,4,3,2,1", string.Join(",", arr));
                    });
                    r.Check("[1,2] -> 2,1", () =>
                    {
                        var arr = new[]{1,2};
                        Solution.Reverse(arr);
                        Assert.Equal("2,1", string.Join(",", arr));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public static class Solution
            {
                public static void Reverse(Span<int> values)
                {
                    int l = 0, r = values.Length - 1;
                    while (l < r)
                    {
                        (values[l], values[r]) = (values[r], values[l]); // swap ends
                        l++; r--;
                    }
                }
            }
            """,
        Hints =
        [
            "Because a Span aliases the caller's memory, writes are visible to them.",
            "Swap the outer pair, then move both indices inward.",
            "Stop when the indices meet.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[1,2,3,4,5] -> 5,4,3,2,1", IsHidden = false },
            new TestCaseSeed { Name = "[1,2] -> 2,1", IsHidden = false },
        ],
    };
}
