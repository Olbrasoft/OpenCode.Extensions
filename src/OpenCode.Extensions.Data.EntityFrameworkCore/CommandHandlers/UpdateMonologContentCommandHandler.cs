using Microsoft.EntityFrameworkCore;
using Olbrasoft.Data.Cqrs;
using Olbrasoft.OpenCode.Extensions.Data.Commands;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.CommandHandlers;

/// <summary>
/// Handler for UpdateMonologContentCommand that replaces content of an existing open monolog.
/// </summary>
public class UpdateMonologContentCommandHandler : ICommandHandler<UpdateMonologContentCommand, bool>
{
    private readonly OpenCodeDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="UpdateMonologContentCommandHandler"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UpdateMonologContentCommandHandler(OpenCodeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<bool> HandleAsync(UpdateMonologContentCommand command, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(command);
        token.ThrowIfCancellationRequested();

        var monolog = await _context.Monologs
            .FirstOrDefaultAsync(m => m.Id == command.MonologId && m.CompletedAt == null, token);

        if (monolog == null)
        {
            return false;
        }

        monolog.Content = command.Content;
        monolog.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(token);

        return true;
    }
}
