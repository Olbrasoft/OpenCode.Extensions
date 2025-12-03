using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Olbrasoft.OpenCode.Extensions.Data.Entities;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.Configurations;

public class ModelConfiguration : IEntityTypeConfiguration<Model>
{
    public void Configure(EntityTypeBuilder<Model> builder)
    {
        builder.ToTable("Models");

        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id)
            .ValueGeneratedOnAdd();

        builder.Property(m => m.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(m => m.Name)
            .IsUnique();

        builder.Property(m => m.DisplayName)
            .HasMaxLength(200);

        builder.HasOne(m => m.Provider)
            .WithMany(p => p.Models)
            .HasForeignKey(m => m.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed data for GitHub Copilot supported models (December 2025)
        // See: https://docs.github.com/en/copilot/reference/ai-models/supported-models
        builder.HasData(
            // Anthropic models (ProviderId = 4)
            new Model { Id = 1, Name = "claude-haiku-4.5", DisplayName = "Claude Haiku 4.5", ProviderId = 4 },
            new Model { Id = 2, Name = "claude-opus-4.1", DisplayName = "Claude Opus 4.1", ProviderId = 4 },
            new Model { Id = 3, Name = "claude-opus-4.5", DisplayName = "Claude Opus 4.5", ProviderId = 4 },
            new Model { Id = 4, Name = "claude-sonnet-4", DisplayName = "Claude Sonnet 4", ProviderId = 4 },
            new Model { Id = 5, Name = "claude-sonnet-4.5", DisplayName = "Claude Sonnet 4.5", ProviderId = 4 },
            
            // OpenAI models (ProviderId = 5)
            new Model { Id = 6, Name = "gpt-4.1", DisplayName = "GPT-4.1", ProviderId = 5 },
            new Model { Id = 7, Name = "gpt-5", DisplayName = "GPT-5", ProviderId = 5 },
            new Model { Id = 8, Name = "gpt-5-mini", DisplayName = "GPT-5 mini", ProviderId = 5 },
            new Model { Id = 9, Name = "gpt-5-codex", DisplayName = "GPT-5-Codex", ProviderId = 5 },
            new Model { Id = 10, Name = "gpt-5.1", DisplayName = "GPT-5.1", ProviderId = 5 },
            new Model { Id = 11, Name = "gpt-5.1-codex", DisplayName = "GPT-5.1-Codex", ProviderId = 5 },
            new Model { Id = 12, Name = "gpt-5.1-codex-mini", DisplayName = "GPT-5.1-Codex-Mini", ProviderId = 5 },
            
            // Google models (ProviderId = 7)
            new Model { Id = 13, Name = "gemini-2.5-pro", DisplayName = "Gemini 2.5 Pro", ProviderId = 7 },
            new Model { Id = 14, Name = "gemini-3-pro", DisplayName = "Gemini 3 Pro", ProviderId = 7 },
            
            // xAI models (ProviderId = 9)
            new Model { Id = 15, Name = "grok-code-fast-1", DisplayName = "Grok Code Fast 1", ProviderId = 9 },
            
            // OpenAI fine-tuned (ProviderId = 5) - Raptor mini is fine-tuned GPT-5 mini
            new Model { Id = 16, Name = "raptor-mini", DisplayName = "Raptor mini", ProviderId = 5 }
        );
    }
}
