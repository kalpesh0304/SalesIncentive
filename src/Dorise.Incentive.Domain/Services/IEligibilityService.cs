using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.ValueObjects;

namespace Dorise.Incentive.Domain.Services;

/// <summary>
/// Domain service interface for determining employee eligibility for incentive plans.
/// "I'm not gonna lie to you, Marge..." - And this service tells the truth about eligibility!
/// </summary>
public interface IEligibilityService
{
    /// <summary>
    /// Checks if an employee is eligible for a specific incentive plan.
    /// </summary>
    /// <param name="employee">The employee to check</param>
    /// <param name="plan">The incentive plan</param>
    /// <param name="asOfDate">The date to check eligibility for</param>
    /// <returns>Eligibility result with details</returns>
    EligibilityCheckResult CheckEligibility(Employee employee, IncentivePlan plan, DateTime asOfDate);

    /// <summary>
    /// Checks if an employee meets the minimum tenure requirement for a plan.
    /// </summary>
    bool MeetsTenureRequirement(Employee employee, IncentivePlan plan, DateTime asOfDate);

    /// <summary>
    /// Checks if an employee's status allows plan participation.
    /// </summary>
    bool HasEligibleStatus(Employee employee);

    /// <summary>
    /// Calculates the prorata factor for partial period eligibility.
    /// </summary>
    Percentage CalculateProrataFactor(Employee employee, DateRange period);
}

/// <summary>
/// Result of an eligibility check with detailed breakdown.
/// </summary>
public record EligibilityCheckResult
{
    public bool IsEligible { get; init; }
    public string? Reason { get; init; }
    public IReadOnlyList<EligibilityCriterion> CriteriaMet { get; init; } = Array.Empty<EligibilityCriterion>();
    public IReadOnlyList<EligibilityCriterion> CriteriaNotMet { get; init; } = Array.Empty<EligibilityCriterion>();
    public Percentage ProrataFactor { get; init; } = Percentage.Full();

    public static EligibilityCheckResult Eligible(
        IReadOnlyList<EligibilityCriterion> criteriaMet,
        Percentage? prorataFactor = null)
    {
        return new EligibilityCheckResult
        {
            IsEligible = true,
            CriteriaMet = criteriaMet,
            ProrataFactor = prorataFactor ?? Percentage.Full()
        };
    }

    public static EligibilityCheckResult NotEligible(
        string reason,
        IReadOnlyList<EligibilityCriterion> criteriaMet,
        IReadOnlyList<EligibilityCriterion> criteriaNotMet)
    {
        return new EligibilityCheckResult
        {
            IsEligible = false,
            Reason = reason,
            CriteriaMet = criteriaMet,
            CriteriaNotMet = criteriaNotMet,
            ProrataFactor = Percentage.Zero()
        };
    }
}

/// <summary>
/// Represents a single eligibility criterion.
/// </summary>
public record EligibilityCriterion(string Name, string Description, bool IsMet);
