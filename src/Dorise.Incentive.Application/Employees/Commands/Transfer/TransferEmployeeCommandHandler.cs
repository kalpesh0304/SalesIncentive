using AutoMapper;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.Employees.DTOs;
using Dorise.Incentive.Domain.Interfaces;

namespace Dorise.Incentive.Application.Employees.Commands.UpdateEmployee;

/// <summary>
/// Handler for TransferEmployeeCommand.
/// </summary>
public class TransferEmployeeCommandHandler : ICommandHandler<TransferEmployeeCommand, EmployeeDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public TransferEmployeeCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<EmployeeDto>> Handle(
        TransferEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(request.Id, cancellationToken);

        if (employee == null)
        {
            return Result<EmployeeDto>.NotFound("Employee", request.Id);
        }

        // Verify department exists
        var department = await _unitOfWork.Departments.GetByIdAsync(request.DepartmentId, cancellationToken);
        if (department == null)
        {
            return Result<EmployeeDto>.NotFound("Department", request.DepartmentId);
        }

        // Verify manager exists if specified
        if (request.ManagerId.HasValue)
        {
            var manager = await _unitOfWork.Employees.GetByIdAsync(request.ManagerId.Value, cancellationToken);
            if (manager == null)
            {
                return Result<EmployeeDto>.NotFound("Manager", request.ManagerId.Value);
            }
        }

        employee.TransferToDepartment(request.DepartmentId, request.ManagerId);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<EmployeeDto>.Success(_mapper.Map<EmployeeDto>(employee));
    }
}
