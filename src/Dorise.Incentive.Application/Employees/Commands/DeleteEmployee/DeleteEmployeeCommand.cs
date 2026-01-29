using Dorise.Incentive.Application.Common.Interfaces;

namespace Dorise.Incentive.Application.Employees.Commands.DeleteEmployee;

/// <summary>
/// Command to soft-delete an employee.
/// "I'm a unitard!" - But employees just get deactivated, not erased!
/// </summary>
public record DeleteEmployeeCommand(Guid Id) : ICommand;
