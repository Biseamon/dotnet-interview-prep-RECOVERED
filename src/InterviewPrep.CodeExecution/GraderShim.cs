namespace InterviewPrep.CodeExecution;

// The "assert shim": a self-contained chunk of C# source that is compiled INTO
// every submission assembly (as a third syntax tree, alongside the user's code and
// the exercise's harness). It provides the tiny testing vocabulary that harnesses
// use — `Assert.*` and `HarnessReport` — WITHOUT any external test framework.
//
// CRITICAL DESIGN CHOICE: HarnessReport.ToJson() returns a plain string. The grader
// only ever pulls a *string* out of the sandbox's AssemblyLoadContext. If it instead
// held a HarnessReport object, that object's Type would belong to the collectible
// load context and pin it in memory, preventing unload. Strings are safe.
public static class GraderShim
{
    // Compiled with the user code, so these types live in the global namespace and
    // are directly usable from harness code (e.g. `Assert.Equal(...)`).
    public const string Source =
        """
        using System;
        using System.Collections.Generic;
        using System.Diagnostics;
        using System.Globalization;
        using System.IO;
        using System.Text;

        // Thrown by Assert.* on failure, carrying stringified expected/actual so the
        // results panel can show a precise diff.
        public sealed class AssertException : Exception
        {
            public string? Expected { get; }
            public string? Actual { get; }
            public AssertException(string message, string? expected = null, string? actual = null)
                : base(message) { Expected = expected; Actual = actual; }
        }

        // Minimal assertion helpers — enough to grade the exercises we author.
        public static class Assert
        {
            public static void Equal<T>(T expected, T actual)
            {
                if (!EqualityComparer<T>.Default.Equals(expected, actual))
                    throw new AssertException("Values are not equal.", Fmt(expected), Fmt(actual));
            }

            public static void True(bool condition, string? message = null)
            {
                if (!condition)
                    throw new AssertException(message ?? "Expected condition to be true.", "true", "false");
            }

            public static void False(bool condition, string? message = null)
            {
                if (condition)
                    throw new AssertException(message ?? "Expected condition to be false.", "false", "true");
            }

            private static string Fmt(object? o) => o?.ToString() ?? "null";
        }

        // Shared data-structure node types, provided to EVERY submission so linked-list
        // and tree exercises can reference them without each redefining (and so learners
        // don't have to — the node type is "given", as on LeetCode). Harnesses build and
        // inspect these; starter signatures reference them.
        public class ListNode
        {
            public int val;
            public ListNode? next;
            public ListNode(int val = 0, ListNode? next = null) { this.val = val; this.next = next; }
        }

        public class TreeNode
        {
            public int val;
            public TreeNode? left;
            public TreeNode? right;
            public TreeNode(int val = 0, TreeNode? left = null, TreeNode? right = null)
            {
                this.val = val; this.left = left; this.right = right;
            }
        }

        // Collects per-case results and serializes them to a JSON array string.
        // Each Check() runs one test case, capturing pass/fail, expected/actual,
        // exceptions, anything written to Console, and elapsed time.
        public sealed class HarnessReport
        {
            private readonly StringBuilder _json = new StringBuilder();
            private bool _first = true;

            public void Check(string name, Action body)
            {
                var sw = Stopwatch.StartNew();
                bool passed = false;
                string? expected = null, actual = null, exType = null, exMsg = null;

                // Redirect Console output for THIS case so we can show what the
                // learner's code printed, then always restore the original writer.
                var originalOut = Console.Out;
                var buffer = new StringWriter();
                Console.SetOut(buffer);
                try
                {
                    body();
                    passed = true;
                }
                catch (AssertException ae)
                {
                    expected = ae.Expected; actual = ae.Actual; exMsg = ae.Message;
                }
                catch (Exception ex)
                {
                    exType = ex.GetType().Name; exMsg = ex.Message;
                }
                finally
                {
                    Console.SetOut(originalOut);
                    sw.Stop();
                }

                Append(name, passed, expected, actual, exType, exMsg, buffer.ToString(), sw.ElapsedMilliseconds);
            }

            private void Append(string name, bool passed, string? expected, string? actual,
                                string? exType, string? exMsg, string stdout, long ms)
            {
                if (!_first) _json.Append(',');
                _first = false;
                _json.Append('{')
                     .Append("\"name\":").Append(Q(name)).Append(',')
                     .Append("\"passed\":").Append(passed ? "true" : "false").Append(',')
                     .Append("\"expected\":").Append(Q(expected)).Append(',')
                     .Append("\"actual\":").Append(Q(actual)).Append(',')
                     .Append("\"exceptionType\":").Append(Q(exType)).Append(',')
                     .Append("\"exceptionMessage\":").Append(Q(exMsg)).Append(',')
                     .Append("\"stdout\":").Append(Q(stdout.Length == 0 ? null : stdout)).Append(',')
                     .Append("\"elapsedMs\":").Append(ms.ToString(CultureInfo.InvariantCulture))
                     .Append('}');
            }

            public string ToJson() => "[" + _json.ToString() + "]";

            // JSON string encoder (quotes + escapes). Hand-rolled to keep the shim
            // dependency-free and its output 100% predictable for the grader's parser.
            private static string Q(string? s)
            {
                if (s == null) return "null";
                var sb = new StringBuilder(s.Length + 2);
                sb.Append('"');
                foreach (char c in s)
                {
                    switch (c)
                    {
                        case '"': sb.Append("\\\""); break;
                        case '\\': sb.Append("\\\\"); break;
                        case '\n': sb.Append("\\n"); break;
                        case '\r': sb.Append("\\r"); break;
                        case '\t': sb.Append("\\t"); break;
                        default:
                            if (c < 0x20) sb.Append("\\u").Append(((int)c).ToString("x4"));
                            else sb.Append(c);
                            break;
                    }
                }
                sb.Append('"');
                return sb.ToString();
            }
        }
        """;
}
