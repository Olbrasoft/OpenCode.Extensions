namespace Olbrasoft.OpenCode.Extensions.Data.Entities;

/// <summary>
/// Session represents a conversation - a collection of messages between participants.
/// </summary>
public class Session
{
    /// <summary>
    /// Unique identifier (from OpenCode).
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Session title or summary.
    /// </summary>
    public string? Title { get; set; }

    /// <summary>
    /// Working directory path where the session was started.
    /// </summary>
    public string? WorkingDirectory { get; set; }

    /// <summary>
    /// When the session was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// When the session was last updated.
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// Messages in this session.
    /// </summary>
    public ICollection<Message> Messages { get; set; } = [];
}
