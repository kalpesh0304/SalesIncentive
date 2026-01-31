using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.PlanAssignments.DTOs;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.Services;
using MediatR;

namespace Dorise.Incentive.Application.PlanAssignments.Queries;

/// <summary>
/// Handler for CheckPlanEligibilityQuery.
/// "I choo-choo-choose you!" - And we check if the plan chooses the employee!
/// </summary>
public class CheckPlanEligibilityQueryHandler : IRequestHandler<CheckPlanEligibilityQuery, EligibilityCheckResultDto>
{
    private readonly IReadOnlyRepository _repository;
    private readonly IEligibilityService _eligibilityService;

    public CheckPlanEligibilityQueryHandler(
        IReadOnlyRepository repository,
        IEligibilityService eligibilityService)
    {
        _repository = repository;
        _eligibilityService = eligibilityService;
    }

    public async Task<EligibilityCheckResultDto> Handle(
        CheckPlanEligibilityQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await _repository.Employees.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null)
        {
            return new EligibilityCheckResultDto
            {
                EmployeeId = request.EmployeeId,
                PlanId = request.PlanId,
                IsEligible = false,
                Reason = "Employee not found"
            };
        }

        var plan = await _repository.IncentivePlans.GetByIdAsync(request.PlanId, cancellationToken);
        if (plan == null)
        {
            return new EligibilityCheckResultDto
            {
                EmployeeId = request.EmployeeId,
                PlanId = request.PlanId,
                IsEligible = false,
                Reason = "Incentive plan not found"
            };
        }

        // Check for existing assignment
        var existingAssignments = await _repository.PlanAssignments.GetByEmployeeAndPlanAsync(
            request.EmployeeId,
            request.PlanId,
            cancellationToken);

        var activeAssignment = existingAssignments.FirstOrDefault(a => a.IsActive);

        // Run eligibility check
        var result = _eligibilityService.CheckEligibility(employee, plan, request.AsOfDate);

        return new EligibilityCheckResultDto
        {
            EmployeeId = request.EmployeeId,
            PlanId = request.PlanId,
            IsEligible = result.IsEligible,
            Reason = result.Reason,
            EligibilityCriteriaMet = result.CriteriaMet
                .Select(c => $"{c.Name}: {c.Description}")
                .ToList(),
            EligibilityCriteriaNotMet = result.CriteriaNotMet
                .Select(c => $"{c.Name}: {c.Description}")
                .ToList(),
            HasExistingAssignment = activeAssignment != null,
            ExistingAssignmentEndDate = activeAssignment?.EffectivePeriod.EndDate
        };
    }
}
