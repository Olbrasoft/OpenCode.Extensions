using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore;

/// <summary>
/// Factory for creating DbContext at design time (for EF Core migrations).
/// </summary>
public class OpenCodeDbContextFactory : IDesignTimeDbContextFactory<OpenCodeDbContext>
{
    public OpenCodeDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<OpenCodeDbContext>();
        
        // Use a default connection string for design time
        // This is only used by EF Core tools (migrations), not at runtime
        optionsBuilder.UseNpgsql(
            "Host=localhost;Database=opencode;Username=jirka;Password=jirka",
            o => o.UseVector());

        return new OpenCodeDbContext(optionsBuilder.Options);
    }
}
