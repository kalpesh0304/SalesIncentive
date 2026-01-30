using Dorise.Incentive.Domain.Common;

namespace Dorise.Incentive.Domain.Events;

/// <summary>
/// Event raised when a new incentive plan is created.
/// </summary>
public sealed class IncentivePlanCreatedEvent : DomainEvent
{
    public Guid PlanId { get; }
    public string PlanCode { get; }

    public IncentivePlanCreatedEvent(Guid planId, string planCode)
    {
        PlanId = planId;
        PlanCode = planCode;
    }
}

/// <summary>
/// Event raised when an incentive plan is activated.
/// </summary>
public sealed class IncentivePlanActivatedEvent : DomainEvent
{
    public Guid PlanId { get; }

    public IncentivePlanActivatedEvent(Guid planId)
    {
        PlanId = planId;
    }
}

/// <summary>
/// Event raised when an incentive plan is suspended.
/// </summary>
public sealed class IncentivePlanSuspendedEvent : DomainEvent
{
    public Guid PlanId { get; }
    public string Reason { get; }

    public IncentivePlanSuspendedEvent(Guid planId, string reason)
    {
        PlanId = planId;
        Reason = reason;
    }
}

/// <summary>
/// Event raised when an incentive plan is cancelled.
/// </summary>
public sealed class IncentivePlanCancelledEvent : DomainEvent
{
    public Guid PlanId { get; }
    public string Reason { get; }

    public IncentivePlanCancelledEvent(Guid planId, string reason)
    {
        PlanId = planId;
        Reason = reason;
    }
}
