using Olbrasoft.Mediation;
using Olbrasoft.OpenCode.Extensions.Data.Commands;
using OpenCode.Extensions.Data.Queries;

namespace OpenCode.Extensions.Services;

/// <summary>
/// Service for managing OpenCode sessions.
/// </summary>
public class SessionService : Service, ISessionService
{
    /// <summary>
    /// Initializes a new instance of <see cref="SessionService"/>.
    /// </summary>
    /// <param name="mediator">The mediator instance for dispatching commands.</param>
    public SessionService(IMediator mediator) : base(mediator)
    {
    }

    /// <inheritdoc />
    public async Task<int> CreateSessionAsync(
        string sessionId,
        string? title,
        string? workingDirectory,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        var command = new CreateSessionCommand
        {
            SessionId = sessionId,
            Title = title,
            WorkingDirectory = workingDirectory,
            CreatedAt = createdAt
        };

        return await Mediator.MediateAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int?> GetSessionIdAsync(string sessionId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        var query = new GetSessionIdBySessionIdQuery
        {
            SessionId = sessionId
        };

        return await Mediator.MediateAsync(query, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<int> GetOrCreateSessionAsync(
        string sessionId,
        string? title,
        string? workingDirectory,
        DateTimeOffset createdAt,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(sessionId);

        // Check if session already exists
        var existingId = await GetSessionIdAsync(sessionId, cancellationToken);
        if (existingId.HasValue)
        {
            return existingId.Value;
        }

        // Create new session
        return await CreateSessionAsync(sessionId, title, workingDirectory, createdAt, cancellationToken);
    }
}
