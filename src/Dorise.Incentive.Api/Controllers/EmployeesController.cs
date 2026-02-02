using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.Employees.Commands.CreateEmployee;
using Dorise.Incentive.Application.Employees.Commands.DeleteEmployee;
using Dorise.Incentive.Application.Employees.Commands.UpdateEmployee;
using Dorise.Incentive.Application.Employees.DTOs;
using Dorise.Incentive.Application.Employees.Queries.GetEmployeeById;
using Dorise.Incentive.Application.Employees.Queries.GetEmployees;
using Dorise.Incentive.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for Employee management operations.
/// "Hi, Super Nintendo Chalmers!" - Managing employees like a superintendent!
/// </summary>
[ApiController]
[Route("api/v1/[controller]")]
[Produces("application/json")]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<EmployeesController> _logger;

    public EmployeesController(IMediator mediator, ILogger<EmployeesController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get all employees with pagination and filtering.
    /// </summary>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 20, max: 100)</param>
    /// <param name="status">Filter by employee status</param>
    /// <param name="departmentId">Filter by department</param>
    /// <param name="search">Search by name or employee code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of employees</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EmployeeSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] Guid? departmentId = null,
        [FromQuery] string? search = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Getting employees - Page: {Page}, PageSize: {PageSize}, Status: {Status}",
            page, pageSize, status);

        // Parse status string to EmployeeStatus enum if provided
        EmployeeStatus? employeeStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<EmployeeStatus>(status, true, out var parsedStatus))
        {
            employeeStatus = parsedStatus;
        }

        var query = new GetEmployeesQuery
        {
            PageNumber = page,
            PageSize = pageSize,
            Status = employeeStatus,
            DepartmentId = departmentId,
            SearchTerm = search
        };
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get employee by ID.
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Employee details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployee(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting employee {EmployeeId}", id);

        var query = new GetEmployeeByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new { Message = $"Employee with ID {id} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Get employee by employee code.
    /// </summary>
    /// <param name="code">Employee code</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Employee details</returns>
    [HttpGet("by-code/{code}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployeeByCode(string code, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting employee by code {EmployeeCode}", code);

        var query = new GetEmployeeByCodeQuery(code);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound(new { Message = $"Employee with code {code} not found" });
        }

        return Ok(result);
    }

    /// <summary>
    /// Create a new employee.
    /// </summary>
    /// <param name="command">Create employee request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created employee</returns>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEmployee(
        [FromBody] CreateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating employee with code {EmployeeCode}", command.EmployeeCode);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return CreatedAtAction(
            nameof(GetEmployee),
            new { id = result.Value },
            result.Value);
    }

    /// <summary>
    /// Update an existing employee.
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="command">Update employee request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated employee</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateEmployee(
        Guid id,
        [FromBody] UpdateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new { Error = "ID in URL does not match ID in request body" });
        }

        _logger.LogInformation("Updating employee {EmployeeId}", id);

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFound(new { Message = result.Error });
            }
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Update employee salary.
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="request">Salary update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated employee</returns>
    [HttpPatch("{id:guid}/salary")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEmployeeSalary(
        Guid id,
        [FromBody] UpdateEmployeeSalaryRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating salary for employee {EmployeeId}", id);

        var command = new UpdateEmployeeSalaryCommand(id, request.BaseSalary, request.Currency);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFound(new { Message = result.Error });
            }
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Change employee status.
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="request">Status change request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated employee</returns>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeEmployeeStatus(
        Guid id,
        [FromBody] ChangeEmployeeStatusRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Changing status for employee {EmployeeId} to {Status}",
            id, request.Status);

        var command = new ChangeEmployeeStatusCommand(id, request.Status);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFound(new { Message = result.Error });
            }
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Transfer employee to another department.
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="request">Transfer request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated employee</returns>
    [HttpPatch("{id:guid}/transfer")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TransferEmployee(
        Guid id,
        [FromBody] TransferEmployeeRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Transferring employee {EmployeeId} to department {DepartmentId}",
            id, request.DepartmentId);

        var command = new TransferEmployeeCommand(id, request.DepartmentId, request.ManagerId);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            if (result.ErrorCode == "NOT_FOUND")
            {
                return NotFound(new { Message = result.Error });
            }
            return BadRequest(new { Error = result.Error, Code = result.ErrorCode });
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Delete (soft) an employee.
    /// </summary>
    /// <param name="id">Employee ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEmployee(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting employee {EmployeeId}", id);

        var command = new DeleteEmployeeCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
        {
            return NotFound(new { Message = result.Error });
        }

        return NoContent();
    }

    /// <summary>
    /// Get employees by department.
    /// </summary>
    /// <param name="departmentId">Department ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of employees in the department</returns>
    [HttpGet("by-department/{departmentId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<EmployeeSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployeesByDepartment(
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting employees for department {DepartmentId}", departmentId);

        var query = new GetEmployeesByDepartmentQuery(departmentId);
        var result = await _mediator.Send(query, cancellationToken);

        return Ok(result);
    }
}

// Request DTOs for PATCH operations
public record UpdateEmployeeSalaryRequest(decimal BaseSalary, string Currency = "INR");
public record ChangeEmployeeStatusRequest(string Status);
public record TransferEmployeeRequest(Guid DepartmentId, Guid? ManagerId);
