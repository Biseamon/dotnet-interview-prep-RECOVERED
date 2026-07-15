namespace InterviewPrep.Infrastructure.Data.Seeding.Algorithms;

// Lesson 9 — Intervals. Sort by start, then sweep — the pattern behind merging,
// overlap detection, and resource scheduling.
internal static partial class AlgorithmsContent
{
    private static LessonSeed IntervalsLesson => new()
    {
        Slug = "intervals",
        Title = "Intervals",
        Order = 9,
        MarkdownContent =
            """
            ## Intervals

            Most interval problems start the same way: **sort by start time**, then sweep
            left to right. From there you can merge overlaps, count overlaps, or find gaps.
            A running "current end" (or a min-heap of end times) does the rest.
            """,
        Exercises =
        [
            MergeIntervals,
            MeetingRooms,
        ],
    };

    private static ExerciseSeed MergeIntervals => new()
    {
        Slug = "merge-intervals",
        Title = "Merge Intervals",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Given intervals `[start, end]`, merge all overlapping ones and return the
            result sorted by start, formatted as `"start-end"` strings. Sort first, then
            extend the current interval while the next one overlaps.
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: sort by start; merge while intervals overlap. Return "s-e" strings.
                public static string[] Merge(int[][] intervals)
                {
                    return new string[0];
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
                    r.Check("[[1,3],[2,6],[8,10],[15,18]] -> 1-6,8-10,15-18", () =>
                        Assert.Equal("1-6,8-10,15-18", string.Join(",",
                            Solution.Merge(new[]{ new[]{1,3}, new[]{2,6}, new[]{8,10}, new[]{15,18} }))));
                    r.Check("[[1,4],[4,5]] -> 1-5 (touching merges)", () =>
                        Assert.Equal("1-5", string.Join(",",
                            Solution.Merge(new[]{ new[]{1,4}, new[]{4,5} }))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            public static class Solution
            {
                public static string[] Merge(int[][] intervals)
                {
                    if (intervals.Length == 0) return Array.Empty<string>();

                    var sorted = intervals.OrderBy(iv => iv[0]).ToArray();
                    var merged = new List<int[]> { sorted[0] };

                    foreach (var iv in sorted.Skip(1))
                    {
                        var last = merged[^1];
                        if (iv[0] <= last[1])                 // overlaps (or touches)
                            last[1] = Math.Max(last[1], iv[1]); // extend the current end
                        else
                            merged.Add(iv);                    // disjoint -> start a new one
                    }

                    return merged.Select(iv => $"{iv[0]}-{iv[1]}").ToArray();
                }
            }
            """,
        Hints =
        [
            "Sort intervals by their start value first.",
            "Keep a 'current' interval; if the next starts on/before current end, extend the end.",
            "Otherwise the next interval is disjoint — begin a new current interval.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[[1,3],[2,6],[8,10],[15,18]] -> 1-6,8-10,15-18", IsHidden = false },
            new TestCaseSeed { Name = "[[1,4],[4,5]] -> 1-5 (touching merges)", IsHidden = false },
        ],
    };

    private static ExerciseSeed MeetingRooms => new()
    {
        Slug = "meeting-rooms",
        Title = "Meeting Rooms (Min Rooms)",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Given meeting `[start, end]` intervals, return the **minimum number of rooms**
            needed so no two overlapping meetings share a room. Classic approach: sort start
            times and end times separately, then sweep with two pointers counting concurrency.
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: find peak concurrency via sorted starts/ends two-pointer sweep.
                public static int MinRooms(int[][] meetings)
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
                    r.Check("[[0,30],[5,10],[15,20]] -> 2", () =>
                        Assert.Equal(2, Solution.MinRooms(new[]{ new[]{0,30}, new[]{5,10}, new[]{15,20} })));
                    r.Check("[[7,10],[2,4]] -> 1 (no overlap)", () =>
                        Assert.Equal(1, Solution.MinRooms(new[]{ new[]{7,10}, new[]{2,4} })));
                    r.Check("[] -> 0", () => Assert.Equal(0, Solution.MinRooms(new int[0][])));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Linq;

            public static class Solution
            {
                public static int MinRooms(int[][] meetings)
                {
                    if (meetings.Length == 0) return 0;

                    var starts = meetings.Select(m => m[0]).OrderBy(x => x).ToArray();
                    var ends   = meetings.Select(m => m[1]).OrderBy(x => x).ToArray();

                    int rooms = 0, maxRooms = 0, e = 0;
                    foreach (var start in starts)
                    {
                        // If a meeting has ended by this start time, free its room.
                        if (start >= ends[e]) { rooms--; e++; }
                        rooms++;                 // this meeting needs a room
                        maxRooms = Math.Max(maxRooms, rooms);
                    }
                    return maxRooms;
                }
            }
            """,
        Hints =
        [
            "The answer is the maximum number of meetings happening at the same time.",
            "Sort start times and end times into two separate arrays.",
            "Sweep starts; whenever a meeting has already ended (start >= earliest end), reuse that room.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "[[0,30],[5,10],[15,20]] -> 2", IsHidden = false },
            new TestCaseSeed { Name = "[[7,10],[2,4]] -> 1 (no overlap)", IsHidden = false },
            new TestCaseSeed { Name = "[] -> 0", IsHidden = true },
        ],
    };
}
