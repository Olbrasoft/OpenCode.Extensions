using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Olbrasoft.OpenCode.Extensions.Data.Entities;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.Configurations;

public class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Label)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Identifier)
            .HasMaxLength(200)
            .IsRequired();

        // Enum → int conversion
        builder.Property(p => p.Type)
            .HasConversion<int>();

        builder.HasIndex(p => p.Identifier)
            .IsUnique();
    }
}
