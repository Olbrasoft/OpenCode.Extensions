using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenCode.Extensions.Services;

/// <summary>
/// Configuration options for OpenAI Embedding Service.
/// </summary>
public class OpenAiEmbeddingOptions
{
    /// <summary>
    /// OpenAI API key.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Embedding model to use (default: text-embedding-3-small).
    /// </summary>
    public string Model { get; set; } = "text-embedding-3-small";

    /// <summary>
    /// OpenAI API base URL.
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
}

/// <summary>
/// OpenAI implementation of embedding service using text-embedding-3-small model.
/// </summary>
public class OpenAiEmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly OpenAiEmbeddingOptions _options;
    private readonly ILogger<OpenAiEmbeddingService> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="OpenAiEmbeddingService"/>.
    /// </summary>
    public OpenAiEmbeddingService(
        HttpClient httpClient,
        IOptions<OpenAiEmbeddingOptions> options,
        ILogger<OpenAiEmbeddingService> logger)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Configure HTTP client
        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_options.ApiKey}");
    }

    /// <inheritdoc />
    public async Task<float[]> GenerateEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        var embeddings = await GenerateEmbeddingsAsync([text], cancellationToken);
        return embeddings[0];
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<float[]>> GenerateEmbeddingsAsync(IEnumerable<string> texts, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(texts);

        var textList = texts.ToList();
        if (textList.Count == 0)
        {
            return [];
        }

        var request = new EmbeddingRequest
        {
            Model = _options.Model,
            Input = textList
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("/embeddings", request, cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<EmbeddingResponse>(cancellationToken);

            if (result?.Data == null || result.Data.Count == 0)
            {
                throw new InvalidOperationException("OpenAI API returned empty embedding data");
            }

            _logger.LogDebug("Generated {Count} embeddings using model {Model}, tokens used: {Tokens}",
                result.Data.Count, _options.Model, result.Usage?.TotalTokens);

            return result.Data
                .OrderBy(d => d.Index)
                .Select(d => d.Embedding)
                .ToList();
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to generate embeddings from OpenAI API");
            throw;
        }
    }

    // Request/Response models for OpenAI Embeddings API
    private class EmbeddingRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("input")]
        public List<string> Input { get; set; } = [];
    }

    private class EmbeddingResponse
    {
        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public List<EmbeddingData> Data { get; set; } = [];

        [JsonPropertyName("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("usage")]
        public EmbeddingUsage? Usage { get; set; }
    }

    private class EmbeddingData
    {
        [JsonPropertyName("object")]
        public string Object { get; set; } = string.Empty;

        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("embedding")]
        public float[] Embedding { get; set; } = [];
    }

    private class EmbeddingUsage
    {
        [JsonPropertyName("prompt_tokens")]
        public int PromptTokens { get; set; }

        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }
    }
}
