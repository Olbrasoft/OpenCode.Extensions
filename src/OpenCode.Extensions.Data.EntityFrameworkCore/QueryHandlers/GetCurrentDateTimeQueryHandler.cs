using Microsoft.EntityFrameworkCore;
using Olbrasoft.Data.Cqrs;
using OpenCode.Extensions.Data.Queries;

namespace OpenCode.Extensions.Data.EntityFrameworkCore.QueryHandlers;

/// <summary>
/// Handler for GetCurrentDateTimeQuery that executes SELECT NOW() on PostgreSQL.
/// This is a POC handler to verify the CQRS pipeline works end-to-end.
/// </summary>
public class GetCurrentDateTimeQueryHandler : IQueryHandler<GetCurrentDateTimeQuery, DateTime>
{
    private readonly DbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="GetCurrentDateTimeQueryHandler"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public GetCurrentDateTimeQueryHandler(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<DateTime> HandleAsync(GetCurrentDateTimeQuery request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        token.ThrowIfCancellationRequested();

        // Execute raw SQL query to get current timestamp from PostgreSQL
        var result = await _context.Database
            .SqlQueryRaw<DateTime>("SELECT NOW()")
            .FirstAsync(token);

        return result;
    }
}
