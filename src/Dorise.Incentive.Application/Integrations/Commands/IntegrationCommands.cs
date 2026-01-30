using Dorise.Incentive.Application.Integrations.DTOs;
using MediatR;

namespace Dorise.Incentive.Application.Integrations.Commands;

/// <summary>
/// Command to trigger ERP sales data import.
/// "I dress myself!" - And I import sales data myself!
/// </summary>
public record ImportSalesDataCommand(
    string Period,
    bool ValidateOnly = false
) : IRequest<SalesDataImportResultDto>;

/// <summary>
/// Command to import sales data from provided records.
/// </summary>
public record ImportSalesDataBatchCommand(
    IEnumerable<SalesDataImportDto> SalesData
) : IRequest<SalesDataImportResultDto>;

/// <summary>
/// Command to trigger HR employee sync.
/// </summary>
public record SyncEmployeesCommand(
    bool IncrementalOnly = false,
    DateTime? Since = null
) : IRequest<HrSyncResultDto>;

/// <summary>
/// Command to sync a single employee from HR.
/// </summary>
public record SyncEmployeeCommand(
    string EmployeeCode
) : IRequest<HrSyncResultDto>;

/// <summary>
/// Command to sync employees from provided data.
/// </summary>
public record SyncEmployeesBatchCommand(
    IEnumerable<HrEmployeeDto> Employees
) : IRequest<HrSyncResultDto>;

/// <summary>
/// Command to export payouts to payroll.
/// </summary>
public record ExportPayoutsCommand(
    string Period
) : IRequest<PayrollExportResultDto>;

/// <summary>
/// Command to export specific calculations to payroll.
/// </summary>
public record ExportCalculationsCommand(
    IEnumerable<Guid> CalculationIds
) : IRequest<PayrollExportResultDto>;

/// <summary>
/// Command to retry failed payroll exports.
/// </summary>
public record RetryPayrollExportCommand(
    string BatchId
) : IRequest<PayrollExportResultDto>;

/// <summary>
/// Command to run scheduled integration sync jobs.
/// </summary>
public record RunScheduledSyncCommand(
    bool IncludeErp = true,
    bool IncludeHr = true,
    string? Period = null
) : IRequest<ScheduledSyncResultDto>;

/// <summary>
/// Result of scheduled sync operation.
/// </summary>
public record ScheduledSyncResultDto
{
    public SalesDataImportResultDto? ErpResult { get; init; }
    public HrSyncResultDto? HrResult { get; init; }
    public DateTime ExecutedAt { get; init; }
    public string JobId { get; init; } = string.Empty;
    public bool OverallSuccess { get; init; }
    public List<string> Errors { get; init; } = new();
}
