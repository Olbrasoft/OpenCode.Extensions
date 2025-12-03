namespace Olbrasoft.OpenCode.Extensions.Data.Entities;

/// <summary>
/// Simple error log - just capture everything that fails.
/// </summary>
public class ErrorLog
{
    public int Id { get; set; }
    
    public DateTimeOffset OccurredAt { get; set; }
    
    /// <summary>
    /// Full error text - message, stack trace, request payload, whatever.
    /// </summary>
    public required string Text { get; set; }
}
