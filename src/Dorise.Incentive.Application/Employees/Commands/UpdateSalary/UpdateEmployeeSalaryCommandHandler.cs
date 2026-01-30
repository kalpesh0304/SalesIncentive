using AutoMapper;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.Employees.DTOs;
using Dorise.Incentive.Domain.Interfaces;

namespace Dorise.Incentive.Application.Employees.Commands.UpdateEmployee;

/// <summary>
/// Handler for UpdateEmployeeSalaryCommand.
/// </summary>
public class UpdateEmployeeSalaryCommandHandler : ICommandHandler<UpdateEmployeeSalaryCommand, EmployeeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public UpdateEmployeeSalaryCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<EmployeeDto>> Handle(
        UpdateEmployeeSalaryCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(request.Id, cancellationToken);

        if (employee == null)
        {
            return Result<EmployeeDto>.NotFound("Employee", request.Id);
        }

        employee.UpdateSalary(request.BaseSalary, request.Currency);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<EmployeeDto>.Success(_mapper.Map<EmployeeDto>(employee));
    }
}
