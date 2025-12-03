using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Olbrasoft.OpenCode.Extensions.Data.Entities;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.ToTable("Messages");

        // Primary key - auto-increment integer
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .ValueGeneratedOnAdd();

        // External message ID from OpenCode - unique index
        builder.Property(m => m.MessageId)
            .HasMaxLength(100)
            .IsRequired();
        builder.HasIndex(m => m.MessageId)
            .IsUnique();

        // Session FK
        builder.HasOne(m => m.Session)
            .WithMany(s => s.Messages)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Self-reference for threaded messages
        builder.HasOne(m => m.ParentMessage)
            .WithMany(m => m.ChildMessages)
            .HasForeignKey(m => m.ParentMessageId)
            .OnDelete(DeleteBehavior.Restrict);

        // Each session can have only ONE root message (message without parent)
        // This is enforced by a unique partial index on SessionId where ParentMessageId IS NULL
        builder.HasIndex(m => m.SessionId)
            .HasFilter("\"ParentMessageId\" IS NULL")
            .IsUnique()
            .HasDatabaseName("IX_Messages_SessionId_RootMessage");

        // Each parent message can have only ONE child message per session (linear conversation chain)
        // This prevents duplicate responses to the same message
        builder.HasIndex(m => new { m.SessionId, m.ParentMessageId })
            .HasFilter("\"ParentMessageId\" IS NOT NULL")
            .IsUnique()
            .HasDatabaseName("IX_Messages_SessionId_ParentMessageId_Unique");

        // Role - enum stored as int (no FK, just conversion)
        builder.Property(m => m.Role)
            .HasConversion<int>();

        // Mode FK (referential table)
        builder.HasOne(m => m.ModeEntity)
            .WithMany(mo => mo.Messages)
            .HasForeignKey(m => m.ModeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Provider FK
        builder.HasOne(m => m.Provider)
            .WithMany(p => p.Messages)
            .HasForeignKey(m => m.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Participant FK - who created/sent this message
        builder.HasOne(m => m.Participant)
            .WithMany(p => p.Messages)
            .HasForeignKey(m => m.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        // Content - required and cannot be empty
        builder.Property(m => m.Content)
            .IsRequired();
        builder.ToTable(t => t.HasCheckConstraint("CK_Messages_Content_NotEmpty", "length(\"Content\") > 0"));

        // Cost column configuration
        builder.Property(m => m.Cost)
            .HasPrecision(18, 8);
    }
}
