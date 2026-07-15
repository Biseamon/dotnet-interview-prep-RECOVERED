namespace InterviewPrep.Infrastructure.Data.Seeding.Algorithms;

// Lesson 6 — Binary Search. Halving the search space each step for O(log n). The
// hard part is always the boundary conditions.
internal static partial class AlgorithmsContent
{
    private static LessonSeed BinarySearchLesson => new()
    {
        Slug = "binary-search",
        Title = "Binary Search",
        Order = 6,
        MarkdownContent =
            """
            ## Binary Search

            On sorted data, compare the middle and discard half the range each step —
            **O(log n)**. Pitfalls interviewers watch for:
            - `mid = low + (high - low) / 2` avoids integer overflow.
            - Get the loop bound (`<` vs `<=`) and the update (`mid ± 1`) right.
            - Many problems reduce to "binary search on the answer".
            """,
        Exercises =
        [
            BinarySearchExercise,
            SearchRotated,
            KthLargest,
        ],
    };

    private static ExerciseSeed BinarySearchExercise => new()
    {
        Slug = "binary-search",
        Title = "Binary Search",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Given a **sorted** array and a target, return the index of the target or
            `-1` if absent. Textbook binary search — O(log n).
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: classic binary search. Return index or -1.
                public static int Search(int[] nums, int target)
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
                    r.Check("[-1,0,3,5,9,12], 9 -> 4", () => Assert.Equal(4, Solution.Search(new[]{-1,0,3,5,9,12}, 9)));
                    r.Check("[-1,0,3,5,9,12], 2 -> -1", () => Assert.Equal(-1, Solution.Search(new[]{-1,0,3,5,9,12}, 2)));
                    r.Check("[5], 5 -> 0", () => Assert.Equal(0, Solution.Search(new[]{5}, 5)));
                    r.Check("[], 1 -> -1", () => Assert.Equal(-1, Solution.Search(new int[0], 1)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static int Search(int[] nums, int target)
                {
                    int low = 0, high = nums.Length - 1;
                    while (low <= high)
                    {
                        int mid = low + (high - low) / 2; // overflow-safe midpoint
                        if (nums[mid] == target) return mid;
                        if (nums[mid] < target) low = mid + 1;  // target is to the right
                        else high = mid - 1;                    // target is to the left
                    }
                    return -1;
                }
            }
            """,
        Hints =
        [
            "Maintain a [low, high] range and inspect the middle.",
            "Use `low + (high - low) / 2` to avoid overflow.",
            "Loop while low <= high; move low/high past mid depending on the comparison.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[-1,0,3,5,9,12], 9 -> 4", IsHidden = false },
            new TestCaseSeed { Name = "[-1,0,3,5,9,12], 2 -> -1", IsHidden = false },
            new TestCaseSeed { Name = "[], 1 -> -1", IsHidden = true },
        ],
    };

    private static ExerciseSeed SearchRotated => new()
    {
        Slug = "search-rotated-sorted-array",
        Title = "Search in Rotated Sorted Array",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            A sorted array was rotated at an unknown pivot (e.g. `[4,5,6,7,0,1,2]`).
            Find the index of `target`, or `-1`. Still O(log n): at each step one half
            is sorted — decide which, then narrow.
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: modified binary search; identify the sorted half each step.
                public static int Search(int[] nums, int target)
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
                    r.Check("[4,5,6,7,0,1,2], 0 -> 4", () => Assert.Equal(4, Solution.Search(new[]{4,5,6,7,0,1,2}, 0)));
                    r.Check("[4,5,6,7,0,1,2], 3 -> -1", () => Assert.Equal(-1, Solution.Search(new[]{4,5,6,7,0,1,2}, 3)));
                    r.Check("[1], 0 -> -1", () => Assert.Equal(-1, Solution.Search(new[]{1}, 0)));
                    r.Check("[5,1,3], 5 -> 0", () => Assert.Equal(0, Solution.Search(new[]{5,1,3}, 5)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static int Search(int[] nums, int target)
                {
                    int low = 0, high = nums.Length - 1;
                    while (low <= high)
                    {
                        int mid = low + (high - low) / 2;
                        if (nums[mid] == target) return mid;

                        if (nums[low] <= nums[mid]) // left half [low..mid] is sorted
                        {
                            if (nums[low] <= target && target < nums[mid]) high = mid - 1;
                            else low = mid + 1;
                        }
                        else // right half [mid..high] is sorted
                        {
                            if (nums[mid] < target && target <= nums[high]) low = mid + 1;
                            else high = mid - 1;
                        }
                    }
                    return -1;
                }
            }
            """,
        Hints =
        [
            "At any mid, at least one side (low..mid or mid..high) is fully sorted.",
            "Check which side is sorted by comparing nums[low] and nums[mid].",
            "If target lies within the sorted side's range, search there; else search the other side.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[4,5,6,7,0,1,2], 0 -> 4", IsHidden = false },
            new TestCaseSeed { Name = "[4,5,6,7,0,1,2], 3 -> -1", IsHidden = false },
        ],
    };

    private static ExerciseSeed KthLargest => new()
    {
        Slug = "kth-largest-element",
        Title = "Kth Largest Element in an Array",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Return the `k`th **largest** element (by value, with duplicates counted).
            A min-heap of size k is the classic O(n log k) approach; a sort is fine too.
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: return the kth largest value.
                public static int FindKthLargest(int[] nums, int k)
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
                    r.Check("[3,2,1,5,6,4], k=2 -> 5", () => Assert.Equal(5, Solution.FindKthLargest(new[]{3,2,1,5,6,4}, 2)));
                    r.Check("[3,2,3,1,2,4,5,5,6], k=4 -> 4", () => Assert.Equal(4, Solution.FindKthLargest(new[]{3,2,3,1,2,4,5,5,6}, 4)));
                    r.Check("[1], k=1 -> 1", () => Assert.Equal(1, Solution.FindKthLargest(new[]{1}, 1)));
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
                public static int FindKthLargest(int[] nums, int k)
                {
                    // Min-heap of the k largest seen so far. The heap's root is the kth
                    // largest once all elements are processed. PriorityQueue is a min-heap.
                    var heap = new PriorityQueue<int, int>();
                    foreach (var n in nums)
                    {
                        heap.Enqueue(n, n);
                        if (heap.Count > k) heap.Dequeue(); // drop the smallest
                    }
                    return heap.Peek();
                }
            }
            """,
        Hints =
        [
            "Sorting descending and taking index k-1 works (O(n log n)).",
            "For O(n log k), keep a min-heap of size k (C#'s PriorityQueue is a min-heap).",
            "After processing all elements, the heap's smallest is the kth largest overall.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[3,2,1,5,6,4], k=2 -> 5", IsHidden = false },
            new TestCaseSeed { Name = "[3,2,3,1,2,4,5,5,6], k=4 -> 4", IsHidden = false },
        ],
    };
}
