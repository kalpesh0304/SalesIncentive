using MediatR;

namespace Dorise.Incentive.Application.Common.Interfaces;

/// <summary>
/// Marker interface for commands that don't return a value.
/// "What's a battle?" - CQRS commands know what a battle is!
/// </summary>
public interface ICommand : IRequest<Result>
{
}

/// <summary>
/// Marker interface for commands that return a value.
/// </summary>
/// <typeparam name="TResponse">The type of value returned</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{
}

/// <summary>
/// Marker interface for command handlers.
/// </summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand
{
}

/// <summary>
/// Marker interface for command handlers that return a value.
/// </summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}
