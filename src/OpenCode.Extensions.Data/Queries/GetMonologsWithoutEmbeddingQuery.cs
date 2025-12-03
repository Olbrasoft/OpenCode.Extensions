using Olbrasoft.Data.Cqrs;
using Olbrasoft.Mediation;
using Olbrasoft.OpenCode.Extensions.Data.Entities;

namespace Olbrasoft.OpenCode.Extensions.Data.Queries;

/// <summary>
/// Query to get closed monologs that don't have embeddings yet.
/// </summary>
public class GetMonologsWithoutEmbeddingQuery : BaseQuery<IReadOnlyList<Monolog>>
{
    /// <summary>
    /// Maximum number of monologs to return.
    /// </summary>
    public int Limit { get; init; } = 100;

    /// <summary>
    /// Initializes a new instance of <see cref="GetMonologsWithoutEmbeddingQuery"/>.
    /// </summary>
    public GetMonologsWithoutEmbeddingQuery() { }

    /// <summary>
    /// Initializes a new instance of <see cref="GetMonologsWithoutEmbeddingQuery"/> with mediator.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    public GetMonologsWithoutEmbeddingQuery(IMediator mediator) : base(mediator) { }
}
