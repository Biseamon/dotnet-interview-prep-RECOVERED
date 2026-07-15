namespace InterviewPrep.Infrastructure.Data.Seeding.Patterns;

// Behavioral patterns — how objects communicate: Strategy, Observer, Command.
internal static partial class DesignPatternsContent
{
    private static LessonSeed BehavioralLesson => new()
    {
        Slug = "behavioral-patterns",
        Title = "Behavioral Patterns",
        Order = 3,
        MarkdownContent =
            """
            ## Behavioral Patterns

            These govern how objects collaborate:
            - **Strategy** — swap an algorithm at runtime behind a common interface.
            - **Observer** — subjects notify subscribers of state changes (the basis of
              events / pub-sub).
            - **Command** — wrap an action as an object, enabling queues, logging, and undo.
            """,
        Exercises =
        [
            Strategy,
            Observer,
            Command,
            State,
            TemplateMethod,
            ChainOfResponsibility,
            Mediator,
            Iterator,
            Memento,
            Visitor,
            Interpreter,
        ],
    };

    private static ExerciseSeed Strategy => new()
    {
        Slug = "strategy",
        Title = "Strategy",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Given the `IDiscount` interface (provided), implement `NoDiscount`,
            `PercentOff` (constructed with a percentage), and a `Checkout.Total` that
            applies whichever strategy it's given. The algorithm is chosen at runtime.
            """,
        StarterCode =
            """
            // PROVIDED — do not modify:
            public interface IDiscount { decimal Apply(decimal price); }

            // TODO: two strategies + a Checkout that uses one.
            public sealed class NoDiscount : IDiscount
            {
                public decimal Apply(decimal price) => throw new System.NotImplementedException();
            }

            public sealed class PercentOff : IDiscount
            {
                public PercentOff(decimal percent) => throw new System.NotImplementedException();
                public decimal Apply(decimal price) => throw new System.NotImplementedException();
            }

            public static class Checkout
            {
                public static decimal Total(decimal price, IDiscount discount)
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
                    r.Check("NoDiscount leaves price unchanged", () =>
                        Assert.Equal(100m, Checkout.Total(100m, new NoDiscount())));
                    r.Check("20% off 100 -> 80", () =>
                        Assert.Equal(80m, Checkout.Total(100m, new PercentOff(20m))));
                    r.Check("10% off 50 -> 45", () =>
                        Assert.Equal(45m, Checkout.Total(50m, new PercentOff(10m))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public interface IDiscount { decimal Apply(decimal price); }

            public sealed class NoDiscount : IDiscount
            {
                public decimal Apply(decimal price) => price; // identity strategy
            }

            public sealed class PercentOff : IDiscount
            {
                private readonly decimal _percent;
                public PercentOff(decimal percent) => _percent = percent;
                public decimal Apply(decimal price) => price * (1 - _percent / 100m);
            }

            public static class Checkout
            {
                // The context is agnostic to which concrete strategy it uses.
                public static decimal Total(decimal price, IDiscount discount) => discount.Apply(price);
            }
            """,
        Hints =
        [
            "Each strategy is a small class implementing IDiscount.Apply.",
            "PercentOff stores its percentage and returns price * (1 - pct/100).",
            "Checkout.Total just delegates to discount.Apply(price) — it doesn't care which one.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "NoDiscount leaves price unchanged", IsHidden = false },
            new TestCaseSeed { Name = "20% off 100 -> 80", IsHidden = false },
            new TestCaseSeed { Name = "10% off 50 -> 45", IsHidden = true },
        ],
    };

    private static ExerciseSeed Observer => new()
    {
        Slug = "observer",
        Title = "Observer",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Implement a `Subject` that holds an int value and notifies subscribers when
            it changes. Support `Subscribe`, `Unsubscribe`, and `SetValue` (which pushes
            the new value to all current subscribers via the provided `IObserver`).
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            // PROVIDED — do not modify:
            public interface IObserver { void Update(int value); }

            public sealed class Subject
            {
                // TODO: track subscribers; SetValue notifies them all.
                public void Subscribe(IObserver o) => throw new System.NotImplementedException();
                public void Unsubscribe(IObserver o) => throw new System.NotImplementedException();
                public void SetValue(int value) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                // A spy observer that records the last value it was pushed.
                private sealed class Spy : IObserver
                {
                    public int Last = -1;
                    public void Update(int value) => Last = value;
                }

                public static string Run()
                {
                    var r = new HarnessReport();

                    r.Check("subscribers receive updates", () =>
                    {
                        var s = new Subject();
                        var spy = new Spy();
                        s.Subscribe(spy);
                        s.SetValue(5);
                        Assert.Equal(5, spy.Last);
                    });

                    r.Check("unsubscribed observers stop receiving", () =>
                    {
                        var s = new Subject();
                        var spy = new Spy();
                        s.Subscribe(spy);
                        s.SetValue(5);
                        s.Unsubscribe(spy);
                        s.SetValue(9);
                        Assert.Equal(5, spy.Last); // still 5, not 9
                    });

                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public interface IObserver { void Update(int value); }

            public sealed class Subject
            {
                private readonly List<IObserver> _observers = new();

                public void Subscribe(IObserver o) => _observers.Add(o);
                public void Unsubscribe(IObserver o) => _observers.Remove(o);

                public void SetValue(int value)
                {
                    // Notify every current subscriber. (ToArray guards against a
                    // subscriber mutating the list during iteration.)
                    foreach (var o in _observers.ToArray())
                        o.Update(value);
                }
            }
            """,
        Hints =
        [
            "Keep a List<IObserver> of subscribers.",
            "Subscribe/Unsubscribe add and remove from that list.",
            "SetValue iterates the list and calls Update(value) on each.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "subscribers receive updates", IsHidden = false },
            new TestCaseSeed { Name = "unsubscribed observers stop receiving", IsHidden = false },
        ],
    };

    private static ExerciseSeed Command => new()
    {
        Slug = "command",
        Title = "Command (with Undo)",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Using the provided `ICommand` (Execute/Undo) and `Light` receiver, implement
            `LightOnCommand` (turns the light on; undo turns it off) and a
            `RemoteControl` that runs commands and can `UndoLast()`. Commands-as-objects
            are what make undo/redo and queuing possible.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            // PROVIDED — do not modify:
            public interface ICommand { void Execute(); void Undo(); }
            public sealed class Light { public bool On { get; set; } }

            // TODO: a command that turns a Light on (undo turns it off).
            public sealed class LightOnCommand : ICommand
            {
                public LightOnCommand(Light light) => throw new System.NotImplementedException();
                public void Execute() => throw new System.NotImplementedException();
                public void Undo() => throw new System.NotImplementedException();
            }

            // TODO: runs commands and can undo the most recent.
            public sealed class RemoteControl
            {
                public void Press(ICommand command) => throw new System.NotImplementedException();
                public void UndoLast() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();

                    r.Check("pressing a command executes it", () =>
                    {
                        var light = new Light();
                        var remote = new RemoteControl();
                        remote.Press(new LightOnCommand(light));
                        Assert.True(light.On);
                    });

                    r.Check("undo reverses the last command", () =>
                    {
                        var light = new Light();
                        var remote = new RemoteControl();
                        remote.Press(new LightOnCommand(light));
                        remote.UndoLast();
                        Assert.False(light.On);
                    });

                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public interface ICommand { void Execute(); void Undo(); }
            public sealed class Light { public bool On { get; set; } }

            public sealed class LightOnCommand : ICommand
            {
                private readonly Light _light;      // the receiver the command acts on
                public LightOnCommand(Light light) => _light = light;
                public void Execute() => _light.On = true;
                public void Undo() => _light.On = false;
            }

            public sealed class RemoteControl
            {
                private readonly Stack<ICommand> _history = new(); // for undo

                public void Press(ICommand command)
                {
                    command.Execute();
                    _history.Push(command); // remember it so we can undo
                }

                public void UndoLast()
                {
                    if (_history.Count > 0)
                        _history.Pop().Undo();
                }
            }
            """,
        Hints =
        [
            "LightOnCommand holds the Light and flips On in Execute/Undo.",
            "RemoteControl.Press should Execute the command AND remember it.",
            "Store executed commands on a stack; UndoLast pops and calls Undo.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "pressing a command executes it", IsHidden = false },
            new TestCaseSeed { Name = "undo reverses the last command", IsHidden = false },
        ],
    };

    private static ExerciseSeed State => new()
    {
        Slug = "state",
        Title = "State",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            The State pattern lets an object change behaviour as its internal state
            changes — each state is its own object. Using the provided `IState`, implement
            `OffState` and `OnState` so that `Next()` toggles between them (`Off → On →
            Off`) and `Name` reports `"off"` / `"on"`.
            """,
        StarterCode =
            """
            // PROVIDED — do not modify:
            public interface IState { IState Next(); string Name { get; } }

            // TODO: two states that flip to each other on Next().
            public sealed class OffState : IState
            {
                public string Name => throw new System.NotImplementedException();
                public IState Next() => throw new System.NotImplementedException();
            }

            public sealed class OnState : IState
            {
                public string Name => throw new System.NotImplementedException();
                public IState Next() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("Off -> On -> Off", () =>
                    {
                        IState s = new OffState();
                        Assert.Equal("off", s.Name);
                        s = s.Next();
                        Assert.Equal("on", s.Name);
                        s = s.Next();
                        Assert.Equal("off", s.Name);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public interface IState { IState Next(); string Name { get; } }

            public sealed class OffState : IState
            {
                public string Name => "off";
                public IState Next() => new OnState(); // transition to the other state
            }

            public sealed class OnState : IState
            {
                public string Name => "on";
                public IState Next() => new OffState();
            }
            """,
        Hints =
        [
            "Each state is a class implementing IState.",
            "OffState.Next returns a new OnState, and vice versa.",
            "Name is just the state's label.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "Off -> On -> Off", IsHidden = false },
        ],
    };

    private static ExerciseSeed TemplateMethod => new()
    {
        Slug = "template-method",
        Title = "Template Method",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            The Template Method defines an algorithm's skeleton in a base class, letting
            subclasses fill in specific steps. Given the provided `Report` base (its
            `Generate()` is the template), implement `SalesReport` by overriding `Body()`
            to return `"sales"`. `Generate()` should then yield `"H|sales|F"`.
            """,
        StarterCode =
            """
            // PROVIDED — do not modify:
            public abstract class Report
            {
                // The template method: fixed skeleton, variable steps.
                public string Generate() => $"{Header()}|{Body()}|{Footer()}";
                protected virtual string Header() => "H";
                protected abstract string Body();
                protected virtual string Footer() => "F";
            }

            // TODO: fill in the one varying step.
            public sealed class SalesReport : Report
            {
                protected override string Body() => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("template assembles the report", () =>
                        Assert.Equal("H|sales|F", new SalesReport().Generate()));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public abstract class Report
            {
                public string Generate() => $"{Header()}|{Body()}|{Footer()}";
                protected virtual string Header() => "H";
                protected abstract string Body();
                protected virtual string Footer() => "F";
            }

            public sealed class SalesReport : Report
            {
                // Only the varying step is overridden; the skeleton stays in the base.
                protected override string Body() => "sales";
            }
            """,
        Hints =
        [
            "You only need to override the abstract Body() step.",
            "The base class's Generate() already sequences Header/Body/Footer.",
            "Return \"sales\" from Body().",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "template assembles the report", IsHidden = false },
        ],
    };

    private static ExerciseSeed ChainOfResponsibility => new()
    {
        Slug = "chain-of-responsibility",
        Title = "Chain of Responsibility",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Requests pass along a chain until a handler accepts one. Using the provided
            `Approver` base (with `Next`/`SetNext`), implement `TeamLead` (approves
            amounts ≤ 10 → `"team"`), `Manager` (≤ 100 → `"manager"`), and `Director`
            (anything → `"director"`). Each delegates to `Next` when it can't approve.
            """,
        StarterCode =
            """
            // PROVIDED — do not modify:
            public abstract class Approver
            {
                public Approver? Next { get; private set; }
                public Approver SetNext(Approver next) { Next = next; return next; }
                public abstract string Approve(int amount);
            }

            // TODO: three approvers, each handling its limit or passing to Next.
            public sealed class TeamLead : Approver
            {
                public override string Approve(int amount) => throw new System.NotImplementedException();
            }
            public sealed class Manager : Approver
            {
                public override string Approve(int amount) => throw new System.NotImplementedException();
            }
            public sealed class Director : Approver
            {
                public override string Approve(int amount) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                private static Approver BuildChain()
                {
                    var lead = new TeamLead();
                    lead.SetNext(new Manager()).SetNext(new Director());
                    return lead;
                }

                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("5 -> team", () => Assert.Equal("team", BuildChain().Approve(5)));
                    r.Check("50 -> manager", () => Assert.Equal("manager", BuildChain().Approve(50)));
                    r.Check("500 -> director", () => Assert.Equal("director", BuildChain().Approve(500)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public abstract class Approver
            {
                public Approver? Next { get; private set; }
                public Approver SetNext(Approver next) { Next = next; return next; }
                public abstract string Approve(int amount);
            }

            public sealed class TeamLead : Approver
            {
                public override string Approve(int amount) =>
                    amount <= 10 ? "team" : Next!.Approve(amount); // handle or pass on
            }
            public sealed class Manager : Approver
            {
                public override string Approve(int amount) =>
                    amount <= 100 ? "manager" : Next!.Approve(amount);
            }
            public sealed class Director : Approver
            {
                public override string Approve(int amount) => "director"; // end of chain
            }
            """,
        Hints =
        [
            "Each handler checks its own limit first.",
            "If it can't approve, delegate to Next.Approve(amount).",
            "Director is the terminal handler — it always approves.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "5 -> team", IsHidden = false },
            new TestCaseSeed { Name = "50 -> manager", IsHidden = false },
            new TestCaseSeed { Name = "500 -> director", IsHidden = false },
        ],
    };

    private static ExerciseSeed Mediator => new()
    {
        Slug = "mediator",
        Title = "Mediator",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            A Mediator centralizes communication so colleagues don't reference each other
            directly. Implement a `ChatRoom` where `Register(colleague)` adds a participant
            and `Broadcast(sender, message)` delivers the message to **every colleague
            except the sender**. (`Colleague` is provided with a `Received` list.)
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            // PROVIDED — do not modify:
            public sealed class Colleague
            {
                public string Name { get; }
                public List<string> Received { get; } = new();
                public Colleague(string name) => Name = name;
            }

            public sealed class ChatRoom
            {
                // TODO: register participants; broadcast to everyone but the sender.
                public void Register(Colleague c) => throw new System.NotImplementedException();
                public void Broadcast(Colleague sender, string message) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("broadcast reaches others, not the sender", () =>
                    {
                        var room = new ChatRoom();
                        var a = new Colleague("a");
                        var b = new Colleague("b");
                        var c = new Colleague("c");
                        room.Register(a); room.Register(b); room.Register(c);
                        room.Broadcast(a, "hi");
                        Assert.Equal(0, a.Received.Count);        // sender excluded
                        Assert.True(b.Received.Contains("hi"));
                        Assert.True(c.Received.Contains("hi"));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public sealed class Colleague
            {
                public string Name { get; }
                public List<string> Received { get; } = new();
                public Colleague(string name) => Name = name;
            }

            public sealed class ChatRoom
            {
                private readonly List<Colleague> _members = new();
                public void Register(Colleague c) => _members.Add(c);

                public void Broadcast(Colleague sender, string message)
                {
                    // The mediator knows all members; colleagues never reference each other.
                    foreach (var m in _members)
                        if (!ReferenceEquals(m, sender))
                            m.Received.Add(message);
                }
            }
            """,
        Hints =
        [
            "The ChatRoom holds the list of registered colleagues.",
            "Broadcast iterates members and appends the message to each Received list.",
            "Skip the sender with a reference comparison.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "broadcast reaches others, not the sender", IsHidden = false },
        ],
    };

    private static ExerciseSeed Iterator => new()
    {
        Slug = "iterator",
        Title = "Iterator",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            The Iterator pattern exposes sequential access without revealing the underlying
            structure. In C#, `yield return` builds an iterator for you. Implement
            `Numbers(start, count)` returning an `IEnumerable<int>` of `count` consecutive
            integers beginning at `start`.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: yield `count` integers starting from `start`.
                public static IEnumerable<int> Numbers(int start, int count)
                {
                    yield break;
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
                    r.Check("Numbers(5,3) -> 5,6,7", () =>
                        Assert.Equal("5,6,7", string.Join(",", Solution.Numbers(5, 3))));
                    r.Check("Numbers(0,0) -> empty", () =>
                        Assert.Equal("", string.Join(",", Solution.Numbers(0, 0))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                // `yield return` builds a lazy iterator (a state machine) automatically.
                public static IEnumerable<int> Numbers(int start, int count)
                {
                    for (int i = 0; i < count; i++)
                        yield return start + i;
                }
            }
            """,
        Hints =
        [
            "`yield return` produces one element at a time.",
            "Loop `count` times, yielding start + i.",
            "No elements yielded (count 0) gives an empty sequence.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "Numbers(5,3) -> 5,6,7", IsHidden = false },
            new TestCaseSeed { Name = "Numbers(0,0) -> empty", IsHidden = true },
        ],
    };

    private static ExerciseSeed Memento => new()
    {
        Slug = "memento",
        Title = "Memento",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            The Memento pattern captures an object's state so it can be restored later
            (undo). Implement an `Editor`: `Type(text)` appends, `Content` reads, `Save()`
            returns an opaque memento of the current state, and `Restore(memento)` rolls
            back to it.
            """,
        StarterCode =
            """
            public sealed class Editor
            {
                private string _content = "";

                public void Type(string text) => _content += text;
                public string Content => _content;

                // TODO: capture and restore state via an opaque memento object.
                public object Save() => throw new System.NotImplementedException();
                public void Restore(object memento) => throw new System.NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("restore rolls back to a saved point", () =>
                    {
                        var editor = new Editor();
                        editor.Type("hello");
                        var snapshot = editor.Save();
                        editor.Type(" world");
                        Assert.Equal("hello world", editor.Content);
                        editor.Restore(snapshot);
                        Assert.Equal("hello", editor.Content);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public sealed class Editor
            {
                private string _content = "";

                public void Type(string text) => _content += text;
                public string Content => _content;

                // The memento holds the captured state; its internals are opaque to callers.
                public object Save() => new Memento(_content);
                public void Restore(object memento) => _content = ((Memento)memento).State;

                private sealed record Memento(string State);
            }
            """,
        Hints =
        [
            "Save should snapshot the current content into a small object.",
            "A private record holding the string works well as the memento.",
            "Restore casts the memento back and reassigns the content.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "restore rolls back to a saved point", IsHidden = false },
        ],
    };

    private static ExerciseSeed Visitor => new()
    {
        Slug = "visitor",
        Title = "Visitor",
        Difficulty = "Hard",
        Kind = "Class",
        Prompt =
            """
            The Visitor pattern adds operations to a type hierarchy without modifying it,
            via **double dispatch**. Given the provided `Shape`/`Circle`/`Square` (each with
            `Accept`) and `IShapeVisitor`, implement an `AreaVisitor` computing each shape's
            area (`Circle` → πr², `Square` → s²).
            """,
        StarterCode =
            """
            using System;

            // PROVIDED — do not modify:
            public interface IShapeVisitor { double Visit(Circle c); double Visit(Square s); }
            public abstract class Shape { public abstract double Accept(IShapeVisitor v); }
            public sealed class Circle : Shape
            {
                public double Radius { get; }
                public Circle(double radius) => Radius = radius;
                public override double Accept(IShapeVisitor v) => v.Visit(this);
            }
            public sealed class Square : Shape
            {
                public double Side { get; }
                public Square(double side) => Side = side;
                public override double Accept(IShapeVisitor v) => v.Visit(this);
            }

            // TODO: implement the visitor that computes areas.
            public sealed class AreaVisitor : IShapeVisitor
            {
                public double Visit(Circle c) => throw new NotImplementedException();
                public double Visit(Square s) => throw new NotImplementedException();
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
                    var visitor = new AreaVisitor();
                    r.Check("circle area via visitor", () =>
                        Assert.True(Math.Abs(new Circle(2).Accept(visitor) - Math.PI * 4) < 1e-9));
                    r.Check("square area via visitor", () =>
                        Assert.True(Math.Abs(new Square(3).Accept(visitor) - 9) < 1e-9));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public interface IShapeVisitor { double Visit(Circle c); double Visit(Square s); }
            public abstract class Shape { public abstract double Accept(IShapeVisitor v); }
            public sealed class Circle : Shape
            {
                public double Radius { get; }
                public Circle(double radius) => Radius = radius;
                public override double Accept(IShapeVisitor v) => v.Visit(this);
            }
            public sealed class Square : Shape
            {
                public double Side { get; }
                public Square(double side) => Side = side;
                public override double Accept(IShapeVisitor v) => v.Visit(this);
            }

            public sealed class AreaVisitor : IShapeVisitor
            {
                // Each Visit overload knows the concrete type — that's the double dispatch:
                // shape.Accept(v) calls v.Visit(concreteShape).
                public double Visit(Circle c) => Math.PI * c.Radius * c.Radius;
                public double Visit(Square s) => s.Side * s.Side;
            }
            """,
        Hints =
        [
            "The shapes already call the matching Visit overload in Accept.",
            "You only implement the two Visit methods.",
            "Compute πr² for Circle and s² for Square.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "circle area via visitor", IsHidden = false },
            new TestCaseSeed { Name = "square area via visitor", IsHidden = false },
        ],
    };

    private static ExerciseSeed Interpreter => new()
    {
        Slug = "interpreter",
        Title = "Interpreter",
        Difficulty = "Hard",
        Kind = "Class",
        Prompt =
            """
            The Interpreter pattern evaluates sentences in a little language, one node type
            per grammar rule. Given `IExpr` with `Eval(context)`, implement `Var` (looks up
            a variable), `And` (both true), and `Not` (negates) for boolean expressions.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            // PROVIDED — do not modify:
            public interface IExpr { bool Eval(Dictionary<string, bool> context); }

            // TODO: three expression node types.
            public sealed class Var : IExpr
            {
                public Var(string name) => throw new System.NotImplementedException();
                public bool Eval(Dictionary<string, bool> context) => throw new System.NotImplementedException();
            }
            public sealed class And : IExpr
            {
                public And(IExpr left, IExpr right) => throw new System.NotImplementedException();
                public bool Eval(Dictionary<string, bool> context) => throw new System.NotImplementedException();
            }
            public sealed class Not : IExpr
            {
                public Not(IExpr inner) => throw new System.NotImplementedException();
                public bool Eval(Dictionary<string, bool> context) => throw new System.NotImplementedException();
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
                    var ctx = new Dictionary<string, bool> { ["x"] = true, ["y"] = false };
                    r.Check("x AND (NOT y) -> true", () =>
                    {
                        IExpr expr = new And(new Var("x"), new Not(new Var("y")));
                        Assert.True(expr.Eval(ctx));
                    });
                    r.Check("x AND y -> false", () =>
                    {
                        IExpr expr = new And(new Var("x"), new Var("y"));
                        Assert.False(expr.Eval(ctx));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public interface IExpr { bool Eval(Dictionary<string, bool> context); }

            public sealed class Var : IExpr
            {
                private readonly string _name;
                public Var(string name) => _name = name;
                public bool Eval(Dictionary<string, bool> context) => context[_name];
            }
            public sealed class And : IExpr
            {
                private readonly IExpr _left, _right;
                public And(IExpr left, IExpr right) { _left = left; _right = right; }
                public bool Eval(Dictionary<string, bool> context) => _left.Eval(context) && _right.Eval(context);
            }
            public sealed class Not : IExpr
            {
                private readonly IExpr _inner;
                public Not(IExpr inner) => _inner = inner;
                public bool Eval(Dictionary<string, bool> context) => !_inner.Eval(context);
            }
            """,
        Hints =
        [
            "Each node stores its operands and implements Eval.",
            "Var looks its name up in the context dictionary.",
            "And/Not evaluate their sub-expressions recursively, then combine.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "x AND (NOT y) -> true", IsHidden = false },
            new TestCaseSeed { Name = "x AND y -> false", IsHidden = false },
        ],
    };
}
