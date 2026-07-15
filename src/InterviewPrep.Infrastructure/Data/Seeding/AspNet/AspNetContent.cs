namespace InterviewPrep.Infrastructure.Data.Seeding.AspNet;

// The "ASP.NET Core Internals" topic. Rather than requiring the ASP.NET framework
// assemblies in the sandbox, these exercises have the learner BUILD the core
// mechanisms themselves — the middleware pipeline, a DI container with lifetimes,
// route-template matching, and model validation. This is exactly how interviews
// probe "how does X actually work under the hood?".
internal static partial class AspNetContent
{
    public static TopicSeed Topic => new()
    {
        Slug = "aspnet-core",
        Name = "ASP.NET Core Internals",
        Description = "Build the mechanisms yourself: the middleware pipeline, DI lifetimes, routing, and validation.",
        Order = 7,
        Lessons =
        [
            InternalsLesson,
        ],
    };

    private static LessonSeed InternalsLesson => new()
    {
        Slug = "aspnet-internals",
        Title = "How ASP.NET Core Works",
        Order = 1,
        MarkdownContent =
            """
            ## ASP.NET Core Internals

            The framework is less magic than it looks. The essentials interviewers ask about:
            - **Middleware pipeline** — an onion: each middleware runs code, calls `next()`,
              then runs code on the way back out. Order matters.
            - **Dependency Injection lifetimes** — *transient* (new every resolve),
              *scoped* (one per request), *singleton* (one forever). The classic bug is
              capturing a scoped service in a singleton.
            - **Routing** — match a URL against templates like `users/{id}` and extract values.
            - **Model validation** — reject bad input before it reaches your logic.

            You'll implement small versions of each.
            """,
        Exercises =
        [
            MiddlewarePipeline,
            DiLifetimes,
            RouteMatching,
            ModelValidation,
        ],
    };

