using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.Employees.DTOs;

namespace Dorise.Incentive.Application.Employees.Commands.UpdateEmployee;

/// <summary>
/// Command to update an employee's salary.
/// "My cat's breath smells like cat food!" - But money doesn't smell, it just grows!
/// </summary>
public record UpdateEmployeeSalaryCommand(
    Guid Id,
    decimal BaseSalary,
    string Currency = "INR") : ICommand<EmployeeDto>;
