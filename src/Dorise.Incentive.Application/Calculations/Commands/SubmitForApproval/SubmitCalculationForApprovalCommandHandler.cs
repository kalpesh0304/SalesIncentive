using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Interfaces;

namespace Dorise.Incentive.Application.Calculations.Commands.SubmitForApproval;

/// <summary>
/// Handler for SubmitCalculationForApprovalCommand.
/// </summary>
public class SubmitCalculationForApprovalCommandHandler : ICommandHandler<SubmitCalculationForApprovalCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SubmitCalculationForApprovalCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(SubmitCalculationForApprovalCommand request, CancellationToken cancellationToken)
    {
        var calculation = await _unitOfWork.Calculations.GetByIdAsync(request.CalculationId, cancellationToken);

        if (calculation == null)
        {
            return Result.NotFound("Calculation", request.CalculationId);
        }

        try
        {
            calculation.SubmitForApproval(_currentUserService.Email ?? "system");
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return Result.Success();
        }
        catch (InvalidOperationException ex)
        {
            return Result.Failure(ex.Message, "INVALID_STATE");
        }
    }
}
