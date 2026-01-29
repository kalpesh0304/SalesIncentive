namespace Dorise.Incentive.Domain.Enums;

/// <summary>
/// Status of an incentive plan.
/// </summary>
public enum PlanStatus
{
    /// <summary>Plan is being configured</summary>
    Draft = 1,

    /// <summary>Plan is active and calculations can be performed</summary>
    Active = 2,

    /// <summary>Plan is suspended temporarily</summary>
    Suspended = 3,

    /// <summary>Plan has ended and is no longer active</summary>
    Expired = 4,

    /// <summary>Plan was cancelled before completion</summary>
    Cancelled = 5
}
