using Microsoft.EntityFrameworkCore;
using Olbrasoft.OpenCode.Extensions.Data.Commands;
using Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore;
using Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.CommandHandlers;
using Olbrasoft.OpenCode.Extensions.Data.Entities;
using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace OpenCode.Extensions.Data.EntityFrameworkCore.Tests.CommandHandlers;

public class CloseMonologCommandHandlerTests
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
            Role = Role.Assistant,
            FirstMessageId = "msg-001",
            Content = "Response content",
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
    public async Task HandleAsync_ValidCommand_ClosesMonolog()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var monolog = await CreateOpenMonologAsync(context);
        var handler = new CloseMonologCommandHandler(context);
        var completedAt = DateTimeOffset.UtcNow;

        var command = new CloseMonologCommand
        {
            MonologId = monolog.Id,
            LastMessageId = "msg-final",
            CompletedAt = completedAt
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        var closed = await context.Monologs.FindAsync(monolog.Id);
        Assert.NotNull(closed);
        Assert.Equal("msg-final", closed.LastMessageId);
        Assert.Equal(completedAt, closed.CompletedAt);
        Assert.False(closed.IsAborted);
    }

    [Fact]
    public async Task HandleAsync_WithFinalContent_UpdatesContent()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var monolog = await CreateOpenMonologAsync(context);
        var handler = new CloseMonologCommandHandler(context);

        var command = new CloseMonologCommand
        {
            MonologId = monolog.Id,
            LastMessageId = "msg-final",
            CompletedAt = DateTimeOffset.UtcNow,
            FinalContent = "Final complete response"
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        var closed = await context.Monologs.FindAsync(monolog.Id);
        Assert.NotNull(closed);
        Assert.Equal("Final complete response", closed.Content);
    }

    [Fact]
    public async Task HandleAsync_WithTokenInfo_SetsTokens()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var monolog = await CreateOpenMonologAsync(context);
        var handler = new CloseMonologCommandHandler(context);

        var command = new CloseMonologCommand
        {
            MonologId = monolog.Id,
            LastMessageId = "msg-final",
            CompletedAt = DateTimeOffset.UtcNow,
            TokensInput = 500,
            TokensOutput = 1200,
            Cost = 0.0025m
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        var closed = await context.Monologs.FindAsync(monolog.Id);
        Assert.NotNull(closed);
        Assert.Equal(500, closed.TokensInput);
        Assert.Equal(1200, closed.TokensOutput);
        Assert.Equal(0.0025m, closed.Cost);
    }

    [Fact]
    public async Task HandleAsync_IsAborted_SetsAbortedFlag()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var monolog = await CreateOpenMonologAsync(context);
        var handler = new CloseMonologCommandHandler(context);

        var command = new CloseMonologCommand
        {
            MonologId = monolog.Id,
            LastMessageId = "msg-abort",
            CompletedAt = DateTimeOffset.UtcNow,
            IsAborted = true
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        var closed = await context.Monologs.FindAsync(monolog.Id);
        Assert.NotNull(closed);
        Assert.True(closed.IsAborted);
    }

    [Fact]
    public async Task HandleAsync_NonExistentMonolog_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new CloseMonologCommandHandler(context);

        var command = new CloseMonologCommand
        {
            MonologId = 9999,
            LastMessageId = "msg-final",
            CompletedAt = DateTimeOffset.UtcNow
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HandleAsync_AlreadyClosedMonolog_ReturnsFalse()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var monolog = await CreateOpenMonologAsync(context);
        monolog.CompletedAt = DateTimeOffset.UtcNow.AddMinutes(-5);
        monolog.LastMessageId = "msg-already-closed";
        await context.SaveChangesAsync();

        var handler = new CloseMonologCommandHandler(context);

        var command = new CloseMonologCommand
        {
            MonologId = monolog.Id,
            LastMessageId = "msg-try-close-again",
            CompletedAt = DateTimeOffset.UtcNow
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
        await Task.Delay(10);

        var handler = new CloseMonologCommandHandler(context);

        var command = new CloseMonologCommand
        {
            MonologId = monolog.Id,
            LastMessageId = "msg-final",
            CompletedAt = DateTimeOffset.UtcNow
        };

        // Act
        await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        var closed = await context.Monologs.FindAsync(monolog.Id);
        Assert.NotNull(closed);
        Assert.True(closed.UpdatedAt > originalUpdatedAt);
    }

    [Fact]
    public async Task HandleAsync_NullCommand_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new CloseMonologCommandHandler(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            handler.HandleAsync(null!, CancellationToken.None));
    }

    [Fact]
    public void Constructor_NullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new CloseMonologCommandHandler(null!));
    }

    [Fact]
    public async Task HandleAsync_WithoutOptionalFields_SetsOnlyRequired()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var monolog = await CreateOpenMonologAsync(context);
        var handler = new CloseMonologCommandHandler(context);

        var command = new CloseMonologCommand
        {
            MonologId = monolog.Id,
            LastMessageId = "msg-final",
            CompletedAt = DateTimeOffset.UtcNow
            // No FinalContent, TokensInput, TokensOutput, Cost
        };

        // Act
        var result = await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        Assert.True(result);
        var closed = await context.Monologs.FindAsync(monolog.Id);
        Assert.NotNull(closed);
        Assert.Equal("Response content", closed.Content); // Original content preserved
        Assert.Null(closed.TokensInput);
        Assert.Null(closed.TokensOutput);
        Assert.Null(closed.Cost);
    }
}
