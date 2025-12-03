using Olbrasoft.OpenCode.Extensions.Data.Commands;

namespace OpenCode.Extensions.Services;

/// <summary>
/// Service for managing OpenCode sessions.
/// </summary>
public interface ISessionService
{
    /// <summary>
    /// Creates a new session in the database.
    /// </summary>
    /// <param name="sessionId">The unique session identifier from OpenCode.</param>
    /// <param name="title">Optional title for the session.</param>
    /// <param name="workingDirectory">Optional working directory where the session was started.</param>
    /// <param name="createdAt">When the session was created.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The database Id of the created session.</returns>
    Task<int> CreateSessionAsync(
        string sessionId,
        string? title,
        string? workingDirectory,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets existing session Id or creates a new session if it doesn't exist.
    /// </summary>
    /// <param name="sessionId">The unique session identifier from OpenCode.</param>
    /// <param name="title">Optional title for the session (used only when creating).</param>
    /// <param name="workingDirectory">Optional working directory (used only when creating).</param>
    /// <param name="createdAt">When the session was created (used only when creating).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The database Id of the existing or newly created session.</returns>
    Task<int> GetOrCreateSessionAsync(
        string sessionId,
        string? title,
        string? workingDirectory,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the database Id of a session by its external SessionId.
    /// </summary>
    /// <param name="sessionId">The external session identifier from OpenCode.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The database Id if found, null otherwise.</returns>
    Task<int?> GetSessionIdAsync(string sessionId, CancellationToken cancellationToken = default);
}
