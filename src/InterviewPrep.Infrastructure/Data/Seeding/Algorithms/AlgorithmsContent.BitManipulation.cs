namespace InterviewPrep.Infrastructure.Data.Seeding.Algorithms;

// Lesson 11 — Bit Manipulation. A NeetCode-150 category that's a common interview
// staple: XOR tricks, counting bits, and the missing-number sum trick.
internal static partial class AlgorithmsContent
{
    private static LessonSeed BitManipulationLesson => new()
    {
        Slug = "bit-manipulation",
        Title = "Bit Manipulation",
        Order = 11,
        MarkdownContent =
            """
            ## Bit Manipulation

            Work directly on the binary bits of a number. Key tricks:
            - **XOR** (`^`): `x ^ x = 0` and `x ^ 0 = x` — so XOR-ing everything cancels pairs.
            - **`n & (n - 1)`** clears the lowest set bit — count set bits by repeating it.
            - **`n & 1`** checks the last bit; **`n >> 1`** drops it.
            """,
        Exercises = [SingleNumber, NumberOfOneBits, CountingBits, MissingNumber],
    };

    private static ExerciseSeed SingleNumber => new()
    {
        Slug = "single-number",
        Title = "Single Number",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Every value appears twice except one. Find the single one in **O(n) time and O(1)
            space**. XOR is magic here: pairs cancel to 0, leaving the loner.
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: XOR everything together.
                public static int SingleNumber(int[] nums)
                {
                    return 0;
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
                    r.Check("[2,2,1] -> 1", () => Assert.Equal(1, Solution.SingleNumber(new[]{2,2,1})));
                    r.Check("[4,1,2,1,2] -> 4", () => Assert.Equal(4, Solution.SingleNumber(new[]{4,1,2,1,2})));
                    r.Check("[7] -> 7", () => Assert.Equal(7, Solution.SingleNumber(new[]{7})));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static int SingleNumber(int[] nums)
                {
                    int result = 0;
                    foreach (var n in nums) result ^= n; // pairs cancel, loner remains
                    return result;
                }
            }
            """,
        Hints =
        [
            "XOR of a number with itself is 0.",
            "XOR everything together — the duplicates cancel out.",
        ],
        TestCases =
        [
            new() { Name = "[2,2,1] -> 1", IsHidden = false },
            new() { Name = "[4,1,2,1,2] -> 4", IsHidden = false },
        ],
    };

    private static ExerciseSeed NumberOfOneBits => new()
    {
        Slug = "number-of-one-bits",
        Title = "Number of 1 Bits",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Count the set bits (1s) in an unsigned 32-bit integer (the "Hamming weight"). The
            trick `n & (n - 1)` clears the lowest set bit, so loop until n is 0.
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: count set bits, e.g. with n & (n-1).
                public static int HammingWeight(uint n)
                {
                    return 0;
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
                    r.Check("11 (1011) -> 3", () => Assert.Equal(3, Solution.HammingWeight(11)));
                    r.Check("128 (10000000) -> 1", () => Assert.Equal(1, Solution.HammingWeight(128)));
                    r.Check("0 -> 0", () => Assert.Equal(0, Solution.HammingWeight(0)));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static int HammingWeight(uint n)
                {
                    int count = 0;
                    while (n != 0)
                    {
                        n &= n - 1;  // clears the lowest set bit
                        count++;
                    }
                    return count;
                }
            }
            """,
        Hints =
        [
            "`n & (n - 1)` removes the lowest 1 bit.",
            "Count how many times you can do that before n hits 0.",
        ],
        TestCases =
        [
            new() { Name = "11 (1011) -> 3", IsHidden = false },
            new() { Name = "128 (10000000) -> 1", IsHidden = false },
        ],
    };

    private static ExerciseSeed CountingBits => new()
    {
        Slug = "counting-bits",
        Title = "Counting Bits",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Return an array where `result[i]` is the number of set bits in `i`, for `i` from 0
            to `n`. DP trick: `bits[i] = bits[i >> 1] + (i & 1)` (bits of i/2, plus i's last bit).
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: bits[i] = bits[i >> 1] + (i & 1).
                public static int[] CountBits(int n)
                {
                    return new int[0];
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
                    r.Check("n=5 -> 0,1,1,2,1,2", () =>
                        Assert.Equal("0,1,1,2,1,2", string.Join(",", Solution.CountBits(5))));
                    r.Check("n=2 -> 0,1,1", () =>
                        Assert.Equal("0,1,1", string.Join(",", Solution.CountBits(2))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static int[] CountBits(int n)
                {
                    var bits = new int[n + 1];
                    for (int i = 1; i <= n; i++)
                        bits[i] = bits[i >> 1] + (i & 1); // half's bits + this last bit
                    return bits;
                }
            }
            """,
        Hints =
        [
            "bits[0] = 0.",
            "i >> 1 is i without its last bit; add back (i & 1) for that bit.",
            "Fill the array left to right using earlier answers.",
        ],
        TestCases =
        [
            new() { Name = "n=5 -> 0,1,1,2,1,2", IsHidden = false },
            new() { Name = "n=2 -> 0,1,1", IsHidden = false },
        ],
    };

    private static ExerciseSeed MissingNumber => new()
    {
        Slug = "missing-number",
        Title = "Missing Number",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Given `n` distinct numbers taken from `0..n` (so exactly one is missing), find it.
            Sum trick: the full sum is `n(n+1)/2`; subtract the actual sum. (XOR also works.)
            """,
        StarterCode =
            """
            public static class Solution
            {
                // TODO: expected total minus actual total.
                public static int MissingNumber(int[] nums)
                {
                    return 0;
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
                    r.Check("[3,0,1] -> 2", () => Assert.Equal(2, Solution.MissingNumber(new[]{3,0,1})));
                    r.Check("[0,1] -> 2", () => Assert.Equal(2, Solution.MissingNumber(new[]{0,1})));
                    r.Check("[9,6,4,2,3,5,7,0,1] -> 8", () =>
                        Assert.Equal(8, Solution.MissingNumber(new[]{9,6,4,2,3,5,7,0,1})));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            public static class Solution
            {
                public static int MissingNumber(int[] nums)
                {
                    int n = nums.Length;
                    int expected = n * (n + 1) / 2;
                    int actual = 0;
                    foreach (var x in nums) actual += x;
                    return expected - actual;
                }
            }
            """,
        Hints =
        [
            "The numbers should be 0..n; their full sum is n*(n+1)/2.",
            "Subtract the actual sum to reveal the missing value.",
        ],
        TestCases =
        [
            new() { Name = "[3,0,1] -> 2", IsHidden = false },
            new() { Name = "[9,6,4,2,3,5,7,0,1] -> 8", IsHidden = false },
        ],
    };
}
