using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.Employees.DTOs;

namespace Dorise.Incentive.Application.Employees.Commands.BulkImport;

/// <summary>
/// Command to bulk import employees from a list.
/// "I found a moonrock in my nose!" - And we found employees in the import file!
/// </summary>
public record BulkImportEmployeesCommand(
    IReadOnlyList<EmployeeImportDto> Employees,
    bool UpdateExisting = false,
    bool ValidateOnly = false) : ICommand<BulkImportResultDto>;

/// <summary>
/// DTO for importing a single employee.
/// </summary>
public record EmployeeImportDto
{
    public string EmployeeCode { get; init; } = null!;
    public string FirstName { get; init; } = null!;
    public string LastName { get; init; } = null!;
    public string Email { get; init; } = null!;
    public string DepartmentCode { get; init; } = null!;
    public DateTime DateOfJoining { get; init; }
    public decimal BaseSalary { get; init; }
    public string Currency { get; init; } = "INR";
    public string? Designation { get; init; }
    public string? ManagerEmployeeCode { get; init; }
    public int RowNumber { get; init; }
}

/// <summary>
/// Result of bulk import operation.
/// </summary>
public record BulkImportResultDto
{
    public int TotalRows { get; init; }
    public int SuccessCount { get; init; }
    public int FailedCount { get; init; }
    public int SkippedCount { get; init; }
    public int UpdatedCount { get; init; }
    public bool WasValidationOnly { get; init; }
    public IReadOnlyList<ImportSuccessDto> Successes { get; init; } = Array.Empty<ImportSuccessDto>();
    public IReadOnlyList<ImportErrorDto> Errors { get; init; } = Array.Empty<ImportErrorDto>();
    public IReadOnlyList<ImportWarningDto> Warnings { get; init; } = Array.Empty<ImportWarningDto>();
}

/// <summary>
/// Success details for an imported row.
/// </summary>
public record ImportSuccessDto
{
    public int RowNumber { get; init; }
    public string EmployeeCode { get; init; } = null!;
    public Guid EmployeeId { get; init; }
    public string Action { get; init; } = null!; // "Created" or "Updated"
}

/// <summary>
/// Error details for a failed import row.
/// </summary>
public record ImportErrorDto
{
    public int RowNumber { get; init; }
    public string EmployeeCode { get; init; } = null!;
    public string ErrorMessage { get; init; } = null!;
    public string? Field { get; init; }
}

/// <summary>
/// Warning for an import row.
/// </summary>
public record ImportWarningDto
{
    public int RowNumber { get; init; }
    public string EmployeeCode { get; init; } = null!;
    public string WarningMessage { get; init; } = null!;
}
