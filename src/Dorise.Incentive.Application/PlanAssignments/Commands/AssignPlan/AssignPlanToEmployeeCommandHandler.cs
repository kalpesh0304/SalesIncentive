using AutoMapper;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.PlanAssignments.DTOs;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.PlanAssignments.Commands.AssignPlan;

/// <summary>
/// Handler for AssignPlanToEmployeeCommand.
/// </summary>
public class AssignPlanToEmployeeCommandHandler : ICommandHandler<AssignPlanToEmployeeCommand, PlanAssignmentDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<AssignPlanToEmployeeCommandHandler> _logger;

    public AssignPlanToEmployeeCommandHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<AssignPlanToEmployeeCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PlanAssignmentDto>> Handle(
        AssignPlanToEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        // Validate employee exists and is active
        var employee = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null)
        {
            return Result<PlanAssignmentDto>.NotFound("Employee", request.EmployeeId);
        }

        if (!employee.IsActive)
        {
            return Result<PlanAssignmentDto>.Failure(
                "Cannot assign plan to inactive employee",
                "EMPLOYEE_INACTIVE");
        }

        // Validate plan exists and is active
        var plan = await _unitOfWork.IncentivePlans.GetByIdAsync(request.IncentivePlanId, cancellationToken);
        if (plan == null)
        {
            return Result<PlanAssignmentDto>.NotFound("IncentivePlan", request.IncentivePlanId);
        }

        if (!plan.IsActive)
        {
            return Result<PlanAssignmentDto>.Failure(
                "Cannot assign inactive plan",
                "PLAN_INACTIVE");
        }

        // Check for overlapping assignments
        var existingAssignments = await _unitOfWork.PlanAssignments.GetByEmployeeAndPlanAsync(
            request.EmployeeId,
            request.IncentivePlanId,
            cancellationToken);

        var effectivePeriod = DateRange.Create(
            request.EffectiveFrom,
            request.EffectiveTo ?? plan.EffectivePeriod.EndDate);

        foreach (var existing in existingAssignments)
        {
            if (existing.OverlapsWith(effectivePeriod))
            {
                return Result<PlanAssignmentDto>.Failure(
                    $"Assignment overlaps with existing assignment (ID: {existing.Id})",
                    "OVERLAPPING_ASSIGNMENT");
            }
        }

        // Create assignment
        var assignment = PlanAssignment.Create(
            request.EmployeeId,
            request.IncentivePlanId,
            request.EffectiveFrom,
            request.EffectiveTo,
            request.CustomTarget,
            request.CustomTargetUnit,
            request.Notes);

        await _unitOfWork.PlanAssignments.AddAsync(assignment, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Assigned plan {PlanId} to employee {EmployeeId} with assignment ID {AssignmentId}",
            request.IncentivePlanId, request.EmployeeId, assignment.Id);

        // Reload with related data for mapping
        var result = await _unitOfWork.PlanAssignments.GetWithDetailsAsync(assignment.Id, cancellationToken);
        return Result<PlanAssignmentDto>.Success(_mapper.Map<PlanAssignmentDto>(result));
    }
}
