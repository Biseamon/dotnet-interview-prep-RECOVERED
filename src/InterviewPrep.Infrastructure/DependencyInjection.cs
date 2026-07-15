using InterviewPrep.Application.Content;
using InterviewPrep.Application.Resume;
using InterviewPrep.Infrastructure.Data;
using InterviewPrep.Infrastructure.Data.Seeding;
using InterviewPrep.Infrastructure.Resume;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace InterviewPrep.Infrastructure;

// Composition root for the Infrastructure layer. The API calls this one method to
// register everything infra-related, keeping Program.cs clean and hiding EF/provider
// details behind the abstraction boundary.
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services, string connectionString, OllamaOptions ollama)
    {
        // Register the EF Core DbContext against PostgreSQL (Npgsql provider).
        // Scoped lifetime (the default) = one DbContext per HTTP request, which is
        // the correct unit-of-work boundary for a web app.
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString));

        // Repository + seeder. Scoped so they share the request's DbContext.
        services.AddScoped<IContentRepository, EfContentRepository>();
        services.AddScoped<ContentSeeder>();

        // --- Resume builder ---
        // The local Ollama connection settings (base URL + model), bound from config.
        services.AddSingleton(ollama);
        // Typed HttpClient for the on-device AI. Generous timeout: local generation
        // over a full resume + job description can take a while on a laptop.
        services.AddHttpClient<IResumeAssistant, OllamaResumeAssistant>(c =>
        {
            c.BaseAddress = new Uri(ollama.BaseUrl);
            c.Timeout = TimeSpan.FromMinutes(5);
        });
        // Document read (PdfPig/OpenXml) + render (QuestPDF/OpenXml). Stateless → singleton.
        services.AddSingleton<IResumeDocumentService, ResumeDocumentService>();

        return services;
    }
}
