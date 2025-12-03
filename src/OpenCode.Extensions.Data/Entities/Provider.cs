namespace Olbrasoft.OpenCode.Extensions.Data.Entities;

/// <summary>
/// Provider represents the source/channel from which a message originated.
/// Examples: Keyboard, VoiceAssistant, Anthropic API, OpenAI API.
/// </summary>
public class Provider
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Provider name (e.g., "Keyboard", "VoiceAssistant", "Anthropic").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Description of the provider.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Messages received through this provider.
    /// </summary>
    public ICollection<Message> Messages { get; set; } = [];

    /// <summary>
    /// AI models available from this provider.
    /// </summary>
    public ICollection<Model> Models { get; set; } = [];
}
