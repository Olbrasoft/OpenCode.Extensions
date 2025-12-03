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

        // Content is required and cannot be empty
        if (string.IsNullOrWhiteSpace(command.Content))
        {
            throw new ArgumentException("Message content cannot be null or empty.", nameof(command));
        }

        // Get or create session by external session identifier
        var session = await _context.Sessions
            .FirstOrDefaultAsync(s => s.SessionId == command.SessionId, token);

        if (session == null)
        {
            // Auto-create session if it doesn't exist yet
            session = new Session
            {
                SessionId = command.SessionId,
                Title = null,
                WorkingDirectory = null,
                CreatedAt = command.CreatedAt
            };
            _context.Sessions.Add(session);
            await _context.SaveChangesAsync(token);
        }

        // Get or create participant by identifier (case-insensitive)
        var participant = await _context.Participants
            .FirstOrDefaultAsync(p => p.Identifier.ToLower() == command.ParticipantIdentifier.ToLower(), token);

        if (participant == null)
        {
            // Auto-create participant if it doesn't exist
            // Determine participant type based on identifier pattern:
            // - "user-*" -> Human (ParticipantTypeId = 1)
            // - everything else -> AiModel (ParticipantTypeId = 2)
            var participantTypeId = command.ParticipantIdentifier.StartsWith("user-", StringComparison.OrdinalIgnoreCase) ? 1 : 2;
            
            participant = new Participant
            {
                Identifier = command.ParticipantIdentifier,
                Label = command.ParticipantIdentifier, // Use identifier as label initially
                ParticipantTypeId = participantTypeId
            };
            _context.Participants.Add(participant);
            await _context.SaveChangesAsync(token);
        }

        // Normalize provider name: "github-copilot" -> "GitHubCopilot"
        var normalizedProviderName = NormalizeProviderName(command.ProviderName);
        
        // Get provider by normalized name (case-insensitive)
        var provider = await _context.Providers
            .FirstOrDefaultAsync(p => p.Name.ToLower() == normalizedProviderName.ToLower(), token);

        if (provider == null)
        {
            throw new InvalidOperationException($"Provider with name '{command.ProviderName}' (normalized: '{normalizedProviderName}') not found.");
        }

        // Get mode ID from enum value (null for user messages -> default to Build)
        var modeId = command.Mode.HasValue ? (int)command.Mode.Value : 1;

        // Get parent message ID if specified
        int? parentMessageId = null;
        if (!string.IsNullOrEmpty(command.ParentMessageId))
        {
            var parentMessage = await _context.Messages
                .FirstOrDefaultAsync(m => m.MessageId == command.ParentMessageId, token);
            parentMessageId = parentMessage?.Id;
        }

        // Check if message already exists (upsert logic)
        var existingMessage = await _context.Messages
            .FirstOrDefaultAsync(m => m.MessageId == command.MessageId, token);

        if (existingMessage != null)
        {
            // Update existing message - especially content which may come later
            if (command.Content != null)
            {
                existingMessage.Content = command.Content;
            }
            // Update other fields that may have changed
            if (command.TokensInput.HasValue)
            {
                existingMessage.TokensInput = command.TokensInput;
            }
            if (command.TokensOutput.HasValue)
            {
                existingMessage.TokensOutput = command.TokensOutput;
            }
            if (command.Cost.HasValue)
            {
                existingMessage.Cost = command.Cost;
            }
            
            await _context.SaveChangesAsync(token);
            return existingMessage.Id;
        }

        // Create new message
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

    /// <summary>
    /// Normalizes provider names from OpenCode format to database format.
    /// E.g., "github-copilot" -> "GitHubCopilot", "anthropic" -> "Anthropic"
    /// </summary>
    private static string NormalizeProviderName(string providerName)
    {
        // Map of OpenCode provider IDs to database provider names
        return providerName.ToLowerInvariant() switch
        {
            "github-copilot" => "GitHubCopilot",
            "anthropic" => "Anthropic",
            "openai" => "OpenAI",
            "google" => "Google",
            "azure-openai" => "AzureOpenAI",
            "xai" => "xAI",
            "humaninput" => "HumanInput",
            "voiceassistant" => "VoiceAssistant",
            _ => providerName // Return as-is if not in map
        };
    }
}
