using InterviewPrep.Application.Grading;
using Microsoft.Extensions.DependencyInjection;

namespace InterviewPrep.Application;

// Composition root for the Application layer — the use-case services.
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Orchestrator that ties the repository + code runner together.
        services.AddScoped<IGradingService, GradingService>();

        // TimeProvider.System is the real clock. Registering the abstraction (rather
        // than calling DateTime.UtcNow directly) keeps time-dependent logic testable.
        services.AddSingleton(TimeProvider.System);

        return services;
    }
}
