namespace InterviewPrep.Infrastructure.Data.Seeding.SystemDesign;

// "System Design Building Blocks" — the data structures and primitives that show up
// in real systems and "design X" interview rounds: an LRU cache, a TTL store, a trie,
// a heap, union-find, and a token-bucket rate limiter. All deterministic and gradeable.
internal static partial class SystemDesignContent
{
    public static TopicSeed Topic => new()
    {
        Slug = "system-design",
        Name = "System Design Building Blocks",
        Description = "LRU cache, TTL store, trie, heap, union-find, rate limiter — the primitives behind scalable systems.",
        Order = 11,
        Lessons =
        [
            StructuresLesson,
            ScalingLesson,
            ScalingPatternsLesson,
            ReliabilityLesson,
        ],
    };

    // =========================================================================
    private static LessonSeed StructuresLesson => new()
    {
        Slug = "design-structures",
        Title = "Caching & Data Structures",
        Order = 1,
        MarkdownContent =
            """
            ## Caching & Data Structures

            The workhorses behind real systems:
            - **LRU cache** — O(1) get/put with a hash map + doubly-linked list; evict the
              least-recently-used entry when full.
            - **TTL store** — key/value with expiry (a manual clock keeps it testable).
            - **Trie** — prefix tree for autocomplete / dictionary lookups.
            - **Min-heap** — the basis of priority queues and "top-K" problems.
            """,
        Exercises =
        [
            LruCache,
            TtlCache,
            Trie,
            MinHeap,
        ],
    };

    private static ExerciseSeed LruCache => new()
    {
        Slug = "lru-cache",
        Title = "LRU Cache",
        Difficulty = "Hard",
        Kind = "Class",
        Prompt =
            """
            Design an `LruCache(capacity)` with O(1) `Get(key)` (returns value or `-1`) and
            `Put(key, value)`. When full, `Put` evicts the **least-recently-used** entry. Any
            access (get or put) makes a key most-recently-used. Hash map + doubly-linked list.
            """,
        StarterCode =
            """
            public sealed class LruCache
            {
                public LruCache(int capacity) => throw new System.NotImplementedException();
                public int Get(int key) => throw new System.NotImplementedException();
                public void Put(int key, int value) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("evicts least-recently-used", () =>
                    {
                        var c = new LruCache(2);
                        c.Put(1, 1);
                        c.Put(2, 2);
                        Assert.Equal(1, c.Get(1));   // 1 is now most-recent
                        c.Put(3, 3);                 // evicts key 2 (LRU)
                        Assert.Equal(-1, c.Get(2));
                        c.Put(4, 4);                 // evicts key 1
                        Assert.Equal(-1, c.Get(1));
                        Assert.Equal(3, c.Get(3));
                        Assert.Equal(4, c.Get(4));
                    });
                    r.Check("put updates existing key", () =>
                    {
                        var c = new LruCache(2);
                        c.Put(1, 1);
                        c.Put(1, 10);
                        Assert.Equal(10, c.Get(1));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public sealed class LruCache
            {
                private readonly int _capacity;
                private readonly Dictionary<int, LinkedListNode<(int Key, int Value)>> _map = new();
                // Front = most-recently-used, back = least-recently-used.
                private readonly LinkedList<(int Key, int Value)> _order = new();

                public LruCache(int capacity) => _capacity = capacity;

                public int Get(int key)
                {
                    if (!_map.TryGetValue(key, out var node)) return -1;
                    Touch(node);              // mark most-recently-used
                    return node.Value.Value;
                }

                public void Put(int key, int value)
                {
                    if (_map.TryGetValue(key, out var existing))
                    {
                        existing.Value = (key, value);
                        Touch(existing);
                        return;
                    }
                    if (_map.Count == _capacity)
                    {
                        var lru = _order.Last!;   // evict from the back
                        _order.RemoveLast();
                        _map.Remove(lru.Value.Key);
                    }
                    var node = new LinkedListNode<(int, int)>((key, value));
                    _order.AddFirst(node);
                    _map[key] = node;
                }

                private void Touch(LinkedListNode<(int Key, int Value)> node)
                {
                    _order.Remove(node);
                    _order.AddFirst(node);
                }
            }
            """,
        Hints =
        [
            "Combine a Dictionary (O(1) lookup) with a doubly-linked list (O(1) reorder).",
            "Keep most-recently-used at the front; evict from the back when full.",
            "Both Get and Put move the touched key to the front.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "evicts least-recently-used", IsHidden = false },
            new TestCaseSeed { Name = "put updates existing key", IsHidden = false },
        ],
    };

