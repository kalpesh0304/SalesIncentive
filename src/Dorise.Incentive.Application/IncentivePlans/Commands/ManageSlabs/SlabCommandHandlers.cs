using AutoMapper;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.IncentivePlans.DTOs;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.IncentivePlans.Commands.ManageSlabs;

/// <summary>
/// Handler for AddSlabCommand.
/// </summary>
public class AddSlabCommandHandler : ICommandHandler<AddSlabCommand, SlabDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AddSlabCommandHandler> _logger;

    public AddSlabCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AddSlabCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<SlabDto>> Handle(AddSlabCommand request, CancellationToken cancellationToken)
    {
        var plan = await _unitOfWork.IncentivePlans.GetWithSlabsAsync(request.PlanId, cancellationToken);

        if (plan == null)
        {
            return Result<SlabDto>.NotFound("IncentivePlan", request.PlanId);
        }

        if (plan.Status != PlanStatus.Draft)
        {
            return Result<SlabDto>.Failure(
                "Can only add slabs to Draft plans",
                "INVALID_STATUS");
        }

        // Check for overlapping ranges
        foreach (var existingSlab in plan.Slabs)
        {
            if (RangesOverlap(request.FromPercentage, request.ToPercentage,
                existingSlab.FromPercentage.Value, existingSlab.ToPercentage.Value))
            {
                return Result<SlabDto>.Failure(
                    $"Slab range overlaps with existing slab '{existingSlab.Name}'",
                    "OVERLAPPING_RANGE");
            }
        }

        var slab = Slab.Create(
            request.Name,
            request.Description,
            request.Order,
            Percentage.Create(request.FromPercentage),
            Percentage.Create(request.ToPercentage),
            Percentage.Create(request.PayoutPercentage),
            request.FixedAmount.HasValue ? Money.Create(request.FixedAmount.Value, "INR") : null);

        plan.AddSlab(slab);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Added slab {SlabId} to plan {PlanId}", slab.Id, plan.Id);

        return Result<SlabDto>.Success(_mapper.Map<SlabDto>(slab));
    }

    private bool RangesOverlap(decimal from1, decimal to1, decimal from2, decimal to2)
    {
        return from1 < to2 && from2 < to1;
    }
}

/// <summary>
/// Handler for UpdateSlabCommand.
/// </summary>
public class UpdateSlabCommandHandler : ICommandHandler<UpdateSlabCommand, SlabDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateSlabCommandHandler> _logger;

    public UpdateSlabCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateSlabCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<SlabDto>> Handle(UpdateSlabCommand request, CancellationToken cancellationToken)
    {
        var plan = await _unitOfWork.IncentivePlans.GetWithSlabsAsync(request.PlanId, cancellationToken);

        if (plan == null)
        {
            return Result<SlabDto>.NotFound("IncentivePlan", request.PlanId);
        }

        var slab = plan.Slabs.FirstOrDefault(s => s.Id == request.SlabId);
        if (slab == null)
        {
            return Result<SlabDto>.NotFound("Slab", request.SlabId);
        }

        if (plan.Status != PlanStatus.Draft)
        {
            return Result<SlabDto>.Failure(
                "Can only update slabs in Draft plans",
                "INVALID_STATUS");
        }

        slab.Update(
            request.Name,
            request.Description,
            request.Order,
            Percentage.Create(request.FromPercentage),
            Percentage.Create(request.ToPercentage),
            Percentage.Create(request.PayoutPercentage),
            request.FixedAmount.HasValue ? Money.Create(request.FixedAmount.Value, "INR") : null);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated slab {SlabId} in plan {PlanId}", slab.Id, plan.Id);

        return Result<SlabDto>.Success(_mapper.Map<SlabDto>(slab));
    }
}

/// <summary>
/// Handler for RemoveSlabCommand.
/// </summary>
public class RemoveSlabCommandHandler : ICommandHandler<RemoveSlabCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RemoveSlabCommandHandler> _logger;

    public RemoveSlabCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<RemoveSlabCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(RemoveSlabCommand request, CancellationToken cancellationToken)
    {
        var plan = await _unitOfWork.IncentivePlans.GetWithSlabsAsync(request.PlanId, cancellationToken);

        if (plan == null)
        {
            return Result.NotFound("IncentivePlan", request.PlanId);
        }

        var slab = plan.Slabs.FirstOrDefault(s => s.Id == request.SlabId);
        if (slab == null)
        {
            return Result.NotFound("Slab", request.SlabId);
        }

        if (plan.Status != PlanStatus.Draft)
        {
            return Result.Failure("Can only remove slabs from Draft plans", "INVALID_STATUS");
        }

        plan.RemoveSlab(slab);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Removed slab {SlabId} from plan {PlanId}", request.SlabId, plan.Id);

        return Result.Success();
    }
}

/// <summary>
/// Handler for ReorderSlabsCommand.
/// </summary>
public class ReorderSlabsCommandHandler : ICommandHandler<ReorderSlabsCommand, IReadOnlyList<SlabDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ReorderSlabsCommandHandler> _logger;

    public ReorderSlabsCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ReorderSlabsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<SlabDto>>> Handle(
        ReorderSlabsCommand request,
        CancellationToken cancellationToken)
    {
        var plan = await _unitOfWork.IncentivePlans.GetWithSlabsAsync(request.PlanId, cancellationToken);

        if (plan == null)
        {
            return Result<IReadOnlyList<SlabDto>>.NotFound("IncentivePlan", request.PlanId);
        }

        if (plan.Status != PlanStatus.Draft)
        {
            return Result<IReadOnlyList<SlabDto>>.Failure(
                "Can only reorder slabs in Draft plans",
                "INVALID_STATUS");
        }

        foreach (var orderItem in request.SlabOrders)
        {
            var slab = plan.Slabs.FirstOrDefault(s => s.Id == orderItem.SlabId);
            if (slab != null)
            {
                slab.SetOrder(orderItem.NewOrder);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reordered slabs for plan {PlanId}", plan.Id);

        var orderedSlabs = plan.Slabs.OrderBy(s => s.Order).ToList();
        return Result<IReadOnlyList<SlabDto>>.Success(_mapper.Map<IReadOnlyList<SlabDto>>(orderedSlabs));
    }
}

/// <summary>
/// Handler for GetPlanSlabsQuery.
/// </summary>
public class GetPlanSlabsQueryHandler : IRequestHandler<GetPlanSlabsQuery, IReadOnlyList<SlabDto>?>
{
    private readonly IReadOnlyRepository _repository;
    private readonly IMapper _mapper;

    public GetPlanSlabsQueryHandler(IReadOnlyRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<SlabDto>?> Handle(
        GetPlanSlabsQuery request,
        CancellationToken cancellationToken)
    {
        var plan = await _repository.IncentivePlans.GetWithSlabsAsync(request.PlanId, cancellationToken);

        if (plan == null)
        {
            return null;
        }

        var orderedSlabs = plan.Slabs.OrderBy(s => s.Order).ToList();
        return _mapper.Map<IReadOnlyList<SlabDto>>(orderedSlabs);
    }
}
