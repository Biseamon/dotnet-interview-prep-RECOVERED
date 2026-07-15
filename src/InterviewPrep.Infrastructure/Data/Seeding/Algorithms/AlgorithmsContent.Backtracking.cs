namespace InterviewPrep.Infrastructure.Data.Seeding.Algorithms;

// Lesson 8 — Backtracking. Build candidates incrementally and abandon ("backtrack")
// a path as soon as it can't lead to a solution. Exercises return string arrays so
// grading can canonicalize (sort) and stay order-independent.
internal static partial class AlgorithmsContent
{
    private static LessonSeed BacktrackingLesson => new()
    {
        Slug = "backtracking",
        Title = "Backtracking",
        Order = 8,
        MarkdownContent =
            """
            ## Backtracking

            Explore choices depth-first; at each step **choose**, recurse, then **un-choose**
            (backtrack) to try the next option. It's how you enumerate subsets, permutations,
            and combinations — and prune invalid branches early (N-Queens, Sudoku).

            Template: `for each choice: pick it → recurse → undo it`.
            """,
        Exercises =
        [
            Subsets,
            Permutations,
            GenerateParentheses,
        ],
    };

    private static ExerciseSeed Subsets => new()
    {
        Slug = "subsets",
        Title = "Subsets (Power Set)",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Return all subsets of `nums` (the power set). Each subset is a comma-joined
            string (the empty subset is `""`), keeping elements in input order. There are
            2ⁿ subsets. Order of the returned array doesn't matter — grading sorts it.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: backtrack — for each index, choose to include it or not.
                public static string[] Subsets(int[] nums)
                {
                    return new string[0];
                }
            }
            """,
        HarnessCode =
            """
            using System;
            using System.Linq;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("[1,2,3] -> 8 subsets", () =>
                    {
                        var got = Solution.Subsets(new[]{1,2,3});
                        Assert.Equal(",1,1,2,1,2,3,1,3,2,2,3,3", Canon(got));
                    });
                    r.Check("[] -> just empty subset", () =>
                        Assert.Equal("", Canon(Solution.Subsets(new int[0]))));
                    return r.ToJson();
                }

                // Sort the subsets so comparison is order-independent, then concatenate.
                private static string Canon(string[] xs) => string.Join(",", xs.OrderBy(s => s, StringComparer.Ordinal));
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                public static string[] Subsets(int[] nums)
                {
                    var results = new List<string>();
                    var current = new List<int>();

                    void Backtrack(int start)
                    {
                        results.Add(string.Join(",", current)); // record the current subset
                        for (int i = start; i < nums.Length; i++)
                        {
                            current.Add(nums[i]);   // choose
                            Backtrack(i + 1);       // recurse
                            current.RemoveAt(current.Count - 1); // un-choose
                        }
                    }

                    Backtrack(0);
                    return results.ToArray();
                }
            }
            """,
        Hints =
        [
            "At each index you either include the element or skip it.",
            "Record the current partial subset at every node of the recursion.",
            "Choose → recurse → un-choose (remove the last added element).",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[1,2,3] -> 8 subsets", IsHidden = false },
            new TestCaseSeed { Name = "[] -> just empty subset", IsHidden = true },
        ],
    };

    private static ExerciseSeed Permutations => new()
    {
        Slug = "permutations",
        Title = "Permutations",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Return all permutations of `nums`, each as a comma-joined string. There are n!
            of them. Order of the returned array doesn't matter — grading sorts it.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: backtrack, tracking which elements are already used.
                public static string[] Permute(int[] nums)
                {
                    return new string[0];
                }
            }
            """,
        HarnessCode =
            """
            using System;
            using System.Linq;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("[1,2,3] -> 6 permutations", () =>
                        Assert.Equal("123|132|213|231|312|321", Canon(Solution.Permute(new[]{1,2,3}))));
                    r.Check("[1] -> [1]", () => Assert.Equal("1", Canon(Solution.Permute(new[]{1}))));
                    return r.ToJson();
                }

                // Strip commas, sort, and pipe-join for a stable comparison.
                private static string Canon(string[] xs) =>
                    string.Join("|", xs.Select(s => s.Replace(",", "")).OrderBy(s => s, StringComparer.Ordinal));
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                public static string[] Permute(int[] nums)
                {
                    var results = new List<string>();
                    var current = new List<int>();
                    var used = new bool[nums.Length];

                    void Backtrack()
                    {
                        if (current.Count == nums.Length)
                        {
                            results.Add(string.Join(",", current));
                            return;
                        }
                        for (int i = 0; i < nums.Length; i++)
                        {
                            if (used[i]) continue; // skip elements already in this permutation
                            used[i] = true; current.Add(nums[i]);   // choose
                            Backtrack();                            // recurse
                            used[i] = false; current.RemoveAt(current.Count - 1); // un-choose
                        }
                    }

                    Backtrack();
                    return results.ToArray();
                }
            }
            """,
        Hints =
        [
            "Track which indices are already used in the current permutation.",
            "When the current list is full, record it.",
            "Choose an unused element → recurse → mark it unused again.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[1,2,3] -> 6 permutations", IsHidden = false },
            new TestCaseSeed { Name = "[1] -> [1]", IsHidden = true },
        ],
    };

    private static ExerciseSeed GenerateParentheses => new()
    {
        Slug = "generate-parentheses",
        Title = "Generate Parentheses",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Return all combinations of `n` pairs of **well-formed** parentheses. Backtrack,
            adding `(` while you have opens left, and `)` only while it wouldn't break the
            balance. Order of the returned array doesn't matter — grading sorts it.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: backtrack tracking counts of '(' and ')' used.
                public static string[] Generate(int n)
                {
                    return new string[0];
                }
            }
            """,
        HarnessCode =
            """
            using System;
            using System.Linq;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("n=3 -> 5 combinations", () =>
                        Assert.Equal("((()))|(()())|(())()|()(())|()()()", Canon(Solution.Generate(3))));
                    r.Check("n=1 -> ()", () => Assert.Equal("()", Canon(Solution.Generate(1))));
                    return r.ToJson();
                }

                private static string Canon(string[] xs) =>
                    string.Join("|", xs.OrderBy(s => s, StringComparer.Ordinal));
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;
            using System.Text;

            public static class Solution
            {
                public static string[] Generate(int n)
                {
                    var results = new List<string>();
                    var sb = new StringBuilder();

                    void Backtrack(int open, int close)
                    {
                        if (sb.Length == 2 * n) { results.Add(sb.ToString()); return; }

                        if (open < n)               // can still open
                        {
                            sb.Append('(');
                            Backtrack(open + 1, close);
                            sb.Length--;            // backtrack
                        }
                        if (close < open)           // can close only if it stays balanced
                        {
                            sb.Append(')');
                            Backtrack(open, close + 1);
                            sb.Length--;
                        }
                    }

                    Backtrack(0, 0);
                    return results.ToArray();
                }
            }
            """,
        Hints =
        [
            "Track how many '(' and ')' you've placed.",
            "You may add '(' while open < n, and ')' only while close < open.",
            "When the string reaches length 2n, it's a valid combination.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "n=3 -> 5 combinations", IsHidden = false },
            new TestCaseSeed { Name = "n=1 -> ()", IsHidden = true },
        ],
    };
}
