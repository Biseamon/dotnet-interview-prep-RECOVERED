using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using InterviewPrep.Application.Resume;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using UglyToad.PdfPig;
// Both QuestPDF and OpenXml define `Color` / `Document`. Alias the OpenXml `Color`
// (used only in the DOCX helpers) and fully-qualify the two `Document` references.
using Color = DocumentFormat.OpenXml.Wordprocessing.Color;

namespace InterviewPrep.Infrastructure.Resume;

// Reads uploaded resumes to text (PdfPig / OpenXml / plain) and renders the structured
// model to downloadable PDF (QuestPDF) and DOCX (OpenXml). Every rendered document is
// single-column, standard fonts, no tables/graphics — the layout ATS parsers read cleanly.
// Templates are typographic variants of one shared layout, so PDF and DOCX stay in sync.
public sealed class ResumeDocumentService : IResumeDocumentService
{
    // ---- Extraction (upload → text) ----

    public string ExtractText(Stream file, string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => ExtractPdf(file),
            ".docx" => ExtractDocx(file),
            ".txt" or ".md" or ".text" => new StreamReader(file).ReadToEnd(),
            _ => throw new UnsupportedResumeFormatException(fileName),
        };
    }

    private static string ExtractPdf(Stream file)
    {
        // PdfPig needs a seekable stream; buffer to a MemoryStream if necessary.
        using var buffer = ToSeekable(file);
        using var pdf = PdfDocument.Open(buffer);
        // ContentOrderTextExtractor preserves reading order better than raw page.Text.
        var pages = pdf.GetPages().Select(p =>
            UglyToad.PdfPig.DocumentLayoutAnalysis.TextExtractor.ContentOrderTextExtractor.GetText(p));
        return string.Join("\n\n", pages);
    }

    private static string ExtractDocx(Stream file)
    {
        using var buffer = ToSeekable(file);
        using var doc = WordprocessingDocument.Open(buffer, false);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body is null) return "";
        // One line per paragraph so bullets/sections survive as separate lines.
        return string.Join("\n", body.Descendants<Paragraph>().Select(p => p.InnerText));
    }

    private static MemoryStream ToSeekable(Stream file)
    {
        var ms = new MemoryStream();
        file.CopyTo(ms);
        ms.Position = 0;
        return ms;
    }

    // ---- Rendering (model → document) ----

    public RenderedResume Render(ResumeModel model, string template, ResumeFormat format)
    {
        var style = TemplateStyle.Resolve(template);
        var safeName = SafeFileName(model.Contact.FullName);
        return format switch
        {
            ResumeFormat.Pdf => new RenderedResume(
                RenderPdf(model, style), "application/pdf", $"{safeName}.pdf"),
            ResumeFormat.Docx => new RenderedResume(
                RenderDocx(model, style),
                "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                $"{safeName}.docx"),
            _ => throw new ArgumentOutOfRangeException(nameof(format)),
        };
    }

    // ---- PDF (QuestPDF) ----

    private static byte[] RenderPdf(ResumeModel m, TemplateStyle s)
    {
        return QuestPDF.Fluent.Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(s.MarginPt);
                page.DefaultTextStyle(t => t.FontFamily(s.BodyFont).FontSize(s.BodySize).FontColor(Colors.Grey.Darken4));

                page.Content().Column(col =>
                {
                    col.Spacing(s.SectionGap);

                    // --- Header: name, title, contact line ---
                    col.Item().Text(m.Contact.FullName).FontFamily(s.HeadingFont).FontSize(s.NameSize).Bold().FontColor(s.Accent);
                    if (!string.IsNullOrWhiteSpace(m.Contact.Title))
                        col.Item().Text(m.Contact.Title).FontSize(s.BodySize + 2).FontColor(Colors.Grey.Darken2);

                    var contactBits = new[] { m.Contact.Email, m.Contact.Phone, m.Contact.Location, m.Contact.Website }
                        .Where(x => !string.IsNullOrWhiteSpace(x));
                    var contactLine = string.Join("  •  ", contactBits);
                    if (contactLine.Length > 0)
                        col.Item().Text(contactLine).FontSize(s.BodySize - 0.5f).FontColor(Colors.Grey.Darken1);

                    // --- Summary ---
                    if (!string.IsNullOrWhiteSpace(m.Summary))
                    {
                        PdfHeading(col, "Summary", s);
                        col.Item().Text(m.Summary);
                    }

                    // --- Experience ---
                    if (m.Experience.Count > 0)
                    {
                        PdfHeading(col, "Experience", s);
                        foreach (var e in m.Experience)
                        {
                            col.Item().PaddingBottom(s.EntryGap).Column(entry =>
                            {
                                entry.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(t =>
                                    {
                                        t.Span(e.Role).SemiBold();
                                        if (!string.IsNullOrWhiteSpace(e.Company)) t.Span($"  —  {e.Company}");
                                    });
                                    var dates = DateRange(e.StartDate, e.EndDate);
                                    if (dates.Length > 0)
                                        row.ConstantItem(130).AlignRight().Text(dates).FontColor(Colors.Grey.Darken1).FontSize(s.BodySize - 0.5f);
                                });
                                if (!string.IsNullOrWhiteSpace(e.Location))
                                    entry.Item().Text(e.Location).Italic().FontSize(s.BodySize - 1).FontColor(Colors.Grey.Darken1);
                                foreach (var b in e.Bullets.Where(b => !string.IsNullOrWhiteSpace(b)))
                                    entry.Item().PaddingTop(1).Row(r =>
                                    {
                                        r.ConstantItem(12).Text("•");
                                        r.RelativeItem().Text(b);
                                    });
                            });
                        }
                    }

                    // --- Education ---
                    if (m.Education.Count > 0)
                    {
                        PdfHeading(col, "Education", s);
                        foreach (var ed in m.Education)
                        {
                            col.Item().PaddingBottom(s.EntryGap / 2).Column(entry =>
                            {
                                entry.Item().Row(row =>
                                {
                                    row.RelativeItem().Text(t =>
                                    {
                                        t.Span(ed.Degree).SemiBold();
                                        if (!string.IsNullOrWhiteSpace(ed.School)) t.Span($"  —  {ed.School}");
                                    });
                                    var dates = DateRange(ed.StartDate, ed.EndDate);
                                    if (dates.Length > 0)
                                        row.ConstantItem(130).AlignRight().Text(dates).FontColor(Colors.Grey.Darken1).FontSize(s.BodySize - 0.5f);
                                });
                                if (!string.IsNullOrWhiteSpace(ed.Details))
                                    entry.Item().Text(ed.Details).FontSize(s.BodySize - 0.5f).FontColor(Colors.Grey.Darken1);
                            });
                        }
                    }

                    // --- Skills (comma-joined, single line block — ATS reads it as text) ---
                    if (m.Skills.Count > 0)
                    {
                        PdfHeading(col, "Skills", s);
                        col.Item().Text(string.Join(", ", m.Skills.Where(x => !string.IsNullOrWhiteSpace(x))));
                    }
                });
            });
        }).GeneratePdf();
    }

    private static void PdfHeading(ColumnDescriptor col, string text, TemplateStyle s)
    {
        var label = s.UppercaseHeadings ? text.ToUpperInvariant() : text;
        var item = col.Item().PaddingTop(s.SectionGap / 2);
        if (s.HeadingRule)
            item = item.BorderBottom(1).BorderColor(Colors.Grey.Lighten1).PaddingBottom(2);
        item.Text(label).FontFamily(s.HeadingFont).FontSize(s.HeadingSize).Bold()
            .LetterSpacing(s.UppercaseHeadings ? 0.06f : 0f).FontColor(s.Accent);
    }

    // ---- DOCX (OpenXml) ----

    private static byte[] RenderDocx(ResumeModel m, TemplateStyle s)
    {
        using var ms = new MemoryStream();
        using (var doc = WordprocessingDocument.Create(ms, WordprocessingDocumentType.Document))
        {
            doc.AddMainDocumentPart().Document = new DocumentFormat.OpenXml.Wordprocessing.Document();
            var b = doc.MainDocumentPart!.Document.AppendChild(new Body());

            // Header
            b.AppendChild(Line(m.Contact.FullName, s.HeadingFont, s.NameSize, bold: true, color: s.AccentHex));
            if (!string.IsNullOrWhiteSpace(m.Contact.Title))
                b.AppendChild(Line(m.Contact.Title, s.BodyFont, s.BodySize + 2, color: "444444"));
            var contactLine = string.Join("  •  ", new[] { m.Contact.Email, m.Contact.Phone, m.Contact.Location, m.Contact.Website }
                .Where(x => !string.IsNullOrWhiteSpace(x)));
            if (contactLine.Length > 0)
                b.AppendChild(Line(contactLine, s.BodyFont, s.BodySize - 1, color: "666666"));

            if (!string.IsNullOrWhiteSpace(m.Summary))
            {
                b.AppendChild(Heading("Summary", s));
                b.AppendChild(Line(m.Summary, s.BodyFont, s.BodySize));
            }

            if (m.Experience.Count > 0)
            {
                b.AppendChild(Heading("Experience", s));
                foreach (var e in m.Experience)
                {
                    var head = e.Role;
                    if (!string.IsNullOrWhiteSpace(e.Company)) head += $"  —  {e.Company}";
                    var dates = DateRange(e.StartDate, e.EndDate);
                    if (dates.Length > 0) head += $"   ({dates})";
                    b.AppendChild(Line(head, s.BodyFont, s.BodySize, bold: true));
                    if (!string.IsNullOrWhiteSpace(e.Location))
                        b.AppendChild(Line(e.Location, s.BodyFont, s.BodySize - 1, italic: true, color: "666666"));
                    foreach (var bullet in e.Bullets.Where(x => !string.IsNullOrWhiteSpace(x)))
                        b.AppendChild(Bullet(bullet, s));
                }
            }

            if (m.Education.Count > 0)
            {
                b.AppendChild(Heading("Education", s));
                foreach (var ed in m.Education)
                {
                    var head = ed.Degree;
                    if (!string.IsNullOrWhiteSpace(ed.School)) head += $"  —  {ed.School}";
                    var dates = DateRange(ed.StartDate, ed.EndDate);
                    if (dates.Length > 0) head += $"   ({dates})";
                    b.AppendChild(Line(head, s.BodyFont, s.BodySize, bold: true));
                    if (!string.IsNullOrWhiteSpace(ed.Details))
                        b.AppendChild(Line(ed.Details, s.BodyFont, s.BodySize - 1, color: "666666"));
                }
            }

            if (m.Skills.Count > 0)
            {
                b.AppendChild(Heading("Skills", s));
                b.AppendChild(Line(string.Join(", ", m.Skills.Where(x => !string.IsNullOrWhiteSpace(x))), s.BodyFont, s.BodySize));
            }
        }
        return ms.ToArray();
    }

    // A normal paragraph with a single styled run.
    private static Paragraph Line(string text, string font, float pt, bool bold = false, bool italic = false, string? color = null)
    {
        var runProps = new RunProperties(new RunFonts { Ascii = font, HighAnsi = font })
        {
            FontSize = new FontSize { Val = ((int)(pt * 2)).ToString() }, // sz is half-points
        };
        if (bold) runProps.Bold = new Bold();
        if (italic) runProps.Italic = new Italic();
        if (color is not null) runProps.Color = new Color { Val = color };

        return new Paragraph(
            new ParagraphProperties(new SpacingBetweenLines { After = "80", Line = "252", LineRule = LineSpacingRuleValues.Auto }),
            new Run(runProps, new Text(text) { Space = SpaceProcessingModeValues.Preserve }));
    }

    // A section heading paragraph (bold, optional uppercase + bottom rule).
    private static Paragraph Heading(string text, TemplateStyle s)
    {
        var label = s.UppercaseHeadings ? text.ToUpperInvariant() : text;
        var pProps = new ParagraphProperties(new SpacingBetweenLines { Before = "160", After = "60" });
        if (s.HeadingRule)
            pProps.ParagraphBorders = new ParagraphBorders(
                new BottomBorder { Val = BorderValues.Single, Size = 4, Color = "BBBBBB", Space = 1 });
        var runProps = new RunProperties(
            new RunFonts { Ascii = s.HeadingFont, HighAnsi = s.HeadingFont },
            new Bold(),
            new Color { Val = s.AccentHex },
            new FontSize { Val = ((int)(s.HeadingSize * 2)).ToString() });
        return new Paragraph(pProps, new Run(runProps, new Text(label)));
    }

    // A bullet line: "• text" with a hanging indent (ATS-safe standard bullet).
    private static Paragraph Bullet(string text, TemplateStyle s)
    {
        var pProps = new ParagraphProperties(
            new Indentation { Left = "360", Hanging = "180" },
            new SpacingBetweenLines { After = "40", Line = "252", LineRule = LineSpacingRuleValues.Auto });
        var runProps = new RunProperties(
            new RunFonts { Ascii = s.BodyFont, HighAnsi = s.BodyFont },
            new FontSize { Val = ((int)(s.BodySize * 2)).ToString() });
        return new Paragraph(pProps, new Run(runProps, new Text($"• {text}") { Space = SpaceProcessingModeValues.Preserve }));
    }

    // ---- Shared helpers ----

    private static string DateRange(string start, string end)
    {
        start = start?.Trim() ?? "";
        end = end?.Trim() ?? "";
        if (start.Length == 0 && end.Length == 0) return "";
        if (start.Length == 0) return end;
        if (end.Length == 0) return start;
        return $"{start} – {end}";
    }

    private static string SafeFileName(string name)
    {
        var trimmed = string.IsNullOrWhiteSpace(name) ? "resume" : name.Trim();
        var cleaned = new string(trimmed.Select(c => char.IsLetterOrDigit(c) ? c : '_').ToArray());
        return $"{cleaned}_Resume".Trim('_');
    }
}

// A template = a set of typographic knobs over the one shared single-column layout.
// All ATS-safe: differences are fonts, sizes, spacing, and heading treatment only.
internal sealed record TemplateStyle(
    string BodyFont, string HeadingFont,
    float BodySize, float NameSize, float HeadingSize,
    float MarginPt, float SectionGap, float EntryGap,
    bool UppercaseHeadings, bool HeadingRule,
    string Accent, string AccentHex)
{
    public static TemplateStyle Resolve(string? slug) => slug switch
    {
        "compact" => new("Arial", "Arial", 9.5f, 18, 11, 30, 6, 5, true, false, Colors.Grey.Darken4, "222222"),
        "modern" => new("Helvetica", "Helvetica", 10, 24, 11.5f, 40, 9, 8, false, false, "#0f766e", "0F766E"),
        _ /* classic */ => new("Times New Roman", "Georgia", 10.5f, 22, 12, 42, 8, 7, true, true, Colors.Grey.Darken4, "222222"),
    };
}
