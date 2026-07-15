using System.Text;
using System.Text.Json;
using InterviewPrep.Application.Grading;
using InterviewPrep.Domain;
using InterviewPrep.Domain.Grading;
using Microsoft.Data.Sqlite;

namespace InterviewPrep.CodeExecution;

// Grades SQL exercises by executing the learner's query against a fresh in-memory
// SQLite database. The exercise's harness (JSON) provides the schema+seed and the
// reference query; we run both and compare result sets.
//
//   HarnessCode JSON: { "setup": "CREATE TABLE …; INSERT …",
//                       "solution": "SELECT …",
//                       "ordered": false }
//
// "ordered": true means row order matters (the query has ORDER BY); otherwise rows
// are compared as an unordered multiset.
public sealed class SqlRunner : IExerciseRunner
{
    public ExerciseLanguage Language => ExerciseLanguage.Sql;

    private sealed record Spec(string Setup, string Solution, bool Ordered);

    public GradeResult Run(RunRequest request)
    {
        var spec = JsonSerializer.Deserialize<Spec>(request.HarnessCode,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("Invalid SQL harness spec.");

        // A private, in-memory database that lives only for this grading run.
        using var conn = new SqliteConnection("Data Source=:memory:");
        conn.Open();

        // 1. Build the schema + seed data (authoring bug if this throws → surface as server error).
        Exec(conn, spec.Setup);

        // 2. Expected rows from the reference query.
        List<string[]> expected;
        try { expected = Query(conn, spec.Solution, out _); }
        catch (Exception ex) { throw new InvalidOperationException("Reference SQL failed: " + ex.Message); }

        // 3. The learner's query. A SQL error is the equivalent of a compile error.
        List<string[]> actual;
        string[] actualCols;
        try
        {
            actual = Query(conn, request.UserSource, out actualCols);
        }
        catch (SqliteException ex)
        {
            return GradeResult.FromCompileErrors(new[]
            {
                new CompileError("Error", "SQL", ex.Message, 1, 1, 1, 1),
            });
        }

        // 4. Compare result sets → one test case with a readable diff.
        var pass = Compare(expected, actual, spec.Ordered);
        var result = new TestCaseResult(
            Name: "query returns the expected rows",
            Passed: pass,
            Expected: pass ? null : Grid(expected),
            Actual: pass ? null : Grid(actual, actualCols),
            ExceptionType: null,
            ExceptionMessage: null,
            Stdout: null,
            ElapsedMs: 0);

        return GradeResult.FromTestResults(new[] { result });
    }

    private static void Exec(SqliteConnection conn, string sql)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        cmd.ExecuteNonQuery();
    }

    private static List<string[]> Query(SqliteConnection conn, string sql, out string[] columns)
    {
        using var cmd = conn.CreateCommand();
        cmd.CommandText = sql;
        using var reader = cmd.ExecuteReader();

        columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToArray();
        var rows = new List<string[]>();
        while (reader.Read())
        {
            var row = new string[reader.FieldCount];
            for (int i = 0; i < reader.FieldCount; i++)
                row[i] = reader.IsDBNull(i) ? "NULL" : reader.GetValue(i).ToString() ?? "";
            rows.Add(row);
        }
        return rows;
    }

    private static bool Compare(List<string[]> a, List<string[]> b, bool ordered)
    {
        if (a.Count != b.Count) return false;
        if (a.Count > 0 && a[0].Length != b[0].Length) return false;

        static string Key(string[] r) => string.Join("", r);
        if (ordered)
            return a.Select(Key).SequenceEqual(b.Select(Key));

        // Unordered: compare as multisets (sorted).
        return a.Select(Key).OrderBy(x => x, StringComparer.Ordinal)
                .SequenceEqual(b.Select(Key).OrderBy(x => x, StringComparer.Ordinal));
    }

    // A small text grid for the results panel diff.
    private static string Grid(List<string[]> rows, string[]? cols = null)
    {
        var sb = new StringBuilder();
        if (cols is { Length: > 0 }) sb.Append(string.Join(" | ", cols)).Append('\n');
        foreach (var r in rows.Take(12)) sb.Append(string.Join(" | ", r)).Append('\n');
        if (rows.Count > 12) sb.Append($"… (+{rows.Count - 12} more rows)");
        if (rows.Count == 0) sb.Append("(no rows)");
        return sb.ToString().TrimEnd();
    }
}
