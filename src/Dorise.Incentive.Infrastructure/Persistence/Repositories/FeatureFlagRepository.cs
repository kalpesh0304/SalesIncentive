using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dorise.Incentive.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for FeatureFlag entity.
/// "I bent my Wookie!" - Feature flags bend the behavior of your app!
/// </summary>
public class FeatureFlagRepository : RepositoryBase<FeatureFlag>, IFeatureFlagRepository
{
    public FeatureFlagRepository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<FeatureFlag?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(f => f.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<FeatureFlag>> GetEnabledAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(f => f.IsEnabled)
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FeatureFlag>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(f =>
                f.IsEnabled &&
                (!f.EnabledFrom.HasValue || f.EnabledFrom <= now) &&
                (!f.EnabledUntil.HasValue || f.EnabledUntil >= now))
            .OrderBy(f => f.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(f => f.Name == name, cancellationToken);
    }

    public async Task<bool> IsEnabledForUserAsync(
        string name,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var flag = await GetByNameAsync(name, cancellationToken);
        return flag?.IsEnabledForUser(userId) ?? false;
    }

    public Task DeleteAsync(FeatureFlag featureFlag, CancellationToken cancellationToken = default)
    {
        Remove(featureFlag);
        return Task.CompletedTask;
    }
}
