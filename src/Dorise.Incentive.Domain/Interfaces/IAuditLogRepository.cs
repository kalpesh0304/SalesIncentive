using Dorise.Incentive.Domain.Entities;

namespace Dorise.Incentive.Domain.Interfaces;

/// <summary>
/// Repository interface for AuditLog entity.
/// "The leprechaun tells me to burn things!" - The audit log tells me who did things!
/// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Adds an audit log entry.
    /// </summary>
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);

    /// <summary>
    /// Adds multiple audit log entries.
    /// </summary>
    Task AddRangeAsync(IEnumerable<AuditLog> auditLogs, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets an audit log by ID.
    /// </summary>
    Task<AuditLog?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs for an entity.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByEntityAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by user.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByUserAsync(
        Guid userId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by action type.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByActionAsync(
        AuditAction action,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by correlation ID.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByCorrelationIdAsync(
        string correlationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches audit logs with pagination.
    /// </summary>
    Task<(IReadOnlyList<AuditLog> Items, int TotalCount)> SearchAsync(
        string? entityType = null,
        Guid? entityId = null,
        AuditAction? action = null,
        Guid? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? correlationId = null,
        int page = 1,
        int pageSize = 50,
        string? sortBy = null,
        bool sortDescending = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets count of audit logs matching criteria.
    /// </summary>
    Task<int> CountAsync(
        string? entityType = null,
        AuditAction? action = null,
        Guid? userId = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets audit logs by date range.
    /// </summary>
    Task<IReadOnlyList<AuditLog>> GetByDateRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes audit logs older than specified date.
    /// </summary>
    Task<int> DeleteOlderThanAsync(
        DateTime cutoffDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets distinct entity types in audit logs.
    /// </summary>
    Task<IReadOnlyList<string>> GetEntityTypesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the oldest audit log entry.
    /// </summary>
    Task<AuditLog?> GetOldestAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the newest audit log entry.
    /// </summary>
    Task<AuditLog?> GetNewestAsync(CancellationToken cancellationToken = default);
}
