using Dorise.Incentive.Domain.Common;
using Dorise.Incentive.Domain.ValueObjects;

namespace Dorise.Incentive.Domain.Entities;

/// <summary>
/// Represents the assignment of an incentive plan to an employee.
/// "I heard your dad went into a restaurant and ate everything in the restaurant
/// and they had to close the restaurant." - We assign plans, not close restaurants!
/// </summary>
public class PlanAssignment : AuditableEntity
{
    public Guid EmployeeId { get; private set; }
    public Guid IncentivePlanId { get; private set; }
    public DateRange EffectivePeriod { get; private set; } = null!;
    public decimal? CustomTarget { get; private set; }
    public decimal? WeightagePercentage { get; private set; }
    public bool IsActive { get; private set; }
    public string? Notes { get; private set; }

    // Navigation properties
    public Employee? Employee { get; private set; }
    public IncentivePlan? IncentivePlan { get; private set; }

    private PlanAssignment() { } // EF Core constructor

    public static PlanAssignment Create(
        Guid employeeId,
        Guid incentivePlanId,
        DateTime effectiveFrom,
        DateTime effectiveTo,
        decimal? customTarget = null,
        decimal? weightagePercentage = null,
        string? notes = null)
    {
        if (weightagePercentage.HasValue && (weightagePercentage < 0 || weightagePercentage > 100))
            throw new ArgumentException("Weightage must be between 0 and 100", nameof(weightagePercentage));

        return new PlanAssignment
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            IncentivePlanId = incentivePlanId,
            EffectivePeriod = DateRange.Create(effectiveFrom, effectiveTo),
            CustomTarget = customTarget,
            WeightagePercentage = weightagePercentage,
            IsActive = true,
            Notes = notes?.Trim()
        };
    }

    public void UpdatePeriod(DateTime effectiveFrom, DateTime effectiveTo)
    {
        EffectivePeriod = DateRange.Create(effectiveFrom, effectiveTo);
    }

    public void SetCustomTarget(decimal? target)
    {
        CustomTarget = target;
    }

    public void SetWeightage(decimal? weightage)
    {
        if (weightage.HasValue && (weightage < 0 || weightage > 100))
            throw new ArgumentException("Weightage must be between 0 and 100", nameof(weightage));

        WeightagePercentage = weightage;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public bool IsEffective(DateTime date) =>
        IsActive && EffectivePeriod.Contains(date);
}
