using Microsoft.EntityFrameworkCore;
using Olbrasoft.Data.Cqrs;
using Olbrasoft.OpenCode.Extensions.Data.Commands;
using Olbrasoft.OpenCode.Extensions.Data.Entities;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.CommandHandlers;

/// <summary>
/// Handler for CreateMonologCommand that creates a new monolog in the database.
/// </summary>
public class CreateMonologCommandHandler : ICommandHandler<CreateMonologCommand, int>
{
    private readonly OpenCodeDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="CreateMonologCommandHandler"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CreateMonologCommandHandler(OpenCodeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<int> HandleAsync(CreateMonologCommand command, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(command);
        token.ThrowIfCancellationRequested();

        var now = DateTimeOffset.UtcNow;

        var monolog = new Monolog
        {
            SessionId = command.SessionId,
            ParentMonologId = command.ParentMonologId,
            Role = command.Role,
            FirstMessageId = command.FirstMessageId,
            Content = command.Content,
            ParticipantId = command.ParticipantId,
            ProviderId = command.ProviderId,
            ModeId = command.ModeId,
            StartedAt = command.StartedAt,
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Monologs.Add(monolog);
        await _context.SaveChangesAsync(token);

        return monolog.Id;
    }
}
