using Microsoft.EntityFrameworkCore;
using Olbrasoft.OpenCode.Extensions.Data.Commands;
using Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore;
using Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.CommandHandlers;
using Olbrasoft.OpenCode.Extensions.Data.Entities;
using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace OpenCode.Extensions.Data.EntityFrameworkCore.Tests.CommandHandlers;

public class CreateMonologCommandHandlerTests
{
    private static OpenCodeDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<OpenCodeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new OpenCodeDbContext(options);
    }

    private static async Task<(Session session, Participant participant, Provider provider, ModeEntity mode)> SeedTestDataAsync(OpenCodeDbContext context)
    {
        var participant = new Participant
        {
            Id = Guid.NewGuid(),
            Label = "TestUser",
            Identifier = "test-user",
            Type = ParticipantType.Human
        };

        var provider = new Provider
        {
            Id = 100,
            Name = "TestProvider",
            Description = "Test"
        };

        var mode = new ModeEntity
        {
            Id = 1,
            Name = "Build"
        };

        var session = new Session
        {
            SessionId = "test-session-" + Guid.NewGuid(),
            Title = "Test Session",
            WorkingDirectory = "/home/test",
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Participants.Add(participant);
        context.Providers.Add(provider);
        context.Modes.Add(mode);
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        return (session, participant, provider, mode);
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesMonolog()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var (session, participant, provider, mode) = await SeedTestDataAsync(context);
        var handler = new CreateMonologCommandHandler(context);

        var command = new CreateMonologCommand
        {
            SessionId = session.Id,
            Role = Role.User,
            FirstMessageId = "msg-001",
            Content = "Hello, this is a test message",
            ParticipantId = participant.Id,
            ProviderId = provider.Id,
            ModeId = mode.Id,
            StartedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result > 0);
        var created = await context.Monologs.FindAsync(result);
        Assert.NotNull(created);
        Assert.Equal(session.Id, created.SessionId);
        Assert.Equal(Role.User, created.Role);
        Assert.Equal("msg-001", created.FirstMessageId);
        Assert.Equal("Hello, this is a test message", created.Content);
        Assert.Null(created.CompletedAt);
    }

    [Fact]
    public async Task HandleAsync_WithParentMonolog_SetsParentId()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var (session, participant, provider, mode) = await SeedTestDataAsync(context);
        var handler = new CreateMonologCommandHandler(context);

        // Create parent monolog first
        var parentCommand = new CreateMonologCommand
        {
            SessionId = session.Id,
            Role = Role.User,
            FirstMessageId = "msg-001",
            Content = "User question",
            ParticipantId = participant.Id,
            ProviderId = provider.Id,
            ModeId = mode.Id,
            StartedAt = DateTimeOffset.UtcNow
        };

        var parentId = await handler.HandleAsync(parentCommand, CancellationToken.None);

        // Create child (assistant response)
        var childCommand = new CreateMonologCommand
        {
            SessionId = session.Id,
            ParentMonologId = parentId,
            Role = Role.Assistant,
            FirstMessageId = "msg-002",
            Content = "Assistant response",
            ParticipantId = participant.Id,
            ProviderId = provider.Id,
            ModeId = mode.Id,
            StartedAt = DateTimeOffset.UtcNow
        };

        // Act
        var childId = await handler.HandleAsync(childCommand, CancellationToken.None);

        // Assert
        var child = await context.Monologs.FindAsync(childId);
        Assert.NotNull(child);
        Assert.Equal(parentId, child.ParentMonologId);
        Assert.Equal(Role.Assistant, child.Role);
    }

    [Fact]
    public async Task HandleAsync_NullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new CreateMonologCommandHandler(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            handler.HandleAsync(null!, CancellationToken.None));
    }

    [Fact]
    public async Task HandleAsync_CancelledToken_ThrowsOperationCancelledException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var (session, participant, provider, mode) = await SeedTestDataAsync(context);
        var handler = new CreateMonologCommandHandler(context);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var command = new CreateMonologCommand
        {
            SessionId = session.Id,
            Role = Role.User,
            FirstMessageId = "msg-001",
            Content = "Test",
            ParticipantId = participant.Id,
            ProviderId = provider.Id,
            ModeId = mode.Id,
            StartedAt = DateTimeOffset.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            handler.HandleAsync(command, cts.Token));
    }

    [Fact]
    public void Constructor_NullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CreateMonologCommandHandler(null!));
    }

    [Fact]
    public async Task HandleAsync_SetsCreatedAtAndUpdatedAt()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var (session, participant, provider, mode) = await SeedTestDataAsync(context);
        var handler = new CreateMonologCommandHandler(context);
        var beforeCreate = DateTimeOffset.UtcNow;

        var command = new CreateMonologCommand
        {
            SessionId = session.Id,
            Role = Role.User,
            FirstMessageId = "msg-001",
            Content = "Test",
            ParticipantId = participant.Id,
            ProviderId = provider.Id,
            ModeId = mode.Id,
            StartedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);
        var afterCreate = DateTimeOffset.UtcNow;

        // Assert
        var created = await context.Monologs.FindAsync(result);
        Assert.NotNull(created);
        Assert.True(created.CreatedAt >= beforeCreate && created.CreatedAt <= afterCreate);
        Assert.True(created.UpdatedAt >= beforeCreate && created.UpdatedAt <= afterCreate);
        Assert.Equal(created.CreatedAt, created.UpdatedAt);
    }
}
