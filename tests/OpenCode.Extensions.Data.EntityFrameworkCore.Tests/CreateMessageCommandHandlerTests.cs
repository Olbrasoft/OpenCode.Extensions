using Microsoft.EntityFrameworkCore;
using Olbrasoft.OpenCode.Extensions.Data.Commands;
using Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore;
using Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.CommandHandlers;
using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace OpenCode.Extensions.Data.EntityFrameworkCore.Tests;

public class CreateMessageCommandHandlerTests
{
    private static async Task<OpenCodeDbContext> CreateSqliteContextWithSeedData()
    {
        var options = new DbContextOptionsBuilder<OpenCodeDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        var context = new OpenCodeDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        return context;
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesMessage()
    {
        // Arrange
        using var context = await CreateSqliteContextWithSeedData();

        // Create a session first
        var session = new Olbrasoft.OpenCode.Extensions.Data.Entities.Session
        {
            SessionId = "test-session-123",
            Title = "Test Session",
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        var handler = new CreateMessageCommandHandler(context);
        var command = new CreateMessageCommand
        {
            MessageId = "msg-001",
            SessionId = "test-session-123",
            Role = Role.User,
            Mode = Mode.Build,
            ParticipantIdentifier = "user-jirka",
            ProviderName = "HumanInput",
            Content = "Hello, World!",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result > 0);
        
        var message = await context.Messages
            .Include(m => m.Participant)
            .Include(m => m.Provider)
            .FirstOrDefaultAsync(m => m.MessageId == "msg-001");

        Assert.NotNull(message);
        Assert.Equal("Hello, World!", message.Content);
        Assert.Equal(Role.User, message.Role);
        Assert.Equal((int)Mode.Build, message.ModeId);
        Assert.Equal("Jirka", message.Participant?.Label); // Uses existing seed data
        Assert.Equal("HumanInput", message.Provider?.Name);
    }

    [Fact]
    public async Task HandleAsync_AiAssistantMessage_CreatesMessageWithCorrectParticipant()
    {
        // Arrange
        using var context = await CreateSqliteContextWithSeedData();

        var session = new Olbrasoft.OpenCode.Extensions.Data.Entities.Session
        {
            SessionId = "ai-session",
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        var handler = new CreateMessageCommandHandler(context);
        var command = new CreateMessageCommand
        {
            MessageId = "ai-msg-001",
            SessionId = "ai-session",
            Role = Role.Assistant,
            Mode = Mode.Build,
            ParticipantIdentifier = "claude-sonnet-4.5",
            ProviderName = "Anthropic",
            Content = "Here is my response...",
            TokensInput = 100,
            TokensOutput = 500,
            Cost = 0.0025m,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result > 0);

        var message = await context.Messages
            .Include(m => m.Participant)
            .Include(m => m.Provider)
            .FirstOrDefaultAsync(m => m.MessageId == "ai-msg-001");

        Assert.NotNull(message);
        Assert.Equal(Role.Assistant, message.Role);
        Assert.Equal("Claude Sonnet 4.5", message.Participant?.Label); // Uses existing seed data
        Assert.Equal("Anthropic", message.Provider?.Name);
        Assert.Equal(100, message.TokensInput);
        Assert.Equal(500, message.TokensOutput);
        Assert.Equal(0.0025m, message.Cost);
    }

    [Fact]
    public async Task HandleAsync_NonExistentSession_AutoCreatesSession()
    {
        // Arrange
        using var context = await CreateSqliteContextWithSeedData();

        var handler = new CreateMessageCommandHandler(context);
        var command = new CreateMessageCommand
        {
            MessageId = "msg-001",
            SessionId = "auto-created-session",
            Role = Role.User,
            Mode = Mode.Build,
            ParticipantIdentifier = "user-jirka",
            ProviderName = "HumanInput",
            Content = "Test",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result > 0);
        
        // Verify session was auto-created
        var session = await context.Sessions.FirstOrDefaultAsync(s => s.SessionId == "auto-created-session");
        Assert.NotNull(session);
    }

    [Fact]
    public async Task HandleAsync_NonExistentParticipant_AutoCreatesParticipant()
    {
        // Arrange
        using var context = await CreateSqliteContextWithSeedData();

        var session = new Olbrasoft.OpenCode.Extensions.Data.Entities.Session
        {
            SessionId = "test-session",
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        var handler = new CreateMessageCommandHandler(context);
        var command = new CreateMessageCommand
        {
            MessageId = "msg-001",
            SessionId = "test-session",
            Role = Role.Assistant,
            Mode = Mode.Build,
            ParticipantIdentifier = "new-ai-model-2025",
            ProviderName = "Anthropic",
            Content = "Test",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result > 0);
        
        // Verify participant was auto-created as AI (ParticipantTypeId = 2 for AiModel)
        var participant = await context.Participants.FirstOrDefaultAsync(p => p.Identifier == "new-ai-model-2025");
        Assert.NotNull(participant);
        Assert.Equal(2, participant.ParticipantTypeId); // 2 = AiModel
    }

    [Fact]
    public async Task HandleAsync_InvalidProvider_ThrowsException()
    {
        // Arrange
        using var context = await CreateSqliteContextWithSeedData();

        var session = new Olbrasoft.OpenCode.Extensions.Data.Entities.Session
        {
            SessionId = "test-session",
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        var handler = new CreateMessageCommandHandler(context);
        var command = new CreateMessageCommand
        {
            MessageId = "msg-001",
            SessionId = "test-session",
            Role = Role.User,
            Mode = Mode.Build,
            ParticipantIdentifier = "user-jirka",
            ProviderName = "UnknownProvider",
            Content = "Test",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.HandleAsync(command, CancellationToken.None));

        Assert.Contains("UnknownProvider", exception.Message);
        Assert.Contains("not found", exception.Message);
    }

    [Fact]
    public async Task HandleAsync_WithParentMessage_SetsParentMessageId()
    {
        // Arrange
        using var context = await CreateSqliteContextWithSeedData();

        var session = new Olbrasoft.OpenCode.Extensions.Data.Entities.Session
        {
            SessionId = "threaded-session",
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        var handler = new CreateMessageCommandHandler(context);

        // Create parent message (user question)
        var parentCommand = new CreateMessageCommand
        {
            MessageId = "parent-msg",
            SessionId = "threaded-session",
            Role = Role.User,
            Mode = Mode.Build,
            ParticipantIdentifier = "user-jirka",
            ProviderName = "HumanInput",
            Content = "What is SOLID?",
            CreatedAt = DateTimeOffset.UtcNow
        };
        await handler.HandleAsync(parentCommand, CancellationToken.None);

        // Create child message (AI response)
        var childCommand = new CreateMessageCommand
        {
            MessageId = "child-msg",
            SessionId = "threaded-session",
            Role = Role.Assistant,
            Mode = Mode.Build,
            ParticipantIdentifier = "claude-sonnet-4.5",
            ProviderName = "Anthropic",
            Content = "SOLID is a set of five design principles...",
            ParentMessageId = "parent-msg",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = await handler.HandleAsync(childCommand, CancellationToken.None);

        // Assert
        var childMessage = await context.Messages
            .Include(m => m.ParentMessage)
            .FirstOrDefaultAsync(m => m.MessageId == "child-msg");

        Assert.NotNull(childMessage);
        Assert.NotNull(childMessage.ParentMessage);
        Assert.Equal("parent-msg", childMessage.ParentMessage.MessageId);
        Assert.Equal("What is SOLID?", childMessage.ParentMessage.Content);
    }
}
