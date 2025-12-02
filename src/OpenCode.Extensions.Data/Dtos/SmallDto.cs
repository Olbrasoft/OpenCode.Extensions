namespace Olbrasoft.OpenCode.Extensions.Data.Dtos;

/// <summary>
/// Base DTO with Id property.
/// </summary>
public abstract class SmallDto
{
    /// <summary>
    /// Entity identifier.
    /// </summary>
    public int Id { get; set; }
}

/// <summary>
/// Base DTO with Guid Id property.
/// </summary>
public abstract class SmallGuidDto
{
    /// <summary>
    /// Entity identifier.
    /// </summary>
    public Guid Id { get; set; }
}

/// <summary>
/// Base DTO with string Id property.
/// </summary>
public abstract class SmallStringDto
{
    /// <summary>
    /// Entity identifier.
    /// </summary>
    public required string Id { get; set; }
}
