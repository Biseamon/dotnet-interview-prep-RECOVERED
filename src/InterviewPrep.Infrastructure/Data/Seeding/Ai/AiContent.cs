namespace InterviewPrep.Infrastructure.Data.Seeding.Ai;

// The "AI & LLM Engineering" topic. The building blocks of modern AI apps —
// embeddings/similarity, RAG chunking, prompt templating, token budgeting, sampling
// — implemented as plain C# so they grade with the existing Roslyn runner. No API
// keys or model calls; just the logic interviewers actually probe.
internal static partial class AiContent
{
    public static TopicSeed Topic => new()
    {
        Slug = "ai-llm",
        Name = "AI & LLM Engineering",
        Description = "Embeddings, cosine similarity, RAG chunking, prompt templates, token budgets, and sampling — in code.",
        Order = 15,
        Lessons =
        [
            new LessonSeed
            {
                Slug = "embeddings-similarity", Title = "Embeddings & Similarity", Order = 1,
                MarkdownContent =
                    """
                    ## Embeddings & Similarity

                    An **embedding** turns text into a vector of numbers so similar meanings sit
                    close together. **Cosine similarity** measures the angle between two vectors
                    (1 = identical direction, 0 = unrelated) — it's how vector search / RAG finds
                    relevant chunks. Picking the top scores is how a model chooses likely tokens.
                    """,
                Exercises = [CosineSimilarity, TopK],
            },
            new LessonSeed
            {
                Slug = "rag-prompting", Title = "RAG & Prompting", Order = 2,
                MarkdownContent =
                    """
                    ## RAG & Prompting

                    **Retrieval-Augmented Generation** feeds relevant text into the prompt. That
                    means **chunking** documents (with overlap so context isn't cut mid-idea),
                    filling a **prompt template**, estimating **tokens** to stay within the model's
                    context window, and **trimming** old chat history to fit that budget.
                    """,
                Exercises = [Chunk, PromptTemplate, EstimateTokens, TrimHistory],
            },
            RetrievalDecodingLesson,
        ],
    };

    private static ExerciseSeed CosineSimilarity => new()
    {
        Slug = "cosine-similarity",
        Title = "Cosine Similarity",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Implement `CosineSimilarity(a, b)` = dot(a,b) / (‖a‖·‖b‖) for two equal-length
            vectors. This is the core of vector search: 1 means same direction, 0 means unrelated.
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: dot product divided by the product of the magnitudes.
                public static double CosineSimilarity(double[] a, double[] b)
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
                    r.Check("identical vectors -> 1", () =>
                        Assert.True(Math.Abs(Solution.CosineSimilarity(new[]{1.0,2,3}, new[]{1.0,2,3}) - 1) < 1e-9));
                    r.Check("orthogonal vectors -> 0", () =>
                        Assert.True(Math.Abs(Solution.CosineSimilarity(new[]{1.0,0}, new[]{0.0,1}) - 0) < 1e-9));
                    r.Check("same direction, different scale -> 1", () =>
                        Assert.True(Math.Abs(Solution.CosineSimilarity(new[]{1.0,0}, new[]{5.0,0}) - 1) < 1e-9));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public static class Solution
            {
                public static double CosineSimilarity(double[] a, double[] b)
                {
                    double dot = 0, magA = 0, magB = 0;
                    for (int i = 0; i < a.Length; i++)
                    {
                        dot += a[i] * b[i];
                        magA += a[i] * a[i];
                        magB += b[i] * b[i];
                    }
                    return dot / (Math.Sqrt(magA) * Math.Sqrt(magB));
                }
            }
            """,
        Hints =
        [
            "Dot product = sum of a[i]*b[i].",
            "Magnitude of a vector = sqrt(sum of squares).",
            "Divide the dot product by the product of the two magnitudes.",
        ],
        TestCases =
        [
            new() { Name = "identical vectors -> 1", IsHidden = false },
            new() { Name = "orthogonal vectors -> 0", IsHidden = false },
        ],
    };

    private static ExerciseSeed TopK => new()
    {
        Slug = "top-k-logits",
        Title = "Top-K Sampling",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Given model `logits` (scores per token), return the **indices** of the `k` highest,
            ordered from highest to lowest. This is the shortlist top-k sampling draws from.
            """,
        StarterCode =
            """
            using System;
            using System.Linq;

            public static class Solution
            {
                // TODO: indices of the k largest logits, highest first.
                public static int[] TopK(double[] logits, int k)
                {
                    return new int[0];
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
                    r.Check("[0.1,0.9,0.3,0.5], k=2 -> [1,3]", () =>
                        Assert.Equal("1,3", string.Join(",", Solution.TopK(new[]{0.1,0.9,0.3,0.5}, 2))));
                    r.Check("k=1 picks the max index", () =>
                        Assert.Equal("2", string.Join(",", Solution.TopK(new[]{0.2,0.1,0.7}, 1))));
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
                public static int[] TopK(double[] logits, int k) =>
                    logits.Select((value, index) => (value, index))
                          .OrderByDescending(t => t.value)
                          .Take(k)
                          .Select(t => t.index)
                          .ToArray();
            }
            """,
        Hints =
        [
            "Pair each logit with its index before sorting so you don't lose positions.",
            "Order by value descending, take k, then select the indices.",
        ],
        TestCases =
        [
            new() { Name = "[0.1,0.9,0.3,0.5], k=2 -> [1,3]", IsHidden = false },
            new() { Name = "k=1 picks the max index", IsHidden = false },
        ],
    };

