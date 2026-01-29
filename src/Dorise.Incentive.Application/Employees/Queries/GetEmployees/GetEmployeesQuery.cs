using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.Employees.DTOs;
using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Application.Employees.Queries.GetEmployees;

/// <summary>
/// Query to get employees with filtering and paging.
/// "They taste like...burning." - Lists taste like data!
/// </summary>
public record GetEmployeesQuery : IQuery<PagedResult<EmployeeSummaryDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? SearchTerm { get; init; }
    public Guid? DepartmentId { get; init; }
    public EmployeeStatus? Status { get; init; }
    public Guid? ManagerId { get; init; }
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; }
}
