using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.IncentivePlans.Commands.ActivatePlan;
using Dorise.Incentive.Application.IncentivePlans.Commands.CreatePlan;
using Dorise.Incentive.Application.IncentivePlans.Commands.UpdatePlan;
using Dorise.Incentive.Application.IncentivePlans.DTOs;
using Dorise.Incentive.Application.IncentivePlans.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for Incentive Plan management.
/// "When I grow up, I want to be a principal or a caterpillar!" - Plans help dreams become reality!
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class IncentivePlansController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<IncentivePlansController> _logger;

    public IncentivePlansController(IMediator mediator, ILogger<IncentivePlansController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all incentive plans with filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<IncentivePlanSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlans(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? planType = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting incentive plans - Page: {Page}, Status: {Status}, Type: {Type}",
            page, status, planType);

        var query = new GetIncentivePlansQuery(page, pageSize, status, planType, search);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get incentive plan by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(IncentivePlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlan(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting incentive plan {PlanId}", id);

        var query = new GetIncentivePlanByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new { Message = $"Incentive plan with ID {id} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get incentive plan by code.
    /// </summary>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(IncentivePlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlanByCode(string code, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting incentive plan by code {PlanCode}", code);

        var query = new GetIncentivePlanByCodeQuery(code);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new { Message = $"Incentive plan with code {code} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get plan with all slabs.
    /// </summary>
    [HttpGet("{id:guid}/with-slabs")]
    [ProducesResponseType(typeof(IncentivePlanWithSlabsDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlanWithSlabs(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting incentive plan {PlanId} with slabs", id);

        var query = new GetIncentivePlanWithSlabsQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new { Message = $"Incentive plan with ID {id} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get active plans effective on a specific date.
    /// </summary>
    [HttpGet("active")]
    [ProducesResponseType(typeof(IReadOnlyList<IncentivePlanSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActivePlans(
        [FromQuery] DateTime? effectiveDate = null,
        CancellationToken cancellationToken = default)
    {
        var date = effectiveDate ?? DateTime.UtcNow.Date;
        _logger.LogInformation("Getting active plans effective on {Date}", date);

        var query = new GetActivePlansQuery(date);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Create a new incentive plan.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(IncentivePlanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePlan(
        [FromBody] CreateIncentivePlanCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating incentive plan with code {PlanCode}", command.Code);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return CreatedAtAction(
            nameof(GetPlan),
            new { id = result.Value!.Id },
            result.Value);
    }

    /// <summary>
    /// Update an existing incentive plan.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(IncentivePlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePlan(
        Guid id,
        [FromBody] UpdateIncentivePlanCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { Error = "ID mismatch" });
        }

        _logger.LogInformation("Updating incentive plan {PlanId}", id);

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
    /// Activate an incentive plan.
    /// </summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(typeof(IncentivePlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ActivatePlan(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Activating incentive plan {PlanId}", id);

        var command = new ActivateIncentivePlanCommand(id);
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
    /// Suspend an incentive plan.
    /// </summary>
    [HttpPost("{id:guid}/suspend")]
    [ProducesResponseType(typeof(IncentivePlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SuspendPlan(
        Guid id,
        [FromBody] SuspendPlanRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Suspending incentive plan {PlanId}", id);

        var command = new SuspendIncentivePlanCommand(id, request.Reason);
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
    /// Cancel an incentive plan.
    /// </summary>
    [HttpPost("{id:guid}/cancel")]
    [ProducesResponseType(typeof(IncentivePlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelPlan(
        Guid id,
        [FromBody] CancelPlanRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling incentive plan {PlanId}", id);

        var command = new CancelIncentivePlanCommand(id, request.Reason);
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
    /// Clone an existing plan.
    /// </summary>
    [HttpPost("{id:guid}/clone")]
    [ProducesResponseType(typeof(IncentivePlanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ClonePlan(
        Guid id,
        [FromBody] ClonePlanRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cloning incentive plan {PlanId} as {NewCode}", id, request.NewCode);

        var command = new CloneIncentivePlanCommand(id, request.NewCode, request.NewName);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFound(new { Message = result.Error });
            }
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return CreatedAtAction(nameof(GetPlan), new { id = result.Value!.Id }, result.Value);
    }

    /// <summary>
    /// Validate plan configuration.
    /// </summary>
    [HttpPost("{id:guid}/validate")]
    [ProducesResponseType(typeof(PlanValidationResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidatePlan(
        Guid id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Validating incentive plan {PlanId}", id);

        var query = new ValidateIncentivePlanQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new { Message = $"Incentive plan with ID {id} not found" });
        }

        return Ok(result);
    }
}

// Request DTOs
public record SuspendPlanRequest(string Reason);
public record CancelPlanRequest(string Reason);
public record ClonePlanRequest(string NewCode, string NewName);
