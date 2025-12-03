using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore;

namespace OpenCode.Extensions.Data.EntityFrameworkCore;

/// <summary>
/// Factory for creating DbContext at design time (for EF Core migrations).
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<OpenCodeDbContext>
{
    public OpenCodeDbContext CreateDbContext(string[] args)
    {
        // Build configuration from appsettings.json in the Api project
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../OpenCode.Extensions.Api"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var optionsBuilder = new DbContextOptionsBuilder<OpenCodeDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new OpenCodeDbContext(optionsBuilder.Options);
    }
}
