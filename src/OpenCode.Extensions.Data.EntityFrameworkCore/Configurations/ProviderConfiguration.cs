using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Olbrasoft.OpenCode.Extensions.Data.Entities;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.Configurations;

public class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(500);

        builder.HasIndex(p => p.Name)
            .IsUnique();

        // Seed data for Providers
        builder.HasData(
            new Provider { Id = 1, Name = "Keyboard", Description = "Direct typing to terminal" },
            new Provider { Id = 2, Name = "VoiceAssistant", Description = "ContinuousListener - voice input" },
            new Provider { Id = 3, Name = "HumanCombination", Description = "Combination of keyboard and voice" },
            new Provider { Id = 4, Name = "Anthropic", Description = "Claude models via Anthropic API" },
            new Provider { Id = 5, Name = "OpenAI", Description = "GPT models via OpenAI API" },
            new Provider { Id = 6, Name = "GitHubCopilot", Description = "GitHub Copilot (routes to various models)" },
            new Provider { Id = 7, Name = "Google", Description = "Gemini models via Google AI" },
            new Provider { Id = 8, Name = "AzureOpenAI", Description = "Azure OpenAI Service" },
            new Provider { Id = 9, Name = "xAI", Description = "Grok models via xAI" }
        );
    }
}
