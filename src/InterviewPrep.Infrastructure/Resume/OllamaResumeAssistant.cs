using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using InterviewPrep.Application.Resume;
using Microsoft.Extensions.Logging;

namespace InterviewPrep.Infrastructure.Resume;

// Options for the local Ollama connection, bound from configuration in Program.cs.
public sealed record OllamaOptions(string BaseUrl, string Model);

// IResumeAssistant backed by a LOCAL Ollama instance (default http://localhost:11434).
// The whole app runs on-device, so resume PII never leaves the machine. We call Ollama's
// /api/chat with `format` set to a JSON Schema (structured outputs) so parsing/analysis
// come back as reliable JSON rather than prose we'd have to scrape.
//
// This is the codebase's first outbound HttpClient; it's a typed client (see
// AddHttpClient<IResumeAssistant, OllamaResumeAssistant> in DependencyInjection).
public sealed class OllamaResumeAssistant(HttpClient http, OllamaOptions options, ILogger<OllamaResumeAssistant> logger) : IResumeAssistant
{
    // Bigger than Ollama's 4096 default so the prompt + JSON output fit without the model
    // running out of context (a resume + job description + schema is easily >4k tokens).
    private const int NumCtx = 12288;

    // Ollama's plain "return valid JSON" mode (vs. a rigid schema). Used for analysis so the
    // model actually writes the rewrites rather than filling schema-required fields with "".
    private static readonly JsonElement JsonFormat = JsonSerializer.SerializeToElement("json");

