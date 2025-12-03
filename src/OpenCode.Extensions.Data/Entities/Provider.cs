namespace Olbrasoft.OpenCode.Extensions.Data.Entities;

/// <summary>
/// Provider represents the source/channel from which a message originated.
/// Examples: HumanInput, VoiceAssistant, Anthropic API, OpenAI API.
/// </summary>
public class Provider
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Provider name (e.g., "HumanInput", "VoiceAssistant", "Anthropic").
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
}
