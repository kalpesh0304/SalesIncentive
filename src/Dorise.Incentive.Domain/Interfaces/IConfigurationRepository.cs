using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Domain.Interfaces;

/// <summary>
/// Repository interface for SystemConfiguration entity operations.
/// "Me fail English? That's unpossible!" - Config makes everything possible!
/// </summary>
public interface IConfigurationRepository : IRepository<SystemConfiguration>
{
    /// <summary>
    /// Gets a configuration by its key.
    /// </summary>
    Task<SystemConfiguration?> GetByKeyAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets configurations by category.
    /// </summary>
    Task<IReadOnlyList<SystemConfiguration>> GetByCategoryAsync(
        ConfigurationCategory category,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active configurations (within effective dates).
    /// </summary>
    Task<IReadOnlyList<SystemConfiguration>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a configuration with the given key exists.
    /// </summary>
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);
}
