namespace InterviewPrep.Infrastructure.Data.Seeding.Testing;

// The "Unit Testing" topic — writing testable code and test doubles. Each exercise
// is graded as C#: you implement code that a good test suite would drive (pure
// functions, injected dependencies, fakes/spies), reinforcing the habits behind the
// AAA pattern and the test pyramid.
internal static class TestingContent
{
    public static TopicSeed Topic => new()
    {
        Slug = "unit-testing",
        Name = "Unit Testing",
        Description = "Testable design and test doubles: pure functions, injected clocks, fakes, spies, and boundary cases.",
        Order = 16,
        Lessons =
        [
            new LessonSeed
            {
                Slug = "testable-code", Title = "Writing Testable Code", Order = 1,
                MarkdownContent =
                    """
                    ## Writing Testable Code

                    Good tests follow **Arrange–Act–Assert**: set up inputs, call the code, check
                    the result. Code is easy to test when it's **deterministic** and its
                    dependencies are **injected** (so tests can pass fakes). The **test pyramid**
                    says: many fast unit tests, fewer integration tests, fewest end-to-end.
                    """,
                Exercises = [FizzBuzz, TestableClock, BoundaryValidation],
            },
            new LessonSeed
            {
                Slug = "test-doubles", Title = "Test Doubles", Order = 2,
                MarkdownContent =
                    """
                    ## Test Doubles

                    Real dependencies (databases, email, time) make tests slow and flaky, so we
                    swap in **doubles**: a **fake** is a working lightweight implementation (an
                    in-memory repo), a **stub** returns canned data, and a **spy/mock** records how
                    it was called so you can assert on interactions.
                    """,
                Exercises = [FakeRepository, SpyLogger],
            },
        ],
    };

