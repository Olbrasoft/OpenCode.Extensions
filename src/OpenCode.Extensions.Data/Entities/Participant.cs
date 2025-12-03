using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace Olbrasoft.OpenCode.Extensions.Data.Entities;

/// <summary>
/// Participant in the conversation - who created or sent the message.
/// Can be human, AI model, script, or system.
/// </summary>
public class Participant
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Human-readable label (e.g., "Jirka", "Claude Sonnet 4", "Deploy Bot").
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Technical identifier (e.g., "user-jirka", "claude-sonnet-4-20250514", "ci-github-actions").
    /// </summary>
    public required string Identifier { get; set; }

    /// <summary>
    /// Type of participant (Human, AiModel, Script, System).
    /// </summary>
    public ParticipantType Type { get; set; }

    /// <summary>
    /// Messages created by this participant.
    /// </summary>
    public ICollection<Message> Messages { get; set; } = [];

    /// <summary>
    /// Monologs created by this participant.
    /// </summary>
    public ICollection<Monolog> Monologs { get; set; } = [];
}
