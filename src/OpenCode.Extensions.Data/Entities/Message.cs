using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace Olbrasoft.OpenCode.Extensions.Data.Entities;

/// <summary>
/// Message represents a single unit of communication within a session.
/// </summary>
public class Message
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
    /// Session this message belongs to (foreign key).
    /// </summary>
    public int SessionId { get; set; }

    /// <summary>
    /// Navigation property to Session.
    /// </summary>
    public Session? Session { get; set; }

    /// <summary>
    /// Parent message ID for threaded conversations (self-reference).
    /// </summary>
    public int? ParentMessageId { get; set; }

    /// <summary>
    /// Navigation property to parent message.
    /// </summary>
    public Message? ParentMessage { get; set; }

    /// <summary>
    /// Child messages (replies to this message).
    /// </summary>
    public ICollection<Message> ChildMessages { get; set; } = [];

    /// <summary>
    /// Role in conversation (User or Assistant).
    /// Stored as integer, mapped to Role enum.
    /// </summary>
    public Role Role { get; set; }

    /// <summary>
    /// Mode ID (foreign key to Modes table).
    /// </summary>
    public int ModeId { get; set; }

    /// <summary>
    /// Navigation property to ModeEntity.
    /// </summary>
    public ModeEntity? ModeEntity { get; set; }

    /// <summary>
    /// Provider ID - where the message came from (foreign key to Providers table).
    /// </summary>
    public int ProviderId { get; set; }

    /// <summary>
    /// Navigation property to Provider.
    /// </summary>
    public Provider? Provider { get; set; }

    /// <summary>
    /// Participant ID - who created/sent this message (foreign key to Participants table).
    /// </summary>
    public int ParticipantId { get; set; }

    /// <summary>
    /// Navigation property to Participant.
    /// </summary>
    public Participant? Participant { get; set; }

    /// <summary>
    /// Message content as text.
    /// </summary>
    public string? Content { get; set; }

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
    /// When the message was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}
