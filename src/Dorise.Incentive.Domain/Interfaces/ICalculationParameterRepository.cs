using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Domain.Interfaces;

/// <summary>
/// Repository interface for CalculationParameter entity operations.
/// "The doctor said I wouldn't have so many nosebleeds if I kept my finger outta there."
/// - Calculation parameters keep the numbers in check!
/// </summary>
public interface ICalculationParameterRepository : IRepository<CalculationParameter>
{
    /// <summary>
    /// Gets a parameter by name and scope.
    /// </summary>
    Task<CalculationParameter?> GetByNameAndScopeAsync(
        string parameterName,
        ParameterScope scope,
        Guid? scopeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the effective parameter at a given date.
    /// </summary>
    Task<CalculationParameter?> GetEffectiveParameterAsync(
        string parameterName,
        ParameterScope scope,
        Guid? scopeId,
        DateTime asOfDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all parameters by scope.
    /// </summary>
    Task<IReadOnlyList<CalculationParameter>> GetByScopeAsync(
        ParameterScope scope,
        Guid? scopeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches parameters with filters.
    /// </summary>
    Task<IReadOnlyList<CalculationParameter>> SearchAsync(
        string? parameterName,
        ParameterScope? scope,
        Guid? scopeId,
        DateTime? effectiveFrom,
        DateTime? effectiveTo,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a parameter with the given name and scope exists.
    /// </summary>
    Task<bool> ExistsAsync(
        string parameterName,
        ParameterScope scope,
        Guid? scopeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all effective parameters for a scope at a given date.
    /// </summary>
    Task<IReadOnlyList<CalculationParameter>> GetEffectiveForScopeAsync(
        ParameterScope scope,
        Guid? scopeId,
        DateTime asOfDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a calculation parameter.
    /// </summary>
    Task DeleteAsync(CalculationParameter parameter, CancellationToken cancellationToken = default);
}
