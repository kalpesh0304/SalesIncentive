using Dorise.Incentive.Application.Approvals.DTOs;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Application.Approvals.Queries;

/// <summary>
/// Query to get pending approvals for a user.
/// "I'm Idaho!" - And these are my pending approvals!
/// </summary>
public record GetPendingApprovalsForUserQuery(
    Guid ApproverId,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedApprovalResult>;

/// <summary>
/// Query to get approval history for a user.
/// </summary>
public record GetApprovalHistoryQuery(
    Guid ApproverId,
    ApprovalStatus? Status = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedApprovalResult>;

/// <summary>
/// Query to get approval dashboard data.
/// </summary>
public record GetApprovalDashboardQuery(
    Guid ApproverId) : IQuery<ApprovalDashboardDto>;

/// <summary>
/// Query to get approval by ID.
/// </summary>
public record GetApprovalByIdQuery(
    Guid ApprovalId) : IQuery<ApprovalDetailDto?>;

/// <summary>
/// Query to get approvals for a specific calculation.
/// </summary>
public record GetApprovalsForCalculationQuery(
    Guid CalculationId) : IQuery<IReadOnlyList<ApprovalDetailDto>>;

/// <summary>
/// Query to get all pending approvals (admin view).
/// </summary>
public record GetAllPendingApprovalsQuery(
    int Page = 1,
    int PageSize = 20) : IQuery<PagedApprovalResult>;

/// <summary>
/// Query to get overdue approvals (for monitoring).
/// </summary>
public record GetOverdueApprovalsQuery(
    int Page = 1,
    int PageSize = 50) : IQuery<PagedApprovalResult>;

/// <summary>
/// Query to get approval statistics for reporting.
/// </summary>
public record GetApprovalStatisticsQuery(
    DateTime? StartDate = null,
    DateTime? EndDate = null) : IQuery<ApprovalStatisticsDto>;

/// <summary>
/// DTO for approval statistics.
/// </summary>
public record ApprovalStatisticsDto
{
    public int TotalApprovals { get; init; }
    public int PendingCount { get; init; }
    public int ApprovedCount { get; init; }
    public int RejectedCount { get; init; }
    public int EscalatedCount { get; init; }
    public int ExpiredCount { get; init; }
    public decimal AverageApprovalTimeHours { get; init; }
    public IReadOnlyList<ApprovalsByLevelDto> ByLevel { get; init; } = Array.Empty<ApprovalsByLevelDto>();
}

/// <summary>
/// Approvals breakdown by level.
/// </summary>
public record ApprovalsByLevelDto
{
    public int Level { get; init; }
    public string LevelName { get; init; } = null!;
    public int Count { get; init; }
    public decimal AverageTimeHours { get; init; }
}
