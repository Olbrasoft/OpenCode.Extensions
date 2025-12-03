namespace Olbrasoft.OpenCode.Extensions.Data.Entities;

/// <summary>
/// Reference table entity for Role enum.
/// Used for foreign key relationships in database.
/// </summary>
public class RoleEntity
{
    /// <summary>
    /// Primary key - matches Role enum value.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Role name (User, Assistant).
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Messages with this role.
    /// </summary>
    public ICollection<Message> Messages { get; set; } = [];
}
