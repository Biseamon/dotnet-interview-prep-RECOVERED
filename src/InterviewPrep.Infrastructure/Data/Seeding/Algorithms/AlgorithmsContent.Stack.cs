namespace InterviewPrep.Infrastructure.Data.Seeding.Algorithms;

// Lesson 3 — Stacks. LIFO structure for matching/nesting problems and for
// designing data structures with O(1) auxiliary queries.
internal static partial class AlgorithmsContent
{
    private static LessonSeed StackLesson => new()
    {
        Slug = "stacks",
        Title = "Stacks",
        Order = 3,
        MarkdownContent =
            """
            ## Stacks

            A **stack** (LIFO) shines when the most recent unmatched thing is what you
            need next: matching brackets, evaluating expressions, and "next greater
            element" style problems. In C#: `Stack<T>` with `Push`, `Pop`, `Peek`.
            """,
        Exercises =
        [
            ValidParentheses,
            MinStack,
            EvalRpn,
        ],
    };

    private static ExerciseSeed ValidParentheses => new()
    {
        Slug = "valid-parentheses",
        Title = "Valid Parentheses",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Given a string of `()[]{}`, return `true` if every bracket is closed by the
            matching type in the correct order. Push opens onto a stack; on a close,
            the top must be its partner.
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: use a stack of expected closing brackets.
                public static bool IsValid(string s)
                {
                    throw new NotImplementedException();
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
                    r.Check("'()[]{}' -> true", () => Assert.True(Solution.IsValid("()[]{}")));
                    r.Check("'(]' -> false", () => Assert.False(Solution.IsValid("(]")));
                    r.Check("'([)]' -> false", () => Assert.False(Solution.IsValid("([)]")));
                    r.Check("'{[]}' -> true", () => Assert.True(Solution.IsValid("{[]}")));
                    r.Check("'(' -> false", () => Assert.False(Solution.IsValid("(")));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                public static bool IsValid(string s)
                {
                    var stack = new Stack<char>();
                    var pairs = new Dictionary<char, char> { [')'] = '(', [']'] = '[', ['}'] = '{' };
                    foreach (var c in s)
                    {
                        if (pairs.ContainsKey(c))
                        {
                            // Closing bracket: top must be the matching opener.
                            if (stack.Count == 0 || stack.Pop() != pairs[c]) return false;
                        }
                        else stack.Push(c); // opening bracket
                    }
                    return stack.Count == 0; // nothing left unmatched
                }
            }
            """,
        Hints =
        [
            "Push opening brackets onto a stack.",
            "On a closing bracket, the top of the stack must be its matching opener.",
            "At the end the stack must be empty, or some opener was never closed.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "'()[]{}' -> true", IsHidden = false },
            new TestCaseSeed { Name = "'([)]' -> false", IsHidden = false },
            new TestCaseSeed { Name = "'(' -> false", IsHidden = true },
        ],
    };

    private static ExerciseSeed MinStack => new()
    {
        Slug = "min-stack",
        Title = "Min Stack",
        Difficulty = "Medium",
        Kind = "Class",
        Prompt =
            """
            Design a `MinStack` supporting `Push(int)`, `Pop()`, `Top()`, and
            `GetMin()` — all in **O(1)**. Trick: alongside the values, keep a second
            stack of the running minimum.
            """,
        StarterCode =
            """
            using System;

            public class MinStack
            {
                // TODO: implement all four in O(1). Hint: a second stack of minimums.
                public MinStack() { }
                public void Push(int val) => throw new NotImplementedException();
                public void Pop() => throw new NotImplementedException();
                public int Top() => throw new NotImplementedException();
                public int GetMin() => throw new NotImplementedException();
            }
            """,
        HarnessCode =
            """
            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("push -2,0,-3; GetMin=-3", () =>
                    {
                        var s = new MinStack();
                        s.Push(-2); s.Push(0); s.Push(-3);
                        Assert.Equal(-3, s.GetMin());
                    });
                    r.Check("after pop; GetMin=-2, Top=0", () =>
                    {
                        var s = new MinStack();
                        s.Push(-2); s.Push(0); s.Push(-3);
                        s.Pop();
                        Assert.Equal(0, s.Top());
                        Assert.Equal(-2, s.GetMin());
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public class MinStack
            {
                private readonly Stack<int> _values = new();
                private readonly Stack<int> _mins = new(); // running minimum, parallel to _values

                public MinStack() { }

                public void Push(int val)
                {
                    _values.Push(val);
                    // The new min is the smaller of val and the previous min.
                    _mins.Push(_mins.Count == 0 ? val : System.Math.Min(val, _mins.Peek()));
                }

                public void Pop() { _values.Pop(); _mins.Pop(); }
                public int Top() => _values.Peek();
                public int GetMin() => _mins.Peek();
            }
            """,
        Hints =
        [
            "GetMin must be O(1) — you can't scan the stack each call.",
            "Keep a second stack that stores the minimum-so-far at each level.",
            "On Push, push min(val, currentMin). On Pop, pop both stacks together.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "push -2,0,-3; GetMin=-3", IsHidden = false },
            new TestCaseSeed { Name = "after pop; GetMin=-2, Top=0", IsHidden = false },
        ],
    };

    private static ExerciseSeed EvalRpn => new()
    {
        Slug = "eval-rpn",
        Title = "Evaluate Reverse Polish Notation",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Evaluate an arithmetic expression in **Reverse Polish Notation** (operators
            follow their operands). Tokens are integers and `+ - * /` (integer division,
            truncating toward zero). Push numbers; on an operator, pop two and combine.
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: stack-based RPN evaluation.
                public static int EvalRPN(string[] tokens)
                {
                    throw new NotImplementedException();
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
                    r.Check("['2','1','+','3','*'] -> 9", () =>
                        Assert.Equal(9, Solution.EvalRPN(new[]{"2","1","+","3","*"})));
                    r.Check("['4','13','5','/','+'] -> 6", () =>
                        Assert.Equal(6, Solution.EvalRPN(new[]{"4","13","5","/","+"})));
                    r.Check("['10','6','9','3','+','-11','*','/','*','17','+','5','+'] -> 22", () =>
                        Assert.Equal(22, Solution.EvalRPN(new[]{"10","6","9","3","+","-11","*","/","*","17","+","5","+"})));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Collections.Generic;

            public static class Solution
            {
                public static int EvalRPN(string[] tokens)
                {
                    var stack = new Stack<int>();
                    foreach (var t in tokens)
                    {
                        if (t is "+" or "-" or "*" or "/")
                        {
                            // Order matters for - and /: b is the SECOND operand.
                            int b = stack.Pop(), a = stack.Pop();
                            stack.Push(t switch
                            {
                                "+" => a + b,
                                "-" => a - b,
                                "*" => a * b,
                                _   => a / b,
                            });
                        }
                        else stack.Push(int.Parse(t));
                    }
                    return stack.Pop();
                }
            }
            """,
        Hints =
        [
            "Push integer tokens onto a stack.",
            "On an operator, pop the top two values and push the result.",
            "Mind the order: for a - b and a / b, the first popped value is the right operand.",
        ],
        TestCases =
        [
            new TestCaseSeed { Name = "['2','1','+','3','*'] -> 9", IsHidden = false },
            new TestCaseSeed { Name = "['4','13','5','/','+'] -> 6", IsHidden = false },
        ],
    };
}
