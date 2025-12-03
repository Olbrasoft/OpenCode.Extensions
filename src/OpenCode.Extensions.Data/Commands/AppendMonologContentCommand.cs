using Olbrasoft.Data.Cqrs;
using Olbrasoft.Mediation;

namespace Olbrasoft.OpenCode.Extensions.Data.Commands;

/// <summary>
/// Command to append content to an existing open monolog.
/// Used when user sends multiple no-reply messages.
/// </summary>
public class AppendMonologContentCommand : BaseCommand<bool>
{
    /// <summary>
    /// The monolog database ID to append content to.
    /// </summary>
    public required int MonologId { get; init; }

    /// <summary>
    /// Content to append (will be joined with "\n\n").
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="AppendMonologContentCommand"/>.
    /// </summary>
    public AppendMonologContentCommand() { }

    /// <summary>
    /// Initializes a new instance of <see cref="AppendMonologContentCommand"/> with mediator.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    public AppendMonologContentCommand(IMediator mediator) : base(mediator) { }
}
