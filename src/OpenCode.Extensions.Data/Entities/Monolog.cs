using Olbrasoft.OpenCode.Extensions.Data.Enums;
using Pgvector;

namespace Olbrasoft.OpenCode.Extensions.Data.Entities;

/// <summary>
/// Monolog represents a continuous speech by one participant until the other speaks.
/// This is the primary unit of conversation storage with vector search capability.
/// </summary>
public class Monolog
{
    /// <summary>
    /// Internal database identifier (auto-increment).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Session this monolog belongs to (foreign key).
    /// </summary>
    public int SessionId { get; set; }

    /// <summary>
    /// Navigation property to Session.
    /// </summary>
    public Session? Session { get; set; }

    /// <summary>
    /// Parent monolog ID for conversation threading.
    /// User monolog can be NULL (starts conversation).
    /// Assistant monolog MUST have a parent (always responds to user).
    /// </summary>
    public int? ParentMonologId { get; set; }

    /// <summary>
    /// Navigation property to parent monolog.
    /// </summary>
    public Monolog? ParentMonolog { get; set; }

    /// <summary>
    /// Child monologs (responses to this monolog).
    /// </summary>
    public ICollection<Monolog> ChildMonologs { get; set; } = [];

    /// <summary>
    /// Role in conversation (User = 1, Assistant = 2).
    /// </summary>
    public Role Role { get; set; }

    /// <summary>
    /// First OpenCode message ID that started this monolog.
    /// REQUIRED - monolog always starts from an OpenCode event.
    /// </summary>
    public required string FirstMessageId { get; set; }

    /// <summary>
    /// Last OpenCode message ID when monolog was closed.
    /// NULL while monolog is still open.
    /// REQUIRED once CompletedAt is set.
    /// </summary>
    public string? LastMessageId { get; set; }

    /// <summary>
    /// Monolog content (text).
    /// For user monologs, multiple no-reply messages are joined with "\n\n".
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Vector embedding for semantic search (1536 dimensions for text-embedding-3-small).
    /// Generated asynchronously when monolog is closed.
    /// </summary>
    public Vector? Embedding { get; set; }

    /// <summary>
    /// Participant ID - who created this monolog (foreign key).
    /// REQUIRED - we always know WHO is communicating.
    /// </summary>
    public Guid ParticipantId { get; set; }

    /// <summary>
    /// Navigation property to Participant.
    /// </summary>
    public Participant? Participant { get; set; }

    /// <summary>
    /// Provider ID - where the message came from (foreign key).
    /// REQUIRED - we always know WHERE the message came from.
    /// </summary>
    public int ProviderId { get; set; }

    /// <summary>
    /// Navigation property to Provider.
    /// </summary>
    public Provider? Provider { get; set; }

    /// <summary>
    /// Mode ID - Build or Plan (foreign key).
    /// REQUIRED - we always know WHAT mode we're in.
    /// </summary>
    public int ModeId { get; set; }

    /// <summary>
    /// Navigation property to ModeEntity.
    /// </summary>
    public ModeEntity? ModeEntity { get; set; }

    /// <summary>
    /// Number of input tokens (for AI responses).
    /// Nullable - may not be available, especially for user messages.
    /// </summary>
    public int? TokensInput { get; set; }

    /// <summary>
    /// Number of output tokens (for AI responses).
    /// Nullable - may not be available, especially for user messages.
    /// </summary>
    public int? TokensOutput { get; set; }

    /// <summary>
    /// Cost of the monolog in USD (for AI responses).
    /// Nullable - may not be available.
    /// </summary>
    public decimal? Cost { get; set; }

    /// <summary>
    /// When the monolog started.
    /// </summary>
    public DateTimeOffset StartedAt { get; set; }

    /// <summary>
    /// When the monolog was completed/closed.
    /// NULL means monolog is still open.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Whether the monolog was aborted by user (Escape key).
    /// </summary>
    public bool IsAborted { get; set; }

    /// <summary>
    /// When the record was created in database.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// When the record was last updated in database.
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
