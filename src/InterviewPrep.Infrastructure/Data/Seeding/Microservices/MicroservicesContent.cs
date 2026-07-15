namespace InterviewPrep.Infrastructure.Data.Seeding.Microservices;

// The "Microservices" topic — the coordination patterns that make distributed systems
// work: Saga (distributed transactions with compensation), the Outbox (reliable event
// publishing), an API Gateway (single entry point / routing), and Service Registry
// (discovery + load balancing). Implemented as plain C#.
internal static class MicroservicesContent
{
    public static TopicSeed Topic => new()
    {
        Slug = "microservices",
        Name = "Microservices",
        Description = "Saga transactions, the Outbox pattern, API Gateway routing, and service discovery — the glue of distributed systems.",
        Order = 17,
        Lessons =
        [
            new LessonSeed
            {
                Slug = "microservices-patterns", Title = "Coordination Patterns", Order = 1,
                MarkdownContent =
                    """
                    ## Coordination Patterns

                    Distributed systems can't use one big transaction, so:
                    - **Saga** — a sequence of local steps, each with a **compensating** action to
                      undo it if a later step fails (eventual consistency).
                    - **Outbox** — write events in the same transaction as your data, then publish
                      them reliably, so you never "saved the row but lost the event".
                    - **API Gateway** — one entry point that routes requests to the right service.
                    - **Service Registry** — services register themselves; clients discover and
                      load-balance across instances.
                    """,
                Exercises = [Saga, Outbox, ApiGateway, ServiceRegistry],
            },
        ],
    };

    private static ExerciseSeed Saga => new()
    {
        Slug = "saga-orchestrator",
        Title = "Saga (with Compensation)",
        Difficulty = "Hard",
        Kind = "Class",
        Prompt =
            """
            Implement a `Saga`: `AddStep(action, compensate)` queues a step, and `Execute()`
            runs steps in order. If a step's `action` returns false (fails), run the
            **compensations for the already-completed steps in reverse order**, then return
            false. If all steps succeed, return true.
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;

            public sealed class Saga
            {
                public void AddStep(Func<bool> action, Action compensate) => throw new NotImplementedException();

                // TODO: run steps; on failure, compensate completed steps in reverse.
                public bool Execute() => throw new NotImplementedException();
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

                    r.Check("all succeed -> true, no compensation", () =>
                    {
                        var log = new List<string>();
                        var saga = new Saga();
                        saga.AddStep(() => { log.Add("A"); return true; }, () => log.Add("~A"));
                        saga.AddStep(() => { log.Add("B"); return true; }, () => log.Add("~B"));
                        Assert.True(saga.Execute());
                        Assert.Equal("A,B", string.Join(",", log));
                    });

                    r.Check("middle step fails -> compensate completed, in reverse", () =>
                    {
                        var log = new List<string>();
                        var saga = new Saga();
                        saga.AddStep(() => { log.Add("A"); return true; }, () => log.Add("~A"));
                        saga.AddStep(() => { log.Add("B"); return true; }, () => log.Add("~B"));
                        saga.AddStep(() => { log.Add("C"); return false; }, () => log.Add("~C")); // fails
                        Assert.False(saga.Execute());
                        // C failed (no ~C), then undo B then A.
                        Assert.Equal("A,B,C,~B,~A", string.Join(",", log));
                    });

                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Collections.Generic;

            public sealed class Saga
            {
                private readonly List<(Func<bool> action, Action compensate)> _steps = new();

                public void AddStep(Func<bool> action, Action compensate) => _steps.Add((action, compensate));

                public bool Execute()
                {
                    var completed = new Stack<Action>(); // compensations, newest on top
                    foreach (var (action, compensate) in _steps)
                    {
                        if (!action())
                        {
                            while (completed.Count > 0) completed.Pop()(); // undo in reverse
                            return false;
                        }
                        completed.Push(compensate);
                    }
                    return true;
                }
            }
            """,
        Hints =
        [
            "Track the compensations of completed steps on a stack.",
            "If a step's action returns false, pop and run each compensation (reverse order).",
            "The failing step itself isn't compensated (its action didn't complete).",
        ],
        TestCases =
        [
            new() { Name = "all succeed -> true, no compensation", IsHidden = false },
            new() { Name = "middle step fails -> compensate completed, in reverse", IsHidden = false },
        ],
    };

