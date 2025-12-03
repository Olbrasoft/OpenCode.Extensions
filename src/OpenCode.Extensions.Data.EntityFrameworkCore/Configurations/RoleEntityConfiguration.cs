using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Olbrasoft.OpenCode.Extensions.Data.Entities;
using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.Configurations;

public class RoleEntityConfiguration : IEntityTypeConfiguration<RoleEntity>
{
    public void Configure(EntityTypeBuilder<RoleEntity> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(r => r.Id);
        
        // No auto-increment - we control the IDs to match enum values
        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(r => r.Name)
            .IsUnique();

        // Seed data
        builder.HasData(
            new RoleEntity { Id = (int)Role.User, Name = nameof(Role.User) },
            new RoleEntity { Id = (int)Role.Assistant, Name = nameof(Role.Assistant) }
        );
    }
}
