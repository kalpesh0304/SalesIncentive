using System.Text;
using Dorise.Incentive.Application.Integrations.DTOs;
using Dorise.Incentive.Application.Integrations.Services;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Integrations;

/// <summary>
/// Implementation of Payroll integration service for payout export.
/// "What's a battle?" - And what's a payroll? It's where the incentives go!
/// </summary>
public class PayrollIntegrationService : IPayrollIntegrationService
{
    private readonly ICalculationRepository _calculationRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IIncentivePlanRepository _planRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PayrollIntegrationService> _logger;

    private readonly List<PayrollExportResultDto> _exportHistory = new();
    private DateTime? _lastSyncAt;
    private string? _lastSyncStatus;
    private int? _lastSyncRecordCount;
    private string? _lastSyncError;

    public PayrollIntegrationService(
        ICalculationRepository calculationRepository,
        IEmployeeRepository employeeRepository,
        IIncentivePlanRepository planRepository,
        IDepartmentRepository departmentRepository,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<PayrollIntegrationService> logger)
    {
        _calculationRepository = calculationRepository;
        _employeeRepository = employeeRepository;
        _planRepository = planRepository;
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<PayrollExportResultDto> ExportPayoutsAsync(
        string period,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Exporting payouts to payroll for period {Period}", period);

        try
        {
            var payouts = await GetPayoutsForExportAsync(period, cancellationToken);
            var calculationIds = payouts.Select(p => p.CalculationId).ToList();

            return await ExportPayoutsAsync(calculationIds, cancellationToken);
        }
        catch (Exception ex)
        {
            _lastSyncStatus = "Failed";
            _lastSyncError = ex.Message;
            _logger.LogError(ex, "Failed to export payouts for period {Period}", period);
            throw;
        }
    }

    public async Task<PayrollExportResultDto> ExportPayoutsAsync(
        IEnumerable<Guid> calculationIds,
        CancellationToken cancellationToken = default)
    {
        var idList = calculationIds.ToList();
        var batchId = Guid.NewGuid().ToString();

        _logger.LogInformation("Processing {Count} calculations for payroll export, BatchId: {BatchId}",
            idList.Count, batchId);

        var errors = new List<ExportErrorDto>();
        var exportedCount = 0;
        var totalAmount = 0m;

        foreach (var calculationId in idList)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var calculation = await _calculationRepository.GetWithDetailsAsync(
                    calculationId, cancellationToken);

                if (calculation == null)
                {
                    errors.Add(new ExportErrorDto
                    {
                        CalculationId = calculationId,
                        ErrorMessage = "Calculation not found"
                    });
                    continue;
                }

                if (calculation.Status != CalculationStatus.Approved)
                {
                    errors.Add(new ExportErrorDto
                    {
                        CalculationId = calculationId,
                        EmployeeCode = calculation.Employee?.EmployeeCode?.Value,
                        ErrorMessage = $"Calculation is not approved. Current status: {calculation.Status}"
                    });
                    continue;
                }

                // In a real implementation, send to payroll system here
                // For demo, just mark as exported
                calculation.MarkPaid(batchId);
                _calculationRepository.Update(calculation);

                totalAmount += calculation.NetIncentive.Amount;
                exportedCount++;

                _logger.LogDebug("Exported calculation {CalculationId} for {Amount:C}",
                    calculationId, calculation.NetIncentive.Amount);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error exporting calculation {CalculationId}", calculationId);
                errors.Add(new ExportErrorDto
                {
                    CalculationId = calculationId,
                    ErrorMessage = ex.Message
                });
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _lastSyncAt = DateTime.UtcNow;
        _lastSyncStatus = errors.Count == 0 ? "Success" : "PartialSuccess";
        _lastSyncRecordCount = exportedCount;

        var result = new PayrollExportResultDto
        {
            TotalRecords = idList.Count,
            ExportedRecords = exportedCount,
            FailedRecords = errors.Count,
            TotalAmount = totalAmount,
            Errors = errors,
            ExportedAt = DateTime.UtcNow,
            BatchId = batchId
        };

        _exportHistory.Insert(0, result);
        if (_exportHistory.Count > 100) _exportHistory.RemoveAt(100);

        _logger.LogInformation(
            "Payroll export completed. Exported: {Exported}, Failed: {Failed}, Total: {Total:C}",
            exportedCount, errors.Count, totalAmount);

        return result;
    }

    public async Task<IReadOnlyList<PayrollPayoutDto>> GetPayoutsForExportAsync(
        string period,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting payouts ready for export for period {Period}", period);

        // Parse period
        var periodDate = DateTime.ParseExact(period + "-01", "yyyy-MM-dd", null);
        var dateRange = DateRange.Create(periodDate, periodDate.AddMonths(1).AddDays(-1));

        var calculations = await _calculationRepository.GetApprovedForPaymentAsync(
            dateRange, cancellationToken);

        var payouts = new List<PayrollPayoutDto>();

        foreach (var calc in calculations)
        {
            var employee = await _employeeRepository.GetByIdAsync(calc.EmployeeId, cancellationToken);
            var plan = await _planRepository.GetByIdAsync(calc.IncentivePlanId, cancellationToken);

            if (employee == null || plan == null) continue;

            // Fetch department name
            string? departmentName = null;
            if (employee.DepartmentId != Guid.Empty)
            {
                var department = await _departmentRepository.GetByIdAsync(
                    employee.DepartmentId, cancellationToken);
                departmentName = department?.Name;
            }

            // Calculate tax and deduction from gross vs net difference
            var taxAmount = calc.GrossIncentive.Amount - calc.NetIncentive.Amount;

            payouts.Add(new PayrollPayoutDto
            {
                EmployeeCode = employee.EmployeeCode.Value,
                EmployeeName = employee.FullName,
                Period = period,
                GrossAmount = calc.GrossIncentive.Amount,
                NetAmount = calc.NetIncentive.Amount,
                TaxAmount = taxAmount > 0 ? taxAmount : 0,
                DeductionAmount = 0m,
                PlanCode = plan.Code,
                PlanName = plan.Name,
                CalculationId = calc.Id,
                ApprovedAt = calc.ModifiedAt ?? calc.CreatedAt,
                ApprovedBy = calc.CalculatedBy,
                Department = departmentName
            });
        }

        _logger.LogInformation("Found {Count} approved payouts for export", payouts.Count);

        return payouts;
    }

    public async Task<PayrollExportResultDto> GenerateExportFileAsync(
        string period,
        string format = "csv",
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating export file for period {Period} in {Format} format",
            period, format);

        var payouts = await GetPayoutsForExportAsync(period, cancellationToken);

        var content = format.ToLowerInvariant() switch
        {
            "csv" => GenerateCsvContent(payouts),
            "json" => GenerateJsonContent(payouts),
            _ => GenerateCsvContent(payouts)
        };

        var fileName = $"PayrollExport_{period}_{DateTime.UtcNow:yyyyMMddHHmmss}.{format}";
        var filePath = Path.Combine(Path.GetTempPath(), fileName);
        await File.WriteAllTextAsync(filePath, content, cancellationToken);

        _logger.LogInformation("Generated export file: {FilePath}", filePath);

        return new PayrollExportResultDto
        {
            TotalRecords = payouts.Count,
            ExportedRecords = payouts.Count,
            FailedRecords = 0,
            TotalAmount = payouts.Sum(p => p.NetAmount),
            Errors = new List<ExportErrorDto>(),
            ExportedAt = DateTime.UtcNow,
            BatchId = Guid.NewGuid().ToString(),
            ExportFilePath = filePath
        };
    }

