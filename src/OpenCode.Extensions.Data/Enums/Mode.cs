namespace Olbrasoft.OpenCode.Extensions.Data.Enums;

/// <summary>
/// Mode determines what AI can do in the conversation.
/// </summary>
public enum Mode
{
    /// <summary>
    /// AI can modify files, run commands (full access).
    /// </summary>
    Build = 1,

    /// <summary>
    /// AI only suggests and plans (read-only).
    /// </summary>
    Plan = 2
}
