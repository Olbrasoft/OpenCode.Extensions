using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Olbrasoft.OpenCode.Extensions.Data.Entities;
using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.Configurations;

public class ParticipantTypeEntityConfiguration : IEntityTypeConfiguration<ParticipantTypeEntity>
{
    public void Configure(EntityTypeBuilder<ParticipantTypeEntity> builder)
    {
        builder.ToTable("ParticipantTypes");

        builder.HasKey(pt => pt.Id);

        builder.Property(pt => pt.Name)
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(pt => pt.Name)
            .IsUnique();

        // Seed data matching ParticipantType enum
        builder.HasData(
            new ParticipantTypeEntity { Id = (int)ParticipantType.Human, Name = nameof(ParticipantType.Human) },
            new ParticipantTypeEntity { Id = (int)ParticipantType.AiModel, Name = nameof(ParticipantType.AiModel) },
            new ParticipantTypeEntity { Id = (int)ParticipantType.Script, Name = nameof(ParticipantType.Script) },
            new ParticipantTypeEntity { Id = (int)ParticipantType.System, Name = nameof(ParticipantType.System) }
        );
    }
}
