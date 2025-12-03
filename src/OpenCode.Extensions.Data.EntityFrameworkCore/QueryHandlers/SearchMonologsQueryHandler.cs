using Microsoft.EntityFrameworkCore;
using Olbrasoft.Data.Cqrs;
using Olbrasoft.OpenCode.Extensions.Data.Queries;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.QueryHandlers;

/// <summary>
/// Handler for SearchMonologsQuery.
/// Performs semantic vector search using pgvector's cosine similarity.
/// </summary>
public class SearchMonologsQueryHandler : IQueryHandler<SearchMonologsQuery, IReadOnlyList<MonologSearchResult>>
{
    private readonly OpenCodeDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="SearchMonologsQueryHandler"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public SearchMonologsQueryHandler(OpenCodeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<MonologSearchResult>> HandleAsync(SearchMonologsQuery query, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(query);
        token.ThrowIfCancellationRequested();

        var queryVector = new Vector(query.QueryEmbedding);

        var baseQuery = _context.Monologs
            .Where(m => m.Embedding != null && m.CompletedAt != null);

        // Filter by session if specified
        if (query.SessionId.HasValue)
        {
            baseQuery = baseQuery.Where(m => m.SessionId == query.SessionId.Value);
        }

        // Perform vector search with cosine distance
        // Cosine distance = 1 - cosine similarity, so similarity = 1 - distance
        var results = await baseQuery
            .Select(m => new
            {
                Monolog = m,
                Distance = m.Embedding!.CosineDistance(queryVector)
            })
            .Where(x => (1 - x.Distance) >= query.MinSimilarity)
            .OrderBy(x => x.Distance)
            .Take(query.Limit)
            .ToListAsync(token);

        return results
            .Select(r => new MonologSearchResult
            {
                Monolog = r.Monolog,
                Similarity = 1 - r.Distance
            })
            .ToList();
    }
}
