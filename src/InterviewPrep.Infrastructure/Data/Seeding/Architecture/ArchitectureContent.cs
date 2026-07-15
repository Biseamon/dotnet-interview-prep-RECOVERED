namespace InterviewPrep.Infrastructure.Data.Seeding.Architecture;

// "Architecture & Distributed Systems" — the conceptual senior-interview material
// (architectural styles, CAP, messaging, idempotency) delivered as rich lessons,
// each paired with a small gradeable exercise that makes the concept concrete.
internal static partial class ArchitectureContent
{
    public static TopicSeed Topic => new()
    {
        Slug = "architecture",
        Name = "Architecture & Distributed Systems",
        Description = "Architectural styles, caching, load balancing, CAP, messaging, and idempotency — the system-design round.",
        Order = 12,
        Lessons =
        [
            StylesLesson,
            DistributedLesson,
        ],
    };

    // =========================================================================
    private static LessonSeed StylesLesson => new()
    {
        Slug = "architectural-styles",
        Title = "Architectural Styles",
        Order = 1,
        MarkdownContent =
            """
            ## Architectural Styles

            **Layered / N-tier** — presentation → application → domain → infrastructure.
            Simple and familiar, but layers can leak and the domain often ends up depending on
            infrastructure.

            **Clean / Onion / Hexagonal (Ports & Adapters)** — the *dependency rule* points
            **inward**: the domain has zero dependencies; infrastructure (EF, HTTP, queues)
            implements interfaces the inner layers define. This is exactly how *this app* is
            built (`Domain` ← `Application` ← `Infrastructure`/`Api`). Benefit: the core is
            testable and swappable; cost: more indirection.

            **Vertical Slice** — organize by feature, not by layer. Each slice owns its
            request→response path. Less shared abstraction, less ceremony, great for CQRS.

            **Modular Monolith vs Microservices** — start with a *modular monolith* (clear
            module boundaries, one deployable). Extract microservices only when you need
            **independent scaling/deployment** or team autonomy — you pay for it with network
            calls, distributed transactions, and operational complexity. "Don't distribute
            until you must" is the senior answer.

            The exercises below make two cross-cutting concerns concrete: **cache-aside** and
            **load balancing**.
            """,
        Exercises =
        [
            CacheAside,
            RoundRobin,
        ],
    };

