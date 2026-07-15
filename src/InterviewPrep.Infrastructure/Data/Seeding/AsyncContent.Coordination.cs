namespace InterviewPrep.Infrastructure.Data.Seeding;

// Lesson 2 for the Async topic — Task coordination: retry, timeout, and first-to-win.
internal static partial class AsyncContent
{
    private static LessonSeed CoordinationLesson => new()
    {
        Slug = "task-coordination",
        Title = "Task Coordination",
        Order = 2,
        MarkdownContent =
            """
            ## Coordinating more than one task

            `Task.WhenAll` waits for *all* tasks; `Task.WhenAny` completes as soon as the
            *first* one does. Almost every real-world async pattern is built from these two
            plus `await` and `try/catch`:

            - **Retry** — re-attempt a flaky operation a few times before giving up, usually
              with a growing **backoff** delay so you don't hammer a struggling dependency.
            - **Timeout** — race the real work against `Task.Delay` with `Task.WhenAny`;
              whichever finishes first wins. If the delay wins, you time out.
            - **First-to-complete (hedging)** — fire the same request at several replicas and
              take whichever answers first.
            - **Fan-out / error aggregation** — start many tasks, `WhenAll` them, and decide
              how to surface failures.

            ## Why interviewers probe this

            Coordination is where async bugs hide: silent swallowed exceptions, retries that
            loop forever, timeouts that leak the underlying task, and `WhenAll` that only shows
            you the *first* of several failures. Being able to reason about these out loud is a
            strong signal.

            ## Common traps

            - **`Task.WhenAny` doesn't cancel the loser.** When the timeout wins, the real task
              keeps running in the background. In production you'd pass it a `CancellationToken`
              to actually stop the work; `WhenAny` alone only stops *waiting*.
            - **`await Task.WhenAll(tasks)` only rethrows the FIRST exception**, even though the
              returned task's `.Exception` is an `AggregateException` holding *all* of them. If
              you need every error, inspect the faulted tasks yourself.
            - **Retrying non-idempotent work** (a POST that charges a card) can double-charge.
              Retry is for transient, idempotent failures.
            - **Fixed-delay retries** stampede a recovering service. Grow the delay each attempt
              (linear or exponential backoff).

            ## Worked example: timeout that surfaces the real result

            ```csharp
            var winner = await Task.WhenAny(work, Task.Delay(ms));
            if (winner != work) throw new TimeoutException();
            return await work; // already done — also re-throws work's own exception if it failed
            ```

            The final `await work` matters: even though the task is complete, awaiting it is how
            you unwrap the value *and* let any exception it stored propagate cleanly.
            """,
        Exercises =
        [
            RetryAsync,
            RetryWithBackoffAsync,
            TimeoutAsync,
            FirstToComplete,
            WhenAllFirstError,
        ],
    };

