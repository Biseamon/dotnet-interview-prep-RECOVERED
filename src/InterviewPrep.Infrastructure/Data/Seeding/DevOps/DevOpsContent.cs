using System.Text.Json;

namespace InterviewPrep.Infrastructure.Data.Seeding.DevOps;

// The "DevOps" topic — you write real Dockerfiles, CI workflows, and Kubernetes
// manifests. The RuleRunner (Language = Config) can't spin up infrastructure, so it
// grades your file against STRUCTURAL rules (each a regex that must be present). This
// teaches the required shape of these files — exactly what interviews probe.
internal static class DevOpsContent
{
    // Build the harness: a JSON list of named regex rules the submission must satisfy.
    private static string RuleHarness(params (string name, string pattern)[] rules) =>
        JsonSerializer.Serialize(new
        {
            rules = rules.Select(r => new { name = r.name, pattern = r.pattern, hidden = false }),
        });

    private static ExerciseSeed Cfg(string slug, string title, string difficulty, string prompt,
        string starter, string reference, (string name, string pattern)[] rules, string[] hints) => new()
    {
        Slug = slug, Title = title, Difficulty = difficulty, Kind = "Function", Language = "Config",
        TimeoutSeconds = 5, Prompt = prompt, StarterCode = starter,
        HarnessCode = RuleHarness(rules),
        ReferenceSolution = reference,
        Hints = hints.ToList(),
        TestCases = rules.Select(r => new TestCaseSeed { Name = r.name, IsHidden = false }).ToList(),
    };

