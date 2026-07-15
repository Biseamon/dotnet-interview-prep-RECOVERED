// =============================================================================
// InterviewPrep.Api — application entry point (ASP.NET Core minimal API host).
// Composition root: wires the Application, Infrastructure, and CodeExecution
// layers, applies migrations + seeds content, warms up Roslyn, and maps endpoints.
// =============================================================================

using InterviewPrep.Api.Endpoints;
using InterviewPrep.Application;
using InterviewPrep.CodeExecution;
using InterviewPrep.Infrastructure;
using InterviewPrep.Infrastructure.Data;
using InterviewPrep.Infrastructure.Data.Seeding;
using InterviewPrep.Infrastructure.Resume;
using Microsoft.EntityFrameworkCore;

// QuestPDF is free under the Community license for individuals / small businesses.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// --- Configuration: the Postgres connection string (appsettings + env overrides) ---
var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Missing 'Postgres' connection string.");

// --- Resume builder: local Ollama connection (base URL + model), config-driven ---
var ollama = new OllamaOptions(
    builder.Configuration["Ollama:BaseUrl"] ?? "http://localhost:11434",
    builder.Configuration["Ollama:Model"] ?? "llama3.1:8b");

// --- CORS for the Vite dev server (different origin during development) ---
const string DevCorsPolicy = "DevCors";
builder.Services.AddCors(options =>
    options.AddPolicy(DevCorsPolicy, policy => policy
        .WithOrigins("http://localhost:5173")
        .AllowAnyHeader()
        .AllowAnyMethod()));

// --- Register each layer via its own composition-root extension ---
builder.Services.AddApplication();                            // grading service, TimeProvider
builder.Services.AddInfrastructure(connectionString, ollama); // EF Core, repo, seeder, resume services
builder.Services.AddCodeExecution();                          // Roslyn code runner (ICodeRunner)

// Serialize enums as their string names in JSON (e.g. "Passed" not 4) so the
// frontend can switch on readable values.
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

var app = builder.Build();

app.UseCors(DevCorsPolicy);

// --- Startup: apply migrations, seed content, warm Roslyn ---
await using (var scope = app.Services.CreateAsyncScope())
{
    var services = scope.ServiceProvider;

    // Apply any pending EF Core migrations so the schema is current.
    var db = services.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();

    // Seed authored content (idempotent — skips topics already present).
    var seeder = services.GetRequiredService<ContentSeeder>();
    await seeder.SeedAsync(SeedCatalog.All);
    await seeder.SeedDrillsAsync(); // interview-drill question bank
}

// Warm up Roslyn on a background thread so the first real submission is fast
// (first compile carries a ~1-2s JIT/warmup cost). Fire-and-forget is fine here.
_ = Task.Run(RoslynCodeRunner.Warmup);

// --- Endpoints ---
app.MapGet("/api/health", () => Results.Ok(new { status = "ok" }));
app.MapContentEndpoints();
app.MapResumeEndpoints();

app.Run();

// Exposed for the integration test project (WebApplicationFactory<Program>).
public partial class Program { }
