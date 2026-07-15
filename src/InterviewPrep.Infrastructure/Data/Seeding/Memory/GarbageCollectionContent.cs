namespace InterviewPrep.Infrastructure.Data.Seeding.Memory;

// The "Memory & Garbage Collection" topic. The GC frees managed memory for you,
// but you still control UNMANAGED resource lifetimes (IDisposable) and can reduce
// GC pressure (pooling). These are the gradeable, interview-relevant angles.
internal static partial class GarbageCollectionContent
{
    public static TopicSeed Topic => new()
    {
        Slug = "garbage-collection",
        Name = "Memory & Garbage Collection",
        Description = "Deterministic cleanup with IDisposable, disposal guards, and reducing GC pressure via pooling.",
        Order = 5,
        Lessons =
        [
            DisposalLesson,
            ValueTypesLesson,
        ],
    };

    private static LessonSeed DisposalLesson => new()
    {
        Slug = "disposal-and-pooling",
        Title = "Disposal & Pooling",
        Order = 1,
        MarkdownContent =
            """
            ## Memory & Garbage Collection

            The **GC** reclaims managed objects automatically (generational: Gen 0/1/2).
            What it does NOT do for you:
            - Release **unmanaged** resources (files, sockets, handles) — that's what
              `IDisposable` / `using` are for (deterministic cleanup).
            - Avoid allocations — high allocation rates mean more GC pauses. **Pooling**
              reuses objects to cut that pressure.

            Rules of thumb: implement `IDisposable` when you hold unmanaged/expensive
            resources; make `Dispose` idempotent; guard members with
            `ObjectDisposedException` after disposal.
            """,
        Exercises =
        [
            DisposableBasic,
            DisposeGuard,
            ObjectPool,
        ],
    };

    private static ExerciseSeed DisposableBasic => new()
    {
        Slug = "idisposable-basic",
        Title = "Implement IDisposable",
        Difficulty = "Easy",
        Kind = "Class",
        Prompt =
            """
            Implement `FileHandle : IDisposable`. `Dispose()` should mark it disposed
            (set `Disposed = true`) and be **idempotent** — calling it twice must not
            throw. This is the foundation of deterministic cleanup with `using`.
            """,
        StarterCode =
            """
            using System;

            public sealed class FileHandle : IDisposable
            {
                public bool Disposed { get; private set; }

                // TODO: mark disposed; safe to call more than once.
                public void Dispose() => throw new NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("not disposed initially, disposed after Dispose", () =>
                    {
                        var f = new FileHandle();
                        Assert.False(f.Disposed);
                        f.Dispose();
                        Assert.True(f.Disposed);
                    });
                    r.Check("Dispose is idempotent", () =>
                    {
                        var f = new FileHandle();
                        f.Dispose();
                        f.Dispose(); // must not throw
                        Assert.True(f.Disposed);
                    });
                    r.Check("using disposes at end of scope", () =>
                    {
                        FileHandle captured;
                        using (var f = new FileHandle()) { captured = f; }
                        Assert.True(captured.Disposed);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public sealed class FileHandle : IDisposable
            {
                public bool Disposed { get; private set; }

                public void Dispose()
                {
                    if (Disposed) return; // idempotent guard
                    Disposed = true;
                    // (real code would release the unmanaged handle here)
                }
            }
            """,
        Hints =
        [
            "Set Disposed = true inside Dispose.",
            "Guard with `if (Disposed) return;` so a second call is a no-op.",
            "`using` calls Dispose automatically when the block exits.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "not disposed initially, disposed after Dispose", IsHidden = false },
            new TestCaseSeed { Name = "Dispose is idempotent", IsHidden = false },
            new TestCaseSeed { Name = "using disposes at end of scope", IsHidden = true },
        ],
    };

