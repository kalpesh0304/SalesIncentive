using System.Diagnostics;
using AutoMapper;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.Services;
using Dorise.Incentive.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.Calculations.Commands.BatchCalculation;

/// <summary>
/// Handler for RunBatchCalculationCommand.
/// </summary>
public class RunBatchCalculationCommandHandler : ICommandHandler<RunBatchCalculationCommand, BatchCalculationResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIncentiveCalculationService _calculationService;
    private readonly IEligibilityService _eligibilityService;
    private readonly IMapper _mapper;
    private readonly ILogger<RunBatchCalculationCommandHandler> _logger;

    public RunBatchCalculationCommandHandler(
        IUnitOfWork unitOfWork,
        IIncentiveCalculationService calculationService,
        IEligibilityService eligibilityService,
        IMapper mapper,
        ILogger<RunBatchCalculationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _calculationService = calculationService;
        _eligibilityService = eligibilityService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<BatchCalculationResultDto>> Handle(
        RunBatchCalculationCommand request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var results = new List<BatchCalculationItemDto>();
        var errors = new List<BatchCalculationErrorDto>();
        var period = DateRange.Create(request.PeriodStart, request.PeriodEnd);

        _logger.LogInformation(
            "Starting batch calculation for period {Start} to {End}",
            request.PeriodStart, request.PeriodEnd);

        // Get the plan(s) to use
        var plans = new List<IncentivePlan>();
        if (request.IncentivePlanId.HasValue)
        {
            var plan = await _unitOfWork.IncentivePlans.GetWithSlabsAsync(
                request.IncentivePlanId.Value, cancellationToken);
            if (plan == null)
            {
                return Result<BatchCalculationResultDto>.NotFound("IncentivePlan", request.IncentivePlanId.Value);
            }
            plans.Add(plan);
        }
        else
        {
            // Get all active plans for the period
            var activePlans = await _unitOfWork.IncentivePlans.GetActivePlansAsync(
                request.PeriodEnd, cancellationToken);
            plans.AddRange(activePlans);
        }

        if (plans.Count == 0)
        {
            return Result<BatchCalculationResultDto>.Failure(
                "No active plans found for the specified period",
                "NO_ACTIVE_PLANS");
        }

        // Get employees to calculate
        IReadOnlyList<Employee> employees;
        if (request.EmployeeAchievements != null && request.EmployeeAchievements.Count > 0)
        {
            // Use provided employee list
            var employeeIds = request.EmployeeAchievements.Select(e => e.EmployeeId).ToList();
            employees = await _unitOfWork.Employees.GetByIdsAsync(employeeIds, cancellationToken);
        }
        else if (request.DepartmentId.HasValue)
        {
            // Get employees by department
            employees = await _unitOfWork.Employees.GetByDepartmentAsync(
                request.DepartmentId.Value, cancellationToken: cancellationToken);
        }
        else
        {
            // Get all active employees
            employees = await _unitOfWork.Employees.GetActiveAsync(cancellationToken);
        }

        var achievementLookup = request.EmployeeAchievements?
            .ToDictionary(e => e.EmployeeId, e => e.ActualValue)
            ?? new Dictionary<Guid, decimal>();

        decimal totalGross = 0;
        decimal totalNet = 0;
        int skipped = 0;

        foreach (var employee in employees)
        {
            foreach (var plan in plans)
            {
                try
                {
                    // Check if employee has an assignment for this plan
                    var hasAssignment = await _unitOfWork.PlanAssignments.HasActiveAssignmentAsync(
                        employee.Id, plan.Id, request.PeriodEnd, cancellationToken);

                    if (!hasAssignment)
                    {
                        skipped++;
                        continue;
                    }

                    // Check eligibility
                    var eligibility = _eligibilityService.CheckEligibility(employee, plan, request.PeriodEnd);
                    if (!eligibility.IsEligible)
                    {
                        errors.Add(new BatchCalculationErrorDto
                        {
                            EmployeeId = employee.Id,
                            EmployeeCode = employee.EmployeeCode.Value,
                            ErrorMessage = eligibility.Reason ?? "Not eligible",
                            ErrorCode = "NOT_ELIGIBLE"
                        });
                        continue;
                    }

                    // Check for existing calculation
                    var existingCalc = await _unitOfWork.Calculations.GetLatestAsync(
                        employee.Id, plan.Id, period, cancellationToken);
                    if (existingCalc != null && existingCalc.IsActive)
                    {
                        skipped++;
                        continue;
                    }

                    // Get achievement value
                    decimal actualValue = 0;
                    if (achievementLookup.TryGetValue(employee.Id, out var providedValue))
                    {
                        actualValue = providedValue;
                    }

                    // Run calculation
                    var calcResult = _calculationService.Calculate(employee, plan, actualValue, period);

                    if (!calcResult.Success)
                    {
                        errors.Add(new BatchCalculationErrorDto
                        {
                            EmployeeId = employee.Id,
                            EmployeeCode = employee.EmployeeCode.Value,
                            ErrorMessage = calcResult.Message ?? "Calculation failed",
                            ErrorCode = "CALCULATION_FAILED"
                        });
                        continue;
                    }

                    // Create calculation entity
                    var calculation = Calculation.Create(
                        employee.Id,
                        plan.Id,
                        period,
                        plan.Target.TargetValue,
                        actualValue,
                        employee.BaseSalary);

                    calculation.SetCalculationResult(
                        calcResult.GrossIncentive,
                        calcResult.NetIncentive,
                        calcResult.Achievement,
                        calcResult.AppliedSlab?.Id);

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

                    await _unitOfWork.Calculations.AddAsync(calculation, cancellationToken);

                    totalGross += calcResult.GrossIncentive.Amount;
                    totalNet += calcResult.NetIncentive.Amount;

                    results.Add(new BatchCalculationItemDto
                    {
                        CalculationId = calculation.Id,
                        EmployeeId = employee.Id,
                        EmployeeCode = employee.EmployeeCode.Value,
                        EmployeeName = employee.FullName,
                        ActualValue = actualValue,
                        AchievementPercentage = calcResult.Achievement.Value,
                        GrossIncentive = calcResult.GrossIncentive.Amount,
                        NetIncentive = calcResult.NetIncentive.Amount,
                        AppliedSlab = calcResult.AppliedSlab?.Name,
                        Status = calculation.Status.ToString()
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error calculating for employee {EmployeeId}", employee.Id);
                    errors.Add(new BatchCalculationErrorDto
                    {
                        EmployeeId = employee.Id,
                        EmployeeCode = employee.EmployeeCode.Value,
                        ErrorMessage = ex.Message,
                        ErrorCode = "EXCEPTION"
                    });
                }
            }
        }

        // Save all calculations
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        stopwatch.Stop();

        _logger.LogInformation(
            "Batch calculation completed. Success: {Success}, Failed: {Failed}, Skipped: {Skipped}, Time: {Time}ms",
            results.Count, errors.Count, skipped, stopwatch.ElapsedMilliseconds);

        return Result<BatchCalculationResultDto>.Success(new BatchCalculationResultDto
        {
            TotalEmployees = employees.Count,
            SuccessCount = results.Count,
            FailedCount = errors.Count,
            SkippedCount = skipped,
            TotalGrossIncentive = totalGross,
            TotalNetIncentive = totalNet,
            Currency = "INR",
            ProcessingTime = stopwatch.Elapsed,
            Results = results,
            Errors = errors
        });
    }
}