    private static ExerciseSeed RetryAsync => new()
    {
        Slug = "retry-async",
        Title = "Retry an Async Operation",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Implement `RetryAsync(operation, maxAttempts)` that awaits `operation`; if it
            throws, retry up to `maxAttempts` total. Return the result on success, or
            rethrow the last exception if all attempts fail.
            """,
        StarterCode =
            """
            using System;
            using System.Threading.Tasks;

            public static class Solution
            {
                // TODO: try up to maxAttempts; return on success, rethrow after the last failure.
                public static async Task<int> RetryAsync(Func<Task<int>> operation, int maxAttempts)
                {
                    throw new NotImplementedException();
                }
            }
            """,
        HarnessCode =
            """
            using System;
            using System.Threading.Tasks;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();

                    r.Check("succeeds after 2 failures", () =>
                    {
                        int calls = 0;
                        Func<Task<int>> op = () =>
                        {
                            calls++;
                            if (calls < 3) throw new InvalidOperationException("flaky");
                            return Task.FromResult(42);
                        };
                        Assert.Equal(42, Solution.RetryAsync(op, 5).GetAwaiter().GetResult());
                    });

                    r.Check("rethrows if all attempts fail", () =>
                    {
                        Func<Task<int>> op = () => throw new InvalidOperationException("always");
                        bool threw = false;
                        try { Solution.RetryAsync(op, 3).GetAwaiter().GetResult(); }
                        catch (InvalidOperationException) { threw = true; }
                        Assert.True(threw);
                    });

                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Threading.Tasks;

            public static class Solution
            {
                public static async Task<int> RetryAsync(Func<Task<int>> operation, int maxAttempts)
                {
                    for (int attempt = 1; ; attempt++)
                    {
                        try
                        {
                            return await operation(); // success -> done
                        }
                        catch when (attempt < maxAttempts)
                        {
                            // Swallow and retry only while attempts remain; the filter lets the
                            // final failure propagate naturally.
                        }
                    }
                }
            }
            """,
        Hints =
        [
            "Loop up to maxAttempts times, awaiting the operation inside a try.",
            "On success, return immediately.",
            "Use an exception filter (`catch when (attempt < maxAttempts)`) so the last failure rethrows.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "succeeds after 2 failures", IsHidden = false },
            new TestCaseSeed { Name = "rethrows if all attempts fail", IsHidden = false },
        ],
    };

    private static ExerciseSeed TimeoutAsync => new()
    {
        Slug = "timeout-async",
        Title = "Add a Timeout to a Task",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Implement `WithTimeout(task, milliseconds)`: return the task's result if it
            finishes in time, otherwise throw `TimeoutException`. Race the task against
            `Task.Delay` using `Task.WhenAny`.
            """,
        StarterCode =
            """
            using System;
            using System.Threading.Tasks;

            public static class Solution
            {
                // TODO: race `task` vs Task.Delay(ms); throw TimeoutException if delay wins.
                public static async Task<int> WithTimeout(Task<int> task, int milliseconds)
                {
                    throw new NotImplementedException();
                }
            }
            """,
        HarnessCode =
            """
            using System;
            using System.Threading.Tasks;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();

                    r.Check("fast task returns its value", () =>
                    {
                        var fast = Task.FromResult(7);
                        Assert.Equal(7, Solution.WithTimeout(fast, 200).GetAwaiter().GetResult());
                    });

                    r.Check("slow task times out", () =>
                    {
                        var slow = Task.Delay(1000).ContinueWith(_ => 1);
                        bool threw = false;
                        try { Solution.WithTimeout(slow, 50).GetAwaiter().GetResult(); }
                        catch (TimeoutException) { threw = true; }
                        Assert.True(threw);
                    });

                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Threading.Tasks;

            public static class Solution
            {
                public static async Task<int> WithTimeout(Task<int> task, int milliseconds)
                {
                    // Whichever completes first wins the race.
                    var winner = await Task.WhenAny(task, Task.Delay(milliseconds));
                    if (winner != task)
                        throw new TimeoutException();
                    return await task; // already complete; also surfaces its exceptions
                }
            }
            """,
        Hints =
        [
            "Task.WhenAny returns the first task to finish.",
            "Compare the winner to your task; if it's the delay, throw TimeoutException.",
            "Otherwise `await task` to get (and unwrap) the result.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "fast task returns its value", IsHidden = false },
            new TestCaseSeed { Name = "slow task times out", IsHidden = false },
        ],
    };

