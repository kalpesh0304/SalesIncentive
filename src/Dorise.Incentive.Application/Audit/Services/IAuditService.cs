using Dorise.Incentive.Application.Audit.DTOs;
using Dorise.Incentive.Domain.Entities;

namespace Dorise.Incentive.Application.Audit.Services;

/// <summary>
/// Service interface for audit logging and compliance tracking.
/// "They taste like burning!" - Bad changes burn, good audit logs don't!
/// </summary>
public interface IAuditService
{
    #region Audit Logging

    /// <summary>
    /// Logs an audit entry.
    /// </summary>
    Task LogAsync(
        CreateAuditLogRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a create action.
    /// </summary>
    Task LogCreateAsync<T>(
        T entity,
        string? reason = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Logs an update action.
    /// </summary>
    Task LogUpdateAsync<T>(
        T oldEntity,
        T newEntity,
        string? reason = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Logs a delete action.
    /// </summary>
    Task LogDeleteAsync<T>(
        T entity,
        string? reason = null,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Logs a view/access action.
    /// </summary>
    Task LogAccessAsync(
        string entityType,
        Guid entityId,
        bool isSensitive = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an export action.
    /// </summary>
    Task LogExportAsync(
        string entityType,
        int recordCount,
        string format,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an approval action.
    /// </summary>
    Task LogApprovalAsync(
        Guid entityId,
        bool approved,
        string? comments = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a login event.
    /// </summary>
    Task LogLoginAsync(
        Guid userId,
        string userName,
        bool success,
        string? failureReason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a logout event.
    /// </summary>
    Task LogLogoutAsync(
        Guid userId,
        string userName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a system event.
    /// </summary>
    Task LogSystemEventAsync(
        string eventType,
        string description,
        Dictionary<string, object>? data = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Audit Queries

    /// <summary>
    /// Searches audit logs with filters.
    /// </summary>
    Task<AuditLogPagedResult> SearchAsync(
        AuditLogSearchQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit log by ID.
    /// </summary>
    Task<AuditLogDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit history for an entity.
    /// </summary>
    Task<IReadOnlyList<AuditLogDto>> GetEntityHistoryAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit summary for an entity.
    /// </summary>
    Task<EntityAuditSummaryDto> GetEntitySummaryAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user activity summary.
    /// </summary>
    Task<UserActivitySummaryDto> GetUserActivityAsync(
        Guid userId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit statistics.
    /// </summary>
    Task<AuditStatisticsDto> GetStatisticsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Compliance Reports

    /// <summary>
    /// Gets approval compliance report.
    /// </summary>
    Task<ApprovalComplianceReportDto> GetApprovalComplianceReportAsync(
        string period,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets data access report.
    /// </summary>
    Task<DataAccessReportDto> GetDataAccessReportAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets system change log.
    /// </summary>
    Task<IReadOnlyList<SystemChangeLogDto>> GetSystemChangeLogAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    #endregion

    #region Retention

    /// <summary>
    /// Gets retention policy.
    /// </summary>
    Task<RetentionPolicyDto> GetRetentionPolicyAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Purges old audit logs based on retention policy.
    /// </summary>
    Task<int> PurgeOldLogsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Archives audit logs for a period.
    /// </summary>
    Task<string> ArchiveLogsAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    #endregion
}
