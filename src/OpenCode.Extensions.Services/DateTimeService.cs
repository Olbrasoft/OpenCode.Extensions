using OpenCode.Extensions.Data.Queries;

namespace OpenCode.Extensions.Services;

/// <summary>
/// Service implementation for retrieving date and time information from the database.
/// </summary>
public class DateTimeService : Service, IDateTimeService
{
    /// <summary>
    /// Initializes a new instance of <see cref="DateTimeService"/>.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    public DateTimeService(IMediator mediator) : base(mediator)
    {
    }

    /// <inheritdoc />
    public async Task<DateTime> GetCurrentDateTimeAsync(CancellationToken cancellationToken = default)
    {
        var query = new GetCurrentDateTimeQuery();
        return await Mediator.MediateAsync(query, cancellationToken);
    }
}
