using Dorise.Incentive.Application.Calculations.DTOs;
using Dorise.Incentive.Application.Common.Interfaces;

namespace Dorise.Incentive.Application.Calculations.Commands.BatchCalculation;

/// <summary>
/// Command to run batch calculation for multiple employees.
/// "I can't believe I ate the whole thing!" - Batch calculations eat ALL the data!
/// </summary>
public record RunBatchCalculationCommand(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    Guid? IncentivePlanId,
    Guid? DepartmentId,
    IReadOnlyList<EmployeeAchievementData>? EmployeeAchievements) : ICommand<BatchCalculationResultDto>;

/// <summary>
/// Achievement data for a single employee in batch.
/// </summary>
public record EmployeeAchievementData(Guid EmployeeId, decimal ActualValue);

/// <summary>
/// Result of batch calculation.
/// </summary>
public record BatchCalculationResultDto
{
    public int TotalEmployees { get; init; }
    public int SuccessCount { get; init; }
    public int FailedCount { get; init; }
    public int SkippedCount { get; init; }
    public decimal TotalGrossIncentive { get; init; }
    public decimal TotalNetIncentive { get; init; }
    public string Currency { get; init; } = "INR";
    public TimeSpan ProcessingTime { get; init; }
    public IReadOnlyList<BatchCalculationItemDto> Results { get; init; } = Array.Empty<BatchCalculationItemDto>();
    public IReadOnlyList<BatchCalculationErrorDto> Errors { get; init; } = Array.Empty<BatchCalculationErrorDto>();
}

/// <summary>
/// Individual result in batch calculation.
/// </summary>
public record BatchCalculationItemDto
{
    public Guid CalculationId { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = null!;
    public string EmployeeName { get; init; } = null!;
    public decimal ActualValue { get; init; }
    public decimal AchievementPercentage { get; init; }
    public decimal GrossIncentive { get; init; }
    public decimal NetIncentive { get; init; }
    public string? AppliedSlab { get; init; }
    public string Status { get; init; } = null!;
}

/// <summary>
/// Error details for failed batch calculation.
/// </summary>
public record BatchCalculationErrorDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = null!;
    public string ErrorMessage { get; init; } = null!;
    public string ErrorCode { get; init; } = null!;
}
