namespace InterviewPrep.Infrastructure.Data.Seeding.Algorithms;

// Lesson 1 — Arrays & Hashing. The bread-and-butter of coding rounds: using a
// hash set/map to trade space for O(n) time instead of an O(n^2) scan.
internal static partial class AlgorithmsContent
{
    private static LessonSeed ArraysAndHashingLesson => new()
    {
        Slug = "arrays-hashing",
        Title = "Arrays & Hashing",
        Order = 1,
        MarkdownContent =
            """
            ## Arrays & Hashing

            The single most common optimization in interviews: replace a nested-loop
            **O(n²)** scan with a **hash set / dictionary** for **O(n)** time, spending
            O(n) extra space. Watch for: "have I seen this before?", "how many times?",
            and "group things by a key".

            - `HashSet<T>` — membership in O(1).
            - `Dictionary<K,V>` — counts, indices, grouping.
            - Remember: hashing trades **space for time**.
            """,
        Exercises =
        [
            TwoSum,
            ContainsDuplicate,
            ValidAnagram,
            GroupAnagrams,
            TopKFrequent,
        ],
    };

    private static ExerciseSeed TwoSum => new()
    {
        Slug = "two-sum",
        Title = "Two Sum",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Given an integer array `nums` and a target, return the **indices** of the
            two numbers that add up to `target`. Exactly one solution exists; you may
            not reuse the same element. Aim for **O(n)** using a dictionary.
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: return the two indices whose values sum to target.
                public static int[] TwoSum(int[] nums, int target)
                {
                    throw new NotImplementedException();
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
                    r.Check("[2,7,11,15], 9 -> [0,1]", () =>
                        Assert.Equal("0,1", Norm(Solution.TwoSum(new[]{2,7,11,15}, 9))));
                    r.Check("[3,2,4], 6 -> [1,2]", () =>
                        Assert.Equal("1,2", Norm(Solution.TwoSum(new[]{3,2,4}, 6))));
                    r.Check("[3,3], 6 -> [0,1]", () =>
                        Assert.Equal("0,1", Norm(Solution.TwoSum(new[]{3,3}, 6))));
                    return r.ToJson();
                }

                // Order-independent comparison of an index pair.
                private static string Norm(int[] pair)
                {
                    var s = pair.OrderBy(x => x).ToArray();
                    return s[0] + "," + s[1];
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Collections.Generic;

            public static class Solution
            {
                public static int[] TwoSum(int[] nums, int target)
                {
                    // value -> index seen so far. For each element, check if its
                    // complement (target - value) was already seen: that's the pair.
                    var seen = new Dictionary<int, int>();
                    for (int i = 0; i < nums.Length; i++)
                    {
                        int need = target - nums[i];
                        if (seen.TryGetValue(need, out int j))
                            return new[] { j, i };
                        seen[nums[i]] = i; // record AFTER the check to avoid reuse
                    }
                    return Array.Empty<int>();
                }
            }
            """,
        Hints =
        [
            "A brute-force double loop is O(n²). Can a lookup remove the inner loop?",
            "As you scan, store each value's index in a dictionary.",
            "For each value, check whether `target - value` is already in the dictionary.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[2,7,11,15], 9 -> [0,1]", IsHidden = false },
            new TestCaseSeed { Name = "[3,2,4], 6 -> [1,2]", IsHidden = false },
            new TestCaseSeed { Name = "[3,3], 6 -> [0,1]", IsHidden = true },
        ],
    };

    private static ExerciseSeed ContainsDuplicate => new()
    {
        Slug = "contains-duplicate",
        Title = "Contains Duplicate",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Return `true` if any value appears **at least twice** in `nums`, and
            `false` if every element is distinct. Target **O(n)** time.
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: return true if any value repeats.
                public static bool ContainsDuplicate(int[] nums)
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
                    r.Check("[1,2,3,1] -> true", () => Assert.True(Solution.ContainsDuplicate(new[]{1,2,3,1})));
                    r.Check("[1,2,3,4] -> false", () => Assert.False(Solution.ContainsDuplicate(new[]{1,2,3,4})));
                    r.Check("[] -> false", () => Assert.False(Solution.ContainsDuplicate(new int[0])));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                public static bool ContainsDuplicate(int[] nums)
                {
                    // HashSet.Add returns false if the value is already present.
                    var seen = new HashSet<int>();
                    foreach (var n in nums)
                        if (!seen.Add(n)) return true;
                    return false;
                }
            }
            """,
        Hints =
        [
            "A HashSet gives O(1) membership checks.",
            "`HashSet<T>.Add` returns false when the item already exists — use that directly.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[1,2,3,1] -> true", IsHidden = false },
            new TestCaseSeed { Name = "[1,2,3,4] -> false", IsHidden = false },
            new TestCaseSeed { Name = "[] -> false", IsHidden = true },
        ],
    };

