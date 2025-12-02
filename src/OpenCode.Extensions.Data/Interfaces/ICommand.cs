namespace Olbrasoft.OpenCode.Extensions.Data.Interfaces;

/// <summary>
/// Marker interface for all commands.
/// </summary>
public interface ICommand
{
}

/// <summary>
/// Command that returns a result.
/// </summary>
/// <typeparam name="TResult">The type of the result returned by the command.</typeparam>
public interface ICommand<TResult> : ICommand
{
}
