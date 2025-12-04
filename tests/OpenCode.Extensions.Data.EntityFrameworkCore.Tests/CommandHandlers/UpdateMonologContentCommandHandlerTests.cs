using Microsoft.EntityFrameworkCore;
using Olbrasoft.OpenCode.Extensions.Data.Commands;
using Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore;
using Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.CommandHandlers;
using Olbrasoft.OpenCode.Extensions.Data.Entities;
using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace OpenCode.Extensions.Data.EntityFrameworkCore.Tests.CommandHandlers;

public class UpdateMonologContentCommandHandlerTests
{
    private static OpenCodeDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<OpenCodeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new OpenCodeDbContext(options);
    }

    private static async Task<Monolog> CreateOpenMonologAsync(OpenCodeDbContext context, string content = "Original content")
    {
        var participant = new Participant
        {
            Id = 100,
            Label = "TestUser",
            Identifier = "test-user",
            Type = ParticipantType.Human
        };

        var provider = new Provider { Id = 100, Name = "TestProvider", Description = "Test" };
        var mode = new ModeEntity { Id = 1, Name = "Build" };
        var session = new Session
        {
            SessionId = "test-session-" + Guid.NewGuid(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Participants.Add(participant);
        context.Providers.Add(provider);
        context.Modes.Add(mode);
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        var monolog = new Monolog
        {
            SessionId = session.Id,
            Role = Role.Assistant,
            FirstMessageId = "msg-001",
            Content = content,
            ParticipantId = participant.Id,
            ProviderId = provider.Id,
            ModeId = mode.Id,
            StartedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Monologs.Add(monolog);
        await context.SaveChangesAsync();

        return monolog;
    }

    [Fact]
    public async Task HandleAsync_ValidCommand_ReplacesContent()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var monolog = await CreateOpenMonologAsync(context, "Old content");
        var handler = new UpdateMonologContentCommandHandler(context);

        var command = new UpdateMonologContentCommand
        {
            MonologId = monolog.Id,
            Content = "New content"
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        var updated = await context.Monologs.FindAsync(monolog.Id);
        Assert.NotNull(updated);
        Assert.Equal("New content", updated.Content);
    }

    [Fact]
    public async Task HandleAsync_NonExistentMonolog_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new UpdateMonologContentCommandHandler(context);

        var command = new UpdateMonologContentCommand
        {
            MonologId = 9999,
            Content = "Test"
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HandleAsync_ClosedMonolog_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var monolog = await CreateOpenMonologAsync(context);
        monolog.CompletedAt = DateTimeOffset.UtcNow;
        monolog.LastMessageId = "msg-end";
        await context.SaveChangesAsync();

        var handler = new UpdateMonologContentCommandHandler(context);

        var command = new UpdateMonologContentCommand
        {
            MonologId = monolog.Id,
            Content = "Should not update"
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result);
        // Verify content was not changed
        var unchanged = await context.Monologs.FindAsync(monolog.Id);
        Assert.NotNull(unchanged);
        Assert.Equal("Original content", unchanged.Content);
    }

    [Fact]
    public async Task HandleAsync_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var monolog = await CreateOpenMonologAsync(context);
        var originalUpdatedAt = monolog.UpdatedAt;
        await Task.Delay(10);

        var handler = new UpdateMonologContentCommandHandler(context);

        var command = new UpdateMonologContentCommand
        {
            MonologId = monolog.Id,
            Content = "Updated"
        };

        // Act
        await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        var updated = await context.Monologs.FindAsync(monolog.Id);
        Assert.NotNull(updated);
        Assert.True(updated.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public async Task HandleAsync_NullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new UpdateMonologContentCommandHandler(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            handler.HandleAsync(null!, CancellationToken.None));
    }

    [Fact]
    public void Constructor_NullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new UpdateMonologContentCommandHandler(null!));
    }

    [Fact]
    public async Task HandleAsync_EmptyContent_SetsEmptyString()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var monolog = await CreateOpenMonologAsync(context, "Has content");
        var handler = new UpdateMonologContentCommandHandler(context);

        var command = new UpdateMonologContentCommand
        {
            MonologId = monolog.Id,
            Content = string.Empty
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        var updated = await context.Monologs.FindAsync(monolog.Id);
        Assert.NotNull(updated);
        Assert.Equal(string.Empty, updated.Content);
    }

    [Fact]
    public async Task HandleAsync_StreamingScenario_MultipleUpdates()
    {
        // Arrange - simulates streaming where content grows with each update
        using var context = CreateInMemoryContext();
        var monolog = await CreateOpenMonologAsync(context, "");
        var handler = new UpdateMonologContentCommandHandler(context);

        // Act - simulate streaming chunks
        await handler.HandleAsync(new UpdateMonologContentCommand { MonologId = monolog.Id, Content = "Hello" }, CancellationToken.None);
        await handler.HandleAsync(new UpdateMonologContentCommand { MonologId = monolog.Id, Content = "Hello, " }, CancellationToken.None);
        await handler.HandleAsync(new UpdateMonologContentCommand { MonologId = monolog.Id, Content = "Hello, World!" }, CancellationToken.None);

        // Assert
        var updated = await context.Monologs.FindAsync(monolog.Id);
        Assert.NotNull(updated);
        Assert.Equal("Hello, World!", updated.Content);
    }
}
