using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Olbrasoft.OpenCode.Extensions.Data.Entities;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.Configurations;

public class SessionConfiguration : IEntityTypeConfiguration<Session>
{
    public void Configure(EntityTypeBuilder<Session> builder)
    {
        // Primary key - auto-increment integer
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id)
            .ValueGeneratedOnAdd();

        // External identifier from OpenCode - unique index
        builder.Property(s => s.SessionId)
            .HasMaxLength(100)
            .IsRequired();
        builder.HasIndex(s => s.SessionId)
            .IsUnique();

        builder.Property(s => s.Title)
            .HasMaxLength(500);

        builder.Property(s => s.WorkingDirectory)
            .HasMaxLength(1000);

        builder.HasMany(s => s.Messages)
            .WithOne(m => m.Session)
            .HasForeignKey(m => m.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
