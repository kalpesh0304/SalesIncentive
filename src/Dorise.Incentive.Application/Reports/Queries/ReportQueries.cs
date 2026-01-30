using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.Reports.DTOs;

namespace Dorise.Incentive.Application.Reports.Queries;

/// <summary>
/// Query to generate payout report.
/// "I'm learnding!" - Learn about your payouts!
/// </summary>
public record GetPayoutReportQuery(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    Guid? DepartmentId = null,
    Guid? PlanId = null) : IQuery<PayoutReportDto>;

/// <summary>
/// Query to generate achievement summary report.
/// </summary>
public record GetAchievementSummaryQuery(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    Guid? DepartmentId = null) : IQuery<AchievementSummaryDto>;

/// <summary>
/// Query to generate variance analysis report.
/// </summary>
public record GetVarianceAnalysisQuery(
    DateTime CurrentPeriodStart,
    DateTime CurrentPeriodEnd,
    DateTime PreviousPeriodStart,
    DateTime PreviousPeriodEnd,
    Guid? DepartmentId = null,
    int TopCount = 10) : IQuery<VarianceAnalysisDto>;

/// <summary>
/// Query to get executive dashboard data.
/// </summary>
public record GetDashboardQuery(
    int TopPerformerCount = 10) : IQuery<DashboardDto>;

/// <summary>
/// Query to export payout report.
/// </summary>
public record ExportPayoutReportQuery(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    ExportFormat Format = ExportFormat.Excel,
    Guid? DepartmentId = null,
    Guid? PlanId = null) : IQuery<ExportResultDto>;

/// <summary>
/// Query to export achievement summary report.
/// </summary>
public record ExportAchievementSummaryQuery(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    ExportFormat Format = ExportFormat.Excel,
    Guid? DepartmentId = null) : IQuery<ExportResultDto>;

/// <summary>
/// Query to export employee calculations.
/// </summary>
public record ExportCalculationsQuery(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    ExportFormat Format = ExportFormat.Excel,
    Guid? DepartmentId = null,
    Guid? PlanId = null) : IQuery<ExportResultDto>;

/// <summary>
/// Query to get period comparison report.
/// </summary>
public record GetPeriodComparisonQuery(
    IReadOnlyList<PeriodDefinition> Periods,
    Guid? DepartmentId = null) : IQuery<PeriodComparisonDto>;

/// <summary>
/// Period definition for comparison.
/// </summary>
public record PeriodDefinition(
    string Label,
    DateTime StartDate,
    DateTime EndDate);

/// <summary>
/// Period comparison result.
/// </summary>
public record PeriodComparisonDto
{
    public DateTime GeneratedAt { get; init; }
    public IReadOnlyList<PeriodDataDto> Periods { get; init; } = Array.Empty<PeriodDataDto>();
}

/// <summary>
/// Data for a single period in comparison.
/// </summary>
public record PeriodDataDto
{
    public string Label { get; init; } = null!;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int EmployeeCount { get; init; }
    public decimal TotalIncentive { get; init; }
    public decimal AverageIncentive { get; init; }
    public decimal AverageAchievement { get; init; }
    public string Currency { get; init; } = "INR";
}

/// <summary>
/// Export format options.
/// </summary>
public enum ExportFormat
{
    Excel,
    Csv,
    Pdf
}
