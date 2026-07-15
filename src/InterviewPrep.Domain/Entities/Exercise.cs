namespace InterviewPrep.Domain.Entities;

// The core interactive unit: a coding problem the learner solves in the editor.
//
// SECURITY/CONTENT NOTE: several fields here are HIDDEN from the public API
// (HarnessCode, ReferenceSolution) — they must never be serialized into the
// exercise-detail DTO, or the learner could just read the answer. The API layer
// maps to a trimmed DTO; these live only in the domain/DB and the grader.
public class Exercise
{
    public int Id { get; set; }

    public int LessonId { get; set; }
    public Lesson? Lesson { get; set; }

    public required string Slug { get; set; }
    public required string Title { get; set; }

    // The problem statement shown to the learner (Markdown).
    public required string Prompt { get; set; }

    // A concise, plain-English "explain like I'm 5" idea for this exercise — the
    // learning material shown before the problem. Optional (nullable) so exercises
    // without one still work; populated from the central Explanations map at seed time.
    public string? Explanation { get; set; }

    public Difficulty Difficulty { get; set; }

    // Informational — what shape of code is expected (drives UI copy).
    public ExerciseKind Kind { get; set; }

    // Which language the learner writes → which grader runs it + editor syntax mode.
    public ExerciseLanguage Language { get; set; } = ExerciseLanguage.CSharp;

    // The pre-filled editor contents: a stub with `// TODO` markers that also
    // PIN the required signature. Because the grading harness is compiled against
    // this signature, a wrong signature becomes a clean compiler error.
    public required string StarterCode { get; set; }

    // HIDDEN. The C# test harness compiled alongside the submission. Exposes a
    // `public static class __Harness { public static HarnessReport Run() {...} }`
    // that drives the user's code and records pass/fail. Authoritative for correctness.
    public required string HarnessCode { get; set; }

    // HIDDEN. A fully-worked, richly-commented model answer — revealed only after
    // the learner passes or explicitly gives up. Doubles as teaching material.
    public required string ReferenceSolution { get; set; }

    // Per-exercise execution budget (seconds). Guards against infinite loops /
    // deadlocks in submissions. Null → use the grader's default.
    public int? TimeoutSeconds { get; set; }

    // Progressive hints, revealed one at a time (ordered by Hint.Order).
    public List<Hint> Hints { get; set; } = [];

    // Example/edge test cases. Some are shown as worked examples (IsHidden=false);
    // hidden ones only surface as pass/fail. The harness is the source of truth —
    // these rows are display metadata + optional data-driven inputs.
    public List<TestCase> TestCases { get; set; } = [];
}
