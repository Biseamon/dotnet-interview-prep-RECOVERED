namespace InterviewPrep.Infrastructure.Data.Seeding.Patterns;

// Creational patterns — control HOW objects are created: Singleton, Factory
// Method, Builder.
internal static partial class DesignPatternsContent
{
    private static LessonSeed CreationalLesson => new()
    {
        Slug = "creational-patterns",
        Title = "Creational Patterns",
        Order = 1,
        MarkdownContent =
            """
            ## Creational Patterns

            These control object construction:
            - **Singleton** — exactly one instance, globally accessible (use sparingly;
              prefer DI). Thread-safety matters.
            - **Factory Method** — a method decides which concrete type to create, so
              callers depend on an abstraction.
            - **Builder** — assemble a complex object step by step with a fluent API.
            """,
        Exercises =
        [
            Singleton,
            FactoryMethod,
            Builder,
            AbstractFactory,
            Prototype,
        ],
    };

    private static ExerciseSeed Singleton => new()
    {
        Slug = "singleton",
        Title = "Thread-Safe Singleton",
        Difficulty = "Easy",
        Kind = "Class",
        Prompt =
            """
            Implement `Config` as a **thread-safe singleton**: a private constructor, a
            static `Instance` that always returns the *same* object, and a mutable
            `Value` to prove state persists. The simplest thread-safe approach in C# is
            a `static readonly` field initialized eagerly (the CLR guarantees it runs once).
            """,
        StarterCode =
            """
            public sealed class Config
            {
                // TODO: private constructor + a static Instance returning one shared object.
                public static Config Instance => throw new System.NotImplementedException();

                public int Value { get; set; }
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("Instance returns the same object", () =>
                        Assert.True(ReferenceEquals(Config.Instance, Config.Instance)));
                    r.Check("state persists across accesses", () =>
                    {
                        Config.Instance.Value = 42;
                        Assert.Equal(42, Config.Instance.Value);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public sealed class Config
            {
                // Eager static-readonly init is thread-safe: the runtime guarantees the
                // static field initializer runs exactly once, before first use.
                private static readonly Config _instance = new Config();

                private Config() { } // prevents `new Config()` from outside

                public static Config Instance => _instance;

                public int Value { get; set; }
            }
            """,
        Hints =
        [
            "Make the constructor private so no one else can `new` it.",
            "Hold the single instance in a `private static readonly` field.",
            "Eager static-readonly initialization is inherently thread-safe in .NET.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "Instance returns the same object", IsHidden = false },
            new TestCaseSeed { Name = "state persists across accesses", IsHidden = false },
        ],
    };

