using InterviewPrep.Application.Grading;
using Microsoft.Extensions.DependencyInjection;

namespace InterviewPrep.CodeExecution;

// Registers the code-execution layer: one IExerciseRunner per language. Runners are
// stateless and thread-safe (each Run() builds its own isolated context), so
// singletons are fine. GradingService picks the right one by the exercise's Language.
public static class DependencyInjection
{
    public static IServiceCollection AddCodeExecution(this IServiceCollection services)
    {
        services.AddSingleton<IExerciseRunner, RoslynCodeRunner>(); // C#
        services.AddSingleton<IExerciseRunner, SqlRunner>();        // SQL via SQLite
        services.AddSingleton<IExerciseRunner, RuleRunner>();       // Dockerfile/YAML rules
        return services;
    }
}
