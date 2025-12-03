using Olbrasoft.Data.Cqrs;
using Olbrasoft.Mediation;

namespace Olbrasoft.OpenCode.Extensions.Data.Commands;

/// <summary>
/// Command to create a new session in the database.
/// </summary>
public class CreateSessionCommand : BaseCommand<int>
{
    /// <summary>
    /// The unique session identifier from OpenCode.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Optional title for the session.
    /// </summary>
    public string? Title { get; init; }

    /// <summary>
    /// Optional working directory where the session was started.
    /// </summary>
    public string? WorkingDirectory { get; init; }

    /// <summary>
    /// When the session was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="CreateSessionCommand"/>.
    /// </summary>
    public CreateSessionCommand() { }

    /// <summary>
    /// Initializes a new instance of <see cref="CreateSessionCommand"/> with mediator.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    public CreateSessionCommand(IMediator mediator) : base(mediator) { }
}