    private static ExerciseSeed Chunk => new()
    {
        Slug = "text-chunking",
        Title = "Chunk Text (with Overlap)",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Split `words` into chunks of `size` words that **overlap** by `overlap` words, and
            return each chunk as a space-joined string. Overlap keeps context from being cut
            mid-thought — essential for good RAG retrieval. (Step = size − overlap; stop once a
            chunk reaches the end.)
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            public static class Solution
            {
                // TODO: sliding chunks of `size`, stepping by (size - overlap).
                public static string[] Chunk(string[] words, int size, int overlap)
                {
                    return new string[0];
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
                    r.Check("[a,b,c,d,e] size 3 overlap 1", () =>
                        Assert.Equal("a b c|c d e",
                            string.Join("|", Solution.Chunk(new[]{"a","b","c","d","e"}, 3, 1))));
                    r.Check("no overlap tiles exactly", () =>
                        Assert.Equal("a b|c d",
                            string.Join("|", Solution.Chunk(new[]{"a","b","c","d"}, 2, 0))));
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
                public static string[] Chunk(string[] words, int size, int overlap)
                {
                    var result = new List<string>();
                    int step = size - overlap;
                    for (int i = 0; i < words.Length; i += step)
                    {
                        result.Add(string.Join(" ", words.Skip(i).Take(size)));
                        if (i + size >= words.Length) break; // reached the end
                    }
                    return result.ToArray();
                }
            }
            """,
        Hints =
        [
            "Step forward by (size - overlap) each time so chunks overlap.",
            "Each chunk is words[i .. i+size] joined by spaces.",
            "Stop once a chunk covers the last word.",
        ],
        TestCases =
        [
            new() { Name = "[a,b,c,d,e] size 3 overlap 1", IsHidden = false },
            new() { Name = "no overlap tiles exactly", IsHidden = false },
        ],
    };

    private static ExerciseSeed PromptTemplate => new()
    {
        Slug = "prompt-template",
        Title = "Fill a Prompt Template",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Implement `Fill(template, vars)` that replaces every `{{key}}` placeholder in the
            template with its value from the dictionary. This is how prompt templates inject
            variables (user question, retrieved context, etc.).
            """,
        StarterCode =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                // TODO: replace each {{key}} with vars[key].
                public static string Fill(string template, Dictionary<string, string> vars)
                {
                    return template;
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
                    r.Check("fills placeholders", () =>
                    {
                        var vars = new Dictionary<string, string> { ["name"] = "Ada", ["role"] = "engineer" };
                        Assert.Equal("Hi Ada, the engineer.", Solution.Fill("Hi {{name}}, the {{role}}.", vars));
                    });
                    r.Check("repeated placeholder", () =>
                    {
                        var vars = new Dictionary<string, string> { ["x"] = "1" };
                        Assert.Equal("1 and 1", Solution.Fill("{{x}} and {{x}}", vars));
                    });
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System.Collections.Generic;

            public static class Solution
            {
                public static string Fill(string template, Dictionary<string, string> vars)
                {
                    foreach (var (key, value) in vars)
                        template = template.Replace("{{" + key + "}}", value);
                    return template;
                }
            }
            """,
        Hints =
        [
            "Loop over each key/value in the dictionary.",
            "Replace the literal \"{{key}}\" with its value.",
            "string.Replace replaces every occurrence.",
        ],
        TestCases =
        [
            new() { Name = "fills placeholders", IsHidden = false },
            new() { Name = "repeated placeholder", IsHidden = false },
        ],
    };

    private static ExerciseSeed EstimateTokens => new()
    {
        Slug = "estimate-tokens",
        Title = "Estimate Tokens",
        Difficulty = "Easy",
        Kind = "Function",
        Prompt =
            """
            Estimate the token count of `text` using the common rule of thumb: **~4 characters
            per token**, rounded up. Token budgeting decides how much context fits in the model's
            window. Return 0 for empty text.
            """,
        StarterCode =
            """
            using System;

            public static class Solution
            {
                // TODO: ceil(length / 4).
                public static int EstimateTokens(string text)
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
                    r.Check("4 chars -> 1 token", () => Assert.Equal(1, Solution.EstimateTokens("abcd")));
                    r.Check("11 chars -> 3 tokens", () => Assert.Equal(3, Solution.EstimateTokens("hello world")));
                    r.Check("empty -> 0", () => Assert.Equal(0, Solution.EstimateTokens("")));
                    return r.ToJson();
                }
            }
            """,
        ReferenceSolution =
            """
            using System;

            public static class Solution
            {
                public static int EstimateTokens(string text) =>
                    (int)Math.Ceiling(text.Length / 4.0);
            }
            """,
        Hints =
        [
            "Divide the character count by 4.0 (use a double so it doesn't truncate).",
            "Round up with Math.Ceiling, then cast to int.",
        ],
        TestCases =
        [
            new() { Name = "4 chars -> 1 token", IsHidden = false },
            new() { Name = "11 chars -> 3 tokens", IsHidden = false },
            new() { Name = "empty -> 0", IsHidden = true },
        ],
    };

    private static ExerciseSeed TrimHistory => new()
    {
        Slug = "trim-history",
        Title = "Trim Chat History to a Budget",
        Difficulty = "Medium",
        Kind = "Function",
        Prompt =
            """
            Given chat `messages` and their `tokens`, keep the **most recent** messages that fit
            within `budget` tokens, preserving original order. This is how you stop a long
            conversation from overflowing the context window.
            """,
        StarterCode =
            """
            using System;
            using System.Collections.Generic;
            using System.Linq;

            public static class Solution
            {
                // TODO: keep the newest messages whose token sum fits the budget, in order.
                public static string[] Trim(string[] messages, int[] tokens, int budget)
                {
                    return new string[0];
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
                    r.Check("keeps the newest that fit", () =>
                    {
                        var msgs = new[]{"m1","m2","m3"};
                        var toks = new[]{5,5,5};
                        Assert.Equal("m2,m3", string.Join(",", Solution.Trim(msgs, toks, 12)));
                    });
                    r.Check("all fit", () =>
                        Assert.Equal("m1,m2", string.Join(",", Solution.Trim(new[]{"m1","m2"}, new[]{2,2}, 100))));
                    r.Check("none fit -> empty", () =>
                        Assert.Equal("", string.Join(",", Solution.Trim(new[]{"m1"}, new[]{50}, 10))));
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
                public static string[] Trim(string[] messages, int[] tokens, int budget)
                {
                    // Walk from the newest message backward, accumulating tokens until the
                    // next one wouldn't fit; keep that suffix in original order.
                    int total = 0, start = messages.Length;
                    for (int i = messages.Length - 1; i >= 0; i--)
                    {
                        if (total + tokens[i] > budget) break;
                        total += tokens[i];
                        start = i;
                    }
                    return messages.Skip(start).ToArray();
                }
            }
            """,
        Hints =
        [
            "Iterate from the last message toward the first.",
            "Accumulate tokens; stop before one would exceed the budget.",
            "Return the kept suffix in original order.",
        ],
        TestCases =
        [
            new() { Name = "keeps the newest that fit", IsHidden = false },
            new() { Name = "all fit", IsHidden = false },
            new() { Name = "none fit -> empty", IsHidden = true },
        ],
    };
}
