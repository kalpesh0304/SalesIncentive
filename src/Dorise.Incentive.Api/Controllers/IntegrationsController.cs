using Dorise.Incentive.Application.Integrations.DTOs;
using Dorise.Incentive.Application.Integrations.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for external system integrations (ERP, HR, Payroll).
/// "The pointy kitty took it!" - And this controller takes data from external systems!
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,IntegrationManager")]
public class IntegrationsController : ControllerBase
{
    private readonly IErpIntegrationService _erpService;
    private readonly IHrIntegrationService _hrService;
    private readonly IPayrollIntegrationService _payrollService;
    private readonly ILogger<IntegrationsController> _logger;

    public IntegrationsController(
        IErpIntegrationService erpService,
        IHrIntegrationService hrService,
        IPayrollIntegrationService payrollService,
        ILogger<IntegrationsController> logger)
    {
        _erpService = erpService;
        _hrService = hrService;
        _payrollService = payrollService;
        _logger = logger;
    }

    #region Integration Status

    /// <summary>
    /// Gets the status of all integrations.
    /// </summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(IReadOnlyList<IntegrationSyncStatusDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllStatus(CancellationToken cancellationToken)
    {
        var statuses = await Task.WhenAll(
            _erpService.GetSyncStatusAsync(cancellationToken),
            _hrService.GetSyncStatusAsync(cancellationToken),
            _payrollService.GetSyncStatusAsync(cancellationToken));

        return Ok(statuses);
    }

