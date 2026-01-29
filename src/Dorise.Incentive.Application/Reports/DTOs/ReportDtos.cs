using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Application.Reports.DTOs;

/// <summary>
/// DTO for payout report data.
/// "I'm a star!" - Shine bright with payout data!
/// </summary>
public record PayoutReportDto
{
    public DateTime GeneratedAt { get; init; }
    public string Period { get; init; } = null!;
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public PayoutSummaryDto Summary { get; init; } = null!;
    public IReadOnlyList<PayoutDetailDto> Details { get; init; } = Array.Empty<PayoutDetailDto>();
}

/// <summary>
/// Summary section of payout report.
/// </summary>
public record PayoutSummaryDto
{
    public int TotalEmployees { get; init; }
    public int EligibleEmployees { get; init; }
    public int PaidEmployees { get; init; }
    public decimal TotalGrossIncentive { get; init; }
    public decimal TotalNetIncentive { get; init; }
    public decimal TotalDeductions { get; init; }
    public string Currency { get; init; } = "INR";
    public decimal AverageIncentive { get; init; }
    public decimal MedianIncentive { get; init; }
    public decimal MinIncentive { get; init; }
    public decimal MaxIncentive { get; init; }
}

/// <summary>
/// Individual payout detail for a single employee.
/// </summary>
public record PayoutDetailDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = null!;
    public string EmployeeName { get; init; } = null!;
    public string Department { get; init; } = null!;
    public string PlanCode { get; init; } = null!;
    public string PlanName { get; init; } = null!;
    public decimal TargetValue { get; init; }
    public decimal ActualValue { get; init; }
    public decimal AchievementPercentage { get; init; }
    public decimal GrossIncentive { get; init; }
    public decimal NetIncentive { get; init; }
    public string Currency { get; init; } = "INR";
    public string SlabApplied { get; init; } = null!;
    public CalculationStatus Status { get; init; }
    public DateTime? PaidDate { get; init; }
}

/// <summary>
/// DTO for achievement summary report.
/// "Super Nintendo Chalmers!" - Super achievement tracking!
/// </summary>
public record AchievementSummaryDto
{
    public DateTime GeneratedAt { get; init; }
    public string Period { get; init; } = null!;
    public DateTime PeriodStart { get; init; }
    public DateTime PeriodEnd { get; init; }
    public OverallAchievementDto Overall { get; init; } = null!;
    public IReadOnlyList<DepartmentAchievementDto> ByDepartment { get; init; } = Array.Empty<DepartmentAchievementDto>();
    public IReadOnlyList<PlanAchievementDto> ByPlan { get; init; } = Array.Empty<PlanAchievementDto>();
    public IReadOnlyList<AchievementBandDto> ByAchievementBand { get; init; } = Array.Empty<AchievementBandDto>();
}

/// <summary>
/// Overall achievement statistics.
/// </summary>
public record OverallAchievementDto
{
    public int TotalEmployees { get; init; }
    public decimal AverageAchievement { get; init; }
    public decimal MedianAchievement { get; init; }
    public int AboveTargetCount { get; init; }
    public int AtTargetCount { get; init; }
    public int BelowTargetCount { get; init; }
    public decimal AboveTargetPercentage { get; init; }
}

/// <summary>
/// Achievement by department.
/// </summary>
public record DepartmentAchievementDto
{
    public Guid DepartmentId { get; init; }
    public string DepartmentName { get; init; } = null!;
    public int EmployeeCount { get; init; }
    public decimal AverageAchievement { get; init; }
    public decimal TotalTarget { get; init; }
    public decimal TotalActual { get; init; }
    public decimal TotalIncentive { get; init; }
    public string Currency { get; init; } = "INR";
}

/// <summary>
/// Achievement by plan.
/// </summary>
public record PlanAchievementDto
{
    public Guid PlanId { get; init; }
    public string PlanCode { get; init; } = null!;
    public string PlanName { get; init; } = null!;
    public int EmployeeCount { get; init; }
    public decimal AverageAchievement { get; init; }
    public decimal TotalIncentive { get; init; }
    public string Currency { get; init; } = "INR";
}

/// <summary>
/// Achievement band distribution.
/// </summary>
public record AchievementBandDto
{
    public string BandName { get; init; } = null!;
    public decimal MinPercentage { get; init; }
    public decimal MaxPercentage { get; init; }
    public int EmployeeCount { get; init; }
    public decimal Percentage { get; init; }
}

