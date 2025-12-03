using Olbrasoft.Data.Cqrs;
using Olbrasoft.Mediation;

namespace OpenCode.Extensions.Data.Queries;

/// <summary>
/// Query to get the database Id of a session by its external SessionId.
/// Returns null if the session does not exist.
/// </summary>
public class GetSessionIdBySessionIdQuery : BaseQuery<int?>
{
    /// <summary>
    /// The external session identifier from OpenCode.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="GetSessionIdBySessionIdQuery"/>.
    /// </summary>
    public GetSessionIdBySessionIdQuery() { }

    /// <summary>
    /// Initializes a new instance of <see cref="GetSessionIdBySessionIdQuery"/> with mediator.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    public GetSessionIdBySessionIdQuery(IMediator mediator) : base(mediator) { }
}
