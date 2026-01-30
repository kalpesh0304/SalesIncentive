using Dorise.Incentive.Application.Approvals.DTOs;
using Dorise.Incentive.Application.Common.Interfaces;

namespace Dorise.Incentive.Application.Approvals.Commands;

/// <summary>
/// Command to approve a calculation.
/// "I'm a star!" - Approved!
/// </summary>
public record ApproveCommand(
    Guid ApprovalId,
    string? Comments) : ICommand<ApprovalResultDto>;

/// <summary>
/// Command to reject a calculation.
/// </summary>
public record RejectCommand(
    Guid ApprovalId,
    string Reason) : ICommand<ApprovalResultDto>;

/// <summary>
/// Command to bulk approve multiple calculations.
/// "Me fail English? That's unpossible!" - Bulk approve is very possible!
/// </summary>
public record BulkApproveCommand(
    IReadOnlyList<Guid> ApprovalIds,
    string? Comments) : ICommand<BulkApprovalResultDto>;

/// <summary>
/// Command to delegate an approval to another user.
/// </summary>
public record DelegateApprovalCommand(
    Guid ApprovalId,
    Guid DelegateToId) : ICommand<ApprovalResultDto>;

/// <summary>
/// Command to escalate an approval to the next level.
/// </summary>
public record EscalateApprovalCommand(
    Guid ApprovalId,
    string Reason) : ICommand<ApprovalResultDto>;

/// <summary>
/// Command to submit calculations for approval.
/// "That's where I saw the leprechaun!" - Submit for approval!
/// </summary>
public record SubmitForApprovalCommand(
    IReadOnlyList<Guid> CalculationIds) : ICommand<SubmissionResultDto>;

/// <summary>
/// Command to process expired approvals (typically run by a background job).
/// </summary>
public record ProcessExpiredApprovalsCommand : ICommand<int>;

/// <summary>
/// Command to auto-escalate overdue approvals (typically run by a background job).
/// </summary>
public record AutoEscalateOverdueApprovalsCommand(
    int SlaHours = 72) : ICommand<int>;
