namespace InterviewPrep.Domain;

// Difficulty of an exercise — drives UI badges and lets us order a lesson from
// gentle to hard. Stored as a string in the DB (via a value converter) so rows
// stay human-readable and reordering the enum can't silently corrupt data.
public enum Difficulty
{
    Easy,
    Medium,
    Hard,
}

// The "shape" of what the learner must write. Purely informational — it drives
// the prompt UI and hints ("implement this interface" vs "write this function").
// The grading harness binds to the user's code regardless of Kind.
public enum ExerciseKind
{
    Function,   // implement a single method (e.g. int Fib(int n))
    Interface,  // implement a given interface (e.g. IStack<T>)
    Class,      // build a whole class (e.g. a Strategy-pattern context)
}

// What LANGUAGE the learner writes — decides which grader runs the submission and
// which Monaco syntax mode the editor uses. Defaults to CSharp for existing content.
public enum ExerciseLanguage
{
    CSharp, // compiled + tested by the Roslyn runner
    Sql,    // executed against an in-memory SQLite database, result set compared
    Config, // Dockerfile / YAML / manifest, checked against structural rules
}

// Outcome of grading a submission. Ordered from "didn't even build" upward so
// the frontend can branch cleanly on a single value.
public enum GradeStatus
{
    CompileError, // user code didn't compile — show Monaco markers
    Failed,       // compiled + ran, but one or more tests failed
    RuntimeError, // compiled but threw before/around tests (uncaught exception)
    Timeout,      // exceeded the per-exercise time budget (e.g. infinite loop)
    Passed,       // all tests green 🎉
}
