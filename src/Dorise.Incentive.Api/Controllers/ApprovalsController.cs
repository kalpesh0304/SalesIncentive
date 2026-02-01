using Dorise.Incentive.Application.Approvals.Commands;
using Dorise.Incentive.Application.Approvals.DTOs;
using Dorise.Incentive.Application.Approvals.Queries;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for approval workflow operations.
/// "I'm learnding!" - Approving the learning process!
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class ApprovalsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public ApprovalsController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Get my pending approvals.
    /// </summary>
    [HttpGet("pending")]
    [ProducesResponseType(typeof(PagedApprovalResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPendingApprovals(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var approverId = _currentUser.UserId != null ? Guid.Parse(_currentUser.UserId) : Guid.Empty;
        var query = new GetPendingApprovalsForUserQuery(approverId, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            success => Ok(success),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Get my approval history.
    /// </summary>
    [HttpGet("history")]
    [ProducesResponseType(typeof(PagedApprovalResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyApprovalHistory(
        [FromQuery] ApprovalStatus? status = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var approverId = _currentUser.UserId != null ? Guid.Parse(_currentUser.UserId) : Guid.Empty;
        var query = new GetApprovalHistoryQuery(approverId, status, page, pageSize);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            success => Ok(success),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Get approval dashboard (counts by status).
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApprovalDashboardDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApprovalDashboard(CancellationToken cancellationToken = default)
    {
        var approverId = _currentUser.UserId != null ? Guid.Parse(_currentUser.UserId) : Guid.Empty;
        var query = new GetApprovalDashboardQuery(approverId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            success => Ok(success),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Get a specific approval request.
    /// </summary>
    [HttpGet("{approvalId:guid}")]
    [ProducesResponseType(typeof(ApprovalDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetApproval(
        Guid approvalId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetApprovalByIdQuery(approvalId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            approval => approval != null ? Ok(approval) : NotFound(),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Approve a calculation.
    /// </summary>
    [HttpPost("{approvalId:guid}/approve")]
    [ProducesResponseType(typeof(ApprovalResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Approve(
        Guid approvalId,
        [FromBody] ApproveRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new ApproveCommand(approvalId, request.Comments);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            success => Ok(success),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Reject a calculation.
    /// </summary>
    [HttpPost("{approvalId:guid}/reject")]
    [ProducesResponseType(typeof(ApprovalResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Reject(
        Guid approvalId,
        [FromBody] RejectRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new RejectCommand(approvalId, request.Reason);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            success => Ok(success),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Bulk approve multiple calculations.
    /// </summary>
    [HttpPost("bulk-approve")]
    [ProducesResponseType(typeof(BulkApprovalResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkApprove(
        [FromBody] BulkApproveRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new BulkApproveCommand(request.ApprovalIds, request.Comments);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            success => Ok(success),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Delegate approval to another user.
    /// </summary>
    [HttpPost("{approvalId:guid}/delegate")]
    [ProducesResponseType(typeof(ApprovalResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Delegate(
        Guid approvalId,
        [FromBody] DelegateRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new DelegateApprovalCommand(approvalId, request.DelegateToId);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            success => Ok(success),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Escalate an approval to the next level.
    /// </summary>
    [HttpPost("{approvalId:guid}/escalate")]
    [ProducesResponseType(typeof(ApprovalResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Escalate(
        Guid approvalId,
        [FromBody] EscalateRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new EscalateApprovalCommand(approvalId, request.Reason);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            success => Ok(success),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Submit calculations for approval.
    /// </summary>
    [HttpPost("submit")]
    [ProducesResponseType(typeof(SubmissionResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitForApproval(
        [FromBody] SubmitForApprovalRequest request,
        CancellationToken cancellationToken = default)
    {
        var command = new SubmitForApprovalCommand(request.CalculationIds);
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match<IActionResult>(
            success => Ok(success),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Get approvals for a specific calculation.
    /// </summary>
    [HttpGet("calculation/{calculationId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<ApprovalDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApprovalsForCalculation(
        Guid calculationId,
        CancellationToken cancellationToken = default)
    {
        var query = new GetApprovalsForCalculationQuery(calculationId);
        var result = await _mediator.Send(query, cancellationToken);

        return result.Match<IActionResult>(
            success => Ok(success),
            error => BadRequest(new { Error = error }));
    }
}

// Request DTOs
public record ApproveRequest(string? Comments);
public record RejectRequest(string Reason);
public record BulkApproveRequest(IReadOnlyList<Guid> ApprovalIds, string? Comments);
public record DelegateRequest(Guid DelegateToId);
public record EscalateRequest(string Reason);
public record SubmitForApprovalRequest(IReadOnlyList<Guid> CalculationIds);
