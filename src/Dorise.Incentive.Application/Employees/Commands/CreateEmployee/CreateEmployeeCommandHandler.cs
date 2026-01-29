using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.ValueObjects;

namespace Dorise.Incentive.Application.Employees.Commands.CreateEmployee;

/// <summary>
/// Handler for CreateEmployeeCommand.
/// "Inflammable means flammable? What a country!" - Creating employees is more straightforward!
/// </summary>
public class CreateEmployeeCommandHandler : ICommandHandler<CreateEmployeeCommand, Guid>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateEmployeeCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
    {
        // Check if employee code already exists
        var employeeCode = EmployeeCode.Create(request.EmployeeCode);
        if (await _unitOfWork.Employees.CodeExistsAsync(employeeCode, cancellationToken))
        {
            return Result.Failure<Guid>($"Employee with code '{request.EmployeeCode}' already exists.", "DUPLICATE_CODE");
        }

        // Check if email already exists
        var existingByEmail = await _unitOfWork.Employees.GetByEmailAsync(request.Email, cancellationToken);
        if (existingByEmail != null)
        {
            return Result.Failure<Guid>($"Employee with email '{request.Email}' already exists.", "DUPLICATE_EMAIL");
        }

        // Validate department exists
        var department = await _unitOfWork.Departments.GetByIdAsync(request.DepartmentId, cancellationToken);
        if (department == null)
        {
            return Result.NotFound<Guid>("Department", request.DepartmentId);
        }

        // Validate manager exists if provided
        if (request.ManagerId.HasValue)
        {
            var manager = await _unitOfWork.Employees.GetByIdAsync(request.ManagerId.Value, cancellationToken);
            if (manager == null)
            {
                return Result.NotFound<Guid>("Manager", request.ManagerId.Value);
            }
        }

        // Create employee
        var employee = Employee.Create(
            request.EmployeeCode,
            request.FirstName,
            request.LastName,
            request.Email,
            request.DepartmentId,
            request.DateOfJoining,
            request.BaseSalary,
            request.Currency,
            request.Designation,
            request.ManagerId);

        if (!string.IsNullOrWhiteSpace(request.AzureAdObjectId))
        {
            employee.SetAzureAdObjectId(request.AzureAdObjectId);
        }

        await _unitOfWork.Employees.AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(employee.Id);
    }
}
