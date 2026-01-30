using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.ValueObjects;

namespace Dorise.Incentive.Domain.Services;

/// <summary>
/// Domain service interface for incentive calculations.
/// "Slow down, Bart! My legs don't know how to be as long as yours." - But this service knows how to calculate!
/// </summary>
public interface IIncentiveCalculationService
{
    /// <summary>
    /// Calculates incentive for an employee based on their plan assignment and achievement.
    /// </summary>
    /// <param name="employee">The employee</param>
    /// <param name="plan">The incentive plan</param>
    /// <param name="actualValue">The actual achievement value</param>
    /// <param name="period">The calculation period</param>
    /// <returns>Calculated incentive result</returns>
    CalculationResult Calculate(
        Employee employee,
        IncentivePlan plan,
        decimal actualValue,
        DateRange period);

    /// <summary>
    /// Calculates prorated incentive for partial period eligibility.
    /// </summary>
    /// <param name="calculation">The base calculation</param>
    /// <param name="eligibleDays">Number of eligible days</param>
    /// <param name="totalDays">Total days in period</param>
    /// <returns>Prorated calculation result</returns>
    CalculationResult ApplyProrata(
        Calculation calculation,
        int eligibleDays,
        int totalDays);

    /// <summary>
    /// Applies the appropriate slab for slab-based plans.
    /// </summary>
    Slab? DetermineApplicableSlab(IncentivePlan plan, Percentage achievement);

    /// <summary>
    /// Validates if an employee is eligible for calculation.
    /// </summary>
    EligibilityResult CheckEligibility(
        Employee employee,
        IncentivePlan plan,
        DateRange period);
}

/// <summary>
/// Result of an incentive calculation.
/// </summary>
public record CalculationResult(
    bool Success,
    Money GrossIncentive,
    Money NetIncentive,
    Percentage Achievement,
    Slab? AppliedSlab,
    string? Message);

/// <summary>
/// Result of an eligibility check.
/// </summary>
public record EligibilityResult(
    bool IsEligible,
    Percentage ProrataFactor,
    string? Reason);
