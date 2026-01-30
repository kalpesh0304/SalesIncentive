using Dorise.Incentive.Application.Dashboard.DTOs;

namespace Dorise.Incentive.Application.Dashboard.Services;

/// <summary>
/// Service interface for dashboard data and analytics.
/// "The leprechaun tells me to burn things." - The dashboard tells me to analyze things!
/// </summary>
public interface IDashboardService
{
    /// <summary>
    /// Get full dashboard data with all metrics.
    /// </summary>
    Task<DashboardDto> GetDashboardAsync(
        DashboardFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get quick stats for header cards.
    /// </summary>
    Task<QuickStatsDto> GetQuickStatsAsync(
        string period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get summary KPIs.
    /// </summary>
    Task<SummaryKpisDto> GetSummaryKpisAsync(
        string period,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get calculation metrics.
    /// </summary>
    Task<CalculationMetricsDto> GetCalculationMetricsAsync(
        string period,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get approval metrics.
    /// </summary>
    Task<ApprovalMetricsDto> GetApprovalMetricsAsync(
        string period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get payment metrics.
    /// </summary>
    Task<PaymentMetricsDto> GetPaymentMetricsAsync(
        string period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get performance metrics.
    /// </summary>
    Task<PerformanceMetricsDto> GetPerformanceMetricsAsync(
        string period,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get incentive trends over time.
    /// </summary>
    Task<IReadOnlyList<TrendDataPointDto>> GetIncentiveTrendsAsync(
        int months = 12,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get top performers.
    /// </summary>
    Task<IReadOnlyList<TopPerformerDto>> GetTopPerformersAsync(
        string period,
        int count = 10,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get department summaries.
    /// </summary>
    Task<IReadOnlyList<DepartmentSummaryDto>> GetDepartmentSummariesAsync(
        string period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get dashboard alerts.
    /// </summary>
    Task<IReadOnlyList<AlertDto>> GetAlertsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get leaderboard.
    /// </summary>
    Task<LeaderboardDto> GetLeaderboardAsync(
        string period,
        int topN = 100,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get period comparison data.
    /// </summary>
    Task<PeriodComparisonDto> GetPeriodComparisonAsync(
        string currentPeriod,
        string previousPeriod,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get specific widget data.
    /// </summary>
    Task<WidgetDataDto> GetWidgetDataAsync(
        string widgetId,
        DashboardFilterDto filter,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get pending approvals that need attention.
    /// </summary>
    Task<IReadOnlyList<PendingApprovalSummaryDto>> GetUrgentApprovalsAsync(
        int count = 10,
        CancellationToken cancellationToken = default);
}
