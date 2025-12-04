using Olbrasoft.Data.Cqrs;
using Olbrasoft.Mediation;
using Olbrasoft.OpenCode.Extensions.Data.Entities;

namespace Olbrasoft.OpenCode.Extensions.Data.Queries;

/// <summary>
/// Query for semantic vector search across monologs.
/// Returns monologs ordered by similarity to the query embedding.
/// </summary>
public class SearchMonologsQuery : BaseQuery<IReadOnlyList<MonologSearchResult>>
{
    /// <summary>
    /// The embedding vector to search for (1536 dimensions).
    /// </summary>
    public required float[] QueryEmbedding { get; init; }

    /// <summary>
    /// Optional session ID to limit search scope.
    /// </summary>
    public int? SessionId { get; init; }

    /// <summary>
    /// Maximum number of results to return.
    /// </summary>
    public int Limit { get; init; } = 10;

    /// <summary>
    /// Minimum similarity threshold (0.0 to 1.0).
    /// Results below this threshold are excluded.
    /// </summary>
    public double MinSimilarity { get; init; } = 0.5;

    /// <summary>
    /// Initializes a new instance of <see cref="SearchMonologsQuery"/>.
    /// </summary>
    public SearchMonologsQuery() { }

    /// <summary>
    /// Initializes a new instance of <see cref="SearchMonologsQuery"/> with mediator.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    public SearchMonologsQuery(IMediator mediator) : base(mediator) { }
}

/// <summary>
/// Result of a semantic search including the monolog and similarity score.
/// </summary>
public class MonologSearchResult
{
    /// <summary>
    /// The matched monolog.
    /// </summary>
    public required Monolog Monolog { get; init; }

    /// <summary>
    /// Cosine similarity score (0.0 to 1.0, higher is more similar).
    /// </summary>
    public required double Similarity { get; init; }
}
