using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.PlanAssignments.Commands.UnassignPlan;

/// <summary>
/// Handler for UnassignPlanCommand.
/// </summary>
public class UnassignPlanCommandHandler : ICommandHandler<UnassignPlanCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UnassignPlanCommandHandler> _logger;

    public UnassignPlanCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<UnassignPlanCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result> Handle(UnassignPlanCommand request, CancellationToken cancellationToken)
    {
        var assignment = await _unitOfWork.PlanAssignments.GetByIdAsync(request.Id, cancellationToken);

        if (assignment == null)
        {
            return Result.NotFound("PlanAssignment", request.Id);
        }

        if (!assignment.IsActive)
        {
            return Result.Failure("Assignment is already ended", "ALREADY_ENDED");
        }

        if (request.EffectiveDate < assignment.EffectiveFrom)
        {
            return Result.Failure(
                "End date cannot be before assignment start date",
                "INVALID_END_DATE");
        }

        assignment.End(request.EffectiveDate);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Ended plan assignment {AssignmentId} effective {EffectiveDate}",
            request.Id, request.EffectiveDate);

        return Result.Success();
    }
}
