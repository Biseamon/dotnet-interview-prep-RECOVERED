// =============================================================================
// LESSON 01 — async / await  (Week 1, Day 1-2)
// =============================================================================
// Run this file:   cd lessons/Lesson01_Async && dotnet run
// Read top-to-bottom. Each demo is a self-contained method called from Main.
// Every line that matters has a "what + why" comment — that's the interview gold.
// =============================================================================

using System.Diagnostics; // Stopwatch, to PROVE that concurrent work is faster.

// -----------------------------------------------------------------------------
// Top-level statements: this IS the Main method. The compiler wraps it for us.
// Because we 'await' below, the compiler makes this entry point async for us.
// -----------------------------------------------------------------------------

Console.WriteLine("=== 1. Sequential vs concurrent await ===");
await Demo_SequentialVsConcurrent();

Console.WriteLine("\n=== 2. CancellationToken ===");
await Demo_Cancellation();

Console.WriteLine("\n=== 3. Exceptions across awaits ===");
await Demo_Exceptions();

Console.WriteLine("\n=== 4. Task vs ValueTask (with caching) ===");
await Demo_ValueTask();

Console.WriteLine("\nDone. Now re-read the comments and say each 'why' out loud.");


// =============================================================================
// DEMO 1 — The single most important async idea for interviews:
// awaiting in a loop is SEQUENTIAL. Starting tasks then awaiting all is CONCURRENT.
// =============================================================================
static async Task Demo_SequentialVsConcurrent()
{
    var sw = Stopwatch.StartNew(); // start timing

    // --- SEQUENTIAL: each await pauses until that call finishes before the next starts.
    // Total time ≈ sum of all delays (3 x 300ms ≈ 900ms). This is the beginner mistake.
    await FakeApiCall("A", 300); // waits 300ms...
    await FakeApiCall("B", 300); // ...THEN waits another 300ms...
    await FakeApiCall("C", 300); // ...THEN another. They never overlap.
    Console.WriteLine($"Sequential took ~{sw.ElapsedMilliseconds}ms");

    sw.Restart();

    // --- CONCURRENT: start all three FIRST (no await yet), so they run overlapping.
    // Calling the method returns a "hot" Task that is ALREADY running.
    Task<string> a = FakeApiCall("A", 300); // kicked off, running in background
    Task<string> b = FakeApiCall("B", 300); // kicked off too — overlaps with A
    Task<string> c = FakeApiCall("C", 300); // all three in flight now

    // Task.WhenAll awaits them together. Total time ≈ the SLOWEST one (~300ms), not the sum.
    string[] results = await Task.WhenAll(a, b, c); // returns results in the SAME order as args
    Console.WriteLine($"Concurrent took ~{sw.ElapsedMilliseconds}ms, results: {string.Join(",", results)}");

    // INTERVIEW ANGLE:
    // Q: "How would you speed up code that calls three independent APIs in a loop?"
    // A: Don't await inside the loop. Start the tasks, collect them, await Task.WhenAll.
    //    Caveat: only for INDEPENDENT work. If B needs A's result, they must be sequential.
    //    Caveat: WhenAll on 10,000 items can hammer a service — throttle with SemaphoreSlim
    //            or Parallel.ForEachAsync(maxDegreeOfParallelism).
}


// =============================================================================
// DEMO 2 — CancellationToken: how you make async work STOPPABLE (timeouts, user aborts).
// =============================================================================
static async Task Demo_Cancellation()
{
    // A token source is the "remote control"; it hands out tokens and can cancel them.
    // This one auto-cancels after 150ms — a common way to implement a timeout.
    using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(150));

    try
    {
        // Pass the token DOWN into the async call. A well-written async method checks it.
        await FakeApiCall("Slow", 500, cts.Token); // 500ms work, but token fires at 150ms
        Console.WriteLine("Completed (won't reach here — it gets cancelled)");
    }
    catch (OperationCanceledException) // Task.Delay/most APIs throw THIS when cancelled
    {
        // Catching cancellation is normal control flow, not an error you log as a failure.
        Console.WriteLine("Cancelled as expected (timeout hit before work finished)");
    }

    // INTERVIEW ANGLE:
    // Q: "How do you implement a timeout / let a user cancel a request?"
    // A: CancellationTokenSource(timeout) OR CancelAfter. Flow the token through every
    //    async call. In ASP.NET Core, the framework GIVES you HttpContext.RequestAborted —
    //    accept a CancellationToken parameter in your action and it's wired up automatically.
}