    private static ExerciseSeed ValidAnagram => new()
    {
        Slug = "valid-anagram",
        Title = "Valid Anagram",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Given two strings `s` and `t`, return `true` if `t` is an anagram of `s`
            (same characters, same counts). Aim for **O(n)** with a frequency count.
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: return true if t is an anagram of s.
                public static bool IsAnagram(string s, string t)
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
                    r.Check("anagram/nagaram -> true", () => Assert.True(Solution.IsAnagram("anagram","nagaram")));
                    r.Check("rat/car -> false", () => Assert.False(Solution.IsAnagram("rat","car")));
                    r.Check("different lengths -> false", () => Assert.False(Solution.IsAnagram("a","ab")));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                public static bool IsAnagram(string s, string t)
                {
                    if (s.Length != t.Length) return false; // quick reject
                    var counts = new Dictionary<char, int>();
                    foreach (var c in s) counts[c] = counts.GetValueOrDefault(c) + 1;
                    foreach (var c in t)
                    {
                        if (!counts.TryGetValue(c, out int n) || n == 0) return false;
                        counts[c] = n - 1; // consume one occurrence
                    }
                    return true;
                }
            }
            """,
        Hints =
        [
            "Anagrams have identical character frequencies.",
            "Count characters of s in a dictionary, then decrement while scanning t.",
            "If lengths differ they can't be anagrams — reject early.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "anagram/nagaram -> true", IsHidden = false },
            new TestCaseSeed { Name = "rat/car -> false", IsHidden = false },
            new TestCaseSeed { Name = "different lengths -> false", IsHidden = true },
        ],
    };

    private static ExerciseSeed GroupAnagrams => new()
    {
        Slug = "group-anagrams",
        Title = "Group Anagrams",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Group the strings that are anagrams of each other. Return the number of
            groups. (We check the group *count* to keep grading order-independent.)
            Classic approach: key each word by its **sorted characters**.
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: return how many anagram groups the words form.
                public static int GroupCount(string[] words)
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
                    r.Check("eat,tea,tan,ate,nat,bat -> 3", () =>
                        Assert.Equal(3, Solution.GroupCount(new[]{"eat","tea","tan","ate","nat","bat"})));
                    r.Check("[\"\"] -> 1", () => Assert.Equal(1, Solution.GroupCount(new[]{""})));
                    r.Check("a,b,c -> 3", () => Assert.Equal(3, Solution.GroupCount(new[]{"a","b","c"})));
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
                public static int GroupCount(string[] words)
                {
                    // Anagrams share the same multiset of letters, so their SORTED form
                    // is identical — use it as the group key.
                    var groups = new HashSet<string>();
                    foreach (var w in words)
                    {
                        var chars = w.ToCharArray();
                        Array.Sort(chars);
                        groups.Add(new string(chars));
                    }
                    return groups.Count;
                }
            }
            """,
        Hints =
        [
            "Two words are anagrams iff their sorted characters match.",
            "Use the sorted string as a dictionary/set key.",
            "The number of distinct keys is the number of groups.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "eat,tea,tan,ate,nat,bat -> 3", IsHidden = false },
            new TestCaseSeed { Name = "a,b,c -> 3", IsHidden = false },
        ],
    };

    private static ExerciseSeed TopKFrequent => new()
    {
        Slug = "top-k-frequent",
        Title = "Top K Frequent Elements",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Return the `k` most frequent elements in `nums` (any order). Count with a
            dictionary, then take the k highest counts. **Bucket sort** by frequency
            gives O(n); sorting is fine too.
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: return the k most frequent values (order doesn't matter).
                public static int[] TopKFrequent(int[] nums, int k)
                {
                    throw new NotImplementedException();
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
                    r.Check("[1,1,1,2,2,3], k=2 -> {1,2}", () =>
                        Assert.Equal("1,2", Set(Solution.TopKFrequent(new[]{1,1,1,2,2,3}, 2))));
                    r.Check("[1], k=1 -> {1}", () =>
                        Assert.Equal("1", Set(Solution.TopKFrequent(new[]{1}, 1))));
                    r.Check("[4,4,5,5,6], k=2 -> {4,5}", () =>
                        Assert.Equal("4,5", Set(Solution.TopKFrequent(new[]{4,4,5,5,6}, 2))));
                    return r.ToJson();
                }

                // Order-independent: sort the returned values and join.
                private static string Set(int[] xs) => string.Join(",", xs.OrderBy(x => x));
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            public static class Solution
            {
                public static int[] TopKFrequent(int[] nums, int k)
                {
                    // Count occurrences, then take the k keys with the largest counts.
                    var freq = new Dictionary<int, int>();
                    foreach (var n in nums) freq[n] = freq.GetValueOrDefault(n) + 1;

                    return freq.OrderByDescending(kv => kv.Value)
                               .Take(k)
                               .Select(kv => kv.Key)
                               .ToArray();
                }
            }
            """,
        Hints =
        [
            "First build a value -> count dictionary.",
            "Then select the k entries with the highest counts.",
            "OrderByDescending on the count then Take(k) is the simplest correct approach.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[1,1,1,2,2,3], k=2 -> {1,2}", IsHidden = false },
            new TestCaseSeed { Name = "[4,4,5,5,6], k=2 -> {4,5}", IsHidden = false },
        ],
    };
}
