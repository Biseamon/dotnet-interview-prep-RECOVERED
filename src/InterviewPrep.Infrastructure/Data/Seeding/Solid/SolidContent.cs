namespace InterviewPrep.Infrastructure.Data.Seeding.Solid;

// The "SOLID Principles" topic — one exercise per principle, each a small gradeable
// task that demonstrates the idea in code.
internal static partial class SolidContent
{
    public static TopicSeed Topic => new()
    {
        Slug = "solid",
        Name = "SOLID Principles",
        Description = "Single Responsibility, Open/Closed, Liskov, Interface Segregation, and Dependency Inversion — in code.",
        Order = 9,
        Lessons =
        [
            SolidOverviewLesson,
            SrpOcpLessons,
            LspIspDipLessons,
        ],
    };

    // ---- Lesson 1: the map ---------------------------------------------------
    private static LessonSeed SolidOverviewLesson => new()
    {
        Slug = "solid-principles",
        Title = "SOLID: The Big Picture",
        Order = 1,
        MarkdownContent =
            """
            ## Why SOLID exists

            SOLID is five design principles (Robert C. Martin popularised the acronym) that
            all push toward the same goal: **code that is cheap to change**. Not clever code —
            *changeable* code. Requirements move; the principles keep a small change from
            rippling into a big one.

            | Letter | Principle | One-line litmus test |
            |--------|-----------|----------------------|
            | **S** | Single Responsibility | "What is the *one* reason this class would change?" |
            | **O** | Open/Closed | "Can I add a new case without editing existing code?" |
            | **L** | Liskov Substitution | "Can I use the subtype through the base type and not get surprised?" |
            | **I** | Interface Segregation | "Is any implementer forced to stub out methods it doesn't use?" |
            | **D** | Dependency Inversion | "Does my policy depend on an interface, or on a concrete detail?" |

            ### The thread that ties them together

            Four of the five are really about **one seam**: an abstraction (interface or base
            class) placed between a *policy* (high-level intent) and a *detail* (how it's done).

            - **OCP** wants new behaviour to arrive as a new implementation of that abstraction.
            - **LSP** is the rule that makes those implementations *interchangeable* — a subtype
              that breaks the base's promises poisons every `Open/Closed` extension point.
            - **ISP** keeps the abstraction *small* so implementers aren't dragged into methods
              they don't need.
            - **DIP** points the dependency arrow at the abstraction instead of the detail, so
              you can swap or fake the detail freely.

            SRP is the odd one out: it's about *where you draw class boundaries* in the first
            place. Get SRP wrong and the other four have nothing clean to hang off of.

            ### Why interviewers ask about SOLID

            It's a fast probe for design maturity. A junior recites the acronym. A senior says
            "here's a class doing two jobs, here's the smell, here's how I'd split it, and
            here's the test that got easier." Interviewers want the second answer: a **smell →
            refactor → payoff** story, not a definition. The exercises in this topic are built
            to rehearse exactly that motion — each one hands you a design with a flaw (or a
            missing seam) and grades the *behaviour* of your fix.

            ### A caution

            SOLID is a set of *heuristics*, not laws. Over-applied, they produce a fog of
            one-method interfaces and indirection. The skill is knowing *when* the change-cost
            is real. "It might change someday" is not a reason to add a seam; "this has changed
            three times and each time I edited a switch statement" is.
            """,
        Exercises =
        [
            SingleResponsibility,
            SrpSplit,
        ],
    };

    // ---- Lesson 2: SRP + OCP in depth ---------------------------------------
    private static LessonSeed SrpOcpLessons => new()
    {
        Slug = "solid-srp-ocp",
        Title = "S & O: Responsibility and Extension",
        Order = 2,
        MarkdownContent =
            """
            ## Single Responsibility Principle

            **Definition.** A class should have one reason to change — one *actor* (a
            stakeholder or concern) it answers to. If the accountants and the DBAs would both
            demand edits to the same class for unrelated reasons, that class has two
            responsibilities.

            **The smell it fixes.** The *god class* / *swiss-army-knife*: a `Report` that
            computes totals, formats HTML, and writes a file. A change to the file format
            forces you to recompile and re-test the tax math. Merge conflicts cluster on it
            because everyone touches it.

            **Before → after.**

            ```csharp
            // BEFORE: three concerns tangled together.
            class Invoice {
                decimal Total() { /* pricing rules */ }
                string ToHtml() { /* presentation */ }
                void Save(string path) { /* persistence */ }
            }

            // AFTER: one reason to change each.
            class Invoice          { public decimal Total() { ... } }   // pricing
            class InvoiceRenderer  { public string ToHtml(Invoice i) { ... } }   // presentation
            class InvoiceRepository{ public void Save(Invoice i) { ... } }   // persistence
            ```

            The payoff is testability: `Total()` can be unit-tested without touching a disk or
            an HTML parser.

            ## Open/Closed Principle

            **Definition.** Software entities should be **open for extension, closed for
            modification**. You add new behaviour by adding new code, not by editing code that
            already works.

            **The smell it fixes.** The *growing switch* (or `if/else` ladder) on a type tag:

            ```csharp
            // SMELL: every new shape edits this method — and risks the old cases.
            double Area(Shape s) => s.Kind switch {
                "circle" => Math.PI * s.R * s.R,
                "square" => s.Side * s.Side,
                // add "triangle" here... and in five other switches...
            };
            ```

            **The fix: polymorphism / Strategy.** Push each case behind a common abstraction so
            the dispatcher never grows:

            ```csharp
            abstract class Shape { public abstract double Area(); }
            double Total(IEnumerable<Shape> xs) => xs.Sum(x => x.Area()); // never edited again
            ```

            A new `Triangle : Shape` is *added*; `Total` is *closed*. When the set of variants
            is registered at runtime (a dictionary of `IDiscountRule`, a list of handlers), you
            get OCP *and* pluggability — new rules drop in without a redeploy of the core.

            **Why interviewers ask.** "How would you add a third payment method?" instantly
            separates candidates who reach for a new `switch` case from those who reach for a
            new implementation of an existing interface. The second answer scales; the first
            accumulates risk in one hot method.
            """,
        Exercises =
        [
            OpenClosed,
            OcpStrategyDiscount,
            OcpHandlerPipeline,
        ],
    };

