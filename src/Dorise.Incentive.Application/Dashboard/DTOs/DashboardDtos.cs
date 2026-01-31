using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Application.Dashboard.DTOs;

/// <summary>
/// Main dashboard data containing all KPIs and metrics.
/// "Hi, Super Nintendo Chalmers!" - Hi, Super Dashboard Data!
/// </summary>
public record DashboardDto
{
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
    public required string Period { get; init; }
    public required SummaryKpisDto Summary { get; init; }
    public required CalculationMetricsDto CalculationMetrics { get; init; }
    public required ApprovalMetricsDto ApprovalMetrics { get; init; }
    public required PaymentMetricsDto PaymentMetrics { get; init; }
    public required PerformanceMetricsDto PerformanceMetrics { get; init; }
    public IReadOnlyList<TrendDataPointDto> IncentiveTrends { get; init; } = new List<TrendDataPointDto>();
    public IReadOnlyList<TopPerformerDto> TopPerformers { get; init; } = new List<TopPerformerDto>();
    public IReadOnlyList<DepartmentSummaryDto> DepartmentSummaries { get; init; } = new List<DepartmentSummaryDto>();
    public IReadOnlyList<AlertDto> Alerts { get; init; } = new List<AlertDto>();
}

/// <summary>
/// Summary KPIs for quick overview.
/// </summary>
public record SummaryKpisDto
{
    public int TotalEmployees { get; init; }
    public int ActiveEmployees { get; init; }
    public int EligibleEmployees { get; init; }
    public int TotalCalculations { get; init; }
    public decimal TotalIncentiveAmount { get; init; }
    public decimal AverageIncentive { get; init; }
    public decimal MedianIncentive { get; init; }
    public decimal TotalPaidAmount { get; init; }
    public decimal TotalPendingAmount { get; init; }
    public double AverageAchievementPercentage { get; init; }
    public string Currency { get; init; } = "USD";
}

/// <summary>
/// Calculation-related metrics.
/// </summary>
public record CalculationMetricsDto
{
    public int TotalCalculations { get; init; }
    public int PendingCalculations { get; init; }
    public int CompletedCalculations { get; init; }
    public int FailedCalculations { get; init; }
    public double CalculationSuccessRate { get; init; }
    public Dictionary<CalculationStatus, int> ByStatus { get; init; } = new();
    public Dictionary<string, int> ByPlan { get; init; } = new();
    public DateTime? LastCalculationRun { get; init; }
    public double AverageProcessingTimeMs { get; init; }
}

/// <summary>
/// Approval workflow metrics.
/// </summary>
public record ApprovalMetricsDto
{
    public int TotalPendingApprovals { get; init; }
    public int ApprovedThisPeriod { get; init; }
    public int RejectedThisPeriod { get; init; }
    public double ApprovalRate { get; init; }
    public double AverageApprovalTimeHours { get; init; }
    public int OverduApprovals { get; init; }
    public int ApproachingSlaApprovals { get; init; }
    public Dictionary<string, int> ByApprover { get; init; } = new();
    public Dictionary<ApprovalStatus, int> ByStatus { get; init; } = new();
    public IReadOnlyList<PendingApprovalSummaryDto> UrgentApprovals { get; init; } = new List<PendingApprovalSummaryDto>();
}

/// <summary>
/// Payment metrics.
/// </summary>
public record PaymentMetricsDto
{
    public decimal TotalPaidAmount { get; init; }
    public decimal TotalPendingPaymentAmount { get; init; }
    public int PaidCount { get; init; }
    public int PendingPaymentCount { get; init; }
    public DateTime? LastPaymentDate { get; init; }
    public DateTime? NextPaymentDate { get; init; }
    public Dictionary<string, decimal> PaymentsByMonth { get; init; } = new();
    public string Currency { get; init; } = "USD";
}

/// <summary>
/// Performance metrics.
/// </summary>
public record PerformanceMetricsDto
{
    public double AverageAchievement { get; init; }
    public double MedianAchievement { get; init; }
    public double HighestAchievement { get; init; }
    public double LowestAchievement { get; init; }
    public int AboveTargetCount { get; init; }
    public int AtTargetCount { get; init; }
    public int BelowTargetCount { get; init; }
    public double AboveTargetPercentage { get; init; }
    public Dictionary<string, double> AchievementByDepartment { get; init; } = new();
    public Dictionary<string, double> AchievementByPlan { get; init; } = new();
}

/// <summary>
/// Trend data point for charts.
/// </summary>
public record TrendDataPointDto
{
    public required string Period { get; init; }
    public DateTime Date { get; init; }
    public decimal TotalIncentive { get; init; }
    public int CalculationCount { get; init; }
    public double AverageAchievement { get; init; }
    public decimal AverageIncentive { get; init; }
}

