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
