namespace Olbrasoft.OpenCode.Extensions.Data.Entities;

/// <summary>
/// Reference table for ParticipantType enum.
/// </summary>
public class ParticipantTypeEntity
{
    /// <summary>
    /// Primary key (matches ParticipantType enum value).
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name of the participant type.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Participants of this type.
    /// </summary>
    public ICollection<Participant> Participants { get; set; } = [];
}
