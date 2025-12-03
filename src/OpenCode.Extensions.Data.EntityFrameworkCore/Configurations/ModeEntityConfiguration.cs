using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Olbrasoft.OpenCode.Extensions.Data.Entities;
using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.Configurations;

public class ModeEntityConfiguration : IEntityTypeConfiguration<ModeEntity>
{
    public void Configure(EntityTypeBuilder<ModeEntity> builder)
    {
        builder.ToTable("Modes");

        builder.HasKey(m => m.Id);
        
        // No auto-increment - we control the IDs to match enum values
        builder.Property(m => m.Id)
            .ValueGeneratedNever();

        builder.Property(m => m.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(m => m.Name)
            .IsUnique();

        // Seed data
        builder.HasData(
            new ModeEntity { Id = (int)Mode.Build, Name = nameof(Mode.Build) },
            new ModeEntity { Id = (int)Mode.Plan, Name = nameof(Mode.Plan) }
        );
    }
}
