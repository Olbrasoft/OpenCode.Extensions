using Olbrasoft.Data.Cqrs;
using Olbrasoft.Mediation;

namespace OpenCode.Extensions.Data.Queries;

/// <summary>
/// Query to get the current date and time from the database.
/// This is a simple POC query to verify the CQRS pipeline works end-to-end.
/// </summary>
public class GetCurrentDateTimeQuery : BaseQuery<DateTime>
{
    /// <summary>
    /// Initializes a new instance of <see cref="GetCurrentDateTimeQuery"/>.
    /// </summary>
    public GetCurrentDateTimeQuery() { }

    /// <summary>
    /// Initializes a new instance of <see cref="GetCurrentDateTimeQuery"/> with mediator.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    public GetCurrentDateTimeQuery(IMediator mediator) : base(mediator) { }
}
