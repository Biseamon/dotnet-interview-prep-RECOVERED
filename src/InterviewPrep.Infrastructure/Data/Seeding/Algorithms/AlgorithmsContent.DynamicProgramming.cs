namespace InterviewPrep.Infrastructure.Data.Seeding.Algorithms;

// Lesson 7 — Dynamic Programming. Break a problem into overlapping subproblems and
// cache their answers (bottom-up tabulation or top-down memoization).
internal static partial class AlgorithmsContent
{
    private static LessonSeed DynamicProgrammingLesson => new()
    {
        Slug = "dynamic-programming",
        Title = "Dynamic Programming",
        Order = 7,
        MarkdownContent =
            """
            ## Dynamic Programming

            DP applies when a problem has **optimal substructure** (the answer is built from
            answers to subproblems) and **overlapping subproblems** (the same subproblems
            recur). Cache subproblem results to turn exponential recursion into linear time.

            Recipe: define the state, write the recurrence (how a state depends on smaller
            ones), pick base cases, then fill a table bottom-up (or memoize top-down).
            """,
        Exercises =
        [
            ClimbingStairs,
            HouseRobber,
            CoinChange,
            LongestIncreasingSubsequence,
        ],
    };

    private static ExerciseSeed ClimbingStairs => new()
    {
        Slug = "climbing-stairs",
        Title = "Climbing Stairs",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            You climb 1 or 2 steps at a time. Return the number of distinct ways to reach
            step `n`. The recurrence is Fibonacci: `ways(n) = ways(n-1) + ways(n-2)`.
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: ways(n) = ways(n-1) + ways(n-2), iteratively.
                public static int ClimbStairs(int n)
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
                    r.Check("n=2 -> 2", () => Assert.Equal(2, Solution.ClimbStairs(2)));
                    r.Check("n=3 -> 3", () => Assert.Equal(3, Solution.ClimbStairs(3)));
                    r.Check("n=5 -> 8", () => Assert.Equal(8, Solution.ClimbStairs(5)));
                    r.Check("n=1 -> 1", () => Assert.Equal(1, Solution.ClimbStairs(1)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static int ClimbStairs(int n)
                {
                    // Only the last two results are needed — O(1) space.
                    int prev = 1, curr = 1;
                    for (int i = 2; i <= n; i++)
                    {
                        int next = prev + curr;
                        prev = curr;
                        curr = next;
                    }
                    return curr;
                }
            }
            """,
        Hints =
        [
            "To reach step n you came from n-1 (one step) or n-2 (two steps).",
            "So ways(n) = ways(n-1) + ways(n-2) — Fibonacci.",
            "Track just the previous two values; no array needed.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "n=2 -> 2", IsHidden = false },
            new TestCaseSeed { Name = "n=5 -> 8", IsHidden = false },
            new TestCaseSeed { Name = "n=1 -> 1", IsHidden = true },
        ],
    };

    private static ExerciseSeed HouseRobber => new()
    {
        Slug = "house-robber",
        Title = "House Robber",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Given house values, return the maximum you can rob without taking two
            **adjacent** houses. State: best up to house i is
            `max(best(i-1), best(i-2) + value[i])`.
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: DP — for each house, skip it or take it + best two back.
                public static int Rob(int[] nums)
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
                    r.Check("[1,2,3,1] -> 4", () => Assert.Equal(4, Solution.Rob(new[]{1,2,3,1})));
                    r.Check("[2,7,9,3,1] -> 12", () => Assert.Equal(12, Solution.Rob(new[]{2,7,9,3,1})));
                    r.Check("[] -> 0", () => Assert.Equal(0, Solution.Rob(new int[0])));
                    r.Check("[5] -> 5", () => Assert.Equal(5, Solution.Rob(new[]{5})));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public static class Solution
            {
                public static int Rob(int[] nums)
                {
                    int prev = 0, curr = 0; // best excluding / including consideration
                    foreach (var value in nums)
                    {
                        // Either skip this house (curr) or rob it (prev + value).
                        int next = Math.Max(curr, prev + value);
                        prev = curr;
                        curr = next;
                    }
                    return curr;
                }
            }
            """,
        Hints =
        [
            "At each house you either skip it (keep the previous best) or rob it (best from two houses back + this value).",
            "Track two rolling values: best up to i-1 and i-2.",
            "The answer is the best after the last house.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[1,2,3,1] -> 4", IsHidden = false },
            new TestCaseSeed { Name = "[2,7,9,3,1] -> 12", IsHidden = false },
            new TestCaseSeed { Name = "[] -> 0", IsHidden = true },
        ],
    };

