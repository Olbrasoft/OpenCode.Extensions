using Olbrasoft.Data.Cqrs;
using Olbrasoft.Mediation;
using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace Olbrasoft.OpenCode.Extensions.Data.Commands;

/// <summary>
/// Command to create a new message in the database.
/// </summary>
public class CreateMessageCommand : BaseCommand<int>
{
    /// <summary>
    /// The unique message identifier from OpenCode.
    /// </summary>
    public required string MessageId { get; init; }

    /// <summary>
    /// The external session identifier from OpenCode.
    /// </summary>
    public required string SessionId { get; init; }

    /// <summary>
    /// Role in conversation (User or Assistant).
    /// </summary>
    public Role Role { get; init; }

    /// <summary>
    /// Mode of operation (Build or Plan).
    /// </summary>
    public Mode Mode { get; init; }

    /// <summary>
    /// Identifier of the participant who created the message.
    /// For AI: model identifier (e.g., "claude-sonnet-4.5")
    /// For human: user identifier (e.g., "user-jirka")
    /// </summary>
    public required string ParticipantIdentifier { get; init; }

    /// <summary>
    /// Provider identifier (e.g., "HumanInput", "VoiceAssistant", "Anthropic").
    /// </summary>
    public required string ProviderName { get; init; }

    /// <summary>
    /// Message content as text.
    /// </summary>
    public string? Content { get; init; }

    /// <summary>
    /// Number of input tokens (for AI responses).
    /// </summary>
    public int? TokensInput { get; init; }

    /// <summary>
    /// Number of output tokens (for AI responses).
    /// </summary>
    public int? TokensOutput { get; init; }

    /// <summary>
    /// Cost of the message in USD (for AI responses).
    /// </summary>
    public decimal? Cost { get; init; }

    /// <summary>
    /// When the message was created.
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// Optional parent message ID for threaded conversations.
    /// </summary>
    public string? ParentMessageId { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="CreateMessageCommand"/>.
    /// </summary>
    public CreateMessageCommand() { }

    /// <summary>
    /// Initializes a new instance of <see cref="CreateMessageCommand"/> with mediator.
    /// </summary>
    /// <param name="mediator">The mediator instance.</param>
    public CreateMessageCommand(IMediator mediator) : base(mediator) { }
}
