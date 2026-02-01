using Asp.Versioning;
using Dorise.Incentive.Application.Reports.DTOs;
using Dorise.Incentive.Application.Reports.Queries;
using Dorise.Incentive.Application.Reports.Services;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for generating and exporting reports.
/// "I'm a unitard!" - Unified reporting endpoints!
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IReportGenerationService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(
        IMediator mediator,
        IReportGenerationService reportService,
        ILogger<ReportsController> logger)
    {
        _mediator = mediator;
        _reportService = reportService;
        _logger = logger;
    }

    /// <summary>
    /// Get payout report for a period.
    /// </summary>
    [HttpGet("payout")]
    [ProducesResponseType(typeof(PayoutReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPayoutReport(
        [FromQuery] DateTime periodStart,
        [FromQuery] DateTime periodEnd,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? planId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetPayoutReportQuery(periodStart, periodEnd, departmentId, planId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            success => Ok(success),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Export payout report to file.
    /// </summary>
    [HttpGet("payout/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportPayoutReport(
        [FromQuery] DateTime periodStart,
        [FromQuery] DateTime periodEnd,
        [FromQuery] ExportFormat format = ExportFormat.Excel,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? planId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new ExportPayoutReportQuery(periodStart, periodEnd, format, departmentId, planId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            export => File(export.Content, export.ContentType, export.FileName),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Get achievement summary report for a period.
    /// </summary>
    [HttpGet("achievement")]
    [ProducesResponseType(typeof(AchievementSummaryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAchievementSummary(
        [FromQuery] DateTime periodStart,
        [FromQuery] DateTime periodEnd,
        [FromQuery] Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAchievementSummaryQuery(periodStart, periodEnd, departmentId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            success => Ok(success),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Export achievement summary report to file.
    /// </summary>
    [HttpGet("achievement/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportAchievementSummary(
        [FromQuery] DateTime periodStart,
        [FromQuery] DateTime periodEnd,
        [FromQuery] ExportFormat format = ExportFormat.Excel,
        [FromQuery] Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new ExportAchievementSummaryQuery(periodStart, periodEnd, format, departmentId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            export => File(export.Content, export.ContentType, export.FileName),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Get variance analysis comparing two periods.
    /// </summary>
    [HttpGet("variance")]
    [ProducesResponseType(typeof(VarianceAnalysisDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetVarianceAnalysis(
        [FromQuery] DateTime currentPeriodStart,
        [FromQuery] DateTime currentPeriodEnd,
        [FromQuery] DateTime previousPeriodStart,
        [FromQuery] DateTime previousPeriodEnd,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] int topCount = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetVarianceAnalysisQuery(
            currentPeriodStart,
            currentPeriodEnd,
            previousPeriodStart,
            previousPeriodEnd,
            departmentId,
            topCount);

        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            success => Ok(success),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Get period comparison report.
    /// </summary>
    [HttpPost("comparison")]
    [ProducesResponseType(typeof(PeriodComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetPeriodComparison(
        [FromBody] PeriodComparisonRequest request,
        CancellationToken cancellationToken = default)
    {
        var periods = request.Periods.Select(p =>
            new PeriodDefinition(p.Label, p.StartDate, p.EndDate)).ToList();

        var query = new GetPeriodComparisonQuery(periods, request.DepartmentId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            success => Ok(success),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Export calculations data to file.
    /// </summary>
    [HttpGet("calculations/export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportCalculations(
        [FromQuery] DateTime periodStart,
        [FromQuery] DateTime periodEnd,
        [FromQuery] ExportFormat format = ExportFormat.Excel,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] Guid? planId = null,
        CancellationToken cancellationToken = default)
    {
        var query = new ExportCalculationsQuery(periodStart, periodEnd, format, departmentId, planId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            export => File(export.Content, export.ContentType, export.FileName),
            error => BadRequest(new { Error = error }));
    }
    /// <summary>
    /// Get available report types.
    /// </summary>
    [HttpGet("types")]
    [ProducesResponseType(typeof(IReadOnlyList<ReportTypeInfoDto>), StatusCodes.Status200OK)]
    public IActionResult GetReportTypes()
    {
        var types = _reportService.GetAvailableReportTypes();
        return Ok(types);
    }

    /// <summary>
    /// Generate a report with specified parameters.
    /// </summary>
    [HttpPost("generate")]
    [ProducesResponseType(typeof(GeneratedReportDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GenerateReport(
        [FromBody] ReportRequestDto request,
        CancellationToken cancellationToken)
    {
        var generatedBy = User.Identity?.Name ?? "System";
        _logger.LogInformation(
            "Generating {ReportType} report for period {Period} by {User}",
            request.ReportType, request.Period, generatedBy);

        var report = await _reportService.GenerateReportAsync(request, generatedBy, cancellationToken);

        if (report.Format != ReportFormats.Json && report.Content != null)
        {
            return File(report.Content, report.ContentType!, report.FileName!);
        }

        return Ok(report);
    }

    /// <summary>
    /// Generate incentive summary report.
    /// </summary>
    [HttpGet("incentive-summary")]
    [ProducesResponseType(typeof(PayoutReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetIncentiveSummary(
        [FromQuery] string period,
        [FromQuery] Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var report = await _reportService.GenerateIncentiveSummaryAsync(
            period, departmentId, cancellationToken);
        return Ok(report);
    }

    /// <summary>
    /// Generate achievement summary report.
    /// </summary>
    [HttpGet("achievement-summary")]
    [ProducesResponseType(typeof(AchievementSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAchievementSummary(
        [FromQuery] string period,
        [FromQuery] Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var report = await _reportService.GenerateAchievementSummaryAsync(
            period, departmentId, cancellationToken);
        return Ok(report);
    }

    /// <summary>
    /// Generate variance analysis report.
    /// </summary>
    [HttpGet("variance-analysis")]
    [ProducesResponseType(typeof(VarianceAnalysisDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVarianceAnalysisReport(
        [FromQuery] string currentPeriod,
        [FromQuery] string? previousPeriod = null,
        [FromQuery] Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var prevPeriod = previousPeriod ?? GetPreviousPeriod(currentPeriod);
        var report = await _reportService.GenerateVarianceAnalysisAsync(
            currentPeriod, prevPeriod, departmentId, cancellationToken);
        return Ok(report);
    }

    /// <summary>
    /// Generate forecast report.
    /// </summary>
    [HttpGet("forecast")]
    [ProducesResponseType(typeof(ForecastReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetForecast(
        [FromQuery] int monthsAhead = 6,
        [FromQuery] Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var report = await _reportService.GenerateForecastAsync(
            monthsAhead, departmentId, cancellationToken);
        return Ok(report);
    }

    /// <summary>
    /// Export report to specified format.
    /// </summary>
    [HttpPost("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportReport(
        [FromBody] ReportRequestDto request,
        [FromQuery] string format = "csv",
        CancellationToken cancellationToken = default)
    {
        var export = await _reportService.ExportReportAsync(request, format, cancellationToken);
        return File(export.Content, export.ContentType, export.FileName);
    }

    /// <summary>
    /// Schedule a report for recurring generation.
    /// </summary>
    [HttpPost("schedules")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(typeof(ReportScheduleDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> ScheduleReport(
        [FromBody] ReportScheduleDto schedule,
        CancellationToken cancellationToken)
    {
        var created = await _reportService.ScheduleReportAsync(schedule, cancellationToken);
        return CreatedAtAction(nameof(GetScheduledReports), new { id = created.ScheduleId }, created);
    }

    /// <summary>
    /// Get all scheduled reports.
    /// </summary>
    [HttpGet("schedules")]
    [ProducesResponseType(typeof(IReadOnlyList<ReportScheduleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetScheduledReports(CancellationToken cancellationToken)
    {
        var schedules = await _reportService.GetScheduledReportsAsync(cancellationToken);
        return Ok(schedules);
    }

    /// <summary>
    /// Cancel a scheduled report.
    /// </summary>
    [HttpDelete("schedules/{scheduleId:guid}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CancelScheduledReport(
        Guid scheduleId,
        CancellationToken cancellationToken)
    {
        await _reportService.CancelScheduledReportAsync(scheduleId, cancellationToken);
        return NoContent();
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

// Request DTOs
public record PeriodComparisonRequest(
    IReadOnlyList<PeriodRequest> Periods,
    Guid? DepartmentId = null);

public record PeriodRequest(
    string Label,
    DateTime StartDate,
    DateTime EndDate);
