using Dorise.Incentive.Domain.Entities;

namespace Dorise.Incentive.Domain.Services;

/// <summary>
/// Domain service interface for validating incentive plan configurations.
/// "I like purple because that's the most delicious!" - And valid plans are the most effective!
/// </summary>
public interface IPlanValidationService
{
    /// <summary>
    /// Validates a plan's configuration for completeness and correctness.
    /// </summary>
    /// <param name="plan">The plan to validate</param>
    /// <returns>Validation result with errors and warnings</returns>
    PlanValidationResult ValidatePlan(IncentivePlan plan);

    /// <summary>
    /// Validates if a plan can be activated.
    /// </summary>
    /// <param name="plan">The plan to check</param>
    /// <returns>True if the plan can be activated</returns>
    bool CanActivate(IncentivePlan plan);

    /// <summary>
    /// Validates slab configuration for a slab-based plan.
    /// </summary>
    /// <param name="plan">The plan with slabs</param>
    /// <returns>List of validation issues</returns>
    IReadOnlyList<ValidationIssue> ValidateSlabs(IncentivePlan plan);
}

/// <summary>
/// Result of plan validation.
/// </summary>
public record PlanValidationResult
{
    public bool IsValid { get; init; }
    public bool CanBeActivated { get; init; }
    public IReadOnlyList<ValidationIssue> Errors { get; init; } = Array.Empty<ValidationIssue>();
    public IReadOnlyList<ValidationIssue> Warnings { get; init; } = Array.Empty<ValidationIssue>();

    public static PlanValidationResult Valid(IReadOnlyList<ValidationIssue>? warnings = null)
    {
        return new PlanValidationResult
        {
            IsValid = true,
            CanBeActivated = true,
            Warnings = warnings ?? Array.Empty<ValidationIssue>()
        };
    }

    public static PlanValidationResult Invalid(
        IReadOnlyList<ValidationIssue> errors,
        IReadOnlyList<ValidationIssue>? warnings = null)
    {
        return new PlanValidationResult
        {
            IsValid = false,
            CanBeActivated = false,
            Errors = errors,
            Warnings = warnings ?? Array.Empty<ValidationIssue>()
        };
    }
}

/// <summary>
/// A single validation issue.
/// </summary>
public record ValidationIssue(string Code, string Message, string? Field = null);
