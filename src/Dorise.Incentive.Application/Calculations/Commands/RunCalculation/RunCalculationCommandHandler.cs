using AutoMapper;
using Dorise.Incentive.Application.Calculations.DTOs;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.Services;
using Dorise.Incentive.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.Calculations.Commands.RunCalculation;

/// <summary>
/// Handler for RunCalculationCommand.
/// "My parents won't let me use scissors!" - But we CAN calculate incentives!
/// </summary>
public class RunCalculationCommandHandler : ICommandHandler<RunCalculationCommand, CalculationDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIncentiveCalculationService _calculationService;
    private readonly IEligibilityService _eligibilityService;
    private readonly IMapper _mapper;
    private readonly ILogger<RunCalculationCommandHandler> _logger;

    public RunCalculationCommandHandler(
        IUnitOfWork unitOfWork,
        IIncentiveCalculationService calculationService,
        IEligibilityService eligibilityService,
        IMapper mapper,
        ILogger<RunCalculationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _calculationService = calculationService;
        _eligibilityService = eligibilityService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CalculationDto>> Handle(
        RunCalculationCommand request,
        CancellationToken cancellationToken)
    {
        // Validate employee exists
        var employee = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null)
        {
            return Result<CalculationDto>.NotFound("Employee", request.EmployeeId);
        }

        // Validate plan exists and is active
        var plan = await _unitOfWork.IncentivePlans.GetWithSlabsAsync(request.IncentivePlanId, cancellationToken);
        if (plan == null)
        {
            return Result<CalculationDto>.NotFound("IncentivePlan", request.IncentivePlanId);
        }

        if (!plan.IsActive)
        {
            return Result<CalculationDto>.Failure(
                "Cannot calculate incentive for inactive plan",
                "PLAN_INACTIVE");
        }

        var period = DateRange.Create(request.PeriodStart, request.PeriodEnd);

        // Check eligibility
        var eligibility = _eligibilityService.CheckEligibility(employee, plan, request.PeriodEnd);
        if (!eligibility.IsEligible)
        {
            return Result<CalculationDto>.Failure(
                $"Employee is not eligible: {eligibility.Reason}",
                "NOT_ELIGIBLE");
        }

        // Check if calculation already exists for this period
        var existingCalculation = await _unitOfWork.Calculations.GetLatestAsync(
            request.EmployeeId,
            request.IncentivePlanId,
            period,
            cancellationToken);

        if (existingCalculation != null && existingCalculation.IsActive)
        {
            return Result<CalculationDto>.Failure(
                "Calculation already exists for this employee, plan, and period. Use recalculate endpoint to update.",
                "DUPLICATE_CALCULATION");
        }

        // Run the calculation
        var result = _calculationService.Calculate(employee, plan, request.ActualValue, period);

        if (!result.Success)
        {
            return Result<CalculationDto>.Failure(
                result.Message ?? "Calculation failed",
                "CALCULATION_FAILED");
        }

        // Create calculation entity
        var calculation = Calculation.Create(
            request.EmployeeId,
            request.IncentivePlanId,
            period,
            plan.Target.TargetValue,
            request.ActualValue,
            employee.BaseSalary);

        // Apply calculation results
        calculation.SetCalculationResult(
            result.GrossIncentive,
            result.NetIncentive,
            result.Achievement,
            result.AppliedSlab?.Id);

        // Apply prorata if needed
        if (eligibility.ProrataFactor.Value < 100)
        {
            calculation.ApplyProrata(eligibility.ProrataFactor);
        }

        // Apply cap if configured and exceeded
        if (plan.MaximumPayout != null && calculation.NetIncentive > plan.MaximumPayout)
        {
            calculation.ApplyCap(plan.MaximumPayout);
        }

        // Check if below threshold
        if (!plan.Target.MeetsMinimumThreshold(request.ActualValue))
        {
            calculation.MarkBelowThreshold();
        }

        // Set notes if provided
        if (!string.IsNullOrEmpty(request.Notes))
        {
            calculation.SetNotes(request.Notes);
        }

        await _unitOfWork.Calculations.AddAsync(calculation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created calculation {CalculationId} for employee {EmployeeId}. " +
            "Achievement: {Achievement}%, Gross: {Gross}, Net: {Net}",
            calculation.Id, employee.Id,
            result.Achievement.Value,
            result.GrossIncentive.Amount,
            result.NetIncentive.Amount);

        // Reload with related data for full DTO mapping
        var createdCalc = await _unitOfWork.Calculations.GetWithDetailsAsync(calculation.Id, cancellationToken);
        return Result<CalculationDto>.Success(_mapper.Map<CalculationDto>(createdCalc));
    }
}
