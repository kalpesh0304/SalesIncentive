using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.Calculations.Commands.VoidCalculation;

/// <summary>
/// Handler for VoidCalculationCommand.
/// </summary>
public class VoidCalculationCommandHandler : ICommandHandler<VoidCalculationCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<VoidCalculationCommandHandler> _logger;

    public VoidCalculationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ILogger<VoidCalculationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result> Handle(VoidCalculationCommand request, CancellationToken cancellationToken)
    {
        var calculation = await _unitOfWork.Calculations.GetByIdAsync(request.CalculationId, cancellationToken);

        if (calculation == null)
        {
            return Result.NotFound("Calculation", request.CalculationId);
        }

        // Cannot void already paid calculations
        if (calculation.Status == CalculationStatus.Paid)
        {
            return Result.Failure(
                "Cannot void a paid calculation. Use adjustment instead.",
                "ALREADY_PAID");
        }

        // Cannot void already voided
        if (calculation.Status == CalculationStatus.Voided)
        {
            return Result.Failure("Calculation is already voided", "ALREADY_VOIDED");
        }

        var voidedBy = _currentUser.Email ?? "system";

        calculation.Void(request.Reason, voidedBy);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Calculation {CalculationId} voided by {VoidedBy}. Reason: {Reason}",
            calculation.Id, voidedBy, request.Reason);

        return Result.Success();
    }
}
