using Microsoft.EntityFrameworkCore;
using Olbrasoft.Data.Cqrs;
using Olbrasoft.OpenCode.Extensions.Data.Entities;
using Olbrasoft.OpenCode.Extensions.Data.Queries;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.QueryHandlers;

/// <summary>
/// Handler for GetOpenMonologQuery.
/// Returns an open monolog for a session and role, or null if not found.
/// </summary>
public class GetOpenMonologQueryHandler : IQueryHandler<GetOpenMonologQuery, Monolog?>
{
    private readonly OpenCodeDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="GetOpenMonologQueryHandler"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public GetOpenMonologQueryHandler(OpenCodeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<Monolog?> HandleAsync(GetOpenMonologQuery query, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(query);
        token.ThrowIfCancellationRequested();

        var monolog = await _context.Monologs
            .Where(m => m.SessionId == query.SessionId
                && m.Role == query.Role
                && m.CompletedAt == null)
            .OrderByDescending(m => m.StartedAt)
            .FirstOrDefaultAsync(token);

        return monolog;
    }
}
