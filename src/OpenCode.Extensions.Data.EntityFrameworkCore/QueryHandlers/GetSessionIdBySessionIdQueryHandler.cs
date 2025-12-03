using Microsoft.EntityFrameworkCore;
using Olbrasoft.Data.Cqrs;
using OpenCode.Extensions.Data.Queries;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.QueryHandlers;

/// <summary>
/// Handler for GetSessionIdBySessionIdQuery.
/// Returns the database Id of a session by its external SessionId, or null if not found.
/// </summary>
public class GetSessionIdBySessionIdQueryHandler : IQueryHandler<GetSessionIdBySessionIdQuery, int?>
{
    private readonly OpenCodeDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="GetSessionIdBySessionIdQueryHandler"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public GetSessionIdBySessionIdQueryHandler(OpenCodeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<int?> HandleAsync(GetSessionIdBySessionIdQuery query, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(query);
        token.ThrowIfCancellationRequested();

        var id = await _context.Sessions
            .Where(s => s.SessionId == query.SessionId)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(token);

        return id;
    }
}
