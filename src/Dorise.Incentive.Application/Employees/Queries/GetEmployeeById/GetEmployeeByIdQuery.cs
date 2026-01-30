using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.Employees.DTOs;

namespace Dorise.Incentive.Application.Employees.Queries.GetEmployeeById;

/// <summary>
/// Query to get an employee by ID.
/// "Tastes like burning!" - But this query tastes like data!
/// </summary>
public record GetEmployeeByIdQuery(Guid EmployeeId) : IQuery<EmployeeDto>;
