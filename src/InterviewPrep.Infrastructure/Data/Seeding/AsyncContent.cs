namespace InterviewPrep.Infrastructure.Data.Seeding;

// Authored content for the "Async & Multithreading" topic. Using C# raw string
// literals (""" ... """) keeps embedded C# (starter/harness/solution) perfectly
// readable and diffable — far nicer than escaping newlines inside JSON.
//
// HARNESS CONTRACT (shared by every exercise, implemented by the grader's shim):
//   public static class __Harness { public static string Run() { ... } }   ← entry point
//   var r = new HarnessReport();                                            ← collects cases
//   r.Check("label", () => Assert.Equal(expected, actual));                 ← one test case
//   return r.ToJson();                                                      ← primitives only
// The user's code is compiled as a class named `Solution` in the same assembly,
// so the harness can call it directly (compile-time bound → clean errors).
internal static partial class AsyncContent
{
    public static TopicSeed Topic => new()
    {
        Slug = "async",
        Name = "Async & Multithreading",
        Description = "Task-based async, concurrency, and the pitfalls interviewers love to probe.",
        Order = 1,
        Lessons =
        [
            new LessonSeed
            {
                Slug = "async-basics",
                Title = "Async Fundamentals",
                Order = 1,
                MarkdownContent =
                    """
                    ## What `async`/`await` actually does

                    `async`/`await` lets a method **pause at an I/O wait without blocking a thread**.
                    When you `await` a `Task` that isn't finished yet, the method **returns control to
                    its caller** and the thread is freed to do other work. When the awaited work
                    completes, a continuation resumes the rest of your method — often on a different
                    thread. Nothing is running "in the background" for you; `await` is about *not
                    holding a thread hostage while you wait*, which is why it scales servers so well.

                    Under the hood the compiler rewrites your method into a **state machine**: each
                    `await` is a checkpoint where the method can suspend and later resume from exactly
                    that spot, with all your locals preserved.

                    ## Why interviewers probe this

                    Async is where "looks correct" and "is correct" diverge. A candidate who can write
                    `await SomethingAsync()` but can't explain *why blocking on it deadlocks*, or *why a
                    loop of awaits is slow*, hasn't internalized the model. These questions separate
                    "I've used async" from "I understand async."

                    ## Common traps

                    - **`.Result` / `.Wait()` deadlocks.** On a context that pins continuations to one
                      thread (classic ASP.NET, WinForms, WPF), blocking on a task blocks that thread —
                      but the task's continuation needs that same thread to finish. Neither side moves.
                      Rule: **async all the way down**; don't block on async code.
                    - **Awaiting inside a loop = sequential.** `foreach (var x in items) await F(x);`
                      runs one at a time. If the calls are independent, start them all first, then
                      `await Task.WhenAll(...)` so they overlap.
                    - **`async void`.** It can't be awaited and its exceptions crash the process. Use it
                      *only* for event handlers; everywhere else return `Task`/`Task<T>`.
                    - **Forgetting `CancellationToken`.** Cancellable work must accept a token and pass
                      it down, or it can't be stopped.

                    ## Worked example: sequential vs concurrent

                    ```csharp
                    // SLOW — total time ≈ sum of all delays (each await waits for the last).
                    var results = new List<int>();
                    foreach (var f in fetchers) results.Add(await f());

                    // FAST — total time ≈ the single slowest call; they overlap.
                    var tasks = fetchers.Select(f => f()).ToArray(); // start them all NOW
                    var results = await Task.WhenAll(tasks);          // then await together
                    ```

                    The trick is that calling `f()` *starts* the task; the work is already in flight
                    before you await. `WhenAll` also returns results **in input order**, not completion
                    order — handy and easy to forget.

                    Key rules to repeat back in an interview:
                    - Return `Task`/`Task<T>` — never `async void` (except UI event handlers).
                    - Don't block on async (`.Result`/`.Wait()`) — it can deadlock or starve threads.
                    - Flow a `CancellationToken` through calls to make work cancellable.
                    """,
                Exercises =
                [
                    SumAsync,
                    ConcurrentFetch,
                    CancellableDelay,
                ],
            },
            CoordinationLesson,
            ConcurrencyControlLesson,
        ],
    };

