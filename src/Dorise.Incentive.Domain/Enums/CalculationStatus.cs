namespace Dorise.Incentive.Domain.Enums;

/// <summary>
/// Status of an incentive calculation.
/// </summary>
public enum CalculationStatus
{
    /// <summary>Calculation is in progress</summary>
    Pending = 1,

    /// <summary>Calculation completed successfully</summary>
    Calculated = 2,

    /// <summary>Below minimum threshold - no incentive</summary>
    BelowThreshold = 3,

    /// <summary>Calculation was prorated (partial period)</summary>
    Prorated = 4,

    /// <summary>Incentive amount was capped at maximum</summary>
    Capped = 5,

    /// <summary>Submitted for approval</summary>
    PendingApproval = 6,

    /// <summary>Approved by all required approvers</summary>
    Approved = 7,

    /// <summary>Rejected during approval</summary>
    Rejected = 8,

    /// <summary>Paid out to employee</summary>
    Paid = 9,

    /// <summary>Calculation was voided/cancelled</summary>
    Voided = 10,

    /// <summary>Calculation adjusted after initial approval</summary>
    Adjusted = 11,

    /// <summary>Employee on leave - calculation deferred</summary>
    Deferred = 12,

    /// <summary>Employee not eligible for this period</summary>
    Ineligible = 13
}
