using Olbrasoft.Data.Cqrs;
using Olbrasoft.Mediation;

namespace Olbrasoft.OpenCode.Extensions.Data.Commands;

/// <summary>
/// Command to update embedding vector for a closed monolog.
/// </summary>
public class UpdateMonologEmbeddingCommand : BaseCommand<bool>
{
    /// <summary>
    /// The monolog database ID to update.
    /// </summary>
    public required int MonologId { get; init; }

    /// <summary>
    /// The embedding vector (1536 dimensions).
    /// </summary>
    public required float[] Embedding { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="UpdateMonologEmbeddingCommand"/>.
    /// </summary>
    public UpdateMonologEmbeddingCommand() { }

    /// <summary>
    /// Initializes a new instance of <see cref="UpdateMonologEmbeddingCommand"/> with mediator.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    public UpdateMonologEmbeddingCommand(IMediator mediator) : base(mediator) { }
}
