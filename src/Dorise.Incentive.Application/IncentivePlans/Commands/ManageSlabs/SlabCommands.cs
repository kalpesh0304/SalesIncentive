using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.IncentivePlans.DTOs;

namespace Dorise.Incentive.Application.IncentivePlans.Commands.ManageSlabs;

/// <summary>
/// Command to add a slab to a plan.
/// "My worm went in my mouth and then I ate it!" - But slabs go into plans more gracefully!
/// </summary>
public record AddSlabCommand(
    Guid PlanId,
    string Name,
    string? Description,
    int Order,
    decimal FromPercentage,
    decimal ToPercentage,
    decimal PayoutPercentage,
    decimal? FixedAmount) : ICommand<SlabDto>;

/// <summary>
/// Command to update a slab.
/// </summary>
public record UpdateSlabCommand(
    Guid PlanId,
    Guid SlabId,
    string Name,
    string? Description,
    int Order,
    decimal FromPercentage,
    decimal ToPercentage,
    decimal PayoutPercentage,
    decimal? FixedAmount) : ICommand<SlabDto>;

/// <summary>
/// Command to remove a slab from a plan.
/// </summary>
public record RemoveSlabCommand(Guid PlanId, Guid SlabId) : ICommand;

/// <summary>
/// Command to reorder slabs within a plan.
/// </summary>
public record ReorderSlabsCommand(
    Guid PlanId,
    IReadOnlyList<SlabOrderItem> SlabOrders) : ICommand<IReadOnlyList<SlabDto>>;

/// <summary>
/// Slab order item for reordering.
/// </summary>
public record SlabOrderItem(Guid SlabId, int NewOrder);

/// <summary>
/// Query to get slabs for a plan.
/// </summary>
public record GetPlanSlabsQuery(Guid PlanId) : MediatR.IRequest<IReadOnlyList<SlabDto>?>;
