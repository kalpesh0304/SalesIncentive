using Dorise.Incentive.Application.PlanAssignments.Commands.AssignPlan;
using Dorise.Incentive.Application.PlanAssignments.Commands.UnassignPlan;
using Dorise.Incentive.Application.PlanAssignments.DTOs;
using Dorise.Incentive.Application.PlanAssignments.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for managing plan assignments to employees.
/// "I bent my Wookiee!" - But we won't bend assignments, we manage them properly!
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class PlanAssignmentsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<PlanAssignmentsController> _logger;

    public PlanAssignmentsController(IMediator mediator, ILogger<PlanAssignmentsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all plan assignments with filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PlanAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAssignments(
        [FromQuery] Guid? employeeId = null,
        [FromQuery] Guid? planId = null,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting plan assignments - EmployeeId: {EmployeeId}, PlanId: {PlanId}, ActiveOnly: {ActiveOnly}",
            employeeId, planId, activeOnly);

        var query = new GetPlanAssignmentsQuery(employeeId, planId, activeOnly);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get assignment by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PlanAssignmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAssignment(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetPlanAssignmentByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new { Message = $"Assignment with ID {id} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get assignments for an employee.
    /// </summary>
    [HttpGet("employee/{employeeId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<PlanAssignmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeeAssignments(
        Guid employeeId,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting assignments for employee {EmployeeId}", employeeId);

        var query = new GetEmployeeAssignmentsQuery(employeeId, activeOnly);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Assign an incentive plan to an employee.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PlanAssignmentDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignPlan(
        [FromBody] AssignPlanToEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Assigning plan {PlanId} to employee {EmployeeId}",
            command.IncentivePlanId, command.EmployeeId);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return CreatedAtAction(
            nameof(GetAssignment),
            new { id = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Bulk assign a plan to multiple employees.
    /// </summary>
    [HttpPost("bulk")]
    [ProducesResponseType(typeof(BulkAssignmentResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkAssignPlan(
        [FromBody] BulkAssignPlanCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Bulk assigning plan {PlanId} to {Count} employees",
            command.IncentivePlanId, command.EmployeeIds.Count);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Update an existing assignment.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(PlanAssignmentDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAssignment(
        Guid id,
        [FromBody] UpdatePlanAssignmentCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { Error = "ID mismatch" });
        }

        _logger.LogInformation("Updating assignment {AssignmentId}", id);

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
    /// End/unassign a plan from an employee.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnassignPlan(
        Guid id,
        [FromQuery] DateTime? effectiveDate = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Unassigning plan assignment {AssignmentId}", id);

        var command = new UnassignPlanCommand(id, effectiveDate ?? DateTime.UtcNow.Date);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { Message = result.Error });
        }

        return NoContent();
    }

    /// <summary>
    /// Check if an employee is eligible for a plan.
    /// </summary>
    [HttpGet("eligibility")]
    [ProducesResponseType(typeof(EligibilityCheckResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckEligibility(
        [FromQuery] Guid employeeId,
        [FromQuery] Guid planId,
        [FromQuery] DateTime? asOfDate = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Checking eligibility for employee {EmployeeId} and plan {PlanId}",
            employeeId, planId);

        var query = new CheckPlanEligibilityQuery(employeeId, planId, asOfDate ?? DateTime.UtcNow.Date);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }
}
