using Microsoft.EntityFrameworkCore;
using Olbrasoft.Mediation;
using Olbrasoft.OpenCode.Extensions.Data.Commands;
using Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore;
using Olbrasoft.OpenCode.Extensions.Data.Enums;
using OpenCode.Extensions.Services;

var builder = WebApplication.CreateBuilder(args);

// Configure services
builder.Services.AddOpenApi();

// Add DbContext with PostgreSQL
builder.Services.AddDbContext<OpenCodeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Mediation with handlers from EF Core assembly
builder.Services
    .AddMediation(typeof(OpenCodeDbContext).Assembly)
    .UseRequestHandlerMediator();

// Add application services
builder.Services.AddScoped<ISessionService, SessionService>();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// API Endpoints

// Upsert session - creates new or returns existing
app.MapPost("/api/sessions", async (CreateSessionRequest request, ISessionService sessionService, CancellationToken cancellationToken) =>
{
    var id = await sessionService.GetOrCreateSessionAsync(
        request.SessionId,
        request.Title,
        request.WorkingDirectory,
        request.CreatedAt,
        cancellationToken);

    return id > 0
        ? Results.Ok(new { id, sessionId = request.SessionId })
        : Results.BadRequest("Failed to create session");
})
.WithName("CreateSession");

// Get session Id by external SessionId
app.MapGet("/api/sessions/{sessionId}", async (string sessionId, ISessionService sessionService, CancellationToken cancellationToken) =>
{
    var id = await sessionService.GetSessionIdAsync(sessionId, cancellationToken);

    return id.HasValue
        ? Results.Ok(new { id = id.Value, sessionId })
        : Results.NotFound(new { message = $"Session '{sessionId}' not found" });
})
.WithName("GetSession");

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }))
    .WithName("HealthCheck");

// Create message
app.MapPost("/api/messages", async (CreateMessageRequest request, IMediator mediator, CancellationToken cancellationToken) =>
{
    try
    {
        var command = new CreateMessageCommand
        {
            MessageId = request.MessageId,
            SessionId = request.SessionId,
            Role = request.Role,
            Mode = request.Mode,
            ParticipantIdentifier = request.ParticipantIdentifier,
            ProviderName = request.ProviderName,
            Content = request.Content,
            TokensInput = request.TokensInput,
            TokensOutput = request.TokensOutput,
            Cost = request.Cost,
            CreatedAt = request.CreatedAt,
            ParentMessageId = request.ParentMessageId
        };

        var id = await mediator.MediateAsync(command, cancellationToken);

        return Results.Ok(new { id, messageId = request.MessageId });
    }
    catch (InvalidOperationException ex)
    {
        return Results.BadRequest(new { error = ex.Message });
    }
})
.WithName("CreateMessage");

app.Run();

/// <summary>
/// Request model for creating a new session.
/// </summary>
public record CreateSessionRequest(
    string SessionId,
    string? Title,
    string? WorkingDirectory,
    DateTimeOffset CreatedAt);

/// <summary>
/// Request model for creating a new message.
/// </summary>
public record CreateMessageRequest(
    string MessageId,
    string SessionId,
    Role Role,
    Mode Mode,
    string ParticipantIdentifier,
    string ProviderName,
    string? Content,
    int? TokensInput,
    int? TokensOutput,
    decimal? Cost,
    DateTimeOffset CreatedAt,
    string? ParentMessageId);
