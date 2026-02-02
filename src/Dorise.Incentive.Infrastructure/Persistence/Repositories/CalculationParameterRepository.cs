using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dorise.Incentive.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for CalculationParameter entity.
/// "Me fail English? That's unpossible!" - Parameters never fail!
/// </summary>
public class CalculationParameterRepository : RepositoryBase<CalculationParameter>, ICalculationParameterRepository
{
    public CalculationParameterRepository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<CalculationParameter?> GetByNameAndScopeAsync(
        string parameterName,
        ParameterScope scope,
        Guid? scopeId = null,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(p =>
                p.ParameterName == parameterName &&
                p.Scope == scope &&
                p.ScopeId == scopeId,
                cancellationToken);
    }

    public async Task<CalculationParameter?> GetEffectiveParameterAsync(
        string parameterName,
        ParameterScope scope,
        Guid? scopeId,
        DateTime asOfDate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p =>
                p.ParameterName == parameterName &&
                p.Scope == scope &&
                p.ScopeId == scopeId &&
                p.EffectiveFrom <= asOfDate &&
                (p.EffectiveTo == null || p.EffectiveTo >= asOfDate))
            .OrderByDescending(p => p.EffectiveFrom)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CalculationParameter>> GetByScopeAsync(
        ParameterScope scope,
        Guid? scopeId = null,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p => p.Scope == scope && p.ScopeId == scopeId)
            .OrderBy(p => p.ParameterName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CalculationParameter>> SearchAsync(
        string? parameterName,
        ParameterScope? scope,
        Guid? scopeId,
        DateTime? effectiveFrom,
        DateTime? effectiveTo,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(parameterName))
        {
            query = query.Where(p => p.ParameterName.Contains(parameterName));
        }

        if (scope.HasValue)
        {
            query = query.Where(p => p.Scope == scope.Value);
        }

        if (scopeId.HasValue)
        {
            query = query.Where(p => p.ScopeId == scopeId.Value);
        }

        if (effectiveFrom.HasValue)
        {
            query = query.Where(p => p.EffectiveFrom >= effectiveFrom.Value);
        }

        if (effectiveTo.HasValue)
        {
            query = query.Where(p => p.EffectiveTo == null || p.EffectiveTo <= effectiveTo.Value);
        }

        return await query
            .OrderBy(p => p.ParameterName)
            .ThenByDescending(p => p.EffectiveFrom)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string parameterName,
        ParameterScope scope,
        Guid? scopeId = null,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(p =>
            p.ParameterName == parameterName &&
            p.Scope == scope &&
            p.ScopeId == scopeId,
            cancellationToken);
    }

    public async Task<IReadOnlyList<CalculationParameter>> GetEffectiveForScopeAsync(
        ParameterScope scope,
        Guid? scopeId,
        DateTime asOfDate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(p =>
                p.Scope == scope &&
                p.ScopeId == scopeId &&
                p.EffectiveFrom <= asOfDate &&
                (p.EffectiveTo == null || p.EffectiveTo >= asOfDate))
            .OrderBy(p => p.ParameterName)
            .ToListAsync(cancellationToken);
    }

    public Task DeleteAsync(CalculationParameter parameter, CancellationToken cancellationToken = default)
    {
        Remove(parameter);
        return Task.CompletedTask;
    }
}
