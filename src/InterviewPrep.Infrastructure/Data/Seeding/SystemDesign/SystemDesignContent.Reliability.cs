namespace InterviewPrep.Infrastructure.Data.Seeding.SystemDesign;

// Lesson 4 — reliability & scale primitives: a Bloom filter, exponential backoff,
// weighted round-robin, and event-sourcing fold. All deterministic and gradeable.
internal static partial class SystemDesignContent
{
    private static LessonSeed ReliabilityLesson => new()
    {
        Slug = "reliability-scale",
        Title = "Reliability & Scale",
        Order = 4,
        MarkdownContent =
            """
            ## Reliability & Scale

            - **Bloom filter** — a tiny probabilistic set: "definitely not present" or "maybe
              present" (no false negatives). Saves lookups for huge sets.
            - **Exponential backoff** — wait longer after each retry (100ms, 200ms, 400ms…),
              capped, to avoid hammering a struggling service.
            - **Weighted round-robin** — send more traffic to bigger servers.
            - **Event sourcing** — store the sequence of events; the current state is a fold
              over them (and you can replay/audit).
            """,
        Exercises = [BloomFilter, ExponentialBackoff, WeightedRoundRobin, EventSourcingFold],
    };

    private static ExerciseSeed BloomFilter => new()
    {
        Slug = "bloom-filter",
        Title = "Bloom Filter",
        Difficulty = "Hard",
        Kind = "Class",
        Prompt =
            """
            Implement a `BloomFilter(size, hashes)`: `Add(key)` sets `hashes` bits (from
            different hash seeds), and `MightContain(key)` returns true only if **all** those
            bits are set. It never gives a false negative (added items always return true),
            though rare false positives are possible.
            """,
        StarterCode =
            """
            public sealed class BloomFilter
            {
                public BloomFilter(int size, int hashes) => throw new System.NotImplementedException();
                public void Add(string key) => throw new System.NotImplementedException();
                public bool MightContain(string key) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("no false negatives (added -> true)", () =>
                    {
                        var b = new BloomFilter(128, 3);
                        b.Add("apple"); b.Add("banana");
                        Assert.True(b.MightContain("apple"));
                        Assert.True(b.MightContain("banana"));
                    });
                    r.Check("empty filter contains nothing", () =>
                    {
                        var b = new BloomFilter(128, 3);
                        Assert.False(b.MightContain("apple"));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public sealed class BloomFilter
            {
                private readonly bool[] _bits;
                private readonly int _hashes;

                public BloomFilter(int size, int hashes) { _bits = new bool[size]; _hashes = hashes; }

                public void Add(string key)
                {
                    foreach (var i in Indices(key)) _bits[i] = true;
                }

                public bool MightContain(string key)
                {
                    foreach (var i in Indices(key))
                        if (!_bits[i]) return false; // a clear bit => definitely absent
                    return true;
                }

                // `_hashes` positions from different seeds (FNV-1a variant).
                private IEnumerable<int> Indices(string key)
                {
                    for (int seed = 0; seed < _hashes; seed++)
                    {
                        uint hash = (uint)(2166136261 + seed * 40503);
                        foreach (var c in key) { hash ^= c; hash *= 16777619; }
                        yield return (int)(hash % (uint)_bits.Length);
                    }
                }
            }
            """,
        Hints =
        [
            "Back it with a bool[] of `size` bits.",
            "Derive `hashes` positions per key using different seeds.",
            "Add sets those bits; MightContain returns false if ANY is unset.",
        ],
        TestCases =
        [
            new() { Name = "no false negatives (added -> true)", IsHidden = false },
            new() { Name = "empty filter contains nothing", IsHidden = false },
        ],
    };

