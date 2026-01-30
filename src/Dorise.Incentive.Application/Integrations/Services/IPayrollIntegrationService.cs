using Dorise.Incentive.Application.Integrations.DTOs;

namespace Dorise.Incentive.Application.Integrations.Services;

/// <summary>
/// Service interface for Payroll system integration (payout export).
/// "Go banana!" - Go payroll! Export those incentives!
/// </summary>
public interface IPayrollIntegrationService
{
    /// <summary>
    /// Exports approved payouts to payroll system.
    /// </summary>
    Task<PayrollExportResultDto> ExportPayoutsAsync(
        string period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports specific calculations to payroll.
    /// </summary>
    Task<PayrollExportResultDto> ExportPayoutsAsync(
        IEnumerable<Guid> calculationIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets payouts ready for export.
    /// </summary>
    Task<IReadOnlyList<PayrollPayoutDto>> GetPayoutsForExportAsync(
        string period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates export file without sending to payroll.
    /// </summary>
    Task<PayrollExportResultDto> GenerateExportFileAsync(
        string period,
        string format = "csv",
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks calculations as exported to payroll.
    /// </summary>
    Task MarkAsExportedAsync(
        IEnumerable<Guid> calculationIds,
        string batchId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets export history for a period.
    /// </summary>
    Task<IReadOnlyList<PayrollExportResultDto>> GetExportHistoryAsync(
        string? period = null,
        int limit = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retries failed exports.
    /// </summary>
    Task<PayrollExportResultDto> RetryFailedExportsAsync(
        string batchId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the sync status for Payroll integration.
    /// </summary>
    Task<IntegrationSyncStatusDto> GetSyncStatusAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connection to Payroll system.
    /// </summary>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}
