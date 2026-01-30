using AutoMapper;
using Dorise.Incentive.Application.Calculations.DTOs;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.Services;
using Dorise.Incentive.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.Calculations.Commands.AdjustCalculation;

/// <summary>
/// Handler for AdjustCalculationCommand.
/// </summary>
public class AdjustCalculationCommandHandler : ICommandHandler<AdjustCalculationCommand, CalculationDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly ILogger<AdjustCalculationCommandHandler> _logger;

    public AdjustCalculationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMapper mapper,
        ILogger<AdjustCalculationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CalculationDto>> Handle(
        AdjustCalculationCommand request,
        CancellationToken cancellationToken)
    {
        var calculation = await _unitOfWork.Calculations.GetByIdAsync(request.CalculationId, cancellationToken);

        if (calculation == null)
        {
            return Result<CalculationDto>.NotFound("Calculation", request.CalculationId);
        }

        // Can only adjust approved or calculated status
        if (calculation.Status != CalculationStatus.Approved &&
            calculation.Status != CalculationStatus.Calculated)
        {
            return Result<CalculationDto>.Failure(
                $"Cannot adjust calculation in {calculation.Status} status",
                "INVALID_STATUS");
        }

        var adjuster = _currentUser.Email ?? "system";
        var newAmount = Money.Create(request.NewAmount, calculation.NetIncentive.Currency);

        calculation.Adjust(newAmount, request.Reason, adjuster);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Calculation {CalculationId} adjusted by {Adjuster}. New amount: {Amount}. Reason: {Reason}",
            calculation.Id, adjuster, request.NewAmount, request.Reason);

        return Result<CalculationDto>.Success(_mapper.Map<CalculationDto>(calculation));
    }
}

/// <summary>
/// Handler for RecalculateCommand.
/// </summary>
public class RecalculateCommandHandler : ICommandHandler<RecalculateCommand, CalculationDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIncentiveCalculationService _calculationService;
    private readonly IEligibilityService _eligibilityService;
    private readonly IMapper _mapper;
    private readonly ILogger<RecalculateCommandHandler> _logger;

    public RecalculateCommandHandler(
        IUnitOfWork unitOfWork,
        IIncentiveCalculationService calculationService,
        IEligibilityService eligibilityService,
        IMapper mapper,
        ILogger<RecalculateCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _calculationService = calculationService;
        _eligibilityService = eligibilityService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CalculationDto>> Handle(
        RecalculateCommand request,
        CancellationToken cancellationToken)
    {
        var calculation = await _unitOfWork.Calculations.GetWithDetailsAsync(
            request.CalculationId, cancellationToken);

        if (calculation == null)
        {
            return Result<CalculationDto>.NotFound("Calculation", request.CalculationId);
        }

        // Can only recalculate certain statuses
        var allowedStatuses = new[]
        {
            CalculationStatus.Calculated,
            CalculationStatus.Rejected,
            CalculationStatus.BelowThreshold
        };

        if (!allowedStatuses.Contains(calculation.Status))
        {
            return Result<CalculationDto>.Failure(
                $"Cannot recalculate in {calculation.Status} status",
                "INVALID_STATUS");
        }

        // Get employee and plan
        var employee = await _unitOfWork.Employees.GetByIdAsync(calculation.EmployeeId, cancellationToken);
        var plan = await _unitOfWork.IncentivePlans.GetWithSlabsAsync(calculation.IncentivePlanId, cancellationToken);

        if (employee == null || plan == null)
        {
            return Result<CalculationDto>.Failure(
                "Employee or plan no longer exists",
                "DATA_NOT_FOUND");
        }

        // Use new actual value or keep existing
        var actualValue = request.NewActualValue ?? calculation.ActualValue;

        // Run calculation
        var calcResult = _calculationService.Calculate(employee, plan, actualValue, calculation.CalculationPeriod);

        if (!calcResult.Success)
        {
            return Result<CalculationDto>.Failure(
                calcResult.Message ?? "Recalculation failed",
                "CALCULATION_FAILED");
        }

        // Update calculation
        calculation.Recalculate(
            actualValue,
            calcResult.GrossIncentive,
            calcResult.NetIncentive,
            calcResult.Achievement,
            calcResult.AppliedSlab?.Id);

        // Check eligibility for prorata
        var eligibility = _eligibilityService.CheckEligibility(employee, plan, calculation.CalculationPeriod.EndDate);
        if (eligibility.ProrataFactor.Value < 100)
        {
            calculation.ApplyProrata(eligibility.ProrataFactor);
        }

        if (plan.MaximumPayout != null && calculation.NetIncentive > plan.MaximumPayout)
        {
            calculation.ApplyCap(plan.MaximumPayout);
        }

        if (!plan.Target.MeetsMinimumThreshold(actualValue))
        {
            calculation.MarkBelowThreshold();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Calculation {CalculationId} recalculated. New amount: {Amount}",
            calculation.Id, calcResult.NetIncentive.Amount);

        return Result<CalculationDto>.Success(_mapper.Map<CalculationDto>(calculation));
    }
}
