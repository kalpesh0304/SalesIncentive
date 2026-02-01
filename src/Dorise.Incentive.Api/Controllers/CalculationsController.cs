using Dorise.Incentive.Application.Calculations.Commands.AdjustCalculation;
using Dorise.Incentive.Application.Calculations.Commands.ApproveCalculation;
using Dorise.Incentive.Application.Calculations.Commands.BatchCalculation;
using Dorise.Incentive.Application.Calculations.Commands.RunCalculation;
using Dorise.Incentive.Application.Calculations.Commands.SubmitForApproval;
using Dorise.Incentive.Application.Calculations.Commands.VoidCalculation;
using Dorise.Incentive.Application.Calculations.DTOs;
using Dorise.Incentive.Application.Calculations.Queries;
using Dorise.Incentive.Application.Common.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using BatchCalculationResultDto = Dorise.Incentive.Application.Calculations.DTOs.BatchCalculationResultDto;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for Incentive Calculation operations.
/// "I'm a star! I'm a star!" - And calculations make stars shine with incentives!
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class CalculationsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CalculationsController> _logger;

    public CalculationsController(IMediator mediator, ILogger<CalculationsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get calculations with filtering and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<CalculationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCalculations(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] Guid? employeeId = null,
        [FromQuery] Guid? planId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? periodStart = null,
        [FromQuery] DateTime? periodEnd = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting calculations - Page: {Page}, EmployeeId: {EmployeeId}, Status: {Status}",
            page, employeeId, status);

        var query = new GetCalculationsQuery(page, pageSize, employeeId, planId, status, periodStart, periodEnd);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get calculation by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CalculationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCalculation(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting calculation {CalculationId}", id);

        var query = new GetCalculationByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new { Message = $"Calculation with ID {id} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get calculations for an employee.
    /// </summary>
    [HttpGet("employee/{employeeId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<CalculationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeCalculations(
        Guid employeeId,
        [FromQuery] DateTime? periodStart = null,
        [FromQuery] DateTime? periodEnd = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting calculations for employee {EmployeeId}", employeeId);

        var query = new GetEmployeeCalculationsQuery(employeeId, periodStart, periodEnd);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get calculation summary for a period.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(CalculationPeriodSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPeriodSummary(
        [FromQuery] DateTime periodStart,
        [FromQuery] DateTime periodEnd,
        [FromQuery] Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting calculation summary for period {Start} to {End}",
            periodStart, periodEnd);

        var query = new GetCalculationPeriodSummaryQuery(periodStart, periodEnd, departmentId);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Run calculation for a single employee.
    /// </summary>
    [HttpPost("run")]
    [ProducesResponseType(typeof(CalculationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RunCalculation(
        [FromBody] RunCalculationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Running calculation for employee {EmployeeId} with plan {PlanId}",
            request.EmployeeId, request.IncentivePlanId);

        var command = new RunCalculationCommand(
            request.EmployeeId,
            request.IncentivePlanId,
            request.PeriodStart,
            request.PeriodEnd,
            request.ActualValue,
            request.Notes);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return CreatedAtAction(
            nameof(GetCalculation),
            new { id = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Run batch calculation for multiple employees.
    /// </summary>
    [HttpPost("batch")]
    [ProducesResponseType(typeof(BatchCalculationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RunBatchCalculation(
        [FromBody] BatchCalculationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Running batch calculation for {Count} employees, period {Start} to {End}",
            request.EmployeeAchievements?.Count ?? 0, request.PeriodStart, request.PeriodEnd);

        var command = new RunBatchCalculationCommand(
            request.PeriodStart,
            request.PeriodEnd,
            request.IncentivePlanId,
            request.DepartmentId,
            request.EmployeeAchievements);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Preview calculation without saving.
    /// </summary>
    [HttpPost("preview")]
    [ProducesResponseType(typeof(CalculationPreviewDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> PreviewCalculation(
        [FromBody] RunCalculationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Previewing calculation for employee {EmployeeId}",
            request.EmployeeId);

        var query = new PreviewCalculationQuery(
            request.EmployeeId,
            request.IncentivePlanId,
            request.PeriodStart,
            request.PeriodEnd,
            request.ActualValue);

        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return BadRequest(new { Error = "Unable to preview calculation" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Submit calculation for approval.
    /// </summary>
    [HttpPost("{id:guid}/submit")]
    [ProducesResponseType(typeof(CalculationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SubmitForApproval(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Submitting calculation {CalculationId} for approval", id);

        var command = new SubmitCalculationForApprovalCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFound(new { Message = result.Error });
            }
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return Ok(new { Message = "Calculation submitted for approval" });
    }

    /// <summary>
    /// Approve a calculation.
    /// </summary>
    [HttpPost("{id:guid}/approve")]
    [ProducesResponseType(typeof(CalculationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ApproveCalculation(
        Guid id,
        [FromBody] ApprovalRequest? request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approving calculation {CalculationId}", id);

        var command = new ApproveCalculationCommand(id, request?.Comments);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFound(new { Message = result.Error });
            }
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Reject a calculation.
    /// </summary>
    [HttpPost("{id:guid}/reject")]
    [ProducesResponseType(typeof(CalculationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RejectCalculation(
        Guid id,
        [FromBody] CalculationRejectRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Rejecting calculation {CalculationId}", id);

        var command = new RejectCalculationCommand(id, request.Reason);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFound(new { Message = result.Error });
            }
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Adjust a calculation.
    /// </summary>
    [HttpPost("{id:guid}/adjust")]
    [ProducesResponseType(typeof(CalculationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AdjustCalculation(
        Guid id,
        [FromBody] AdjustmentRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adjusting calculation {CalculationId}", id);

        var command = new AdjustCalculationCommand(
            id,
            request.NewAmount,
            request.Reason);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFound(new { Message = result.Error });
            }
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Void a calculation.
    /// </summary>
    [HttpPost("{id:guid}/void")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> VoidCalculation(
        Guid id,
        [FromBody] VoidRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Voiding calculation {CalculationId}", id);

        var command = new VoidCalculationCommand(id, request.Reason);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFound(new { Message = result.Error });
            }
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return NoContent();
    }

    /// <summary>
    /// Recalculate an existing calculation.
    /// </summary>
    [HttpPost("{id:guid}/recalculate")]
    [ProducesResponseType(typeof(CalculationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Recalculate(
        Guid id,
        [FromBody] RecalculateRequest? request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recalculating calculation {CalculationId}", id);

        var command = new RecalculateCommand(id, request?.NewActualValue);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFound(new { Message = result.Error });
            }
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Get pending approvals for current user.
    /// </summary>
    [HttpGet("pending-approvals")]
    [ProducesResponseType(typeof(IReadOnlyList<CalculationSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingApprovals(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting pending approvals");

        var query = new GetPendingApprovalsQuery();
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }
}

// Request DTOs
public record RunCalculationRequest(
    Guid EmployeeId,
    Guid IncentivePlanId,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal ActualValue,
    string? Notes = null);

public record BatchCalculationRequest(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    Guid? IncentivePlanId,
    Guid? DepartmentId,
    IReadOnlyList<EmployeeAchievementItem>? EmployeeAchievements);

public record EmployeeAchievementItem(Guid EmployeeId, decimal ActualValue);

public record ApprovalRequest(string? Comments);
public record CalculationRejectRequest(string Reason);
public record AdjustmentRequest(decimal NewAmount, string Reason);
public record VoidRequest(string Reason);
public record RecalculateRequest(decimal? NewActualValue);
