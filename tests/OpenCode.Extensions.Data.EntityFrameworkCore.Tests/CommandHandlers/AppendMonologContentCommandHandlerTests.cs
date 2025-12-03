using Microsoft.EntityFrameworkCore;
using Olbrasoft.OpenCode.Extensions.Data.Commands;
using Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore;
using Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.CommandHandlers;
using Olbrasoft.OpenCode.Extensions.Data.Entities;
using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace OpenCode.Extensions.Data.EntityFrameworkCore.Tests.CommandHandlers;

public class AppendMonologContentCommandHandlerTests
{
    private static OpenCodeDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<OpenCodeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new OpenCodeDbContext(options);
    }

    private static async Task<Monolog> CreateOpenMonologAsync(OpenCodeDbContext context)
    {
        var participant = new Participant
        {
            Id = Guid.NewGuid(),
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
            Role = Role.User,
            FirstMessageId = "msg-001",
            Content = "First message",
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
    public async Task HandleAsync_ValidCommand_AppendsContent()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var monolog = await CreateOpenMonologAsync(context);
        var handler = new AppendMonologContentCommandHandler(context);

        var command = new AppendMonologContentCommand
        {
            MonologId = monolog.Id,
            Content = "Second message"
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        var updated = await context.Monologs.FindAsync(monolog.Id);
        Assert.NotNull(updated);
        Assert.Equal("First message\n\nSecond message", updated.Content);
    }

    [Fact]
    public async Task HandleAsync_EmptyOriginalContent_SetsNewContent()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var monolog = await CreateOpenMonologAsync(context);
        monolog.Content = string.Empty;
        await context.SaveChangesAsync();

        var handler = new AppendMonologContentCommandHandler(context);

        var command = new AppendMonologContentCommand
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
        var handler = new AppendMonologContentCommandHandler(context);

        var command = new AppendMonologContentCommand
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

        var handler = new AppendMonologContentCommandHandler(context);

        var command = new AppendMonologContentCommand
        {
            MonologId = monolog.Id,
            Content = "Should not append"
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HandleAsync_UpdatesUpdatedAtTimestamp()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var monolog = await CreateOpenMonologAsync(context);
        var originalUpdatedAt = monolog.UpdatedAt;
        await Task.Delay(10); // Ensure time difference

        var handler = new AppendMonologContentCommandHandler(context);

        var command = new AppendMonologContentCommand
        {
            MonologId = monolog.Id,
            Content = "New content"
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
        var handler = new AppendMonologContentCommandHandler(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            handler.HandleAsync(null!, CancellationToken.None));
    }

    [Fact]
    public void Constructor_NullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new AppendMonologContentCommandHandler(null!));
    }

    [Fact]
    public async Task HandleAsync_MultipleAppends_JoinsWithDoubleNewline()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var monolog = await CreateOpenMonologAsync(context);
        monolog.Content = "First";
        await context.SaveChangesAsync();

        var handler = new AppendMonologContentCommandHandler(context);

        // Act - append twice
        await handler.HandleAsync(new AppendMonologContentCommand { MonologId = monolog.Id, Content = "Second" }, CancellationToken.None);
        await handler.HandleAsync(new AppendMonologContentCommand { MonologId = monolog.Id, Content = "Third" }, CancellationToken.None);

        // Assert
        var updated = await context.Monologs.FindAsync(monolog.Id);
        Assert.NotNull(updated);
        Assert.Equal("First\n\nSecond\n\nThird", updated.Content);
    }
}