    // ---- Lesson 3: LSP + ISP + DIP in depth ---------------------------------
    private static LessonSeed LspIspDipLessons => new()
    {
        Slug = "solid-lsp-isp-dip",
        Title = "L, I & D: Substitution, Segregation, Inversion",
        Order = 3,
        MarkdownContent =
            """
            ## Liskov Substitution Principle

            **Definition.** If `S` is a subtype of `T`, code written against `T` must keep
            working when handed an `S` — without knowing the difference. Subtypes may *not*
            strengthen preconditions, weaken postconditions, or break invariants the base
            promised.

            **The smell it fixes.** The *lying subtype*. The classic is **Square : Rectangle**:
            a `Rectangle` promises "set width and height independently"; a `Square` that keeps
            them equal breaks that promise, so `SetWidth(5); SetHeight(4)` gives area 16 instead
            of 20 and silently corrupts callers. Another: a `Bird` base with `Fly()` and an
            `Ostrich` that throws from `Fly()`.

            **The fix.** Don't force the "is-a" if the contract doesn't hold. Model the shared
            *capability*, not the taxonomy: a read-only `Shape.Area()` that every shape can
            honour, or split `Bird` into `IFlyingBird` / `IWalkingBird`. The tell-tale of an LSP
            violation is a subtype that overrides a method to **throw**, **no-op**, or return a
            value the base forbade.

            **Why it matters for OCP.** Every `Open/Closed` extension point *is* an LSP contract.
            If your new implementation quietly violates the base's promise, every polymorphic
            call site becomes a landmine.

            ## Interface Segregation Principle

            **Definition.** No client should be forced to depend on methods it does not use.
            Prefer several small, role-focused interfaces over one fat one.

            **The smell it fixes.** The *fat interface*: `IMachine { Print(); Scan(); Fax();
            Staple(); }`. A humble printer must now implement `Scan`, `Fax`, and `Staple` —
            usually by throwing `NotSupportedException`. That's an LSP violation *caused by* an
            ISP violation, and it forces recompiles on every implementer when the interface
            grows.

            **Before → after.**

            ```csharp
            // BEFORE
            interface IMachine { void Print(); void Scan(); void Fax(); }

            // AFTER — clients depend only on the role they need.
            interface IPrinter { void Print(); }
            interface IScanner { void Scan(); }
            class SimplePrinter : IPrinter { public void Print() { ... } } // no stubs
            ```

            ## Dependency Inversion Principle

            **Definition.** High-level modules should not depend on low-level modules; **both
            should depend on abstractions**. And abstractions should not depend on details —
            details depend on abstractions.

            **The smell it fixes.** *Concrete coupling*: an `OrderService` that does
            `new SmtpEmailSender()` inside itself. Now the business policy is welded to SMTP —
            you can't test it without a mail server, and switching to SMS means editing the
            policy.

            **Before → after.**

            ```csharp
            // BEFORE: policy depends on a detail.
            class OrderService {
                private readonly SmtpEmailSender _mail = new(); // welded shut
                public void Place(Order o) { ...; _mail.Send(...); }
            }

            // AFTER: policy depends on an abstraction; the detail is injected.
            class OrderService {
                private readonly IMessageSender _sender;
                public OrderService(IMessageSender sender) => _sender = sender;
                public void Place(Order o) { ...; _sender.Send(...); }
            }
            ```

            Now a test injects a `SpySender`, production injects `SmtpSender`, and the policy
            never changes. **DIP is what makes a codebase testable** — it's the principle that
            most directly buys you fast, isolated unit tests. Interviewers probe it with "how
            would you test this class that sends email?" — the DIP answer is "I wouldn't have
            let it `new` the sender in the first place."

            ### Composition over inheritance (the recurring escape hatch)

            Notice how L, I, and D all steer you away from deep inheritance and toward *holding*
            an abstraction (a field) rather than *being* a subclass. Composing small behaviours
            you can swap at runtime beats a rigid class hierarchy you can only change at compile
            time. When an exercise asks you to "refactor an inheritance chain," reach for a
            composed strategy field.
            """,
        Exercises =
        [
            Liskov,
            LspRectangleSquare,
            InterfaceSegregation,
            DependencyInversion,
            DipOrderPolicy,
            CompositionOverInheritance,
        ],
    };