    /// <summary>
    /// Tests connection to all external systems.
    /// </summary>
    [HttpPost("test-connections")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestConnections(CancellationToken cancellationToken)
    {
        var results = new
        {
            Erp = await _erpService.TestConnectionAsync(cancellationToken),
            Hr = await _hrService.TestConnectionAsync(cancellationToken),
            Payroll = await _payrollService.TestConnectionAsync(cancellationToken),
            TestedAt = DateTime.UtcNow
        };

        return Ok(results);
    }

    #endregion

    #region ERP Integration (Sales Data)

    /// <summary>
    /// Gets the ERP integration status.
    /// </summary>
    [HttpGet("erp/status")]
    [ProducesResponseType(typeof(IntegrationSyncStatusDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetErpStatus(CancellationToken cancellationToken)
    {
        var status = await _erpService.GetSyncStatusAsync(cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// Tests the ERP connection.
    /// </summary>
    [HttpPost("erp/test")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestErpConnection(CancellationToken cancellationToken)
    {
        var result = await _erpService.TestConnectionAsync(cancellationToken);
        return Ok(new { Connected = result, TestedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Imports sales data from ERP for a period.
    /// </summary>
    [HttpPost("erp/import/{period}")]
    [ProducesResponseType(typeof(SalesDataImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportSalesData(
        string period,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Import sales data for period {Period}", period);

        if (!IsValidPeriod(period))
        {
            return BadRequest(new { Error = "Period must be in format YYYY-MM" });
        }

        var result = await _erpService.ImportSalesDataAsync(period, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Imports sales data from request body.
    /// </summary>
    [HttpPost("erp/import")]
    [ProducesResponseType(typeof(SalesDataImportResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ImportSalesData(
        [FromBody] IEnumerable<SalesDataImportDto> salesData,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Import sales data from request body");

        var result = await _erpService.ImportSalesDataAsync(salesData, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Fetches sales data from ERP without importing.
    /// </summary>
    [HttpGet("erp/sales/{period}")]
    [ProducesResponseType(typeof(IReadOnlyList<SalesDataImportDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> FetchSalesData(
        string period,
        CancellationToken cancellationToken)
    {
        var data = await _erpService.FetchSalesDataAsync(period, cancellationToken);
        return Ok(data);
    }

    /// <summary>
    /// Validates sales data before import.
    /// </summary>
    [HttpPost("erp/validate")]
    [ProducesResponseType(typeof(IReadOnlyList<ImportErrorDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateSalesData(
        [FromBody] IEnumerable<SalesDataImportDto> salesData,
        CancellationToken cancellationToken)
    {
        var errors = await _erpService.ValidateSalesDataAsync(salesData, cancellationToken);
        return Ok(new { IsValid = !errors.Any(), Errors = errors });
    }

    #endregion

    #region HR Integration (Employee Sync)

    /// <summary>
    /// Gets the HR integration status.
    /// </summary>
    [HttpGet("hr/status")]
    [ProducesResponseType(typeof(IntegrationSyncStatusDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHrStatus(CancellationToken cancellationToken)
    {
        var status = await _hrService.GetSyncStatusAsync(cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// Tests the HR connection.
    /// </summary>
    [HttpPost("hr/test")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestHrConnection(CancellationToken cancellationToken)
    {
        var result = await _hrService.TestConnectionAsync(cancellationToken);
        return Ok(new { Connected = result, TestedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Syncs all employees from HR system.
    /// </summary>
    [HttpPost("hr/sync")]
    [ProducesResponseType(typeof(HrSyncResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncEmployees(CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Full employee sync from HR");

        var result = await _hrService.SyncEmployeesAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Syncs a single employee from HR.
    /// </summary>
    [HttpPost("hr/sync/{employeeCode}")]
    [ProducesResponseType(typeof(HrSyncResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncEmployee(
        string employeeCode,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Sync employee {EmployeeCode} from HR", employeeCode);

        var result = await _hrService.SyncEmployeeAsync(employeeCode, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Syncs employees from request body.
    /// </summary>
    [HttpPost("hr/sync/batch")]
    [ProducesResponseType(typeof(HrSyncResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> SyncEmployees(
        [FromBody] IEnumerable<HrEmployeeDto> employees,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Sync employees from request body");

        var result = await _hrService.SyncEmployeesAsync(employees, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Fetches employees from HR without syncing.
    /// </summary>
    [HttpGet("hr/employees")]
    [ProducesResponseType(typeof(IReadOnlyList<HrEmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> FetchEmployees(CancellationToken cancellationToken)
    {
        var employees = await _hrService.FetchEmployeesAsync(cancellationToken);
        return Ok(employees);
    }

    /// <summary>
    /// Fetches a specific employee from HR.
    /// </summary>
    [HttpGet("hr/employees/{employeeCode}")]
    [ProducesResponseType(typeof(HrEmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> FetchEmployee(
        string employeeCode,
        CancellationToken cancellationToken)
    {
        var employee = await _hrService.FetchEmployeeAsync(employeeCode, cancellationToken);

        if (employee == null)
            return NotFound(new { Error = $"Employee {employeeCode} not found in HR system" });

        return Ok(employee);
    }

    /// <summary>
    /// Gets departments from HR system.
    /// </summary>
    [HttpGet("hr/departments")]
    [ProducesResponseType(typeof(IReadOnlyList<string>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDepartments(CancellationToken cancellationToken)
    {
        var departments = await _hrService.GetDepartmentsAsync(cancellationToken);
        return Ok(departments);
    }

    #endregion

    #region Payroll Integration (Payout Export)

    /// <summary>
    /// Gets the Payroll integration status.
    /// </summary>
    [HttpGet("payroll/status")]
    [ProducesResponseType(typeof(IntegrationSyncStatusDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayrollStatus(CancellationToken cancellationToken)
    {
        var status = await _payrollService.GetSyncStatusAsync(cancellationToken);
        return Ok(status);
    }

    /// <summary>
    /// Tests the Payroll connection.
    /// </summary>
    [HttpPost("payroll/test")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestPayrollConnection(CancellationToken cancellationToken)
    {
        var result = await _payrollService.TestConnectionAsync(cancellationToken);
        return Ok(new { Connected = result, TestedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Exports approved payouts to payroll for a period.
    /// </summary>
    [HttpPost("payroll/export/{period}")]
    [ProducesResponseType(typeof(PayrollExportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ExportPayouts(
        string period,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Export payouts for period {Period}", period);

        if (!IsValidPeriod(period))
        {
            return BadRequest(new { Error = "Period must be in format YYYY-MM" });
        }

        var result = await _payrollService.ExportPayoutsAsync(period, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Exports specific calculations to payroll.
    /// </summary>
    [HttpPost("payroll/export")]
    [ProducesResponseType(typeof(PayrollExportResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportPayouts(
        [FromBody] IEnumerable<Guid> calculationIds,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Export specific calculations to payroll");

        var result = await _payrollService.ExportPayoutsAsync(calculationIds, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Gets payouts ready for export.
    /// </summary>
    [HttpGet("payroll/pending/{period}")]
    [ProducesResponseType(typeof(IReadOnlyList<PayrollPayoutDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingPayouts(
        string period,
        CancellationToken cancellationToken)
    {
        var payouts = await _payrollService.GetPayoutsForExportAsync(period, cancellationToken);
        return Ok(payouts);
    }

    /// <summary>
    /// Generates export file without sending to payroll.
    /// </summary>
    [HttpGet("payroll/download/{period}")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> DownloadPayrollFile(
        string period,
        [FromQuery] string format = "csv",
        CancellationToken cancellationToken = default)
    {
        var result = await _payrollService.GenerateExportFileAsync(period, format, cancellationToken);

        if (string.IsNullOrEmpty(result.ExportFilePath) || !System.IO.File.Exists(result.ExportFilePath))
        {
            return NotFound(new { Error = "Export file not generated" });
        }

        var content = await System.IO.File.ReadAllBytesAsync(result.ExportFilePath, cancellationToken);
        var contentType = format.ToLowerInvariant() switch
        {
            "csv" => "text/csv",
            "json" => "application/json",
            _ => "text/csv"
        };

        return File(content, contentType, Path.GetFileName(result.ExportFilePath));
    }

    /// <summary>
    /// Gets export history.
    /// </summary>
    [HttpGet("payroll/history")]
    [ProducesResponseType(typeof(IReadOnlyList<PayrollExportResultDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExportHistory(
        [FromQuery] string? period = null,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var history = await _payrollService.GetExportHistoryAsync(period, limit, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Retries failed exports.
    /// </summary>
    [HttpPost("payroll/retry/{batchId}")]
    [ProducesResponseType(typeof(PayrollExportResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> RetryFailedExports(
        string batchId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("API: Retry failed exports for batch {BatchId}", batchId);

        var result = await _payrollService.RetryFailedExportsAsync(batchId, cancellationToken);
        return Ok(result);
    }

    #endregion

    private static bool IsValidPeriod(string period)
    {
        return System.Text.RegularExpressions.Regex.IsMatch(period, @"^\d{4}-\d{2}$");
    }
}