    private static ExerciseSeed CacheAside => new()
    {
        Slug = "cache-aside",
        Title = "Cache-Aside",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement the cache-aside pattern: `GetOrLoad(key, loader)` returns the cached
            value if present, otherwise calls `loader(key)`, **stores** the result, and
            returns it. The loader (an expensive DB/HTTP call) must run **at most once per key**.
            """,
        StarterCode =
            """
            using System;

            public sealed class CacheAside
            {
                // TODO: return cached value, or load once, cache, and return.
                public int GetOrLoad(string key, Func<string, int> loader)
                    => throw new NotImplementedException();
            }
            """,
        HarnessCode =
            """
            using System;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("loads once, then serves from cache", () =>
                    {
                        var cache = new CacheAside();
                        int loads = 0;
                        Func<string, int> loader = k => { loads++; return k.Length; };

                        Assert.Equal(4, cache.GetOrLoad("test", loader));
                        Assert.Equal(4, cache.GetOrLoad("test", loader)); // cached
                        Assert.Equal(1, loads);                            // loader ran once
                    });
                    r.Check("different keys load independently", () =>
                    {
                        var cache = new CacheAside();
                        int loads = 0;
                        Func<string, int> loader = k => { loads++; return k.Length; };
                        cache.GetOrLoad("a", loader);
                        cache.GetOrLoad("bb", loader);
                        Assert.Equal(2, loads);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Collections.Generic;

            public sealed class CacheAside
            {
                private readonly Dictionary<string, int> _cache = new();

                public int GetOrLoad(string key, Func<string, int> loader)
                {
                    if (_cache.TryGetValue(key, out var value))
                        return value;                 // cache hit
                    value = loader(key);              // miss -> load
                    _cache[key] = value;              // populate for next time
                    return value;
                }
            }
            """,
        Hints =
        [
            "Check the cache first and return on a hit.",
            "On a miss, call the loader, store the result, then return it.",
            "The dictionary ensures each key's loader runs only once.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "loads once, then serves from cache", IsHidden = false },
            new TestCaseSeed { Name = "different keys load independently", IsHidden = false },
        ],
    };

    private static ExerciseSeed RoundRobin => new()
    {
        Slug = "round-robin-balancer",
        Title = "Round-Robin Load Balancer",
        Difficulty = "Easy",
        Kind = "Class",
        Prompt =
            """
            Implement a `RoundRobin(servers)` load balancer whose `Next()` returns servers in
            rotation, wrapping back to the first after the last. This is the simplest
            distribution strategy for stateless backends.
            """,
        StarterCode =
            """
            public sealed class RoundRobin
            {
                public RoundRobin(string[] servers) => throw new System.NotImplementedException();
                public string Next() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("cycles through servers", () =>
                    {
                        var lb = new RoundRobin(new[]{"a","b","c"});
                        Assert.Equal("a", lb.Next());
                        Assert.Equal("b", lb.Next());
                        Assert.Equal("c", lb.Next());
                        Assert.Equal("a", lb.Next()); // wraps around
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public sealed class RoundRobin
            {
                private readonly string[] _servers;
                private int _index;

                public RoundRobin(string[] servers) => _servers = servers;

                public string Next()
                {
                    var server = _servers[_index];
                    _index = (_index + 1) % _servers.Length; // wrap with modulo
                    return server;
                }
            }
            """,
        Hints =
        [
            "Keep a rotating index.",
            "Return the current server, then advance the index.",
            "Use modulo by the server count to wrap around.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "cycles through servers", IsHidden = false },
        ],
    };

    // =========================================================================
    private static LessonSeed DistributedLesson => new()
    {
        Slug = "distributed-systems",
        Title = "Distributed Systems",
        Order = 2,
        MarkdownContent =
            """
            ## Distributed Systems

            **CAP theorem** — under a network **P**artition you must choose **C**onsistency
            (reject/stall to stay correct) or **A**vailability (answer, possibly stale). Most
            systems are "CP" or "AP" for a given operation; "CA" only exists without partitions.

            **Consistency models** — *strong* (reads see the latest write) vs *eventual*
            (replicas converge over time). Eventual consistency buys availability and scale;
            design the UX to tolerate staleness.

            **Messaging** — queues (point-to-point, work distribution) vs pub/sub (fan-out).
            Async messaging decouples services and absorbs load spikes, at the cost of
            eventual consistency and harder debugging.

            **Idempotency** — networks retry, so consumers must handle **duplicate** messages
            safely. Dedupe by a message/idempotency key so re-delivery is a no-op. Pair with the
            **outbox pattern** (write the event in the same transaction as the state change) to
            avoid "wrote to DB but lost the event" bugs.

            **Sharding** — partition data across nodes by a key (hash or range) to scale writes.
            The exercises make **idempotent consumption** and **shard routing** concrete.
            """,
        Exercises =
        [
            IdempotentConsumer,
            ShardRouter,
        ],
    };

    private static ExerciseSeed IdempotentConsumer => new()
    {
        Slug = "idempotent-consumer",
        Title = "Idempotent Consumer",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Messages can be delivered more than once. Implement an `IdempotentConsumer` whose
            `Process(messageId)` applies the effect only the **first** time it sees an id:
            return `true` when newly processed, `false` for a duplicate. `ProcessedCount`
            counts distinct messages handled.
            """,
        StarterCode =
            """
            public sealed class IdempotentConsumer
            {
                // TODO: dedupe by message id; count distinct processed messages.
                public bool Process(string messageId) => throw new System.NotImplementedException();
                public int ProcessedCount => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("duplicates are no-ops", () =>
                    {
                        var c = new IdempotentConsumer();
                        Assert.True(c.Process("m1"));   // new
                        Assert.False(c.Process("m1"));  // duplicate
                        Assert.True(c.Process("m2"));   // new
                        Assert.Equal(2, c.ProcessedCount);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public sealed class IdempotentConsumer
            {
                private readonly HashSet<string> _seen = new();

                // HashSet.Add returns false if the id was already present -> duplicate.
                public bool Process(string messageId) => _seen.Add(messageId);

                public int ProcessedCount => _seen.Count;
            }
            """,
        Hints =
        [
            "Track processed message ids in a HashSet.",
            "HashSet.Add returns false when the id already exists — that's your duplicate check.",
            "ProcessedCount is just the set's size.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "duplicates are no-ops", IsHidden = false },
        ],
    };

    private static ExerciseSeed ShardRouter => new()
    {
        Slug = "shard-router",
        Title = "Shard Router",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement a `ShardRouter(shardCount)` whose `GetShard(key)` maps a key to a shard
            in `[0, shardCount)` **deterministically** (the same key always routes to the same
            shard). Hash the key, then mod by the shard count. Use `Math.Abs` so negative
            hashes don't produce a negative shard.
            """,
        StarterCode =
            """
            public sealed class ShardRouter
            {
                public ShardRouter(int shardCount) => throw new System.NotImplementedException();

                // TODO: deterministic hash of key, mod shardCount, non-negative.
                public int GetShard(string key) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            using System.Collections.Generic;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("same key -> same shard (deterministic)", () =>
                    {
                        var router = new ShardRouter(4);
                        Assert.Equal(router.GetShard("user-42"), router.GetShard("user-42"));
                    });
                    r.Check("shards stay in range [0, count)", () =>
                    {
                        var router = new ShardRouter(4);
                        foreach (var k in new[]{"a","bb","ccc","dddd","user-1","user-2"})
                        {
                            var s = router.GetShard(k);
                            Assert.True(s >= 0 && s < 4, $"shard {s} out of range");
                        }
                    });
                    r.Check("distributes across more than one shard", () =>
                    {
                        var router = new ShardRouter(4);
                        var shards = new HashSet<int>();
                        for (int i = 0; i < 20; i++) shards.Add(router.GetShard("key" + i));
                        Assert.True(shards.Count > 1);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public sealed class ShardRouter
            {
                private readonly int _shardCount;
                public ShardRouter(int shardCount) => _shardCount = shardCount;

                public int GetShard(string key)
                {
                    // A stable hash (don't rely on string.GetHashCode, which is randomized
                    // per process). FNV-1a keeps routing consistent across runs.
                    uint hash = 2166136261;
                    foreach (var c in key)
                    {
                        hash ^= c;
                        hash *= 16777619;
                    }
                    return (int)(hash % (uint)_shardCount);
                }
            }
            """,
        Hints =
        [
            "Combine the characters into a hash (a simple rolling hash like FNV-1a works).",
            "Mod the hash by shardCount to pick a shard.",
            "Avoid string.GetHashCode — it's randomized per process, so routing wouldn't be stable across restarts.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "same key -> same shard (deterministic)", IsHidden = false },
            new TestCaseSeed { Name = "shards stay in range [0, count)", IsHidden = false },
            new TestCaseSeed { Name = "distributes across more than one shard", IsHidden = true },
        ],
    };
}
