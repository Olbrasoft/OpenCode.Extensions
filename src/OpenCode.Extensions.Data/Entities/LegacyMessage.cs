using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace Olbrasoft.OpenCode.Extensions.Data.Entities;

/// <summary>
/// Fallback table for messages from old sessions that don't meet strict constraints.
/// This entity is temporary - once old sessions are no longer used, it can be removed.
/// </summary>
public class LegacyMessage
{
    /// <summary>
    /// Internal database identifier (auto-increment).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// External message identifier from OpenCode.
    /// </summary>
    public required string MessageId { get; set; }

    /// <summary>
    /// External session identifier from OpenCode (stored as string, not FK).
    /// </summary>
    public required string SessionId { get; set; }

    /// <summary>
    /// Parent message ID from OpenCode (stored as string, not FK).
    /// </summary>
    public string? ParentMessageId { get; set; }

    /// <summary>
    /// Role in conversation (User or Assistant).
    /// </summary>
    public Role Role { get; set; }

    /// <summary>
    /// Message content as text.
    /// </summary>
    public required string Content { get; set; }

    /// <summary>
    /// Participant identifier from OpenCode.
    /// </summary>
    public string? ParticipantIdentifier { get; set; }

    /// <summary>
    /// Provider name from OpenCode.
    /// </summary>
    public string? ProviderName { get; set; }

    /// <summary>
    /// Mode value.
    /// </summary>
    public int? Mode { get; set; }

    /// <summary>
    /// Number of input tokens (for AI responses).
    /// </summary>
    public int? TokensInput { get; set; }

    /// <summary>
    /// Number of output tokens (for AI responses).
    /// </summary>
    public int? TokensOutput { get; set; }

    /// <summary>
    /// Cost of the message in USD (for AI responses).
    /// </summary>
    public decimal? Cost { get; set; }

    /// <summary>
    /// When the message was created in OpenCode.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// When the message was inserted into this fallback table.
    /// </summary>
    public DateTimeOffset InsertedAt { get; set; }

    /// <summary>
    /// Reason why this message ended up in fallback table.
    /// </summary>
    public string? FailureReason { get; set; }
}
