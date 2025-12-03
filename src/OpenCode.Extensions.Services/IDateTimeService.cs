namespace OpenCode.Extensions.Services;

/// <summary>
/// Service for retrieving date and time information.
/// </summary>
public interface IDateTimeService
{
    /// <summary>
    /// Gets the current date and time from the database server.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The current date and time from the database server.</returns>
    Task<DateTime> GetCurrentDateTimeAsync(CancellationToken cancellationToken = default);
}
