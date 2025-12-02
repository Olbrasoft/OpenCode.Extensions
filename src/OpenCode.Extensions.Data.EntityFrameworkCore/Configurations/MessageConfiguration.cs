using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Olbrasoft.OpenCode.Extensions.Data.Entities;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.Configurations;

public class MessageConfiguration : IEntityTypeConfiguration<Message>
{
    public void Configure(EntityTypeBuilder<Message> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.SessionId)
            .HasMaxLength(100)
            .IsRequired();

        // Enum → int conversion
        builder.Property(m => m.Role)
            .HasConversion<int>();

        builder.Property(m => m.Mode)
            .HasConversion<int>();

        builder.HasOne(m => m.Participant)
            .WithMany(p => p.Messages)
            .HasForeignKey(m => m.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Provider)
            .WithMany(p => p.Messages)
            .HasForeignKey(m => m.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
