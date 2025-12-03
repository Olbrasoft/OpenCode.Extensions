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
    public DbSet<LegacyMessage> LegacyMessages => Set<LegacyMessage>();
    public DbSet<Participant> Participants => Set<Participant>();
    public DbSet<Provider> Providers => Set<Provider>();
    public DbSet<ModeEntity> Modes => Set<ModeEntity>();
    public DbSet<Monolog> Monologs => Set<Monolog>();
    public DbSet<LegacyMonolog> LegacyMonologs => Set<LegacyMonolog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OpenCodeDbContext).Assembly);

        // For non-PostgreSQL databases (InMemory, SQLite), ignore Vector properties
        // as they require pgvector extension which is PostgreSQL-specific
        if (!Database.IsNpgsql())
        {
            modelBuilder.Entity<Monolog>().Ignore(m => m.Embedding);
            modelBuilder.Entity<LegacyMonolog>().Ignore(m => m.Embedding);
        }
    }
}
