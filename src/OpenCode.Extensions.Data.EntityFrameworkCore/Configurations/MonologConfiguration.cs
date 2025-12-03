using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Olbrasoft.OpenCode.Extensions.Data.Entities;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.Configurations;

/// <summary>
/// EF Core configuration for Monolog entity.
/// Includes constraints for data integrity and pgvector index for semantic search.
/// </summary>
public class MonologConfiguration : IEntityTypeConfiguration<Monolog>
{
    public void Configure(EntityTypeBuilder<Monolog> builder)
    {
        builder.ToTable("Monologs");

        builder.HasKey(m => m.Id);

        // Session relationship
        builder.HasOne(m => m.Session)
            .WithMany(s => s.Monologs)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-referencing relationship for parent/child monologs
        builder.HasOne(m => m.ParentMonolog)
            .WithMany(m => m.ChildMonologs)
            .HasForeignKey(m => m.ParentMonologId)
            .OnDelete(DeleteBehavior.Restrict);

        // Participant relationship
        builder.HasOne(m => m.Participant)
            .WithMany(p => p.Monologs)
            .HasForeignKey(m => m.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Provider relationship
        builder.HasOne(m => m.Provider)
            .WithMany(p => p.Monologs)
            .HasForeignKey(m => m.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        // ModeEntity relationship
        builder.HasOne(m => m.ModeEntity)
            .WithMany(mode => mode.Monologs)
            .HasForeignKey(m => m.ModeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Required fields
        builder.Property(m => m.FirstMessageId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(m => m.LastMessageId)
            .HasMaxLength(100);

        builder.Property(m => m.Content)
            .IsRequired();

        // Vector embedding handled by Pgvector.EntityFrameworkCore

        // Cost precision
        builder.Property(m => m.Cost)
            .HasPrecision(18, 8);

        // Timestamps
        builder.Property(m => m.StartedAt)
            .IsRequired();

        builder.Property(m => m.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        builder.Property(m => m.UpdatedAt)
            .IsRequired()
            .HasDefaultValueSql("NOW()");

        // Constraint: Assistant monolog MUST have parent
        builder.ToTable(t => t.HasCheckConstraint(
            "CHK_Assistant_Has_Parent",
            "\"Role\" = 1 OR \"ParentMonologId\" IS NOT NULL"));

        // Constraint: Closed monolog MUST have LastMessageId
        builder.ToTable(t => t.HasCheckConstraint(
            "CHK_Closed_Has_LastMessageId",
            "\"CompletedAt\" IS NULL OR \"LastMessageId\" IS NOT NULL"));

        // Indexes
        builder.HasIndex(m => m.SessionId)
            .HasDatabaseName("IX_Monologs_SessionId");

        builder.HasIndex(m => m.ParentMonologId)
            .HasDatabaseName("IX_Monologs_ParentMonologId");

        builder.HasIndex(m => m.ParticipantId)
            .HasDatabaseName("IX_Monologs_ParticipantId");

        builder.HasIndex(m => m.ProviderId)
            .HasDatabaseName("IX_Monologs_ProviderId");

        builder.HasIndex(m => m.ModeId)
            .HasDatabaseName("IX_Monologs_ModeId");

        // Partial index for finding open monologs quickly
        builder.HasIndex(m => new { m.SessionId, m.Role })
            .HasDatabaseName("IX_Monologs_Open")
            .HasFilter("\"CompletedAt\" IS NULL");

        // HNSW index for vector search will be added via raw SQL in migration
        // because EF Core doesn't support HNSW index type directly
    }
}
