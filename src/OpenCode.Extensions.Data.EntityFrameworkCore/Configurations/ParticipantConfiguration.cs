using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Olbrasoft.OpenCode.Extensions.Data.Entities;
using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.Configurations;

public class ParticipantConfiguration : IEntityTypeConfiguration<Participant>
{
    public void Configure(EntityTypeBuilder<Participant> builder)
    {
        builder.ToTable("Participants");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id)
            .ValueGeneratedOnAdd();

        builder.Property(p => p.Label)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Identifier)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(p => p.Identifier)
            .IsUnique();

        // FK to ParticipantTypes table
        builder.HasOne(p => p.ParticipantType)
            .WithMany(pt => pt.Participants)
            .HasForeignKey(p => p.ParticipantTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed data for participants
        // Human participant
        builder.HasData(
            new Participant { Id = 1, Label = "Jirka", Identifier = "user-jirka", ParticipantTypeId = (int)ParticipantType.Human }
        );

        // AI Model participants (GitHub Copilot supported models - December 2025)
        // Anthropic models
        builder.HasData(
            new Participant { Id = 2, Label = "Claude Haiku 4.5", Identifier = "claude-haiku-4.5", ParticipantTypeId = (int)ParticipantType.AiModel },
            new Participant { Id = 3, Label = "Claude Opus 4.1", Identifier = "claude-opus-4.1", ParticipantTypeId = (int)ParticipantType.AiModel },
            new Participant { Id = 4, Label = "Claude Opus 4.5", Identifier = "claude-opus-4.5", ParticipantTypeId = (int)ParticipantType.AiModel },
            new Participant { Id = 5, Label = "Claude Sonnet 4", Identifier = "claude-sonnet-4", ParticipantTypeId = (int)ParticipantType.AiModel },
            new Participant { Id = 6, Label = "Claude Sonnet 4.5", Identifier = "claude-sonnet-4.5", ParticipantTypeId = (int)ParticipantType.AiModel }
        );

        // OpenAI models
        builder.HasData(
            new Participant { Id = 7, Label = "GPT-4.1", Identifier = "gpt-4.1", ParticipantTypeId = (int)ParticipantType.AiModel },
            new Participant { Id = 8, Label = "GPT-5", Identifier = "gpt-5", ParticipantTypeId = (int)ParticipantType.AiModel },
            new Participant { Id = 9, Label = "GPT-5 mini", Identifier = "gpt-5-mini", ParticipantTypeId = (int)ParticipantType.AiModel },
            new Participant { Id = 10, Label = "GPT-5-Codex", Identifier = "gpt-5-codex", ParticipantTypeId = (int)ParticipantType.AiModel },
            new Participant { Id = 11, Label = "GPT-5.1", Identifier = "gpt-5.1", ParticipantTypeId = (int)ParticipantType.AiModel },
            new Participant { Id = 12, Label = "GPT-5.1-Codex", Identifier = "gpt-5.1-codex", ParticipantTypeId = (int)ParticipantType.AiModel },
            new Participant { Id = 13, Label = "GPT-5.1-Codex-Mini", Identifier = "gpt-5.1-codex-mini", ParticipantTypeId = (int)ParticipantType.AiModel },
            new Participant { Id = 14, Label = "Raptor mini", Identifier = "raptor-mini", ParticipantTypeId = (int)ParticipantType.AiModel }
        );

        // Google models
        builder.HasData(
            new Participant { Id = 15, Label = "Gemini 2.5 Pro", Identifier = "gemini-2.5-pro", ParticipantTypeId = (int)ParticipantType.AiModel },
            new Participant { Id = 16, Label = "Gemini 3 Pro", Identifier = "gemini-3-pro", ParticipantTypeId = (int)ParticipantType.AiModel }
        );

        // xAI models
        builder.HasData(
            new Participant { Id = 17, Label = "Grok Code Fast 1", Identifier = "grok-code-fast-1", ParticipantTypeId = (int)ParticipantType.AiModel }
        );
    }
}
