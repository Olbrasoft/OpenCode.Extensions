using Microsoft.EntityFrameworkCore;
using Olbrasoft.OpenCode.Extensions.Data.Entities;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore;

/// <summary>
/// Database context for OpenCode Extensions.
/// </summary>
public class OpenCodeDbContext : DbContext
{
    public OpenCodeDbContext(DbContextOptions<OpenCodeDbContext> options)
        : base(options)
    {
    }

    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Provider> Providers => Set<Provider>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OpenCodeDbContext).Assembly);
    }
}
