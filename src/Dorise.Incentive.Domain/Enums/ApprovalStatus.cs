namespace Dorise.Incentive.Domain.Enums;

/// <summary>
/// Status of an approval request.
/// </summary>
public enum ApprovalStatus
{
    /// <summary>Awaiting approver action</summary>
    Pending = 1,

    /// <summary>Approved by this approver</summary>
    Approved = 2,

    /// <summary>Rejected by this approver</summary>
    Rejected = 3,

    /// <summary>Approval escalated to next level</summary>
    Escalated = 4,

    /// <summary>Approval request was cancelled</summary>
    Cancelled = 5,

    /// <summary>Approval expired due to timeout</summary>
    Expired = 6,

    /// <summary>Delegated to another approver</summary>
    Delegated = 7
}
