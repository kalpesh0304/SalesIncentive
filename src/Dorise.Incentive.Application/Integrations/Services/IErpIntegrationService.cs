using Dorise.Incentive.Application.Integrations.DTOs;

namespace Dorise.Incentive.Application.Integrations.Services;

/// <summary>
/// Service interface for ERP system integration (sales data import).
/// "The doctor said I wouldn't have so many nose bleeds if I kept my finger outta there."
/// - And you wouldn't have missing sales data if you kept the ERP synced!
/// </summary>
public interface IErpIntegrationService
{
    /// <summary>
    /// Imports sales data from ERP for a specific period.
    /// </summary>
    Task<SalesDataImportResultDto> ImportSalesDataAsync(
        string period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports sales data from provided records.
    /// </summary>
    Task<SalesDataImportResultDto> ImportSalesDataAsync(
        IEnumerable<SalesDataImportDto> salesData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches sales data from ERP without importing.
    /// </summary>
    Task<IReadOnlyList<SalesDataImportDto>> FetchSalesDataAsync(
        string period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches sales data for a specific employee.
    /// </summary>
    Task<IReadOnlyList<SalesDataImportDto>> FetchSalesDataForEmployeeAsync(
        string employeeCode,
        string period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates sales data before import.
    /// </summary>
    Task<IReadOnlyList<ImportErrorDto>> ValidateSalesDataAsync(
        IEnumerable<SalesDataImportDto> salesData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the sync status for ERP integration.
    /// </summary>
    Task<IntegrationSyncStatusDto> GetSyncStatusAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connection to ERP system.
    /// </summary>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}
