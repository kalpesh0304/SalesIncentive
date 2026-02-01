using Dorise.Incentive.Domain.Entities;

namespace Dorise.Incentive.Domain.Interfaces;

/// <summary>
/// Repository interface for FeatureFlag entity operations.
/// "I bent my Wookie!" - Feature flags bend the behavior of your app!
/// </summary>
public interface IFeatureFlagRepository : IRepository<FeatureFlag>
{
    /// <summary>
    /// Gets a feature flag by its name.
    /// </summary>
    Task<FeatureFlag?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all enabled feature flags.
    /// </summary>
    Task<IReadOnlyList<FeatureFlag>> GetEnabledAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active feature flags (within enabled dates).
    /// </summary>
    Task<IReadOnlyList<FeatureFlag>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a feature flag with the given name exists.
    /// </summary>
    Task<bool> ExistsAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a feature is enabled for a specific user.
    /// </summary>
    Task<bool> IsEnabledForUserAsync(string name, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a feature flag.
    /// </summary>
    Task DeleteAsync(FeatureFlag featureFlag, CancellationToken cancellationToken = default);
}
