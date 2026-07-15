namespace InterviewPrep.Infrastructure.Data.Seeding.Patterns;

// Structural patterns — compose objects into larger structures: Adapter, Decorator.
internal static partial class DesignPatternsContent
{
    private static LessonSeed StructuralLesson => new()
    {
        Slug = "structural-patterns",
        Title = "Structural Patterns",
        Order = 2,
        MarkdownContent =
            """
            ## Structural Patterns

            These describe how objects are composed:
            - **Adapter** — wrap an incompatible interface so it fits what a client expects.
            - **Decorator** — wrap an object to add behaviour without changing its class;
              decorators implement the same interface they wrap, so they nest.
            """,
        Exercises =
        [
            Adapter,
            Decorator,
            Facade,
            Proxy,
            Composite,
            Bridge,
            Flyweight,
        ],
    };

    private static ExerciseSeed Adapter => new()
    {
        Slug = "adapter",
        Title = "Adapter",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            A `CelsiusSensor` (provided) reports Celsius, but clients need the
            `ITemperature` interface with `Fahrenheit()`. Write `FahrenheitAdapter`
            that wraps the sensor and converts (F = C × 9/5 + 32).
            """,
        StarterCode =
            """
            // PROVIDED — do not modify:
            public sealed class CelsiusSensor { public double ReadCelsius() => 25.0; }
            public interface ITemperature { double Fahrenheit(); }

            public sealed class FahrenheitAdapter : ITemperature
            {
                // TODO: store the sensor and convert its Celsius reading to Fahrenheit.
                public FahrenheitAdapter(CelsiusSensor sensor) => throw new System.NotImplementedException();
                public double Fahrenheit() => throw new System.NotImplementedException();
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
                    r.Check("25C adapts to 77F", () =>
                    {
                        ITemperature t = new FahrenheitAdapter(new CelsiusSensor());
                        Assert.True(Math.Abs(t.Fahrenheit() - 77.0) < 1e-9);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public sealed class CelsiusSensor { public double ReadCelsius() => 25.0; }
            public interface ITemperature { double Fahrenheit(); }

            public sealed class FahrenheitAdapter : ITemperature
            {
                private readonly CelsiusSensor _sensor; // the adaptee we wrap

                public FahrenheitAdapter(CelsiusSensor sensor) => _sensor = sensor;

                // Translate the adaptee's API into the interface the client wants.
                public double Fahrenheit() => _sensor.ReadCelsius() * 9.0 / 5.0 + 32.0;
            }
            """,
        Hints =
        [
            "Store the CelsiusSensor passed to the constructor in a field.",
            "Implement ITemperature.Fahrenheit by calling ReadCelsius and converting.",
            "F = C * 9 / 5 + 32 — use doubles to avoid integer division.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "25C adapts to 77F", IsHidden = false },
        ],
    };

    private static ExerciseSeed Decorator => new()
    {
        Slug = "decorator",
        Title = "Decorator",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Given `ICoffee` (with `Cost()` and `Description()`) and a base `Espresso`
            (provided), implement `MilkDecorator` and `SugarDecorator` that **wrap** an
            `ICoffee`, adding +1 cost & " + milk" and +1 cost & " + sugar" respectively.
            Because decorators implement `ICoffee`, they nest: milk(sugar(espresso)).
            """,
        StarterCode =
            """
            // PROVIDED — do not modify:
            public interface ICoffee { int Cost(); string Description(); }
            public sealed class Espresso : ICoffee
            {
                public int Cost() => 3;
                public string Description() => "espresso";
            }

            // TODO: implement two decorators that each wrap an ICoffee.
            public sealed class MilkDecorator : ICoffee
            {
                public MilkDecorator(ICoffee inner) => throw new System.NotImplementedException();
                public int Cost() => throw new System.NotImplementedException();
                public string Description() => throw new System.NotImplementedException();
            }

            public sealed class SugarDecorator : ICoffee
            {
                public SugarDecorator(ICoffee inner) => throw new System.NotImplementedException();
                public int Cost() => throw new System.NotImplementedException();
                public string Description() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("milk adds 1 to cost", () =>
                        Assert.Equal(4, new MilkDecorator(new Espresso()).Cost()));
                    r.Check("nested decorators stack cost", () =>
                        Assert.Equal(5, new SugarDecorator(new MilkDecorator(new Espresso())).Cost()));
                    r.Check("description composes", () =>
                        Assert.True(new MilkDecorator(new Espresso()).Description().Contains("milk")));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public interface ICoffee { int Cost(); string Description(); }
            public sealed class Espresso : ICoffee
            {
                public int Cost() => 3;
                public string Description() => "espresso";
            }

            public sealed class MilkDecorator : ICoffee
            {
                private readonly ICoffee _inner; // the coffee we're wrapping
                public MilkDecorator(ICoffee inner) => _inner = inner;
                public int Cost() => _inner.Cost() + 1;                    // delegate + add
                public string Description() => _inner.Description() + " + milk";
            }

            public sealed class SugarDecorator : ICoffee
            {
                private readonly ICoffee _inner;
                public SugarDecorator(ICoffee inner) => _inner = inner;
                public int Cost() => _inner.Cost() + 1;
                public string Description() => _inner.Description() + " + sugar";
            }
            """,
        Hints =
        [
            "Each decorator holds an inner ICoffee and implements ICoffee itself.",
            "Cost() calls inner.Cost() then adds its own charge.",
            "Because a decorator IS an ICoffee, you can wrap a decorator in another decorator.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "milk adds 1 to cost", IsHidden = false },
            new TestCaseSeed { Name = "nested decorators stack cost", IsHidden = false },
            new TestCaseSeed { Name = "description composes", IsHidden = true },
        ],
    };

    private static ExerciseSeed Facade => new()
    {
        Slug = "facade",
        Title = "Facade",
        Difficulty = "Easy",
        Kind = "Class",
        Prompt =
            """
            A Facade offers one simple entry point over a complex subsystem. Given the
            provided `Cpu` and `Memory` subsystems, implement `Computer.Start()` to run
            the boot sequence and return `"freeze,load,execute"`.
            """,
        StarterCode =
            """
            // PROVIDED — do not modify:
            public sealed class Cpu { public string Freeze() => "freeze"; public string Execute() => "execute"; }
            public sealed class Memory { public string Load() => "load"; }

            public sealed class Computer
            {
                // TODO: orchestrate the subsystems behind one simple call.
                public string Start() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("Start runs the boot sequence", () =>
                        Assert.Equal("freeze,load,execute", new Computer().Start()));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public sealed class Cpu { public string Freeze() => "freeze"; public string Execute() => "execute"; }
            public sealed class Memory { public string Load() => "load"; }

            public sealed class Computer
            {
                private readonly Cpu _cpu = new();
                private readonly Memory _memory = new();

                // The facade hides the subsystem coordination behind one method.
                public string Start() => $"{_cpu.Freeze()},{_memory.Load()},{_cpu.Execute()}";
            }
            """,
        Hints =
        [
            "The Computer holds the subsystems and calls them in order.",
            "Sequence: cpu.Freeze, memory.Load, cpu.Execute.",
            "Join the three results with commas.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "Start runs the boot sequence", IsHidden = false },
        ],
    };

    private static ExerciseSeed Proxy => new()
    {
        Slug = "proxy",
        Title = "Proxy (Caching)",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            A Proxy stands in for another object to add behaviour transparently. Implement
            a `CachingImage` proxy that wraps the provided `RealImage` and caches its
            `Load()` result, so the expensive real load runs **only once** no matter how
            many times you call `Load()`.
            """,
        StarterCode =
            """
            // PROVIDED — do not modify:
            public interface IImage { string Load(); }
            public sealed class RealImage : IImage
            {
                public int LoadCount { get; private set; }
                public string Load() { LoadCount++; return "pixels"; }
            }

            public sealed class CachingImage : IImage
            {
                // TODO: wrap RealImage; call it at most once, cache the result.
                public CachingImage(RealImage real) => throw new System.NotImplementedException();
                public string Load() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("returns the underlying result", () =>
                    {
                        var proxy = new CachingImage(new RealImage());
                        Assert.Equal("pixels", proxy.Load());
                    });
                    r.Check("caches: real Load runs only once", () =>
                    {
                        var real = new RealImage();
                        var proxy = new CachingImage(real);
                        proxy.Load(); proxy.Load(); proxy.Load();
                        Assert.Equal(1, real.LoadCount);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public interface IImage { string Load(); }
            public sealed class RealImage : IImage
            {
                public int LoadCount { get; private set; }
                public string Load() { LoadCount++; return "pixels"; }
            }

            public sealed class CachingImage : IImage
            {
                private readonly RealImage _real;
                private string? _cached; // null until first load

                public CachingImage(RealImage real) => _real = real;

                // Null-coalescing assignment: compute once, reuse thereafter.
                public string Load() => _cached ??= _real.Load();
            }
            """,
        Hints =
        [
            "Store the RealImage and a nullable cache field.",
            "On Load, if the cache is null, call the real Load and store it.",
            "`_cached ??= _real.Load()` does exactly that in one line.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "returns the underlying result", IsHidden = false },
            new TestCaseSeed { Name = "caches: real Load runs only once", IsHidden = false },
        ],
    };

    private static ExerciseSeed Composite => new()
    {
        Slug = "composite",
        Title = "Composite",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            The Composite pattern lets clients treat individual objects and groups
            uniformly. Given `IComponent` with `Count()` (number of leaves), implement a
            `Leaf` (counts as 1) and a `Composite` that holds children (via `Add`) and
            returns the **sum** of its children's counts. Trees nest arbitrarily.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            // PROVIDED — do not modify:
            public interface IComponent { int Count(); }

            public sealed class Leaf : IComponent
            {
                // TODO: a single leaf counts as 1.
                public int Count() => throw new System.NotImplementedException();
            }

            public sealed class Composite : IComponent
            {
                // TODO: hold children; Count is the sum of their Counts.
                public void Add(IComponent child) => throw new System.NotImplementedException();
                public int Count() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("single leaf -> 1", () => Assert.Equal(1, new Leaf().Count()));
                    r.Check("nested tree sums leaves", () =>
                    {
                        var root = new Composite();
                        root.Add(new Leaf());
                        var branch = new Composite();
                        branch.Add(new Leaf());
                        branch.Add(new Leaf());
                        root.Add(branch);           // root: leaf + (leaf + leaf) = 3
                        Assert.Equal(3, root.Count());
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;
            using System.Linq;

            public interface IComponent { int Count(); }

            public sealed class Leaf : IComponent
            {
                public int Count() => 1;
            }

            public sealed class Composite : IComponent
            {
                private readonly List<IComponent> _children = new();
                public void Add(IComponent child) => _children.Add(child);

                // Uniform treatment: recurse into children, whatever their concrete type.
                public int Count() => _children.Sum(c => c.Count());
            }
            """,
        Hints =
        [
            "Leaf.Count is simply 1.",
            "Composite keeps a list of IComponent children.",
            "Composite.Count sums child.Count() — children may themselves be composites.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "single leaf -> 1", IsHidden = false },
            new TestCaseSeed { Name = "nested tree sums leaves", IsHidden = false },
        ],
    };

    private static ExerciseSeed Bridge => new()
    {
        Slug = "bridge",
        Title = "Bridge",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            The Bridge pattern separates an **abstraction** from its **implementation** so
            they vary independently. Given the provided `IRenderer` implementation
            interface, implement a `Circle` abstraction that holds a renderer and whose
            `Draw()` delegates to `renderer.Render("circle")`.
            """,
        StarterCode =
            """
            // PROVIDED — do not modify:
            public interface IRenderer { string Render(string shape); }

            public sealed class Circle
            {
                // TODO: hold the renderer; Draw delegates to it. Abstraction & impl are decoupled.
                public Circle(IRenderer renderer) => throw new System.NotImplementedException();
                public string Draw() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                // A test renderer standing in for a concrete implementation.
                private sealed class BracketRenderer : IRenderer
                {
                    public string Render(string shape) => $"[{shape}]";
                }

                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("circle draws via its renderer", () =>
                        Assert.Equal("[circle]", new Circle(new BracketRenderer()).Draw()));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public interface IRenderer { string Render(string shape); }

            public sealed class Circle
            {
                private readonly IRenderer _renderer; // the "bridge" to the implementation

                public Circle(IRenderer renderer) => _renderer = renderer;

                // The abstraction stays the same regardless of which renderer is injected.
                public string Draw() => _renderer.Render("circle");
            }
            """,
        Hints =
        [
            "Store the injected IRenderer in a field.",
            "Draw() calls renderer.Render(\"circle\").",
            "Swapping renderers changes HOW it draws without changing Circle.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "circle draws via its renderer", IsHidden = false },
        ],
    };

    private static ExerciseSeed Flyweight => new()
    {
        Slug = "flyweight",
        Title = "Flyweight",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            The Flyweight pattern shares immutable instances to save memory. Implement a
            `GlyphFactory` whose `Get(char)` returns the **same** object for the same
            character (caching), and different objects for different characters.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public sealed class GlyphFactory
            {
                // TODO: cache one shared object per character.
                public object Get(char c) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("same char -> shared instance", () =>
                    {
                        var f = new GlyphFactory();
                        Assert.True(ReferenceEquals(f.Get('a'), f.Get('a')));
                    });
                    r.Check("different chars -> different instances", () =>
                    {
                        var f = new GlyphFactory();
                        Assert.False(ReferenceEquals(f.Get('a'), f.Get('b')));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public sealed class GlyphFactory
            {
                private readonly Dictionary<char, object> _pool = new();

                // Reuse the cached flyweight if present; otherwise create and store one.
                public object Get(char c)
                {
                    if (!_pool.TryGetValue(c, out var glyph))
                    {
                        glyph = new object();
                        _pool[c] = glyph;
                    }
                    return glyph;
                }
            }
            """,
        Hints =
        [
            "Keep a Dictionary<char, object> pool.",
            "Return the cached object if the char is already known.",
            "Otherwise create one, store it, and return it.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "same char -> shared instance", IsHidden = false },
            new TestCaseSeed { Name = "different chars -> different instances", IsHidden = false },
        ],
    };
}
