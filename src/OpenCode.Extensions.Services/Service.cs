namespace OpenCode.Extensions.Services;

/// <summary>
/// Base service class that provides access to the mediator.
/// All services should inherit from this class.
/// </summary>
/// <param name="mediator">The mediator instance for dispatching queries and commands.</param>
public abstract class Service(IMediator mediator)
{
    /// <summary>
    /// Gets the mediator instance.
    /// </summary>
    protected IMediator Mediator { get; } = mediator ?? throw new ArgumentNullException(nameof(mediator));
}
