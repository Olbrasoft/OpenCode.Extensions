using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace Olbrasoft.OpenCode.Extensions.Data.Entities;

/// <summary>
/// Message represents a single unit of communication within a session.
/// </summary>
public class Message
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Session this message belongs to.
    /// </summary>
    public required string SessionId { get; set; }

    /// <summary>
    /// Navigation property to Session.
    /// </summary>
    public Session? Session { get; set; }

    /// <summary>
    /// Role in the conversation (User or Assistant).
    /// </summary>
    public Role Role { get; set; }

    /// <summary>
    /// Mode when this message was created (Build or Plan).
    /// </summary>
    public Mode Mode { get; set; }

    /// <summary>
    /// Participant who created/sent this message.
    /// </summary>
    public Guid ParticipantId { get; set; }

    /// <summary>
    /// Navigation property to Participant.
    /// </summary>
    public Participant? Participant { get; set; }

    /// <summary>
    /// Provider through which the message was received (Keyboard, Voice, API...).
    /// </summary>
    public int ProviderId { get; set; }

    /// <summary>
    /// Navigation property to Provider.
    /// </summary>
    public Provider? Provider { get; set; }

    /// <summary>
    /// Message content as text.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// Raw message parts as JSON (for complex content: text, images, tool calls...).
    /// </summary>
    public string? PartsJson { get; set; }

    /// <summary>
    /// When the message was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// Token count for the message (if available from AI response).
    /// </summary>
    public int? TokenCount { get; set; }
}
