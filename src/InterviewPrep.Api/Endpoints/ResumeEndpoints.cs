using InterviewPrep.Api.Contracts;
using InterviewPrep.Application.Resume;
using InterviewPrep.Infrastructure.Resume;

namespace InterviewPrep.Api.Endpoints;

// Resume-builder endpoints, grouped under /api/resume. All three are stateless
// transforms — no DB, so resume PII never touches Postgres and lives only in the
// browser (React state + localStorage). The AI runs locally via Ollama.
public static class ResumeEndpoints
{
    public static IEndpointRouteBuilder MapResumeEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/resume");

        // Which local model is configured — the UI shows it in the "AI unavailable" hint.
        api.MapGet("/config", (OllamaOptions ollama) =>
            Results.Ok(new { model = ollama.Model, baseUrl = ollama.BaseUrl }));

        // Upload a resume (PDF/DOCX/txt) → extract text → structure into ResumeModel.
        api.MapPost("/parse",
            async (IFormFile file, IResumeDocumentService docs, IResumeAssistant ai, CancellationToken ct) =>
            {
                if (file is null || file.Length == 0)
                    return Results.BadRequest(new { error = "No file uploaded." });

                string text;
                try
                {
                    await using var stream = file.OpenReadStream();
                    text = docs.ExtractText(stream, file.FileName);
                }
                catch (UnsupportedResumeFormatException ex)
                {
                    return Results.BadRequest(new { error = ex.Message });
                }

                if (string.IsNullOrWhiteSpace(text))
                    return Results.BadRequest(new { error = "Couldn't read any text from that file. If it's a scanned/image PDF, the text isn't selectable — export a text-based PDF or upload a DOCX." });

                try
                {
                    var model = await ai.StructureAsync(text, ct);
                    return Results.Ok(model);
                }
                catch (ResumeAssistantUnavailableException ex)
                {
                    return AiUnavailable(ex);
                }
            })
            .DisableAntiforgery(); // this is a same-site local app; no browser form CSRF surface

        // Analyze the resume against a job description → ATS checks + tailoring suggestions.
        api.MapPost("/analyze",
            async (AnalyzeResumeRequest body, IResumeAssistant ai, CancellationToken ct) =>
            {
                if (body?.Resume is null)
                    return Results.BadRequest(new { error = "Missing resume." });
                if (string.IsNullOrWhiteSpace(body.JobDescription))
                    return Results.BadRequest(new { error = "Paste a job description to analyze against." });

                try
                {
                    var analysis = await ai.AnalyzeAsync(body.Resume, body.JobDescription, ct);
                    return Results.Ok(analysis);
                }
                catch (ResumeAssistantUnavailableException ex)
                {
                    return AiUnavailable(ex);
                }
            });

        // Render the structured resume to a downloadable PDF or DOCX in the chosen template.
        api.MapPost("/export",
            (ExportResumeRequest body, IResumeDocumentService docs) =>
            {
                if (body?.Resume is null)
                    return Results.BadRequest(new { error = "Missing resume." });
                if (!Enum.TryParse<ResumeFormat>(body.Format, ignoreCase: true, out var format))
                    return Results.BadRequest(new { error = $"Unknown format '{body.Format}'. Use 'Pdf' or 'Docx'." });

                var rendered = docs.Render(body.Resume, body.Template ?? "classic", format);
                return Results.File(rendered.Bytes, rendered.ContentType, rendered.FileName);
            });

        return app;
    }

    // Surface a missing/unreachable local model as 503 with an actionable message the
    // UI shows verbatim ("start ollama serve" / "ollama pull <model>").
    private static IResult AiUnavailable(ResumeAssistantUnavailableException ex) =>
        Results.Json(new { error = ex.Message }, statusCode: StatusCodes.Status503ServiceUnavailable);
}
