using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Olbrasoft.OpenCode.Extensions.Data.Entities;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.Configurations;

/// <summary>
/// EF Core configuration for LegacyMessage entity.
/// </summary>
public class LegacyMessageConfiguration : IEntityTypeConfiguration<LegacyMessage>
{
    public void Configure(EntityTypeBuilder<LegacyMessage> builder)
    {
        builder.ToTable("LegacyMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.MessageId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.SessionId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.ParentMessageId)
            .HasMaxLength(100);

        builder.Property(m => m.Content)
            .IsRequired();

        builder.Property(m => m.ParticipantIdentifier)
            .HasMaxLength(100);

        builder.Property(m => m.ProviderName)
            .HasMaxLength(100);

        builder.Property(m => m.Cost)
            .HasPrecision(18, 8);

        builder.HasIndex(m => m.MessageId)
            .HasDatabaseName("IX_LegacyMessages_MessageId");

        builder.HasIndex(m => m.SessionId)
            .HasDatabaseName("IX_LegacyMessages_SessionId");
    }
}
