using Microsoft.EntityFrameworkCore;
using Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore;
using Olbrasoft.OpenCode.Extensions.Data.Entities;
using Olbrasoft.OpenCode.Extensions.Data.Enums;

namespace OpenCode.Extensions.Data.EntityFrameworkCore.Tests;

public class OpenCodeDbContextTests
{
    private static OpenCodeDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<OpenCodeDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new OpenCodeDbContext(options);
    }

    [Fact]
    public async Task CanAddAndRetrieveSession()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var session = new Session
        {
            Id = "test-session-1",
            Title = "Test Session",
            WorkingDirectory = "/home/user/project",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        // Assert
        var retrieved = await context.Sessions.FindAsync("test-session-1");
        Assert.NotNull(retrieved);
        Assert.Equal("Test Session", retrieved.Title);
    }

    [Fact]
    public async Task CanAddMessageWithRelations()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var participant = new Participant
        {
            Id = Guid.NewGuid(),
            Label = "Jirka",
            Identifier = "user-jirka",
            Type = ParticipantType.Human
        };

        var provider = new Provider
        {
            Id = 100,
            Name = "TestProvider",
            Description = "Test"
        };

        var session = new Session
        {
            Id = "session-1",
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.Participants.Add(participant);
        context.Providers.Add(provider);
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        var message = new Message
        {
            Id = Guid.NewGuid(),
            SessionId = session.Id,
            ParticipantId = participant.Id,
            ProviderId = provider.Id,
            Role = Role.User,
            Mode = Mode.Build,
            Content = "Hello, World!",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        context.Messages.Add(message);
        await context.SaveChangesAsync();

        // Assert
        var retrieved = await context.Messages
            .Include(m => m.Participant)
            .Include(m => m.Provider)
            .FirstOrDefaultAsync(m => m.Id == message.Id);

        Assert.NotNull(retrieved);
        Assert.Equal("Hello, World!", retrieved.Content);
        Assert.Equal(Role.User, retrieved.Role);
        Assert.Equal(Mode.Build, retrieved.Mode);
        Assert.NotNull(retrieved.Participant);
        Assert.Equal("Jirka", retrieved.Participant.Label);
        Assert.NotNull(retrieved.Provider);
        Assert.Equal("TestProvider", retrieved.Provider.Name);
    }

    [Fact]
    public async Task ProviderSeedData_IsApplied()
    {
        // Arrange - use SQLite instead of InMemory to test seed data
        var options = new DbContextOptionsBuilder<OpenCodeDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        using var context = new OpenCodeDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        // Act
        var providers = await context.Providers.ToListAsync();

        // Assert
        Assert.Equal(8, providers.Count);
        Assert.Contains(providers, p => p.Name == "Keyboard");
        Assert.Contains(providers, p => p.Name == "VoiceAssistant");
        Assert.Contains(providers, p => p.Name == "Anthropic");
    }
}
