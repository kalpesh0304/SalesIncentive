using Dorise.Incentive.Application.Integrations.DTOs;
using Dorise.Incentive.Application.Integrations.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.Integrations.Commands;

/// <summary>
/// Handler for ImportSalesDataCommand.
/// "Look Big Daddy, it's Regular Daddy!" - And look, it's regular sales data import!
/// </summary>
public class ImportSalesDataCommandHandler : IRequestHandler<ImportSalesDataCommand, SalesDataImportResultDto>
{
    private readonly IErpIntegrationService _erpService;
    private readonly ILogger<ImportSalesDataCommandHandler> _logger;

    public ImportSalesDataCommandHandler(
        IErpIntegrationService erpService,
        ILogger<ImportSalesDataCommandHandler> logger)
    {
        _erpService = erpService;
        _logger = logger;
    }

    public async Task<SalesDataImportResultDto> Handle(
        ImportSalesDataCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ImportSalesDataCommand for period {Period}", request.Period);

        if (request.ValidateOnly)
        {
            var data = await _erpService.FetchSalesDataAsync(request.Period, cancellationToken);
            var errors = await _erpService.ValidateSalesDataAsync(data, cancellationToken);

            return new SalesDataImportResultDto
            {
                TotalRecords = data.Count,
                SuccessfulRecords = errors.Any() ? 0 : data.Count,
                FailedRecords = errors.Any() ? data.Count : 0,
                Errors = errors.ToList(),
                ImportedAt = DateTime.UtcNow,
                BatchId = "VALIDATE-ONLY"
            };
        }

        return await _erpService.ImportSalesDataAsync(request.Period, cancellationToken);
    }
}

/// <summary>
/// Handler for ImportSalesDataBatchCommand.
/// </summary>
public class ImportSalesDataBatchCommandHandler : IRequestHandler<ImportSalesDataBatchCommand, SalesDataImportResultDto>
{
    private readonly IErpIntegrationService _erpService;
    private readonly ILogger<ImportSalesDataBatchCommandHandler> _logger;

    public ImportSalesDataBatchCommandHandler(
        IErpIntegrationService erpService,
        ILogger<ImportSalesDataBatchCommandHandler> logger)
    {
        _erpService = erpService;
        _logger = logger;
    }

    public async Task<SalesDataImportResultDto> Handle(
        ImportSalesDataBatchCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ImportSalesDataBatchCommand");

        return await _erpService.ImportSalesDataAsync(request.SalesData, cancellationToken);
    }
}

/// <summary>
/// Handler for SyncEmployeesCommand.
/// </summary>
public class SyncEmployeesCommandHandler : IRequestHandler<SyncEmployeesCommand, HrSyncResultDto>
{
    private readonly IHrIntegrationService _hrService;
    private readonly ILogger<SyncEmployeesCommandHandler> _logger;

    public SyncEmployeesCommandHandler(
        IHrIntegrationService hrService,
        ILogger<SyncEmployeesCommandHandler> logger)
    {
        _hrService = hrService;
        _logger = logger;
    }

    public async Task<HrSyncResultDto> Handle(
        SyncEmployeesCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling SyncEmployeesCommand. Incremental: {Incremental}",
            request.IncrementalOnly);

        if (request.IncrementalOnly && request.Since.HasValue)
        {
            var modifiedEmployees = await _hrService.FetchModifiedEmployeesAsync(
                request.Since.Value, cancellationToken);
            return await _hrService.SyncEmployeesAsync(modifiedEmployees, cancellationToken);
        }

        return await _hrService.SyncEmployeesAsync(cancellationToken);
    }
}

/// <summary>
/// Handler for SyncEmployeeCommand.
/// </summary>
public class SyncEmployeeCommandHandler : IRequestHandler<SyncEmployeeCommand, HrSyncResultDto>
{
    private readonly IHrIntegrationService _hrService;
    private readonly ILogger<SyncEmployeeCommandHandler> _logger;

    public SyncEmployeeCommandHandler(
        IHrIntegrationService hrService,
        ILogger<SyncEmployeeCommandHandler> logger)
    {
        _hrService = hrService;
        _logger = logger;
    }

    public async Task<HrSyncResultDto> Handle(
        SyncEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling SyncEmployeeCommand for {EmployeeCode}",
            request.EmployeeCode);

        return await _hrService.SyncEmployeeAsync(request.EmployeeCode, cancellationToken);
    }
}

/// <summary>
/// Handler for SyncEmployeesBatchCommand.
/// </summary>
public class SyncEmployeesBatchCommandHandler : IRequestHandler<SyncEmployeesBatchCommand, HrSyncResultDto>
{
    private readonly IHrIntegrationService _hrService;
    private readonly ILogger<SyncEmployeesBatchCommandHandler> _logger;

    public SyncEmployeesBatchCommandHandler(
        IHrIntegrationService hrService,
        ILogger<SyncEmployeesBatchCommandHandler> logger)
    {
        _hrService = hrService;
        _logger = logger;
    }

    public async Task<HrSyncResultDto> Handle(
        SyncEmployeesBatchCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling SyncEmployeesBatchCommand");

        return await _hrService.SyncEmployeesAsync(request.Employees, cancellationToken);
    }
}