/// <summary>
/// Top performer summary.
/// </summary>
public record TopPerformerDto
{
    public Guid EmployeeId { get; init; }
    public required string EmployeeName { get; init; }
    public required string EmployeeCode { get; init; }
    public string? Department { get; init; }
    public string? Position { get; init; }
    public double AchievementPercentage { get; init; }
    public decimal IncentiveAmount { get; init; }
    public int Rank { get; init; }
    public double ChangeFromPreviousPeriod { get; init; }
    public string Currency { get; init; } = "USD";
}

/// <summary>
/// Department summary for dashboard.
/// </summary>
public record DepartmentSummaryDto
{
    public Guid DepartmentId { get; init; }
    public required string DepartmentName { get; init; }
    public required string DepartmentCode { get; init; }
    public int EmployeeCount { get; init; }
    public int CalculationCount { get; init; }
    public decimal TotalIncentive { get; init; }
    public decimal AverageIncentive { get; init; }
    public double AverageAchievement { get; init; }
    public int AboveTargetCount { get; init; }
    public double AboveTargetPercentage { get; init; }
    public string Currency { get; init; } = "USD";
}

/// <summary>
/// Dashboard alert/notification.
/// </summary>
public record AlertDto
{
    public required string Title { get; init; }
    public required string Message { get; init; }
    public AlertSeverity Severity { get; init; }
    public AlertCategory Category { get; init; }
    public string? ActionUrl { get; init; }
    public DateTime CreatedAt { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Pending approval summary for dashboard.
/// </summary>
public record PendingApprovalSummaryDto
{
    public Guid CalculationId { get; init; }
    public Guid EmployeeId { get; init; }
    public required string EmployeeName { get; init; }
    public required string Period { get; init; }
    public decimal Amount { get; init; }
    public DateTime SubmittedAt { get; init; }
    public double HoursPending { get; init; }
    public double SlaHoursRemaining { get; init; }
    public bool IsOverdue { get; init; }
    public string Currency { get; init; } = "USD";
}

/// <summary>
/// Alert severity levels.
/// </summary>
public enum AlertSeverity
{
    Info,
    Warning,
    Error,
    Critical
}

/// <summary>
/// Alert categories.
/// </summary>
public enum AlertCategory
{
    Approval,
    Calculation,
    Payment,
    System,
    Compliance,
    Performance
}

/// <summary>
/// Dashboard filter options.
/// </summary>
public record DashboardFilterDto
{
    public string? Period { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? PlanId { get; init; }
    public bool IncludeTrends { get; init; } = true;
    public bool IncludeTopPerformers { get; init; } = true;
    public int TopPerformersCount { get; init; } = 10;
    public bool IncludeDepartmentSummaries { get; init; } = true;
    public bool IncludeAlerts { get; init; } = true;
}

/// <summary>
/// Widget data for individual dashboard widgets.
/// </summary>
public record WidgetDataDto
{
    public required string WidgetId { get; init; }
    public required string Title { get; init; }
    public required string Type { get; init; }
    public object? Data { get; init; }
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Comparison data between periods.
/// </summary>
public record PeriodComparisonDto
{
    public required string CurrentPeriod { get; init; }
    public required string PreviousPeriod { get; init; }
    public decimal CurrentTotal { get; init; }
    public decimal PreviousTotal { get; init; }
    public decimal Difference { get; init; }
    public double PercentageChange { get; init; }
    public int CurrentCount { get; init; }
    public int PreviousCount { get; init; }
    public double CurrentAverageAchievement { get; init; }
    public double PreviousAverageAchievement { get; init; }
    public double AchievementChange { get; init; }
    public string Trend => PercentageChange >= 0 ? "up" : "down";
}

/// <summary>
/// Leaderboard entry.
/// </summary>
public record LeaderboardEntryDto
{
    public int Rank { get; init; }
    public int PreviousRank { get; init; }
    public int RankChange => PreviousRank - Rank;
    public Guid EmployeeId { get; init; }
    public required string EmployeeName { get; init; }
    public required string EmployeeCode { get; init; }
    public string? Department { get; init; }
    public double AchievementPercentage { get; init; }
    public decimal IncentiveAmount { get; init; }
    public int ConsecutiveTopPerformerMonths { get; init; }
    public string Currency { get; init; } = "USD";
}

/// <summary>
/// Full leaderboard response.
/// </summary>
public record LeaderboardDto
{
    public required string Period { get; init; }
    public DateTime GeneratedAt { get; init; } = DateTime.UtcNow;
    public int TotalParticipants { get; init; }
    public IReadOnlyList<LeaderboardEntryDto> Entries { get; init; } = new List<LeaderboardEntryDto>();
}

/// <summary>
/// Quick stats for header/summary cards.
/// </summary>
public record QuickStatsDto
{
    public required string Period { get; init; }
    public decimal TotalIncentives { get; init; }
    public decimal TotalIncentivesChange { get; init; }
    public int TotalEmployees { get; init; }
    public int TotalEmployeesChange { get; init; }
    public double AverageAchievement { get; init; }
    public double AverageAchievementChange { get; init; }
    public int PendingApprovals { get; init; }
    public int OverdueApprovals { get; init; }
    public string Currency { get; init; } = "USD";
}
