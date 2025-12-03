using Olbrasoft.Data.Cqrs;
using Olbrasoft.OpenCode.Extensions.Data.Commands;
using Olbrasoft.OpenCode.Extensions.Data.Entities;

namespace Olbrasoft.OpenCode.Extensions.Data.EntityFrameworkCore.CommandHandlers;

/// <summary>
/// Handler for CreateSessionCommand that creates a new session in the database.
/// </summary>
public class CreateSessionCommandHandler : ICommandHandler<CreateSessionCommand, int>
{
    private readonly OpenCodeDbContext _context;

    /// <summary>
    /// Initializes a new instance of <see cref="CreateSessionCommandHandler"/>.
    /// </summary>
    /// <param name="context">The database context.</param>
    public CreateSessionCommandHandler(OpenCodeDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <inheritdoc />
    public async Task<int> HandleAsync(CreateSessionCommand command, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(command);
        token.ThrowIfCancellationRequested();

        var session = new Session
        {
            SessionId = command.SessionId,
            Title = command.Title,
            WorkingDirectory = command.WorkingDirectory,
            CreatedAt = command.CreatedAt
        };

        _context.Sessions.Add(session);
        await _context.SaveChangesAsync(token);

        // Return the auto-generated database ID
        return session.Id;
    }
}
