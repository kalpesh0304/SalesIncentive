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
[Route("api/v1/[controller]")]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
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
}
