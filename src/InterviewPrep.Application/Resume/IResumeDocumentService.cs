namespace InterviewPrep.Application.Resume;

// Reads uploaded resume files into plain text, and renders the structured model back
// out to downloadable documents. Implemented in Infrastructure (PdfPig/OpenXml for
// reading; QuestPDF/OpenXml for writing). All rendered output is single-column with
// standard fonts and no tables/graphics — the format ATS parsers handle cleanly.
public interface IResumeDocumentService
{
    // Extract raw text from an uploaded file, dispatching on the file name's extension
    // (.pdf / .docx / .txt / .md). Throws UnsupportedResumeFormatException for others.
    string ExtractText(Stream file, string fileName);

    // Render the structured resume to a downloadable document in the requested template
    // and format. Returns the raw bytes plus the MIME type + suggested download filename.
    RenderedResume Render(ResumeModel model, string template, ResumeFormat format);
}

public enum ResumeFormat
{
    Pdf,
    Docx,
}

// The bytes of a rendered resume plus the metadata the endpoint needs to serve it.
public sealed record RenderedResume(byte[] Bytes, string ContentType, string FileName);

// Thrown when an uploaded file's extension isn't one we can parse.
public sealed class UnsupportedResumeFormatException(string fileName)
    : Exception($"Unsupported resume file type: '{fileName}'. Upload a PDF, DOCX, or plain-text file.");
