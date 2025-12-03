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

        var provider = new Provider
        {
            Id = 100,
            Name = "TestProvider",
            Description = "Test"
        };

        var role = new RoleEntity
        {
            Id = (int)Role.User,
            Name = nameof(Role.User)
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

        context.Providers.Add(provider);
        context.Roles.Add(role);
        context.Modes.Add(mode);
        context.Sessions.Add(session);
        await context.SaveChangesAsync();

        var message = new Message
        {
            MessageId = "msg-123",
            SessionId = session.Id,
            ProviderId = provider.Id,
            RoleId = (int)Role.User,
            ModeId = (int)Mode.Build,
            Content = "Hello, World!",
            CreatedAt = DateTimeOffset.UtcNow
        };

        // Act
        context.Messages.Add(message);
        await context.SaveChangesAsync();

        // Assert
        var retrieved = await context.Messages
            .Include(m => m.RoleEntity)
            .Include(m => m.ModeEntity)
            .Include(m => m.Provider)
            .FirstOrDefaultAsync(m => m.MessageId == "msg-123");

        Assert.NotNull(retrieved);
        Assert.Equal("Hello, World!", retrieved.Content);
        Assert.Equal((int)Role.User, retrieved.RoleId);
        Assert.Equal((int)Mode.Build, retrieved.ModeId);
        Assert.NotNull(retrieved.RoleEntity);
        Assert.Equal("User", retrieved.RoleEntity.Name);
        Assert.NotNull(retrieved.ModeEntity);
        Assert.Equal("Build", retrieved.ModeEntity.Name);
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
        Assert.Equal(9, providers.Count); // 9 providers including xAI
        Assert.Contains(providers, p => p.Name == "Keyboard");
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
    public async Task RoleSeedData_IsApplied()
    {
        // Arrange - use SQLite instead of InMemory to test seed data
        var options = new DbContextOptionsBuilder<OpenCodeDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        using var context = new OpenCodeDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        // Act
        var roles = await context.Roles.ToListAsync();

        // Assert
        Assert.Equal(2, roles.Count);
        Assert.Contains(roles, r => r.Id == (int)Role.User && r.Name == "User");
        Assert.Contains(roles, r => r.Id == (int)Role.Assistant && r.Name == "Assistant");
    }

    [Fact]
    public async Task ModelSeedData_IsApplied()
    {
        // Arrange - use SQLite instead of InMemory to test seed data
        var options = new DbContextOptionsBuilder<OpenCodeDbContext>()
            .UseSqlite("DataSource=:memory:")
            .Options;

        using var context = new OpenCodeDbContext(options);
        await context.Database.OpenConnectionAsync();
        await context.Database.EnsureCreatedAsync();

        // Act
        var models = await context.Models.ToListAsync();

        // Assert - 16 models from GitHub Copilot supported models (December 2025)
        Assert.Equal(16, models.Count);
        
        // Anthropic models
        Assert.Contains(models, m => m.Name == "claude-haiku-4.5");
        Assert.Contains(models, m => m.Name == "claude-sonnet-4");
        Assert.Contains(models, m => m.Name == "claude-sonnet-4.5");
        Assert.Contains(models, m => m.Name == "claude-opus-4.1");
        Assert.Contains(models, m => m.Name == "claude-opus-4.5");
        
        // OpenAI models
        Assert.Contains(models, m => m.Name == "gpt-4.1");
        Assert.Contains(models, m => m.Name == "gpt-5");
        Assert.Contains(models, m => m.Name == "gpt-5-mini");
        Assert.Contains(models, m => m.Name == "gpt-5-codex");
        Assert.Contains(models, m => m.Name == "gpt-5.1");
        Assert.Contains(models, m => m.Name == "gpt-5.1-codex");
        Assert.Contains(models, m => m.Name == "gpt-5.1-codex-mini");
        Assert.Contains(models, m => m.Name == "raptor-mini");
        
        // Google models
        Assert.Contains(models, m => m.Name == "gemini-2.5-pro");
        Assert.Contains(models, m => m.Name == "gemini-3-pro");
        
        // xAI models
        Assert.Contains(models, m => m.Name == "grok-code-fast-1");
    }
}