    private static ExerciseSeed ExponentialBackoff => new()
    {
        Slug = "exponential-backoff",
        Title = "Exponential Backoff",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Implement `Delay(attempt, baseMs, maxMs)` = `baseMs · 2^(attempt−1)`, capped at
            `maxMs` (attempt is 1-based). This is the standard retry wait so a struggling
            service gets breathing room instead of a thundering herd.
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: base * 2^(attempt-1), capped at maxMs.
                public static long Delay(int attempt, long baseMs, long maxMs)
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
                    r.Check("doubles each attempt", () =>
                    {
                        Assert.Equal(100L, Solution.Delay(1, 100, 10000));
                        Assert.Equal(200L, Solution.Delay(2, 100, 10000));
                        Assert.Equal(400L, Solution.Delay(3, 100, 10000));
                    });
                    r.Check("caps at maxMs", () =>
                        Assert.Equal(500L, Solution.Delay(10, 100, 500)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public static class Solution
            {
                public static long Delay(int attempt, long baseMs, long maxMs)
                {
                    // Shift left = multiply by 2^(attempt-1). Guard against overflow via the cap.
                    double raw = baseMs * Math.Pow(2, attempt - 1);
                    return (long)Math.Min(maxMs, raw);
                }
            }
            """,
        Hints =
        [
            "The multiplier is 2^(attempt-1): 1, 2, 4, 8…",
            "Multiply baseMs by that.",
            "Cap the result at maxMs with Math.Min.",
        ],
        TestCases =
        [
            new() { Name = "doubles each attempt", IsHidden = false },
            new() { Name = "caps at maxMs", IsHidden = false },
        ],
    };

    private static ExerciseSeed WeightedRoundRobin => new()
    {
        Slug = "weighted-round-robin",
        Title = "Weighted Round-Robin",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement `WeightedRoundRobin(nodes, weights)` where `Next()` returns nodes in
            proportion to their weight, cycling forever. Simplest approach: expand each node
            by its weight into a sequence and cycle it — e.g. A(2), B(1) → A, A, B, A, A, B…
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public sealed class WeightedRoundRobin
            {
                public WeightedRoundRobin(string[] nodes, int[] weights) => throw new System.NotImplementedException();
                public string Next() => throw new System.NotImplementedException();
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
                    r.Check("distributes by weight and cycles", () =>
                    {
                        var lb = new WeightedRoundRobin(new[]{"A","B"}, new[]{2,1});
                        var seq = Enumerable.Range(0, 6).Select(_ => lb.Next());
                        Assert.Equal("A,A,B,A,A,B", string.Join(",", seq));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public sealed class WeightedRoundRobin
            {
                private readonly List<string> _sequence = new();
                private int _index;

                public WeightedRoundRobin(string[] nodes, int[] weights)
                {
                    // Expand each node by its weight: A,A,B for weights 2,1.
                    for (int i = 0; i < nodes.Length; i++)
                        for (int w = 0; w < weights[i]; w++)
                            _sequence.Add(nodes[i]);
                }

                public string Next()
                {
                    var node = _sequence[_index];
                    _index = (_index + 1) % _sequence.Count; // wrap forever
                    return node;
                }
            }
            """,
        Hints =
        [
            "In the constructor, add each node to a list `weight` times.",
            "Next returns the current item and advances the index.",
            "Wrap the index with modulo so it cycles forever.",
        ],
        TestCases = [new() { Name = "distributes by weight and cycles", IsHidden = false }],
    };

    private static ExerciseSeed EventSourcingFold => new()
    {
        Slug = "event-sourcing-fold",
        Title = "Event Sourcing (Fold)",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            In event sourcing you store events and compute state by folding over them. Given
            events like `"deposit:100"` and `"withdraw:30"`, return the final account balance
            (starting at 0). Ignore any event that would overdraw (balance can't go negative).
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: fold events into a balance; skip withdrawals that would overdraw.
                public static int Balance(string[] events)
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
                    r.Check("folds deposits and withdrawals", () =>
                        Assert.Equal(80, Solution.Balance(new[]{"deposit:100","withdraw:30","deposit:10"})));
                    r.Check("skips an overdraw", () =>
                        Assert.Equal(20, Solution.Balance(new[]{"deposit:20","withdraw:50"})));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public static class Solution
            {
                public static int Balance(string[] events)
                {
                    int balance = 0;
                    foreach (var e in events)
                    {
                        var parts = e.Split(':');
                        int amount = int.Parse(parts[1]);
                        if (parts[0] == "deposit") balance += amount;
                        else if (parts[0] == "withdraw" && amount <= balance) balance -= amount; // skip overdraw
                    }
                    return balance;
                }
            }
            """,
        Hints =
        [
            "Start balance at 0 and apply each event in order.",
            "Split each event on ':' to get the type and amount.",
            "Only subtract a withdrawal if it wouldn't go negative.",
        ],
        TestCases =
        [
            new() { Name = "folds deposits and withdrawals", IsHidden = false },
            new() { Name = "skips an overdraw", IsHidden = false },
        ],
    };
}