    public async Task MarkAsExportedAsync(
        IEnumerable<Guid> calculationIds,
        string batchId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Marking {Count} calculations as exported, BatchId: {BatchId}",
            calculationIds.Count(), batchId);

        foreach (var calculationId in calculationIds)
        {
            var calculation = await _calculationRepository.GetByIdAsync(
                calculationId, cancellationToken);

            if (calculation != null && calculation.Status == CalculationStatus.Approved)
            {
                calculation.MarkPaid(batchId);
                _calculationRepository.Update(calculation);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Marked calculations as exported");
    }

    public Task<IReadOnlyList<PayrollExportResultDto>> GetExportHistoryAsync(
        string? period = null,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var history = _exportHistory
            .Take(limit)
            .ToList();

        return Task.FromResult<IReadOnlyList<PayrollExportResultDto>>(history);
    }

    public async Task<PayrollExportResultDto> RetryFailedExportsAsync(
        string batchId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Retrying failed exports for batch {BatchId}", batchId);

        var previousExport = _exportHistory.FirstOrDefault(e => e.BatchId == batchId);
        if (previousExport == null)
        {
            return new PayrollExportResultDto
            {
                TotalRecords = 0,
                FailedRecords = 1,
                Errors = new List<ExportErrorDto>
                {
                    new ExportErrorDto
                    {
                        ErrorMessage = $"Batch {batchId} not found in export history"
                    }
                },
                ExportedAt = DateTime.UtcNow,
                BatchId = batchId
            };
        }

        // Get failed calculation IDs from previous export
        var failedIds = previousExport.Errors
            .Where(e => e.CalculationId != Guid.Empty)
            .Select(e => e.CalculationId)
            .ToList();

        if (!failedIds.Any())
        {
            return new PayrollExportResultDto
            {
                TotalRecords = 0,
                ExportedRecords = 0,
                Errors = new List<ExportErrorDto>(),
                ExportedAt = DateTime.UtcNow,
                BatchId = Guid.NewGuid().ToString()
            };
        }

        return await ExportPayoutsAsync(failedIds, cancellationToken);
    }

    public Task<IntegrationSyncStatusDto> GetSyncStatusAsync(
        CancellationToken cancellationToken = default)
    {
        var config = _configuration.GetSection("Integrations:Payroll");

        return Task.FromResult(new IntegrationSyncStatusDto
        {
            IntegrationType = "Payroll",
            LastSyncAt = _lastSyncAt,
            LastSyncStatus = _lastSyncStatus,
            LastSyncRecordCount = _lastSyncRecordCount,
            LastSyncError = _lastSyncError,
            NextScheduledSync = null, // Payroll is typically on-demand
            IsEnabled = config.GetValue<bool>("Enabled", true)
        });
    }

    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Testing Payroll connection");

        var endpoint = _configuration["Integrations:Payroll:Endpoint"];
        var isConfigured = !string.IsNullOrEmpty(endpoint);

        _logger.LogInformation("Payroll connection test result: {Result}",
            isConfigured ? "Success" : "NotConfigured");

        return Task.FromResult(isConfigured);
    }

