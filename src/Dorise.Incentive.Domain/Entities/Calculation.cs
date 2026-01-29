using Dorise.Incentive.Domain.Common;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Events;
using Dorise.Incentive.Domain.ValueObjects;

namespace Dorise.Incentive.Domain.Entities;

/// <summary>
/// Represents an incentive calculation for an employee in a period.
/// "The doctor said I wouldn't have so many nosebleeds if I kept my finger outta there."
/// - Let's calculate those incentives properly!
/// </summary>
public class Calculation : AuditableEntity, IAggregateRoot
{
    public Guid EmployeeId { get; private set; }
    public Guid IncentivePlanId { get; private set; }
    public Guid? PlanAssignmentId { get; private set; }
    public DateRange CalculationPeriod { get; private set; } = null!;
    public CalculationStatus Status { get; private set; }

    // Target and Achievement
    public decimal TargetValue { get; private set; }
    public decimal ActualValue { get; private set; }
    public Percentage AchievementPercentage { get; private set; } = null!;

    // Payout details
    public Money BaseSalary { get; private set; } = null!;
    public Money GrossIncentive { get; private set; } = null!;
    public Money NetIncentive { get; private set; } = null!;
    public Percentage? ProrataFactor { get; private set; }
    public Guid? AppliedSlabId { get; private set; }

    // Audit trail
    public DateTime CalculatedAt { get; private set; }
    public string? CalculatedBy { get; private set; }
    public string? RejectionReason { get; private set; }
    public string? AdjustmentReason { get; private set; }
    public int Version { get; private set; }
    public Guid? PreviousVersionId { get; private set; }

    // Navigation properties
    public Employee? Employee { get; private set; }
    public IncentivePlan? IncentivePlan { get; private set; }
    public Slab? AppliedSlab { get; private set; }

    private readonly List<Approval> _approvals = new();
    public IReadOnlyCollection<Approval> Approvals => _approvals.AsReadOnly();

    private Calculation() { } // EF Core constructor

    public static Calculation Create(
        Guid employeeId,
        Guid incentivePlanId,
        DateRange period,
        decimal targetValue,
        decimal actualValue,
        Money baseSalary,
        Guid? planAssignmentId = null)
    {
        var achievement = Percentage.Calculate(actualValue, targetValue);

        var calculation = new Calculation
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            IncentivePlanId = incentivePlanId,
            PlanAssignmentId = planAssignmentId,
            CalculationPeriod = period,
            Status = CalculationStatus.Pending,
            TargetValue = targetValue,
            ActualValue = actualValue,
            AchievementPercentage = achievement,
            BaseSalary = baseSalary,
            GrossIncentive = Money.Zero(baseSalary.Currency),
            NetIncentive = Money.Zero(baseSalary.Currency),
            CalculatedAt = DateTime.UtcNow,
            Version = 1
        };

        return calculation;
    }

    public void Calculate(Money grossIncentive, Guid? slabId = null)
    {
        if (Status != CalculationStatus.Pending)
            throw new InvalidOperationException($"Cannot calculate when status is {Status}");

        GrossIncentive = grossIncentive;
        NetIncentive = grossIncentive;
        AppliedSlabId = slabId;
        Status = CalculationStatus.Calculated;
        CalculatedAt = DateTime.UtcNow;

        AddDomainEvent(new CalculationCompletedEvent(Id, EmployeeId, NetIncentive.Amount));
    }

    public void ApplyProrata(Percentage factor)
    {
        if (factor.Value <= 0 || factor.Value > 100)
            throw new ArgumentException("Prorata factor must be between 0 and 100", nameof(factor));

        ProrataFactor = factor;
        NetIncentive = GrossIncentive.Multiply(factor);
        Status = CalculationStatus.Prorated;
    }

    public void ApplyCap(Money maxPayout)
    {
        if (NetIncentive > maxPayout)
        {
            NetIncentive = maxPayout;
            Status = CalculationStatus.Capped;
        }
    }

    public void MarkBelowThreshold()
    {
        Status = CalculationStatus.BelowThreshold;
        GrossIncentive = Money.Zero(BaseSalary.Currency);
        NetIncentive = Money.Zero(BaseSalary.Currency);
    }

    public void MarkIneligible(string reason)
    {
        Status = CalculationStatus.Ineligible;
        GrossIncentive = Money.Zero(BaseSalary.Currency);
        NetIncentive = Money.Zero(BaseSalary.Currency);
        AdjustmentReason = reason;
    }

    public void SubmitForApproval(string submittedBy)
    {
        var validStatuses = new[]
        {
            CalculationStatus.Calculated,
            CalculationStatus.Prorated,
            CalculationStatus.Capped
        };

        if (!validStatuses.Contains(Status))
            throw new InvalidOperationException($"Cannot submit for approval when status is {Status}");

        Status = CalculationStatus.PendingApproval;
        CalculatedBy = submittedBy;

        AddDomainEvent(new CalculationSubmittedForApprovalEvent(Id, EmployeeId));
    }

    public void Approve(string approvedBy)
    {
        if (Status != CalculationStatus.PendingApproval)
            throw new InvalidOperationException($"Cannot approve when status is {Status}");

        Status = CalculationStatus.Approved;
        AddDomainEvent(new CalculationApprovedEvent(Id, EmployeeId, approvedBy));
    }

    public void Reject(string rejectedBy, string reason)
    {
        if (Status != CalculationStatus.PendingApproval)
            throw new InvalidOperationException($"Cannot reject when status is {Status}");

        Status = CalculationStatus.Rejected;
        RejectionReason = reason;
        AddDomainEvent(new CalculationRejectedEvent(Id, EmployeeId, rejectedBy, reason));
    }

    public void MarkPaid(string processedBy)
    {
        if (Status != CalculationStatus.Approved)
            throw new InvalidOperationException($"Cannot mark as paid when status is {Status}");

        Status = CalculationStatus.Paid;
        AddDomainEvent(new CalculationPaidEvent(Id, EmployeeId, NetIncentive.Amount));
    }

    public void Void(string voidedBy, string reason)
    {
        Status = CalculationStatus.Voided;
        AdjustmentReason = reason;
        AddDomainEvent(new CalculationVoidedEvent(Id, EmployeeId, voidedBy, reason));
    }

    public Calculation CreateAdjustment(decimal newActualValue, string reason)
    {
        var adjusted = Create(
            EmployeeId,
            IncentivePlanId,
            CalculationPeriod,
            TargetValue,
            newActualValue,
            BaseSalary,
            PlanAssignmentId);

        adjusted.Version = Version + 1;
        adjusted.PreviousVersionId = Id;
        adjusted.AdjustmentReason = reason;
        adjusted.Status = CalculationStatus.Adjusted;

        return adjusted;
    }

    internal void AddApproval(Approval approval)
    {
        _approvals.Add(approval);
    }
}
