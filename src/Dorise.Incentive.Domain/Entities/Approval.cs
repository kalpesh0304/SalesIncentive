using Dorise.Incentive.Domain.Common;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Events;

namespace Dorise.Incentive.Domain.Entities;

/// <summary>
/// Represents an approval request/response for a calculation.
/// "I sleep in a drawer!" - And approvals sleep in the workflow!
/// </summary>
public class Approval : AuditableEntity
{
    public Guid CalculationId { get; private set; }
    public Guid ApproverId { get; private set; }
    public int ApprovalLevel { get; private set; }
    public ApprovalStatus Status { get; private set; }
    public DateTime? ActionDate { get; private set; }
    public string? Comments { get; private set; }
    public Guid? DelegatedToId { get; private set; }
    public DateTime? DelegatedAt { get; private set; }
    public DateTime? ExpiresAt { get; private set; }

    // Navigation
    public Calculation? Calculation { get; private set; }
    public Employee? Approver { get; private set; }
    public Employee? DelegatedTo { get; private set; }

    private Approval() { } // EF Core constructor

    public static Approval Create(
        Guid calculationId,
        Guid approverId,
        int approvalLevel,
        DateTime? expiresAt = null)
    {
        if (approvalLevel < 1)
            throw new ArgumentException("Approval level must be at least 1", nameof(approvalLevel));

        var approval = new Approval
        {
            Id = Guid.NewGuid(),
            CalculationId = calculationId,
            ApproverId = approverId,
            ApprovalLevel = approvalLevel,
            Status = ApprovalStatus.Pending,
            ExpiresAt = expiresAt
        };

        return approval;
    }

    public void Approve(string? comments = null)
    {
        EnsurePending();

        Status = ApprovalStatus.Approved;
        ActionDate = DateTime.UtcNow;
        Comments = comments?.Trim();
    }

    public void Reject(string reason)
    {
        EnsurePending();

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Rejection reason is required", nameof(reason));

        Status = ApprovalStatus.Rejected;
        ActionDate = DateTime.UtcNow;
        Comments = reason.Trim();
    }

    public void Escalate(string reason)
    {
        EnsurePending();

        Status = ApprovalStatus.Escalated;
        ActionDate = DateTime.UtcNow;
        Comments = reason?.Trim();
    }

    public void Delegate(Guid delegateToId)
    {
        EnsurePending();

        if (delegateToId == ApproverId)
            throw new InvalidOperationException("Cannot delegate to self");

        Status = ApprovalStatus.Delegated;
        DelegatedToId = delegateToId;
        DelegatedAt = DateTime.UtcNow;
    }

    public void Cancel(string reason)
    {
        if (Status == ApprovalStatus.Approved || Status == ApprovalStatus.Rejected)
            throw new InvalidOperationException($"Cannot cancel approval with status {Status}");

        Status = ApprovalStatus.Cancelled;
        Comments = reason?.Trim();
    }

    public void MarkExpired()
    {
        if (Status != ApprovalStatus.Pending)
            throw new InvalidOperationException($"Cannot mark as expired when status is {Status}");

        if (!ExpiresAt.HasValue || DateTime.UtcNow < ExpiresAt.Value)
            throw new InvalidOperationException("Approval has not expired yet");

        Status = ApprovalStatus.Expired;
        ActionDate = DateTime.UtcNow;
    }

    public bool IsExpired => ExpiresAt.HasValue && DateTime.UtcNow > ExpiresAt.Value && Status == ApprovalStatus.Pending;

    public bool IsPending => Status == ApprovalStatus.Pending;

    public bool IsCompleted => Status == ApprovalStatus.Approved || Status == ApprovalStatus.Rejected;

    private void EnsurePending()
    {
        if (Status != ApprovalStatus.Pending)
            throw new InvalidOperationException($"Cannot perform action when status is {Status}");
    }
}
