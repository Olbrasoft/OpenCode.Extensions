using Microsoft.EntityFrameworkCore;
using Olbrasoft.Mediation;
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
builder.Services.AddScoped<IMonologService, MonologService>();

// Add OpenAI Embedding Service
builder.Services.Configure<OpenAiEmbeddingOptions>(
    builder.Configuration.GetSection("OpenAiEmbedding"));
builder.Services.AddHttpClient<IEmbeddingService, OpenAiEmbeddingService>();

// Add Embedding Background Service
builder.Services.Configure<EmbeddingBackgroundServiceOptions>(
    builder.Configuration.GetSection("EmbeddingBackgroundService"));
builder.Services.AddHostedService<EmbeddingBackgroundService>();

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// ==================== Session Endpoints ====================

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

// ==================== Monolog Endpoints ====================

// Get open monolog by session and role
app.MapGet("/api/monologs/open", async (int sessionId, int role, IMonologService monologService, CancellationToken cancellationToken) =>
{
    var monolog = await monologService.GetOpenMonologAsync(sessionId, (Role)role, cancellationToken);

    return monolog != null
        ? Results.Ok(new MonologResponse(monolog))
        : Results.NotFound(new { message = "No open monolog found" });
})
.WithName("GetOpenMonolog");

// Create new monolog
app.MapPost("/api/monologs", async (CreateMonologRequest request, IMonologService monologService, CancellationToken cancellationToken) =>
{
    var id = await monologService.CreateMonologAsync(
        request.SessionId,
        request.ParentMonologId,
        (Role)request.Role,
        request.FirstMessageId,
        request.Content ?? string.Empty,
        request.ParticipantId,
        request.ProviderId,
        request.ModeId,
        request.StartedAt,
        cancellationToken);

    return id > 0
        ? Results.Created($"/api/monologs/{id}", new { id })
        : Results.BadRequest("Failed to create monolog");
})
.WithName("CreateMonolog");

// Append content to monolog
app.MapPut("/api/monologs/{id}/append", async (int id, AppendContentRequest request, IMonologService monologService, CancellationToken cancellationToken) =>
{
    var success = await monologService.AppendContentAsync(id, request.Content, cancellationToken);

    return success
        ? Results.Ok(new { success = true })
        : Results.NotFound(new { message = "Monolog not found or already closed" });
})
.WithName("AppendMonologContent");

// Update/replace content of monolog
app.MapPut("/api/monologs/{id}/content", async (int id, UpdateContentRequest request, IMonologService monologService, CancellationToken cancellationToken) =>
{
    var success = await monologService.UpdateContentAsync(id, request.Content, cancellationToken);

    return success
        ? Results.Ok(new { success = true })
        : Results.NotFound(new { message = "Monolog not found or already closed" });
})
.WithName("UpdateMonologContent");

// Close monolog
app.MapPut("/api/monologs/{id}/close", async (int id, CloseMonologRequest request, IMonologService monologService, CancellationToken cancellationToken) =>
{
    var success = await monologService.CloseMonologAsync(
        id,
        request.LastMessageId,
        request.FinalContent,
        request.CompletedAt,
        request.IsAborted,
        request.TokensInput,
        request.TokensOutput,
        request.Cost,
        cancellationToken);

    return success
        ? Results.Ok(new { success = true })
        : Results.NotFound(new { message = "Monolog not found or already closed" });
})
.WithName("CloseMonolog");

// Search monologs (semantic vector search)
app.MapPost("/api/monologs/search", async (SearchMonologsRequest request, IMonologService monologService, CancellationToken cancellationToken) =>
{
    var results = await monologService.SearchMonologsAsync(
        request.QueryEmbedding,
        request.SessionId,
        request.Limit ?? 10,
        request.MinSimilarity ?? 0.5,
        cancellationToken);

    return Results.Ok(new
    {
        count = results.Count,
        results = results.Select(r => new
        {
            monolog = new MonologResponse(r.Monolog),
            similarity = r.Similarity
        })
    });
})
.WithName("SearchMonologs");

// ==================== Health Check ====================

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTimeOffset.UtcNow }))
    .WithName("HealthCheck");

app.Run();

// ==================== Request/Response Models ====================

/// <summary>
/// Request model for creating a new session.
/// </summary>
public record CreateSessionRequest(
    string SessionId,
    string? Title,
    string? WorkingDirectory,
    DateTimeOffset CreatedAt);

/// <summary>
/// Request model for creating a new monolog.
/// </summary>
public record CreateMonologRequest(
    int SessionId,
    int? ParentMonologId,
    int Role,
    string FirstMessageId,
    string? Content,
    Guid ParticipantId,
    int ProviderId,
    int ModeId,
    DateTimeOffset StartedAt);

/// <summary>
/// Request model for appending content to a monolog.
/// </summary>
public record AppendContentRequest(string Content);

/// <summary>
/// Request model for updating content of a monolog.
/// </summary>
public record UpdateContentRequest(string Content);

/// <summary>
/// Request model for closing a monolog.
/// </summary>
public record CloseMonologRequest(
    string LastMessageId,
    string? FinalContent,
    DateTimeOffset CompletedAt,
    bool IsAborted,
    int? TokensInput,
    int? TokensOutput,
    decimal? Cost);

/// <summary>
/// Request model for searching monologs.
/// </summary>
public record SearchMonologsRequest(
    float[] QueryEmbedding,
    int? SessionId,
    int? Limit,
    double? MinSimilarity);

/// <summary>
/// Response model for monolog data.
/// </summary>
public record MonologResponse
{
    public int Id { get; init; }
    public int SessionId { get; init; }
    public int? ParentMonologId { get; init; }
    public int Role { get; init; }
    public string FirstMessageId { get; init; } = string.Empty;
    public string? LastMessageId { get; init; }
    public string Content { get; init; } = string.Empty;
    public Guid ParticipantId { get; init; }
    public int ProviderId { get; init; }
    public int ModeId { get; init; }
    public int? TokensInput { get; init; }
    public int? TokensOutput { get; init; }
    public decimal? Cost { get; init; }
    public DateTimeOffset StartedAt { get; init; }
    public DateTimeOffset? CompletedAt { get; init; }
    public bool IsAborted { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    public MonologResponse() { }

    public MonologResponse(Olbrasoft.OpenCode.Extensions.Data.Entities.Monolog monolog)
    {
        Id = monolog.Id;
        SessionId = monolog.SessionId;
        ParentMonologId = monolog.ParentMonologId;
        Role = (int)monolog.Role;
        FirstMessageId = monolog.FirstMessageId;
        LastMessageId = monolog.LastMessageId;
        Content = monolog.Content;
        ParticipantId = monolog.ParticipantId;
        ProviderId = monolog.ProviderId;
        ModeId = monolog.ModeId;
        TokensInput = monolog.TokensInput;
        TokensOutput = monolog.TokensOutput;
        Cost = monolog.Cost;
        StartedAt = monolog.StartedAt;
        CompletedAt = monolog.CompletedAt;
        IsAborted = monolog.IsAborted;
        CreatedAt = monolog.CreatedAt;
        UpdatedAt = monolog.UpdatedAt;
    }
}
