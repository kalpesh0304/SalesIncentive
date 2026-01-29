using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.Employees.DTOs;

namespace Dorise.Incentive.Application.Employees.Commands.UpdateEmployee;

/// <summary>
/// Command to transfer an employee to another department.
/// "I found a moonrock in my nose!" - Transfers help employees find new places!
/// </summary>
public record TransferEmployeeCommand(
    Guid Id,
    Guid DepartmentId,
    Guid? ManagerId) : ICommand<EmployeeDto>;
