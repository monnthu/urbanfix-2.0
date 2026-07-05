using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace UrbanFix.API.Data;

public class ReportContextFactory : IDesignTimeDbContextFactory<ReportContext>
{
    public ReportContext CreateDbContext(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{environment}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection no configurada.");

        var optionsBuilder = new DbContextOptionsBuilder<ReportContext>();
        optionsBuilder.UseNpgsql(connectionString);
        return new ReportContext(optionsBuilder.Options);
    }
}