    private static ExerciseSeed SingleResponsibility => new()
    {
        Slug = "srp-tax-calculator",
        Title = "Single Responsibility",
        Difficulty = "Easy",
        Kind = "Class",
        Prompt =
            """
            Single Responsibility means a class does one job. Implement a `TaxCalculator`
            whose only responsibility is computing tax: `Calculate(amount, ratePercent)`
            returns `amount × ratePercent / 100`. (Formatting, persistence, etc. belong in
            other classes.)
            """,
        StarterCode =
            """
            public sealed class TaxCalculator
            {
                // TODO: compute tax = amount * ratePercent / 100. Nothing else.
                public decimal Calculate(decimal amount, decimal ratePercent)
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
                    r.Check("100 @ 20% -> 20", () => Assert.Equal(20m, new TaxCalculator().Calculate(100m, 20m)));
                    r.Check("50 @ 10% -> 5", () => Assert.Equal(5m, new TaxCalculator().Calculate(50m, 10m)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public sealed class TaxCalculator
            {
                // One responsibility: the tax calculation. Easy to test and reuse.
                public decimal Calculate(decimal amount, decimal ratePercent) => amount * ratePercent / 100m;
            }
            """,
        Hints =
        [
            "The formula is amount * ratePercent / 100.",
            "Keep it to just the calculation — that's the whole point of SRP.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "100 @ 20% -> 20", IsHidden = false },
            new TestCaseSeed { Name = "50 @ 10% -> 5", IsHidden = false },
        ],
    };

