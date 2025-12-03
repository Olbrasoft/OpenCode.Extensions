using Olbrasoft.Data.Cqrs;
using Olbrasoft.Mediation;

namespace Olbrasoft.OpenCode.Extensions.Data.Commands;

/// <summary>
/// Command to close an open monolog.
/// Sets CompletedAt, LastMessageId, and optionally token/cost information.
/// </summary>
public class CloseMonologCommand : BaseCommand<bool>
{
    /// <summary>
    /// The monolog database ID to close.
    /// </summary>
    public required int MonologId { get; init; }

    /// <summary>
    /// Last OpenCode message ID when monolog was closed.
    /// </summary>
    public required string LastMessageId { get; init; }

    /// <summary>
    /// Final content of the monolog (optional, for final update).
    /// </summary>
    public string? FinalContent { get; init; }

    /// <summary>
    /// When the monolog was completed.
    /// </summary>
    public required DateTimeOffset CompletedAt { get; init; }

    /// <summary>
    /// Whether the monolog was aborted by user (Escape key).
    /// </summary>
    public bool IsAborted { get; init; }

    /// <summary>
    /// Number of input tokens (for AI responses).
    /// </summary>
    public int? TokensInput { get; init; }

    /// <summary>
    /// Number of output tokens (for AI responses).
    /// </summary>
    public int? TokensOutput { get; init; }

    /// <summary>
    /// Cost of the monolog in USD (for AI responses).
    /// </summary>
    public decimal? Cost { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="CloseMonologCommand"/>.
    /// </summary>
    public CloseMonologCommand() { }

    /// <summary>
    /// Initializes a new instance of <see cref="CloseMonologCommand"/> with mediator.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    public CloseMonologCommand(IMediator mediator) : base(mediator) { }
}
