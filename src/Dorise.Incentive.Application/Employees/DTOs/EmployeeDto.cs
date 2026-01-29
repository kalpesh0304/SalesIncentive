using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Application.Employees.DTOs;

/// <summary>
/// Data transfer object for Employee.
/// "I was in the igloo having a bath!" - Clean DTOs are the best!
/// </summary>
public record EmployeeDto
{
    public Guid Id { get; init; }
    public string EmployeeCode { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public Guid DepartmentId { get; init; }
    public string? DepartmentName { get; init; }
    public Guid? ManagerId { get; init; }
    public string? ManagerName { get; init; }
    public string? Designation { get; init; }
    public EmployeeStatus Status { get; init; }
    public DateTime DateOfJoining { get; init; }
    public DateTime? DateOfLeaving { get; init; }
    public decimal BaseSalary { get; init; }
    public string Currency { get; init; } = "INR";
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
}

/// <summary>
/// Summary DTO for employee lists.
/// </summary>
public record EmployeeSummaryDto
{
    public Guid Id { get; init; }
    public string EmployeeCode { get; init; } = null!;
    public string FullName { get; init; } = null!;
    public string? Designation { get; init; }
    public string? DepartmentName { get; init; }
    public EmployeeStatus Status { get; init; }
}

/// <summary>
/// DTO for employee with plan assignments.
/// </summary>
public record EmployeeWithPlansDto : EmployeeDto
{
    public IReadOnlyList<PlanAssignmentDto> PlanAssignments { get; init; } = Array.Empty<PlanAssignmentDto>();
}

/// <summary>
/// DTO for plan assignment.
/// </summary>
public record PlanAssignmentDto
{
    public Guid Id { get; init; }
    public Guid IncentivePlanId { get; init; }
    public string PlanCode { get; init; } = null!;
    public string PlanName { get; init; } = null!;
    public DateTime EffectiveFrom { get; init; }
    public DateTime EffectiveTo { get; init; }
    public decimal? CustomTarget { get; init; }
    public decimal? WeightagePercentage { get; init; }
    public bool IsActive { get; init; }
}
