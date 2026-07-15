namespace InterviewPrep.Infrastructure.Data.Seeding.Enterprise;

// "Enterprise Patterns" — the application-architecture patterns beyond the GoF set
// that show up in real .NET codebases and senior interviews: data-access patterns
// (Repository, Unit of Work, Specification) and resilience/flow patterns (CQRS,
// Result, Circuit Breaker, Retry).
internal static partial class EnterprisePatternsContent
{
    public static TopicSeed Topic => new()
    {
        Slug = "enterprise-patterns",
        Name = "Enterprise Patterns",
        Description = "Repository, Unit of Work, Specification, CQRS, Mediator, Decorator, Pipeline, Outbox, Result, Circuit Breaker, Retry — the patterns behind real .NET apps.",
        Order = 10,
        Lessons =
        [
            DataAccessLesson,
            OrchestrationLesson,
            ResilienceLesson,
        ],
    };

    // =========================================================================
    private static LessonSeed DataAccessLesson => new()
    {
        Slug = "data-access-patterns",
        Title = "Data Access Patterns",
        Order = 1,
        MarkdownContent =
            """
            ## Data Access Patterns

            The data layer is where most enterprise apps grow tangled: business logic leaks
            SQL, tests need a database, and query rules get copy-pasted. Three patterns keep it
            clean. They are usually taught together because they compose into a single
            "persistence facade."

            ### Repository — *"a collection you can query, backed by a store"*

            **What it solves.** Domain and application code should ask for *entities*, not
            write SQL or touch `DbContext`. A repository is an in-memory-collection-like
            abstraction (`Add`, `GetById`, `GetAll`, `Remove`) over persistence. Swap the
            implementation (SQL, in-memory, HTTP) without touching callers, and unit-test
            services against a fake repository.

            **Tradeoffs / when NOT to use.** EF Core's `DbSet<T>` is *already* a repository,
            and `DbContext` is *already* a unit of work. Wrapping them in a thin generic
            repository often just hides LINQ and adds a leaky layer — you lose `Include`,
            projections, and async streaming. Use a repository when you want a **domain-shaped**
            interface (`IOrderRepository.GetUnshippedOrders()`), not a generic CRUD passthrough.

            **Interview framing.** "A repository is the Collection illusion over a data store.
            The debate isn't *whether* the pattern is good — it's whether a *generic* repository
            over EF earns its keep, since EF already gives you one."

            ### Unit of Work — *"one atomic transaction boundary"*

            **What it solves.** A single business operation often touches several entities. You
            want them to commit **all or nothing**. Unit of Work tracks staged changes and
            flushes them in one transaction; on failure, nothing is applied (rollback). This is
            what gives you *atomicity* — no half-written state.

            **Tradeoffs.** Sharing one UoW per request is the norm (scoped lifetime); sharing it
            across requests corrupts state. Don't call `SaveChanges` inside repositories — that
            defeats the single boundary. The unit of work *owns* commit.

            **Interview framing.** "Repository = *what* to persist; Unit of Work = *when* to
            persist it, atomically."

            ### Specification — *"a named, combinable business rule"*

            **What it solves.** Query predicates (`IsEligibleForDiscount`, `IsOverdue`) get
            duplicated across controllers and services. A Specification wraps one rule as an
            object with `IsSatisfiedBy(item)`. You give it a name, test it in isolation, and —
            crucially — **combine** specs with `And`/`Or`/`Not` instead of writing a new class
            for every combination.

            **Tradeoffs.** In-memory specs (`Func<T,bool>`) can't translate to SQL; for EF you
            need `Expression<Func<T,bool>>` specs so the provider can build a `WHERE`. Overusing
            specifications for trivial one-off filters is ceremony — reach for them when a rule
            is *reused* or *composed*.

            **Interview framing.** "Specification turns a boolean business rule into a
            first-class, composable value — the Strategy pattern applied to predicates."
            """,
        Exercises =
        [
            Repository,
            UnitOfWork,
            Specification,
            SpecificationComposition,
            UnitOfWorkRollback,
        ],
    };

