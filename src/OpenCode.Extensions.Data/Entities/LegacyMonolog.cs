using Pgvector;

namespace Olbrasoft.OpenCode.Extensions.Data.Entities;

/// <summary>
/// LegacyMonolog is a "trash bin" for monologs that failed constraint validation.
/// Same structure as Monolog but with STRING fields instead of FK references.
/// No constraints - accepts any data for debugging purposes.
/// </summary>
public class LegacyMonolog
{
    /// <summary>
    /// Internal database identifier (auto-increment).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Session identifier (STRING - not FK validated).
    /// </summary>
    public string? SessionId { get; set; }

    /// <summary>
    /// Parent monolog identifier (STRING - not FK validated).
    /// </summary>
    public string? ParentMonologId { get; set; }

    /// <summary>
    /// Role in conversation (1 = User, 2 = Assistant).
    /// </summary>
    public int? Role { get; set; }

    /// <summary>
    /// Monolog content.
    /// </summary>
    public string? Content { get; set; }

    /// <summary>
    /// First OpenCode message ID.
    /// </summary>
    public string? FirstMessageId { get; set; }

    /// <summary>
    /// Last OpenCode message ID.
    /// </summary>
    public string? LastMessageId { get; set; }

    /// <summary>
    /// Vector embedding for semantic search (1536 dimensions).
    /// </summary>
    public Vector? Embedding { get; set; }

    /// <summary>
    /// Participant identifier (STRING - not FK validated).
    /// </summary>
    public string? ParticipantIdentifier { get; set; }

    /// <summary>
    /// Provider name (STRING - not FK validated).
    /// </summary>
    public string? ProviderName { get; set; }

    /// <summary>
    /// Mode name (STRING - not FK validated).
    /// </summary>
    public string? ModeName { get; set; }

    /// <summary>
    /// Number of input tokens.
    /// </summary>
    public int? TokensInput { get; set; }

    /// <summary>
    /// Number of output tokens.
    /// </summary>
    public int? TokensOutput { get; set; }

    /// <summary>
    /// Cost in USD.
    /// </summary>
    public decimal? Cost { get; set; }

    /// <summary>
    /// When the monolog started.
    /// </summary>
    public DateTimeOffset? StartedAt { get; set; }

    /// <summary>
    /// When the monolog was completed.
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// Whether the monolog was aborted.
    /// </summary>
    public bool? IsAborted { get; set; }

    /// <summary>
    /// Why this monolog was rejected (constraint violation, missing parent, etc.).
    /// </summary>
    public string? RejectionReason { get; set; }

    /// <summary>
    /// Original JSON payload for debugging.
    /// </summary>
    public string? OriginalPayload { get; set; }

    /// <summary>
    /// When the record was created in database.
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }
}
