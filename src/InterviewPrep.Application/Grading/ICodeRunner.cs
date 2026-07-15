using InterviewPrep.Domain.Grading;

namespace InterviewPrep.Application.Grading;

// The "seam" that hides HOW code is compiled and executed. Today the only
// implementation is the in-process Roslyn runner (Infrastructure); tomorrow we
// could drop in a kill-able out-of-process runner for real sandboxing WITHOUT
// touching the Application layer. That swappability is the whole point of the
// interface — Application depends on this abstraction, not on Roslyn.
public interface ICodeRunner
{
    // Compile the user source together with the harness + assert shim, run the
    // harness, and return a structured result. Never throws for user-code errors
    // (those become CompileError/RuntimeError/Timeout results); only throws if the
    // exercise's OWN harness is broken (a server bug).
    GradeResult Run(RunRequest request);
}

// Everything the runner needs, with no dependency on EF entities — a clean DTO.
public record RunRequest(
    string UserSource,   // the learner's submitted code
    string HarnessCode,  // the exercise's hidden __Harness class
    int TimeoutSeconds); // per-run execution budget