    private static ExerciseSeed MiddlewarePipeline => new()
    {
        Slug = "middleware-pipeline",
        Title = "Build the Middleware Pipeline",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Implement `Run(middlewares, ctx)` that executes an ASP.NET-style pipeline.
            Each middleware is `Action<Context, Action>` — it receives the context and
            `next` (the rest of the pipeline). It may log, call `next()`, then log again.
            Correct nesting produces an **onion order**: `A-in, B-in, B-out, A-out`.
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;

            // PROVIDED — do not modify:
            public sealed class Context { public List<string> Log { get; } = new(); }

            public static class Solution
            {
                // TODO: chain the middlewares so each wraps the next (build from the end).
                public static void Run(List<Action<Context, Action>> middlewares, Context ctx)
                {
                    throw new NotImplementedException();
                }
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
                    r.Check("middlewares nest in onion order", () =>
                    {
                        var ctx = new Context();
                        var pipeline = new List<Action<Context, Action>>
                        {
                            (c, next) => { c.Log.Add("A-in"); next(); c.Log.Add("A-out"); },
                            (c, next) => { c.Log.Add("B-in"); next(); c.Log.Add("B-out"); },
                        };
                        Solution.Run(pipeline, ctx);
                        Assert.Equal("A-in,B-in,B-out,A-out", string.Join(",", ctx.Log));
                    });
                    r.Check("empty pipeline does nothing", () =>
                    {
                        var ctx = new Context();
                        Solution.Run(new List<Action<Context, Action>>(), ctx);
                        Assert.Equal(0, ctx.Log.Count);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Collections.Generic;

            public sealed class Context { public List<string> Log { get; } = new(); }

            public static class Solution
            {
                public static void Run(List<Action<Context, Action>> middlewares, Context ctx)
                {
                    // Build the chain back-to-front: the last middleware's `next` is a no-op,
                    // each earlier one wraps the chain built so far.
                    Action next = () => { };
                    for (int i = middlewares.Count - 1; i >= 0; i--)
                    {
                        var middleware = middlewares[i];
                        var localNext = next;          // capture the current tail
                        next = () => middleware(ctx, localNext);
                    }
                    next(); // kick off the pipeline
                }
            }
            """,
        Hints =
        [
            "Each middleware needs a `next` delegate that runs everything after it.",
            "Build the chain from the LAST middleware backwards; the tail's next is a no-op.",
            "Watch the closure: capture the current `next` in a local before reassigning it.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "middlewares nest in onion order", IsHidden = false },
            new TestCaseSeed { Name = "empty pipeline does nothing", IsHidden = true },
        ],
    };

    private static ExerciseSeed DiLifetimes => new()
    {
        Slug = "di-lifetimes",
        Title = "Mini DI Container (Lifetimes)",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement a tiny `Container` with `RegisterSingleton<T>` and
            `RegisterTransient<T>` (each taking a factory) plus `Resolve<T>()`. A
            **singleton** returns the same instance every resolve; a **transient**
            returns a fresh one each time — the core of DI lifetimes.
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;

            public sealed class Container
            {
                // TODO: store registrations by type; honour singleton vs transient on Resolve.
                public void RegisterSingleton<T>(Func<T> factory) => throw new NotImplementedException();
                public void RegisterTransient<T>(Func<T> factory) => throw new NotImplementedException();
                public T Resolve<T>() => throw new NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("transient resolves a new instance each time", () =>
                    {
                        var c = new Container();
                        c.RegisterTransient(() => new object());
                        Assert.False(ReferenceEquals(c.Resolve<object>(), c.Resolve<object>()));
                    });
                    r.Check("singleton resolves the same instance", () =>
                    {
                        var c = new Container();
                        c.RegisterSingleton(() => new object());
                        Assert.True(ReferenceEquals(c.Resolve<object>(), c.Resolve<object>()));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Collections.Generic;

            public sealed class Container
            {
                // For each type: the factory + whether to cache (singleton) + the cached value.
                private sealed record Registration(Func<object> Factory, bool Singleton)
                {
                    public object? Cached { get; set; }
                }

                private readonly Dictionary<Type, Registration> _map = new();

                public void RegisterSingleton<T>(Func<T> factory) =>
                    _map[typeof(T)] = new Registration(() => factory()!, Singleton: true);

                public void RegisterTransient<T>(Func<T> factory) =>
                    _map[typeof(T)] = new Registration(() => factory()!, Singleton: false);

                public T Resolve<T>()
                {
                    var reg = _map[typeof(T)];
                    if (!reg.Singleton) return (T)reg.Factory(); // fresh every time
                    return (T)(reg.Cached ??= reg.Factory());     // build once, then reuse
                }
            }
            """,
        Hints =
        [
            "Map each registered Type to its factory and a 'is singleton' flag.",
            "Transient: call the factory on every Resolve.",
            "Singleton: build once, cache it, and return the cached instance thereafter.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "transient resolves a new instance each time", IsHidden = false },
            new TestCaseSeed { Name = "singleton resolves the same instance", IsHidden = false },
        ],
    };

    private static ExerciseSeed RouteMatching => new()
    {
        Slug = "route-matching",
        Title = "Route Template Matching",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Implement `Match(template, path)`: given a template like `"users/{id}"` and a
            path like `"users/42"`, return a dictionary of route values (`id -> "42"`), or
            `null` if the path doesn't match. Segments count must match; literal segments
            must be equal; `{name}` captures whatever is in that position.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: split both on '/', compare segment by segment, capture {params}.
                public static Dictionary<string, string>? Match(string template, string path)
                {
                    return null;
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
                    r.Check("captures a single param", () =>
                    {
                        var m = Solution.Match("users/{id}", "users/42");
                        Assert.True(m != null && m["id"] == "42");
                    });
                    r.Check("captures multiple params", () =>
                    {
                        var m = Solution.Match("orders/{oid}/items/{iid}", "orders/7/items/9");
                        Assert.True(m != null && m["oid"] == "7" && m["iid"] == "9");
                    });
                    r.Check("literal mismatch -> null", () =>
                        Assert.True(Solution.Match("users/{id}", "posts/42") == null));
                    r.Check("segment count mismatch -> null", () =>
                        Assert.True(Solution.Match("users/{id}", "users/42/extra") == null));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                public static Dictionary<string, string>? Match(string template, string path)
                {
                    var t = template.Split('/');
                    var p = path.Split('/');
                    if (t.Length != p.Length) return null; // different segment counts never match

                    var values = new Dictionary<string, string>();
                    for (int i = 0; i < t.Length; i++)
                    {
                        if (t[i].StartsWith('{') && t[i].EndsWith('}'))
                            values[t[i][1..^1]] = p[i]; // capture: strip the braces for the key
                        else if (t[i] != p[i])
                            return null;                 // literal segment must match exactly
                    }
                    return values;
                }
            }
            """,
        Hints =
        [
            "Split both strings on '/'; if the counts differ, it can't match.",
            "For each segment: a `{name}` template captures the path value under `name`.",
            "A literal template segment must equal the path segment, else return null.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "captures a single param", IsHidden = false },
            new TestCaseSeed { Name = "captures multiple params", IsHidden = false },
            new TestCaseSeed { Name = "literal mismatch -> null", IsHidden = false },
            new TestCaseSeed { Name = "segment count mismatch -> null", IsHidden = true },
        ],
    };

    private static ExerciseSeed ModelValidation => new()
    {
        Slug = "model-validation",
        Title = "Model Validation",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Implement `Validate(name, age)` returning a list of error messages: `"Name is
            required"` if name is null/empty/whitespace, and `"Age out of range"` if age
            isn't in 0..120. Valid input returns an empty list. This is what
            `[Required]`/`[Range]` do before your action runs.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: collect and return validation errors (empty list = valid).
                public static List<string> Validate(string name, int age)
                {
                    return new List<string>();
                }
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
                    r.Check("valid input -> no errors", () =>
                        Assert.Equal(0, Solution.Validate("Ada", 30).Count));
                    r.Check("blank name -> name error", () =>
                        Assert.True(Solution.Validate("  ", 30).Contains("Name is required")));
                    r.Check("age 200 -> range error", () =>
                        Assert.True(Solution.Validate("Ada", 200).Contains("Age out of range")));
                    r.Check("both invalid -> two errors", () =>
                        Assert.Equal(2, Solution.Validate("", -1).Count));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                public static List<string> Validate(string name, int age)
                {
                    var errors = new List<string>();
                    if (string.IsNullOrWhiteSpace(name)) errors.Add("Name is required");
                    if (age < 0 || age > 120) errors.Add("Age out of range");
                    return errors;
                }
            }
            """,
        Hints =
        [
            "Use string.IsNullOrWhiteSpace for the name check.",
            "Range check: age < 0 or age > 120 is invalid.",
            "Accumulate messages in a list and return it (empty means valid).",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "valid input -> no errors", IsHidden = false },
            new TestCaseSeed { Name = "blank name -> name error", IsHidden = false },
            new TestCaseSeed { Name = "age 200 -> range error", IsHidden = false },
            new TestCaseSeed { Name = "both invalid -> two errors", IsHidden = true },
        ],
    };
}
