using System.Text.Json;
using System.Text.Json.Serialization;
using InterviewPrep.Application.Resume;

namespace InterviewPrep.Infrastructure.Resume;

// Local models are inconsistent about how they emit the ATS checklist even when given a
// JSON schema: some return the intended array of { id, label, status, detail }, others
// collapse it into a map of { id: "warn", ... } or { id: { status, detail } }. This
// converter normalises all of those into a clean AtsCheck list so a model's formatting
// quirk never fails the whole analysis. It also backfills a friendly label from the id.
public sealed class AtsCheckListConverter : JsonConverter<IReadOnlyList<AtsCheck>>
{
    public override IReadOnlyList<AtsCheck> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var checks = new List<AtsCheck>();

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            // Intended shape: [ { id, label, status, detail }, ... ]
            while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
            {
                using var doc = JsonDocument.ParseValue(ref reader);
                var e = doc.RootElement;
                var id = GetString(e, "id");
                checks.Add(new AtsCheck(
                    id,
                    Coalesce(GetString(e, "label"), LabelFor(id)),
                    ParseStatus(GetString(e, "status")),
                    GetString(e, "detail")));
            }
        }
        else if (reader.TokenType == JsonTokenType.StartObject)
        {
            // Collapsed shape: { "quantified": "warn", "dates": { "status": "pass", "detail": "…" } }
            while (reader.Read() && reader.TokenType != JsonTokenType.EndObject)
            {
                var id = reader.GetString() ?? "";
                reader.Read();
                string status, detail = "";
                if (reader.TokenType == JsonTokenType.StartObject)
                {
                    using var doc = JsonDocument.ParseValue(ref reader);
                    status = GetString(doc.RootElement, "status");
                    detail = GetString(doc.RootElement, "detail");
                }
                else
                {
                    status = reader.TokenType == JsonTokenType.String ? reader.GetString() ?? "" : "";
                }
                checks.Add(new AtsCheck(id, LabelFor(id), ParseStatus(status), detail));
            }
        }
        else
        {
            reader.Skip();
        }

        return checks;
    }

    public override void Write(Utf8JsonWriter writer, IReadOnlyList<AtsCheck> value, JsonSerializerOptions options)
    {
        writer.WriteStartArray();
        foreach (var c in value)
        {
            writer.WriteStartObject();
            writer.WriteString("id", c.Id);
            writer.WriteString("label", c.Label);
            writer.WriteString("status", c.Status.ToString());
            writer.WriteString("detail", c.Detail);
            writer.WriteEndObject();
        }
        writer.WriteEndArray();
    }

    private static string GetString(JsonElement e, string prop) =>
        e.ValueKind == JsonValueKind.Object && e.TryGetProperty(prop, out var v) && v.ValueKind == JsonValueKind.String
            ? v.GetString() ?? ""
            : "";

    private static string Coalesce(string a, string b) => string.IsNullOrWhiteSpace(a) ? b : a;

    private static AtsCheckStatus ParseStatus(string s) => s.Trim().ToLowerInvariant() switch
    {
        "pass" or "passed" or "ok" or "good" => AtsCheckStatus.Pass,
        "fail" or "failed" or "missing" or "bad" => AtsCheckStatus.Fail,
        _ => AtsCheckStatus.Warn,
    };

    // Friendly labels for the ids the analysis prompt asks for; unknown ids fall back to
    // a title-cased version of the id so the UI still reads well.
    private static string LabelFor(string id) => id switch
    {
        "quantified" => "Quantified achievements (metrics & %)",
        "dates" => "Consistent date formatting",
        "keywords" => "Keyword match vs. job description",
        "keyword_forms" => "Acronyms spelled out (e.g. CI/CD)",
        "action_verbs" => "Strong action verbs",
        "chronological" => "Reverse-chronological order",
        "contact" => "Complete contact details",
        "sections" => "Standard sections present",
        _ => Title(id),
    };

    private static string Title(string id) =>
        string.IsNullOrWhiteSpace(id) ? "Check"
        : char.ToUpperInvariant(id[0]) + id[1..].Replace('_', ' ');
}
