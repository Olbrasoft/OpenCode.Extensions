using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Olbrasoft.OpenCode.Extensions.Data.Entities;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.Configurations;

/// <summary>
/// EF Core configuration for ModeEntity.
/// </summary>
public class ModeEntityConfiguration : IEntityTypeConfiguration<ModeEntity>
{
    public void Configure(EntityTypeBuilder<ModeEntity> builder)
    {
        builder.ToTable("Modes");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(m => m.Description)
            .HasMaxLength(200);

        builder.HasIndex(m => m.Name)
            .IsUnique()
            .HasDatabaseName("IX_Modes_Name");

        // Seed data for Build and Plan modes
        builder.HasData(
            new ModeEntity { Id = 1, Name = "Build", Description = "AI can modify files, run commands (full access)" },
            new ModeEntity { Id = 2, Name = "Plan", Description = "AI only suggests and plans (read-only)" }
        );
    }
}