    private static ExerciseSeed FactoryMethod => new()
    {
        Slug = "factory-method",
        Title = "Factory Method",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement `ShapeFactory.Create(kind, size)` returning a `Shape` (provided,
            abstract, with `Area()`). Support `"circle"` (size = radius, Area = πr²) and
            `"square"` (size = side, Area = side²). Callers depend only on `Shape`.
            """,
        StarterCode =
            """
            using System;

            // PROVIDED — do not modify:
            public abstract class Shape { public abstract double Area(); }

            public static class ShapeFactory
            {
                // TODO: return the right Shape subtype based on `kind`.
                public static Shape Create(string kind, double size)
                {
                    throw new NotImplementedException();
                }
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
                    r.Check("circle r=2 -> area 4π", () =>
                        Assert.True(Math.Abs(ShapeFactory.Create("circle", 2).Area() - Math.PI * 4) < 1e-9));
                    r.Check("square s=3 -> area 9", () =>
                        Assert.True(Math.Abs(ShapeFactory.Create("square", 3).Area() - 9) < 1e-9));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public abstract class Shape { public abstract double Area(); }

            public static class ShapeFactory
            {
                public static Shape Create(string kind, double size) => kind switch
                {
                    "circle" => new Circle(size),
                    "square" => new Square(size),
                    _ => throw new ArgumentException("unknown kind", nameof(kind)),
                };

                // Concrete products are private — callers only ever see `Shape`.
                private sealed class Circle(double r) : Shape { public override double Area() => Math.PI * r * r; }
                private sealed class Square(double s) : Shape { public override double Area() => s * s; }
            }
            """,
        Hints =
        [
            "Define concrete Circle and Square subclasses of Shape.",
            "A switch on `kind` chooses which to construct.",
            "Return them as the abstract `Shape` so callers stay decoupled from the concrete types.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "circle r=2 -> area 4π", IsHidden = false },
            new TestCaseSeed { Name = "square s=3 -> area 9", IsHidden = false },
        ],
    };

    private static ExerciseSeed Builder => new()
    {
        Slug = "builder",
        Title = "Builder (Fluent)",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement a fluent `BurgerBuilder`: `Patties(int)`, `AddCheese()`,
            `AddBacon()` each return `this` for chaining, and `Build()` returns the
            assembled `Burger`. This separates construction from the final object.
            """,
        StarterCode =
            """
            // PROVIDED — do not modify:
            public sealed class Burger
            {
                public int Patties { get; set; }
                public bool Cheese { get; set; }
                public bool Bacon { get; set; }
            }

            public sealed class BurgerBuilder
            {
                // TODO: fluent methods returning `this`, plus Build().
                public BurgerBuilder Patties(int n) => throw new System.NotImplementedException();
                public BurgerBuilder AddCheese() => throw new System.NotImplementedException();
                public BurgerBuilder AddBacon() => throw new System.NotImplementedException();
                public Burger Build() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("fluent chain builds the right burger", () =>
                    {
                        var b = new BurgerBuilder().Patties(2).AddCheese().Build();
                        Assert.Equal(2, b.Patties);
                        Assert.True(b.Cheese);
                        Assert.False(b.Bacon);
                    });
                    r.Check("bacon flag set independently", () =>
                    {
                        var b = new BurgerBuilder().Patties(1).AddBacon().Build();
                        Assert.True(b.Bacon);
                        Assert.False(b.Cheese);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public sealed class Burger
            {
                public int Patties { get; set; }
                public bool Cheese { get; set; }
                public bool Bacon { get; set; }
            }

            public sealed class BurgerBuilder
            {
                private readonly Burger _burger = new(); // accumulate state here

                // Each setter returns `this`, enabling the fluent chain.
                public BurgerBuilder Patties(int n) { _burger.Patties = n; return this; }
                public BurgerBuilder AddCheese() { _burger.Cheese = true; return this; }
                public BurgerBuilder AddBacon() { _burger.Bacon = true; return this; }
                public Burger Build() => _burger;
            }
            """,
        Hints =
        [
            "Keep a Burger instance inside the builder and mutate it.",
            "Every configuration method should `return this;` so calls can chain.",
            "Build() just returns the accumulated Burger.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "fluent chain builds the right burger", IsHidden = false },
            new TestCaseSeed { Name = "bacon flag set independently", IsHidden = false },
        ],
    };

    private static ExerciseSeed AbstractFactory => new()
    {
        Slug = "abstract-factory",
        Title = "Abstract Factory",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            An Abstract Factory creates **families** of related objects. Given the
            provided `IButton`, `ICheckbox`, and `IUiFactory` interfaces, implement a
            `DarkFactory` whose products render `"[dark button]"` and `"[dark checkbox]"`.
            Swapping the factory swaps the whole family — clients never see concrete types.
            """,
        StarterCode =
            """
            // PROVIDED — do not modify:
            public interface IButton { string Render(); }
            public interface ICheckbox { string Render(); }
            public interface IUiFactory { IButton CreateButton(); ICheckbox CreateCheckbox(); }

            // TODO: a factory that produces the "dark" family of widgets.
            public sealed class DarkFactory : IUiFactory
            {
                public IButton CreateButton() => throw new System.NotImplementedException();
                public ICheckbox CreateCheckbox() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("factory creates dark button", () =>
                        Assert.Equal("[dark button]", ((IUiFactory)new DarkFactory()).CreateButton().Render()));
                    r.Check("factory creates dark checkbox", () =>
                        Assert.Equal("[dark checkbox]", ((IUiFactory)new DarkFactory()).CreateCheckbox().Render()));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public interface IButton { string Render(); }
            public interface ICheckbox { string Render(); }
            public interface IUiFactory { IButton CreateButton(); ICheckbox CreateCheckbox(); }

            public sealed class DarkFactory : IUiFactory
            {
                public IButton CreateButton() => new DarkButton();
                public ICheckbox CreateCheckbox() => new DarkCheckbox();

                // Concrete products of the family are private to the factory.
                private sealed class DarkButton : IButton { public string Render() => "[dark button]"; }
                private sealed class DarkCheckbox : ICheckbox { public string Render() => "[dark checkbox]"; }
            }
            """,
        Hints =
        [
            "Each factory method returns a concrete product as its interface.",
            "Define DarkButton : IButton and DarkCheckbox : ICheckbox.",
            "Keep the concrete products private — clients only see the interfaces.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "factory creates dark button", IsHidden = false },
            new TestCaseSeed { Name = "factory creates dark checkbox", IsHidden = false },
        ],
    };

    private static ExerciseSeed Prototype => new()
    {
        Slug = "prototype",
        Title = "Prototype (Deep Clone)",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement `Clone()` on `Document` to return a **deep copy** — mutating the
            clone's `Tags` must not affect the original. (A shallow copy would share the
            same list; the Prototype pattern is about correct cloning.)
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public sealed class Document
            {
                public string Title { get; set; } = "";
                public List<string> Tags { get; set; } = new();

                // TODO: return a deep copy (Tags must be a NEW list).
                public Document Clone() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("clone copies values", () =>
                    {
                        var doc = new Document { Title = "spec" };
                        doc.Tags.Add("a");
                        var copy = doc.Clone();
                        Assert.Equal("spec", copy.Title);
                        Assert.Equal(1, copy.Tags.Count);
                    });
                    r.Check("clone is independent (deep)", () =>
                    {
                        var doc = new Document { Title = "spec" };
                        doc.Tags.Add("a");
                        var copy = doc.Clone();
                        copy.Tags.Add("b");           // mutate the clone
                        Assert.Equal(1, doc.Tags.Count); // original unaffected
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public sealed class Document
            {
                public string Title { get; set; } = "";
                public List<string> Tags { get; set; } = new();

                public Document Clone() => new()
                {
                    Title = Title,
                    Tags = new List<string>(Tags), // NEW list -> deep, independent copy
                };
            }
            """,
        Hints =
        [
            "A shallow copy would share the same Tags list reference.",
            "Construct a new List<string> from the existing Tags to copy them.",
            "Strings are immutable, so copying the Title reference is fine.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "clone copies values", IsHidden = false },
            new TestCaseSeed { Name = "clone is independent (deep)", IsHidden = false },
        ],
    };
}
