using Microsoft.EntityFrameworkCore;
using Olbrasoft.Data.Cqrs;
using Olbrasoft.OpenCode.Extensions.Data.Commands;
using Pgvector;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.CommandHandlers;

/// <summary>
/// Handler for UpdateMonologEmbeddingCommand that updates the embedding vector for a monolog.
/// </summary>
public class UpdateMonologEmbeddingCommandHandler : ICommandHandler<UpdateMonologEmbeddingCommand, bool>
{
    private readonly OpenCodeDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="UpdateMonologEmbeddingCommandHandler"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public UpdateMonologEmbeddingCommandHandler(OpenCodeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<bool> HandleAsync(UpdateMonologEmbeddingCommand command, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(command);
        token.ThrowIfCancellationRequested();

        var monolog = await _context.Monologs
            .FirstOrDefaultAsync(m => m.Id == command.MonologId, token);

        if (monolog == null)
        {
            return false;
        }

        monolog.Embedding = new Vector(command.Embedding);
        monolog.UpdatedAt = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync(token);

        return true;
    }
}