    private static ExerciseSeed TtlCache => new()
    {
        Slug = "ttl-cache",
        Title = "Key-Value Store with TTL",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement a `TtlCache` where `Set(key, value, expiresAtTick)` stores a value with
            an expiry time, and `Get(key, nowTick)` returns the value only if `nowTick <
            expiresAtTick`, else `null`. (Time is an explicit tick parameter so grading is
            deterministic — no real clock.)
            """,
        StarterCode =
            """
            public sealed class TtlCache
            {
                // TODO: store value + expiry; Get returns null when missing or expired.
                public void Set(string key, string value, long expiresAtTick)
                    => throw new System.NotImplementedException();
                public string? Get(string key, long nowTick)
                    => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("returns value before expiry", () =>
                    {
                        var c = new TtlCache();
                        c.Set("a", "1", 100);
                        Assert.Equal("1", c.Get("a", 50));
                    });
                    r.Check("expired -> null", () =>
                    {
                        var c = new TtlCache();
                        c.Set("a", "1", 100);
                        Assert.True(c.Get("a", 150) == null);
                    });
                    r.Check("missing key -> null", () =>
                        Assert.True(new TtlCache().Get("x", 0) == null));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public sealed class TtlCache
            {
                private readonly Dictionary<string, (string Value, long ExpiresAt)> _store = new();

                public void Set(string key, string value, long expiresAtTick)
                    => _store[key] = (value, expiresAtTick);

                public string? Get(string key, long nowTick)
                {
                    if (!_store.TryGetValue(key, out var entry)) return null;
                    if (nowTick >= entry.ExpiresAt) return null; // expired
                    return entry.Value;
                }
            }
            """,
        Hints =
        [
            "Store each value alongside its expiry tick.",
            "Get returns null if the key is missing.",
            "Also return null when nowTick has reached the expiry.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "returns value before expiry", IsHidden = false },
            new TestCaseSeed { Name = "expired -> null", IsHidden = false },
            new TestCaseSeed { Name = "missing key -> null", IsHidden = true },
        ],
    };

    private static ExerciseSeed Trie => new()
    {
        Slug = "trie",
        Title = "Trie (Prefix Tree)",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement a `Trie` with `Insert(word)`, `Search(word)` (exact match), and
            `StartsWith(prefix)`. Each node holds child links per character and a flag marking
            the end of a word. This powers autocomplete and dictionary lookups.
            """,
        StarterCode =
            """
            public sealed class Trie
            {
                // TODO: character-tree with an end-of-word marker.
                public void Insert(string word) => throw new System.NotImplementedException();
                public bool Search(string word) => throw new System.NotImplementedException();
                public bool StartsWith(string prefix) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    var t = new Trie();
                    t.Insert("apple");
                    r.Check("exact word found", () => Assert.True(t.Search("apple")));
                    r.Check("prefix is not a word", () => Assert.False(t.Search("app")));
                    r.Check("prefix exists", () => Assert.True(t.StartsWith("app")));
                    r.Check("unknown prefix", () => Assert.False(t.StartsWith("xyz")));
                    r.Check("after inserting the prefix as a word", () =>
                    {
                        t.Insert("app");
                        Assert.True(t.Search("app"));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public sealed class Trie
            {
                private sealed class Node
                {
                    public Dictionary<char, Node> Children { get; } = new();
                    public bool IsWord { get; set; }
                }

                private readonly Node _root = new();

                public void Insert(string word)
                {
                    var node = _root;
                    foreach (var c in word)
                    {
                        if (!node.Children.TryGetValue(c, out var next))
                            node.Children[c] = next = new Node();
                        node = next;
                    }
                    node.IsWord = true;
                }

                public bool Search(string word)
                {
                    var node = Walk(word);
                    return node != null && node.IsWord;
                }

                public bool StartsWith(string prefix) => Walk(prefix) != null;

                // Follow the characters; return the node reached, or null if a link is missing.
                private Node? Walk(string s)
                {
                    var node = _root;
                    foreach (var c in s)
                    {
                        if (!node.Children.TryGetValue(c, out var next)) return null;
                        node = next;
                    }
                    return node;
                }
            }
            """,
        Hints =
        [
            "Each node has a dictionary of child chars and an IsWord flag.",
            "Insert walks/creates nodes, marking the last as a word.",
            "Search needs IsWord at the end; StartsWith only needs the path to exist.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "exact word found", IsHidden = false },
            new TestCaseSeed { Name = "prefix is not a word", IsHidden = false },
            new TestCaseSeed { Name = "prefix exists", IsHidden = false },
            new TestCaseSeed { Name = "unknown prefix", IsHidden = true },
        ],
    };

    private static ExerciseSeed MinHeap => new()
    {
        Slug = "min-heap",
        Title = "Binary Min-Heap",
        Difficulty = "Hard",
        Kind = "Class",
        Prompt =
            """
            Implement a binary `MinHeap` backed by an array: `Push(value)`, `Pop()` (remove &
            return the minimum), `Peek()`, and `Count`. Push sifts up; Pop moves the last
            element to the root and sifts down. This is a priority queue from scratch.
            """,
        StarterCode =
            """
            public sealed class MinHeap
            {
                public int Count => throw new System.NotImplementedException();
                public void Push(int value) => throw new System.NotImplementedException();
                public int Peek() => throw new System.NotImplementedException();
                public int Pop() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("pops in ascending order", () =>
                    {
                        var h = new MinHeap();
                        foreach (var v in new[]{5,3,8,1,9,2}) h.Push(v);
                        Assert.Equal(1, h.Peek());
                        Assert.Equal(1, h.Pop());
                        Assert.Equal(2, h.Pop());
                        Assert.Equal(3, h.Pop());
                        Assert.Equal(3, h.Count);
                    });
                    r.Check("single element", () =>
                    {
                        var h = new MinHeap();
                        h.Push(42);
                        Assert.Equal(42, h.Pop());
                        Assert.Equal(0, h.Count);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public sealed class MinHeap
            {
                private readonly List<int> _data = new();
                public int Count => _data.Count;

                public void Push(int value)
                {
                    _data.Add(value);
                    int i = _data.Count - 1;
                    while (i > 0)                              // sift up
                    {
                        int parent = (i - 1) / 2;
                        if (_data[parent] <= _data[i]) break;
                        (_data[parent], _data[i]) = (_data[i], _data[parent]);
                        i = parent;
                    }
                }

                public int Peek() => _data[0];

                public int Pop()
                {
                    int min = _data[0];
                    int last = _data.Count - 1;
                    _data[0] = _data[last];
                    _data.RemoveAt(last);

                    int i = 0, n = _data.Count;
                    while (true)                              // sift down
                    {
                        int l = 2 * i + 1, r = 2 * i + 2, smallest = i;
                        if (l < n && _data[l] < _data[smallest]) smallest = l;
                        if (r < n && _data[r] < _data[smallest]) smallest = r;
                        if (smallest == i) break;
                        (_data[i], _data[smallest]) = (_data[smallest], _data[i]);
                        i = smallest;
                    }
                    return min;
                }
            }
            """,
        Hints =
        [
            "Store the heap in a list; parent of i is (i-1)/2, children are 2i+1 and 2i+2.",
            "Push appends then sifts up while smaller than its parent.",
            "Pop swaps root with the last element, removes it, then sifts down to the smaller child.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "pops in ascending order", IsHidden = false },
            new TestCaseSeed { Name = "single element", IsHidden = false },
        ],
    };

    // =========================================================================
    private static LessonSeed ScalingLesson => new()
    {
        Slug = "scaling-primitives",
        Title = "Scaling Primitives",
        Order = 2,
        MarkdownContent =
            """
            ## Scaling Primitives

            - **Union-Find (Disjoint Set)** — near-O(1) "are these connected?" with path
              compression; powers connectivity, Kruskal's MST, and grouping.
            - **Token-bucket rate limiter** — allow bursts up to a capacity, refilling tokens
              over time; the standard approach to API throttling.
            """,
        Exercises =
        [
            UnionFind,
            TokenBucket,
        ],
    };

    private static ExerciseSeed UnionFind => new()
    {
        Slug = "union-find",
        Title = "Union-Find (Disjoint Set)",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement `UnionFind(n)` over elements `0..n-1` with `Union(a, b)` (merge their
            sets) and `Connected(a, b)` (same set?). Use a parent array with **path
            compression** in `Find` for near-constant-time operations.
            """,
        StarterCode =
            """
            public sealed class UnionFind
            {
                public UnionFind(int n) => throw new System.NotImplementedException();
                public void Union(int a, int b) => throw new System.NotImplementedException();
                public bool Connected(int a, int b) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("connects transitively", () =>
                    {
                        var uf = new UnionFind(5);
                        uf.Union(0, 1);
                        uf.Union(1, 2);
                        Assert.True(uf.Connected(0, 2));   // 0-1-2
                        Assert.False(uf.Connected(0, 3));
                    });
                    r.Check("separate groups stay separate", () =>
                    {
                        var uf = new UnionFind(5);
                        uf.Union(0, 1);
                        uf.Union(3, 4);
                        Assert.True(uf.Connected(3, 4));
                        Assert.False(uf.Connected(1, 4));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public sealed class UnionFind
            {
                private readonly int[] _parent;

                public UnionFind(int n)
                {
                    _parent = new int[n];
                    for (int i = 0; i < n; i++) _parent[i] = i; // each element is its own root
                }

                private int Find(int x)
                {
                    if (_parent[x] != x)
                        _parent[x] = Find(_parent[x]); // path compression: point straight to root
                    return _parent[x];
                }

                public void Union(int a, int b) => _parent[Find(a)] = Find(b);

                public bool Connected(int a, int b) => Find(a) == Find(b);
            }
            """,
        Hints =
        [
            "Start with each element as its own parent (root).",
            "Find follows parents to the root; compress by repointing along the way.",
            "Union links one root under the other; Connected checks equal roots.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "connects transitively", IsHidden = false },
            new TestCaseSeed { Name = "separate groups stay separate", IsHidden = false },
        ],
    };

    private static ExerciseSeed TokenBucket => new()
    {
        Slug = "token-bucket",
        Title = "Token-Bucket Rate Limiter",
        Difficulty = "Hard",
        Kind = "Class",
        Prompt =
            """
            Implement a `TokenBucket(capacity, refillTokens, refillIntervalTicks)`. It starts
            full. `Allow(nowTick)` first refills `refillTokens` for every full
            `refillIntervalTicks` elapsed (capped at capacity), then consumes one token and
            returns true if one was available, else false. Time is an explicit tick parameter.
            """,
        StarterCode =
            """
            public sealed class TokenBucket
            {
                public TokenBucket(int capacity, int refillTokens, long refillIntervalTicks)
                    => throw new System.NotImplementedException();

                // TODO: refill based on elapsed ticks, then try to consume one token.
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
                    r.Check("bursts up to capacity, then blocks", () =>
                    {
                        var b = new TokenBucket(2, 1, 10); // cap 2, +1 token / 10 ticks
                        Assert.True(b.Allow(0));   // 2 -> 1
                        Assert.True(b.Allow(0));   // 1 -> 0
                        Assert.False(b.Allow(0));  // empty
                    });
                    r.Check("refills over time", () =>
                    {
                        var b = new TokenBucket(2, 1, 10);
                        b.Allow(0); b.Allow(0);    // drain to 0
                        Assert.False(b.Allow(5));  // not enough elapsed
                        Assert.True(b.Allow(10));  // +1 token refilled
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public sealed class TokenBucket
            {
                private readonly int _capacity;
                private readonly int _refillTokens;
                private readonly long _interval;
                private int _tokens;
                private long _lastRefillTick;

                public TokenBucket(int capacity, int refillTokens, long refillIntervalTicks)
                {
                    _capacity = capacity;
                    _refillTokens = refillTokens;
                    _interval = refillIntervalTicks;
                    _tokens = capacity;       // starts full
                    _lastRefillTick = 0;
                }

                public bool Allow(long nowTick)
                {
                    // Refill for each full interval elapsed since the last refill.
                    long elapsed = nowTick - _lastRefillTick;
                    if (elapsed >= _interval)
                    {
                        long intervals = elapsed / _interval;
                        _tokens = (int)Math.Min(_capacity, _tokens + intervals * _refillTokens);
                        _lastRefillTick += intervals * _interval;
                    }

                    if (_tokens > 0) { _tokens--; return true; }
                    return false;
                }
            }
            """,
        Hints =
        [
            "Track current tokens and the tick of the last refill.",
            "On Allow, add refillTokens for each full interval elapsed, capped at capacity.",
            "Then consume one token if available; otherwise deny.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "bursts up to capacity, then blocks", IsHidden = false },
            new TestCaseSeed { Name = "refills over time", IsHidden = false },
        ],
    };
}