    // -------------------------------------------------------------------------
    // Exercise 1 (Easy): prove you can write a basic async method.
    // -------------------------------------------------------------------------
    private static ExerciseSeed SumAsync => new()
    {
        Slug = "sum-async",
        Title = "Your First async Method",
        Difficulty = "Easy",
        Kind = "Function",
        TimeoutSeconds = 5,
        Prompt =
            """
            Implement `SumAsync(int a, int b)` so it returns `a + b`.
            It must be an **async** method that awaits at least once (we simulate
            async I/O with `await Task.Yield();`). This warms up the grader pipeline.
            """,
        StarterCode =
            """
            using System.Threading.Tasks;

            public static class Solution
            {
                // TODO: make this async, await once, and return a + b.
                public static Task<int> SumAsync(int a, int b)
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
                    r.Check("SumAsync(2, 3) == 5", () =>
                        Assert.Equal(5, Solution.SumAsync(2, 3).GetAwaiter().GetResult()));
                    r.Check("SumAsync(-1, 1) == 0", () =>
                        Assert.Equal(0, Solution.SumAsync(-1, 1).GetAwaiter().GetResult()));
                    r.Check("SumAsync(0, 0) == 0", () =>
                        Assert.Equal(0, Solution.SumAsync(0, 0).GetAwaiter().GetResult()));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Threading.Tasks;

            public static class Solution
            {
                // 'async' makes this a state machine; 'await Task.Yield()' forces an
                // asynchronous suspension point (a stand-in for real I/O like a DB call).
                public static async Task<int> SumAsync(int a, int b)
                {
                    await Task.Yield(); // hands control back, resumes on the thread pool
                    return a + b;
                }
            }
            """,
        Hints =
        [
            "Add the `async` keyword to the method signature.",
            "An async method that returns `Task<int>` should `return` an `int` directly (the compiler wraps it).",
            "Use `await Task.Yield();` to create an awaited suspension point.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "SumAsync(2, 3) == 5", IsHidden = false },
            new TestCaseSeed { Name = "SumAsync(-1, 1) == 0", IsHidden = false },
            new TestCaseSeed { Name = "SumAsync(0, 0) == 0", IsHidden = true },
        ],
    };

    // -------------------------------------------------------------------------
    // Exercise 2 (Medium): the sequential-vs-concurrent lesson, as code.
    // -------------------------------------------------------------------------
    private static ExerciseSeed ConcurrentFetch => new()
    {
        Slug = "concurrent-fetch",
        Title = "Run Independent Work Concurrently",
        Difficulty = "Medium",
        Kind = "Function",
        TimeoutSeconds = 5,
        Prompt =
            """
            Implement `FetchAllAsync(Func<Task<int>>[] fetchers)` that invokes every
            fetcher and returns their results **as an array, in the same order**.
            Do it **concurrently** (start them all, then await together) — not one at
            a time. Hint: `Task.WhenAll` preserves order.
            """,
        StarterCode =
            """
            using System;
            using System.Linq;
            using System.Threading.Tasks;

            public static class Solution
            {
                // TODO: start all fetchers, then await them together, preserving order.
                public static Task<int[]> FetchAllAsync(Func<Task<int>>[] fetchers)
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

                    r.Check("returns results in order", () =>
                    {
                        Func<Task<int>>[] fs =
                        {
                            async () => { await Task.Delay(20); return 1; },
                            async () => { await Task.Delay(10); return 2; },
                            async () => { await Task.Delay(5);  return 3; },
                        };
                        var result = Solution.FetchAllAsync(fs).GetAwaiter().GetResult();
                        Assert.Equal("1,2,3", string.Join(",", result));
                    });

                    r.Check("runs concurrently (faster than sequential)", () =>
                    {
                        Func<Task<int>>[] fs =
                        {
                            async () => { await Task.Delay(150); return 1; },
                            async () => { await Task.Delay(150); return 2; },
                            async () => { await Task.Delay(150); return 3; },
                        };
                        var sw = System.Diagnostics.Stopwatch.StartNew();
                        Solution.FetchAllAsync(fs).GetAwaiter().GetResult();
                        sw.Stop();
                        // Sequential would be ~450ms; concurrent ~150ms. Wide margin so
                        // the check stays robust under parallel test-host load.
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
                public static Task<int[]> FetchAllAsync(Func<Task<int>>[] fetchers)
                {
                    // Invoke each fetcher NOW to get hot (already-running) tasks, then
                    // hand them to WhenAll. Because we don't await between starts, they
                    // overlap. WhenAll returns results in the same order as the input.
                    var tasks = fetchers.Select(f => f()).ToArray();
                    return Task.WhenAll(tasks);
                }
            }
            """,
        Hints =
        [
            "Call each fetcher to START it before awaiting anything.",
            "Project the array of fetchers into an array of running tasks: `fetchers.Select(f => f())`.",
            "`Task.WhenAll(tasks)` returns a `Task<int[]>` with results in input order — return it directly.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "returns results in order", IsHidden = false },
            new TestCaseSeed { Name = "runs concurrently (faster than sequential)", IsHidden = false },
        ],
    };

