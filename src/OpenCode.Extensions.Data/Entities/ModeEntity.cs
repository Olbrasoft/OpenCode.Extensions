namespace Olbrasoft.OpenCode.Extensions.Data.Entities;

/// <summary>
/// Reference table entity for Mode enum.
/// Used for foreign key relationships in database.
/// </summary>
public class ModeEntity
{
    /// <summary>
    /// Primary key - matches Mode enum value.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Mode name (Build, Plan).
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Messages with this mode.
    /// </summary>
    public ICollection<Message> Messages { get; set; } = [];
}
