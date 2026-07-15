using InterviewPrep.Infrastructure.Data.Seeding.Ai;
using InterviewPrep.Infrastructure.Data.Seeding.Algorithms;
using InterviewPrep.Infrastructure.Data.Seeding.Architecture;
using InterviewPrep.Infrastructure.Data.Seeding.AspNet;
using InterviewPrep.Infrastructure.Data.Seeding.CleanCode;
using InterviewPrep.Infrastructure.Data.Seeding.Concurrency;
using InterviewPrep.Infrastructure.Data.Seeding.DevOps;
using InterviewPrep.Infrastructure.Data.Seeding.EfCore;
using InterviewPrep.Infrastructure.Data.Seeding.Microservices;
using InterviewPrep.Infrastructure.Data.Seeding.Enterprise;
using InterviewPrep.Infrastructure.Data.Seeding.Language;
using InterviewPrep.Infrastructure.Data.Seeding.Memory;
using InterviewPrep.Infrastructure.Data.Seeding.Patterns;
using InterviewPrep.Infrastructure.Data.Seeding.Solid;
using InterviewPrep.Infrastructure.Data.Seeding.Sql;
using InterviewPrep.Infrastructure.Data.Seeding.SystemDesign;
using InterviewPrep.Infrastructure.Data.Seeding.Testing;

namespace InterviewPrep.Infrastructure.Data.Seeding;

// The master list of all authored topics. Each topic lives in its own file/folder
// (AsyncContent, AlgorithmsContent, and later DesignPatterns, Multithreading, GC)
// and is aggregated here. The seeder reads this at startup and inserts anything missing.
public static class SeedCatalog
{
    public static IReadOnlyList<TopicSeed> All =>
    [
        AsyncContent.Topic,
        AlgorithmsContent.Topic,
        DesignPatternsContent.Topic,
        MultithreadingContent.Topic,
        GarbageCollectionContent.Topic,
        CSharpContent.Topic,
        AspNetContent.Topic,
        EfCoreContent.Topic,
        SolidContent.Topic,
        EnterprisePatternsContent.Topic,
        SystemDesignContent.Topic,
        ArchitectureContent.Topic,
        SqlContent.Topic,
        DevOpsContent.Topic,
        AiContent.Topic,
        TestingContent.Topic,
        MicroservicesContent.Topic,
        CleanCodeContent.Topic,
    ];
}
