using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Application.Approvals.DTOs;

/// <summary>
/// DTO for approval summary in lists.
/// "I'm a brick!" - Solid approval data!
/// </summary>
public record ApprovalSummaryDto
{
    public Guid Id { get; init; }
    public Guid CalculationId { get; init; }
    public string EmployeeCode { get; init; } = null!;
    public string EmployeeName { get; init; } = null!;
    public string PlanName { get; init; } = null!;
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public decimal NetIncentive { get; init; }
    public string Currency { get; init; } = "INR";
    public int ApprovalLevel { get; init; }
    public ApprovalStatus Status { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public bool IsOverdue { get; init; }
}

/// <summary>
/// DTO for detailed approval information.
/// </summary>
public record ApprovalDetailDto
{
    public Guid Id { get; init; }
    public Guid CalculationId { get; init; }
    public Guid ApproverId { get; init; }
    public string ApproverName { get; init; } = null!;
    public string ApproverEmail { get; init; } = null!;
    public int ApprovalLevel { get; init; }
    public string ApprovalLevelName { get; init; } = null!;
    public ApprovalStatus Status { get; init; }
    public string StatusDisplay { get; init; } = null!;
    public DateTime? ActionDate { get; init; }
    public string? Comments { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }

    // Delegation info
    public Guid? DelegatedToId { get; init; }
    public string? DelegatedToName { get; init; }
    public DateTime? DelegatedAt { get; init; }

    // Calculation details
    public CalculationSummaryForApproval Calculation { get; init; } = null!;
}

/// <summary>
/// Simplified calculation info for approval context.
/// </summary>
public record CalculationSummaryForApproval
{
    public Guid Id { get; init; }
    public string EmployeeCode { get; init; } = null!;
    public string EmployeeName { get; init; } = null!;
    public string Department { get; init; } = null!;
    public string PlanCode { get; init; } = null!;
    public string PlanName { get; init; } = null!;
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public decimal TargetValue { get; init; }
    public decimal ActualValue { get; init; }
    public decimal AchievementPercentage { get; init; }
    public decimal GrossIncentive { get; init; }
    public decimal NetIncentive { get; init; }
    public string Currency { get; init; } = "INR";
    public CalculationStatus CalculationStatus { get; init; }
}

/// <summary>
/// DTO for approval dashboard.
/// "Super Nintendo Chalmers!" - Super dashboard!
/// </summary>
public record ApprovalDashboardDto
{
    public int PendingCount { get; init; }
    public int ApprovedTodayCount { get; init; }
    public int RejectedTodayCount { get; init; }
    public int OverdueCount { get; init; }
    public int DelegatedToMeCount { get; init; }
    public decimal TotalPendingAmount { get; init; }
    public string Currency { get; init; } = "INR";
    public IReadOnlyList<ApprovalSummaryDto> RecentApprovals { get; init; } = Array.Empty<ApprovalSummaryDto>();
}

/// <summary>
/// Result of an approval action.
/// </summary>
public record ApprovalResultDto
{
    public Guid ApprovalId { get; init; }
    public Guid CalculationId { get; init; }
    public ApprovalStatus NewStatus { get; init; }
    public string Message { get; init; } = null!;
    public bool RequiresNextLevelApproval { get; init; }
    public int? NextApprovalLevel { get; init; }
}

/// <summary>
/// Result of bulk approval operation.
/// </summary>
public record BulkApprovalResultDto
{
    public int TotalProcessed { get; init; }
    public int SuccessCount { get; init; }
    public int FailedCount { get; init; }
    public decimal TotalApprovedAmount { get; init; }
    public string Currency { get; init; } = "INR";
    public IReadOnlyList<ApprovalResultDto> Results { get; init; } = Array.Empty<ApprovalResultDto>();
    public IReadOnlyList<ApprovalErrorDto> Errors { get; init; } = Array.Empty<ApprovalErrorDto>();
}

/// <summary>
/// Error information for failed approvals.
/// </summary>
public record ApprovalErrorDto
{
    public Guid ApprovalId { get; init; }
    public string ErrorMessage { get; init; } = null!;
    public string? ErrorCode { get; init; }
}

/// <summary>
/// Result of submission for approval.
/// </summary>
public record SubmissionResultDto
{
    public int TotalSubmitted { get; init; }
    public int SuccessCount { get; init; }
    public int FailedCount { get; init; }
    public IReadOnlyList<SubmissionItemResultDto> Results { get; init; } = Array.Empty<SubmissionItemResultDto>();
}

/// <summary>
/// Individual submission result.
/// </summary>
public record SubmissionItemResultDto
{
    public Guid CalculationId { get; init; }
    public bool Success { get; init; }
    public Guid? ApprovalId { get; init; }
    public int? ApprovalLevel { get; init; }
    public Guid? AssignedApproverId { get; init; }
    public string? ApproverName { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Paginated approval result.
/// </summary>
public record PagedApprovalResult
{
    public IReadOnlyList<ApprovalSummaryDto> Items { get; init; } = Array.Empty<ApprovalSummaryDto>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
