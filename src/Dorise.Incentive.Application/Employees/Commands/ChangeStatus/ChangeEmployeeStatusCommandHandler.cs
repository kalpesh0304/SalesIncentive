using AutoMapper;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.Employees.DTOs;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;

namespace Dorise.Incentive.Application.Employees.Commands.UpdateEmployee;

/// <summary>
/// Handler for ChangeEmployeeStatusCommand.
/// </summary>
public class ChangeEmployeeStatusCommandHandler : ICommandHandler<ChangeEmployeeStatusCommand, EmployeeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ChangeEmployeeStatusCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<EmployeeDto>> Handle(
        ChangeEmployeeStatusCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(request.Id, cancellationToken);

        if (employee == null)
        {
            return Result<EmployeeDto>.NotFound("Employee", request.Id);
        }

        if (!Enum.TryParse<EmployeeStatus>(request.Status, true, out var status))
        {
            return Result<EmployeeDto>.Failure(
                $"Invalid status '{request.Status}'. Valid values: {string.Join(", ", Enum.GetNames<EmployeeStatus>())}",
                "INVALID_STATUS");
        }

        employee.ChangeStatus(status);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<EmployeeDto>.Success(_mapper.Map<EmployeeDto>(employee));
    }
}
