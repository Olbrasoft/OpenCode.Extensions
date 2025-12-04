using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Olbrasoft.OpenCode.Extensions.Data.Entities;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.Configurations;

/// <summary>
/// EF Core configuration for LegacyMonolog entity.
/// No constraints - this is a "trash bin" for rejected monologs.
/// </summary>
public class LegacyMonologConfiguration : IEntityTypeConfiguration<LegacyMonolog>
{
    public void Configure(EntityTypeBuilder<LegacyMonolog> builder)
    {
        builder.ToTable("LegacyMonologs");

        builder.HasKey(m => m.Id);

        // All string fields - no FK validation
        builder.Property(m => m.SessionId)
            .HasMaxLength(100);

        builder.Property(m => m.ParentMonologId)
            .HasMaxLength(100);

        builder.Property(m => m.FirstMessageId)
            .HasMaxLength(100);

        builder.Property(m => m.LastMessageId)
            .HasMaxLength(100);

        builder.Property(m => m.ParticipantIdentifier)
            .HasMaxLength(200);

        builder.Property(m => m.ProviderName)
            .HasMaxLength(100);

        builder.Property(m => m.ModeName)
            .HasMaxLength(50);

        // Vector embedding - explicitly configure to avoid constructor binding issues
        builder.Property(m => m.Embedding)
            .HasColumnType("vector(1536)");

        // Cost precision
        builder.Property(m => m.Cost)
            .HasPrecision(18, 8);

        // OriginalPayload as JSONB
        builder.Property(m => m.OriginalPayload)
            .HasColumnType("jsonb");

        // Timestamp
        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Indexes for debugging
        builder.HasIndex(m => m.SessionId)
            .HasDatabaseName("IX_LegacyMonologs_SessionId");

        builder.HasIndex(m => m.FirstMessageId)
            .HasDatabaseName("IX_LegacyMonologs_FirstMessageId");

        builder.HasIndex(m => m.CreatedAt)
            .HasDatabaseName("IX_LegacyMonologs_CreatedAt");
    }
}
