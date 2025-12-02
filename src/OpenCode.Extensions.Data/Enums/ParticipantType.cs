namespace Olbrasoft.OpenCode.Extensions.Data.Enums;

/// <summary>
/// Type of participant in the conversation.
/// </summary>
public enum ParticipantType
{
    /// <summary>
    /// Human user (e.g., Jirka, colleague).
    /// </summary>
    Human = 1,

    /// <summary>
    /// AI model (e.g., Claude, GPT-4, Gemini).
    /// </summary>
    AiModel = 2,

    /// <summary>
    /// Automated script or CI/CD pipeline.
    /// </summary>
    Script = 3,

    /// <summary>
    /// System-generated messages.
    /// </summary>
    System = 4
}
