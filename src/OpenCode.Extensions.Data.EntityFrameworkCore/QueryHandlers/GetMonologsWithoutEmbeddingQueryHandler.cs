using Microsoft.EntityFrameworkCore;
using Olbrasoft.Data.Cqrs;
using Olbrasoft.OpenCode.Extensions.Data.Entities;
using Olbrasoft.OpenCode.Extensions.Data.Queries;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.QueryHandlers;

/// <summary>
/// Handler for GetMonologsWithoutEmbeddingQuery.
/// Returns closed monologs that don't have embeddings yet.
/// </summary>
public class GetMonologsWithoutEmbeddingQueryHandler : IQueryHandler<GetMonologsWithoutEmbeddingQuery, IReadOnlyList<Monolog>>
{
    private readonly OpenCodeDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="GetMonologsWithoutEmbeddingQueryHandler"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public GetMonologsWithoutEmbeddingQueryHandler(OpenCodeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<Monolog>> HandleAsync(GetMonologsWithoutEmbeddingQuery query, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(query);
        token.ThrowIfCancellationRequested();

        var monologs = await _context.Monologs
            .Where(m => m.CompletedAt != null && m.Embedding == null && !string.IsNullOrEmpty(m.Content))
            .OrderBy(m => m.CompletedAt)
            .Take(query.Limit)
            .ToListAsync(token);

        return monologs;
    }
}
