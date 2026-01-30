using Dorise.Incentive.Application.Dashboard.DTOs;
using Dorise.Incentive.Application.Dashboard.Services;
using Dorise.Incentive.Application.Reports.DTOs;
using Dorise.Incentive.Application.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for executive dashboard and analytics.
/// "Super Nintendo Chalmers!" - Super dashboards!
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IDashboardService _dashboardService;
    private readonly ILogger<DashboardController> _logger;

    public DashboardController(
        IMediator mediator,
        IDashboardService dashboardService,
        ILogger<DashboardController> logger)
    {
        _mediator = mediator;
        _dashboardService = dashboardService;
        _logger = logger;
    }

    /// <summary>
    /// Get executive dashboard data.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(DashboardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] int topPerformerCount = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardQuery(topPerformerCount);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            success => Ok(success),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Get KPIs only.
    /// </summary>
    [HttpGet("kpis")]
    [ProducesResponseType(typeof(DashboardKpisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetKpis(CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardQuery(0);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            success => Ok(success.Kpis),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Get monthly trend data.
    /// </summary>
    [HttpGet("trend")]
    [ProducesResponseType(typeof(IReadOnlyList<MonthlyTrendDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetMonthlyTrend(CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardQuery(0);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            success => Ok(success.MonthlyTrend),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Get top performers for current month.
    /// </summary>
    [HttpGet("top-performers")]
    [ProducesResponseType(typeof(IReadOnlyList<TopPerformerDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetTopPerformers(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardQuery(count);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            success => Ok(success.TopPerformers),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Get pending actions summary.
    /// </summary>
    [HttpGet("pending-actions")]
    [ProducesResponseType(typeof(IReadOnlyList<PendingActionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPendingActions(CancellationToken cancellationToken = default)
    {
        var query = new GetDashboardQuery(0);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match(
            success => Ok(success.PendingActions),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Get full analytics dashboard with all metrics.
    /// </summary>
    [HttpGet("analytics")]
    [ProducesResponseType(typeof(Application.Dashboard.DTOs.DashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAnalyticsDashboard(
        [FromQuery] DashboardFilterDto filter,
        CancellationToken cancellationToken)
    {
        filter ??= new DashboardFilterDto();
        var dashboard = await _dashboardService.GetDashboardAsync(filter, cancellationToken);
        return Ok(dashboard);
    }

    /// <summary>
    /// Get quick stats for header cards.
    /// </summary>
    [HttpGet("quick-stats")]
    [ProducesResponseType(typeof(QuickStatsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQuickStats(
        [FromQuery] string? period,
        CancellationToken cancellationToken)
    {
        var stats = await _dashboardService.GetQuickStatsAsync(
            period ?? DateTime.UtcNow.ToString("yyyy-MM"),
            cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Get calculation metrics.
    /// </summary>
    [HttpGet("calculation-metrics")]
    [ProducesResponseType(typeof(CalculationMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalculationMetrics(
        [FromQuery] string? period,
        [FromQuery] Guid? departmentId,
        CancellationToken cancellationToken)
    {
        var metrics = await _dashboardService.GetCalculationMetricsAsync(
            period ?? DateTime.UtcNow.ToString("yyyy-MM"),
            departmentId,
            cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Get approval metrics.
    /// </summary>
    [HttpGet("approval-metrics")]
    [ProducesResponseType(typeof(ApprovalMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApprovalMetrics(
        [FromQuery] string? period,
        CancellationToken cancellationToken)
    {
        var metrics = await _dashboardService.GetApprovalMetricsAsync(
            period ?? DateTime.UtcNow.ToString("yyyy-MM"),
            cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Get payment metrics.
    /// </summary>
    [HttpGet("payment-metrics")]
    [ProducesResponseType(typeof(PaymentMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentMetrics(
        [FromQuery] string? period,
        CancellationToken cancellationToken)
    {
        var metrics = await _dashboardService.GetPaymentMetricsAsync(
            period ?? DateTime.UtcNow.ToString("yyyy-MM"),
            cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Get performance metrics.
    /// </summary>
    [HttpGet("performance-metrics")]
    [ProducesResponseType(typeof(PerformanceMetricsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPerformanceMetrics(
        [FromQuery] string? period,
        [FromQuery] Guid? departmentId,
        CancellationToken cancellationToken)
    {
        var metrics = await _dashboardService.GetPerformanceMetricsAsync(
            period ?? DateTime.UtcNow.ToString("yyyy-MM"),
            departmentId,
            cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Get incentive trends over time.
    /// </summary>
    [HttpGet("trends")]
    [ProducesResponseType(typeof(IReadOnlyList<Application.Dashboard.DTOs.TrendDataPointDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIncentiveTrends(
        [FromQuery] int months = 12,
        [FromQuery] Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var trends = await _dashboardService.GetIncentiveTrendsAsync(months, departmentId, cancellationToken);
        return Ok(trends);
    }

    /// <summary>
    /// Get department summaries.
    /// </summary>
    [HttpGet("departments")]
    [ProducesResponseType(typeof(IReadOnlyList<DepartmentSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartmentSummaries(
        [FromQuery] string? period,
        CancellationToken cancellationToken)
    {
        var summaries = await _dashboardService.GetDepartmentSummariesAsync(
            period ?? DateTime.UtcNow.ToString("yyyy-MM"),
            cancellationToken);
        return Ok(summaries);
    }

    /// <summary>
    /// Get dashboard alerts.
    /// </summary>
    [HttpGet("alerts")]
    [ProducesResponseType(typeof(IReadOnlyList<AlertDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAlerts(CancellationToken cancellationToken)
    {
        var alerts = await _dashboardService.GetAlertsAsync(cancellationToken);
        return Ok(alerts);
    }

    /// <summary>
    /// Get leaderboard.
    /// </summary>
    [HttpGet("leaderboard")]
    [ProducesResponseType(typeof(LeaderboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLeaderboard(
        [FromQuery] string? period,
        [FromQuery] int topN = 100,
        [FromQuery] Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var leaderboard = await _dashboardService.GetLeaderboardAsync(
            period ?? DateTime.UtcNow.ToString("yyyy-MM"),
            topN,
            departmentId,
            cancellationToken);
        return Ok(leaderboard);
    }

    /// <summary>
    /// Get period comparison data.
    /// </summary>
    [HttpGet("comparison")]
    [ProducesResponseType(typeof(PeriodComparisonDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPeriodComparison(
        [FromQuery] string currentPeriod,
        [FromQuery] string? previousPeriod,
        [FromQuery] Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var prevPeriod = previousPeriod ?? GetPreviousPeriod(currentPeriod);
        var comparison = await _dashboardService.GetPeriodComparisonAsync(
            currentPeriod,
            prevPeriod,
            departmentId,
            cancellationToken);
        return Ok(comparison);
    }

    /// <summary>
    /// Get specific widget data.
    /// </summary>
    [HttpGet("widget/{widgetId}")]
    [ProducesResponseType(typeof(WidgetDataDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWidgetData(
        string widgetId,
        [FromQuery] DashboardFilterDto filter,
        CancellationToken cancellationToken)
    {
        filter ??= new DashboardFilterDto();
        var data = await _dashboardService.GetWidgetDataAsync(widgetId, filter, cancellationToken);
        return Ok(data);
    }

    /// <summary>
    /// Get urgent approvals requiring attention.
    /// </summary>
    [HttpGet("urgent-approvals")]
    [ProducesResponseType(typeof(IReadOnlyList<PendingApprovalSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUrgentApprovals(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default)
    {
        var approvals = await _dashboardService.GetUrgentApprovalsAsync(count, cancellationToken);
        return Ok(approvals);
    }

    private static string GetPreviousPeriod(string period)
    {
        if (DateTime.TryParseExact(period + "-01", "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out var date))
        {
            return date.AddMonths(-1).ToString("yyyy-MM");
        }
        return DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM");
    }
}
