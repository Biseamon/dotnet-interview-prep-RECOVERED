namespace InterviewPrep.Infrastructure.Data.Seeding.Ai;

// Lesson 3 for AI/LLM — retrieval ranking and decoding: softmax, greedy decoding,
// keyword retrieval, a whitespace tokenizer, and Jaccard similarity. All plain C#.
internal static partial class AiContent
{
    private static LessonSeed RetrievalDecodingLesson => new()
    {
        Slug = "retrieval-decoding",
        Title = "Retrieval & Decoding",
        Order = 3,
        MarkdownContent =
            """
            ## Retrieval & Decoding

            **Softmax** turns raw scores (logits) into probabilities. **Greedy decoding**
            keeps picking the most likely next token until a stop token. On the retrieval
            side, a **tokenizer** turns text into ids, **keyword ranking** finds the most
            relevant document, and **Jaccard similarity** measures overlap (great for
            near-duplicate detection).
            """,
        Exercises = [Softmax, GreedyDecode, KeywordSearch, Tokenizer, Jaccard],
    };

    private static ExerciseSeed Softmax => new()
    {
        Slug = "softmax",
        Title = "Softmax",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Implement `Softmax(logits)` — turn raw scores into probabilities that are all
            positive and sum to 1: `exp(x_i) / Σ exp(x_j)`. Subtract the max first for
            numerical stability. This is how logits become a probability distribution.
            """,
        StarterCode =
            """
            using System;
            using System.Linq;

            public static class Solution
            {
                // TODO: exp(each - max) / sum(exp(... )).
                public static double[] Softmax(double[] logits)
                {
                    return new double[0];
                }
            }
            """,
        HarnessCode =
            """
            using System;
            using System.Linq;

            public static class __Harness
            {
                public static string Run()
                {
                    var r = new HarnessReport();
                    r.Check("probabilities sum to 1", () =>
                        Assert.True(Math.Abs(Solution.Softmax(new[]{1.0,2,3}).Sum() - 1) < 1e-9));
                    r.Check("equal logits -> uniform", () =>
                    {
                        var p = Solution.Softmax(new[]{0.0,0,0});
                        Assert.True(Math.Abs(p[0] - 1.0/3) < 1e-9);
                    });
                    r.Check("bigger logit -> bigger probability", () =>
                    {
                        var p = Solution.Softmax(new[]{1.0,2.0});
                        Assert.True(p[1] > p[0]);
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Linq;

            public static class Solution
            {
                public static double[] Softmax(double[] logits)
                {
                    double max = logits.Max();                       // stability shift
                    var exps = logits.Select(x => Math.Exp(x - max)).ToArray();
                    double sum = exps.Sum();
                    return exps.Select(e => e / sum).ToArray();
                }
            }
            """,
        Hints =
        [
            "Subtract the max logit from each before exp (avoids overflow, same result).",
            "Exponentiate each, then divide by the total.",
            "The outputs are positive and sum to 1.",
        ],
        TestCases =
        [
            new() { Name = "probabilities sum to 1", IsHidden = false },
            new() { Name = "equal logits -> uniform", IsHidden = false },
        ],
    };

    private static ExerciseSeed GreedyDecode => new()
    {
        Slug = "greedy-decode",
        Title = "Greedy Decoding",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Implement `Decode(steps, stopToken)`: at each step you get the logits for that
            position; pick the **argmax** token (index of the highest logit). Stop when you
            pick `stopToken` (don't include it) or run out of steps. Return the chosen tokens.
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: pick argmax at each step until stopToken (exclusive) or end.
                public static int[] Decode(double[][] steps, int stopToken)
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
                    r.Check("picks argmax each step, stops at stop token", () =>
                    {
                        // 3 vocab tokens; token 2 is the stop token.
                        var steps = new[]
                        {
                            new[]{0.1, 0.9, 0.0}, // -> 1
                            new[]{0.8, 0.1, 0.0}, // -> 0
                            new[]{0.0, 0.0, 1.0}, // -> 2 (stop)
                            new[]{1.0, 0.0, 0.0}, // never reached
                        };
                        Assert.Equal("1,0", string.Join(",", Solution.Decode(steps, 2)));
                    });
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
                public static int[] Decode(double[][] steps, int stopToken)
                {
                    var output = new List<int>();
                    foreach (var logits in steps)
                    {
                        int best = 0;
                        for (int i = 1; i < logits.Length; i++)
                            if (logits[i] > logits[best]) best = i;
                        if (best == stopToken) break;
                        output.Add(best);
                    }
                    return output.ToArray();
                }
            }
            """,
        Hints =
        [
            "For each step, find the index of the largest logit (argmax).",
            "If that index is the stop token, stop without adding it.",
            "Otherwise append it and continue.",
        ],
        TestCases = [new() { Name = "picks argmax each step, stops at stop token", IsHidden = false }],
    };

    private static ExerciseSeed KeywordSearch => new()
    {
        Slug = "keyword-search",
        Title = "Keyword Retrieval",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Implement `BestMatch(query, docs)` returning the **index** of the document that
            shares the most **distinct** query words. Ties go to the lowest index. This is the
            simplest form of retrieval ranking (before embeddings).
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            public static class Solution
            {
                // TODO: score each doc by distinct query words it contains; return best index.
                public static int BestMatch(string[] query, string[][] docs)
                {
                    return -1;
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
                    r.Check("picks the most relevant doc", () =>
                    {
                        var query = new[]{"cat","dog"};
                        var docs = new[]
                        {
                            new[]{"the","cat"},          // 1 match
                            new[]{"cat","and","dog"},    // 2 matches
                            new[]{"bird"},               // 0
                        };
                        Assert.Equal(1, Solution.BestMatch(query, docs));
                    });
                    r.Check("tie goes to the lowest index", () =>
                    {
                        var query = new[]{"a"};
                        var docs = new[]{ new[]{"a"}, new[]{"a"} };
                        Assert.Equal(0, Solution.BestMatch(query, docs));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            public static class Solution
            {
                public static int BestMatch(string[] query, string[][] docs)
                {
                    var q = query.ToHashSet();
                    int bestIndex = 0, bestScore = -1;
                    for (int i = 0; i < docs.Length; i++)
                    {
                        int score = docs[i].Distinct().Count(w => q.Contains(w));
                        if (score > bestScore) { bestScore = score; bestIndex = i; } // strict > keeps lowest index on ties
                    }
                    return bestIndex;
                }
            }
            """,
        Hints =
        [
            "Put the query words in a HashSet for O(1) lookup.",
            "Score each doc by how many DISTINCT query words it contains.",
            "Use a strict > when tracking the best so ties keep the earlier doc.",
        ],
        TestCases =
        [
            new() { Name = "picks the most relevant doc", IsHidden = false },
            new() { Name = "tie goes to the lowest index", IsHidden = false },
        ],
    };

    private static ExerciseSeed Tokenizer => new()
    {
        Slug = "whitespace-tokenizer",
        Title = "Whitespace Tokenizer",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Implement `Encode(text, vocab)`: split `text` on spaces and map each word to its id
            in `vocab`. Unknown words map to the id of `"<unk>"`. This is a toy version of what
            a real tokenizer does before a model sees text.
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: split on spaces; map each word to vocab[word] or vocab["<unk>"].
                public static int[] Encode(string text, Dictionary<string, int> vocab)
                {
                    return new int[0];
                }
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
                    var vocab = new Dictionary<string, int> { ["the"]=1, ["cat"]=2, ["<unk>"]=0 };
                    r.Check("maps known words", () =>
                        Assert.Equal("1,2", string.Join(",", Solution.Encode("the cat", vocab))));
                    r.Check("unknown -> <unk> id", () =>
                        Assert.Equal("1,0", string.Join(",", Solution.Encode("the dog", vocab))));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;
            using System.Linq;

            public static class Solution
            {
                public static int[] Encode(string text, Dictionary<string, int> vocab) =>
                    text.Split(' ')
                        .Select(word => vocab.TryGetValue(word, out var id) ? id : vocab["<unk>"])
                        .ToArray();
            }
            """,
        Hints =
        [
            "Split the text on spaces.",
            "For each word, look it up in the vocab.",
            "Fall back to vocab[\"<unk>\"] when the word is missing.",
        ],
        TestCases =
        [
            new() { Name = "maps known words", IsHidden = false },
            new() { Name = "unknown -> <unk> id", IsHidden = false },
        ],
    };

    private static ExerciseSeed Jaccard => new()
    {
        Slug = "jaccard-similarity",
        Title = "Jaccard Similarity",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Implement `Jaccard(a, b)` = |intersection| / |union| of two sets of words (0 to 1).
            It's a quick way to measure text overlap — used for near-duplicate detection and
            de-duplicating retrieved chunks. Two empty sets → 1.
            """,
        StarterCode =
            """
            using System.Collections.Generic;
            using System.Linq;

            public static class Solution
            {
                // TODO: |a ∩ b| / |a ∪ b|. Empty ∩ empty -> 1.
                public static double Jaccard(string[] a, string[] b)
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
                    r.Check("half overlap -> 0.5", () =>
                        Assert.True(Math.Abs(Solution.Jaccard(new[]{"a","b","c"}, new[]{"b","c","d"}) - 0.5) < 1e-9));
                    r.Check("identical -> 1", () =>
                        Assert.True(Math.Abs(Solution.Jaccard(new[]{"a","b"}, new[]{"a","b"}) - 1) < 1e-9));
                    r.Check("disjoint -> 0", () =>
                        Assert.True(Math.Abs(Solution.Jaccard(new[]{"a"}, new[]{"b"}) - 0) < 1e-9));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;
            using System.Linq;

            public static class Solution
            {
                public static double Jaccard(string[] a, string[] b)
                {
                    var setA = a.ToHashSet();
                    var setB = b.ToHashSet();
                    int union = setA.Union(setB).Count();
                    if (union == 0) return 1;                 // both empty
                    int intersection = setA.Intersect(setB).Count();
                    return (double)intersection / union;
                }
            }
            """,
        Hints =
        [
            "Turn both arrays into sets.",
            "Intersection count divided by union count.",
            "Guard the empty/empty case to avoid dividing by zero (return 1).",
        ],
        TestCases =
        [
            new() { Name = "half overlap -> 0.5", IsHidden = false },
            new() { Name = "identical -> 1", IsHidden = false },
            new() { Name = "disjoint -> 0", IsHidden = true },
        ],
    };
}
