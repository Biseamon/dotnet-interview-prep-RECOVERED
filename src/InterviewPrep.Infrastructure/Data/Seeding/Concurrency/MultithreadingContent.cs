namespace InterviewPrep.Infrastructure.Data.Seeding.Concurrency;

// The "Multithreading & Concurrency" topic. Exercises are graded by actually
// spawning threads and asserting correctness — a correct (synchronized) solution
// passes reliably; the point is to teach the primitives that make it correct.
internal static partial class MultithreadingContent
{
    public static TopicSeed Topic => new()
    {
        Slug = "multithreading",
        Name = "Multithreading & Concurrency",
        Description = "Locks, atomics, parallelism, and safe one-time initialization under real threads.",
        Order = 4,
        Lessons =
        [
            SynchronizationLesson,
            CoordinationLesson,
        ],
    };

    private static LessonSeed SynchronizationLesson => new()
    {
        Slug = "synchronization",
        Title = "Synchronization & Atomics",
        Order = 1,
        MarkdownContent =
            """
            ## Synchronization & Atomics

            When multiple threads touch shared state, you need to prevent **races**:
            - `lock` (Monitor) — mutual exclusion around a critical section.
            - `Interlocked` — lock-free atomic operations (`Increment`, `Add`, `Exchange`).
            - `Lazy<T>` — thread-safe one-time initialization.
            - Partition work, accumulate **locally**, then combine — minimizes contention.
            """,
        Exercises =
        [
            ThreadSafeCounter,
            ParallelSum,
            LazyOnceInit,
        ],
    };

    private static ExerciseSeed ThreadSafeCounter => new()
    {
        Slug = "thread-safe-counter",
        Title = "Thread-Safe Counter",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement a `Counter` whose `Increment()` is safe to call from many threads
            at once, with `Value` returning the exact total. A plain `_count++` is a
            **race** (read-modify-write isn't atomic) — use `lock` or `Interlocked`.
            """,
        StarterCode =
            """
            public sealed class Counter
            {
                // TODO: make Increment safe under concurrency.
                public void Increment() => throw new System.NotImplementedException();
                public int Value => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            using System.Collections.Generic;
            using System.Threading;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("10 threads x 1000 increments -> 10000", () =>
                    {
                        var counter = new Counter();
                        var threads = new List<Thread>();
                        for (int t = 0; t < 10; t++)
                        {
                            var th = new Thread(() => { for (int i = 0; i < 1000; i++) counter.Increment(); });
                            threads.Add(th);
                            th.Start();
                        }
                        foreach (var th in threads) th.Join();
                        Assert.Equal(10000, counter.Value);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Threading;

            public sealed class Counter
            {
                private int _count;

                // Interlocked.Increment is a single atomic operation — no lock needed,
                // no torn read-modify-write.
                public void Increment() => Interlocked.Increment(ref _count);

                // Volatile.Read ensures we observe the latest written value.
                public int Value => Volatile.Read(ref _count);
            }
            """,
        Hints =
        [
            "`_count++` is three operations (read, add, write) — two threads can interleave and lose updates.",
            "`Interlocked.Increment(ref _count)` performs the whole thing atomically.",
            "Alternatively wrap `_count++` in a `lock` — either fixes the race.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "10 threads x 1000 increments -> 10000", IsHidden = false },
        ],
    };

    private static ExerciseSeed ParallelSum => new()
    {
        Slug = "parallel-sum",
        Title = "Parallel Sum",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Sum a large array using multiple threads and return the total as a `long`.
            The efficient pattern: partition the range, accumulate a **local** subtotal
            per partition, then combine with `Interlocked.Add` — minimizing contention.
            """,
        StarterCode =
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            public static class Solution
            {
                // TODO: sum concurrently. Accumulate locally, combine atomically.
                public static long SumConcurrently(int[] nums)
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
                    r.Check("sum 1..10000 -> 50005000", () =>
                    {
                        var nums = Enumerable.Range(1, 10000).ToArray();
                        Assert.Equal(50005000L, Solution.SumConcurrently(nums));
                    });
                    r.Check("empty -> 0", () => Assert.Equal(0L, Solution.SumConcurrently(new int[0])));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Concurrent;
            using System.Threading;
            using System.Threading.Tasks;

            public static class Solution
            {
                public static long SumConcurrently(int[] nums)
                {
                    if (nums.Length == 0) return 0; // Partitioner requires a non-empty range
                    long total = 0;
                    // Partitioner splits the index range into chunks handled in parallel.
                    Parallel.ForEach(
                        Partitioner.Create(0, nums.Length),
                        range =>
                        {
                            long local = 0; // no shared state inside the loop
                            for (int i = range.Item1; i < range.Item2; i++)
                                local += nums[i];
                            Interlocked.Add(ref total, local); // combine once per partition
                        });
                    return total;
                }
            }
            """,
        Hints =
        [
            "Don't Interlocked.Add on every element — that serializes all threads.",
            "Give each partition its own local subtotal.",
            "Add each partition's local total to the shared total once, atomically.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "sum 1..10000 -> 50005000", IsHidden = false },
            new TestCaseSeed { Name = "empty -> 0", IsHidden = true },
        ],
    };

    private static ExerciseSeed LazyOnceInit => new()
    {
        Slug = "lazy-once-init",
        Title = "Thread-Safe Lazy Initialization",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement `LazyService` so `Instance` is created **once**, even when many
            threads request it simultaneously, and every caller gets the *same* object.
            Track `CreationCount` (incremented in the factory) to prove single init.
            `Lazy<T>` gives you all of this for free.
            """,
        StarterCode =
            """
            public sealed class LazyService
            {
                // Incremented each time an instance is actually constructed. Must end at 1.
                public static int CreationCount;

                // TODO: return the one shared instance, created lazily & thread-safely.
                public static LazyService Instance => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            using System.Collections.Generic;
            using System.Threading.Tasks;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("concurrent access creates exactly one instance", () =>
                    {
                        var seen = new HashSet<LazyService>();
                        var lockObj = new object();
                        Parallel.For(0, 50, _ =>
                        {
                            var inst = LazyService.Instance;
                            lock (lockObj) seen.Add(inst);
                        });
                        Assert.Equal(1, seen.Count);            // all the same instance
                        Assert.Equal(1, LazyService.CreationCount); // factory ran once
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Threading;

            public sealed class LazyService
            {
                public static int CreationCount;

                // Lazy<T> guarantees the factory runs at most once, even under races
                // (default LazyThreadSafetyMode.ExecutionAndPublication).
                private static readonly Lazy<LazyService> _lazy = new(() =>
                {
                    Interlocked.Increment(ref CreationCount);
                    return new LazyService();
                });

                private LazyService() { }

                public static LazyService Instance => _lazy.Value;
            }
            """,
        Hints =
        [
            "A naive `if (_instance == null) _instance = new(...)` races under threads.",
            "`Lazy<T>` with the default mode runs its factory exactly once.",
            "Expose `_lazy.Value` from Instance; increment CreationCount inside the factory.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "concurrent access creates exactly one instance", IsHidden = false },
        ],
    };
}
