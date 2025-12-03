using Microsoft.EntityFrameworkCore;
using Olbrasoft.Data.Cqrs;
using Olbrasoft.OpenCode.Extensions.Data.Commands;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.CommandHandlers;

/// <summary>
/// Handler for AppendMonologContentCommand that appends content to an existing open monolog.
/// </summary>
public class AppendMonologContentCommandHandler : ICommandHandler<AppendMonologContentCommand, bool>
{
    private readonly OpenCodeDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="AppendMonologContentCommandHandler"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public AppendMonologContentCommandHandler(OpenCodeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<bool> HandleAsync(AppendMonologContentCommand command, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(command);
        token.ThrowIfCancellationRequested();

        var monolog = await _context.Monologs
            .FirstOrDefaultAsync(m => m.Id == command.MonologId && m.CompletedAt == null, token);

        if (monolog == null)
        {
            return false;
        }

        // Append content with double newline separator
        monolog.Content = string.IsNullOrEmpty(monolog.Content)
            ? command.Content
            : $"{monolog.Content}\n\n{command.Content}";

        monolog.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(token);

        return true;
    }
}
