using AutoMapper;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.IncentivePlans.DTOs;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.IncentivePlans.Commands.UpdatePlan;

/// <summary>
/// Handler for UpdateIncentivePlanCommand.
/// </summary>
public class UpdateIncentivePlanCommandHandler : ICommandHandler<UpdateIncentivePlanCommand, IncentivePlanDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<UpdateIncentivePlanCommandHandler> _logger;

    public UpdateIncentivePlanCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<UpdateIncentivePlanCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<IncentivePlanDto>> Handle(
        UpdateIncentivePlanCommand request,
        CancellationToken cancellationToken)
    {
        var plan = await _unitOfWork.IncentivePlans.GetByIdAsync(request.Id, cancellationToken);

        if (plan == null)
        {
            return Result<IncentivePlanDto>.NotFound("IncentivePlan", request.Id);
        }

        // Only Draft plans can be freely updated
        if (plan.Status != PlanStatus.Draft)
        {
            return Result<IncentivePlanDto>.Failure(
                $"Cannot update plan in {plan.Status} status. Only Draft plans can be updated.",
                "INVALID_STATUS");
        }

        plan.Update(
            request.Name,
            request.Description,
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated incentive plan {PlanId}", plan.Id);

        return Result<IncentivePlanDto>.Success(_mapper.Map<IncentivePlanDto>(plan));
    }
}
