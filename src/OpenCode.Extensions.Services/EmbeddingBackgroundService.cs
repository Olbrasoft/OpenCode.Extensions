using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Olbrasoft.Mediation;
using Olbrasoft.OpenCode.Extensions.Data.Commands;
using Olbrasoft.OpenCode.Extensions.Data.Queries;

namespace OpenCode.Extensions.Services;

/// <summary>
/// Configuration options for Embedding Background Service.
/// </summary>
public class EmbeddingBackgroundServiceOptions
{
    /// <summary>
    /// Whether the background service is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Interval between processing cycles in seconds.
    /// </summary>
    public int IntervalSeconds { get; set; } = 30;

    /// <summary>
    /// Batch size for processing monologs without embeddings.
    /// </summary>
    public int BatchSize { get; set; } = 10;
}

/// <summary>
/// Background service that generates embeddings for closed monologs.
/// </summary>
public class EmbeddingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<EmbeddingBackgroundService> _logger;
    private readonly EmbeddingBackgroundServiceOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="EmbeddingBackgroundService"/>.
    /// </summary>
    public EmbeddingBackgroundService(
        IServiceProvider serviceProvider,
        IOptions<EmbeddingBackgroundServiceOptions> options,
        ILogger<EmbeddingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("Embedding background service is disabled");
            return;
        }

        _logger.LogInformation("Embedding background service started with interval {Interval}s and batch size {BatchSize}",
            _options.IntervalSeconds, _options.BatchSize);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessMonologsWithoutEmbeddingsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing monologs for embeddings");
            }

            await Task.Delay(TimeSpan.FromSeconds(_options.IntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Embedding background service stopped");
    }

    private async Task ProcessMonologsWithoutEmbeddingsAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();
        var embeddingService = scope.ServiceProvider.GetService<IEmbeddingService>();

        if (embeddingService == null)
        {
            _logger.LogWarning("EmbeddingService not configured, skipping embedding generation");
            return;
        }

        // Get monologs without embeddings
        var query = new GetMonologsWithoutEmbeddingQuery { Limit = _options.BatchSize };
        var monologs = await mediator.MediateAsync(query, cancellationToken);

        if (monologs.Count == 0)
        {
            return;
        }

        _logger.LogDebug("Processing {Count} monologs without embeddings", monologs.Count);

        foreach (var monolog in monologs)
        {
            try
            {
                // Generate embedding
                var embedding = await embeddingService.GenerateEmbeddingAsync(monolog.Content, cancellationToken);

                // Update monolog with embedding
                var command = new UpdateMonologEmbeddingCommand
                {
                    MonologId = monolog.Id,
                    Embedding = embedding
                };

                var success = await mediator.MediateAsync(command, cancellationToken);

                if (success)
                {
                    _logger.LogDebug("Generated embedding for monolog {MonologId}", monolog.Id);
                }
                else
                {
                    _logger.LogWarning("Failed to update embedding for monolog {MonologId}", monolog.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to generate embedding for monolog {MonologId}", monolog.Id);
            }
        }
    }
}
