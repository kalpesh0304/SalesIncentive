using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dorise.Incentive.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for SystemConfiguration entity.
/// "Me fail English? That's unpossible!" - Configuration never fails!
/// </summary>
public class ConfigurationRepository : RepositoryBase<SystemConfiguration>, IConfigurationRepository
{
    public ConfigurationRepository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<SystemConfiguration?> GetByKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(c => c.Key == key, cancellationToken);
    }

    public async Task<IReadOnlyList<SystemConfiguration>> GetByCategoryAsync(
        ConfigurationCategory category,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(c => c.Category == category)
            .OrderBy(c => c.Key)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<SystemConfiguration>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Where(c =>
                (!c.EffectiveFrom.HasValue || c.EffectiveFrom <= now) &&
                (!c.EffectiveTo.HasValue || c.EffectiveTo >= now))
            .OrderBy(c => c.Key)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(c => c.Key == key, cancellationToken);
    }

    public async Task<IReadOnlyList<SystemConfiguration>> SearchAsync(
        ConfigurationCategory? category,
        string? keyPrefix,
        bool? isEffective,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (category.HasValue)
        {
            query = query.Where(c => c.Category == category.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyPrefix))
        {
            query = query.Where(c => c.Key.StartsWith(keyPrefix));
        }

        if (isEffective.HasValue)
        {
            var now = DateTime.UtcNow;
            if (isEffective.Value)
            {
                query = query.Where(c =>
                    (!c.EffectiveFrom.HasValue || c.EffectiveFrom <= now) &&
                    (!c.EffectiveTo.HasValue || c.EffectiveTo >= now));
            }
            else
            {
                query = query.Where(c =>
                    (c.EffectiveFrom.HasValue && c.EffectiveFrom > now) ||
                    (c.EffectiveTo.HasValue && c.EffectiveTo < now));
            }
        }

        return await query
            .OrderBy(c => c.Key)
            .ToListAsync(cancellationToken);
    }

    public Task DeleteAsync(SystemConfiguration configuration, CancellationToken cancellationToken = default)
    {
        Remove(configuration);
        return Task.CompletedTask;
    }
}