    private static ExerciseSeed FirstToComplete => new()
    {
        Slug = "first-to-complete",
        Title = "First Task to Complete",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Implement `FirstValue(tasks)` returning the result of whichever task completes
            first (e.g. the fastest of several redundant data sources). Use `Task.WhenAny`.
            """,
        StarterCode =
            """
            using System.Threading.Tasks;

            public static class Solution
            {
                // TODO: return the result of the first task to finish.
                public static async Task<int> FirstValue(Task<int>[] tasks)
                {
                    throw new System.NotImplementedException();
                }
            }
            """,
        HarnessCode =
            """
            using System.Threading.Tasks;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("returns the already-completed task's value", () =>
                    {
                        var tasks = new[]
                        {
                            Task.Delay(500).ContinueWith(_ => 1),
                            Task.FromResult(2), // already done -> wins
                            Task.Delay(500).ContinueWith(_ => 3),
                        };
                        Assert.Equal(2, Solution.FirstValue(tasks).GetAwaiter().GetResult());
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Threading.Tasks;

            public static class Solution
            {
                public static async Task<int> FirstValue(Task<int>[] tasks)
                {
                    var first = await Task.WhenAny(tasks); // the first task to complete
                    return await first;                    // unwrap its result
                }
            }
            """,
        Hints =
        [
            "Task.WhenAny(tasks) completes when the first of them does.",
            "It returns the winning Task<int> — await it to get the value.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "returns the already-completed task's value", IsHidden = false },
        ],
    };

    private static ExerciseSeed RetryWithBackoffAsync => new()
    {
        Slug = "retry-with-backoff",
        Title = "Retry with Backoff (Count the Attempts)",
        Difficulty = "Medium",
        Kind = "Function",
        TimeoutSeconds = 5,
        Prompt =
            """
            Implement `RetryWithBackoffAsync(operation, maxAttempts, baseDelayMs)`.

            Await `operation`. If it throws, wait `baseDelayMs * attemptNumber` milliseconds
            (linear backoff: `baseDelayMs` after the 1st failure, `2 * baseDelayMs` after the
            2nd, ...) and try again — up to `maxAttempts` total. Return the result on success,
            or rethrow the last exception if every attempt fails.

            Do **not** delay after the final attempt (there's no point sleeping before giving
            up). Keep delays tiny in tests.
            """,
        StarterCode =
            """
            using System;
            using System.Threading.Tasks;

            public static class Solution
            {
                // TODO: retry up to maxAttempts, sleeping baseDelayMs * attempt between tries.
                public static async Task<int> RetryWithBackoffAsync(
                    Func<Task<int>> operation, int maxAttempts, int baseDelayMs)
                {
                    throw new NotImplementedException();
                }
            }
            """,
        HarnessCode =
            """
            using System;
            using System.Threading.Tasks;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();

                    r.Check("succeeds on the 3rd attempt", () =>
                    {
                        int calls = 0;
                        Func<Task<int>> op = () =>
                        {
                            calls++;
                            if (calls < 3) throw new InvalidOperationException("flaky");
                            return Task.FromResult(99);
                        };
                        var result = Solution.RetryWithBackoffAsync(op, 5, 5).GetAwaiter().GetResult();
                        Assert.Equal(99, result);
                        Assert.Equal(3, calls); // exactly three attempts were made
                    });

                    r.Check("backs off between attempts", () =>
                    {
                        int calls = 0;
                        Func<Task<int>> op = () =>
                        {
                            calls++;
                            if (calls < 3) throw new InvalidOperationException("flaky");
                            return Task.FromResult(1);
                        };
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        Solution.RetryWithBackoffAsync(op, 5, 20).GetAwaiter().GetResult();
                        sw.Stop();
                        // Two failures => waits ~20ms then ~40ms => at least ~55ms total.
                        Assert.True(sw.ElapsedMilliseconds >= 45,
                            $"expected linear backoff (~60ms), took {sw.ElapsedMilliseconds}ms");
                    });

                    r.Check("rethrows after exhausting attempts", () =>
                    {
                        int calls = 0;
                        Func<Task<int>> op = () => { calls++; throw new InvalidOperationException("always"); };
                        bool threw = false;
                        try { Solution.RetryWithBackoffAsync(op, 3, 1).GetAwaiter().GetResult(); }
                        catch (InvalidOperationException) { threw = true; }
                        Assert.True(threw);
                        Assert.Equal(3, calls); // tried exactly maxAttempts times
                    });

                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Threading.Tasks;

            public static class Solution
            {
                public static async Task<int> RetryWithBackoffAsync(
                    Func<Task<int>> operation, int maxAttempts, int baseDelayMs)
                {
                    for (int attempt = 1; ; attempt++)
                    {
                        try
                        {
                            return await operation();
                        }
                        catch when (attempt < maxAttempts)
                        {
                            // Linear backoff: grow the wait with each failed attempt. We only
                            // reach here when attempts remain, so we never sleep before giving up.
                            await Task.Delay(baseDelayMs * attempt);
                        }
                        // On the final attempt the filter is false, so the exception propagates.
                    }
                }
            }
            """,
        Hints =
        [
            "Loop with an `attempt` counter starting at 1; return on success.",
            "Use `catch when (attempt < maxAttempts)` so the final failure rethrows and you never delay after the last try.",
            "Inside the catch, `await Task.Delay(baseDelayMs * attempt)` for linear backoff.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "succeeds on the 3rd attempt", IsHidden = false },
            new TestCaseSeed { Name = "backs off between attempts", IsHidden = false },
            new TestCaseSeed { Name = "rethrows after exhausting attempts", IsHidden = true },
        ],
    };

    private static ExerciseSeed WhenAllFirstError => new()
    {
        Slug = "whenall-first-error",
        Title = "Gather Results, Surface the First Failure",
        Difficulty = "Hard",
        Kind = "Function",
        TimeoutSeconds = 5,
        Prompt =
            """
            Implement `SumAllAsync(Func<Task<int>>[] operations)`.

            Start **all** operations concurrently and sum their results. If they all succeed,
            return the total. If any of them throw, surface the **first** exception (the one
            `await Task.WhenAll` rethrows) rather than swallowing it.

            Edge cases: an **empty** array returns `0` (the sum of nothing). All work should
            run concurrently, not one call at a time.

            Note: `await Task.WhenAll(tasks)` conveniently rethrows the first exception for you
            while still waiting for every task to settle.
            """,
        StarterCode =
            """
            using System;
            using System.Linq;
            using System.Threading.Tasks;

            public static class Solution
            {
                // TODO: run all operations concurrently; sum results, or surface the first error.
                public static async Task<int> SumAllAsync(Func<Task<int>>[] operations)
                {
                    throw new NotImplementedException();
                }
            }
            """,
        HarnessCode =
            """
            using System;
            using System.Threading.Tasks;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();

                    r.Check("sums all successful results", () =>
                    {
                        Func<Task<int>>[] ops =
                        {
                            async () => { await Task.Delay(10); return 10; },
                            async () => { await Task.Delay(5);  return 20; },
                            () => Task.FromResult(30),
                        };
                        var total = Solution.SumAllAsync(ops).GetAwaiter().GetResult();
                        Assert.Equal(60, total);
                    });

                    r.Check("empty input returns 0", () =>
                    {
                        var total = Solution.SumAllAsync(Array.Empty<Func<Task<int>>>())
                            .GetAwaiter().GetResult();
                        Assert.Equal(0, total);
                    });

                    r.Check("surfaces a failure", () =>
                    {
                        Func<Task<int>>[] ops =
                        {
                            () => Task.FromResult(1),
                            async () => { await Task.Delay(5); throw new InvalidOperationException("boom"); },
                            () => Task.FromResult(3),
                        };
                        bool threw = false;
                        try { Solution.SumAllAsync(ops).GetAwaiter().GetResult(); }
                        catch (InvalidOperationException) { threw = true; }
                        Assert.True(threw);
                    });

                    r.Check("runs concurrently", () =>
                    {
                        Func<Task<int>>[] ops =
                        {
                            async () => { await Task.Delay(150); return 1; },
                            async () => { await Task.Delay(150); return 2; },
                            async () => { await Task.Delay(150); return 3; },
                        };
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        Solution.SumAllAsync(ops).GetAwaiter().GetResult();
                        sw.Stop();
                        // Concurrent ≈150ms; sequential ≈450ms. Wide margin so the check
                        // is robust even when the test host is under parallel load.
                        Assert.True(sw.ElapsedMilliseconds < 340,
                            $"took {sw.ElapsedMilliseconds}ms — did you await inside a loop?");
                    });

                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Linq;
            using System.Threading.Tasks;

            public static class Solution
            {
                public static async Task<int> SumAllAsync(Func<Task<int>>[] operations)
                {
                    // Invoke each now to start them concurrently (don't await between starts).
                    var tasks = operations.Select(op => op()).ToArray();

                    // WhenAll returns results in input order; awaiting it rethrows the first
                    // exception if any task faulted. Empty input yields an empty array => sum 0.
                    int[] results = await Task.WhenAll(tasks);
                    return results.Sum();
                }
            }
            """,
        Hints =
        [
            "Call each operation first to start it, collecting an array of running tasks.",
            "`await Task.WhenAll(tasks)` gives you an `int[]` (in order) and rethrows the first error automatically.",
            "Sum the array with LINQ's `.Sum()`; an empty array sums to 0.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "sums all successful results", IsHidden = false },
            new TestCaseSeed { Name = "empty input returns 0", IsHidden = false },
            new TestCaseSeed { Name = "surfaces a failure", IsHidden = false },
            new TestCaseSeed { Name = "runs concurrently", IsHidden = true },
        ],
    };

    // =========================================================================
    // Lesson 3 — Concurrency Control: throttling and producer/consumer.
    // =========================================================================
    private static LessonSeed ConcurrencyControlLesson => new()
    {
        Slug = "concurrency-control",
        Title = "Concurrency Control",
        Order = 3,
        MarkdownContent =
            """
            ## Controlling *how much* runs at once

            Running work concurrently is easy; running the *right amount* concurrently is the
            real skill. Fire 10,000 HTTP calls with `Task.WhenAll` and you'll exhaust sockets,
            trip rate limits, and melt the downstream service. Two tools tame this:

            ### `SemaphoreSlim` — a permit-based throttle

            A `SemaphoreSlim(k)` hands out at most **k** permits. Each task does
            `await gate.WaitAsync()` before its work and `gate.Release()` after (always in a
            `finally`). At most `k` tasks hold a permit at once, so you get **bounded
            parallelism**: start all the tasks, but only `k` run their critical section
            concurrently.

            ```csharp
            var gate = new SemaphoreSlim(maxDegree);
            var tasks = items.Select(async item =>
            {
                await gate.WaitAsync();
                try { return await ProcessAsync(item); }
                finally { gate.Release(); }   // release even if ProcessAsync throws
            });
            var results = await Task.WhenAll(tasks);
            ```

            ### `Channel<T>` — a thread-safe async queue

            `System.Threading.Channels` is the modern **producer/consumer** primitive. Producers
            `await writer.WriteAsync(item)`; when done they call `writer.Complete()`. Consumers
            drain it with `await foreach (var item in reader.ReadAllAsync())`, which ends cleanly
            once the channel is completed. It's allocation-light, lock-free on the hot path, and
            supports backpressure via **bounded** channels.

            ## Why interviewers probe this

            "Process this list concurrently, but never more than N at a time" is one of the most
            common real system-design follow-ups. It tests whether you know `Task.WhenAll` alone
            is a foot-gun at scale, and whether you can reach for the right primitive.

            ## Common traps

            - **Releasing outside `finally`.** If the work throws, a missing `Release()` leaks a
              permit forever and eventually deadlocks the whole pipeline.
            - **Forgetting `writer.Complete()`.** Consumers using `ReadAllAsync()` will hang
              forever waiting for items that never come.
            - **Confusing "started" with "running."** All tasks may be *started*, but the
              semaphore ensures only `k` are past the gate at any moment.
            """,
        Exercises =
        [
            BoundedParallelism,
            ChannelSum,
        ],
    };

    private static ExerciseSeed BoundedParallelism => new()
    {
        Slug = "bounded-parallelism",
        Title = "Bounded Parallelism with SemaphoreSlim",
        Difficulty = "Hard",
        Kind = "Function",
        TimeoutSeconds = 5,
        Prompt =
            """
            Implement `RunThrottledAsync(Func<Task<int>>[] work, int maxConcurrency)`.

            Run every item in `work` and return their results **in input order**, but never let
            more than `maxConcurrency` of them run their body at the same time. Use a
            `SemaphoreSlim` as the gate: acquire a permit before invoking each function, and
            release it (in a `finally`) after.

            Edge cases: empty `work` returns an empty array; if `maxConcurrency >= work.Length`
            everything runs at once.
            """,
        StarterCode =
            """
            using System;
            using System.Linq;
            using System.Threading.Tasks;
            using System.Threading;

            public static class Solution
            {
                // TODO: run all work items but at most maxConcurrency at once; keep input order.
                public static async Task<int[]> RunThrottledAsync(Func<Task<int>>[] work, int maxConcurrency)
                {
                    throw new NotImplementedException();
                }
            }
            """,
        HarnessCode =
            """
            using System;
            using System.Linq;
            using System.Threading;
            using System.Threading.Tasks;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();

                    r.Check("returns results in input order", () =>
                    {
                        Func<Task<int>>[] work =
                        {
                            async () => { await Task.Delay(30); return 1; },
                            async () => { await Task.Delay(10); return 2; },
                            async () => { await Task.Delay(20); return 3; },
                            async () => { await Task.Delay(5);  return 4; },
                        };
                        var result = Solution.RunThrottledAsync(work, 2).GetAwaiter().GetResult();
                        Assert.Equal("1,2,3,4", string.Join(",", result));
                    });

                    r.Check("never exceeds max concurrency", () =>
                    {
                        int current = 0, peak = 0;
                        object gate = new object();
                        Func<Task<int>> make(int v) => async () =>
                        {
                            lock (gate) { current++; if (current > peak) peak = current; }
                            await Task.Delay(30);
                            lock (gate) { current--; }
                            return v;
                        };
                        var work = Enumerable.Range(1, 8).Select(make).ToArray();
                        Solution.RunThrottledAsync(work, 3).GetAwaiter().GetResult();
                        Assert.True(peak <= 3, $"peak concurrency was {peak}, expected <= 3");
                        Assert.True(peak >= 2, $"peak concurrency was {peak}; work did not overlap");
                    });

                    r.Check("empty work returns empty array", () =>
                    {
                        var result = Solution.RunThrottledAsync(Array.Empty<Func<Task<int>>>(), 3)
                            .GetAwaiter().GetResult();
                        Assert.Equal(0, result.Length);
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
                public static async Task<int[]> RunThrottledAsync(Func<Task<int>>[] work, int maxConcurrency)
                {
                    using var gate = new SemaphoreSlim(maxConcurrency);

                    // Start one wrapper task per item. Each waits for a permit before running its
                    // body, so at most maxConcurrency are past the gate at once. Select preserves
                    // order, and WhenAll returns results in that same order.
                    var tasks = work.Select(async f =>
                    {
                        await gate.WaitAsync();
                        try { return await f(); }
                        finally { gate.Release(); } // release even if f() throws
                    });

                    return await Task.WhenAll(tasks);
                }
            }
            """,
        Hints =
        [
            "Create `new SemaphoreSlim(maxConcurrency)` as the gate.",
            "Wrap each work item: `await gate.WaitAsync()`, run it in a `try`, `gate.Release()` in a `finally`.",
            "`Task.WhenAll` over the wrapper tasks preserves input order for the results.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "returns results in input order", IsHidden = false },
            new TestCaseSeed { Name = "never exceeds max concurrency", IsHidden = false },
            new TestCaseSeed { Name = "empty work returns empty array", IsHidden = true },
        ],
    };

    private static ExerciseSeed ChannelSum => new()
    {
        Slug = "channel-producer-consumer",
        Title = "Producer/Consumer with Channel<T>",
        Difficulty = "Hard",
        Kind = "Function",
        TimeoutSeconds = 5,
        Prompt =
            """
            Implement `ProduceThenConsumeAsync(int n)` using `System.Threading.Channels`.

            Spin up a **producer** that writes the integers `1, 2, ..., n` into an unbounded
            `Channel<int>` and then **completes** the writer. Concurrently run a **consumer**
            that reads every item with `await foreach (... in reader.ReadAllAsync())` and sums
            them. Await both, then return the total (which equals `n * (n + 1) / 2`).

            Edge cases: `n == 0` writes nothing, completes immediately, and returns `0`. Make
            sure you call `writer.Complete()` — otherwise the consumer waits forever.
            """,
        StarterCode =
            """
            using System.Threading.Channels;
            using System.Threading.Tasks;

            public static class Solution
            {
                // TODO: producer writes 1..n then Completes; consumer sums via ReadAllAsync.
                public static async Task<int> ProduceThenConsumeAsync(int n)
                {
                    throw new System.NotImplementedException();
                }
            }
            """,
        HarnessCode =
            """
            using System.Threading.Tasks;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();

                    r.Check("sums 1..10 == 55", () =>
                        Assert.Equal(55, Solution.ProduceThenConsumeAsync(10).GetAwaiter().GetResult()));

                    r.Check("sums 1..100 == 5050", () =>
                        Assert.Equal(5050, Solution.ProduceThenConsumeAsync(100).GetAwaiter().GetResult()));

                    r.Check("n == 0 returns 0 (completes cleanly)", () =>
                        Assert.Equal(0, Solution.ProduceThenConsumeAsync(0).GetAwaiter().GetResult()));

                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Threading.Channels;
            using System.Threading.Tasks;

            public static class Solution
            {
                public static async Task<int> ProduceThenConsumeAsync(int n)
                {
                    var channel = Channel.CreateUnbounded<int>();

                    // Producer: write 1..n, then signal there will be no more items.
                    var producer = Task.Run(async () =>
                    {
                        for (int i = 1; i <= n; i++)
                            await channel.Writer.WriteAsync(i);
                        channel.Writer.Complete(); // without this, ReadAllAsync never ends
                    });

                    // Consumer: drain until the channel is completed, summing as we go.
                    var consumer = Task.Run(async () =>
                    {
                        int sum = 0;
                        await foreach (var item in channel.Reader.ReadAllAsync())
                            sum += item;
                        return sum;
                    });

                    await producer;
                    return await consumer;
                }
            }
            """,
        Hints =
        [
            "Create the queue with `Channel.CreateUnbounded<int>()`.",
            "Producer loops `await channel.Writer.WriteAsync(i)` then calls `channel.Writer.Complete()`.",
            "Consumer sums via `await foreach (var x in channel.Reader.ReadAllAsync())`; await both tasks.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "sums 1..10 == 55", IsHidden = false },
            new TestCaseSeed { Name = "sums 1..100 == 5050", IsHidden = false },
            new TestCaseSeed { Name = "n == 0 returns 0 (completes cleanly)", IsHidden = true },
        ],
    };
}
