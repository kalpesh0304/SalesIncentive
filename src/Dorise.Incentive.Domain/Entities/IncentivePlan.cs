using Dorise.Incentive.Domain.Common;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Events;
using Dorise.Incentive.Domain.ValueObjects;

namespace Dorise.Incentive.Domain.Entities;

/// <summary>
/// Represents an incentive plan configuration.
/// "Mrs. Krabappel and Principal Skinner were in the closet making babies
/// and I saw one of the babies and the baby looked at me!" - That's how incentive plans are born!
/// </summary>
public class IncentivePlan : AuditableEntity, IAggregateRoot
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public PlanType PlanType { get; private set; }
    public PlanStatus Status { get; private set; }
    public PaymentFrequency Frequency { get; private set; }
    public DateRange EffectivePeriod { get; private set; } = null!;
    public Target Target { get; private set; } = null!;
    public Money? MaximumPayout { get; private set; }
    public Money? MinimumPayout { get; private set; }
    public bool RequiresApproval { get; private set; }
    public int ApprovalLevels { get; private set; }
    public string? EligibilityCriteria { get; private set; }
    public int Version { get; private set; }

    // Navigation properties
    private readonly List<Slab> _slabs = new();
    public IReadOnlyCollection<Slab> Slabs => _slabs.AsReadOnly();

    private readonly List<PlanAssignment> _assignments = new();
    public IReadOnlyCollection<PlanAssignment> Assignments => _assignments.AsReadOnly();

    private IncentivePlan() { } // EF Core constructor

    public static IncentivePlan Create(
        string code,
        string name,
        PlanType planType,
        PaymentFrequency frequency,
        DateTime effectiveFrom,
        DateTime effectiveTo,
        decimal targetValue,
        decimal minimumThreshold,
        AchievementType achievementType,
        string? description = null,
        string? metricUnit = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Plan code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Plan name is required", nameof(name));

        var plan = new IncentivePlan
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            PlanType = planType,
            Status = PlanStatus.Draft,
            Frequency = frequency,
            EffectivePeriod = DateRange.Create(effectiveFrom, effectiveTo),
            Target = Target.Create(targetValue, minimumThreshold, achievementType, metricUnit),
            RequiresApproval = true,
            ApprovalLevels = 1,
            Version = 1
        };

        plan.AddDomainEvent(new IncentivePlanCreatedEvent(plan.Id, plan.Code));

        return plan;
    }

    public void UpdateDetails(string name, string? description, string? eligibilityCriteria)
    {
        EnsureModifiable();
        Name = name.Trim();
        Description = description?.Trim();
        EligibilityCriteria = eligibilityCriteria?.Trim();
    }

    public void SetPayoutLimits(decimal? maximumPayout, decimal? minimumPayout, string currency = "INR")
    {
        EnsureModifiable();

        if (maximumPayout.HasValue && minimumPayout.HasValue && maximumPayout < minimumPayout)
            throw new ArgumentException("Maximum payout cannot be less than minimum payout");

        MaximumPayout = maximumPayout.HasValue ? Money.Create(maximumPayout.Value, currency) : null;
        MinimumPayout = minimumPayout.HasValue ? Money.Create(minimumPayout.Value, currency) : null;
    }

    public void ConfigureApproval(bool requiresApproval, int approvalLevels = 1)
    {
        EnsureModifiable();

        if (requiresApproval && approvalLevels < 1)
            throw new ArgumentException("At least one approval level is required", nameof(approvalLevels));

        RequiresApproval = requiresApproval;
        ApprovalLevels = requiresApproval ? approvalLevels : 0;
    }

    public void AddSlab(decimal fromPercentage, decimal toPercentage, decimal payoutRate)
    {
        EnsureModifiable();

        if (PlanType != PlanType.SlabBased)
            throw new InvalidOperationException("Slabs can only be added to slab-based plans");

        // Validate no overlap with existing slabs
        var overlapping = _slabs.Any(s =>
            (fromPercentage >= s.FromPercentage && fromPercentage <= s.ToPercentage) ||
            (toPercentage >= s.FromPercentage && toPercentage <= s.ToPercentage));

        if (overlapping)
            throw new InvalidOperationException("Slab ranges cannot overlap");

        var order = _slabs.Count + 1;
        var slab = Slab.Create(Id, fromPercentage, toPercentage, payoutRate, order);
        _slabs.Add(slab);
    }

    public void RemoveSlab(Guid slabId)
    {
        EnsureModifiable();
        var slab = _slabs.FirstOrDefault(s => s.Id == slabId);
        if (slab != null)
        {
            _slabs.Remove(slab);
            ReorderSlabs();
        }
    }

    public void Activate()
    {
        if (Status != PlanStatus.Draft)
            throw new InvalidOperationException($"Cannot activate plan with status {Status}");

        if (PlanType == PlanType.SlabBased && !_slabs.Any())
            throw new InvalidOperationException("Slab-based plan must have at least one slab defined");

        Status = PlanStatus.Active;
        AddDomainEvent(new IncentivePlanActivatedEvent(Id));
    }

    public void Suspend(string reason)
    {
        if (Status != PlanStatus.Active)
            throw new InvalidOperationException($"Cannot suspend plan with status {Status}");

        Status = PlanStatus.Suspended;
        AddDomainEvent(new IncentivePlanSuspendedEvent(Id, reason));
    }

    public void Cancel(string reason)
    {
        if (Status == PlanStatus.Cancelled)
            throw new InvalidOperationException("Plan is already cancelled");

        Status = PlanStatus.Cancelled;
        AddDomainEvent(new IncentivePlanCancelledEvent(Id, reason));
    }

    public bool IsEffective(DateTime date) =>
        Status == PlanStatus.Active && EffectivePeriod.Contains(date);

    /// <summary>
    /// Gets whether the plan is currently active.
    /// </summary>
    public bool IsActive => Status == PlanStatus.Active;

    public Slab? GetApplicableSlab(Percentage achievement)
    {
        return _slabs
            .OrderBy(s => s.Order)
            .FirstOrDefault(s => s.IsInRange(achievement));
    }

    /// <summary>
    /// Updates the plan with new values. Only allowed for Draft plans.
    /// </summary>
    public void Update(
        string name,
        string? description,
        DateRange effectivePeriod,
        Target target,
        Money? maximumPayout,
        Money? minimumPayout,
        bool requiresApproval,
        int approvalLevels,
        int? minimumTenureDays,
        string? eligibilityCriteria)
    {
        EnsureModifiable();

        Name = name.Trim();
        Description = description?.Trim();
        EffectivePeriod = effectivePeriod;
        Target = target;
        MaximumPayout = maximumPayout;
        MinimumPayout = minimumPayout;
        RequiresApproval = requiresApproval;
        ApprovalLevels = requiresApproval ? approvalLevels : 0;
        EligibilityCriteria = eligibilityCriteria?.Trim();
    }

    /// <summary>
    /// Creates a copy of this plan with a new code and name.
    /// </summary>
    public IncentivePlan Clone(string newCode, string newName)
    {
        var cloned = Create(
            newCode,
            newName,
            PlanType,
            Frequency,
            EffectivePeriod.StartDate,
            EffectivePeriod.EndDate,
            Target.TargetValue,
            Target.MinimumThreshold,
            Target.AchievementType,
            Description,
            Target.MetricUnit);

        cloned.MaximumPayout = MaximumPayout;
        cloned.MinimumPayout = MinimumPayout;
        cloned.RequiresApproval = RequiresApproval;
        cloned.ApprovalLevels = ApprovalLevels;
        cloned.EligibilityCriteria = EligibilityCriteria;

        // Clone slabs
        foreach (var slab in _slabs.OrderBy(s => s.Order))
        {
            cloned.AddSlab(slab.FromPercentage, slab.ToPercentage, slab.PayoutRate);
        }

        return cloned;
    }

    private void EnsureModifiable()
    {
        if (Status != PlanStatus.Draft)
            throw new InvalidOperationException($"Cannot modify plan with status {Status}");
    }

    private void ReorderSlabs()
    {
        var order = 1;
        foreach (var slab in _slabs.OrderBy(s => s.FromPercentage))
        {
            slab.SetOrder(order++);
        }
    }
}