    private static ExerciseSeed OpenClosed => new()
    {
        Slug = "ocp-total-area",
        Title = "Open/Closed",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Open/Closed: you should add new behaviour without editing existing code. Given
            the provided polymorphic `Shape` (abstract `Area()`), implement
            `TotalArea(Shape[])` to sum areas. It works for *any* current or future shape
            subtype — no `if (shape is Circle)` branching needed.
            """,
        StarterCode =
            """
            using System;
            using System.Linq;

            // PROVIDED — do not modify:
            public abstract class Shape { public abstract double Area(); }
            public sealed class Circle : Shape
            {
                private readonly double _r;
                public Circle(double r) => _r = r;
                public override double Area() => Math.PI * _r * _r;
            }
            public sealed class Square : Shape
            {
                private readonly double _s;
                public Square(double s) => _s = s;
                public override double Area() => _s * _s;
            }

            public static class Solution
            {
                // TODO: sum areas polymorphically (no type checks).
                public static double TotalArea(Shape[] shapes)
                {
                    return 0;
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
                    r.Check("circle(1) + square(2) = π + 4", () =>
                    {
                        var shapes = new Shape[] { new Circle(1), new Square(2) };
                        Assert.True(Math.Abs(Solution.TotalArea(shapes) - (Math.PI + 4)) < 1e-9);
                    });
                    r.Check("empty -> 0", () => Assert.Equal(0d, Solution.TotalArea(new Shape[0])));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Linq;

            public abstract class Shape { public abstract double Area(); }
            public sealed class Circle : Shape
            {
                private readonly double _r;
                public Circle(double r) => _r = r;
                public override double Area() => Math.PI * _r * _r;
            }
            public sealed class Square : Shape
            {
                private readonly double _s;
                public Square(double s) => _s = s;
                public override double Area() => _s * _s;
            }

            public static class Solution
            {
                // Polymorphism keeps this closed for modification: a new Shape subtype
                // just works, no edits here.
                public static double TotalArea(Shape[] shapes) => shapes.Sum(s => s.Area());
            }
            """,
        Hints =
        [
            "Call each shape's Area() — don't switch on its concrete type.",
            "Sum the results.",
            "That's OCP: new shapes need no change to TotalArea.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "circle(1) + square(2) = π + 4", IsHidden = false },
            new TestCaseSeed { Name = "empty -> 0", IsHidden = true },
        ],
    };

    private static ExerciseSeed Liskov => new()
    {
        Slug = "lsp-substitution",
        Title = "Liskov Substitution",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Liskov: a subtype must be usable anywhere its base is expected, honouring the
            base's contract. The provided `Discount` base guarantees `Apply` never returns
            more than the original price. Implement `PercentageDiscount` so that, used
            through a `Discount` reference, it upholds that contract.
            """,
        StarterCode =
            """
            // PROVIDED — do not modify:
            public abstract class Discount
            {
                // Contract: the returned price is between 0 and the original price.
                public abstract decimal Apply(decimal price);
            }

            public sealed class PercentageDiscount : Discount
            {
                // TODO: take a percent (0..100) and reduce the price by it.
                public PercentageDiscount(decimal percent) => throw new System.NotImplementedException();
                public override decimal Apply(decimal price) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("used via base type, 10% off 100 -> 90", () =>
                    {
                        Discount d = new PercentageDiscount(10m); // substituted for the base
                        Assert.Equal(90m, d.Apply(100m));
                    });
                    r.Check("honours contract: result <= original", () =>
                    {
                        Discount d = new PercentageDiscount(25m);
                        var result = d.Apply(200m);
                        Assert.True(result <= 200m && result >= 0m);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public abstract class Discount
            {
                public abstract decimal Apply(decimal price);
            }

            public sealed class PercentageDiscount : Discount
            {
                private readonly decimal _percent;
                public PercentageDiscount(decimal percent) => _percent = percent;

                // Never returns more than `price` -> upholds the base contract, so it's a
                // safe substitute for Discount anywhere.
                public override decimal Apply(decimal price) => price * (1 - _percent / 100m);
            }
            """,
        Hints =
        [
            "Store the percent and reduce the price by it.",
            "price * (1 - percent/100) keeps the result within [0, price].",
            "Staying within the base's contract is what makes substitution safe.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "used via base type, 10% off 100 -> 90", IsHidden = false },
            new TestCaseSeed { Name = "honours contract: result <= original", IsHidden = false },
        ],
    };

    private static ExerciseSeed InterfaceSegregation => new()
    {
        Slug = "isp-segregation",
        Title = "Interface Segregation",
        Difficulty = "Easy",
        Kind = "Class",
        Prompt =
            """
            Interface Segregation: don't force a class to implement methods it doesn't need.
            Instead of one fat `IMachine { Print; Scan; Fax; }`, the provided design splits
            them. Implement a `SimplePrinter` that implements **only** `IPrinter`.
            """,
        StarterCode =
            """
            // PROVIDED — do not modify (small, focused interfaces):
            public interface IPrinter { string Print(string doc); }
            public interface IScanner { string Scan(); }

            // TODO: a printer implements ONLY what it needs.
            public sealed class SimplePrinter : IPrinter
            {
                public string Print(string doc) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("prints via the focused interface", () =>
                    {
                        IPrinter p = new SimplePrinter();
                        Assert.Equal("printing: report", p.Print("report"));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public interface IPrinter { string Print(string doc); }
            public interface IScanner { string Scan(); }

            // Implements only IPrinter — not burdened with Scan/Fax it doesn't support.
            public sealed class SimplePrinter : IPrinter
            {
                public string Print(string doc) => $"printing: {doc}";
            }
            """,
        Hints =
        [
            "Only implement IPrinter.Print.",
            "Return \"printing: \" followed by the document.",
            "The point: a simple printer shouldn't be forced to implement Scan or Fax.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "prints via the focused interface", IsHidden = false },
        ],
    };

    private static ExerciseSeed DependencyInversion => new()
    {
        Slug = "dip-inversion",
        Title = "Dependency Inversion",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Dependency Inversion: depend on abstractions, not concretions. Implement an
            `AlertService` that takes an `IMessageSender` (provided) via its constructor
            and uses it in `Raise(text)`. Because it depends on the interface, you can inject
            email, SMS, or a test double — without changing `AlertService`.
            """,
        StarterCode =
            """
            // PROVIDED — do not modify:
            public interface IMessageSender { void Send(string message); }

            public sealed class AlertService
            {
                // TODO: depend on the abstraction; Raise formats and sends "ALERT: {text}".
                public AlertService(IMessageSender sender) => throw new System.NotImplementedException();
                public void Raise(string text) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            using System.Collections.Generic;

            public static class __Harness
            {
                // A test double injected in place of a real email/SMS sender.
                private sealed class SpySender : IMessageSender
                {
                    public List<string> Sent { get; } = new();
                    public void Send(string message) => Sent.Add(message);
                }

                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("uses the injected sender", () =>
                    {
                        var spy = new SpySender();
                        new AlertService(spy).Raise("disk full");
                        Assert.Equal(1, spy.Sent.Count);
                        Assert.Equal("ALERT: disk full", spy.Sent[0]);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public interface IMessageSender { void Send(string message); }

            public sealed class AlertService
            {
                private readonly IMessageSender _sender; // abstraction, not a concrete class

                public AlertService(IMessageSender sender) => _sender = sender;

                public void Raise(string text) => _sender.Send($"ALERT: {text}");
            }
            """,
        Hints =
        [
            "Store the injected IMessageSender in a field.",
            "Raise sends \"ALERT: \" + text through it.",
            "Depending on the interface lets you swap implementations (and test with a spy).",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "uses the injected sender", IsHidden = false },
        ],
    };

    // ======================================================================
    //  ADDED EXERCISES — difficulty ladder, refactor-flavoured
    // ======================================================================

    // SRP (Easy/Medium): split a two-job class into two independently-tested types.
    private static ExerciseSeed SrpSplit => new()
    {
        Slug = "srp-split-report",
        Title = "SRP: Split the Two-Job Class",
        Difficulty = "Easy",
        Kind = "Class",
        Prompt =
            """
            A `UserReport` class currently does two unrelated jobs: it computes a total and it
            formats a header string. That's two reasons to change. Split the work into two
            focused classes so each responsibility can be used and tested on its own:

            - `TotalCalculator.Sum(int[] values)` returns the sum of the values.
            - `HeaderFormatter.Format(string title, int count)` returns `"{title} ({count})"`.

            The harness exercises each class independently — neither should depend on the other.
            """,
        StarterCode =
            """
            using System.Linq;

            // TODO: two classes, one responsibility each.
            public sealed class TotalCalculator
            {
                public int Sum(int[] values) => throw new System.NotImplementedException();
            }

            public sealed class HeaderFormatter
            {
                public string Format(string title, int count) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("calculator sums independently", () =>
                        Assert.Equal(6, new TotalCalculator().Sum(new[] { 1, 2, 3 })));
                    r.Check("calculator handles empty", () =>
                        Assert.Equal(0, new TotalCalculator().Sum(new int[0])));
                    r.Check("formatter formats independently", () =>
                        Assert.Equal("Sales (3)", new HeaderFormatter().Format("Sales", 3)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Linq;

            // One reason to change: the totalling rule.
            public sealed class TotalCalculator
            {
                public int Sum(int[] values) => values.Sum();
            }

            // A different reason to change: the presentation format.
            public sealed class HeaderFormatter
            {
                public string Format(string title, int count) => $"{title} ({count})";
            }
            """,
        Hints =
        [
            "Each class should compile and work without referencing the other.",
            "Sum can use a loop or values.Sum() from System.Linq.",
            "Format is just string interpolation: \"{title} ({count})\".",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "calculator sums independently", IsHidden = false },
            new TestCaseSeed { Name = "calculator handles empty", IsHidden = true },
            new TestCaseSeed { Name = "formatter formats independently", IsHidden = false },
        ],
    };

    // OCP via Strategy (Medium): the harness registers a NEW discount rule and expects
    // the calculator to use it without any edit to the calculator.
    private static ExerciseSeed OcpStrategyDiscount => new()
    {
        Slug = "ocp-strategy-discount",
        Title = "OCP: Pluggable Discount Rules",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Build a discount engine that is closed for modification but open for extension. The
            provided `IDiscountRule` abstraction has `decimal ApplyTo(decimal price)`.

            Implement `DiscountCalculator` so that:
            - its constructor takes an `IDiscountRule[]` (an ordered pipeline of rules),
            - `Total(decimal price)` applies each rule in order, feeding the output of one into
              the next, and returns the final price.

            Because it depends only on the interface, the harness can register a brand-new rule
            type it defines itself and your calculator must use it — with no changes to
            `DiscountCalculator`. Also implement `PercentOff` (constructed with a percent, e.g.
            10 means 10% off) as one concrete rule.
            """,
        StarterCode =
            """
            // PROVIDED — do not modify:
            public interface IDiscountRule { decimal ApplyTo(decimal price); }

            public sealed class DiscountCalculator
            {
                // TODO: store the pipeline; Total applies each rule in order.
                public DiscountCalculator(IDiscountRule[] rules) => throw new System.NotImplementedException();
                public decimal Total(decimal price) => throw new System.NotImplementedException();
            }

            public sealed class PercentOff : IDiscountRule
            {
                // TODO: percent = 10 means 10% off.
                public PercentOff(decimal percent) => throw new System.NotImplementedException();
                public decimal ApplyTo(decimal price) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                // A NEW rule the calculator has never seen — proves it's open for extension.
                private sealed class FlatOff : IDiscountRule
                {
                    private readonly decimal _amount;
                    public FlatOff(decimal amount) => _amount = amount;
                    public decimal ApplyTo(decimal price) => price - _amount;
                }

                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("single provided rule: 10% off 100 -> 90", () =>
                    {
                        var calc = new DiscountCalculator(new IDiscountRule[] { new PercentOff(10m) });
                        Assert.Equal(90m, calc.Total(100m));
                    });
                    r.Check("uses a newly-registered rule type without edits", () =>
                    {
                        var calc = new DiscountCalculator(new IDiscountRule[] { new FlatOff(15m) });
                        Assert.Equal(85m, calc.Total(100m));
                    });
                    r.Check("pipeline chains rules in order: 10% then -15 on 100 -> 75", () =>
                    {
                        var calc = new DiscountCalculator(new IDiscountRule[] { new PercentOff(10m), new FlatOff(15m) });
                        Assert.Equal(75m, calc.Total(100m));
                    });
                    r.Check("no rules -> price unchanged", () =>
                    {
                        var calc = new DiscountCalculator(new IDiscountRule[0]);
                        Assert.Equal(100m, calc.Total(100m));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public interface IDiscountRule { decimal ApplyTo(decimal price); }

            public sealed class DiscountCalculator
            {
                private readonly IDiscountRule[] _rules;
                public DiscountCalculator(IDiscountRule[] rules) => _rules = rules;

                // Closed for modification: it never inspects concrete rule types, so any new
                // IDiscountRule slots in without touching this class.
                public decimal Total(decimal price)
                {
                    var result = price;
                    foreach (var rule in _rules)
                        result = rule.ApplyTo(result);
                    return result;
                }
            }

            public sealed class PercentOff : IDiscountRule
            {
                private readonly decimal _percent;
                public PercentOff(decimal percent) => _percent = percent;
                public decimal ApplyTo(decimal price) => price * (1 - _percent / 100m);
            }
            """,
        Hints =
        [
            "DiscountCalculator must never mention a concrete rule type — only IDiscountRule.",
            "Total starts from price and folds each rule's output into the next.",
            "PercentOff: price * (1 - percent/100). 10 percent off 100 is 90.",
            "That the harness can add FlatOff without editing your calculator IS Open/Closed.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "single provided rule: 10% off 100 -> 90", IsHidden = false },
            new TestCaseSeed { Name = "uses a newly-registered rule type without edits", IsHidden = false },
            new TestCaseSeed { Name = "pipeline chains rules in order: 10% then -15 on 100 -> 75", IsHidden = false },
            new TestCaseSeed { Name = "no rules -> price unchanged", IsHidden = true },
        ],
    };

    // OCP via chain of responsibility (Hard): a pipeline of handlers, each an
    // implementation of the same abstraction; adding a handler needs no dispatcher edit.
    private static ExerciseSeed OcpHandlerPipeline => new()
    {
        Slug = "ocp-handler-pipeline",
        Title = "OCP: Chain of Handlers",
        Difficulty = "Hard",
        Kind = "Class",
        Prompt =
            """
            Build an extensible request pipeline (a chain of responsibility). A `Request` has a
            mutable `Log` list and an `Amount`. The provided abstraction is:

                public abstract class Handler { public abstract void Handle(Request req); }

            Implement `Pipeline`:
            - constructor takes a `Handler[]`,
            - `Process(Request req)` calls each handler's `Handle` in order and returns the
              request.

            Implement one concrete handler, `AuditHandler`, that appends `"audited"` to the
            request's `Log`. The harness will add its *own* handler types (that it defines) and
            expect the pipeline to run them in order — with no change to `Pipeline`. This is
            Open/Closed applied to a processing chain: new stages are new classes, never edits
            to the runner.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            // PROVIDED — do not modify:
            public sealed class Request
            {
                public decimal Amount { get; set; }
                public List<string> Log { get; } = new();
            }

            public abstract class Handler
            {
                public abstract void Handle(Request req);
            }

            public sealed class Pipeline
            {
                // TODO: store handlers; Process runs them in order and returns req.
                public Pipeline(Handler[] handlers) => throw new System.NotImplementedException();
                public Request Process(Request req) => throw new System.NotImplementedException();
            }

            public sealed class AuditHandler : Handler
            {
                // TODO: append "audited" to req.Log.
                public override void Handle(Request req) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            using System.Collections.Generic;

            public static class __Harness
            {
                // Handlers the Pipeline has never seen — added without editing Pipeline.
                private sealed class TagHandler : Handler
                {
                    private readonly string _tag;
                    public TagHandler(string tag) => _tag = tag;
                    public override void Handle(Request req) => req.Log.Add(_tag);
                }

                private sealed class DoubleAmountHandler : Handler
                {
                    public override void Handle(Request req) => req.Amount *= 2;
                }

                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("provided AuditHandler runs", () =>
                    {
                        var req = new Pipeline(new Handler[] { new AuditHandler() }).Process(new Request());
                        Assert.Equal(1, req.Log.Count);
                        Assert.Equal("audited", req.Log[0]);
                    });
                    r.Check("runs new handler types in order", () =>
                    {
                        var pipe = new Pipeline(new Handler[] { new TagHandler("a"), new AuditHandler(), new TagHandler("b") });
                        var req = pipe.Process(new Request());
                        Assert.Equal(3, req.Log.Count);
                        Assert.Equal("a", req.Log[0]);
                        Assert.Equal("audited", req.Log[1]);
                        Assert.Equal("b", req.Log[2]);
                    });
                    r.Check("handlers mutate shared state across the chain", () =>
                    {
                        var pipe = new Pipeline(new Handler[] { new DoubleAmountHandler(), new DoubleAmountHandler() });
                        var req = pipe.Process(new Request { Amount = 5m });
                        Assert.Equal(20m, req.Amount);
                    });
                    r.Check("empty pipeline is a no-op", () =>
                    {
                        var req = new Pipeline(new Handler[0]).Process(new Request { Amount = 7m });
                        Assert.Equal(0, req.Log.Count);
                        Assert.Equal(7m, req.Amount);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public sealed class Request
            {
                public decimal Amount { get; set; }
                public List<string> Log { get; } = new();
            }

            public abstract class Handler
            {
                public abstract void Handle(Request req);
            }

            public sealed class Pipeline
            {
                private readonly Handler[] _handlers;
                public Pipeline(Handler[] handlers) => _handlers = handlers;

                // Never inspects concrete handler types -> closed. New stages are new
                // Handler subclasses, added without touching this loop.
                public Request Process(Request req)
                {
                    foreach (var h in _handlers)
                        h.Handle(req);
                    return req;
                }
            }

            public sealed class AuditHandler : Handler
            {
                public override void Handle(Request req) => req.Log.Add("audited");
            }
            """,
        Hints =
        [
            "Pipeline just loops the handlers in order and calls Handle — no type checks.",
            "Process returns the same Request after all handlers have mutated it.",
            "AuditHandler appends the literal \"audited\" to req.Log.",
            "New handler classes plugging in with zero Pipeline edits is the whole point.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "provided AuditHandler runs", IsHidden = false },
            new TestCaseSeed { Name = "runs new handler types in order", IsHidden = false },
            new TestCaseSeed { Name = "handlers mutate shared state across the chain", IsHidden = false },
            new TestCaseSeed { Name = "empty pipeline is a no-op", IsHidden = true },
        ],
    };

    // LSP (Medium/Hard): fix the Rectangle/Square trap so base-typed code stays correct.
    private static ExerciseSeed LspRectangleSquare => new()
    {
        Slug = "lsp-rectangle-square",
        Title = "LSP: Escape the Square Trap",
        Difficulty = "Hard",
        Kind = "Class",
        Prompt =
            """
            The classic Liskov violation: making `Square` a subclass of a mutable `Rectangle`
            breaks callers, because setting width independently of height is a promise a square
            can't keep. Fix it by modelling the shared *capability* instead of a false is-a.

            The provided read-only abstraction is:

                public abstract class Shape { public abstract int Area(); }

            Implement two substitutable shapes:
            - `Rectangle(int width, int height)` -> `Area()` = width * height.
            - `Square(int side)` -> `Area()` = side * side.

            The harness runs a base-typed routine `Shape s` over both and asserts the area
            contract holds for each — no surprises when used through `Shape`.
            """,
        StarterCode =
            """
            // PROVIDED — do not modify. A read-only contract every shape can honour,
            // so there are no independent setters to violate.
            public abstract class Shape { public abstract int Area(); }

            public sealed class Rectangle : Shape
            {
                // TODO
                public Rectangle(int width, int height) => throw new System.NotImplementedException();
                public override int Area() => throw new System.NotImplementedException();
            }

            public sealed class Square : Shape
            {
                // TODO
                public Square(int side) => throw new System.NotImplementedException();
                public override int Area() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                // Base-typed code: it only knows Shape, and must be correct for ANY subtype.
                private static int AreaOf(Shape s) => s.Area();

                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("rectangle via base type", () => Assert.Equal(20, AreaOf(new Rectangle(5, 4))));
                    r.Check("square via base type", () => Assert.Equal(16, AreaOf(new Square(4))));
                    r.Check("square honours its own invariant", () => Assert.Equal(49, new Square(7).Area()));
                    r.Check("both are usable through Shape[]", () =>
                    {
                        Shape[] shapes = { new Rectangle(2, 3), new Square(5) };
                        Assert.Equal(6, shapes[0].Area());
                        Assert.Equal(25, shapes[1].Area());
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public abstract class Shape { public abstract int Area(); }

            // No mutable width/height setters -> nothing for a subtype to lie about.
            public sealed class Rectangle : Shape
            {
                private readonly int _width, _height;
                public Rectangle(int width, int height) { _width = width; _height = height; }
                public override int Area() => _width * _height;
            }

            // Square is a peer, not a subclass of Rectangle: it can't break a promise it
            // never inherited. Used through Shape it's a perfect substitute.
            public sealed class Square : Shape
            {
                private readonly int _side;
                public Square(int side) => _side = side;
                public override int Area() => _side * _side;
            }
            """,
        Hints =
        [
            "The fix isn't in the math — it's in NOT making Square inherit Rectangle's setters.",
            "Both extend the read-only Shape independently, so neither can break the other.",
            "Rectangle.Area = width*height; Square.Area = side*side.",
            "Base-typed code (Shape s => s.Area()) must be correct for both — that's Liskov.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "rectangle via base type", IsHidden = false },
            new TestCaseSeed { Name = "square via base type", IsHidden = false },
            new TestCaseSeed { Name = "square honours its own invariant", IsHidden = false },
            new TestCaseSeed { Name = "both are usable through Shape[]", IsHidden = true },
        ],
    };

    // DIP (Medium/Hard): invert a policy to depend on an abstraction; harness injects a fake.
    private static ExerciseSeed DipOrderPolicy => new()
    {
        Slug = "dip-order-policy",
        Title = "DIP: Invert the Order Policy",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            A high-level policy, `OrderProcessor`, must not depend on a concrete database or
            notifier. Invert both dependencies onto abstractions so the harness can inject
            fakes and verify the policy's behaviour in isolation.

            Provided abstractions:

                public interface IOrderStore { void Save(int orderId); }
                public interface INotifier   { void Notify(string message); }

            Implement `OrderProcessor(IOrderStore store, INotifier notifier)` with a method
            `Place(int orderId)` that:
            1. saves the order via the store, then
            2. notifies with the exact message `"order {orderId} placed"`.

            The harness injects a fake store and a fake notifier and asserts both were driven
            correctly, in that order — proving the policy depends only on the abstractions.
            """,
        StarterCode =
            """
            // PROVIDED — do not modify:
            public interface IOrderStore { void Save(int orderId); }
            public interface INotifier { void Notify(string message); }

            public sealed class OrderProcessor
            {
                // TODO: depend on the two abstractions; Place saves then notifies.
                public OrderProcessor(IOrderStore store, INotifier notifier) => throw new System.NotImplementedException();
                public void Place(int orderId) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            using System.Collections.Generic;

            public static class __Harness
            {
                private sealed class FakeStore : IOrderStore
                {
                    public List<int> Saved { get; } = new();
                    public void Save(int orderId) => Saved.Add(orderId);
                }

                private sealed class FakeNotifier : INotifier
                {
                    public List<string> Messages { get; } = new();
                    public void Notify(string message) => Messages.Add(message);
                }

                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("saves the order via the injected store", () =>
                    {
                        var store = new FakeStore();
                        var notifier = new FakeNotifier();
                        new OrderProcessor(store, notifier).Place(42);
                        Assert.Equal(1, store.Saved.Count);
                        Assert.Equal(42, store.Saved[0]);
                    });
                    r.Check("notifies with the exact message", () =>
                    {
                        var store = new FakeStore();
                        var notifier = new FakeNotifier();
                        new OrderProcessor(store, notifier).Place(7);
                        Assert.Equal(1, notifier.Messages.Count);
                        Assert.Equal("order 7 placed", notifier.Messages[0]);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public interface IOrderStore { void Save(int orderId); }
            public interface INotifier { void Notify(string message); }

            public sealed class OrderProcessor
            {
                private readonly IOrderStore _store;      // abstractions, not concretions
                private readonly INotifier _notifier;

                public OrderProcessor(IOrderStore store, INotifier notifier)
                {
                    _store = store;
                    _notifier = notifier;
                }

                public void Place(int orderId)
                {
                    _store.Save(orderId);
                    _notifier.Notify($"order {orderId} placed");
                }
            }
            """,
        Hints =
        [
            "Store both injected interfaces in readonly fields.",
            "Place calls _store.Save first, then _notifier.Notify.",
            "The message is exactly \"order {orderId} placed\".",
            "Depending on interfaces is what lets the harness inject fakes for both.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "saves the order via the injected store", IsHidden = false },
            new TestCaseSeed { Name = "notifies with the exact message", IsHidden = false },
        ],
    };

    // Composition over inheritance (Hard): refactor behaviour into a swappable field.
    private static ExerciseSeed CompositionOverInheritance => new()
    {
        Slug = "composition-over-inheritance",
        Title = "Compose, Don't Inherit",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            A rigid inheritance chain (`Duck` -> `RubberDuck` -> `DecoyDuck` ...) forces every
            variation to be a new subclass. Refactor to composition: a single `Duck` that
            *holds* a swappable behaviour, so new behaviours are plugged in at construction
            time, not baked into a subclass.

            Provided abstraction:

                public interface IQuackBehavior { string Quack(); }

            Implement:
            - `QuackSound : IQuackBehavior` returning `"Quack"`.
            - `Mute : IQuackBehavior` returning `""` (empty string).
            - `Duck(IQuackBehavior behavior)` with `MakeSound()` delegating to the behavior,
              and `SetBehavior(IQuackBehavior behavior)` to swap it at runtime.

            The harness composes a Duck with different behaviours (including one it defines
            itself) and swaps behaviour on a live instance — no new Duck subclass anywhere.
            """,
        StarterCode =
            """
            // PROVIDED — do not modify:
            public interface IQuackBehavior { string Quack(); }

            public sealed class QuackSound : IQuackBehavior
            {
                public string Quack() => throw new System.NotImplementedException();
            }

            public sealed class Mute : IQuackBehavior
            {
                public string Quack() => throw new System.NotImplementedException();
            }

            public sealed class Duck
            {
                // TODO: hold a swappable behavior instead of subclassing per variation.
                public Duck(IQuackBehavior behavior) => throw new System.NotImplementedException();
                public string MakeSound() => throw new System.NotImplementedException();
                public void SetBehavior(IQuackBehavior behavior) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                // A behavior defined by the harness — composed in without a new Duck subclass.
                private sealed class Squeak : IQuackBehavior
                {
                    public string Quack() => "Squeak";
                }

                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("composed with Quack behavior", () =>
                        Assert.Equal("Quack", new Duck(new QuackSound()).MakeSound()));
                    r.Check("composed with Mute behavior", () =>
                        Assert.Equal("", new Duck(new Mute()).MakeSound()));
                    r.Check("composed with a harness-defined behavior", () =>
                        Assert.Equal("Squeak", new Duck(new Squeak()).MakeSound()));
                    r.Check("behavior can be swapped at runtime", () =>
                    {
                        var duck = new Duck(new QuackSound());
                        Assert.Equal("Quack", duck.MakeSound());
                        duck.SetBehavior(new Squeak());
                        Assert.Equal("Squeak", duck.MakeSound());
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public interface IQuackBehavior { string Quack(); }

            public sealed class QuackSound : IQuackBehavior
            {
                public string Quack() => "Quack";
            }

            public sealed class Mute : IQuackBehavior
            {
                public string Quack() => "";
            }

            public sealed class Duck
            {
                // Holds behaviour (composition) rather than being a behaviour (inheritance),
                // so variations plug in — and even change at runtime — with no new subclass.
                private IQuackBehavior _behavior;
                public Duck(IQuackBehavior behavior) => _behavior = behavior;
                public string MakeSound() => _behavior.Quack();
                public void SetBehavior(IQuackBehavior behavior) => _behavior = behavior;
            }
            """,
        Hints =
        [
            "Duck should have an IQuackBehavior field, not a subclass per sound.",
            "MakeSound just returns _behavior.Quack().",
            "SetBehavior reassigns the field, changing behaviour on a live instance.",
            "New sounds are new IQuackBehavior classes composed in — never new Duck subclasses.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "composed with Quack behavior", IsHidden = false },
            new TestCaseSeed { Name = "composed with Mute behavior", IsHidden = false },
            new TestCaseSeed { Name = "composed with a harness-defined behavior", IsHidden = false },
            new TestCaseSeed { Name = "behavior can be swapped at runtime", IsHidden = true },
        ],
    };
}
