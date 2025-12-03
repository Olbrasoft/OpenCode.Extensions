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
            SessionId = "test-session-1",
            Title = "Test Session",
            WorkingDirectory = "/home/user/project",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        // Assert
        var retrieved = await context.Sessions.FirstOrDefaultAsync(s => s.SessionId == "test-session-1");
        Assert.NotNull(retrieved);
        Assert.Equal("Test Session", retrieved.Title);
        Assert.True(retrieved.Id > 0); // Auto-generated integer ID
    }

    [Fact]
    public async Task CanAddMessageWithRelations()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var participantType = new ParticipantTypeEntity
        {
            Id = (int)ParticipantType.Human,
            Name = nameof(ParticipantType.Human)
        };

        var participant = new Participant
        {
            Id = 1,
            Label = "Test User",
            Identifier = "test-user",
            ParticipantTypeId = (int)ParticipantType.Human
        };

        var provider = new Provider
        {
            Id = 100,
            Name = "TestProvider",
            Description = "Test"
        };

        var mode = new ModeEntity
        {
            Id = (int)Mode.Build,
            Name = nameof(Mode.Build)
        };

        var session = new Session
        {
            SessionId = "session-1",
            CreatedAt = DateTimeOffset.UtcNow
        };

        context.ParticipantTypes.Add(participantType);
        context.Participants.Add(participant);
        context.Providers.Add(provider);
        context.Modes.Add(mode);
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        var message = new Message
        {
            MessageId = "msg-123",
            SessionId = session.Id,
            ProviderId = provider.Id,
            ParticipantId = participant.Id,
            Role = Role.User,
            ModeId = (int)Mode.Build,
            Content = "Hello, World!",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        context.Messages.Add(message);
        await context.SaveChangesAsync();

        // Assert
        var retrieved = await context.Messages
            .Include(m => m.Participant)
            .Include(m => m.ModeEntity)
            .Include(m => m.Provider)
            .FirstOrDefaultAsync(m => m.MessageId == "msg-123");

        Assert.NotNull(retrieved);
        Assert.Equal("Hello, World!", retrieved.Content);
        Assert.Equal(Role.User, retrieved.Role);
        Assert.Equal((int)Mode.Build, retrieved.ModeId);
        Assert.NotNull(retrieved.Participant);
        Assert.Equal("Test User", retrieved.Participant.Label);
        Assert.NotNull(retrieved.ModeEntity);
        Assert.Equal("Build", retrieved.ModeEntity.Name);
        Assert.NotNull(retrieved.Provider);
        Assert.Equal("TestProvider", retrieved.Provider.Name);
    }

    [Fact]
    public async Task CanAddMessageWithParentMessage()
    {
        // Arrange
        using var context = CreateInMemoryContext();

        var participantType = new ParticipantTypeEntity
        {
            Id = (int)ParticipantType.Human,
            Name = nameof(ParticipantType.Human)
        };

        var humanParticipant = new Participant
        {
            Id = 1,
            Label = "User",
            Identifier = "user",
            ParticipantTypeId = (int)ParticipantType.Human
        };

        var aiParticipantType = new ParticipantTypeEntity
        {
            Id = (int)ParticipantType.AiModel,
            Name = nameof(ParticipantType.AiModel)
        };

        var aiParticipant = new Participant
        {
            Id = 2,
            Label = "Claude",
            Identifier = "claude-sonnet-4",
            ParticipantTypeId = (int)ParticipantType.AiModel
        };

        var provider = new Provider { Id = 1, Name = "HumanInput" };
        var aiProvider = new Provider { Id = 2, Name = "Anthropic" };
        var mode = new ModeEntity { Id = (int)Mode.Build, Name = nameof(Mode.Build) };
        var session = new Session { SessionId = "session-1", CreatedAt = DateTimeOffset.UtcNow };

        context.ParticipantTypes.Add(participantType);
        context.ParticipantTypes.Add(aiParticipantType);
        context.Participants.Add(humanParticipant);
        context.Participants.Add(aiParticipant);
        context.Providers.Add(provider);
        context.Providers.Add(aiProvider);
        context.Modes.Add(mode);
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        // User asks a question
        var userMessage = new Message
        {
            MessageId = "msg-1",
            SessionId = session.Id,
            ProviderId = provider.Id,
            ParticipantId = humanParticipant.Id,
            Role = Role.User,
            ModeId = (int)Mode.Build,
            Content = "What is SOLID?",
            CreatedAt = DateTimeOffset.UtcNow
        };
        context.Messages.Add(userMessage);
        await context.SaveChangesAsync();

        // Assistant responds (with ParentMessageId)
        var assistantMessage = new Message
        {
            MessageId = "msg-2",
            SessionId = session.Id,
            ProviderId = aiProvider.Id,
            ParticipantId = aiParticipant.Id,
            Role = Role.Assistant,
            ModeId = (int)Mode.Build,
            Content = "SOLID is a set of five design principles...",
            ParentMessageId = userMessage.Id,
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        context.Messages.Add(assistantMessage);
        await context.SaveChangesAsync();

        // Assert
        var retrieved = await context.Messages
            .Include(m => m.ParentMessage)
            .Include(m => m.Participant)
            .FirstOrDefaultAsync(m => m.MessageId == "msg-2");

        Assert.NotNull(retrieved);
        Assert.Equal(Role.Assistant, retrieved.Role);
        Assert.NotNull(retrieved.ParentMessage);
        Assert.Equal("msg-1", retrieved.ParentMessage.MessageId);
        Assert.Equal("What is SOLID?", retrieved.ParentMessage.Content);
        Assert.NotNull(retrieved.Participant);
        Assert.Equal("Claude", retrieved.Participant.Label);
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
        Assert.Equal(8, providers.Count); // 8 providers (HumanInput, VoiceAssistant, Anthropic, OpenAI, GitHubCopilot, Google, AzureOpenAI, xAI)
        Assert.Contains(providers, p => p.Name == "HumanInput");
        Assert.Contains(providers, p => p.Name == "VoiceAssistant");
        Assert.Contains(providers, p => p.Name == "Anthropic");
        Assert.Contains(providers, p => p.Name == "OpenAI");
        Assert.Contains(providers, p => p.Name == "Google");
        Assert.Contains(providers, p => p.Name == "xAI");
    }

    [Fact]
    public async Task ModeSeedData_IsApplied()
    {
        // Arrange - use SQLite instead of InMemory to test seed data
        var options = new DbContextOptionsBuilder<OpenCodeDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        using var context = new OpenCodeDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        // Act
        var modes = await context.Modes.ToListAsync();

        // Assert
        Assert.Equal(2, modes.Count);
        Assert.Contains(modes, m => m.Id == (int)Mode.Build && m.Name == "Build");
        Assert.Contains(modes, m => m.Id == (int)Mode.Plan && m.Name == "Plan");
    }

    [Fact]
    public async Task ParticipantTypeSeedData_IsApplied()
    {
        // Arrange - use SQLite instead of InMemory to test seed data
        var options = new DbContextOptionsBuilder<OpenCodeDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        using var context = new OpenCodeDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        // Act
        var types = await context.ParticipantTypes.ToListAsync();

        // Assert
        Assert.Equal(4, types.Count);
        Assert.Contains(types, t => t.Id == (int)ParticipantType.Human && t.Name == "Human");
        Assert.Contains(types, t => t.Id == (int)ParticipantType.AiModel && t.Name == "AiModel");
        Assert.Contains(types, t => t.Id == (int)ParticipantType.Script && t.Name == "Script");
        Assert.Contains(types, t => t.Id == (int)ParticipantType.System && t.Name == "System");
    }

    [Fact]
    public async Task ParticipantSeedData_IsApplied()
    {
        // Arrange - use SQLite instead of InMemory to test seed data
        var options = new DbContextOptionsBuilder<OpenCodeDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        using var context = new OpenCodeDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        // Act
        var participants = await context.Participants.ToListAsync();

        // Assert - 17 participants: 1 human + 16 AI models
        Assert.Equal(17, participants.Count);

        // Human
        Assert.Contains(participants, p => p.Identifier == "user-jirka" && p.ParticipantTypeId == (int)ParticipantType.Human);

        // Anthropic models
        Assert.Contains(participants, p => p.Identifier == "claude-haiku-4.5");
        Assert.Contains(participants, p => p.Identifier == "claude-sonnet-4");
        Assert.Contains(participants, p => p.Identifier == "claude-sonnet-4.5");
        Assert.Contains(participants, p => p.Identifier == "claude-opus-4.1");
        Assert.Contains(participants, p => p.Identifier == "claude-opus-4.5");

        // OpenAI models
        Assert.Contains(participants, p => p.Identifier == "gpt-4.1");
        Assert.Contains(participants, p => p.Identifier == "gpt-5");
        Assert.Contains(participants, p => p.Identifier == "gpt-5-mini");
        Assert.Contains(participants, p => p.Identifier == "gpt-5-codex");
        Assert.Contains(participants, p => p.Identifier == "gpt-5.1");
        Assert.Contains(participants, p => p.Identifier == "gpt-5.1-codex");
        Assert.Contains(participants, p => p.Identifier == "gpt-5.1-codex-mini");
        Assert.Contains(participants, p => p.Identifier == "raptor-mini");

        // Google models
        Assert.Contains(participants, p => p.Identifier == "gemini-2.5-pro");
        Assert.Contains(participants, p => p.Identifier == "gemini-3-pro");

        // xAI models
        Assert.Contains(participants, p => p.Identifier == "grok-code-fast-1");
    }
}
