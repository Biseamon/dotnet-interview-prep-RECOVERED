namespace InterviewPrep.Domain.Entities;

// A test case for an Exercise. Two roles:
//  1. DISPLAY — visible cases (IsHidden=false) are shown as worked examples so
//     the learner understands the expected behaviour.
//  2. DATA — optional InputJson/ExpectedJson can drive a data-driven harness.
// The compiled harness remains authoritative for correctness; these rows exist
// for UI and (optionally) to parameterize the harness.
public class TestCase
{
    public int Id { get; set; }

    public int ExerciseId { get; set; }
    public Exercise? Exercise { get; set; }

    // Human-readable label, e.g. "fib(10) == 55". Shown in the results panel.
    public required string Name { get; set; }

    // Hidden cases don't reveal their input/expected — the learner only sees
    // pass/fail. Visible ones act as documentation.
    public bool IsHidden { get; set; }

    // Optional JSON-encoded input/expected for data-driven harnesses. Null for
    // exercises whose harness hard-codes its assertions.
    public string? InputJson { get; set; }
    public string? ExpectedJson { get; set; }

    public int Order { get; set; }
}
