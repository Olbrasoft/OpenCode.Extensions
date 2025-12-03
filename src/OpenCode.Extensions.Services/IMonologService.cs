using Olbrasoft.OpenCode.Extensions.Data.Entities;
using Olbrasoft.OpenCode.Extensions.Data.Enums;
using Olbrasoft.OpenCode.Extensions.Data.Queries;

namespace OpenCode.Extensions.Services;

/// <summary>
/// Service interface for managing Monologs.
/// </summary>
public interface IMonologService
{
    /// <summary>
    /// Creates a new monolog.
    /// </summary>
    Task<int> CreateMonologAsync(
        int sessionId,
        int? parentMonologId,
        Role role,
        string firstMessageId,
        string content,
        Guid participantId,
        int providerId,
        int modeId,
        DateTimeOffset startedAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Appends content to an existing open monolog.
    /// </summary>
    Task<bool> AppendContentAsync(int monologId, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates/replaces content of an existing open monolog.
    /// </summary>
    Task<bool> UpdateContentAsync(int monologId, string content, CancellationToken cancellationToken = default);

    /// <summary>
    /// Closes an open monolog.
    /// </summary>
    Task<bool> CloseMonologAsync(
        int monologId,
        string lastMessageId,
        string? finalContent,
        DateTimeOffset completedAt,
        bool isAborted,
        int? tokensInput,
        int? tokensOutput,
        decimal? cost,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an open monolog for a session and role.
    /// </summary>
    Task<Monolog?> GetOpenMonologAsync(int sessionId, Role role, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches monologs using semantic vector search.
    /// </summary>
    Task<IReadOnlyList<MonologSearchResult>> SearchMonologsAsync(
        float[] queryEmbedding,
        int? sessionId,
        int limit,
        double minSimilarity,
        CancellationToken cancellationToken = default);
}
