using Dorise.Incentive.Application.Employees.Commands.BulkImport;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for bulk import operations.
/// "I'm learnding!" - And the system is learning from import files!
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class ImportController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ImportController> _logger;

    public ImportController(IMediator mediator, ILogger<ImportController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Bulk import employees from JSON payload.
    /// </summary>
    /// <param name="command">Import command with employee data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Import result with success/failure details</returns>
    [HttpPost("employees")]
    [ProducesResponseType(typeof(BulkImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportEmployees(
        [FromBody] BulkImportEmployeesCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Starting bulk import of {Count} employees. ValidateOnly: {ValidateOnly}, UpdateExisting: {UpdateExisting}",
            command.Employees.Count, command.ValidateOnly, command.UpdateExisting);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Validate employee import data without persisting.
    /// </summary>
    /// <param name="employees">List of employees to validate</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Validation result</returns>
    [HttpPost("employees/validate")]
    [ProducesResponseType(typeof(BulkImportResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateEmployeeImport(
        [FromBody] IReadOnlyList<EmployeeImportDto> employees,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Validating import of {Count} employees", employees.Count);

        var command = new BulkImportEmployeesCommand(employees, ValidateOnly: true);
        var result = await _mediator.Send(command, cancellationToken);

        return Ok(result.Value);
    }

    /// <summary>
    /// Get import template with sample data.
    /// </summary>
    /// <returns>Sample import format</returns>
    [HttpGet("employees/template")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult GetImportTemplate()
    {
        var template = new
        {
            Description = "Employee import template",
            Instructions = new[]
            {
                "EmployeeCode: Unique employee identifier (required)",
                "FirstName: Employee's first name (required)",
                "LastName: Employee's last name (required)",
                "Email: Valid email address (required)",
                "DepartmentCode: Must match existing department code (required)",
                "DateOfJoining: Date in ISO format yyyy-MM-dd (required)",
                "BaseSalary: Monthly base salary amount (required)",
                "Currency: 3-letter ISO currency code, default INR",
                "Designation: Job title (optional)",
                "ManagerEmployeeCode: Manager's employee code (optional)"
            },
            SampleData = new[]
            {
                new EmployeeImportDto
                {
                    EmployeeCode = "EMP001",
                    FirstName = "John",
                    LastName = "Doe",
                    Email = "john.doe@company.com",
                    DepartmentCode = "SALES",
                    DateOfJoining = new DateTime(2024, 1, 15),
                    BaseSalary = 50000,
                    Currency = "INR",
                    Designation = "Sales Representative",
                    RowNumber = 1
                },
                new EmployeeImportDto
                {
                    EmployeeCode = "EMP002",
                    FirstName = "Jane",
                    LastName = "Smith",
                    Email = "jane.smith@company.com",
                    DepartmentCode = "SALES",
                    DateOfJoining = new DateTime(2024, 2, 1),
                    BaseSalary = 75000,
                    Currency = "INR",
                    Designation = "Senior Sales Representative",
                    ManagerEmployeeCode = "EMP001",
                    RowNumber = 2
                }
            }
        };

        return Ok(template);
    }
}