    // -------------------------------------------------------------------------
    // Exercise 3 (Medium): honor a CancellationToken and throw when cancelled.
    // -------------------------------------------------------------------------
    private static ExerciseSeed CancellableDelay => new()
    {
        Slug = "cancellable-delay",
        Title = "Honor a CancellationToken",
        Difficulty = "Medium",
        Kind = "Function",
        TimeoutSeconds = 5,
        Prompt =
            """
            Implement `CountUpAsync(int n, CancellationToken token)` that returns the sum
            `1 + 2 + ... + n`, awaiting a tiny `Task.Delay` between steps to simulate work.

            It must be **cancellable**: if the token is already cancelled (or gets cancelled
            mid-flight), it must throw `OperationCanceledException` instead of finishing.
            Flow the token through your `Task.Delay` and check it before doing work.

            Edge cases: a pre-cancelled token must throw *before* returning any result;
            `n == 0` with a live token returns `0`.
            """,
        StarterCode =
            """
            using System.Threading;
            using System.Threading.Tasks;

            public static class Solution
            {
                // TODO: sum 1..n, awaiting Task.Delay(1, token) each step; throw if cancelled.
                public static Task<int> CountUpAsync(int n, CancellationToken token)
                {
                    throw new System.NotImplementedException();
                }
            }
            """,
        HarnessCode =
            """
            using System;
            using System.Threading;
            using System.Threading.Tasks;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();

                    r.Check("sums 1..5 with a live token", () =>
                    {
                        var result = Solution.CountUpAsync(5, CancellationToken.None).GetAwaiter().GetResult();
                        Assert.Equal(15, result);
                    });

                    r.Check("pre-cancelled token throws", () =>
                    {
                        using var cts = new CancellationTokenSource();
                        cts.Cancel();
                        bool threw = false;
                        try { Solution.CountUpAsync(5, cts.Token).GetAwaiter().GetResult(); }
                        catch (OperationCanceledException) { threw = true; }
                        Assert.True(threw, "expected OperationCanceledException on a pre-cancelled token");
                    });

                    r.Check("n == 0 returns 0", () =>
                    {
                        var result = Solution.CountUpAsync(0, CancellationToken.None).GetAwaiter().GetResult();
                        Assert.Equal(0, result);
                    });

                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Threading;
            using System.Threading.Tasks;

            public static class Solution
            {
                public static async Task<int> CountUpAsync(int n, CancellationToken token)
                {
                    // Throw immediately if we were handed an already-cancelled token.
                    token.ThrowIfCancellationRequested();

                    int sum = 0;
                    for (int i = 1; i <= n; i++)
                    {
                        // Passing the token makes the delay itself cancellable: if the
                        // token trips while we wait, Task.Delay throws OperationCanceledException.
                        await Task.Delay(1, token);
                        sum += i;
                    }
                    return sum;
                }
            }
            """,
        Hints =
        [
            "Call `token.ThrowIfCancellationRequested()` up front so a pre-cancelled token fails fast.",
            "Pass the token into `Task.Delay(1, token)` — it will throw if cancelled while waiting.",
            "With n == 0 the loop never runs, so you just return the initial sum of 0.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "sums 1..5 with a live token", IsHidden = false },
            new TestCaseSeed { Name = "pre-cancelled token throws", IsHidden = false },
            new TestCaseSeed { Name = "n == 0 returns 0", IsHidden = true },
        ],
    };
}
