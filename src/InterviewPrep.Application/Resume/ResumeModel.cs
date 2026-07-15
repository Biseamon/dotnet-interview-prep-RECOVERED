namespace InterviewPrep.Application.Resume;

// The structured resume — the single model everything hangs off. The AI parses an
// uploaded file into this shape, tailors it to a job description, templates render it,
// and PDF/DOCX export it. Keeping the resume structured (rather than a blob of text) is
// what makes the output genuinely ATS-safe: a single-column render of known sections.
//
// Positional `sealed record`s to match the app-wide DTO style (see GamificationStats).
// Lists default to empty so a partial parse never yields nulls the renderer must guard.

public sealed record ResumeModel(
    ResumeContact Contact,
    string Summary,
    IReadOnlyList<ResumeExperience> Experience,
    IReadOnlyList<ResumeEducation> Education,
    IReadOnlyList<string> Skills)
{
    public static ResumeModel Empty { get; } = new(
        new ResumeContact("", "", "", "", "", ""),
        "",
        [],
        [],
        []);
}

// Header block. Everything optional-by-emptiness — a resume may omit a website, etc.
public sealed record ResumeContact(
    string FullName,
    string Title,     // e.g. "Senior .NET Engineer" — the headline under the name
    string Email,
    string Phone,
    string Location,  // "City, Country" — ATS parsers read this for location matching
    string Website);  // portfolio / LinkedIn / GitHub URL

// One job. Bullets are the accomplishment lines the AI rewrites to match a job.
public sealed record ResumeExperience(
    string Company,
    string Role,
    string StartDate,  // free-form strings ("Jan 2022", "2022") — dates vary too much to type
    string EndDate,    // "Present" is valid
    string Location,
    IReadOnlyList<string> Bullets);

public sealed record ResumeEducation(
    string School,
    string Degree,     // "B.Sc. Computer Science"
    string StartDate,
    string EndDate,
    string Details);   // honours, GPA, relevant coursework — optional

// ---- Analysis: the AI's job-tailoring feedback for the right-hand panel ----

// The full analysis result. MatchScore is 0..100. The lists drive the suggestion cards.
public sealed record ResumeAnalysis(
    int MatchScore,
    string Summary,                              // one-paragraph overall assessment
    IReadOnlyList<string> MissingKeywords,       // ATS keywords in the JD absent from the resume
    IReadOnlyList<string> Strengths,             // what already aligns well
    IReadOnlyList<AtsCheck> AtsChecks,           // the ATS quality checklist (see below)
    IReadOnlyList<BulletSuggestion> BulletSuggestions,
    string? SummarySuggestion);                  // a rewritten professional-summary paragraph, if any

// One ATS best-practice check, rendered as a pass/warn/fail row in the checklist card.
// These are the concrete "strong points" ATS and recruiters look for: quantified
// achievements (percentages / hard numbers), consistent date formatting, keyword
// coverage against the job description, strong action verbs, and complete contact/
// section coverage. The AI both scores each check and, where it fails, its fixes flow
// into MissingKeywords / BulletSuggestions / SummarySuggestion.
public sealed record AtsCheck(
    string Id,          // stable key: "quantified" | "dates" | "keywords" | "action_verbs" | "contact" | "sections"
    string Label,       // human label, e.g. "Quantified achievements (metrics & %)"
    AtsCheckStatus Status,
    string Detail);     // one line: what passed, or exactly what to fix

public enum AtsCheckStatus
{
    Pass,   // meets the ATS best practice
    Warn,   // partially — some bullets quantified, some dates inconsistent, etc.
    Fail,   // missing entirely
}

// A proposed rewrite of a single experience bullet, addressed by its position so the
// UI can apply it in place. ExperienceIndex/BulletIndex are 0-based into ResumeModel.
public sealed record BulletSuggestion(
    int ExperienceIndex,
    int BulletIndex,
    string Original,
    string Suggested,
    string Reason);   // why the rewrite helps (keyword added, quantified, stronger verb…)
