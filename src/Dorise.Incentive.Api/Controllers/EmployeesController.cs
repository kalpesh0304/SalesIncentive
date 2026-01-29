using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for Employee operations.
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
    /// Get all employees with pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting employees - Page: {Page}, PageSize: {PageSize}", page, pageSize);
        // TODO: Implement query handler
        return Ok(new { Message = "Employees endpoint ready", Page = page, PageSize = pageSize });
    }

    /// <summary>
    /// Get employee by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployee(Guid id, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting employee {EmployeeId}", id);
        // TODO: Implement query handler
        return Ok(new { Id = id, Message = "Employee endpoint ready" });
    }

    /// <summary>
    /// Create a new employee.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEmployee(
        [FromBody] object command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new employee");
        // TODO: Implement command handler
        var id = Guid.NewGuid();
        return CreatedAtAction(nameof(GetEmployee), new { id }, new { Id = id, Message = "Created" });
    }
}