    private static ExerciseSeed Outbox => new()
    {
        Slug = "outbox-pattern",
        Title = "Outbox Pattern",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement an `Outbox`: `Add(evt)` stores an event as unpublished, `GetUnpublished()`
            returns the ones not yet sent, and `MarkPublished(evt)` marks one done. A background
            publisher drains unpublished events — guaranteeing they're delivered at least once.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public sealed class Outbox
            {
                public void Add(string evt) => throw new System.NotImplementedException();
                public string[] GetUnpublished() => throw new System.NotImplementedException();
                public void MarkPublished(string evt) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("tracks unpublished, then drains", () =>
                    {
                        var outbox = new Outbox();
                        outbox.Add("OrderPlaced");
                        outbox.Add("EmailQueued");
                        Assert.Equal(2, outbox.GetUnpublished().Length);
                        outbox.MarkPublished("OrderPlaced");
                        Assert.Equal("EmailQueued", string.Join(",", outbox.GetUnpublished()));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;
            using System.Linq;

            public sealed class Outbox
            {
                // event -> published?  (insertion order preserved for stable draining)
                private readonly List<string> _events = new();
                private readonly HashSet<string> _published = new();

                public void Add(string evt) => _events.Add(evt);

                public string[] GetUnpublished() =>
                    _events.Where(e => !_published.Contains(e)).ToArray();

                public void MarkPublished(string evt) => _published.Add(evt);
            }
            """,
        Hints =
        [
            "Keep the events in order and track which are published.",
            "GetUnpublished filters out the published ones.",
            "MarkPublished records that one was sent.",
        ],
        TestCases = [new() { Name = "tracks unpublished, then drains", IsHidden = false }],
    };

    private static ExerciseSeed ApiGateway => new()
    {
        Slug = "api-gateway-router",
        Title = "API Gateway Routing",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement an `ApiGateway`: `Route(prefix, service)` registers a path prefix →
            backend service, and `Resolve(path)` returns the service for the **longest matching
            prefix**, or `null` if none match. This is how a gateway forwards requests.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public sealed class ApiGateway
            {
                public void Route(string prefix, string service) => throw new System.NotImplementedException();

                // TODO: return the service for the longest matching prefix, else null.
                public string? Resolve(string path) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    var gw = new ApiGateway();
                    gw.Route("/users", "user-svc");
                    gw.Route("/users/admin", "admin-svc");
                    gw.Route("/orders", "order-svc");

                    r.Check("routes by prefix", () => Assert.Equal("user-svc", gw.Resolve("/users/42")));
                    r.Check("longest prefix wins", () => Assert.Equal("admin-svc", gw.Resolve("/users/admin/9")));
                    r.Check("no match -> null", () => Assert.True(gw.Resolve("/nope") == null));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;
            using System.Linq;

            public sealed class ApiGateway
            {
                private readonly Dictionary<string, string> _routes = new();

                public void Route(string prefix, string service) => _routes[prefix] = service;

                public string? Resolve(string path) =>
                    _routes.Keys
                        .Where(prefix => path.StartsWith(prefix))
                        .OrderByDescending(prefix => prefix.Length) // longest match first
                        .Select(prefix => _routes[prefix])
                        .FirstOrDefault();
            }
            """,
        Hints =
        [
            "Store prefix -> service in a dictionary.",
            "Resolve: keep prefixes the path starts with, pick the longest.",
            "Return null when nothing matches.",
        ],
        TestCases =
        [
            new() { Name = "routes by prefix", IsHidden = false },
            new() { Name = "longest prefix wins", IsHidden = false },
            new() { Name = "no match -> null", IsHidden = true },
        ],
    };

    private static ExerciseSeed ServiceRegistry => new()
    {
        Slug = "service-registry",
        Title = "Service Registry (Discovery)",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement a `ServiceRegistry`: `Register(name, instance)` adds an instance URL for a
            service, and `Discover(name)` returns instances **round-robin** (spreading load), or
            `null` if the service is unknown.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public sealed class ServiceRegistry
            {
                public void Register(string name, string instance) => throw new System.NotImplementedException();

                // TODO: round-robin across a service's instances; null if unknown.
                public string? Discover(string name) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    var reg = new ServiceRegistry();
                    reg.Register("user-svc", "10.0.0.1");
                    reg.Register("user-svc", "10.0.0.2");

                    r.Check("round-robins instances", () =>
                    {
                        Assert.Equal("10.0.0.1", reg.Discover("user-svc"));
                        Assert.Equal("10.0.0.2", reg.Discover("user-svc"));
                        Assert.Equal("10.0.0.1", reg.Discover("user-svc")); // wraps
                    });
                    r.Check("unknown service -> null", () => Assert.True(reg.Discover("nope") == null));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public sealed class ServiceRegistry
            {
                private readonly Dictionary<string, List<string>> _instances = new();
                private readonly Dictionary<string, int> _next = new();

                public void Register(string name, string instance)
                {
                    if (!_instances.TryGetValue(name, out var list))
                        _instances[name] = list = new List<string>();
                    list.Add(instance);
                }

                public string? Discover(string name)
                {
                    if (!_instances.TryGetValue(name, out var list) || list.Count == 0) return null;
                    int i = _next.GetValueOrDefault(name);
                    _next[name] = (i + 1) % list.Count; // advance, wrapping
                    return list[i];
                }
            }
            """,
        Hints =
        [
            "Map each service name to a list of instances.",
            "Track a per-service index and advance it (mod count) each Discover.",
            "Return null when the name isn't registered.",
        ],
        TestCases =
        [
            new() { Name = "round-robins instances", IsHidden = false },
            new() { Name = "unknown service -> null", IsHidden = false },
        ],
    };
}
