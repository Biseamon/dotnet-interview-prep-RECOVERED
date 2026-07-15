namespace InterviewPrep.Infrastructure.Data.Seeding.Algorithms;

// Lesson 2 — Two Pointers & Sliding Window. Linear scans that maintain a pair of
// indices (or a moving window) to avoid nested loops.
internal static partial class AlgorithmsContent
{
    private static LessonSeed TwoPointersLesson => new()
    {
        Slug = "two-pointers",
        Title = "Two Pointers & Sliding Window",
        Order = 2,
        MarkdownContent =
            """
            ## Two Pointers & Sliding Window

            Two indices moving through a sequence turn many O(n²) problems into O(n):

            - **Opposite ends** converging (`left`/`right`) — palindromes, sorted-pair sums.
            - **Sliding window** — a `[left, right]` range that grows/shrinks to satisfy a
              constraint (longest substring without repeats, etc.).
            - **Fast/slow** — cycle detection (Floyd's tortoise & hare).
            """,
        Exercises =
        [
            ValidPalindrome,
            TwoSumSorted,
            LongestSubstringNoRepeat,
            MaxProfit,
        ],
    };

    private static ExerciseSeed ValidPalindrome => new()
    {
        Slug = "valid-palindrome",
        Title = "Valid Palindrome",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Return `true` if `s` reads the same forwards and backwards, considering
            **only alphanumeric characters** and ignoring case. Use two pointers from
            the ends inward — O(n) time, O(1) space.
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: two pointers from both ends, skip non-alphanumerics, compare.
                public static bool IsPalindrome(string s)
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
                    r.Check("'A man, a plan, a canal: Panama' -> true", () =>
                        Assert.True(Solution.IsPalindrome("A man, a plan, a canal: Panama")));
                    r.Check("'race a car' -> false", () => Assert.False(Solution.IsPalindrome("race a car")));
                    r.Check("' ' -> true", () => Assert.True(Solution.IsPalindrome(" ")));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public static class Solution
            {
                public static bool IsPalindrome(string s)
                {
                    int l = 0, r = s.Length - 1;
                    while (l < r)
                    {
                        // Advance past characters we ignore.
                        while (l < r && !char.IsLetterOrDigit(s[l])) l++;
                        while (l < r && !char.IsLetterOrDigit(s[r])) r--;
                        if (char.ToLowerInvariant(s[l]) != char.ToLowerInvariant(s[r]))
                            return false;
                        l++; r--;
                    }
                    return true;
                }
            }
            """,
        Hints =
        [
            "Keep a `left` and `right` index moving toward each other.",
            "Skip characters where `char.IsLetterOrDigit` is false.",
            "Compare lowercased characters; mismatch means not a palindrome.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "'A man, a plan, a canal: Panama' -> true", IsHidden = false },
            new TestCaseSeed { Name = "'race a car' -> false", IsHidden = false },
        ],
    };

    private static ExerciseSeed TwoSumSorted => new()
    {
        Slug = "two-sum-sorted",
        Title = "Two Sum II (Sorted Input)",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Given a **sorted** array, return the 1-based indices of the two numbers
            that add up to `target`. Because it's sorted, two pointers from the ends
            give O(n) time, O(1) space (no hash map needed).
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: two pointers; move them based on whether the sum is too big/small.
                public static int[] TwoSumSorted(int[] numbers, int target)
                {
                    throw new NotImplementedException();
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
                    r.Check("[2,7,11,15], 9 -> [1,2]", () =>
                        Assert.Equal("1,2", string.Join(",", Solution.TwoSumSorted(new[]{2,7,11,15}, 9))));
                    r.Check("[2,3,4], 6 -> [1,3]", () =>
                        Assert.Equal("1,3", string.Join(",", Solution.TwoSumSorted(new[]{2,3,4}, 6))));
                    r.Check("[-1,0], -1 -> [1,2]", () =>
                        Assert.Equal("1,2", string.Join(",", Solution.TwoSumSorted(new[]{-1,0}, -1))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public static class Solution
            {
                public static int[] TwoSumSorted(int[] numbers, int target)
                {
                    int l = 0, r = numbers.Length - 1;
                    while (l < r)
                    {
                        int sum = numbers[l] + numbers[r];
                        if (sum == target) return new[] { l + 1, r + 1 }; // 1-based
                        if (sum < target) l++;   // need a bigger sum -> move left up
                        else r--;                // too big -> move right down
                    }
                    return Array.Empty<int>();
                }
            }
            """,
        Hints =
        [
            "The array is sorted — exploit that instead of hashing.",
            "Start pointers at both ends. If the sum is too small, advance the left pointer; if too big, retreat the right.",
            "Return 1-based indices.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[2,7,11,15], 9 -> [1,2]", IsHidden = false },
            new TestCaseSeed { Name = "[2,3,4], 6 -> [1,3]", IsHidden = false },
        ],
    };

    private static ExerciseSeed LongestSubstringNoRepeat => new()
    {
        Slug = "longest-substring-no-repeat",
        Title = "Longest Substring Without Repeating Characters",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Return the length of the longest substring of `s` with **no repeating
            characters**. Classic **sliding window**: grow the right edge, and when a
            duplicate appears, shrink from the left until it's gone.
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: sliding window; track characters currently in the window.
                public static int LengthOfLongestSubstring(string s)
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
                    r.Check("'abcabcbb' -> 3", () => Assert.Equal(3, Solution.LengthOfLongestSubstring("abcabcbb")));
                    r.Check("'bbbbb' -> 1", () => Assert.Equal(1, Solution.LengthOfLongestSubstring("bbbbb")));
                    r.Check("'pwwkew' -> 3", () => Assert.Equal(3, Solution.LengthOfLongestSubstring("pwwkew")));
                    r.Check("'' -> 0", () => Assert.Equal(0, Solution.LengthOfLongestSubstring("")));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Collections.Generic;

            public static class Solution
            {
                public static int LengthOfLongestSubstring(string s)
                {
                    var window = new HashSet<char>();
                    int left = 0, best = 0;
                    for (int right = 0; right < s.Length; right++)
                    {
                        // Shrink from the left until s[right] is not a duplicate.
                        while (window.Contains(s[right]))
                            window.Remove(s[left++]);
                        window.Add(s[right]);
                        best = Math.Max(best, right - left + 1);
                    }
                    return best;
                }
            }
            """,
        Hints =
        [
            "Maintain a window [left, right] of unique characters.",
            "Use a HashSet of characters currently in the window.",
            "When s[right] is already in the set, remove s[left] and advance left until it's free.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "'abcabcbb' -> 3", IsHidden = false },
            new TestCaseSeed { Name = "'pwwkew' -> 3", IsHidden = false },
            new TestCaseSeed { Name = "'' -> 0", IsHidden = true },
        ],
    };

    private static ExerciseSeed MaxProfit => new()
    {
        Slug = "best-time-buy-sell-stock",
        Title = "Best Time to Buy and Sell Stock",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Given daily `prices`, return the maximum profit from **one** buy then a
            later sell (0 if no profit is possible). Track the minimum price seen so
            far and the best profit — one pass, O(n).
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: track min price so far and best profit in one pass.
                public static int MaxProfit(int[] prices)
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
                    r.Check("[7,1,5,3,6,4] -> 5", () => Assert.Equal(5, Solution.MaxProfit(new[]{7,1,5,3,6,4})));
                    r.Check("[7,6,4,3,1] -> 0", () => Assert.Equal(0, Solution.MaxProfit(new[]{7,6,4,3,1})));
                    r.Check("[1] -> 0", () => Assert.Equal(0, Solution.MaxProfit(new[]{1})));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public static class Solution
            {
                public static int MaxProfit(int[] prices)
                {
                    int minSoFar = int.MaxValue, best = 0;
                    foreach (var p in prices)
                    {
                        minSoFar = Math.Min(minSoFar, p);      // cheapest buy up to here
                        best = Math.Max(best, p - minSoFar);   // best sell at today's price
                    }
                    return best;
                }
            }
            """,
        Hints =
        [
            "You must buy before you sell — scan left to right.",
            "Track the minimum price seen so far.",
            "At each day, the best sale today is price - minSoFar; keep the max.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[7,1,5,3,6,4] -> 5", IsHidden = false },
            new TestCaseSeed { Name = "[7,6,4,3,1] -> 0", IsHidden = false },
        ],
    };
}