    private static string GenerateCsvContent(IReadOnlyList<PayrollPayoutDto> payouts)
    {
        var sb = new StringBuilder();

        // Header
        sb.AppendLine("EmployeeCode,EmployeeName,Period,GrossAmount,NetAmount,TaxAmount,DeductionAmount,PlanCode,PlanName,Department,ApprovedAt,ApprovedBy");

        // Data
        foreach (var payout in payouts)
        {
            sb.AppendLine($"{payout.EmployeeCode},{EscapeCsv(payout.EmployeeName)},{payout.Period},{payout.GrossAmount:F2},{payout.NetAmount:F2},{payout.TaxAmount:F2},{payout.DeductionAmount:F2},{payout.PlanCode},{EscapeCsv(payout.PlanName)},{EscapeCsv(payout.Department ?? "")},{payout.ApprovedAt:yyyy-MM-dd},{payout.ApprovedBy ?? ""}");
        }

        return sb.ToString();
    }

    private static string GenerateJsonContent(IReadOnlyList<PayrollPayoutDto> payouts)
    {
        return System.Text.Json.JsonSerializer.Serialize(payouts, new System.Text.Json.JsonSerializerOptions
        {
            WriteIndented = true
        });
    }

    private static string EscapeCsv(string value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;

        if (value.Contains(',') || value.Contains('"') || value.Contains('\n'))
        {
            return $"\"{value.Replace("\"", "\"\"")}\"";
        }

        return value;
    }
}
