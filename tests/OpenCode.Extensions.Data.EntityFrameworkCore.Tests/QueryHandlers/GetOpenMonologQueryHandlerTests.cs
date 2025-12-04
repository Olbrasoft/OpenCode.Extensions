using Microsoft.EntityFrameworkCore;
using Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore;
using Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.QueryHandlers;
using Olbrasoft.OpenCode.Extensions.Data.Entities;
using Olbrasoft.OpenCode.Extensions.Data.Enums;
using Olbrasoft.OpenCode.Extensions.Data.Queries;

namespace OpenCode.Extensions.Data.EntityFrameworkCore.Tests.QueryHandlers;

public class GetOpenMonologQueryHandlerTests
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

        return (session, participant, provider, mode);
    }

    [Fact]
    public async Task HandleAsync_OpenMonologExists_ReturnsMonolog()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var (session, participant, provider, mode) = await SeedTestDataAsync(context);

        var monolog = new Monolog
        {
            SessionId = session.Id,
            Role = Role.User,
            FirstMessageId = "msg-001",
            Content = "Open monolog",
            ParticipantId = participant.Id,
            ProviderId = provider.Id,
            ModeId = mode.Id,
            StartedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Monologs.Add(monolog);
        await context.SaveChangesAsync();

        var handler = new GetOpenMonologQueryHandler(context);
        var query = new GetOpenMonologQuery
        {
            SessionId = session.Id,
            Role = Role.User
        };

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(monolog.Id, result.Id);
        Assert.Equal("Open monolog", result.Content);
    }

    [Fact]
    public async Task HandleAsync_NoOpenMonolog_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var (session, _, _, _) = await SeedTestDataAsync(context);

        var handler = new GetOpenMonologQueryHandler(context);
        var query = new GetOpenMonologQuery
        {
            SessionId = session.Id,
            Role = Role.User
        };

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task HandleAsync_ClosedMonologOnly_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var (session, participant, provider, mode) = await SeedTestDataAsync(context);

        var monolog = new Monolog
        {
            SessionId = session.Id,
            Role = Role.User,
            FirstMessageId = "msg-001",
            LastMessageId = "msg-end",
            Content = "Closed monolog",
            ParticipantId = participant.Id,
            ProviderId = provider.Id,
            ModeId = mode.Id,
            StartedAt = DateTimeOffset.UtcNow,
            CompletedAt = DateTimeOffset.UtcNow, // Closed!
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Monologs.Add(monolog);
        await context.SaveChangesAsync();

        var handler = new GetOpenMonologQueryHandler(context);
        var query = new GetOpenMonologQuery
        {
            SessionId = session.Id,
            Role = Role.User
        };

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task HandleAsync_DifferentRole_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var (session, participant, provider, mode) = await SeedTestDataAsync(context);

        var monolog = new Monolog
        {
            SessionId = session.Id,
            Role = Role.Assistant, // Different role
            FirstMessageId = "msg-001",
            Content = "Assistant monolog",
            ParticipantId = participant.Id,
            ProviderId = provider.Id,
            ModeId = mode.Id,
            StartedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Monologs.Add(monolog);
        await context.SaveChangesAsync();

        var handler = new GetOpenMonologQueryHandler(context);
        var query = new GetOpenMonologQuery
        {
            SessionId = session.Id,
            Role = Role.User // Looking for User
        };

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task HandleAsync_DifferentSession_ReturnsNull()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var (session, participant, provider, mode) = await SeedTestDataAsync(context);

        var monolog = new Monolog
        {
            SessionId = session.Id,
            Role = Role.User,
            FirstMessageId = "msg-001",
            Content = "Session 1 monolog",
            ParticipantId = participant.Id,
            ProviderId = provider.Id,
            ModeId = mode.Id,
            StartedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Monologs.Add(monolog);
        await context.SaveChangesAsync();

        var handler = new GetOpenMonologQueryHandler(context);
        var query = new GetOpenMonologQuery
        {
            SessionId = 9999, // Different session
            Role = Role.User
        };

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task HandleAsync_MultipleOpenMonologs_ReturnsMostRecent()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var (session, participant, provider, mode) = await SeedTestDataAsync(context);

        var olderMonolog = new Monolog
        {
            SessionId = session.Id,
            Role = Role.User,
            FirstMessageId = "msg-001",
            Content = "Older",
            ParticipantId = participant.Id,
            ProviderId = provider.Id,
            ModeId = mode.Id,
            StartedAt = DateTimeOffset.UtcNow.AddMinutes(-10),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        var newerMonolog = new Monolog
        {
            SessionId = session.Id,
            Role = Role.User,
            FirstMessageId = "msg-002",
            Content = "Newer",
            ParticipantId = participant.Id,
            ProviderId = provider.Id,
            ModeId = mode.Id,
            StartedAt = DateTimeOffset.UtcNow,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        context.Monologs.AddRange(olderMonolog, newerMonolog);
        await context.SaveChangesAsync();

        var handler = new GetOpenMonologQueryHandler(context);
        var query = new GetOpenMonologQuery
        {
            SessionId = session.Id,
            Role = Role.User
        };

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Newer", result.Content);
    }

    [Fact]
    public async Task HandleAsync_NullQuery_ThrowsArgumentNullException()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var handler = new GetOpenMonologQueryHandler(context);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            handler.HandleAsync(null!, CancellationToken.None));
    }

    [Fact]
    public void Constructor_NullContext_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new GetOpenMonologQueryHandler(null!));
    }
}
