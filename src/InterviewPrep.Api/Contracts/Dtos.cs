using InterviewPrep.Domain.Entities;

namespace InterviewPrep.Api.Contracts;

// API response DTOs. These are the PUBLIC shape sent to the browser. Crucially,
// they OMIT hidden fields (HarnessCode, ReferenceSolution, hint text, hidden test
// details) so the client can never read the answer. Mapping lives here so the
// domain entities stay free of serialization concerns.

// Dashboard shape: topic + its lessons + all exercise slugs (so the UI computes
// per-topic progress by intersecting with the solved set — no N+1 of lesson calls).
public record TopicDto(
    string Slug,
    string Name,
    string Description,
    IReadOnlyList<LessonSummaryDto> Lessons,
    IReadOnlyList<string> ExerciseSlugs);
public record LessonSummaryDto(string Slug, string Title);

// Topic page shape: lessons each carrying their exercise summaries.
public record TopicDetailDto(string Slug, string Name, string Description, IReadOnlyList<LessonDto> Lessons);

public record LessonDto(string Slug, string Title, string MarkdownContent, IReadOnlyList<ExerciseSummaryDto> Exercises);
public record ExerciseSummaryDto(string Slug, string Title, string Difficulty);

// Full exercise as seen by the learner: prompt + starter code + VISIBLE example
// tests + hint COUNT (not the hint text). Harness/solution deliberately absent.
public record ExerciseDto(
    string Slug,
    string Title,
    string Prompt,
    string? Explanation,   // ELI5 "the idea" learning material
    string Difficulty,
    string Kind,
    string Language,       // CSharp | Sql | Config → editor syntax mode
    string StarterCode,
    int HintCount,
    IReadOnlyList<TestCaseDto> VisibleTests,
    string? TopicSlug); // owning topic, so the UI can navigate to siblings

public record TestCaseDto(string Name);

public record GradeRequest(string Source);

// Body for POST /api/drill/complete — the learner's drill score.
public record DrillCompleteRequest(int CorrectCount, int Total);

// --- Resume builder request bodies (responses are the Application.Resume records directly) ---

// POST /api/resume/analyze — the structured resume + the target job description.
public record AnalyzeResumeRequest(
    InterviewPrep.Application.Resume.ResumeModel Resume,
    string JobDescription);

// POST /api/resume/export — the structured resume + chosen template + format ("Pdf" | "Docx").
public record ExportResumeRequest(
    InterviewPrep.Application.Resume.ResumeModel Resume,
    string Template,
    string Format);

// Static mappers from entity → DTO.
public static class DtoMapping
{
    public static TopicDto ToDto(this Topic t) => new(
        t.Slug, t.Name, t.Description,
        t.Lessons.Select(l => new LessonSummaryDto(l.Slug, l.Title)).ToList(),
        t.Lessons.SelectMany(l => l.Exercises).Select(e => e.Slug).ToList());

    public static TopicDetailDto ToDetailDto(this Topic t) => new(
        t.Slug, t.Name, t.Description,
        t.Lessons.Select(l => l.ToDto()).ToList());

    public static LessonDto ToDto(this Lesson l) => new(
        l.Slug, l.Title, l.MarkdownContent,
        l.Exercises.Select(x => new ExerciseSummaryDto(x.Slug, x.Title, x.Difficulty.ToString())).ToList());

    public static ExerciseDto ToDto(this Exercise x) => new(
        x.Slug,
        x.Title,
        x.Prompt,
        x.Explanation,
        x.Difficulty.ToString(),
        x.Kind.ToString(),
        x.Language.ToString(),
        x.StarterCode,
        x.Hints.Count,
        // Only non-hidden test cases are exposed as worked examples.
        x.TestCases.Where(tc => !tc.IsHidden).Select(tc => new TestCaseDto(tc.Name)).ToList(),
        x.Lesson?.Topic?.Slug);
}
