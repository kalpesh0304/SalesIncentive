using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Application.Calculations.DTOs;

/// <summary>
/// Data transfer object for Calculation.
/// "I'm in danger!" - But calculations keep finances safe!
/// </summary>
public record CalculationDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = null!;
    public string EmployeeName { get; init; } = null!;
    public Guid IncentivePlanId { get; init; }
    public string PlanCode { get; init; } = null!;
    public string PlanName { get; init; } = null!;
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public CalculationStatus Status { get; init; }
    public string StatusDisplay { get; init; } = null!;

    // Target and Achievement
    public decimal TargetValue { get; init; }
    public decimal ActualValue { get; init; }
    public decimal AchievementPercentage { get; init; }

    // Payout Details
    public decimal BaseSalary { get; init; }
    public decimal GrossIncentive { get; init; }
    public decimal NetIncentive { get; init; }
    public string Currency { get; init; } = "INR";
    public decimal? ProrataFactor { get; init; }
    public Guid? AppliedSlabId { get; init; }
    public string? AppliedSlabDescription { get; init; }

    // Audit
    public DateTime CalculatedAt { get; init; }
    public string? CalculatedBy { get; init; }
    public string? RejectionReason { get; init; }
    public string? AdjustmentReason { get; init; }
    public int Version { get; init; }

    public IReadOnlyList<ApprovalDto> Approvals { get; init; } = Array.Empty<ApprovalDto>();
}

/// <summary>
/// Summary DTO for calculation lists.
/// </summary>
public record CalculationSummaryDto
{
    public Guid Id { get; init; }
    public string EmployeeCode { get; init; } = null!;
    public string EmployeeName { get; init; } = null!;
    public string PlanName { get; init; } = null!;
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public CalculationStatus Status { get; init; }
    public decimal AchievementPercentage { get; init; }
    public decimal NetIncentive { get; init; }
    public string Currency { get; init; } = "INR";
}

/// <summary>
/// DTO for approval information.
/// </summary>
public record ApprovalDto
{
    public Guid Id { get; init; }
    public Guid ApproverId { get; init; }
    public string ApproverName { get; init; } = null!;
    public int ApprovalLevel { get; init; }
    public ApprovalStatus Status { get; init; }
    public DateTime? ActionDate { get; init; }
    public string? Comments { get; init; }
}

/// <summary>
/// DTO for calculation run request.
/// </summary>
public record RunCalculationRequestDto
{
    public Guid EmployeeId { get; init; }
    public Guid IncentivePlanId { get; init; }
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public decimal ActualValue { get; init; }
}

/// <summary>
/// DTO for bulk calculation run request.
/// </summary>
public record BulkCalculationRequestDto
{
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? IncentivePlanId { get; init; }
    public IReadOnlyList<EmployeeAchievementDto> Achievements { get; init; } = Array.Empty<EmployeeAchievementDto>();
}

/// <summary>
/// DTO for employee achievement data.
/// </summary>
public record EmployeeAchievementDto
{
    public Guid EmployeeId { get; init; }
    public decimal ActualValue { get; init; }
}

/// <summary>
/// DTO for batch calculation results.
/// "Me fail English? That's unpossible!" - But batch calculations never fail!
/// </summary>
public record BatchCalculationResultDto
{
    public int TotalProcessed { get; init; }
    public int SuccessCount { get; init; }
    public int FailedCount { get; init; }
    public decimal TotalIncentiveAmount { get; init; }
    public string Currency { get; init; } = "INR";
    public DateTime ProcessedAt { get; init; }
    public TimeSpan Duration { get; init; }
    public IReadOnlyList<CalculationSummaryDto> Calculations { get; init; } = Array.Empty<CalculationSummaryDto>();
    public IReadOnlyList<BatchCalculationErrorDto> Errors { get; init; } = Array.Empty<BatchCalculationErrorDto>();
}

/// <summary>
/// DTO for batch calculation error.
/// </summary>
public record BatchCalculationErrorDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = null!;
    public string ErrorMessage { get; init; } = null!;
    public string? ErrorCode { get; init; }
}

/// <summary>
/// DTO for calculation period summary.
/// "I'm learnding!" - Learning from calculation summaries!
/// </summary>
public record CalculationPeriodSummaryDto
{
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public int TotalCalculations { get; init; }
    public int PendingCount { get; init; }
    public int CalculatedCount { get; init; }
    public int PendingApprovalCount { get; init; }
    public int ApprovedCount { get; init; }
    public int RejectedCount { get; init; }
    public int PaidCount { get; init; }
    public int VoidedCount { get; init; }
    public decimal TotalGrossIncentive { get; init; }
    public decimal TotalNetIncentive { get; init; }
    public decimal AverageAchievement { get; init; }
    public string Currency { get; init; } = "INR";
    public IReadOnlyList<PlanSummaryDto> ByPlan { get; init; } = Array.Empty<PlanSummaryDto>();
}

/// <summary>
/// Summary by plan for period summary.
/// </summary>
public record PlanSummaryDto
{
    public Guid PlanId { get; init; }
    public string PlanName { get; init; } = null!;
    public int CalculationCount { get; init; }
    public decimal TotalIncentive { get; init; }
    public decimal AverageAchievement { get; init; }
}

/// <summary>
/// DTO for calculation preview (without saving).
/// "That's where I saw the leprechaun!" - Preview your incentives before committing!
/// </summary>
public record CalculationPreviewDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = null!;
    public string EmployeeName { get; init; } = null!;
    public Guid IncentivePlanId { get; init; }
    public string PlanName { get; init; } = null!;
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }

    // Target and Achievement
    public decimal TargetValue { get; init; }
    public decimal ActualValue { get; init; }
    public decimal AchievementPercentage { get; init; }

    // Calculated Amounts
    public decimal GrossIncentive { get; init; }
    public decimal NetIncentive { get; init; }
    public string Currency { get; init; } = "INR";

    // Applied Rules
    public Guid? AppliedSlabId { get; init; }
    public string? AppliedSlabDescription { get; init; }
    public decimal? ProrataFactor { get; init; }
    public bool IsCapped { get; init; }
    public decimal? CapAmount { get; init; }
    public bool IsBelowThreshold { get; init; }

    // Eligibility
    public bool IsEligible { get; init; }
    public string? IneligibilityReason { get; init; }

    // Breakdown
    public IReadOnlyList<CalculationBreakdownDto> Breakdown { get; init; } = Array.Empty<CalculationBreakdownDto>();
}

/// <summary>
/// DTO for calculation breakdown details.
/// </summary>
public record CalculationBreakdownDto
{
    public string Component { get; init; } = null!;
    public string Description { get; init; } = null!;
    public decimal Value { get; init; }
    public string? Formula { get; init; }
}
