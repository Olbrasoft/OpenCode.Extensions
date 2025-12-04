using Microsoft.EntityFrameworkCore;
using Olbrasoft.Data.Cqrs;
using Olbrasoft.OpenCode.Extensions.Data.Commands;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.CommandHandlers;

/// <summary>
/// Handler for CloseMonologCommand that closes an open monolog.
/// </summary>
public class CloseMonologCommandHandler : ICommandHandler<CloseMonologCommand, bool>
{
    private readonly OpenCodeDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="CloseMonologCommandHandler"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CloseMonologCommandHandler(OpenCodeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<bool> HandleAsync(CloseMonologCommand command, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(command);
        token.ThrowIfCancellationRequested();

        var monolog = await _context.Monologs
            .FirstOrDefaultAsync(m => m.Id == command.MonologId && m.CompletedAt == null, token);

        if (monolog == null)
        {
            return false;
        }

        // Update final content if provided
        if (!string.IsNullOrEmpty(command.FinalContent))
        {
            monolog.Content = command.FinalContent;
        }

        // Close the monolog
        monolog.LastMessageId = command.LastMessageId;
        monolog.CompletedAt = command.CompletedAt;
        monolog.IsAborted = command.IsAborted;

        // Set token/cost information if provided
        if (command.TokensInput.HasValue)
        {
            monolog.TokensInput = command.TokensInput;
        }

        if (command.TokensOutput.HasValue)
        {
            monolog.TokensOutput = command.TokensOutput;
        }

        if (command.Cost.HasValue)
        {
            monolog.Cost = command.Cost;
        }

        monolog.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(token);

        return true;
    }
}
