using Dorise.Incentive.Application.Reports.DTOs;

namespace Dorise.Incentive.Application.Reports.Services;

/// <summary>
/// Service interface for generating various reports.
/// "I choo-choo-choose you!" - I choo-choo-choose the right report format!
/// </summary>
public interface IReportGenerationService
{
    /// <summary>
    /// Generate a report based on request parameters.
    /// </summary>
    Task<GeneratedReportDto> GenerateReportAsync(
        ReportRequestDto request,
        string generatedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate incentive summary report.
    /// </summary>
    Task<PayoutReportDto> GenerateIncentiveSummaryAsync(
        string period,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate achievement summary report.
    /// </summary>
    Task<AchievementSummaryDto> GenerateAchievementSummaryAsync(
        string period,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate variance analysis report.
    /// </summary>
    Task<VarianceAnalysisDto> GenerateVarianceAnalysisAsync(
        string currentPeriod,
        string previousPeriod,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate forecast report.
    /// </summary>
    Task<ForecastReportDto> GenerateForecastAsync(
        int monthsAhead = 6,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generate executive dashboard report.
    /// </summary>
    Task<DashboardDto> GenerateDashboardReportAsync(
        string period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Export report to specific format.
    /// </summary>
    Task<ExportResultDto> ExportReportAsync(
        ReportRequestDto request,
        string format,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get available report types.
    /// </summary>
    IReadOnlyList<ReportTypeInfoDto> GetAvailableReportTypes();

    /// <summary>
    /// Schedule a report for recurring generation.
    /// </summary>
    Task<ReportScheduleDto> ScheduleReportAsync(
        ReportScheduleDto schedule,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get scheduled reports.
    /// </summary>
    Task<IReadOnlyList<ReportScheduleDto>> GetScheduledReportsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cancel a scheduled report.
    /// </summary>
    Task CancelScheduledReportAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Report type information.
/// </summary>
public record ReportTypeInfoDto
{
    public required string TypeId { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<string> SupportedFormats { get; init; } = new List<string>();
    public IReadOnlyList<ReportParameterDto> Parameters { get; init; } = new List<ReportParameterDto>();
}

/// <summary>
/// Report parameter definition.
/// </summary>
public record ReportParameterDto
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public bool Required { get; init; }
    public string? DefaultValue { get; init; }
    public string? Description { get; init; }
}
