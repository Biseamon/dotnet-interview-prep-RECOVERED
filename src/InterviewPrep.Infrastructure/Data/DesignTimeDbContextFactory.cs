using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace InterviewPrep.Infrastructure.Data;

// Used ONLY by the EF Core CLI tools (`dotnet ef migrations add`, `database update`)
// at DESIGN time. Those tools need to construct an AppDbContext without running the
// full API host, so we hand them a context configured with the local dev connection.
// It is never used at runtime — the app uses the DI-registered context instead.
public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            // Matches docker-compose.yml credentials.
            .UseNpgsql("Host=localhost;Port=5432;Database=dojo;Username=dojo;Password=dojo_dev_pw")
            .Options;

        return new AppDbContext(options);
    }
}
