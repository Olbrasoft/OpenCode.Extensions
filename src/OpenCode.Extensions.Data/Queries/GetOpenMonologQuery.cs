using Olbrasoft.Data.Cqrs;
using Olbrasoft.Mediation;
using Olbrasoft.OpenCode.Extensions.Data.Entities;
using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace Olbrasoft.OpenCode.Extensions.Data.Queries;

/// <summary>
/// Query to find an open monolog in a session for a specific role.
/// Returns null if no open monolog exists.
/// </summary>
public class GetOpenMonologQuery : BaseQuery<Monolog?>
{
    /// <summary>
    /// Session database ID to search in.
    /// </summary>
    public required int SessionId { get; init; }

    /// <summary>
    /// Role to filter by (User or Assistant).
    /// </summary>
    public required Role Role { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="GetOpenMonologQuery"/>.
    /// </summary>
    public GetOpenMonologQuery() { }

    /// <summary>
    /// Initializes a new instance of <see cref="GetOpenMonologQuery"/> with mediator.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    public GetOpenMonologQuery(IMediator mediator) : base(mediator) { }
}
