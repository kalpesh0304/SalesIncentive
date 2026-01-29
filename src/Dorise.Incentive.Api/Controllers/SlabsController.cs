using Dorise.Incentive.Application.IncentivePlans.Commands.ManageSlabs;
using Dorise.Incentive.Application.IncentivePlans.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for managing slabs within incentive plans.
/// "I sleep in a drawer!" - And slabs sleep in tiers!
/// </summary>
[ApiController]
[Route("api/v1/plans/{planId:guid}/slabs")]
[Produces("application/json")]
public class SlabsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<SlabsController> _logger;

    public SlabsController(IMediator mediator, ILogger<SlabsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all slabs for a plan.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<SlabDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSlabs(Guid planId, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting slabs for plan {PlanId}", planId);

        var query = new GetPlanSlabsQuery(planId);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new { Message = $"Plan with ID {planId} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Add a new slab to a plan.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(SlabDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddSlab(
        Guid planId,
        [FromBody] AddSlabRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding slab to plan {PlanId}", planId);

        var command = new AddSlabCommand(
            planId,
            request.Name,
            request.Description,
            request.Order,
            request.FromPercentage,
            request.ToPercentage,
            request.PayoutPercentage,
            request.FixedAmount);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFound(new { Message = result.Error });
            }
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return CreatedAtAction(nameof(GetSlabs), new { planId }, result.Value);
    }

    /// <summary>
    /// Update an existing slab.
    /// </summary>
    [HttpPut("{slabId:guid}")]
    [ProducesResponseType(typeof(SlabDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateSlab(
        Guid planId,
        Guid slabId,
        [FromBody] UpdateSlabRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating slab {SlabId} in plan {PlanId}", slabId, planId);

        var command = new UpdateSlabCommand(
            planId,
            slabId,
            request.Name,
            request.Description,
            request.Order,
            request.FromPercentage,
            request.ToPercentage,
            request.PayoutPercentage,
            request.FixedAmount);

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
    /// Remove a slab from a plan.
    /// </summary>
    [HttpDelete("{slabId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveSlab(
        Guid planId,
        Guid slabId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing slab {SlabId} from plan {PlanId}", slabId, planId);

        var command = new RemoveSlabCommand(planId, slabId);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { Message = result.Error });
        }

        return NoContent();
    }

    /// <summary>
    /// Reorder slabs within a plan.
    /// </summary>
    [HttpPost("reorder")]
    [ProducesResponseType(typeof(IReadOnlyList<SlabDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ReorderSlabs(
        Guid planId,
        [FromBody] ReorderSlabsRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Reordering slabs for plan {PlanId}", planId);

        var command = new ReorderSlabsCommand(planId, request.SlabOrders);
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
}

// Request DTOs
public record AddSlabRequest(
    string Name,
    string? Description,
    int Order,
    decimal FromPercentage,
    decimal ToPercentage,
    decimal PayoutPercentage,
    decimal? FixedAmount);

public record UpdateSlabRequest(
    string Name,
    string? Description,
    int Order,
    decimal FromPercentage,
    decimal ToPercentage,
    decimal PayoutPercentage,
    decimal? FixedAmount);

public record ReorderSlabsRequest(IReadOnlyList<SlabOrderDto> SlabOrders);
public record SlabOrderDto(Guid SlabId, int NewOrder);
