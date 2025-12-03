namespace Olbrasoft.OpenCode.Extensions.Data.Entities;

/// <summary>
/// Participant in the conversation - who created or sent the message.
/// Can be human, AI model, script, or system.
/// </summary>
public class Participant
{
    /// <summary>
    /// Primary key (auto-increment integer).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Human-readable label (e.g., "Jirka", "Claude Sonnet 4", "Deploy Bot").
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Technical identifier (e.g., "user-jirka", "claude-sonnet-4", "ci-github-actions").
    /// </summary>
    public required string Identifier { get; set; }

    /// <summary>
    /// Type of participant (foreign key to ParticipantTypes table).
    /// </summary>
    public int ParticipantTypeId { get; set; }

    /// <summary>
    /// Navigation property to ParticipantTypeEntity.
    /// </summary>
    public ParticipantTypeEntity? ParticipantType { get; set; }

    /// <summary>
    /// Messages created by this participant.
    /// </summary>
    public ICollection<Message> Messages { get; set; } = [];
}
