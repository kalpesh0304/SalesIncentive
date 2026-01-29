using MediatR;

namespace Dorise.Incentive.Application.Common.Interfaces;

/// <summary>
/// Marker interface for queries.
/// "I'm a big boy now!" - And queries are big CQRS patterns!
/// </summary>
/// <typeparam name="TResponse">The type of value returned</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{
}

/// <summary>
/// Marker interface for query handlers.
/// </summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>
{
}
