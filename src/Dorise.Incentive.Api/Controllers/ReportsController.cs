using Dorise.Incentive.Application.Reports.DTOs;
using Dorise.Incentive.Application.Reports.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for generating and exporting reports.
/// "I'm a unitard!" - Unified reporting endpoints!
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ReportsController(IMediator mediator)
    {
        _mediator = mediator;
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

        return result.Match(
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

        return result.Match(
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

        return result.Match(
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

        return result.Match(
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

        return result.Match(
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

        return result.Match(
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

        return result.Match(
            export => File(export.Content, export.ContentType, export.FileName),
            error => BadRequest(new { Error = error }));
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