/// <summary>
/// DTO for variance analysis report.
/// "I bent my Wookiee!" - But we won't bend the numbers!
/// </summary>
public record VarianceAnalysisDto
{
    public DateTime GeneratedAt { get; init; }
    public string CurrentPeriod { get; init; } = null!;
    public string PreviousPeriod { get; init; } = null!;
    public VarianceSummaryDto Summary { get; init; } = null!;
    public IReadOnlyList<EmployeeVarianceDto> TopGainers { get; init; } = Array.Empty<EmployeeVarianceDto>();
    public IReadOnlyList<EmployeeVarianceDto> TopDecliners { get; init; } = Array.Empty<EmployeeVarianceDto>();
    public IReadOnlyList<DepartmentVarianceDto> ByDepartment { get; init; } = Array.Empty<DepartmentVarianceDto>();
}

/// <summary>
/// Variance summary.
/// </summary>
public record VarianceSummaryDto
{
    public decimal CurrentPeriodTotal { get; init; }
    public decimal PreviousPeriodTotal { get; init; }
    public decimal AbsoluteVariance { get; init; }
    public decimal PercentageVariance { get; init; }
    public decimal CurrentAverageAchievement { get; init; }
    public decimal PreviousAverageAchievement { get; init; }
    public decimal AchievementVariance { get; init; }
    public string Currency { get; init; } = "INR";
}

/// <summary>
/// Employee-level variance.
/// </summary>
public record EmployeeVarianceDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = null!;
    public string EmployeeName { get; init; } = null!;
    public string Department { get; init; } = null!;
    public decimal CurrentIncentive { get; init; }
    public decimal PreviousIncentive { get; init; }
    public decimal AbsoluteVariance { get; init; }
    public decimal PercentageVariance { get; init; }
    public decimal CurrentAchievement { get; init; }
    public decimal PreviousAchievement { get; init; }
    public string Currency { get; init; } = "INR";
}

/// <summary>
/// Department-level variance.
/// </summary>
public record DepartmentVarianceDto
{
    public Guid DepartmentId { get; init; }
    public string DepartmentName { get; init; } = null!;
    public decimal CurrentTotal { get; init; }
    public decimal PreviousTotal { get; init; }
    public decimal AbsoluteVariance { get; init; }
    public decimal PercentageVariance { get; init; }
    public int CurrentEmployeeCount { get; init; }
    public int PreviousEmployeeCount { get; init; }
    public string Currency { get; init; } = "INR";
}

/// <summary>
/// DTO for executive dashboard.
/// </summary>
public record DashboardDto
{
    public DateTime GeneratedAt { get; init; }
    public string CurrentPeriod { get; init; } = null!;
    public DashboardKpisDto Kpis { get; init; } = null!;
    public IReadOnlyList<MonthlyTrendDto> MonthlyTrend { get; init; } = Array.Empty<MonthlyTrendDto>();
    public IReadOnlyList<TopPerformerDto> TopPerformers { get; init; } = Array.Empty<TopPerformerDto>();
    public IReadOnlyList<PendingActionDto> PendingActions { get; init; } = Array.Empty<PendingActionDto>();
}

/// <summary>
/// Key performance indicators.
/// </summary>
public record DashboardKpisDto
{
    public decimal TotalIncentiveYtd { get; init; }
    public decimal TotalIncentiveCurrentMonth { get; init; }
    public int TotalEmployees { get; init; }
    public int EligibleEmployees { get; init; }
    public decimal AverageAchievement { get; init; }
    public int PendingApprovals { get; init; }
    public decimal BudgetUtilization { get; init; }
    public string Currency { get; init; } = "INR";
}

/// <summary>
/// Monthly trend data point.
/// </summary>
public record MonthlyTrendDto
{
    public string Month { get; init; } = null!;
    public int Year { get; init; }
    public decimal TotalIncentive { get; init; }
    public int EmployeeCount { get; init; }
    public decimal AverageAchievement { get; init; }
    public string Currency { get; init; } = "INR";
}

/// <summary>
/// Top performer info.
/// </summary>
public record TopPerformerDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = null!;
    public string EmployeeName { get; init; } = null!;
    public string Department { get; init; } = null!;
    public decimal Achievement { get; init; }
    public decimal Incentive { get; init; }
    public string Currency { get; init; } = "INR";
    public int Rank { get; init; }
}

/// <summary>
/// Pending action item.
/// </summary>
public record PendingActionDto
{
    public string ActionType { get; init; } = null!;
    public int Count { get; init; }
    public string Description { get; init; } = null!;
    public string Url { get; init; } = null!;
}

/// <summary>
/// Export result.
/// </summary>
public record ExportResultDto
{
    public string FileName { get; init; } = null!;
    public string ContentType { get; init; } = null!;
    public byte[] Content { get; init; } = Array.Empty<byte>();
    public int RecordCount { get; init; }
    public DateTime GeneratedAt { get; init; }
}
