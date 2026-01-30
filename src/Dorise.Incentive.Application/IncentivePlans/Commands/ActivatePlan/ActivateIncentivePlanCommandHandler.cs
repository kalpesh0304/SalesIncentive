using AutoMapper;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.IncentivePlans.DTOs;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.IncentivePlans.Commands.ActivatePlan;

/// <summary>
/// Handler for ActivateIncentivePlanCommand.
/// </summary>
public class ActivateIncentivePlanCommandHandler : ICommandHandler<ActivateIncentivePlanCommand, IncentivePlanDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ActivateIncentivePlanCommandHandler> _logger;

    public ActivateIncentivePlanCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ActivateIncentivePlanCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IncentivePlanDto>> Handle(
        ActivateIncentivePlanCommand request,
        CancellationToken cancellationToken)
    {
        var plan = await _unitOfWork.IncentivePlans.GetWithSlabsAsync(request.Id, cancellationToken);

        if (plan == null)
        {
            return Result<IncentivePlanDto>.NotFound("IncentivePlan", request.Id);
        }

        // Validate plan can be activated
        if (plan.Status != PlanStatus.Draft && plan.Status != PlanStatus.Suspended)
        {
            return Result<IncentivePlanDto>.Failure(
                $"Cannot activate plan in {plan.Status} status. Only Draft or Suspended plans can be activated.",
                "INVALID_STATUS");
        }

        // Validate plan has required configuration
        var validationErrors = ValidatePlanForActivation(plan);
        if (validationErrors.Count > 0)
        {
            return Result<IncentivePlanDto>.Failure(
                $"Plan cannot be activated: {string.Join("; ", validationErrors)}",
                "VALIDATION_FAILED");
        }

        plan.Activate();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Activated incentive plan {PlanId}", plan.Id);

        return Result<IncentivePlanDto>.Success(_mapper.Map<IncentivePlanDto>(plan));
    }

    private List<string> ValidatePlanForActivation(Domain.Entities.IncentivePlan plan)
    {
        var errors = new List<string>();

        // Check effective period
        if (plan.EffectivePeriod.EndDate <= DateTime.UtcNow.Date)
        {
            errors.Add("Plan effective period has already ended");
        }

        // Check for slab-based plans
        if (plan.PlanType == PlanType.SlabBased && (plan.Slabs == null || plan.Slabs.Count == 0))
        {
            errors.Add("Slab-based plans must have at least one slab configured");
        }

        // Validate slabs cover full range
        if (plan.PlanType == PlanType.SlabBased && plan.Slabs?.Count > 0)
        {
            var orderedSlabs = plan.Slabs.OrderBy(s => s.Order).ToList();

            // Check first slab starts at minimum threshold or 0
            if (orderedSlabs[0].FromPercentage > plan.Target.MinimumThreshold)
            {
                errors.Add($"First slab should start at or below minimum threshold ({plan.Target.MinimumThreshold}%)");
            }
        }

        return errors;
    }
}

/// <summary>
/// Handler for SuspendIncentivePlanCommand.
/// </summary>
public class SuspendIncentivePlanCommandHandler : ICommandHandler<SuspendIncentivePlanCommand, IncentivePlanDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<SuspendIncentivePlanCommandHandler> _logger;

    public SuspendIncentivePlanCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<SuspendIncentivePlanCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IncentivePlanDto>> Handle(
        SuspendIncentivePlanCommand request,
        CancellationToken cancellationToken)
    {
        var plan = await _unitOfWork.IncentivePlans.GetByIdAsync(request.Id, cancellationToken);

        if (plan == null)
        {
            return Result<IncentivePlanDto>.NotFound("IncentivePlan", request.Id);
        }

        if (plan.Status != PlanStatus.Active)
        {
            return Result<IncentivePlanDto>.Failure(
                $"Cannot suspend plan in {plan.Status} status. Only Active plans can be suspended.",
                "INVALID_STATUS");
        }

        plan.Suspend(request.Reason);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Suspended incentive plan {PlanId}. Reason: {Reason}", plan.Id, request.Reason);

        return Result<IncentivePlanDto>.Success(_mapper.Map<IncentivePlanDto>(plan));
    }
}

/// <summary>
/// Handler for CancelIncentivePlanCommand.
/// </summary>
public class CancelIncentivePlanCommandHandler : ICommandHandler<CancelIncentivePlanCommand, IncentivePlanDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CancelIncentivePlanCommandHandler> _logger;

    public CancelIncentivePlanCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CancelIncentivePlanCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IncentivePlanDto>> Handle(
        CancelIncentivePlanCommand request,
        CancellationToken cancellationToken)
    {
        var plan = await _unitOfWork.IncentivePlans.GetByIdAsync(request.Id, cancellationToken);

        if (plan == null)
        {
            return Result<IncentivePlanDto>.NotFound("IncentivePlan", request.Id);
        }

        if (plan.Status == PlanStatus.Cancelled)
        {
            return Result<IncentivePlanDto>.Failure("Plan is already cancelled", "ALREADY_CANCELLED");
        }

        plan.Cancel(request.Reason);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cancelled incentive plan {PlanId}. Reason: {Reason}", plan.Id, request.Reason);

        return Result<IncentivePlanDto>.Success(_mapper.Map<IncentivePlanDto>(plan));
    }
}

/// <summary>
/// Handler for CloneIncentivePlanCommand.
/// </summary>
public class CloneIncentivePlanCommandHandler : ICommandHandler<CloneIncentivePlanCommand, IncentivePlanDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CloneIncentivePlanCommandHandler> _logger;

    public CloneIncentivePlanCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CloneIncentivePlanCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IncentivePlanDto>> Handle(
        CloneIncentivePlanCommand request,
        CancellationToken cancellationToken)
    {
        var sourcePlan = await _unitOfWork.IncentivePlans.GetWithSlabsAsync(request.SourceId, cancellationToken);

        if (sourcePlan == null)
        {
            return Result<IncentivePlanDto>.NotFound("IncentivePlan", request.SourceId);
        }

        // Check new code doesn't exist
        var existingPlan = await _unitOfWork.IncentivePlans.GetByCodeAsync(request.NewCode, cancellationToken);
        if (existingPlan != null)
        {
            return Result<IncentivePlanDto>.Failure(
                $"Plan with code '{request.NewCode}' already exists",
                "DUPLICATE_CODE");
        }

        // Clone the plan
        var clonedPlan = sourcePlan.Clone(request.NewCode, request.NewName);

        await _unitOfWork.IncentivePlans.AddAsync(clonedPlan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Cloned incentive plan {SourcePlanId} to new plan {NewPlanId} with code {NewCode}",
            sourcePlan.Id, clonedPlan.Id, request.NewCode);

        return Result<IncentivePlanDto>.Success(_mapper.Map<IncentivePlanDto>(clonedPlan));
    }
}
