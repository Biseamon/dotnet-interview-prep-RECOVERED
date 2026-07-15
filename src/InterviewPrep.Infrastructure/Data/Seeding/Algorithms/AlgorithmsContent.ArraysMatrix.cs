namespace InterviewPrep.Infrastructure.Data.Seeding.Algorithms;

// Lesson 12 — Arrays, Matrix & 2-D DP. Frequently-asked classics that round out the
// NeetCode-150 coverage: Kadane's max subarray, product-except-self, rotate-image,
// and a 2-D dynamic-programming grid.
internal static partial class AlgorithmsContent
{
    private static LessonSeed ArraysMatrixLesson => new()
    {
        Slug = "arrays-matrix",
        Title = "Arrays, Matrix & 2-D DP",
        Order = 12,
        MarkdownContent =
            """
            ## Arrays, Matrix & 2-D DP

            A grab-bag of interview favorites:
            - **Kadane's algorithm** — max subarray sum in one pass.
            - **Product except self** — prefix × suffix products, no division.
            - **Rotate image** — transpose then reverse rows, in place.
            - **2-D DP** — build a grid of subproblem answers (unique paths).
            """,
        Exercises = [MaxSubArray, ProductExceptSelf, RotateImage, UniquePaths],
    };

    private static ExerciseSeed MaxSubArray => new()
    {
        Slug = "maximum-subarray",
        Title = "Maximum Subarray (Kadane)",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Return the largest sum of any **contiguous** subarray. Kadane's algorithm: walk
            once, keeping the best sum ending here — either extend the previous run or restart
            at the current element.
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: Kadane — running best ending here, track the overall best.
                public static int MaxSubArray(int[] nums)
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
                    r.Check("[-2,1,-3,4,-1,2,1,-5,4] -> 6", () =>
                        Assert.Equal(6, Solution.MaxSubArray(new[]{-2,1,-3,4,-1,2,1,-5,4})));
                    r.Check("[1] -> 1", () => Assert.Equal(1, Solution.MaxSubArray(new[]{1})));
                    r.Check("[5,4,-1,7,8] -> 23", () => Assert.Equal(23, Solution.MaxSubArray(new[]{5,4,-1,7,8})));
                    r.Check("all negative -> largest single", () => Assert.Equal(-1, Solution.MaxSubArray(new[]{-3,-1,-2})));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public static class Solution
            {
                public static int MaxSubArray(int[] nums)
                {
                    int best = nums[0], current = nums[0];
                    for (int i = 1; i < nums.Length; i++)
                    {
                        // Extend the run, or start fresh at nums[i] — whichever is bigger.
                        current = Math.Max(nums[i], current + nums[i]);
                        best = Math.Max(best, current);
                    }
                    return best;
                }
            }
            """,
        Hints =
        [
            "Track the best sum that ENDS at the current index.",
            "At each step it's max(nums[i], current + nums[i]) — restart if the run went negative.",
            "Keep the overall best seen.",
        ],
        TestCases =
        [
            new() { Name = "[-2,1,-3,4,-1,2,1,-5,4] -> 6", IsHidden = false },
            new() { Name = "[5,4,-1,7,8] -> 23", IsHidden = false },
            new() { Name = "all negative -> largest single", IsHidden = true },
        ],
    };

    private static ExerciseSeed ProductExceptSelf => new()
    {
        Slug = "product-except-self",
        Title = "Product of Array Except Self",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Return an array where `result[i]` is the product of all elements **except** `nums[i]`
            — **without using division**. Multiply a prefix pass by a suffix pass.
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: prefix products, then multiply by suffix products. No division.
                public static int[] ProductExceptSelf(int[] nums)
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
                    r.Check("[1,2,3,4] -> 24,12,8,6", () =>
                        Assert.Equal("24,12,8,6", string.Join(",", Solution.ProductExceptSelf(new[]{1,2,3,4}))));
                    r.Check("[-1,1,0,-3,3] -> 0,0,9,0,0", () =>
                        Assert.Equal("0,0,9,0,0", string.Join(",", Solution.ProductExceptSelf(new[]{-1,1,0,-3,3}))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static int[] ProductExceptSelf(int[] nums)
                {
                    int n = nums.Length;
                    var result = new int[n];

                    int prefix = 1;
                    for (int i = 0; i < n; i++) { result[i] = prefix; prefix *= nums[i]; }

                    int suffix = 1;
                    for (int i = n - 1; i >= 0; i--) { result[i] *= suffix; suffix *= nums[i]; }

                    return result;
                }
            }
            """,
        Hints =
        [
            "result[i] = (product of everything before i) × (product of everything after i).",
            "First pass left-to-right fills result[i] with the prefix product.",
            "Second pass right-to-left multiplies in the suffix product.",
        ],
        TestCases =
        [
            new() { Name = "[1,2,3,4] -> 24,12,8,6", IsHidden = false },
            new() { Name = "[-1,1,0,-3,3] -> 0,0,9,0,0", IsHidden = false },
        ],
    };

