using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Olbrasoft.OpenCode.Extensions.Data.Entities;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
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

        // Role FK (referential table)
        builder.HasOne(m => m.RoleEntity)
            .WithMany(r => r.Messages)
            .HasForeignKey(m => m.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

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

        // Model FK (nullable for user messages)
        builder.HasOne(m => m.Model)
            .WithMany(mo => mo.Messages)
            .HasForeignKey(m => m.ModelId)
            .OnDelete(DeleteBehavior.Restrict);

        // Cost column configuration
        builder.Property(m => m.Cost)
            .HasPrecision(18, 8);
    }
}
