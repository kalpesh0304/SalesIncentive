using AutoMapper;
using Dorise.Incentive.Application.Calculations.DTOs;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.Calculations.Commands.ApproveCalculation;

/// <summary>
/// Handler for ApproveCalculationCommand.
/// </summary>
public class ApproveCalculationCommandHandler : ICommandHandler<ApproveCalculationCommand, CalculationDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly ILogger<ApproveCalculationCommandHandler> _logger;

    public ApproveCalculationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMapper mapper,
        ILogger<ApproveCalculationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CalculationDto>> Handle(
        ApproveCalculationCommand request,
        CancellationToken cancellationToken)
    {
        var calculation = await _unitOfWork.Calculations.GetWithApprovalsAsync(
            request.CalculationId, cancellationToken);

        if (calculation == null)
        {
            return Result<CalculationDto>.NotFound("Calculation", request.CalculationId);
        }

        if (calculation.Status != CalculationStatus.PendingApproval)
        {
            return Result<CalculationDto>.Failure(
                $"Cannot approve calculation in {calculation.Status} status",
                "INVALID_STATUS");
        }

        var approver = _currentUser.Email ?? "system";

        calculation.Approve(approver, request.Comments);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Calculation {CalculationId} approved by {Approver}",
            calculation.Id, approver);

        return Result<CalculationDto>.Success(_mapper.Map<CalculationDto>(calculation));
    }
}

/// <summary>
/// Handler for RejectCalculationCommand.
/// </summary>
public class RejectCalculationCommandHandler : ICommandHandler<RejectCalculationCommand, CalculationDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly IMapper _mapper;
    private readonly ILogger<RejectCalculationCommandHandler> _logger;

    public RejectCalculationCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        IMapper mapper,
        ILogger<RejectCalculationCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<CalculationDto>> Handle(
        RejectCalculationCommand request,
        CancellationToken cancellationToken)
    {
        var calculation = await _unitOfWork.Calculations.GetWithApprovalsAsync(
            request.CalculationId, cancellationToken);

        if (calculation == null)
        {
            return Result<CalculationDto>.NotFound("Calculation", request.CalculationId);
        }

        if (calculation.Status != CalculationStatus.PendingApproval)
        {
            return Result<CalculationDto>.Failure(
                $"Cannot reject calculation in {calculation.Status} status",
                "INVALID_STATUS");
        }

        var rejector = _currentUser.Email ?? "system";

        calculation.Reject(rejector, request.Reason);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Calculation {CalculationId} rejected by {Rejector}. Reason: {Reason}",
            calculation.Id, rejector, request.Reason);

        return Result<CalculationDto>.Success(_mapper.Map<CalculationDto>(calculation));
    }
}
