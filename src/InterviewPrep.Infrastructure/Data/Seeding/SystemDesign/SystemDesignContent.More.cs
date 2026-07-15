namespace InterviewPrep.Infrastructure.Data.Seeding.SystemDesign;

// Lesson 3 — more real system-design components: sliding-window rate limiting, a ring
// buffer, consistent hashing, and a leaderboard. All deterministic and gradeable.
internal static partial class SystemDesignContent
{
    private static LessonSeed ScalingPatternsLesson => new()
    {
        Slug = "scaling-patterns-2",
        Title = "More Scaling Components",
        Order = 3,
        MarkdownContent =
            """
            ## More Scaling Components

            - **Sliding-window rate limiter** — count requests in the last N ticks (smoother
              than fixed windows).
            - **Ring buffer** — a fixed-size circular buffer that overwrites the oldest entry
              (logs, metrics, streaming).
            - **Consistent hashing** — map keys to nodes so adding/removing a node moves few
              keys (sharding, caches).
            - **Leaderboard** — keep a running top-N by score.
            """,
        Exercises =
        [
            SlidingWindow,
            RingBuffer,
            ConsistentHashing,
            Leaderboard,
        ],
    };

    private static ExerciseSeed SlidingWindow => new()
    {
        Slug = "sliding-window-rate-limiter",
        Title = "Sliding-Window Rate Limiter",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement `SlidingWindow(limit, windowTicks)`. `Allow(nowTick)` permits a request
            only if fewer than `limit` requests occurred in the last `windowTicks` (i.e. with
            time > now − window). Time is an explicit tick so grading is deterministic.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public sealed class SlidingWindow
            {
                public SlidingWindow(int limit, long windowTicks) => throw new System.NotImplementedException();

                // TODO: drop timestamps outside the window; allow if under the limit.
                public bool Allow(long nowTick) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("limits within the window, frees up after", () =>
                    {
                        var w = new SlidingWindow(2, 10);
                        Assert.True(w.Allow(0));   // 1st
                        Assert.True(w.Allow(1));   // 2nd
                        Assert.False(w.Allow(2));  // over limit within window
                        Assert.True(w.Allow(11));  // tick 0 fell out of the window
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public sealed class SlidingWindow
            {
                private readonly int _limit;
                private readonly long _window;
                private readonly Queue<long> _times = new();

                public SlidingWindow(int limit, long windowTicks)
                {
                    _limit = limit;
                    _window = windowTicks;
                }

                public bool Allow(long nowTick)
                {
                    // Evict timestamps older than the window.
                    while (_times.Count > 0 && _times.Peek() <= nowTick - _window)
                        _times.Dequeue();

                    if (_times.Count >= _limit) return false;
                    _times.Enqueue(nowTick);
                    return true;
                }
            }
            """,
        Hints =
        [
            "Keep a queue of the timestamps of allowed requests.",
            "On each call, dequeue timestamps that are now outside the window.",
            "If the remaining count is under the limit, record and allow; else deny.",
        ],
        TestCases = [new() { Name = "limits within the window, frees up after", IsHidden = false }],
    };

    private static ExerciseSeed RingBuffer => new()
    {
        Slug = "ring-buffer",
        Title = "Ring Buffer",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement a fixed-capacity `RingBuffer`: `Add(value)` appends, and once full it
            **overwrites the oldest** entry. `ToArray()` returns the contents oldest→newest.
            Used for logs, metrics, and streaming windows.
            """,
        StarterCode =
            """
            public sealed class RingBuffer
            {
                public RingBuffer(int capacity) => throw new System.NotImplementedException();
                public void Add(int value) => throw new System.NotImplementedException();
                public int[] ToArray() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("overwrites oldest when full", () =>
                    {
                        var b = new RingBuffer(3);
                        b.Add(1); b.Add(2); b.Add(3);
                        Assert.Equal("1,2,3", string.Join(",", b.ToArray()));
                        b.Add(4);
                        Assert.Equal("2,3,4", string.Join(",", b.ToArray()));
                        b.Add(5);
                        Assert.Equal("3,4,5", string.Join(",", b.ToArray()));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;
            using System.Linq;

            public sealed class RingBuffer
            {
                private readonly int _capacity;
                private readonly LinkedList<int> _items = new();

                public RingBuffer(int capacity) => _capacity = capacity;

                public void Add(int value)
                {
                    _items.AddLast(value);
                    if (_items.Count > _capacity) _items.RemoveFirst(); // drop oldest
                }

                public int[] ToArray() => _items.ToArray();
            }
            """,
        Hints =
        [
            "Track items in insertion order (a linked list or an array + head index).",
            "After adding, if the size exceeds capacity, remove the oldest.",
            "ToArray returns them oldest → newest.",
        ],
        TestCases = [new() { Name = "overwrites oldest when full", IsHidden = false }],
    };

    private static ExerciseSeed ConsistentHashing => new()
    {
        Slug = "consistent-hashing",
        Title = "Consistent Hashing Ring",
        Difficulty = "Hard",
        Kind = "Class",
        Prompt =
            """
            Implement a `HashRing`: `AddNode(name)` places a node on a hash ring, and
            `GetNode(key)` returns the first node clockwise from the key's hash (wrapping around).
            This maps keys to nodes so adding/removing a node moves few keys. Use a stable hash
            (not string.GetHashCode).
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            public sealed class HashRing
            {
                public void AddNode(string name) => throw new NotImplementedException();

                // TODO: first node whose hash >= key's hash, wrapping to the smallest if none.
                public string GetNode(string key) => throw new NotImplementedException();
            }
            """,
        HarnessCode =
            """
            using System.Collections.Generic;
            using System.Linq;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    var ring = new HashRing();
                    ring.AddNode("A"); ring.AddNode("B"); ring.AddNode("C");

                    r.Check("same key maps to the same node", () =>
                        Assert.Equal(ring.GetNode("user-42"), ring.GetNode("user-42")));
                    r.Check("every key maps to a real node", () =>
                    {
                        var valid = new[] { "A", "B", "C" };
                        for (int i = 0; i < 30; i++)
                            Assert.True(valid.Contains(ring.GetNode("k" + i)));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            public sealed class HashRing
            {
                private readonly SortedDictionary<uint, string> _ring = new();

                public void AddNode(string name) => _ring[Hash(name)] = name;

                public string GetNode(string key)
                {
                    var h = Hash(key);
                    // First node clockwise (hash >= h); wrap to the smallest if past the end.
                    foreach (var slot in _ring.Keys)
                        if (slot >= h) return _ring[slot];
                    return _ring.Values.First();
                }

                // FNV-1a: stable across runs (unlike string.GetHashCode).
                private static uint Hash(string s)
                {
                    uint hash = 2166136261;
                    foreach (var c in s) { hash ^= c; hash *= 16777619; }
                    return hash;
                }
            }
            """,
        Hints =
        [
            "Hash each node onto a ring (a SortedDictionary keeps them ordered).",
            "For a key, find the first node with hash >= the key's hash.",
            "If none is larger, wrap around to the smallest node. Use a stable hash like FNV-1a.",
        ],
        TestCases =
        [
            new() { Name = "same key maps to the same node", IsHidden = false },
            new() { Name = "every key maps to a real node", IsHidden = false },
        ],
    };

    private static ExerciseSeed Leaderboard => new()
    {
        Slug = "leaderboard-topn",
        Title = "Leaderboard (Top-N)",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement a `Leaderboard`: `AddScore(player, score)` sets (or updates) a player's
            score, and `Top(n)` returns the top `n` player names by score, highest first.
            """,
        StarterCode =
            """
            using System.Collections.Generic;
            using System.Linq;

            public sealed class Leaderboard
            {
                public void AddScore(string player, int score) => throw new System.NotImplementedException();
                public string[] Top(int n) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("returns top players by score", () =>
                    {
                        var lb = new Leaderboard();
                        lb.AddScore("ann", 50);
                        lb.AddScore("bob", 80);
                        lb.AddScore("cid", 30);
                        Assert.Equal("bob,ann", string.Join(",", lb.Top(2)));
                    });
                    r.Check("update changes the ranking", () =>
                    {
                        var lb = new Leaderboard();
                        lb.AddScore("ann", 50);
                        lb.AddScore("bob", 80);
                        lb.AddScore("ann", 100); // ann overtakes
                        Assert.Equal("ann,bob", string.Join(",", lb.Top(2)));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;
            using System.Linq;

            public sealed class Leaderboard
            {
                private readonly Dictionary<string, int> _scores = new();

                public void AddScore(string player, int score) => _scores[player] = score;

                public string[] Top(int n) =>
                    _scores.OrderByDescending(kv => kv.Value)
                           .Take(n)
                           .Select(kv => kv.Key)
                           .ToArray();
            }
            """,
        Hints =
        [
            "Store each player's current score in a dictionary (so updates overwrite).",
            "Top(n): order by score descending, take n, select the names.",
        ],
        TestCases =
        [
            new() { Name = "returns top players by score", IsHidden = false },
            new() { Name = "update changes the ranking", IsHidden = false },
        ],
    };
}