    private static ExerciseSeed DisposeGuard => new()
    {
        Slug = "dispose-guard",
        Title = "Guard Against Use-After-Dispose",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement `Connection : IDisposable`. `Query()` returns `42` while open, but
            once disposed it must throw `ObjectDisposedException`. `Dispose()` stays
            idempotent. This is the standard "fail loudly after disposal" contract.
            """,
        StarterCode =
            """
            using System;

            public sealed class Connection : IDisposable
            {
                public bool Disposed { get; private set; }

                // TODO: return 42 when open; throw ObjectDisposedException when disposed.
                public int Query() => throw new NotImplementedException();

                public void Dispose() => throw new NotImplementedException();
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
                    r.Check("Query works before dispose", () =>
                    {
                        var c = new Connection();
                        Assert.Equal(42, c.Query());
                    });
                    r.Check("Query after dispose throws ObjectDisposedException", () =>
                    {
                        var c = new Connection();
                        c.Dispose();
                        bool threw = false;
                        try { c.Query(); }
                        catch (ObjectDisposedException) { threw = true; }
                        Assert.True(threw);
                    });
                    r.Check("Dispose is idempotent", () =>
                    {
                        var c = new Connection();
                        c.Dispose();
                        c.Dispose();
                        Assert.True(c.Disposed);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public sealed class Connection : IDisposable
            {
                public bool Disposed { get; private set; }

                public int Query()
                {
                    // Fail fast if the object has been disposed.
                    ObjectDisposedException.ThrowIf(Disposed, this);
                    return 42;
                }

                public void Dispose() => Disposed = true; // idempotent: setting true twice is fine
            }
            """,
        Hints =
        [
            "In Query, check Disposed first and throw ObjectDisposedException if set.",
            "`ObjectDisposedException.ThrowIf(Disposed, this)` is the modern one-liner.",
            "Dispose just sets the flag; setting it true twice is naturally idempotent.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "Query works before dispose", IsHidden = false },
            new TestCaseSeed { Name = "Query after dispose throws ObjectDisposedException", IsHidden = false },
            new TestCaseSeed { Name = "Dispose is idempotent", IsHidden = true },
        ],
    };

    private static ExerciseSeed ObjectPool => new()
    {
        Slug = "object-pool",
        Title = "Object Pool (Reduce GC Pressure)",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement a simple `ObjectPool<T>` with `Rent()` and `Return(item)`. `Rent`
            hands back a pooled instance if available, otherwise a new one; `Return` puts
            an instance back for reuse. Reusing objects reduces allocations and therefore
            GC pressure — a common hot-path optimization.
            """,
        StarterCode =
            """
            public sealed class ObjectPool<T> where T : class, new()
            {
                // TODO: Rent reuses a returned instance or creates one; Return stores it.
                public T Rent() => throw new System.NotImplementedException();
                public void Return(T item) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("returned instance is reused", () =>
                    {
                        var pool = new ObjectPool<object>();
                        var a = pool.Rent();
                        pool.Return(a);
                        var b = pool.Rent();
                        Assert.True(ReferenceEquals(a, b)); // same object came back
                    });
                    r.Check("rent on empty pool creates a new instance", () =>
                    {
                        var pool = new ObjectPool<object>();
                        var a = pool.Rent();
                        Assert.True(a != null);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public sealed class ObjectPool<T> where T : class, new()
            {
                private readonly Stack<T> _available = new();

                // Reuse a pooled instance if one exists; otherwise allocate a fresh one.
                public T Rent() => _available.Count > 0 ? _available.Pop() : new T();

                // Hand the instance back for future reuse.
                public void Return(T item) => _available.Push(item);
            }
            """,
        Hints =
        [
            "Keep returned instances in a stack (or queue).",
            "Rent pops a stored instance if available, else `new T()`.",
            "Return pushes the instance back so the next Rent reuses it.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "returned instance is reused", IsHidden = false },
            new TestCaseSeed { Name = "rent on empty pool creates a new instance", IsHidden = false },
        ],
    };
}
