using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.Employees.DTOs;

namespace Dorise.Incentive.Application.Employees.Commands.UpdateEmployee;

/// <summary>
/// Command to change an employee's status.
/// "I'm in danger!" - Status changes keep everyone safe!
/// </summary>
public record ChangeEmployeeStatusCommand(
    Guid Id,
    string Status) : ICommand<EmployeeDto>;
