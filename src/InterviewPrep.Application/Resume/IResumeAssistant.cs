namespace InterviewPrep.Application.Resume;

// The AI abstraction for the resume builder. Implemented in Infrastructure against a
// LOCAL Ollama instance (the app runs entirely on-device; resume PII never leaves the
// machine). Kept as an interface here so the Application/API layers never see the HTTP
// client — mirrors how IContentRepository / IExerciseRunner hide their infrastructure.
public interface IResumeAssistant
{
    // Turn the raw extracted text of an uploaded resume into the structured model.
    Task<ResumeModel> StructureAsync(string rawText, CancellationToken ct);

    // Score the resume against a target job description and return tailoring feedback:
    // ATS checks, missing keywords, and per-bullet rewrite suggestions.
    Task<ResumeAnalysis> AnalyzeAsync(ResumeModel resume, string jobDescription, CancellationToken ct);
}

// Thrown when the local Ollama service can't be reached or the configured model is
// missing, so the endpoint can return a clear, actionable message ("start Ollama /
// pull the model") instead of a raw 500.
public sealed class ResumeAssistantUnavailableException(string message, Exception? inner = null)
    : Exception(message, inner);