    public static TopicSeed Topic => new()
    {
        Slug = "devops",
        Name = "DevOps & Containers",
        Description = "Write real Dockerfiles, CI pipelines, and Kubernetes manifests — checked against the structure they must have.",
        Order = 14,
        Lessons =
        [
            new LessonSeed
            {
                Slug = "containers-ci", Title = "Containers & CI", Order = 1,
                MarkdownContent =
                    """
                    ## Containers & CI

                    A **Dockerfile** describes how to build your app image (multi-stage: build
                    with the SDK, run on a slim runtime). **docker-compose** wires multiple
                    containers together locally. A **CI workflow** (GitHub Actions) builds and
                    tests every push. You write the file; grading checks it has the required
                    instructions.
                    """,
                Exercises =
                [
                    Cfg("dockerfile-dotnet", "Multi-stage Dockerfile", "Medium",
                        "Write a **multi-stage Dockerfile** for a .NET app: build with the SDK image (restore + publish), then run on the slim ASP.NET runtime image with an ENTRYPOINT.",
                        "# Stage 1: build with the .NET SDK\nFROM \nWORKDIR /src\nCOPY . .\nRUN \nRUN \n\n# Stage 2: slim runtime\nFROM \nWORKDIR /app\nCOPY --from=build /app .\nENTRYPOINT ",
                        "FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build\nWORKDIR /src\nCOPY . .\nRUN dotnet restore\nRUN dotnet publish -c Release -o /app\n\nFROM mcr.microsoft.com/dotnet/aspnet:9.0\nWORKDIR /app\nCOPY --from=build /app .\nENTRYPOINT [\"dotnet\", \"MyApp.dll\"]",
                        [
                            ("builds with the .NET SDK image", @"FROM\s+\S*dotnet/sdk"),
                            ("restores packages", @"dotnet\s+restore"),
                            ("publishes the app", @"dotnet\s+publish"),
                            ("runs on the slim ASP.NET runtime image", @"FROM\s+\S*dotnet/aspnet"),
                            ("defines an ENTRYPOINT", @"ENTRYPOINT"),
                        ],
                        ["Stage 1 FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build.", "RUN dotnet restore, then RUN dotnet publish -c Release -o /app.", "Stage 2 FROM …/dotnet/aspnet:9.0, then ENTRYPOINT [\"dotnet\", \"MyApp.dll\"]."]),

                    Cfg("github-actions-ci", "GitHub Actions CI", "Medium",
                        "Write a **GitHub Actions** workflow that triggers on push, checks out the code, sets up .NET, then builds and tests.",
                        "name: CI\non:\n  \njobs:\n  build:\n    runs-on: \n    steps:\n      - uses: \n      - uses: \n      - run: \n      - run: ",
                        "name: CI\non:\n  push:\n    branches: [ main ]\njobs:\n  build:\n    runs-on: ubuntu-latest\n    steps:\n      - uses: actions/checkout@v4\n      - uses: actions/setup-dotnet@v4\n        with:\n          dotnet-version: '9.0.x'\n      - run: dotnet build\n      - run: dotnet test",
                        [
                            ("triggers on push", @"push:"),
                            ("defines a jobs section", @"jobs:"),
                            ("runs on an OS runner", @"runs-on:"),
                            ("checks out the repository", @"actions/checkout"),
                            ("sets up .NET", @"actions/setup-dotnet"),
                            ("builds the project", @"dotnet\s+build"),
                            ("runs the tests", @"dotnet\s+test"),
                        ],
                        ["`on:` → `push:`; then a `jobs:` section.", "Steps: actions/checkout, actions/setup-dotnet.", "Two run steps: `dotnet build` and `dotnet test`."]),

                    Cfg("docker-compose", "docker-compose", "Medium",
                        "Write a **docker-compose** file with a `web` service (built from the current dir, port exposed) and a `db` service running Postgres, where web `depends_on` db.",
                        "services:\n  web:\n    build: \n    ports:\n      - \n    depends_on:\n      - \n  db:\n    image: ",
                        "services:\n  web:\n    build: .\n    ports:\n      - \"8080:8080\"\n    depends_on:\n      - db\n  db:\n    image: postgres:17\n    environment:\n      POSTGRES_PASSWORD: dev",
                        [
                            ("declares services", @"services:"),
                            ("exposes a port", @"ports:"),
                            ("web depends on db", @"depends_on:"),
                            ("runs a postgres image", @"image:\s*postgres"),
                        ],
                        ["Top-level `services:` with `web` and `db`.", "web has `build: .`, `ports:`, and `depends_on: [db]`.", "db uses `image: postgres:17`."]),
                ],
            },
            new LessonSeed
            {
                Slug = "kubernetes", Title = "Kubernetes", Order = 2,
                MarkdownContent =
                    """
                    ## Kubernetes

                    A **Deployment** declares how many replicas of your container to run and
                    keeps them healthy. A **Service** gives those pods a stable address and load-
                    balances traffic to them (matched by labels). You write the manifest; grading
                    checks the required fields are present.
                    """,
                Exercises =
                [
                    Cfg("k8s-deployment", "Kubernetes Deployment", "Medium",
                        "Write a **Deployment** manifest: 3 replicas of a container from image `myapp:1.0` exposing containerPort 8080.",
                        "apiVersion: apps/v1\nkind: \nmetadata:\n  name: web\nspec:\n  replicas: \n  selector:\n    matchLabels:\n      app: web\n  template:\n    metadata:\n      labels:\n        app: web\n    spec:\n      containers:\n        - name: web\n          image: \n          ports:\n            - containerPort: ",
                        "apiVersion: apps/v1\nkind: Deployment\nmetadata:\n  name: web\nspec:\n  replicas: 3\n  selector:\n    matchLabels:\n      app: web\n  template:\n    metadata:\n      labels:\n        app: web\n    spec:\n      containers:\n        - name: web\n          image: myapp:1.0\n          ports:\n            - containerPort: 8080",
                        [
                            ("is a Deployment", @"kind:\s*Deployment"),
                            ("sets replicas", @"replicas:\s*\d"),
                            ("defines containers", @"containers:"),
                            ("specifies an image", @"image:\s*\S+"),
                            ("exposes a container port", @"containerPort:\s*\d"),
                        ],
                        ["`kind: Deployment`, `spec.replicas: 3`.", "A `containers:` list with `name`, `image: myapp:1.0`.", "`ports: - containerPort: 8080`."]),

                    Cfg("k8s-service", "Kubernetes Service", "Medium",
                        "Write a **Service** manifest that selects pods with label `app: web` and maps port 80 to the pods' targetPort 8080.",
                        "apiVersion: v1\nkind: \nmetadata:\n  name: web\nspec:\n  selector:\n    app: web\n  ports:\n    - port: \n      targetPort: ",
                        "apiVersion: v1\nkind: Service\nmetadata:\n  name: web\nspec:\n  selector:\n    app: web\n  ports:\n    - port: 80\n      targetPort: 8080",
                        [
                            ("is a Service", @"kind:\s*Service"),
                            ("has a selector", @"selector:"),
                            ("maps a service port", @"port:\s*\d"),
                            ("targets the container port", @"targetPort:\s*\d"),
                        ],
                        ["`kind: Service`, with a `selector:` of `app: web`.", "`ports:` with `port: 80` and `targetPort: 8080`."]),
                ],
            },
        ],
    };
}
