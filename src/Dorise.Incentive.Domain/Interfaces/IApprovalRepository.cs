using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Domain.Interfaces;

/// <summary>
/// Repository interface for Approval entities.
/// "I choo-choo-choose you!" - To approve this calculation!
/// </summary>
public interface IApprovalRepository : IAggregateRepository<Approval>
{
    /// <summary>
    /// Gets pending approvals for a specific approver.
    /// </summary>
    Task<IReadOnlyList<Approval>> GetPendingForApproverAsync(
        Guid approverId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all approvals for a calculation.
    /// </summary>
    Task<IReadOnlyList<Approval>> GetForCalculationAsync(
        Guid calculationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending approvals that have expired.
    /// </summary>
    Task<IReadOnlyList<Approval>> GetExpiredAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets pending approvals for escalation (past SLA).
    /// </summary>
    Task<IReadOnlyList<Approval>> GetForEscalationAsync(
        int slaHours,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated approvals for an approver with optional filtering.
    /// </summary>
    Task<(IReadOnlyList<Approval> Items, int TotalCount)> GetPagedForApproverAsync(
        Guid approverId,
        ApprovalStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all pending approvals with pagination (admin view).
    /// </summary>
    Task<(IReadOnlyList<Approval> Items, int TotalCount)> GetAllPendingPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets approval counts grouped by status for an approver.
    /// </summary>
    Task<Dictionary<ApprovalStatus, int>> GetStatusCountsForApproverAsync(
        Guid approverId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current pending approval for a calculation (if any).
    /// </summary>
    Task<Approval?> GetCurrentPendingAsync(
        Guid calculationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if approver has any pending approvals.
    /// </summary>
    Task<bool> HasPendingApprovalsAsync(
        Guid approverId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets approvals delegated to a specific user.
    /// </summary>
    Task<IReadOnlyList<Approval>> GetDelegatedToUserAsync(
        Guid delegatedToId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets approval history for an approver (all statuses).
    /// </summary>
    Task<IReadOnlyList<Approval>> GetHistoryForApproverAsync(
        Guid approverId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets approval statistics for a date range.
    /// </summary>
    Task<ApprovalStatistics> GetStatisticsAsync(
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistics for approvals.
/// </summary>
public class ApprovalStatistics
{
    public int TotalCount { get; init; }
    public int PendingCount { get; init; }
    public int ApprovedCount { get; init; }
    public int RejectedCount { get; init; }
    public int EscalatedCount { get; init; }
    public int ExpiredCount { get; init; }
    public decimal AverageApprovalTimeHours { get; init; }
    public IReadOnlyList<ApprovalLevelStatistics> ByLevel { get; init; } = Array.Empty<ApprovalLevelStatistics>();
}

/// <summary>
/// Statistics per approval level.
/// </summary>
public class ApprovalLevelStatistics
{
    public int Level { get; init; }
    public int Count { get; init; }
    public decimal AverageTimeHours { get; init; }
}