    private static ExerciseSeed Repository => new()
    {
        Slug = "repository",
        Title = "Generic Repository",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement a generic `InMemoryRepository<T>` (for `T : IEntity`, provided) with
            `Add`, `GetById(id)` (null if absent), and `GetAll()`. This is the collection-like
            abstraction the Repository pattern puts in front of a data store.
            """,
        StarterCode =
            """
            using System.Collections.Generic;
            using System.Linq;

            // PROVIDED — do not modify:
            public interface IEntity { int Id { get; } }

            public sealed class InMemoryRepository<T> where T : IEntity
            {
                // TODO: store by Id; implement Add / GetById / GetAll.
                public void Add(T item) => throw new System.NotImplementedException();
                public T? GetById(int id) => throw new System.NotImplementedException();
                public IReadOnlyList<T> GetAll() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            using System.Linq;

            public static class __Harness
            {
                private sealed record User(int Id, string Name) : IEntity;

                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("add and get by id", () =>
                    {
                        var repo = new InMemoryRepository<User>();
                        repo.Add(new User(1, "ann"));
                        repo.Add(new User(2, "bob"));
                        Assert.Equal("bob", repo.GetById(2)!.Name);
                    });
                    r.Check("missing id -> null", () =>
                    {
                        var repo = new InMemoryRepository<User>();
                        Assert.True(repo.GetById(99) == null);
                    });
                    r.Check("get all returns everything", () =>
                    {
                        var repo = new InMemoryRepository<User>();
                        repo.Add(new User(1, "ann"));
                        repo.Add(new User(2, "bob"));
                        Assert.Equal(2, repo.GetAll().Count);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;
            using System.Linq;

            public interface IEntity { int Id { get; } }

            public sealed class InMemoryRepository<T> where T : IEntity
            {
                private readonly Dictionary<int, T> _store = new();

                public void Add(T item) => _store[item.Id] = item;
                public T? GetById(int id) => _store.TryGetValue(id, out var item) ? item : default;
                public IReadOnlyList<T> GetAll() => _store.Values.ToList();
            }
            """,
        Hints =
        [
            "A Dictionary<int, T> keyed by Id makes lookups O(1).",
            "GetById returns default(T) (null for reference types) when the key is missing.",
            "GetAll materializes the stored values into a list.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "add and get by id", IsHidden = false },
            new TestCaseSeed { Name = "missing id -> null", IsHidden = false },
            new TestCaseSeed { Name = "get all returns everything", IsHidden = true },
        ],
    };

    private static ExerciseSeed UnitOfWork => new()
    {
        Slug = "unit-of-work",
        Title = "Unit of Work",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement a `UnitOfWork` that batches changes and commits them together.
            `RegisterNew(item)` stages an item, `PendingCount` reports how many are staged,
            and `Commit()` returns all staged items **and clears the pending set** (so a
            second commit yields nothing).
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public sealed class UnitOfWork
            {
                // TODO: stage items; Commit returns and clears them.
                public void RegisterNew(string item) => throw new System.NotImplementedException();
                public int PendingCount => throw new System.NotImplementedException();
                public IReadOnlyList<string> Commit() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("stages then commits atomically", () =>
                    {
                        var uow = new UnitOfWork();
                        uow.RegisterNew("a");
                        uow.RegisterNew("b");
                        Assert.Equal(2, uow.PendingCount);
                        var committed = uow.Commit();
                        Assert.Equal(2, committed.Count);
                        Assert.Equal(0, uow.PendingCount);   // cleared after commit
                        Assert.Equal(0, uow.Commit().Count); // nothing left to commit
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public sealed class UnitOfWork
            {
                private readonly List<string> _pending = new();

                public void RegisterNew(string item) => _pending.Add(item);
                public int PendingCount => _pending.Count;

                public IReadOnlyList<string> Commit()
                {
                    var committed = new List<string>(_pending); // snapshot
                    _pending.Clear();                            // atomic "flush"
                    return committed;
                }
            }
            """,
        Hints =
        [
            "Keep staged items in a list.",
            "Commit should copy the staged items, then clear the list.",
            "After clearing, PendingCount is 0 and a second Commit returns empty.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "stages then commits atomically", IsHidden = false },
        ],
    };

    private static ExerciseSeed Specification => new()
    {
        Slug = "specification",
        Title = "Specification Pattern",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            The Specification pattern turns a business rule into a combinable object. Given
            `ISpecification<T>` and a concrete `PredicateSpec<T>` (provided), implement
            `AndSpecification<T>` whose `IsSatisfiedBy` is true only when **both** specs are.
            """,
        StarterCode =
            """
            using System;

            // PROVIDED — do not modify:
            public interface ISpecification<T> { bool IsSatisfiedBy(T item); }
            public sealed class PredicateSpec<T> : ISpecification<T>
            {
                private readonly Func<T, bool> _predicate;
                public PredicateSpec(Func<T, bool> predicate) => _predicate = predicate;
                public bool IsSatisfiedBy(T item) => _predicate(item);
            }

            // TODO: combine two specs with AND.
            public sealed class AndSpecification<T> : ISpecification<T>
            {
                public AndSpecification(ISpecification<T> left, ISpecification<T> right)
                    => throw new NotImplementedException();
                public bool IsSatisfiedBy(T item) => throw new NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    var even = new PredicateSpec<int>(x => x % 2 == 0);
                    var positive = new PredicateSpec<int>(x => x > 0);
                    var evenAndPositive = new AndSpecification<int>(even, positive);

                    r.Check("4 is even AND positive", () => Assert.True(evenAndPositive.IsSatisfiedBy(4)));
                    r.Check("-2 is even but not positive", () => Assert.False(evenAndPositive.IsSatisfiedBy(-2)));
                    r.Check("3 is positive but not even", () => Assert.False(evenAndPositive.IsSatisfiedBy(3)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public interface ISpecification<T> { bool IsSatisfiedBy(T item); }
            public sealed class PredicateSpec<T> : ISpecification<T>
            {
                private readonly Func<T, bool> _predicate;
                public PredicateSpec(Func<T, bool> predicate) => _predicate = predicate;
                public bool IsSatisfiedBy(T item) => _predicate(item);
            }

            public sealed class AndSpecification<T> : ISpecification<T>
            {
                private readonly ISpecification<T> _left, _right;
                public AndSpecification(ISpecification<T> left, ISpecification<T> right)
                {
                    _left = left; _right = right;
                }
                public bool IsSatisfiedBy(T item) => _left.IsSatisfiedBy(item) && _right.IsSatisfiedBy(item);
            }
            """,
        Hints =
        [
            "Store both specifications.",
            "IsSatisfiedBy returns left.IsSatisfiedBy(item) && right.IsSatisfiedBy(item).",
            "This is how specs compose into bigger rules without new classes per combination.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "4 is even AND positive", IsHidden = false },
            new TestCaseSeed { Name = "-2 is even but not positive", IsHidden = false },
            new TestCaseSeed { Name = "3 is positive but not even", IsHidden = true },
        ],
    };

    private static ExerciseSeed SpecificationComposition => new()
    {
        Slug = "specification-composition",
        Title = "Specification Composition (And/Or/Not)",
        Difficulty = "Hard",
        Kind = "Class",
        Prompt =
            """
            Take the Specification pattern further: make specs fully composable. Given
            `ISpecification<T>` (provided) with `IsSatisfiedBy`, implement three combinators —
            `AndSpec<T>`, `OrSpec<T>`, `NotSpec<T>` — and a static `Filter` helper that returns
            every item in a sequence satisfying a spec.

            Then a composite like `active AND (premium OR admin) AND NOT banned` must filter a
            collection correctly. This is why the pattern earns its keep: arbitrary rule trees
            without a new class per combination.
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;

            // PROVIDED — do not modify:
            public interface ISpecification<T> { bool IsSatisfiedBy(T item); }
            public sealed class Spec<T> : ISpecification<T>
            {
                private readonly Func<T, bool> _p;
                public Spec(Func<T, bool> p) => _p = p;
                public bool IsSatisfiedBy(T item) => _p(item);
            }

            // TODO: implement the three combinators.
            public sealed class AndSpec<T> : ISpecification<T>
            {
                public AndSpec(ISpecification<T> a, ISpecification<T> b) => throw new NotImplementedException();
                public bool IsSatisfiedBy(T item) => throw new NotImplementedException();
            }
            public sealed class OrSpec<T> : ISpecification<T>
            {
                public OrSpec(ISpecification<T> a, ISpecification<T> b) => throw new NotImplementedException();
                public bool IsSatisfiedBy(T item) => throw new NotImplementedException();
            }
            public sealed class NotSpec<T> : ISpecification<T>
            {
                public NotSpec(ISpecification<T> inner) => throw new NotImplementedException();
                public bool IsSatisfiedBy(T item) => throw new NotImplementedException();
            }

            public static class SpecFilter
            {
                // TODO: return items satisfying spec, preserving order.
                public static List<T> Filter<T>(IEnumerable<T> items, ISpecification<T> spec)
                    => throw new NotImplementedException();
            }
            """,
        HarnessCode =
            """
            using System.Collections.Generic;

            public static class __Harness
            {
                private sealed record Member(string Name, bool Active, bool Premium, bool Admin, bool Banned);

                public static string Run()
                {
                    var r = new HarnessReport();

                    var active  = new Spec<Member>(m => m.Active);
                    var premium = new Spec<Member>(m => m.Premium);
                    var admin   = new Spec<Member>(m => m.Admin);
                    var banned  = new Spec<Member>(m => m.Banned);

                    // active AND (premium OR admin) AND NOT banned
                    ISpecification<Member> composite =
                        new AndSpec<Member>(
                            new AndSpec<Member>(active, new OrSpec<Member>(premium, admin)),
                            new NotSpec<Member>(banned));

                    var people = new List<Member>
                    {
                        new("ann", Active: true,  Premium: true,  Admin: false, Banned: false), // pass
                        new("bob", Active: true,  Premium: false, Admin: true,  Banned: false), // pass (admin)
                        new("cid", Active: true,  Premium: true,  Admin: false, Banned: true),  // fail (banned)
                        new("dan", Active: false, Premium: true,  Admin: true,  Banned: false), // fail (inactive)
                        new("eve", Active: true,  Premium: false, Admin: false, Banned: false), // fail (neither)
                    };

                    r.Check("And combines both", () =>
                    {
                        var s = new AndSpec<Member>(active, premium);
                        Assert.True(s.IsSatisfiedBy(people[0]));
                        Assert.False(s.IsSatisfiedBy(people[1]));
                    });
                    r.Check("Or needs either", () =>
                    {
                        var s = new OrSpec<Member>(premium, admin);
                        Assert.True(s.IsSatisfiedBy(people[1]));   // admin
                        Assert.False(s.IsSatisfiedBy(people[4]));  // neither
                    });
                    r.Check("Not inverts", () =>
                        Assert.True(new NotSpec<Member>(banned).IsSatisfiedBy(people[0])));

                    r.Check("composite filters collection correctly", () =>
                    {
                        var matched = SpecFilter.Filter(people, composite);
                        Assert.Equal(2, matched.Count);
                        Assert.Equal("ann", matched[0].Name);
                        Assert.Equal("bob", matched[1].Name);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Collections.Generic;

            public interface ISpecification<T> { bool IsSatisfiedBy(T item); }
            public sealed class Spec<T> : ISpecification<T>
            {
                private readonly Func<T, bool> _p;
                public Spec(Func<T, bool> p) => _p = p;
                public bool IsSatisfiedBy(T item) => _p(item);
            }

            public sealed class AndSpec<T> : ISpecification<T>
            {
                private readonly ISpecification<T> _a, _b;
                public AndSpec(ISpecification<T> a, ISpecification<T> b) { _a = a; _b = b; }
                public bool IsSatisfiedBy(T item) => _a.IsSatisfiedBy(item) && _b.IsSatisfiedBy(item);
            }
            public sealed class OrSpec<T> : ISpecification<T>
            {
                private readonly ISpecification<T> _a, _b;
                public OrSpec(ISpecification<T> a, ISpecification<T> b) { _a = a; _b = b; }
                public bool IsSatisfiedBy(T item) => _a.IsSatisfiedBy(item) || _b.IsSatisfiedBy(item);
            }
            public sealed class NotSpec<T> : ISpecification<T>
            {
                private readonly ISpecification<T> _inner;
                public NotSpec(ISpecification<T> inner) => _inner = inner;
                public bool IsSatisfiedBy(T item) => !_inner.IsSatisfiedBy(item);
            }

            public static class SpecFilter
            {
                public static List<T> Filter<T>(IEnumerable<T> items, ISpecification<T> spec)
                {
                    var result = new List<T>();
                    foreach (var item in items)
                        if (spec.IsSatisfiedBy(item)) result.Add(item);
                    return result;
                }
            }
            """,
        Hints =
        [
            "Each combinator stores its operand specs and delegates to their IsSatisfiedBy.",
            "AndSpec uses &&, OrSpec uses ||, NotSpec uses ! — the boolean logic mirrors the operator name.",
            "Filter iterates once and keeps items where spec.IsSatisfiedBy is true, preserving order.",
            "Because every combinator is itself an ISpecification<T>, they nest into arbitrary trees.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "And combines both", IsHidden = false },
            new TestCaseSeed { Name = "Or needs either", IsHidden = false },
            new TestCaseSeed { Name = "Not inverts", IsHidden = false },
            new TestCaseSeed { Name = "composite filters collection correctly", IsHidden = true },
        ],
    };

    private static ExerciseSeed UnitOfWorkRollback => new()
    {
        Slug = "unit-of-work-rollback",
        Title = "Unit of Work — Commit & Rollback Atomicity",
        Difficulty = "Hard",
        Kind = "Class",
        Prompt =
            """
            Model the real value of Unit of Work: **atomicity**. You have an in-memory `Store`
            (a name→balance map, provided) and a `UnitOfWork` that stages operations without
            touching the store until `Commit()`.

            - `RegisterSet(key, value)` stages a write.
            - `Commit()` validates every staged op first (values must be >= 0); if ALL are
              valid it applies them to the store and clears the batch and returns true. If ANY
              staged value is negative it applies NOTHING (rollback), clears the batch, and
              returns false.
            - `Rollback()` discards the batch without touching the store.

            The key behavioral guarantee: a bad op mid-batch leaves the store in its original
            state — no partial writes.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            // PROVIDED — do not modify:
            public sealed class Store
            {
                private readonly Dictionary<string, int> _data = new();
                public void Apply(string key, int value) => _data[key] = value;
                public int? Get(string key) => _data.TryGetValue(key, out var v) ? v : (int?)null;
                public int Count => _data.Count;
            }

            public sealed class UnitOfWork
            {
                public UnitOfWork(Store store) => throw new System.NotImplementedException();

                public void RegisterSet(string key, int value) => throw new System.NotImplementedException();
                public int PendingCount => throw new System.NotImplementedException();

                // Apply all staged ops iff every value is valid (>= 0); else apply none.
                public bool Commit() => throw new System.NotImplementedException();

                // Discard the batch without touching the store.
                public void Rollback() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();

                    r.Check("commit applies all staged writes", () =>
                    {
                        var store = new Store();
                        var uow = new UnitOfWork(store);
                        uow.RegisterSet("ann", 100);
                        uow.RegisterSet("bob", 50);
                        Assert.Equal(2, uow.PendingCount);
                        Assert.True(uow.Commit());
                        Assert.Equal(100, store.Get("ann"));
                        Assert.Equal(50, store.Get("bob"));
                        Assert.Equal(0, uow.PendingCount); // batch cleared
                    });

                    r.Check("a bad op mid-batch leaves NO partial state", () =>
                    {
                        var store = new Store();
                        var uow = new UnitOfWork(store);
                        uow.RegisterSet("ann", 100);
                        uow.RegisterSet("bob", -5);   // invalid
                        uow.RegisterSet("cid", 30);
                        Assert.False(uow.Commit());   // whole batch rejected
                        Assert.Equal(0, store.Count);  // nothing applied — atomic
                        Assert.True(store.Get("ann") == null);
                    });

                    r.Check("rollback discards without touching store", () =>
                    {
                        var store = new Store();
                        var uow = new UnitOfWork(store);
                        uow.RegisterSet("ann", 10);
                        uow.Rollback();
                        Assert.Equal(0, uow.PendingCount);
                        Assert.Equal(0, store.Count);
                    });

                    r.Check("failed commit clears batch (fresh start)", () =>
                    {
                        var store = new Store();
                        var uow = new UnitOfWork(store);
                        uow.RegisterSet("x", -1);
                        Assert.False(uow.Commit());
                        Assert.Equal(0, uow.PendingCount);
                        uow.RegisterSet("y", 7);      // new batch is independent
                        Assert.True(uow.Commit());
                        Assert.Equal(7, store.Get("y"));
                        Assert.Equal(1, store.Count);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public sealed class Store
            {
                private readonly Dictionary<string, int> _data = new();
                public void Apply(string key, int value) => _data[key] = value;
                public int? Get(string key) => _data.TryGetValue(key, out var v) ? v : (int?)null;
                public int Count => _data.Count;
            }

            public sealed class UnitOfWork
            {
                private readonly Store _store;
                private readonly List<(string Key, int Value)> _pending = new();

                public UnitOfWork(Store store) => _store = store;

                public void RegisterSet(string key, int value) => _pending.Add((key, value));
                public int PendingCount => _pending.Count;

                public bool Commit()
                {
                    // Validate the WHOLE batch before applying anything.
                    foreach (var op in _pending)
                    {
                        if (op.Value < 0)
                        {
                            _pending.Clear(); // rollback: apply nothing
                            return false;
                        }
                    }
                    foreach (var op in _pending)
                        _store.Apply(op.Key, op.Value);
                    _pending.Clear();
                    return true;
                }

                public void Rollback() => _pending.Clear();
            }
            """,
        Hints =
        [
            "Stage ops in a list of (key, value); do NOT write to the store in RegisterSet.",
            "Commit must validate the entire batch first — loop once to check, only then loop again to apply.",
            "If validation fails, clear the batch and return false without applying a single op (atomicity).",
            "Rollback and a failed commit both just clear the pending list.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "commit applies all staged writes", IsHidden = false },
            new TestCaseSeed { Name = "a bad op mid-batch leaves NO partial state", IsHidden = false },
            new TestCaseSeed { Name = "rollback discards without touching store", IsHidden = false },
            new TestCaseSeed { Name = "failed commit clears batch (fresh start)", IsHidden = true },
        ],
    };

    // =========================================================================
    private static LessonSeed OrchestrationLesson => new()
    {
        Slug = "orchestration-patterns",
        Title = "Mediator, Decorator & Pipelines",
        Order = 2,
        MarkdownContent =
            """
            ## Mediator, Decorator & Pipelines

            Once you have commands and queries, the next question is *how requests reach their
            handlers* and *how cross-cutting concerns wrap them*. These patterns are the
            backbone of MediatR-style architectures.

            ### Mediator — *"send a request; the mediator finds its one handler"*

            **What it solves.** Instead of a controller newing up and calling handlers directly
            (tight coupling, giant constructors), you `Send(request)` to a mediator. It looks up
            the single handler registered for that request type and dispatches. Callers depend
            on one `IMediator`, not on N services.

            **Tradeoffs / when NOT to use.** It adds indirection — jumping to a handler is a
            "find usages" hunt, not a call graph. For a tiny app the layer is pure ceremony.
            And a mediator is *not* a message bus: it's in-process, synchronous, one handler per
            request. Reaching for MediatR to "decouple" inside a single small service is a
            common over-engineering smell.

            **Interview framing.** "A mediator centralizes dispatch: one entry point maps a
            request type to its handler, so senders and handlers never reference each other."

            ### Decorator — *"wrap a component to add behavior without changing it"*

            **What it solves.** You want to add caching, logging, or metrics to a repository or
            handler *without editing it* (Open/Closed). A decorator implements the same
            interface, holds the inner instance, and adds behavior around the delegated call. A
            **caching decorator** checks a cache first and only calls the inner component on a
            miss — so a repeated read hits the cache, and the expensive call happens once per
            key.

            **Tradeoffs.** Decorator order matters (cache-then-log vs log-then-cache behave
            differently). Deep decorator stacks are hard to debug. DI containers wire them via
            `Decorate<>` registrations (Scrutor).

            **Interview framing.** "Decorator = same interface, wraps an instance, adds behavior
            around it. Caching, retry, and logging decorators are how you add cross-cutting
            concerns without touching the core."

            ### Pipeline behaviors — *"a chain of middleware around every handler"*

            **What it solves.** Validation, logging, and transactions apply to *every* request.
            Rather than a decorator per handler, a **pipeline** threads the request through an
            ordered list of behaviors (validation → logging → handler), each able to short-
            circuit (a validation failure never reaches the handler) or wrap the next step. It's
            the Chain of Responsibility applied to request handling — the same idea as ASP.NET
            middleware.

            **Interview framing.** "Pipeline behaviors are middleware for your application layer:
            one ordered chain wrapping every handler, so cross-cutting logic lives in one place."
            """,
        Exercises =
        [
            Mediator,
            CachingDecorator,
            Pipeline,
        ],
    };

    private static ExerciseSeed Mediator => new()
    {
        Slug = "mediator-dispatch",
        Title = "Mediator Dispatch",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement a minimal `Mediator`. Requests implement `IRequest<TResponse>` (provided)
            and each has exactly one handler implementing `IHandler<TRequest, TResponse>`. You
            `Register` a handler and `Send(request)` — the mediator must route to the correct
            handler **by request type** and return its response. Sending an unregistered request
            throws `InvalidOperationException`.
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;

            // PROVIDED — do not modify:
            public interface IRequest<TResponse> { }
            public interface IHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
            {
                TResponse Handle(TRequest request);
            }

            public sealed class Mediator
            {
                // TODO: store handlers keyed by request type; route Send to the right one.
                public void Register<TRequest, TResponse>(IHandler<TRequest, TResponse> handler)
                    where TRequest : IRequest<TResponse>
                    => throw new NotImplementedException();

                public TResponse Send<TResponse>(IRequest<TResponse> request)
                    => throw new NotImplementedException();
            }
            """,
        HarnessCode =
            """
            using System;

            public static class __Harness
            {
                private sealed record Ping(string Text) : IRequest<string>;
                private sealed record Add(int A, int B) : IRequest<int>;

                private sealed class PingHandler : IHandler<Ping, string>
                {
                    public string Handle(Ping request) => "pong:" + request.Text;
                }
                private sealed class AddHandler : IHandler<Add, int>
                {
                    public int Handle(Add request) => request.A + request.B;
                }

                public static string Run()
                {
                    var r = new HarnessReport();
                    var m = new Mediator();
                    m.Register<Ping, string>(new PingHandler());
                    m.Register<Add, int>(new AddHandler());

                    r.Check("routes Ping to its handler", () =>
                        Assert.Equal("pong:hi", m.Send(new Ping("hi"))));
                    r.Check("routes Add to its handler by type", () =>
                        Assert.Equal(7, m.Send(new Add(3, 4))));
                    r.Check("unregistered request throws", () =>
                    {
                        bool threw = false;
                        try { new Mediator().Send(new Ping("x")); }
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
            using System.Collections.Generic;

            public interface IRequest<TResponse> { }
            public interface IHandler<TRequest, TResponse> where TRequest : IRequest<TResponse>
            {
                TResponse Handle(TRequest request);
            }

            public sealed class Mediator
            {
                // Store each handler as a type-erased invoker: it boxes the request in,
                // calls the strongly-typed Handle, and boxes the response out. This avoids
                // `dynamic` (whose runtime binder isn't available in this sandbox).
                private readonly Dictionary<Type, Func<object, object?>> _handlers = new();

                public void Register<TRequest, TResponse>(IHandler<TRequest, TResponse> handler)
                    where TRequest : IRequest<TResponse>
                    => _handlers[typeof(TRequest)] = req => handler.Handle((TRequest)req);

                public TResponse Send<TResponse>(IRequest<TResponse> request)
                {
                    var requestType = request.GetType();
                    if (!_handlers.TryGetValue(requestType, out var invoke))
                        throw new InvalidOperationException($"No handler for {requestType.Name}");

                    return (TResponse)invoke(request)!;
                }
            }
            """,
        Hints =
        [
            "Key a Dictionary<Type, object> by typeof(TRequest) in Register.",
            "In Send, look up by request.GetType() — the concrete runtime type of the request.",
            "Throw InvalidOperationException when no handler is registered for the type.",
            "Register can capture the typed handler in a `Func<object, object?>` that casts the request and calls Handle — then Send just casts the result back to TResponse.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "routes Ping to its handler", IsHidden = false },
            new TestCaseSeed { Name = "routes Add to its handler by type", IsHidden = false },
            new TestCaseSeed { Name = "unregistered request throws", IsHidden = true },
        ],
    };

    private static ExerciseSeed CachingDecorator => new()
    {
        Slug = "caching-decorator",
        Title = "Caching Decorator",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement a caching **decorator** over a repository. Both implement
            `IProductRepository` (provided) with `GetName(int id)`. `CachingRepository` wraps an
            inner repository and caches results per id: the first `GetName(id)` calls through to
            the inner repo, subsequent calls for the same id return the cached value **without**
            hitting the inner repo. Different ids each fetch once.

            The behavioral guarantee: the underlying call happens exactly **once per key**.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            // PROVIDED — do not modify:
            public interface IProductRepository { string GetName(int id); }

            public sealed class CachingRepository : IProductRepository
            {
                public CachingRepository(IProductRepository inner) => throw new System.NotImplementedException();

                // TODO: return cached value if present; else fetch from inner and cache it.
                public string GetName(int id) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            using System.Collections.Generic;

            public static class __Harness
            {
                // Counts how many times the "database" is actually hit, per id.
                private sealed class CountingRepo : IProductRepository
                {
                    public Dictionary<int, int> Hits { get; } = new();
                    public string GetName(int id)
                    {
                        Hits[id] = Hits.TryGetValue(id, out var c) ? c + 1 : 1;
                        return "product-" + id;
                    }
                }

                public static string Run()
                {
                    var r = new HarnessReport();

                    r.Check("returns correct value", () =>
                    {
                        var cache = new CachingRepository(new CountingRepo());
                        Assert.Equal("product-7", cache.GetName(7));
                    });

                    r.Check("cache hit avoids re-fetch (once per key)", () =>
                    {
                        var inner = new CountingRepo();
                        var cache = new CachingRepository(inner);
                        cache.GetName(1);
                        cache.GetName(1);
                        cache.GetName(1);
                        Assert.Equal(1, inner.Hits[1]); // fetched exactly once
                    });

                    r.Check("distinct keys each fetch once", () =>
                    {
                        var inner = new CountingRepo();
                        var cache = new CachingRepository(inner);
                        cache.GetName(1);
                        cache.GetName(2);
                        cache.GetName(1);
                        cache.GetName(2);
                        Assert.Equal(1, inner.Hits[1]);
                        Assert.Equal(1, inner.Hits[2]);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public interface IProductRepository { string GetName(int id); }

            public sealed class CachingRepository : IProductRepository
            {
                private readonly IProductRepository _inner;
                private readonly Dictionary<int, string> _cache = new();

                public CachingRepository(IProductRepository inner) => _inner = inner;

                public string GetName(int id)
                {
                    if (_cache.TryGetValue(id, out var cached))
                        return cached;                 // cache hit — no inner call
                    var value = _inner.GetName(id);    // miss — fetch once
                    _cache[id] = value;                // memoize
                    return value;
                }
            }
            """,
        Hints =
        [
            "The decorator holds the inner repo AND a Dictionary<int, string> cache.",
            "On GetName, check the cache first; return immediately on a hit.",
            "On a miss, call the inner repo exactly once, store the result, then return it.",
            "Same interface + wrapped instance + added behavior = the Decorator pattern.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "returns correct value", IsHidden = false },
            new TestCaseSeed { Name = "cache hit avoids re-fetch (once per key)", IsHidden = false },
            new TestCaseSeed { Name = "distinct keys each fetch once", IsHidden = true },
        ],
    };

    private static ExerciseSeed Pipeline => new()
    {
        Slug = "pipeline-behaviors",
        Title = "Pipeline Behaviors (Validation → Logging → Handler)",
        Difficulty = "Hard",
        Kind = "Class",
        Prompt =
            """
            Build a request pipeline: an ordered list of behaviors wrapping a handler, exactly
            like MediatR's `IPipelineBehavior`. Each `IBehavior` receives the request and a
            `next` delegate; it may run code before/after `next`, or short-circuit by NOT
            calling it.

            Implement `Pipeline.Execute(request, behaviors, handler)` that composes the
            behaviors around the handler **in order** (first behavior is outermost). Then:

            - A `ValidationBehavior` that throws if the request is invalid (never calls next).
            - A `LoggingBehavior` that appends "before"/"after" markers to a shared log around
              next.

            Order must be: validation runs first (can block), then logging wraps the handler.
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;

            // PROVIDED — do not modify:
            public interface IBehavior
            {
                // Call next() to continue the chain; skip it to short-circuit.
                int Handle(int request, Func<int> next);
            }

            public static class Pipeline
            {
                // TODO: compose behaviors[0] outermost ... handler innermost, then run it.
                public static int Execute(int request, IReadOnlyList<IBehavior> behaviors, Func<int, int> handler)
                    => throw new NotImplementedException();
            }

            // TODO: throw ArgumentException("invalid") when request < 0; else continue.
            public sealed class ValidationBehavior : IBehavior
            {
                public int Handle(int request, Func<int> next) => throw new NotImplementedException();
            }

            // TODO: log $"before:{request}" then run next, then log $"after:{result}"; return result.
            public sealed class LoggingBehavior : IBehavior
            {
                private readonly List<string> _log;
                public LoggingBehavior(List<string> log) => _log = log;
                public int Handle(int request, Func<int> next) => throw new NotImplementedException();
            }
            """,
        HarnessCode =
            """
            using System;
            using System.Collections.Generic;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();

                    r.Check("behaviors wrap handler in order", () =>
                    {
                        var log = new List<string>();
                        var behaviors = new List<IBehavior>
                        {
                            new ValidationBehavior(),
                            new LoggingBehavior(log),
                        };
                        // handler doubles the request
                        int result = Pipeline.Execute(10, behaviors, req => req * 2);
                        Assert.Equal(20, result);
                        Assert.Equal(2, log.Count);
                        Assert.Equal("before:10", log[0]);
                        Assert.Equal("after:20", log[1]);
                    });

                    r.Check("validation short-circuits before handler runs", () =>
                    {
                        var log = new List<string>();
                        bool handlerRan = false;
                        var behaviors = new List<IBehavior>
                        {
                            new ValidationBehavior(),
                            new LoggingBehavior(log),
                        };
                        bool threw = false;
                        try
                        {
                            Pipeline.Execute(-1, behaviors, req => { handlerRan = true; return req; });
                        }
                        catch (ArgumentException) { threw = true; }
                        Assert.True(threw);
                        Assert.False(handlerRan);   // handler never reached
                        Assert.Equal(0, log.Count); // logging (inner) never ran either
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Collections.Generic;

            public interface IBehavior
            {
                int Handle(int request, Func<int> next);
            }

            public static class Pipeline
            {
                public static int Execute(int request, IReadOnlyList<IBehavior> behaviors, Func<int, int> handler)
                {
                    // Innermost step: the handler itself.
                    Func<int> next = () => handler(request);

                    // Wrap from the LAST behavior inward, so behaviors[0] ends up outermost.
                    for (int i = behaviors.Count - 1; i >= 0; i--)
                    {
                        var behavior = behaviors[i];
                        var current = next;               // capture the inner chain
                        next = () => behavior.Handle(request, current);
                    }
                    return next();
                }
            }

            public sealed class ValidationBehavior : IBehavior
            {
                public int Handle(int request, Func<int> next)
                {
                    if (request < 0) throw new ArgumentException("invalid");
                    return next();
                }
            }

            public sealed class LoggingBehavior : IBehavior
            {
                private readonly List<string> _log;
                public LoggingBehavior(List<string> log) => _log = log;
                public int Handle(int request, Func<int> next)
                {
                    _log.Add($"before:{request}");
                    var result = next();
                    _log.Add($"after:{result}");
                    return result;
                }
            }
            """,
        Hints =
        [
            "Start with next = () => handler(request) as the innermost step.",
            "Fold behaviors from last to first so behaviors[0] wraps everything (outermost).",
            "Capture the current 'next' in a local before reassigning, or the closure sees the wrong delegate.",
            "ValidationBehavior throws without calling next, so nothing inside it (logging, handler) runs.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "behaviors wrap handler in order", IsHidden = false },
            new TestCaseSeed { Name = "validation short-circuits before handler runs", IsHidden = false },
        ],
    };

    // =========================================================================
    private static LessonSeed ResilienceLesson => new()
    {
        Slug = "resilience-and-flow",
        Title = "CQRS, Result & Resilience",
        Order = 3,
        MarkdownContent =
            """
            ## CQRS, Result & Resilience

            The last group covers request-flow shape (CQRS, Result), reliable side effects
            (Outbox), and surviving flaky dependencies (Circuit Breaker, Retry).

            ### CQRS — *"separate the write model from the read model"*

            **What it solves.** Reads and writes have different needs: writes enforce invariants
            and stay normalized; reads want denormalized, fast projections. CQRS splits them —
            commands mutate state and return little; queries return data and mutate nothing.
            Even at the method level, the split clarifies intent and lets the two sides evolve
            (and scale) independently.

            **Tradeoffs / when NOT to use.** Full CQRS with separate read/write databases and
            eventual consistency is a big commitment — only worth it under real read/write
            asymmetry. For a CRUD admin screen it's overkill. Start with the *conceptual* split
            (command vs query methods) before separate stores.

            **Interview framing.** "CQRS is a spectrum: at minimum, command methods don't return
            data and query methods don't mutate. The heavyweight version — separate models and
            stores with eventual consistency — is a scaling tool, not a default."

            ### Result — *"errors as values, not exceptions"*

            **What it solves.** Expected failures (validation, not-found, business-rule
            violations) aren't exceptional — modeling them as thrown exceptions hides control
            flow and is slow. `Result<T>` returns success-with-value or failure-with-error as a
            value. `Map`/`Bind` chain steps that skip on the first failure — "railway-oriented
            programming," where success stays on the happy track and a failure short-circuits.

            **Tradeoffs.** Reserve exceptions for the truly exceptional (bugs, infrastructure
            faults). Overusing Result for everything, including programmer errors, is as noisy as
            overusing exceptions. Mixing both styles inconsistently is the real trap.

            **Interview framing.** "Result makes expected failures explicit in the type, so the
            compiler and reader see them. Exceptions are for the unexpected."

            ### Outbox / domain events — *"reliably publish after commit"*

            **What it solves.** After saving an order you want to publish an `OrderPlaced` event.
            If you write to the DB and then call a broker, a crash in between loses the event
            (dual-write problem). The **Outbox** stages events in the *same transaction* as the
            data; a separate step dispatches them *after* commit. So events fire only if the data
            was durably committed — no lost or phantom events.

            **Interview framing.** "Outbox solves the dual-write problem: persist the event with
            the data atomically, then dispatch it after commit, so 'saved' and 'published' can't
            diverge."

            ### Circuit Breaker — *"stop calling a failing dependency"*

            After too many consecutive failures, trip to *Open* and fail fast instead of
            hammering a sick service — giving it room to recover. A later *half-open* probe tests
            recovery. Prevents cascading failures and thread-pool exhaustion.

            ### Retry — *"re-attempt transient faults"*

            Re-run an operation a bounded number of times for transient errors (timeouts, blips),
            usually with backoff. Pair with a circuit breaker (retry the blip, break on the
            outage) and only retry *idempotent* operations. In .NET, reach for Polly / the
            `Microsoft.Extensions.Http.Resilience` stack rather than hand-rolling these.
            """,
        Exercises =
        [
            Cqrs,
            ResultPattern,
            OutboxDispatch,
            CircuitBreaker,
            RetryPolicy,
        ],
    };

    private static ExerciseSeed Cqrs => new()
    {
        Slug = "cqrs",
        Title = "CQRS (Command/Query Separation)",
        Difficulty = "Easy",
        Kind = "Class",
        Prompt =
            """
            Implement a `CounterService` with separate write and read paths: `Execute` on an
            `IncrementCommand` mutates the count (returns nothing meaningful), and `Execute`
            on a `GetCountQuery` returns the current count without mutating. Commands change
            state; queries don't.
            """,
        StarterCode =
            """
            // PROVIDED — do not modify:
            public record IncrementCommand(int By);
            public record GetCountQuery();

            public sealed class CounterService
            {
                // TODO: the command mutates; the query reads.
                public void Execute(IncrementCommand command) => throw new System.NotImplementedException();
                public int Execute(GetCountQuery query) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("commands mutate, query reads", () =>
                    {
                        var svc = new CounterService();
                        svc.Execute(new IncrementCommand(5));
                        svc.Execute(new IncrementCommand(3));
                        Assert.Equal(8, svc.Execute(new GetCountQuery()));
                    });
                    r.Check("query does not mutate", () =>
                    {
                        var svc = new CounterService();
                        svc.Execute(new IncrementCommand(2));
                        svc.Execute(new GetCountQuery());
                        Assert.Equal(2, svc.Execute(new GetCountQuery())); // still 2
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public record IncrementCommand(int By);
            public record GetCountQuery();

            public sealed class CounterService
            {
                private int _count;

                // Write side.
                public void Execute(IncrementCommand command) => _count += command.By;

                // Read side — no mutation.
                public int Execute(GetCountQuery query) => _count;
            }
            """,
        Hints =
        [
            "Hold the count in a private field.",
            "The command overload adds command.By to it.",
            "The query overload just returns it — never changes state.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "commands mutate, query reads", IsHidden = false },
            new TestCaseSeed { Name = "query does not mutate", IsHidden = false },
        ],
    };

    private static ExerciseSeed ResultPattern => new()
    {
        Slug = "result-pattern",
        Title = "Result<T> (Railway-Oriented)",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement a `Result<T>` that represents success (with a value) or failure (with an
            error message), plus `Map` that transforms the value **only on success** and
            passes failures through untouched. This models errors as values instead of throwing.
            """,
        StarterCode =
            """
            using System;

            public sealed class Result<T>
            {
                public bool IsSuccess { get; }
                public T? Value { get; }
                public string? Error { get; }

                private Result(bool ok, T? value, string? error)
                {
                    IsSuccess = ok; Value = value; Error = error;
                }

                public static Result<T> Ok(T value) => throw new NotImplementedException();
                public static Result<T> Fail(string error) => throw new NotImplementedException();

                // TODO: map the value if success; propagate the error otherwise.
                public Result<U> Map<U>(Func<T, U> f) => throw new NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("map transforms a success", () =>
                    {
                        var result = Result<int>.Ok(5).Map(x => x * 2);
                        Assert.True(result.IsSuccess);
                        Assert.Equal(10, result.Value);
                    });
                    r.Check("map propagates a failure", () =>
                    {
                        var result = Result<int>.Fail("bad input").Map(x => x * 2);
                        Assert.False(result.IsSuccess);
                        Assert.Equal("bad input", result.Error);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public sealed class Result<T>
            {
                public bool IsSuccess { get; }
                public T? Value { get; }
                public string? Error { get; }

                private Result(bool ok, T? value, string? error)
                {
                    IsSuccess = ok; Value = value; Error = error;
                }

                public static Result<T> Ok(T value) => new(true, value, null);
                public static Result<T> Fail(string error) => new(false, default, error);

                public Result<U> Map<U>(Func<T, U> f) =>
                    IsSuccess ? Result<U>.Ok(f(Value!)) : Result<U>.Fail(Error!);
            }
            """,
        Hints =
        [
            "Ok stores the value with IsSuccess = true; Fail stores the error.",
            "Map: if success, apply f and wrap in Result<U>.Ok.",
            "If failure, return Result<U>.Fail with the same error — don't call f.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "map transforms a success", IsHidden = false },
            new TestCaseSeed { Name = "map propagates a failure", IsHidden = false },
        ],
    };

    private static ExerciseSeed OutboxDispatch => new()
    {
        Slug = "outbox-dispatch",
        Title = "Outbox — Dispatch Domain Events After Commit",
        Difficulty = "Hard",
        Kind = "Class",
        Prompt =
            """
            Model the Outbox pattern's core guarantee: **events are dispatched only after a
            successful commit, never before, and never lost or duplicated.**

            Implement `OutboxUnitOfWork`:
            - `AddEvent(name)` stages a domain event (does NOT dispatch it).
            - `Commit(shouldSucceed)` — if `shouldSucceed` is false, the "transaction" fails:
              staged events are discarded and NOTHING is dispatched (returns false). If true, it
              commits, then dispatches every staged event *in order* to the provided
              `dispatched` sink, clears the outbox, and returns true.
            - Events staged before a failed commit must never reach the sink.

            The behavioral guarantee: dispatch happens strictly *after* a successful commit.
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;

            public sealed class OutboxUnitOfWork
            {
                // The sink receives event names in dispatch order, only after a good commit.
                public OutboxUnitOfWork(Action<string> dispatch) => throw new NotImplementedException();

                public void AddEvent(string name) => throw new NotImplementedException();
                public int PendingCount => throw new NotImplementedException();

                // shouldSucceed==false => rollback, dispatch nothing, return false.
                // shouldSucceed==true  => commit, then dispatch all staged events in order, clear, return true.
                public bool Commit(bool shouldSucceed) => throw new NotImplementedException();
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

                    r.Check("dispatches staged events after a successful commit", () =>
                    {
                        var dispatched = new List<string>();
                        var uow = new OutboxUnitOfWork(dispatched.Add);
                        uow.AddEvent("OrderPlaced");
                        uow.AddEvent("StockReserved");
                        Assert.Equal(0, dispatched.Count);   // nothing yet — before commit
                        Assert.True(uow.Commit(shouldSucceed: true));
                        Assert.Equal(2, dispatched.Count);
                        Assert.Equal("OrderPlaced", dispatched[0]);
                        Assert.Equal("StockReserved", dispatched[1]);
                        Assert.Equal(0, uow.PendingCount);   // outbox drained
                    });

                    r.Check("failed commit dispatches nothing (no phantom events)", () =>
                    {
                        var dispatched = new List<string>();
                        var uow = new OutboxUnitOfWork(dispatched.Add);
                        uow.AddEvent("OrderPlaced");
                        Assert.False(uow.Commit(shouldSucceed: false));
                        Assert.Equal(0, dispatched.Count);   // rollback -> nothing published
                        Assert.Equal(0, uow.PendingCount);   // discarded
                    });

                    r.Check("events survive only through the commit that dispatches them", () =>
                    {
                        var dispatched = new List<string>();
                        var uow = new OutboxUnitOfWork(dispatched.Add);
                        uow.AddEvent("A");
                        uow.Commit(shouldSucceed: false);    // A discarded
                        uow.AddEvent("B");
                        uow.Commit(shouldSucceed: true);     // only B dispatched
                        Assert.Equal(1, dispatched.Count);
                        Assert.Equal("B", dispatched[0]);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Collections.Generic;

            public sealed class OutboxUnitOfWork
            {
                private readonly Action<string> _dispatch;
                private readonly List<string> _outbox = new();

                public OutboxUnitOfWork(Action<string> dispatch) => _dispatch = dispatch;

                public void AddEvent(string name) => _outbox.Add(name); // staged, not dispatched
                public int PendingCount => _outbox.Count;

                public bool Commit(bool shouldSucceed)
                {
                    if (!shouldSucceed)
                    {
                        _outbox.Clear();   // rollback: drop staged events, publish nothing
                        return false;
                    }

                    // Commit succeeded -> now (and only now) dispatch, in order.
                    foreach (var name in _outbox)
                        _dispatch(name);
                    _outbox.Clear();
                    return true;
                }
            }
            """,
        Hints =
        [
            "AddEvent only stages into a list — never dispatch there.",
            "On a failed commit, clear the outbox and return false without touching the sink.",
            "On success, loop the staged events IN ORDER, call dispatch, then clear.",
            "This ordering — dispatch strictly after commit — is what prevents lost/phantom events.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "dispatches staged events after a successful commit", IsHidden = false },
            new TestCaseSeed { Name = "failed commit dispatches nothing (no phantom events)", IsHidden = false },
            new TestCaseSeed { Name = "events survive only through the commit that dispatches them", IsHidden = true },
        ],
    };

    private static ExerciseSeed CircuitBreaker => new()
    {
        Slug = "circuit-breaker",
        Title = "Circuit Breaker",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement a `CircuitBreaker` that trips **Open** after `failureThreshold`
            consecutive failures. While Closed, `Allow()` returns true; once Open, it returns
            false (fail fast). A success while Closed resets the failure count. (We omit the
            timed half-open state to keep it deterministic.)
            """,
        StarterCode =
            """
            public sealed class CircuitBreaker
            {
                public CircuitBreaker(int failureThreshold) => throw new System.NotImplementedException();

                public string State => throw new System.NotImplementedException(); // "Closed" or "Open"
                public bool Allow() => throw new System.NotImplementedException();
                public void RecordSuccess() => throw new System.NotImplementedException();
                public void RecordFailure() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("trips open after threshold failures", () =>
                    {
                        var cb = new CircuitBreaker(3);
                        Assert.True(cb.Allow());
                        cb.RecordFailure(); cb.RecordFailure(); cb.RecordFailure();
                        Assert.Equal("Open", cb.State);
                        Assert.False(cb.Allow());
                    });
                    r.Check("success resets the failure count", () =>
                    {
                        var cb = new CircuitBreaker(3);
                        cb.RecordFailure(); cb.RecordFailure();
                        cb.RecordSuccess();       // reset
                        cb.RecordFailure(); cb.RecordFailure();
                        Assert.Equal("Closed", cb.State); // only 2 in a row since reset
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public sealed class CircuitBreaker
            {
                private readonly int _threshold;
                private int _consecutiveFailures;
                private bool _open;

                public CircuitBreaker(int failureThreshold) => _threshold = failureThreshold;

                public string State => _open ? "Open" : "Closed";
                public bool Allow() => !_open;

                public void RecordSuccess()
                {
                    _consecutiveFailures = 0; // a good call clears the streak
                }

                public void RecordFailure()
                {
                    _consecutiveFailures++;
                    if (_consecutiveFailures >= _threshold)
                        _open = true; // trip
                }
            }
            """,
        Hints =
        [
            "Track consecutive failures and an open flag.",
            "RecordFailure increments the count and trips Open at the threshold.",
            "RecordSuccess resets the count to 0.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "trips open after threshold failures", IsHidden = false },
            new TestCaseSeed { Name = "success resets the failure count", IsHidden = false },
        ],
    };

    private static ExerciseSeed RetryPolicy => new()
    {
        Slug = "retry-policy",
        Title = "Retry Policy",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement a `RetryPolicy(maxAttempts)` whose `Execute(operation)` runs the
            operation, retrying on exception up to `maxAttempts` total attempts. Return the
            result on success; rethrow the last exception if every attempt fails.
            """,
        StarterCode =
            """
            using System;

            public sealed class RetryPolicy
            {
                public RetryPolicy(int maxAttempts) => throw new NotImplementedException();

                // TODO: try up to maxAttempts; return on success, rethrow after the last failure.
                public int Execute(Func<int> operation) => throw new NotImplementedException();
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
                    r.Check("succeeds after transient failures", () =>
                    {
                        int calls = 0;
                        Func<int> op = () =>
                        {
                            calls++;
                            if (calls < 3) throw new InvalidOperationException("transient");
                            return 42;
                        };
                        Assert.Equal(42, new RetryPolicy(5).Execute(op));
                    });
                    r.Check("rethrows after exhausting attempts", () =>
                    {
                        Func<int> op = () => throw new InvalidOperationException("always");
                        bool threw = false;
                        try { new RetryPolicy(3).Execute(op); }
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

            public sealed class RetryPolicy
            {
                private readonly int _maxAttempts;
                public RetryPolicy(int maxAttempts) => _maxAttempts = maxAttempts;

                public int Execute(Func<int> operation)
                {
                    for (int attempt = 1; ; attempt++)
                    {
                        try { return operation(); }
                        catch when (attempt < _maxAttempts) { /* retry */ }
                    }
                }
            }
            """,
        Hints =
        [
            "Loop up to maxAttempts, calling the operation in a try.",
            "Return immediately on success.",
            "An exception filter `catch when (attempt < _maxAttempts)` lets the final failure propagate.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "succeeds after transient failures", IsHidden = false },
            new TestCaseSeed { Name = "rethrows after exhausting attempts", IsHidden = false },
        ],
    };
}
