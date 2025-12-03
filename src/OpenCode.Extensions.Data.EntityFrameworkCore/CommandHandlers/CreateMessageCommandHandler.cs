using Microsoft.EntityFrameworkCore;
using Olbrasoft.Data.Cqrs;
using Olbrasoft.OpenCode.Extensions.Data.Commands;
using Olbrasoft.OpenCode.Extensions.Data.Entities;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.CommandHandlers;

/// <summary>
/// Handler for CreateMessageCommand that creates a new message in the database.
/// </summary>
public class CreateMessageCommandHandler : ICommandHandler<CreateMessageCommand, int>
{
    private readonly OpenCodeDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="CreateMessageCommandHandler"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CreateMessageCommandHandler(OpenCodeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<int> HandleAsync(CreateMessageCommand command, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(command);
        token.ThrowIfCancellationRequested();

        // Get session ID by external session identifier
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.SessionId == command.SessionId, token);

        if (session == null)
        {
            throw new InvalidOperationException($"Session with ID '{command.SessionId}' not found.");
        }

        // Get participant by identifier
        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.Identifier == command.ParticipantIdentifier, token);

        if (participant == null)
        {
            throw new InvalidOperationException($"Participant with identifier '{command.ParticipantIdentifier}' not found.");
        }

        // Get provider by name
        var provider = await _context.Providers
            .FirstOrDefaultAsync(p => p.Name == command.ProviderName, token);

        if (provider == null)
        {
            throw new InvalidOperationException($"Provider with name '{command.ProviderName}' not found.");
        }

        // Get mode ID from enum value
        var modeId = (int)command.Mode;

        // Get parent message ID if specified
        int? parentMessageId = null;
        if (!string.IsNullOrEmpty(command.ParentMessageId))
        {
            var parentMessage = await _context.Messages
                .FirstOrDefaultAsync(m => m.MessageId == command.ParentMessageId, token);
            parentMessageId = parentMessage?.Id;
        }

        var message = new Message
        {
            MessageId = command.MessageId,
            SessionId = session.Id,
            ParentMessageId = parentMessageId,
            Role = command.Role,
            ModeId = modeId,
            ProviderId = provider.Id,
            ParticipantId = participant.Id,
            Content = command.Content,
            TokensInput = command.TokensInput,
            TokensOutput = command.TokensOutput,
            Cost = command.Cost,
            CreatedAt = command.CreatedAt
        };

        _context.Messages.Add(message);
        await _context.SaveChangesAsync(token);

        // Return the auto-generated database ID
        return message.Id;
    }
}
