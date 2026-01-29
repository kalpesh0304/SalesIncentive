using Dorise.Incentive.Domain.Common;
using System.Linq.Expressions;

namespace Dorise.Incentive.Domain.Interfaces;

/// <summary>
/// Generic repository interface for domain entities.
/// "Go banana!" - And go fetch those entities!
/// </summary>
/// <typeparam name="T">Entity type that inherits from BaseEntity</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Gets an entity by its unique identifier.
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all entities.
    /// </summary>
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds entities matching the specified predicate.
    /// </summary>
    Task<IReadOnlyList<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Finds a single entity matching the specified predicate.
    /// </summary>
    Task<T?> FindOneAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if any entity matches the specified predicate.
    /// </summary>
    Task<bool> ExistsAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts entities matching the specified predicate.
    /// </summary>
    Task<int> CountAsync(
        Expression<Func<T, bool>>? predicate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds a new entity.
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple entities.
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing entity.
    /// </summary>
    void Update(T entity);

    /// <summary>
    /// Removes an entity.
    /// </summary>
    void Remove(T entity);

    /// <summary>
    /// Removes multiple entities.
    /// </summary>
    void RemoveRange(IEnumerable<T> entities);
}

/// <summary>
/// Repository interface for aggregate roots with additional capabilities.
/// </summary>
/// <typeparam name="T">Aggregate root type</typeparam>
public interface IAggregateRepository<T> : IRepository<T> where T : BaseEntity, IAggregateRoot
{
    /// <summary>
    /// Gets an entity by ID with eager loading of related entities.
    /// </summary>
    Task<T?> GetByIdWithIncludesAsync(
        Guid id,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes);

    /// <summary>
    /// Finds entities with eager loading of related entities.
    /// </summary>
    Task<IReadOnlyList<T>> FindWithIncludesAsync(
        Expression<Func<T, bool>> predicate,
        CancellationToken cancellationToken = default,
        params Expression<Func<T, object>>[] includes);
}
