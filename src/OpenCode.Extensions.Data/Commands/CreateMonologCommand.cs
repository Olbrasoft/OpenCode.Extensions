using Olbrasoft.Data.Cqrs;
using Olbrasoft.Mediation;
using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace Olbrasoft.OpenCode.Extensions.Data.Commands;

/// <summary>
/// Command to create a new monolog in the database.
/// </summary>
public class CreateMonologCommand : BaseCommand<int>
{
    /// <summary>
    /// Session database ID this monolog belongs to.
    /// </summary>
    public required int SessionId { get; init; }

    /// <summary>
    /// Parent monolog ID for conversation threading.
    /// NULL for user monologs that start a new conversation.
    /// REQUIRED for assistant monologs.
    /// </summary>
    public int? ParentMonologId { get; init; }

    /// <summary>
    /// Role in conversation (User or Assistant).
    /// </summary>
    public required Role Role { get; init; }

    /// <summary>
    /// First OpenCode message ID that started this monolog.
    /// </summary>
    public required string FirstMessageId { get; init; }

    /// <summary>
    /// Initial content of the monolog.
    /// </summary>
    public string Content { get; init; } = string.Empty;

    /// <summary>
    /// Participant ID - who created this monolog.
    /// </summary>
    public required int ParticipantId { get; init; }

    /// <summary>
    /// Provider ID - where the message came from.
    /// </summary>
    public required int ProviderId { get; init; }

    /// <summary>
    /// Mode ID - Build or Plan.
    /// </summary>
    public required int ModeId { get; init; }

    /// <summary>
    /// When the monolog started.
    /// </summary>
    public required DateTimeOffset StartedAt { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="CreateMonologCommand"/>.
    /// </summary>
    public CreateMonologCommand() { }

    /// <summary>
    /// Initializes a new instance of <see cref="CreateMonologCommand"/> with mediator.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    public CreateMonologCommand(IMediator mediator) : base(mediator) { }
}