// =============================================================================
// DEMO 3 — Exceptions: an exception inside an async method is captured on the Task
// and RE-THROWN at the await. That's why try/catch around 'await' works normally.
// =============================================================================
static async Task Demo_Exceptions()
{
    try
    {
        // The exception is thrown INSIDE the task, stored on it, and surfaced when we await.
        await ThrowsAfterDelay();
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"Caught: {ex.Message}");
    }

    // THE 'async void' TRAP — the #1 async footgun:
    // An 'async void' method's exception CANNOT be awaited or caught by the caller.
    // It bubbles straight to the runtime and can CRASH THE PROCESS.
    // Rule: async methods return Task/Task<T>. The ONLY exception is event handlers.
    //   BAD:  async void DoWork()      → exceptions escape, caller can't await it
    //   GOOD: async Task DoWork()      → awaitable, catchable

    // INTERVIEW ANGLE:
    // Q: "Why is async void dangerous?"
    // A: You can't await it (caller can't know when it's done or if it failed), and its
    //    exceptions escape try/catch and can crash the app. Use async Task everywhere
    //    except UI event handlers.
}


// =============================================================================
// DEMO 4 — Task vs ValueTask: an allocation optimization for HOT, often-synchronous paths.
// =============================================================================
static async Task Demo_ValueTask()
{
    var cache = new TinyCache();

    // First call: value not cached → real async work happens → allocates a Task internally.
    Console.WriteLine($"First  get: {await cache.GetAsync("user:1")}");

    // Second call: value IS cached → returns synchronously with NO Task allocation.
    // That's the ValueTask win: on the common (cached) path you avoid heap allocation.
    Console.WriteLine($"Second get: {await cache.GetAsync("user:1")}");

    // INTERVIEW ANGLE:
    // Q: "Task vs ValueTask — when would you use ValueTask?"
    // A: Use Task by default. Reach for ValueTask ONLY on very hot paths that USUALLY
    //    complete synchronously (e.g. a cache hit), to avoid a Task allocation per call.
    //    Rules: never await a ValueTask twice, don't store it, don't WhenAll it directly
    //    (convert with .AsTask()). Premature use adds complexity for no real gain.
}


// =============================================================================
// HELPERS  (these simulate I/O — a DB call, an HTTP call, etc.)
// =============================================================================

// Simulates an async I/O call. Task.Delay is a stand-in for "waiting on the network".
// KEY POINT: Task.Delay does NOT block a thread — it schedules a continuation and frees
// the thread to do other work. That non-blocking wait is the whole point of async.
static async Task<string> FakeApiCall(string name, int ms, CancellationToken ct = default)
{
    await Task.Delay(ms, ct); // non-blocking wait; throws OperationCanceledException if ct fires
    return $"{name}:ok";
}

// Demonstrates that exceptions travel through the Task and re-throw at the await point.
static async Task ThrowsAfterDelay()
{
    await Task.Delay(50);
    throw new InvalidOperationException("boom (thrown inside async, surfaced at await)");
}

// A tiny cache to show ValueTask's synchronous fast-path.
sealed class TinyCache
{
    private readonly Dictionary<string, string> _store = new();

    // Return type is ValueTask<string>: cheap when we can answer WITHOUT awaiting.
    public async ValueTask<string> GetAsync(string key)
    {
        if (_store.TryGetValue(key, out var cached))
            return cached; // SYNCHRONOUS return — no Task allocated. This is the ValueTask payoff.

        // Cache miss: do the "expensive" async work (simulated DB/HTTP load).
        await Task.Delay(100);
        var value = $"loaded-{key}";
        _store[key] = value; // populate cache so the next call hits the fast path
        return value;
    }
}
