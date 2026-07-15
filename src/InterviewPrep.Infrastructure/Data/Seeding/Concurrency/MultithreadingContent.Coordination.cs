namespace InterviewPrep.Infrastructure.Data.Seeding.Concurrency;

// Lesson 2 for Concurrency — coordinating concurrent work: throttling with a
// semaphore, and thread-safe collections.
internal static partial class MultithreadingContent
{
    private static LessonSeed CoordinationLesson => new()
    {
        Slug = "coordination",
        Title = "Coordination & Concurrent Collections",
        Order = 2,
        MarkdownContent =
            """
            ## Coordination & Concurrent Collections

            - **SemaphoreSlim** — cap how many operations run at once (a permit pool). Great
              for throttling calls to a rate-limited service.
            - **ConcurrentDictionary** — a lock-free-ish map safe for concurrent readers and
              writers; `AddOrUpdate` handles the read-modify-write atomically.
            """,
        Exercises =
        [
            SemaphoreThrottle,
            ConcurrentCount,
        ],
    };

    private static ExerciseSeed SemaphoreThrottle => new()
    {
        Slug = "semaphore-throttle",
        Title = "Throttle with SemaphoreSlim",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Implement `RunThrottled(ops, limit)` that runs all `ops` but with **at most
            `limit` running concurrently**. Use a `SemaphoreSlim(limit)`: await a permit
            before each op, release it after. Await all ops before returning.
            """,
        StarterCode =
            """
            using System;
            using System.Threading.Tasks;

            public static class Solution
            {
                // TODO: gate each op with a SemaphoreSlim(limit); run all, then await them.
                public static async Task RunThrottled(Func<Task>[] ops, int limit)
                {
                    throw new NotImplementedException();
                }
            }
            """,
        HarnessCode =
            """
            using System;
            using System.Linq;
            using System.Threading.Tasks;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("never exceeds the concurrency limit", () =>
                    {
                        int active = 0, peak = 0, completed = 0;
                        var gate = new object();
                        var ops = Enumerable.Range(0, 12).Select(_ => (Func<Task>)(async () =>
                        {
                            lock (gate) { active++; if (active > peak) peak = active; }
                            await Task.Delay(25);
                            lock (gate) { active--; completed++; }
                        })).ToArray();

                        Solution.RunThrottled(ops, 3).GetAwaiter().GetResult();

                        Assert.True(peak <= 3, $"peak concurrency was {peak}, expected <= 3");
                        Assert.Equal(12, completed);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Linq;
            using System.Threading;
            using System.Threading.Tasks;

            public static class Solution
            {
                public static async Task RunThrottled(Func<Task>[] ops, int limit)
                {
                    using var semaphore = new SemaphoreSlim(limit);
                    var tasks = ops.Select(async op =>
                    {
                        await semaphore.WaitAsync();   // acquire a permit (blocks past `limit`)
                        try { await op(); }
                        finally { semaphore.Release(); } // always give the permit back
                    });
                    await Task.WhenAll(tasks);
                }
            }
            """,
        Hints =
        [
            "SemaphoreSlim(limit) hands out `limit` permits.",
            "Each op awaits a permit, runs, then releases in a finally.",
            "Start all ops (they queue on the semaphore) and Task.WhenAll them.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "never exceeds the concurrency limit", IsHidden = false },
        ],
    };

    private static ExerciseSeed ConcurrentCount => new()
    {
        Slug = "concurrent-count",
        Title = "Count with ConcurrentDictionary",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Count word frequencies while processing the input **in parallel**. Use a
            `ConcurrentDictionary<string,int>` with `AddOrUpdate` so concurrent increments
            don't lose updates. Return the count for the given `word`.
            """,
        StarterCode =
            """
            using System.Collections.Concurrent;
            using System.Threading.Tasks;

            public static class Solution
            {
                // TODO: tally words in parallel with a ConcurrentDictionary; return count of `word`.
                public static int CountOf(string[] words, string word)
                {
                    return 0;
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
                    var words = Enumerable.Repeat("a", 500)
                        .Concat(Enumerable.Repeat("b", 300))
                        .Concat(Enumerable.Repeat("c", 200))
                        .ToArray();
                    r.Check("counts 'a' -> 500 under parallelism", () =>
                        Assert.Equal(500, Solution.CountOf(words, "a")));
                    r.Check("counts 'b' -> 300", () =>
                        Assert.Equal(300, Solution.CountOf(words, "b")));
                    r.Check("missing word -> 0", () =>
                        Assert.Equal(0, Solution.CountOf(words, "z")));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Concurrent;
            using System.Collections.Generic;
            using System.Threading.Tasks;

            public static class Solution
            {
                public static int CountOf(string[] words, string word)
                {
                    var counts = new ConcurrentDictionary<string, int>();
                    // AddOrUpdate is atomic per key — safe under Parallel.ForEach.
                    Parallel.ForEach(words, w =>
                        counts.AddOrUpdate(w, 1, (_, existing) => existing + 1));
                    return counts.TryGetValue(word, out var n) ? n : 0;
                }
            }
            """,
        Hints =
        [
            "A plain Dictionary isn't safe for concurrent writes — use ConcurrentDictionary.",
            "AddOrUpdate(key, 1, (k, old) => old + 1) increments atomically.",
            "Process the words with Parallel.ForEach, then read the target count.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "counts 'a' -> 500 under parallelism", IsHidden = false },
            new TestCaseSeed { Name = "counts 'b' -> 300", IsHidden = false },
            new TestCaseSeed { Name = "missing word -> 0", IsHidden = true },
        ],
    };
}
