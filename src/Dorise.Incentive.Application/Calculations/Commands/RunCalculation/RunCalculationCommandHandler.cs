using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.Services;
using Dorise.Incentive.Domain.ValueObjects;

namespace Dorise.Incentive.Application.Calculations.Commands.RunCalculation;

/// <summary>
/// Handler for RunCalculationCommand.
/// "My parents won't let me use scissors!" - But we CAN calculate incentives!
/// </summary>
public class RunCalculationCommandHandler : ICommandHandler<RunCalculationCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIncentiveCalculationService _calculationService;

    public RunCalculationCommandHandler(
        IUnitOfWork unitOfWork,
        IIncentiveCalculationService calculationService)
    {
        _unitOfWork = unitOfWork;
        _calculationService = calculationService;
    }

    public async Task<Result<Guid>> Handle(RunCalculationCommand request, CancellationToken cancellationToken)
    {
        // Validate employee exists
        var employee = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null)
        {
            return Result.NotFound<Guid>("Employee", request.EmployeeId);
        }

        // Validate plan exists and is active
        var plan = await _unitOfWork.IncentivePlans.GetWithSlabsAsync(request.IncentivePlanId, cancellationToken);
        if (plan == null)
        {
            return Result.NotFound<Guid>("IncentivePlan", request.IncentivePlanId);
        }

        var period = DateRange.Create(request.PeriodStart, request.PeriodEnd);

        // Check if calculation already exists for this period
        var existingCalculation = await _unitOfWork.Calculations.GetLatestAsync(
            request.EmployeeId,
            request.IncentivePlanId,
            period,
            cancellationToken);

        if (existingCalculation != null)
        {
            return Result.Failure<Guid>(
                "Calculation already exists for this employee, plan, and period. Use adjustment endpoint to modify.",
                "DUPLICATE_CALCULATION");
        }

        // Run the calculation
        var result = _calculationService.Calculate(employee, plan, request.ActualValue, period);

        if (!result.Success)
        {
            return Result.Failure<Guid>(result.Message ?? "Calculation failed", "CALCULATION_FAILED");
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
        calculation.Calculate(result.GrossIncentive, result.AppliedSlab?.Id);

        // Apply prorata if needed
        if (result.NetIncentive.Amount != result.GrossIncentive.Amount)
        {
            var prorataPercentage = result.GrossIncentive.Amount > 0
                ? result.NetIncentive.Amount / result.GrossIncentive.Amount * 100
                : 0;
            calculation.ApplyProrata(Percentage.Create(prorataPercentage));
        }

        // Apply cap if configured
        if (plan.MaximumPayout != null)
        {
            calculation.ApplyCap(plan.MaximumPayout);
        }

        // Check if below threshold
        if (!plan.Target.MeetsMinimumThreshold(request.ActualValue))
        {
            calculation.MarkBelowThreshold();
        }

        await _unitOfWork.Calculations.AddAsync(calculation, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(calculation.Id);
    }
}
