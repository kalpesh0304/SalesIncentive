using Dorise.Incentive.Application.Common.Interfaces;

namespace Dorise.Incentive.Application.Employees.Commands.CreateEmployee;

/// <summary>
/// Command to create a new employee.
/// "I bent my wookie!" - Creating employees, not bending Wookiees!
/// </summary>
public record CreateEmployeeCommand : ICommand<Guid>
{
    public string EmployeeCode { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public Guid DepartmentId { get; init; }
    public DateTime DateOfJoining { get; init; }
    public decimal BaseSalary { get; init; }
    public string Currency { get; init; } = "INR";
    public string? Designation { get; init; }
    public Guid? ManagerId { get; init; }
    public string? AzureAdObjectId { get; init; }
}
