using Olbrasoft.Mediation;
using Olbrasoft.OpenCode.Extensions.Data.Commands;
using Olbrasoft.OpenCode.Extensions.Data.Entities;
using Olbrasoft.OpenCode.Extensions.Data.Enums;
using Olbrasoft.OpenCode.Extensions.Data.Queries;

namespace OpenCode.Extensions.Services;

/// <summary>
/// Service for managing Monologs.
/// </summary>
public class MonologService : Service, IMonologService
{
    /// <summary>
    /// Initializes a new instance of <see cref="MonologService"/>.
    /// </summary>
    /// <param name="mediator">The mediator instance for dispatching commands.</param>
    public MonologService(IMediator mediator) : base(mediator)
    {
    }

    /// <inheritdoc />
    public async Task<int> CreateMonologAsync(
        int sessionId,
        int? parentMonologId,
        Role role,
        string firstMessageId,
        string content,
        Guid participantId,
        int providerId,
        int modeId,
        DateTimeOffset startedAt,
        CancellationToken cancellationToken = default)
    {
        var command = new CreateMonologCommand
        {
            SessionId = sessionId,
            ParentMonologId = parentMonologId,
            Role = role,
            FirstMessageId = firstMessageId,
            Content = content,
            ParticipantId = participantId,
            ProviderId = providerId,
            ModeId = modeId,
            StartedAt = startedAt
        };

        return await Mediator.MediateAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> AppendContentAsync(int monologId, string content, CancellationToken cancellationToken = default)
    {
        var command = new AppendMonologContentCommand
        {
            MonologId = monologId,
            Content = content
        };

        return await Mediator.MediateAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> UpdateContentAsync(int monologId, string content, CancellationToken cancellationToken = default)
    {
        var command = new UpdateMonologContentCommand
        {
            MonologId = monologId,
            Content = content
        };

        return await Mediator.MediateAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<bool> CloseMonologAsync(
        int monologId,
        string lastMessageId,
        string? finalContent,
        DateTimeOffset completedAt,
        bool isAborted,
        int? tokensInput,
        int? tokensOutput,
        decimal? cost,
        CancellationToken cancellationToken = default)
    {
        var command = new CloseMonologCommand
        {
            MonologId = monologId,
            LastMessageId = lastMessageId,
            FinalContent = finalContent,
            CompletedAt = completedAt,
            IsAborted = isAborted,
            TokensInput = tokensInput,
            TokensOutput = tokensOutput,
            Cost = cost
        };

        return await Mediator.MediateAsync(command, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<Monolog?> GetOpenMonologAsync(int sessionId, Role role, CancellationToken cancellationToken = default)
    {
        var query = new GetOpenMonologQuery
        {
            SessionId = sessionId,
            Role = role
        };

        return await Mediator.MediateAsync(query, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MonologSearchResult>> SearchMonologsAsync(
        float[] queryEmbedding,
        int? sessionId,
        int limit,
        double minSimilarity,
        CancellationToken cancellationToken = default)
    {
        var query = new SearchMonologsQuery
        {
            QueryEmbedding = queryEmbedding,
            SessionId = sessionId,
            Limit = limit,
            MinSimilarity = minSimilarity
        };

        return await Mediator.MediateAsync(query, cancellationToken);
    }
}
