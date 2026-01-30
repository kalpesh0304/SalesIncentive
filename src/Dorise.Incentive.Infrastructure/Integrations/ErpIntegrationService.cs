using Dorise.Incentive.Application.Integrations.DTOs;
using Dorise.Incentive.Application.Integrations.Services;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Integrations;

/// <summary>
/// Implementation of ERP integration service for sales data import.
/// "My cat's breath smells like cat food." - And this service smells like sales data!
/// </summary>
public class ErpIntegrationService : IErpIntegrationService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ICalculationRepository _calculationRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ErpIntegrationService> _logger;

    private DateTime? _lastSyncAt;
    private string? _lastSyncStatus;
    private int? _lastSyncRecordCount;
    private string? _lastSyncError;

    public ErpIntegrationService(
        IEmployeeRepository employeeRepository,
        ICalculationRepository calculationRepository,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<ErpIntegrationService> logger)
    {
        _employeeRepository = employeeRepository;
        _calculationRepository = calculationRepository;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SalesDataImportResultDto> ImportSalesDataAsync(
        string period,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting sales data import from ERP for period {Period}", period);

        try
        {
            var salesData = await FetchSalesDataAsync(period, cancellationToken);
            return await ImportSalesDataAsync(salesData, cancellationToken);
        }
        catch (Exception ex)
        {
            _lastSyncStatus = "Failed";
            _lastSyncError = ex.Message;
            _logger.LogError(ex, "Failed to import sales data for period {Period}", period);
            throw;
        }
    }

    public async Task<SalesDataImportResultDto> ImportSalesDataAsync(
        IEnumerable<SalesDataImportDto> salesData,
        CancellationToken cancellationToken = default)
    {
        var dataList = salesData.ToList();
        var result = new SalesDataImportResultDto
        {
            TotalRecords = dataList.Count,
            ImportedAt = DateTime.UtcNow,
            BatchId = Guid.NewGuid().ToString()
        };

        _logger.LogInformation("Processing {Count} sales records for import", dataList.Count);

        var errors = new List<ImportErrorDto>();
        var successCount = 0;
        var skipCount = 0;
        var failCount = 0;
        var rowNumber = 0;

        foreach (var record in dataList)
        {
            rowNumber++;
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                // Validate the record
                var validationErrors = ValidateSalesRecord(record, rowNumber);
                if (validationErrors.Any())
                {
                    errors.AddRange(validationErrors);
                    failCount++;
                    continue;
                }

                // Find the employee
                var employee = await _employeeRepository.GetByCodeAsync(
                    new EmployeeCode(record.EmployeeCode),
                    cancellationToken);

                if (employee == null)
                {
                    errors.Add(new ImportErrorDto
                    {
                        RowNumber = rowNumber,
                        EmployeeCode = record.EmployeeCode,
                        ErrorMessage = $"Employee not found: {record.EmployeeCode}",
                        FieldName = "EmployeeCode"
                    });
                    failCount++;
                    continue;
                }

                // Check if data already exists for this period
                var period = DateRange.Create(
                    DateTime.ParseExact(record.Period + "-01", "yyyy-MM-dd", null),
                    DateTime.ParseExact(record.Period + "-01", "yyyy-MM-dd", null).AddMonths(1).AddDays(-1));

                var existingCalc = await _calculationRepository.GetLatestAsync(
                    employee.Id,
                    Guid.Empty, // TODO: Get plan ID from context
                    period,
                    cancellationToken);

                if (existingCalc != null)
                {
                    _logger.LogDebug(
                        "Skipping existing calculation for employee {EmployeeCode} period {Period}",
                        record.EmployeeCode, record.Period);
                    skipCount++;
                    continue;
                }

                // Store sales data (in a real implementation, this would update a sales data table)
                // For now, we just log it as successfully processed
                _logger.LogDebug(
                    "Imported sales data for {EmployeeCode}: Sales={Sales:C}, Target={Target:C}",
                    record.EmployeeCode, record.SalesAmount, record.TargetAmount);

                successCount++;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error processing row {RowNumber}", rowNumber);
                errors.Add(new ImportErrorDto
                {
                    RowNumber = rowNumber,
                    EmployeeCode = record.EmployeeCode,
                    ErrorMessage = ex.Message
                });
                failCount++;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _lastSyncAt = DateTime.UtcNow;
        _lastSyncStatus = failCount == 0 ? "Success" : "PartialSuccess";
        _lastSyncRecordCount = successCount;

        var importResult = result with
        {
            SuccessfulRecords = successCount,
            FailedRecords = failCount,
            SkippedRecords = skipCount,
            Errors = errors
        };

        _logger.LogInformation(
            "Sales data import completed. Success: {Success}, Failed: {Failed}, Skipped: {Skipped}",
            successCount, failCount, skipCount);

        return importResult;
    }

    public Task<IReadOnlyList<SalesDataImportDto>> FetchSalesDataAsync(
        string period,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching sales data from ERP for period {Period}", period);

        // In a real implementation, this would call an external ERP API
        // For now, return sample data for demonstration
        var sampleData = new List<SalesDataImportDto>
        {
            new SalesDataImportDto
            {
                EmployeeCode = "EMP001",
                Period = period,
                SalesAmount = 125000m,
                TargetAmount = 100000m,
                ProductCategory = "Electronics",
                Region = "North",
                Channel = "Direct",
                TransactionDate = DateTime.UtcNow,
                ExternalReference = $"ERP-{Guid.NewGuid():N}"
            },
            new SalesDataImportDto
            {
                EmployeeCode = "EMP002",
                Period = period,
                SalesAmount = 95000m,
                TargetAmount = 100000m,
                ProductCategory = "Software",
                Region = "South",
                Channel = "Partner",
                TransactionDate = DateTime.UtcNow,
                ExternalReference = $"ERP-{Guid.NewGuid():N}"
            }
        };

        return Task.FromResult<IReadOnlyList<SalesDataImportDto>>(sampleData);
    }

    public async Task<IReadOnlyList<SalesDataImportDto>> FetchSalesDataForEmployeeAsync(
        string employeeCode,
        string period,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Fetching sales data from ERP for employee {EmployeeCode} period {Period}",
            employeeCode, period);

        var allData = await FetchSalesDataAsync(period, cancellationToken);
        return allData.Where(d => d.EmployeeCode == employeeCode).ToList();
    }

    public Task<IReadOnlyList<ImportErrorDto>> ValidateSalesDataAsync(
        IEnumerable<SalesDataImportDto> salesData,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<ImportErrorDto>();
        var rowNumber = 0;

        foreach (var record in salesData)
        {
            rowNumber++;
            errors.AddRange(ValidateSalesRecord(record, rowNumber));
        }

        _logger.LogInformation("Validated {Count} records, found {ErrorCount} errors",
            rowNumber, errors.Count);

        return Task.FromResult<IReadOnlyList<ImportErrorDto>>(errors);
    }

    public Task<IntegrationSyncStatusDto> GetSyncStatusAsync(
        CancellationToken cancellationToken = default)
    {
        var config = _configuration.GetSection("Integrations:Erp");

        return Task.FromResult(new IntegrationSyncStatusDto
        {
            IntegrationType = "ERP",
            LastSyncAt = _lastSyncAt,
            LastSyncStatus = _lastSyncStatus,
            LastSyncRecordCount = _lastSyncRecordCount,
            LastSyncError = _lastSyncError,
            NextScheduledSync = _lastSyncAt?.AddMinutes(
                config.GetValue<int>("SyncIntervalMinutes", 60)),
            IsEnabled = config.GetValue<bool>("Enabled", true)
        });
    }

    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Testing ERP connection");

        // In a real implementation, this would test the actual connection
        var endpoint = _configuration["Integrations:Erp:Endpoint"];
        var isConfigured = !string.IsNullOrEmpty(endpoint);

        _logger.LogInformation("ERP connection test result: {Result}",
            isConfigured ? "Success" : "NotConfigured");

        return Task.FromResult(isConfigured);
    }

    private static List<ImportErrorDto> ValidateSalesRecord(SalesDataImportDto record, int rowNumber)
    {
        var errors = new List<ImportErrorDto>();

        if (string.IsNullOrWhiteSpace(record.EmployeeCode))
        {
            errors.Add(new ImportErrorDto
            {
                RowNumber = rowNumber,
                EmployeeCode = record.EmployeeCode,
                ErrorMessage = "Employee code is required",
                FieldName = "EmployeeCode"
            });
        }

        if (string.IsNullOrWhiteSpace(record.Period))
        {
            errors.Add(new ImportErrorDto
            {
                RowNumber = rowNumber,
                EmployeeCode = record.EmployeeCode,
                ErrorMessage = "Period is required",
                FieldName = "Period"
            });
        }
        else if (!System.Text.RegularExpressions.Regex.IsMatch(record.Period, @"^\d{4}-\d{2}$"))
        {
            errors.Add(new ImportErrorDto
            {
                RowNumber = rowNumber,
                EmployeeCode = record.EmployeeCode,
                ErrorMessage = "Period must be in format YYYY-MM",
                FieldName = "Period"
            });
        }

        if (record.SalesAmount < 0)
        {
            errors.Add(new ImportErrorDto
            {
                RowNumber = rowNumber,
                EmployeeCode = record.EmployeeCode,
                ErrorMessage = "Sales amount cannot be negative",
                FieldName = "SalesAmount"
            });
        }

        if (record.TargetAmount < 0)
        {
            errors.Add(new ImportErrorDto
            {
                RowNumber = rowNumber,
                EmployeeCode = record.EmployeeCode,
                ErrorMessage = "Target amount cannot be negative",
                FieldName = "TargetAmount"
            });
        }

        return errors;
    }
}
