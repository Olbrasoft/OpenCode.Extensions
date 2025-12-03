using Microsoft.EntityFrameworkCore;
using OpenCode.Extensions.Data.EntityFrameworkCore.QueryHandlers;
using OpenCode.Extensions.Data.Queries;

namespace OpenCode.Extensions.Data.EntityFrameworkCore.Tests;

/// <summary>
/// Integration tests that verify end-to-end communication with PostgreSQL database.
/// These tests require a running PostgreSQL instance.
/// </summary>
/// <remarks>
/// Connection string: postgresql://opencode:jZhWpPDcKOWlipeHZUDP@127.0.0.1:5432/opencode
/// </remarks>
public class PostgresIntegrationTests : IDisposable
{
    private const string ConnectionString = "Host=127.0.0.1;Port=5432;Database=opencode;Username=opencode;Password=jZhWpPDcKOWlipeHZUDP";
    private readonly DbContext _context;

    public PostgresIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<DbContext>()
            .UseNpgsql(ConnectionString)
            .Options;

        _context = new TestDbContext(options);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    /// <summary>
    /// Minimal DbContext for integration testing raw SQL queries.
    /// </summary>
    private class TestDbContext : DbContext
    {
        public TestDbContext(DbContextOptions<DbContext> options) : base(options) { }
    }

    [Fact]
    public async Task GetCurrentDateTimeQueryHandler_ExecutesSelectNow_ReturnsValidDateTime()
    {
        // Arrange
        var handler = new GetCurrentDateTimeQueryHandler(_context);
        var query = new GetCurrentDateTimeQuery();

        // Act
        var result = await handler.HandleAsync(query, CancellationToken.None);

        // Assert
        // Result should be within 1 minute of current time
        var now = DateTime.UtcNow;
        var difference = Math.Abs((result.ToUniversalTime() - now).TotalMinutes);
        Assert.True(difference < 1, $"Database time {result} differs from local time {now} by more than 1 minute");
    }

    [Fact]
    public async Task GetCurrentDateTimeQueryHandler_MultipleCalls_ReturnsIncreasingTime()
    {
        // Arrange
        var handler = new GetCurrentDateTimeQueryHandler(_context);

        // Act
        var result1 = await handler.HandleAsync(new GetCurrentDateTimeQuery(), CancellationToken.None);
        await Task.Delay(100); // Small delay
        var result2 = await handler.HandleAsync(new GetCurrentDateTimeQuery(), CancellationToken.None);

        // Assert
        Assert.True(result2 >= result1, $"Second call {result2} should be >= first call {result1}");
    }

    [Fact]
    public async Task GetCurrentDateTimeQueryHandler_WithCancellationToken_DoesNotThrow()
    {
        // Arrange
        var handler = new GetCurrentDateTimeQueryHandler(_context);
        var query = new GetCurrentDateTimeQuery();
        using var cts = new CancellationTokenSource();

        // Act
        var result = await handler.HandleAsync(query, cts.Token);

        // Assert
        Assert.NotEqual(default, result);
    }

    [Fact]
    public async Task RawSqlQuery_SelectNow_ReturnsValidTimestamp()
    {
        // Arrange & Act - Direct SQL execution test
        // Note: SqlQueryRaw<T> expects a column named "Value" for scalar types
        var result = await _context.Database
            .SqlQueryRaw<DateTime>("SELECT NOW() AS \"Value\"")
            .FirstAsync();

        // Assert
        var now = DateTime.UtcNow;
        var difference = Math.Abs((result.ToUniversalTime() - now).TotalMinutes);
        Assert.True(difference < 1, $"Database time {result} differs significantly from system time {now}");
    }
}