    private static ExerciseSeed RotateImage => new()
    {
        Slug = "rotate-image",
        Title = "Rotate Image (90°)",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Rotate an `n×n` matrix **90° clockwise, in place**. The clean trick: **transpose**
            the matrix (swap across the diagonal), then **reverse each row**.
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: transpose, then reverse each row. Mutate `matrix` in place.
                public static void Rotate(int[][] matrix)
                {
                }
            }
            """,
        HarnessCode =
            """
            using System.Linq;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("rotates 3x3 clockwise", () =>
                    {
                        var m = new[] { new[]{1,2,3}, new[]{4,5,6}, new[]{7,8,9} };
                        Solution.Rotate(m);
                        var flat = string.Join(",", m.SelectMany(row => row));
                        Assert.Equal("7,4,1,8,5,2,9,6,3", flat);
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
                public static void Rotate(int[][] matrix)
                {
                    int n = matrix.Length;

                    // Transpose: swap matrix[i][j] with matrix[j][i].
                    for (int i = 0; i < n; i++)
                        for (int j = i + 1; j < n; j++)
                            (matrix[i][j], matrix[j][i]) = (matrix[j][i], matrix[i][j]);

                    // Reverse each row.
                    foreach (var row in matrix) Array.Reverse(row);
                }
            }
            """,
        Hints =
        [
            "Transpose first: swap [i][j] with [j][i] for j > i.",
            "Then reverse each row to complete the clockwise turn.",
            "Both steps mutate the matrix in place — no extra grid.",
        ],
        TestCases = [new() { Name = "rotates 3x3 clockwise", IsHidden = false }],
    };

    private static ExerciseSeed UniquePaths => new()
    {
        Slug = "unique-paths",
        Title = "Unique Paths (2-D DP)",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            A robot at the top-left of an `m×n` grid can only move right or down. Count the
            distinct paths to the bottom-right. 2-D DP: paths to a cell = paths from above +
            paths from the left (edges have exactly 1 path).
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: dp[r][c] = dp[r-1][c] + dp[r][c-1]; first row/col = 1.
                public static int UniquePaths(int m, int n)
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
                    r.Check("3x7 -> 28", () => Assert.Equal(28, Solution.UniquePaths(3, 7)));
                    r.Check("3x2 -> 3", () => Assert.Equal(3, Solution.UniquePaths(3, 2)));
                    r.Check("1x1 -> 1", () => Assert.Equal(1, Solution.UniquePaths(1, 1)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static int UniquePaths(int m, int n)
                {
                    var dp = new int[m, n];
                    for (int r = 0; r < m; r++)
                        for (int c = 0; c < n; c++)
                            dp[r, c] = (r == 0 || c == 0)
                                ? 1                                   // only one way along an edge
                                : dp[r - 1, c] + dp[r, c - 1];        // from above + from the left
                    return dp[m - 1, n - 1];
                }
            }
            """,
        Hints =
        [
            "Every cell in the first row or first column has exactly one path.",
            "Otherwise dp[r][c] = dp[r-1][c] + dp[r][c-1].",
            "The answer is the bottom-right cell.",
        ],
        TestCases =
        [
            new() { Name = "3x7 -> 28", IsHidden = false },
            new() { Name = "3x2 -> 3", IsHidden = false },
        ],
    };
}