    private static ExerciseSeed CoinChange => new()
    {
        Slug = "coin-change",
        Title = "Coin Change (Fewest Coins)",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Given coin denominations and a target `amount`, return the **fewest** coins that
            sum to it, or `-1` if impossible. Bottom-up: `dp[a] = 1 + min(dp[a - coin])`
            over all coins.
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: dp over amounts 0..amount; dp[a] = fewest coins to make a.
                public static int CoinChange(int[] coins, int amount)
                {
                    return -1;
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
                    r.Check("[1,2,5], 11 -> 3", () => Assert.Equal(3, Solution.CoinChange(new[]{1,2,5}, 11)));
                    r.Check("[2], 3 -> -1", () => Assert.Equal(-1, Solution.CoinChange(new[]{2}, 3)));
                    r.Check("[1], 0 -> 0", () => Assert.Equal(0, Solution.CoinChange(new[]{1}, 0)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public static class Solution
            {
                public static int CoinChange(int[] coins, int amount)
                {
                    // dp[a] = fewest coins to make amount a. Use amount+1 as "infinity".
                    var dp = new int[amount + 1];
                    Array.Fill(dp, amount + 1);
                    dp[0] = 0;

                    for (int a = 1; a <= amount; a++)
                        foreach (var coin in coins)
                            if (coin <= a)
                                dp[a] = Math.Min(dp[a], dp[a - coin] + 1);

                    return dp[amount] > amount ? -1 : dp[amount];
                }
            }
            """,
        Hints =
        [
            "dp[a] = fewest coins to make amount a; dp[0] = 0.",
            "For each amount, try each coin: dp[a] = min(dp[a], dp[a-coin] + 1).",
            "Initialize with a sentinel (amount+1); if it survives, return -1.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[1,2,5], 11 -> 3", IsHidden = false },
            new TestCaseSeed { Name = "[2], 3 -> -1", IsHidden = false },
            new TestCaseSeed { Name = "[1], 0 -> 0", IsHidden = true },
        ],
    };

    private static ExerciseSeed LongestIncreasingSubsequence => new()
    {
        Slug = "longest-increasing-subsequence",
        Title = "Longest Increasing Subsequence",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Return the length of the longest strictly-increasing subsequence (not
            necessarily contiguous). O(n²) DP: `dp[i]` = LIS ending at `i` =
            `1 + max(dp[j])` for `j < i` with `nums[j] < nums[i]`.
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: dp[i] = longest increasing subsequence ending at i.
                public static int LengthOfLIS(int[] nums)
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
                    r.Check("[10,9,2,5,3,7,101,18] -> 4", () =>
                        Assert.Equal(4, Solution.LengthOfLIS(new[]{10,9,2,5,3,7,101,18})));
                    r.Check("[0,1,0,3,2,3] -> 4", () =>
                        Assert.Equal(4, Solution.LengthOfLIS(new[]{0,1,0,3,2,3})));
                    r.Check("[7,7,7] -> 1", () => Assert.Equal(1, Solution.LengthOfLIS(new[]{7,7,7})));
                    r.Check("[] -> 0", () => Assert.Equal(0, Solution.LengthOfLIS(new int[0])));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public static class Solution
            {
                public static int LengthOfLIS(int[] nums)
                {
                    if (nums.Length == 0) return 0;
                    var dp = new int[nums.Length];
                    Array.Fill(dp, 1); // each element alone is a subsequence of length 1

                    int best = 1;
                    for (int i = 1; i < nums.Length; i++)
                    {
                        for (int j = 0; j < i; j++)
                            if (nums[j] < nums[i])
                                dp[i] = Math.Max(dp[i], dp[j] + 1);
                        best = Math.Max(best, dp[i]);
                    }
                    return best;
                }
            }
            """,
        Hints =
        [
            "dp[i] is the LIS length that ends exactly at index i.",
            "For each i, look back at all j < i with nums[j] < nums[i] and extend the best.",
            "The answer is the maximum dp value.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[10,9,2,5,3,7,101,18] -> 4", IsHidden = false },
            new TestCaseSeed { Name = "[0,1,0,3,2,3] -> 4", IsHidden = false },
            new TestCaseSeed { Name = "[] -> 0", IsHidden = true },
        ],
    };
}