    private static ExerciseSeed FizzBuzz => new()
    {
        Slug = "fizzbuzz",
        Title = "FizzBuzz",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            The classic warm-up. Return `"Fizz"` if `n` is divisible by 3, `"Buzz"` if by 5,
            `"FizzBuzz"` if by both, otherwise the number as a string. Think about the test
            cases you'd write: 3, 5, 15, and a plain number.
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: Fizz / Buzz / FizzBuzz / the number.
                public static string FizzBuzz(int n)
                {
                    return "";
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
                    r.Check("3 -> Fizz", () => Assert.Equal("Fizz", Solution.FizzBuzz(3)));
                    r.Check("5 -> Buzz", () => Assert.Equal("Buzz", Solution.FizzBuzz(5)));
                    r.Check("15 -> FizzBuzz", () => Assert.Equal("FizzBuzz", Solution.FizzBuzz(15)));
                    r.Check("7 -> \"7\"", () => Assert.Equal("7", Solution.FizzBuzz(7)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static string FizzBuzz(int n)
                {
                    if (n % 15 == 0) return "FizzBuzz"; // check both first
                    if (n % 3 == 0) return "Fizz";
                    if (n % 5 == 0) return "Buzz";
                    return n.ToString();
                }
            }
            """,
        Hints =
        [
            "Check divisibility by BOTH (15) first, or you'll never reach FizzBuzz.",
            "Then check 3, then 5.",
            "Otherwise return n.ToString().",
        ],
        TestCases =
        [
            new() { Name = "3 -> Fizz", IsHidden = false },
            new() { Name = "15 -> FizzBuzz", IsHidden = false },
        ],
    };

    private static ExerciseSeed TestableClock => new()
    {
        Slug = "testable-clock",
        Title = "Inject the Clock",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Code that calls `DateTime.Now` is hard to test (the answer changes every run).
            Instead, **inject** an `IClock` (provided). Implement `IsExpired(expiresAt, clock)`
            using `clock.Now` so a test can pass a fixed time.
            """,
        StarterCode =
            """
            using System;

            // PROVIDED — do not modify:
            public interface IClock { DateTime Now { get; } }

            public static class Solution
            {
                // TODO: expired when the injected clock is at/after expiresAt.
                public static bool IsExpired(DateTime expiresAt, IClock clock)
                {
                    return false;
                }
            }
            """,
        HarnessCode =
            """
            using System;

            public static class __Harness
            {
                // A fake clock frozen at a known time — exactly what makes this testable.
                private sealed class FixedClock : IClock
                {
                    public DateTime Now { get; }
                    public FixedClock(DateTime now) => Now = now;
                }

                public static string Run()
                {
                    var r = new HarnessReport();
                    var expiry = new DateTime(2030, 1, 1);
                    r.Check("before expiry -> not expired", () =>
                        Assert.False(Solution.IsExpired(expiry, new FixedClock(new DateTime(2029, 1, 1)))));
                    r.Check("after expiry -> expired", () =>
                        Assert.True(Solution.IsExpired(expiry, new FixedClock(new DateTime(2031, 1, 1)))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public interface IClock { DateTime Now { get; } }

            public static class Solution
            {
                public static bool IsExpired(DateTime expiresAt, IClock clock) => clock.Now >= expiresAt;
            }
            """,
        Hints =
        [
            "Use clock.Now, never DateTime.Now — that's the whole point.",
            "Expired means the current time has reached or passed expiresAt.",
        ],
        TestCases =
        [
            new() { Name = "before expiry -> not expired", IsHidden = false },
            new() { Name = "after expiry -> expired", IsHidden = false },
        ],
    };

    private static ExerciseSeed BoundaryValidation => new()
    {
        Slug = "boundary-validation",
        Title = "Boundary Testing",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Implement `IsValidAge(age)` — valid when `0 ≤ age ≤ 120`. Bugs hide at the edges,
            so the tests deliberately probe the boundaries: −1, 0, 120, and 121.
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: inclusive range 0..120.
                public static bool IsValidAge(int age)
                {
                    return false;
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
                    r.Check("-1 -> invalid", () => Assert.False(Solution.IsValidAge(-1)));
                    r.Check("0 -> valid (lower edge)", () => Assert.True(Solution.IsValidAge(0)));
                    r.Check("120 -> valid (upper edge)", () => Assert.True(Solution.IsValidAge(120)));
                    r.Check("121 -> invalid", () => Assert.False(Solution.IsValidAge(121)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static bool IsValidAge(int age) => age >= 0 && age <= 120;
            }
            """,
        Hints =
        [
            "Inclusive on both ends: >= 0 AND <= 120.",
            "Getting <= vs < right at the boundary is exactly what boundary tests catch.",
        ],
        TestCases =
        [
            new() { Name = "0 -> valid (lower edge)", IsHidden = false },
            new() { Name = "121 -> invalid", IsHidden = false },
        ],
    };

    private static ExerciseSeed FakeRepository => new()
    {
        Slug = "fake-repository",
        Title = "Fake Repository",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement an in-memory **fake** of the provided `IUserRepo` for use in tests — a
            real working implementation without a database. `Add` stores, `Get` returns the
            name or `null` if missing.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            // PROVIDED — do not modify:
            public interface IUserRepo { void Add(string id, string name); string? Get(string id); }

            // TODO: a lightweight in-memory implementation for tests.
            public sealed class FakeUserRepo : IUserRepo
            {
                public void Add(string id, string name) => throw new System.NotImplementedException();
                public string? Get(string id) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("stores and retrieves", () =>
                    {
                        IUserRepo repo = new FakeUserRepo();
                        repo.Add("1", "Ada");
                        Assert.Equal("Ada", repo.Get("1"));
                    });
                    r.Check("missing id -> null", () =>
                        Assert.True(new FakeUserRepo().Get("nope") == null));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public interface IUserRepo { void Add(string id, string name); string? Get(string id); }

            public sealed class FakeUserRepo : IUserRepo
            {
                private readonly Dictionary<string, string> _store = new();
                public void Add(string id, string name) => _store[id] = name;
                public string? Get(string id) => _store.TryGetValue(id, out var name) ? name : null;
            }
            """,
        Hints =
        [
            "A Dictionary<string,string> is a perfect in-memory backing store.",
            "Get returns null when the id isn't present (TryGetValue).",
        ],
        TestCases =
        [
            new() { Name = "stores and retrieves", IsHidden = false },
            new() { Name = "missing id -> null", IsHidden = false },
        ],
    };

    private static ExerciseSeed SpyLogger => new()
    {
        Slug = "spy-logger",
        Title = "Spy (Record Interactions)",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            A **spy** records how it was called so a test can assert on the interaction.
            Implement `SpyLogger : ILogger` that stores every logged message in a public
            `Messages` list — so a test can verify what was logged and how many times.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            // PROVIDED — do not modify:
            public interface ILogger { void Log(string message); }

            public sealed class SpyLogger : ILogger
            {
                // TODO: expose recorded messages; Log appends to them.
                public List<string> Messages { get; } = new();
                public void Log(string message) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("records each call", () =>
                    {
                        var spy = new SpyLogger();
                        ILogger log = spy;
                        log.Log("a");
                        log.Log("b");
                        Assert.Equal(2, spy.Messages.Count);
                        Assert.Equal("a", spy.Messages[0]);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public interface ILogger { void Log(string message); }

            public sealed class SpyLogger : ILogger
            {
                public List<string> Messages { get; } = new();
                public void Log(string message) => Messages.Add(message);
            }
            """,
        Hints =
        [
            "Log just appends the message to the Messages list.",
            "Tests then assert on Messages.Count and its contents.",
        ],
        TestCases = [new() { Name = "records each call", IsHidden = false }],
    };
}
