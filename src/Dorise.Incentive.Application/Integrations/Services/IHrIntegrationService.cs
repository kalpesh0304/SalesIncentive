using Dorise.Incentive.Application.Integrations.DTOs;

namespace Dorise.Incentive.Application.Integrations.Services;

/// <summary>
/// Service interface for HR system integration (employee sync).
/// "Mrs. Krabappel and Principal Skinner were in the closet making babies and I saw one of
/// the babies and the baby looked at me!" - And the HR system looked at me and gave me employee data!
/// </summary>
public interface IHrIntegrationService
{
    /// <summary>
    /// Syncs all employees from HR system.
    /// </summary>
    Task<HrSyncResultDto> SyncEmployeesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs employees from provided data.
    /// </summary>
    Task<HrSyncResultDto> SyncEmployeesAsync(
        IEnumerable<HrEmployeeDto> employees,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches all employees from HR without syncing.
    /// </summary>
    Task<IReadOnlyList<HrEmployeeDto>> FetchEmployeesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches a specific employee from HR.
    /// </summary>
    Task<HrEmployeeDto?> FetchEmployeeAsync(
        string employeeCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Fetches employees modified since a specific date.
    /// </summary>
    Task<IReadOnlyList<HrEmployeeDto>> FetchModifiedEmployeesAsync(
        DateTime since,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Syncs a single employee from HR.
    /// </summary>
    Task<HrSyncResultDto> SyncEmployeeAsync(
        string employeeCode,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets departments from HR system.
    /// </summary>
    Task<IReadOnlyList<string>> GetDepartmentsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the sync status for HR integration.
    /// </summary>
    Task<IntegrationSyncStatusDto> GetSyncStatusAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connection to HR system.
    /// </summary>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}
