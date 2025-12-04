namespace Olbrasoft.OpenCode.Extensions.Data.Entities;

/// <summary>
/// Mode entity for database storage.
/// Represents Build or Plan mode in OpenCode.
/// </summary>
public class ModeEntity
{
    /// <summary>
    /// Unique identifier (matches Mode enum value).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Mode name (e.g., "Build", "Plan").
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Description of the mode.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Monologs in this mode.
    /// </summary>
    public ICollection<Monolog> Monologs { get; set; } = [];
}