    // Case-insensitive so the model's JSON keys map to our records regardless of casing;
    // camelCase enum converter so "pass"/"warn"/"fail" bind to AtsCheckStatus; omit nulls
    // so an unset `think` field is dropped rather than sent as null.
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web)
    {
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase),
            new AtsCheckListConverter(), // tolerate array OR object-map shapes for atsChecks
        },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        NumberHandling = JsonNumberHandling.AllowReadingFromString, // some models quote numbers
    };

    public async Task<ResumeModel> StructureAsync(string rawText, CancellationToken ct)
    {
        const string system =
            "You are a resume parser. Extract the resume text into the given JSON structure, copying the " +
            "candidate's real content faithfully — never invent experience, dates, or skills.\n" +
            "- The first non-empty line is usually the candidate's NAME (contact.fullName); a short line under " +
            "it is their headline TITLE.\n" +
            "- The line containing email / phone / city / links is the CONTACT line: split it into contact.email, " +
            "contact.phone, contact.location, and contact.website.\n" +
            "- For each job, role is the job TITLE and company is the EMPLOYER: a line like 'Software Engineer, " +
            "Acme Corp' means role='Software Engineer', company='Acme Corp'.\n" +
            "- For EACH job, extract the date range into startDate and endDate (use 'Present' for a current role), " +
            "and copy EVERY bullet / accomplishment line into that job's bullets array, verbatim. Never leave " +
            "bullets empty when the source has them.\n" +
            "- Extract each education entry's school, degree, and dates.\n" +
            "Use an empty string for a field that is genuinely absent; keep dates exactly as written.";

        var user = "Resume text:\n\n" + Truncate(rawText, 24_000);

        logger.LogInformation("Resume ▶ structuring {Chars} chars of resume text with model '{Model}' (first call may load the model into memory — can take 30-60s)…", rawText.Length, options.Model);
        var model = await ChatAsync<ResumeModel>("structure", system, user, ResumeSchema, ct);
        logger.LogInformation("Resume ✓ structured: {Jobs} jobs, {Skills} skills for '{Name}'", model.Experience?.Count ?? 0, model.Skills?.Count ?? 0, model.Contact?.FullName);
        return Normalize(model);
    }

    public async Task<ResumeAnalysis> AnalyzeAsync(ResumeModel resume, string jobDescription, CancellationToken ct)
    {
        // The ATS rubric is spelled out so the model scores the concrete strong points
        // recruiters/ATS look for. Framing (per recruiter research): a modern ATS mostly
        // PARSES and RANKS resumes for a human reviewer — it rarely auto-rejects. So the
        // goal is: parse cleanly, rank higher on keyword relevance, and read well for the
        // human. Target a realistic ~75-85% match (high enough to compete, natural enough
        // to still read like a person wrote it) — do NOT push toward keyword stuffing.
        const string system =
            "You are an expert ATS (Applicant Tracking System) resume reviewer. A modern ATS parses a resume and " +
            "ranks it for a human recruiter; it rarely auto-rejects. Your job: help the resume parse cleanly, rank " +
            "higher for this job, and read well for a person. Compare the candidate's structured resume against the " +
            "target job description and return JSON matching the schema.\n" +
            "Scoring rubric (matchScore 0-100): keyword overlap with the job description (weighting terms that appear " +
            "in the title/summary/skills more than those buried in bullets), relevant experience, and ATS-friendliness. " +
            "A strong resume lands around 75-85; do not reward keyword stuffing.\n" +
            "You MUST populate atsChecks with EXACTLY these eight ids, each status pass|warn|fail and a one-line detail:\n" +
            "  - 'quantified': are accomplishments quantified with metrics and percentages (e.g. 'cut latency 40%', " +
            "'led team of 6')? warn if only some bullets are; fail if none are.\n" +
            "  - 'dates': are all start/end dates present and in ONE consistent month-year format (e.g. 'Jun 2021'), " +
            "ranges with an en dash, current role as 'Present'? warn on inconsistency, fail if missing.\n" +
            "  - 'keywords': does the resume cover the key hard skills/tools/title named in the job description? " +
            "prioritise exact wording from the posting.\n" +
            "  - 'keyword_forms': are important acronyms given BOTH spelled-out and short forms (e.g. 'Continuous " +
            "Integration (CI/CD)') so either search matches? warn/fail if only one form is used.\n" +
            "  - 'action_verbs': do bullets start with strong varied action verbs (Led, Built, Reduced) rather than " +
            "'Responsible for', and avoid repeating the same verb on consecutive bullets?\n" +
            "  - 'chronological': is experience in reverse-chronological order (most recent first)? warn/fail otherwise.\n" +
            "  - 'contact': are name, email, phone and location all present (ATS needs these to parse the candidate)?\n" +
            "  - 'sections': are the standard sections (summary, experience, education, skills) all present, non-empty, " +
            "and using conventional headings?\n" +
            "missingKeywords: important hard-skill/tool/title terms from the job description absent from the resume " +
            "(use the posting's exact wording).\n" +
            "summarySuggestion: a rewritten professional-summary paragraph tailored to the job, or empty string if the current one is already strong.";

        var user =
            "JOB DESCRIPTION:\n" + Truncate(jobDescription, 12_000) +
            "\n\nCANDIDATE RESUME (JSON):\n" + JsonSerializer.Serialize(resume, Json);

        logger.LogInformation("Resume ▶ analyzing against a {Chars}-char job description with model '{Model}'…", jobDescription.Length, options.Model);
        // Two focused calls beat one overloaded one on small local models: the assessment call
        // (score / ATS checks / keywords / summary) and a SEPARATE bullet-rewrite call. When we
        // ask for everything at once the model skips the rewrites; asking on its own with an
        // enumerated bullet list reliably produces real rewrites. Plain JSON mode (not a rigid
        // schema) so the model writes genuine content rather than empty schema-required strings.
        var analysis = await ChatAsync<ResumeAnalysis>("analyze", system, user, JsonFormat, ct);
        // Drop any "missing" keyword the resume already contains ANYWHERE (a bullet, the summary,
        // etc.) — the model tends to judge coverage from the Skills section alone and re-suggests
        // terms the candidate already has (e.g. "Kubernetes" that's in an experience bullet).
        var haystack = ResumeHaystack(resume);
        var missing = (analysis.MissingKeywords ?? [])
            .Where(k => !string.IsNullOrWhiteSpace(k) && !haystack.Contains(k, StringComparison.OrdinalIgnoreCase))
            .ToList();
        var suggestions = await RewriteBulletsAsync(resume, jobDescription, missing, ct);
        logger.LogInformation("Resume ✓ analyzed: score {Score}, {Missing} missing keywords, {Suggestions} bullet rewrites", analysis.MatchScore, missing.Count, suggestions.Count);
        return analysis with
        {
            MatchScore = Math.Clamp(analysis.MatchScore, 0, 100),
            Summary = analysis.Summary ?? "",
            MissingKeywords = missing,
            Strengths = analysis.Strengths ?? [],
            AtsChecks = analysis.AtsChecks ?? [],
            BulletSuggestions = suggestions,
            SummarySuggestion = string.IsNullOrWhiteSpace(analysis.SummarySuggestion) ? null : analysis.SummarySuggestion,
        };
    }

    // A focused, single-purpose call that ONLY rewrites weak bullets. Small local models do
    // this far better on its own than buried inside the big analysis prompt. We hand the model
    // an explicitly enumerated bullet list ([exp i | bullet j] text) so it can address each one
    // by index, then map the result back onto the resume (filling Original from the real bullet).
    private async Task<IReadOnlyList<BulletSuggestion>> RewriteBulletsAsync(
        ResumeModel resume, string jobDescription, IReadOnlyList<string> missingKeywords, CancellationToken ct)
    {
        // Nothing to rewrite if there are no bullets.
        if (!resume.Experience.Any(e => e.Bullets.Count > 0)) return [];

        const string system =
            "You tailor resume bullet points to a target job. Return ONLY a JSON object of the form " +
            "{\"suggestions\":[{\"experienceIndex\":0,\"bulletIndex\":0,\"suggested\":\"...\",\"reason\":\"...\"}]}.\n" +
            "Rewrite any bullet that could better match the job — this includes not only weak bullets (vague, no metric, " +
            "starting with 'Responsible for') but also ALREADY-STRONG bullets that could naturally incorporate one of " +
            "the missing job keywords the candidate plausibly has experience with. In 'suggested' give the FULL rewritten " +
            "bullet — lead with a strong action verb, weave in the relevant job keyword(s) naturally, keep any existing " +
            "metric and add numbers/percentages where the candidate's own content supports it — and in 'reason' briefly " +
            "say what improved (e.g. 'adds \"Kubernetes\" keyword').\n" +
            "IMPORTANT — QUANTIFY: for EVERY bullet that contains no number, %, or quantity, you MUST show the " +
            "candidate how to quantify it. You must not invent a fake number, so insert a clearly-bracketed " +
            "PLACEHOLDER exactly where a metric belongs — e.g. '…reducing deployment time by [X]%', 'serving [N] " +
            "users', 'across [N] client projects', 'saving [$Y]/yr', 'cutting build time from [X] to [Y] min' — and " +
            "in 'reason' tell them what to fill in (e.g. 'add the % time saved'). Add the bracketed placeholder even " +
            "when you are also adding a keyword. Every unquantified bullet you return MUST contain at least one " +
            "[bracketed] placeholder metric. Keep it in square brackets so it's obvious they must replace it.\n" +
            "RULES: Give AT MOST ONE suggestion per bullet (never multiple variants of the same bullet). Only add a " +
            "keyword where it AUTHENTICALLY fits that specific accomplishment — never force an unrelated keyword (e.g. " +
            "don't add 'mentoring' to a REST-API bullet) and never stuff several keywords awkwardly into one line; the " +
            "result must read naturally. Skip a bullet that is already well-tailored AND already quantified. Use the " +
            "exact experienceIndex and bulletIndex shown in brackets. NEVER claim tools or experience the candidate " +
            "does not have, and NEVER write a specific fake number — use a [bracketed] placeholder for any metric you " +
            "cannot derive from the candidate's own content.";

        // Enumerate every bullet with its indices so the model targets them unambiguously.
        var sb = new System.Text.StringBuilder();
        sb.Append("JOB DESCRIPTION:\n").Append(Truncate(jobDescription, 8000)).Append('\n');
        if (missingKeywords.Count > 0)
            sb.Append("\nMISSING JOB KEYWORDS to weave in where the candidate honestly has the experience: ")
              .Append(string.Join(", ", missingKeywords.Take(20))).Append('\n');
        sb.Append("\nRESUME BULLETS (tailor the improvable ones):\n");
        for (var i = 0; i < resume.Experience.Count; i++)
        {
            var exp = resume.Experience[i];
            for (var j = 0; j < exp.Bullets.Count; j++)
                sb.Append($"[exp {i} | bullet {j}] ({exp.Role} @ {exp.Company}) {exp.Bullets[j]}\n");
        }

        RewriteResponse? result;
        try
        {
            result = await ChatAsync<RewriteResponse>("rewrite", system, sb.ToString(), JsonFormat, ct);
        }
        catch (ResumeAssistantUnavailableException)
        {
            return []; // rewrites are a bonus — don't fail the whole analysis if this call hiccups
        }

        return (result.Suggestions ?? [])
            .Where(s => !string.IsNullOrWhiteSpace(s.Suggested)
                && s.ExperienceIndex >= 0 && s.ExperienceIndex < resume.Experience.Count
                && resume.Experience[s.ExperienceIndex].Bullets.Count > 0)
            .Select(s =>
            {
                var bi = Math.Clamp(s.BulletIndex, 0, resume.Experience[s.ExperienceIndex].Bullets.Count - 1);
                return new BulletSuggestion(s.ExperienceIndex, bi,
                    resume.Experience[s.ExperienceIndex].Bullets[bi], s.Suggested!, s.Reason ?? "");
            })
            // Drop non-suggestions: the model sometimes emits meta-commentary ("no-op", "I will
            // skip this") as the suggested text, or echoes the bullet back unchanged.
            .Where(s => !LooksLikeMetaCommentary(s.Suggested)
                && !s.Suggested.Trim().Equals(s.Original.Trim(), StringComparison.OrdinalIgnoreCase))
            // At most one rewrite per bullet (the model sometimes emits several variants of one),
            // and cap the total so the panel stays focused.
            .GroupBy(s => (s.ExperienceIndex, s.BulletIndex))
            .Select(g => g.First())
            .Take(6)
            .ToList();
    }

    // Everything the resume says, lowercased into one blob — used to tell whether a "missing"
    // keyword is actually already present somewhere.
    private static string ResumeHaystack(ResumeModel r)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(r.Contact.FullName).Append(' ').Append(r.Contact.Title).Append(' ').Append(r.Summary).Append(' ');
        foreach (var e in r.Experience)
        {
            sb.Append(e.Role).Append(' ').Append(e.Company).Append(' ');
            foreach (var b in e.Bullets) sb.Append(b).Append(' ');
        }
        foreach (var ed in r.Education) sb.Append(ed.Degree).Append(' ').Append(ed.School).Append(' ').Append(ed.Details).Append(' ');
        foreach (var s in r.Skills) sb.Append(s).Append(' ');
        return sb.ToString();
    }

    private sealed record RewriteResponse(List<RewriteItem>? Suggestions);
    private sealed record RewriteItem(int ExperienceIndex, int BulletIndex, string? Suggested, string? Reason);

    // A rewritten bullet should read like a bullet. Reject the model's occasional prose about
    // what it's doing ("this bullet already… I will skip", "no change required", "per instructions").
    private static readonly string[] MetaPhrases =
        ["no-op", "no change", "requires no change", "i will skip", "per instruction", "already contains",
         "already strong", "already well", "the suggestion is", "this bullet", "cannot improve", "leave as"];

    private static bool LooksLikeMetaCommentary(string s)
    {
        var t = s.ToLowerInvariant();
        return MetaPhrases.Any(p => t.Contains(p));
    }

    // ---- Ollama transport ----

    private async Task<T> ChatAsync<T>(string op, string system, string user, JsonElement schema, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();
        // We don't want the model to "think" here — for structured extraction it wastes the
        // context budget (a reasoning model can spend thousands of tokens thinking and then
        // return empty content) and adds latency. Send think:false; if the model doesn't
        // support the flag (older/non-reasoning models 400 on it), retry without it.
        var messages = new[] { new OllamaMessage("system", system), new OllamaMessage("user", user) };
        logger.LogInformation("Resume … POST {BaseUrl}/api/chat ({Op}) — waiting for the local model to respond…", options.BaseUrl, op);
        var res = await SendAsync(new OllamaChatRequest(options.Model, messages, Stream: false, Format: schema,
            Options: new OllamaGenOptions(0.2, NumCtx), Think: false), ct);

        if (res.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var body = await res.Content.ReadAsStringAsync(ct);
            if (body.Contains("think", StringComparison.OrdinalIgnoreCase))
            {
                res.Dispose();
                res = await SendAsync(new OllamaChatRequest(options.Model, messages, Stream: false, Format: schema,
                    Options: new OllamaGenOptions(0.2, NumCtx), Think: null), ct);
            }
        }

        if (!res.IsSuccessStatusCode)
        {
            var body = await res.Content.ReadAsStringAsync(ct);
            // Ollama returns 404 with {"error":"model \"x\" not found"} when the model isn't pulled.
            if (body.Contains("not found", StringComparison.OrdinalIgnoreCase))
                throw new ResumeAssistantUnavailableException(
                    $"The model '{options.Model}' isn't installed. Run `ollama pull {options.Model}`.");
            throw new ResumeAssistantUnavailableException(
                $"The local AI returned an error ({(int)res.StatusCode}). {Truncate(body, 300)}");
        }

        var chat = await res.Content.ReadFromJsonAsync<OllamaChatResponse>(Json, ct);
        var content = chat?.Message?.Content;
        logger.LogInformation("Resume … model responded to {Op} in {Seconds:F1}s ({Chars} chars of JSON)", op, sw.Elapsed.TotalSeconds, content?.Length ?? 0);
        if (string.IsNullOrWhiteSpace(content))
            throw new ResumeAssistantUnavailableException("The local AI returned an empty response. Try again.");

        try
        {
            // Some models wrap the JSON in ```json fences or add a stray sentence despite the
            // schema; take the outermost { … } so we deserialize the object, not the wrapper.
            return JsonSerializer.Deserialize<T>(ExtractJson(content), Json)
                ?? throw new ResumeAssistantUnavailableException("The local AI returned malformed JSON. Try again.");
        }
        catch (JsonException ex)
        {
            throw new ResumeAssistantUnavailableException(
                "The local AI returned output that couldn't be parsed. A larger model (e.g. qwen2.5:14b) is more reliable here.", ex);
        }
    }

    // Pull the outermost JSON object out of the model's reply (strips ``` fences / prose).
    private static string ExtractJson(string content)
    {
        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');
        return start >= 0 && end > start ? content[start..(end + 1)] : content;
    }

    // POST to Ollama, translating connection failures into a clear "unavailable" error.
    private async Task<HttpResponseMessage> SendAsync(OllamaChatRequest request, CancellationToken ct)
    {
        try
        {
            return await http.PostAsJsonAsync("/api/chat", request, Json, ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            throw new ResumeAssistantUnavailableException(
                $"Couldn't reach the local AI at {options.BaseUrl}. Start it with `ollama serve`.", ex);
        }
    }

    // Guard against a partial parse producing null strings/collections downstream (React
    // controlled inputs and the renderer both expect strings, not nulls).
    private static ResumeModel Normalize(ResumeModel m)
    {
        var c = m.Contact ?? ResumeModel.Empty.Contact;
        return m with
        {
            Contact = new ResumeContact(N(c.FullName), N(c.Title), N(c.Email), N(c.Phone), N(c.Location), N(c.Website)),
            Summary = N(m.Summary),
            Experience = (m.Experience ?? []).Select(e => new ResumeExperience(
                N(e.Company), N(e.Role), N(e.StartDate), N(e.EndDate), N(e.Location),
                (e.Bullets ?? []).Where(b => !string.IsNullOrWhiteSpace(b)).Select(CleanBullet).ToList())).ToList(),
            Education = (m.Education ?? []).Select(ed => new ResumeEducation(
                N(ed.School), N(ed.Degree), N(ed.StartDate), N(ed.EndDate), N(ed.Details))).ToList(),
            Skills = (m.Skills ?? []).Where(s => !string.IsNullOrWhiteSpace(s)).ToList(),
        };
    }

    private static string N(string? s) => s ?? "";

    // Strip a leading bullet glyph ("- ", "• ", "* ", "· ") the model may have copied
    // verbatim, so the renderer's own bullet isn't doubled up.
    private static string CleanBullet(string b) => b.TrimStart(' ', '\t', '-', '•', '*', '·', '•').Trim();

    private static string Truncate(string s, int max) => s.Length <= max ? s : s[..max];

    // ---- Ollama wire DTOs ----

    private sealed record OllamaChatRequest(
        string Model,
        IReadOnlyList<OllamaMessage> Messages,
        bool Stream,
        JsonElement Format,
        OllamaGenOptions Options,
        bool? Think);

    private sealed record OllamaMessage(string Role, string Content);

    private sealed record OllamaGenOptions(
        double Temperature,
        [property: JsonPropertyName("num_ctx")] int NumCtx);

    private sealed record OllamaChatResponse(OllamaMessage? Message);

    // ---- JSON Schemas (structured outputs) ----

    private static readonly JsonElement ResumeSchema = JsonSerializer.Deserialize<JsonElement>("""
    {
      "type": "object",
      "properties": {
        "contact": {
          "type": "object",
          "properties": {
            "fullName": { "type": "string" },
            "title": { "type": "string" },
            "email": { "type": "string" },
            "phone": { "type": "string" },
            "location": { "type": "string" },
            "website": { "type": "string" }
          },
          "required": ["fullName", "title", "email", "phone", "location", "website"]
        },
        "summary": { "type": "string" },
        "experience": {
          "type": "array",
          "items": {
            "type": "object",
            "properties": {
              "company": { "type": "string" },
              "role": { "type": "string" },
              "startDate": { "type": "string" },
              "endDate": { "type": "string" },
              "location": { "type": "string" },
              "bullets": { "type": "array", "items": { "type": "string" } }
            },
            "required": ["company", "role", "startDate", "endDate", "location", "bullets"]
          }
        },
        "education": {
          "type": "array",
          "items": {
            "type": "object",
            "properties": {
              "school": { "type": "string" },
              "degree": { "type": "string" },
              "startDate": { "type": "string" },
              "endDate": { "type": "string" },
              "details": { "type": "string" }
            },
            "required": ["school", "degree", "startDate", "endDate", "details"]
          }
        },
        "skills": { "type": "array", "items": { "type": "string" } }
      },
      "required": ["contact", "summary", "experience", "education", "skills"]
    }
    """);
    // Analysis uses plain JSON mode (JsonFormat) rather than a rigid schema — see AnalyzeAsync.
}
