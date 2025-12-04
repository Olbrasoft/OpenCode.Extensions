using Olbrasoft.Data.Cqrs;
using Olbrasoft.Mediation;

namespace Olbrasoft.OpenCode.Extensions.Data.Commands;

/// <summary>
/// Command to update/replace the content of an existing open monolog.
/// Used for streaming assistant responses where content is progressively updated.
/// </summary>
public class UpdateMonologContentCommand : BaseCommand<bool>
{
    /// <summary>
    /// The monolog database ID to update.
    /// </summary>
    public required int MonologId { get; init; }

    /// <summary>
    /// New content to replace the existing content.
    /// </summary>
    public required string Content { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="UpdateMonologContentCommand"/>.
    /// </summary>
    public UpdateMonologContentCommand() { }

    /// <summary>
    /// Initializes a new instance of <see cref="UpdateMonologContentCommand"/> with mediator.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    public UpdateMonologContentCommand(IMediator mediator) : base(mediator) { }
}