/// <summary>
/// Handler for ExportPayoutsCommand.
/// </summary>
public class ExportPayoutsCommandHandler : IRequestHandler<ExportPayoutsCommand, PayrollExportResultDto>
{
    private readonly IPayrollIntegrationService _payrollService;
    private readonly ILogger<ExportPayoutsCommandHandler> _logger;

    public ExportPayoutsCommandHandler(
        IPayrollIntegrationService payrollService,
        ILogger<ExportPayoutsCommandHandler> logger)
    {
        _payrollService = payrollService;
        _logger = logger;
    }

    public async Task<PayrollExportResultDto> Handle(
        ExportPayoutsCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ExportPayoutsCommand for period {Period}",
            request.Period);

        return await _payrollService.ExportPayoutsAsync(request.Period, cancellationToken);
    }
}

/// <summary>
/// Handler for ExportCalculationsCommand.
/// </summary>
public class ExportCalculationsCommandHandler : IRequestHandler<ExportCalculationsCommand, PayrollExportResultDto>
{
    private readonly IPayrollIntegrationService _payrollService;
    private readonly ILogger<ExportCalculationsCommandHandler> _logger;

    public ExportCalculationsCommandHandler(
        IPayrollIntegrationService payrollService,
        ILogger<ExportCalculationsCommandHandler> logger)
    {
        _payrollService = payrollService;
        _logger = logger;
    }

    public async Task<PayrollExportResultDto> Handle(
        ExportCalculationsCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling ExportCalculationsCommand for {Count} calculations",
            request.CalculationIds.Count());

        return await _payrollService.ExportPayoutsAsync(request.CalculationIds, cancellationToken);
    }
}

/// <summary>
/// Handler for RetryPayrollExportCommand.
/// </summary>
public class RetryPayrollExportCommandHandler : IRequestHandler<RetryPayrollExportCommand, PayrollExportResultDto>
{
    private readonly IPayrollIntegrationService _payrollService;
    private readonly ILogger<RetryPayrollExportCommandHandler> _logger;

    public RetryPayrollExportCommandHandler(
        IPayrollIntegrationService payrollService,
        ILogger<RetryPayrollExportCommandHandler> logger)
    {
        _payrollService = payrollService;
        _logger = logger;
    }

    public async Task<PayrollExportResultDto> Handle(
        RetryPayrollExportCommand request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling RetryPayrollExportCommand for batch {BatchId}",
            request.BatchId);

        return await _payrollService.RetryFailedExportsAsync(request.BatchId, cancellationToken);
    }
}

/// <summary>
/// Handler for RunScheduledSyncCommand.
/// </summary>
public class RunScheduledSyncCommandHandler : IRequestHandler<RunScheduledSyncCommand, ScheduledSyncResultDto>
{
    private readonly IErpIntegrationService _erpService;
    private readonly IHrIntegrationService _hrService;
    private readonly ILogger<RunScheduledSyncCommandHandler> _logger;

    public RunScheduledSyncCommandHandler(
        IErpIntegrationService erpService,
        IHrIntegrationService hrService,
        ILogger<RunScheduledSyncCommandHandler> logger)
    {
        _erpService = erpService;
        _hrService = hrService;
        _logger = logger;
    }

    public async Task<ScheduledSyncResultDto> Handle(
        RunScheduledSyncCommand request,
        CancellationToken cancellationToken)
    {
        var jobId = Guid.NewGuid().ToString();
        _logger.LogInformation("Handling RunScheduledSyncCommand. JobId: {JobId}", jobId);

        var errors = new List<string>();
        SalesDataImportResultDto? erpResult = null;
        HrSyncResultDto? hrResult = null;

        // Run ERP sync
        if (request.IncludeErp)
        {
            try
            {
                var period = request.Period ?? DateTime.UtcNow.ToString("yyyy-MM");
                erpResult = await _erpService.ImportSalesDataAsync(period, cancellationToken);
                _logger.LogInformation("ERP sync completed. Success: {Success}, Failed: {Failed}",
                    erpResult.SuccessfulRecords, erpResult.FailedRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ERP sync failed");
                errors.Add($"ERP: {ex.Message}");
            }
        }

        // Run HR sync
        if (request.IncludeHr)
        {
            try
            {
                hrResult = await _hrService.SyncEmployeesAsync(cancellationToken);
                _logger.LogInformation("HR sync completed. New: {New}, Updated: {Updated}, Failed: {Failed}",
                    hrResult.NewEmployees, hrResult.UpdatedEmployees, hrResult.FailedRecords);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "HR sync failed");
                errors.Add($"HR: {ex.Message}");
            }
        }

        var result = new ScheduledSyncResultDto
        {
            ErpResult = erpResult,
            HrResult = hrResult,
            ExecutedAt = DateTime.UtcNow,
            JobId = jobId,
            OverallSuccess = !errors.Any(),
            Errors = errors
        };

        _logger.LogInformation("Scheduled sync completed. JobId: {JobId}, Success: {Success}",
            jobId, result.OverallSuccess);

        return result;
    }
}
