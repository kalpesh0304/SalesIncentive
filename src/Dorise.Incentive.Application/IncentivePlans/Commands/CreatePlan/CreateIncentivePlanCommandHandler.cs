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
            request.Description,
            request.PlanType,
            request.Frequency,
            DateRange.Create(request.EffectiveFrom, request.EffectiveTo),
            Target.Create(
                request.TargetValue,
                request.MinimumThreshold,
                request.AchievementType,
                request.MetricUnit),
            request.MaximumPayout.HasValue ? Money.Create(request.MaximumPayout.Value, request.Currency) : null,
            request.MinimumPayout.HasValue ? Money.Create(request.MinimumPayout.Value, request.Currency) : null,
            request.RequiresApproval,
            request.ApprovalLevels,
            request.MinimumTenureDays,
            request.EligibilityCriteria);

        // Add slabs if provided
        if (request.Slabs != null && request.Slabs.Count > 0)
        {
            foreach (var slabDto in request.Slabs.OrderBy(s => s.Order))
            {
                var slab = Slab.Create(
                    slabDto.Name,
                    slabDto.Description,
                    slabDto.Order,
                    Percentage.Create(slabDto.FromPercentage),
                    Percentage.Create(slabDto.ToPercentage),
                    Percentage.Create(slabDto.PayoutPercentage),
                    slabDto.FixedAmount.HasValue ? Money.Create(slabDto.FixedAmount.Value, request.Currency) : null);

                plan.AddSlab(slab);
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
