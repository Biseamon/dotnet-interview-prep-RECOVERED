namespace InterviewPrep.Infrastructure.Data.Seeding.CleanCode;

// The "Clean Code" topic — small, behavior-graded exercises that drill the habits
// interviewers look for: guard clauses over nested ifs, returning expressions directly,
// and naming constants instead of magic numbers. Correctness is graded; the CLEAN
// technique is taught in the prompt/hints/explanation.
internal static class CleanCodeContent
{
    public static TopicSeed Topic => new()
    {
        Slug = "clean-code",
        Name = "Clean Code",
        Description = "Readable-code habits: guard clauses, returning booleans directly, and naming constants instead of magic numbers.",
        Order = 18,
        Lessons =
        [
            new LessonSeed
            {
                Slug = "readable-code", Title = "Readable Code Habits", Order = 1,
                MarkdownContent =
                    """
                    ## Readable Code Habits

                    Clean code reads like prose. Three high-leverage habits:
                    - **Guard clauses** — handle edge cases with early returns instead of deep
                      nesting, so the happy path stays flat.
                    - **Return expressions directly** — `return age >= 18;` beats `if (…) return
                      true; else return false;`.
                    - **Name your constants** — a named `FreeShippingThreshold` explains intent that
                      a bare `50` hides.
                    """,
                Exercises = [GuardClauses, SimplifyBoolean, NoMagicNumbers],
            },
        ],
    };

    private static ExerciseSeed GuardClauses => new()
    {
        Slug = "guard-clauses",
        Title = "Guard Clauses",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Implement `LetterGrade(score)` returning `"invalid"` for a score outside 0–100,
            else `"F"` (<60), `"D"` (<70), `"C"` (<80), `"B"` (<90), otherwise `"A"`. Use
            **guard clauses** — check the invalid case first and return early, keeping the rest
            flat and readable.
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: guard the invalid case first, then map ranges to letters.
                public static string LetterGrade(int score)
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
                    r.Check("-1 -> invalid", () => Assert.Equal("invalid", Solution.LetterGrade(-1)));
                    r.Check("55 -> F", () => Assert.Equal("F", Solution.LetterGrade(55)));
                    r.Check("85 -> B", () => Assert.Equal("B", Solution.LetterGrade(85)));
                    r.Check("100 -> A", () => Assert.Equal("A", Solution.LetterGrade(100)));
                    r.Check("101 -> invalid", () => Assert.Equal("invalid", Solution.LetterGrade(101)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static string LetterGrade(int score)
                {
                    if (score < 0 || score > 100) return "invalid"; // guard clause first
                    if (score < 60) return "F";
                    if (score < 70) return "D";
                    if (score < 80) return "C";
                    if (score < 90) return "B";
                    return "A";
                }
            }
            """,
        Hints =
        [
            "Return early for the invalid range so you don't nest the rest inside an if.",
            "Then a simple ladder of `if (score < …) return …` reads top to bottom.",
        ],
        TestCases =
        [
            new() { Name = "-1 -> invalid", IsHidden = false },
            new() { Name = "85 -> B", IsHidden = false },
        ],
    };

    private static ExerciseSeed SimplifyBoolean => new()
    {
        Slug = "return-boolean-directly",
        Title = "Return the Boolean Directly",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Implement `CanVote(age, isCitizen)` — true only if the person is **18 or older AND a
            citizen**. Return the boolean **expression directly** instead of an
            `if (…) return true; else return false;` — it's shorter and clearer.
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: return the condition directly (no if/else true/false).
                public static bool CanVote(int age, bool isCitizen)
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
                    r.Check("18 + citizen -> true", () => Assert.True(Solution.CanVote(18, true)));
                    r.Check("17 + citizen -> false", () => Assert.False(Solution.CanVote(17, true)));
                    r.Check("30 + non-citizen -> false", () => Assert.False(Solution.CanVote(30, false)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static bool CanVote(int age, bool isCitizen) => age >= 18 && isCitizen;
            }
            """,
        Hints =
        [
            "The whole method body is one expression: `age >= 18 && isCitizen`.",
            "No need for an if — the comparison already produces a bool.",
        ],
        TestCases =
        [
            new() { Name = "18 + citizen -> true", IsHidden = false },
            new() { Name = "17 + citizen -> false", IsHidden = false },
        ],
    };

    private static ExerciseSeed NoMagicNumbers => new()
    {
        Slug = "no-magic-numbers",
        Title = "Name Your Constants",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Implement `ShippingFee(orderTotal)`: free shipping (0) once the order reaches the
            **free-shipping threshold of 50**, otherwise a **flat fee of 5.99**. Name those
            values as constants instead of scattering the raw numbers — the names document intent.
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: name the threshold and fee as constants; return the right fee.
                public static decimal ShippingFee(decimal orderTotal)
                {
                    return 0m;
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
                    r.Check("below threshold -> flat fee", () => Assert.Equal(5.99m, Solution.ShippingFee(30m)));
                    r.Check("at threshold -> free", () => Assert.Equal(0m, Solution.ShippingFee(50m)));
                    r.Check("above threshold -> free", () => Assert.Equal(0m, Solution.ShippingFee(120m)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                private const decimal FreeShippingThreshold = 50m;
                private const decimal FlatFee = 5.99m;

                public static decimal ShippingFee(decimal orderTotal) =>
                    orderTotal >= FreeShippingThreshold ? 0m : FlatFee;
            }
            """,
        Hints =
        [
            "Declare `const decimal FreeShippingThreshold = 50m;` and `FlatFee = 5.99m;`.",
            "Return 0 when the total reaches the threshold, else the flat fee.",
            "Named constants make the rule obvious to the next reader.",
        ],
        TestCases =
        [
            new() { Name = "below threshold -> flat fee", IsHidden = false },
            new() { Name = "at threshold -> free", IsHidden = false },
        ],
    };
}
