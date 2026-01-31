using AutoMapper;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.IncentivePlans.DTOs;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.IncentivePlans.Commands.CreatePlan;

/// <summary>
/// Handler for CreateIncentivePlanCommand.
/// </summary>
public class CreateIncentivePlanCommandHandler : ICommandHandler<CreateIncentivePlanCommand, IncentivePlanDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateIncentivePlanCommandHandler> _logger;

    public CreateIncentivePlanCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateIncentivePlanCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IncentivePlanDto>> Handle(
        CreateIncentivePlanCommand request,
        CancellationToken cancellationToken)
    {
        // Check for duplicate code
        var existingPlan = await _unitOfWork.IncentivePlans.GetByCodeAsync(request.Code, cancellationToken);
        if (existingPlan != null)
        {
            return Result<IncentivePlanDto>.Failure(
                $"Incentive plan with code '{request.Code}' already exists",
                "DUPLICATE_CODE");
        }

        // Create the plan
        var plan = IncentivePlan.Create(
            request.Code,
            request.Name,
            request.PlanType,
            request.Frequency,
            request.EffectiveFrom,
            request.EffectiveTo,
            request.TargetValue,
            request.MinimumThreshold,
            request.AchievementType,
            request.Description,
            request.MetricUnit);

        // Set payout limits
        plan.SetPayoutLimits(request.MaximumPayout, request.MinimumPayout, request.Currency);

        // Configure approval
        plan.ConfigureApproval(request.RequiresApproval, request.ApprovalLevels);

        // Add slabs if provided (only for slab-based plans)
        if (request.Slabs != null && request.Slabs.Count > 0)
        {
            foreach (var slabDto in request.Slabs.OrderBy(s => s.Order))
            {
                plan.AddSlab(
                    slabDto.FromPercentage,
                    slabDto.ToPercentage,
                    slabDto.PayoutPercentage);
            }
        }

        await _unitOfWork.IncentivePlans.AddAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created incentive plan {PlanId} with code {PlanCode}",
            plan.Id, plan.Code);

        return Result<IncentivePlanDto>.Success(_mapper.Map<IncentivePlanDto>(plan));
    }
}
