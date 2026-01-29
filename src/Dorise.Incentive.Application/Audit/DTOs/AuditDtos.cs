using Dorise.Incentive.Domain.Entities;

namespace Dorise.Incentive.Application.Audit.DTOs;

/// <summary>
/// DTO for audit log entries.
/// "Slow down, Bart! My legs don't know how to be as long as yours." - Audit logs keep pace with every change!
/// </summary>
public record AuditLogDto
{
    public Guid Id { get; init; }
    public required string EntityType { get; init; }
    public Guid EntityId { get; init; }
    public AuditAction Action { get; init; }
    public string ActionName => Action.ToString();
    public Dictionary<string, object>? OldValues { get; init; }
    public Dictionary<string, object>? NewValues { get; init; }
    public List<string>? ChangedProperties { get; init; }
    public Guid? UserId { get; init; }
    public string? UserName { get; init; }
    public string? UserEmail { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public string? Reason { get; init; }
    public string? CorrelationId { get; init; }
    public DateTime Timestamp { get; init; }
    public Dictionary<string, object>? AdditionalData { get; init; }
}

/// <summary>
/// Request to create an audit log entry.
/// </summary>
public record CreateAuditLogRequest
{
    public required string EntityType { get; init; }
    public Guid EntityId { get; init; }
    public AuditAction Action { get; init; }
    public object? OldValue { get; init; }
    public object? NewValue { get; init; }
    public string? Reason { get; init; }
    public Dictionary<string, object>? AdditionalData { get; init; }
}

/// <summary>
/// Query parameters for searching audit logs.
/// </summary>
public record AuditLogSearchQuery
{
    public string? EntityType { get; init; }
    public Guid? EntityId { get; init; }
    public AuditAction? Action { get; init; }
    public Guid? UserId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? CorrelationId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
    public string? SortBy { get; init; }
    public bool SortDescending { get; init; } = true;
}

/// <summary>
/// Paginated result for audit logs.
/// </summary>
public record AuditLogPagedResult
{
    public IReadOnlyList<AuditLogDto> Items { get; init; } = new List<AuditLogDto>();
    public int TotalCount { get; init; }
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Audit summary for an entity.
/// </summary>
public record EntityAuditSummaryDto
{
    public required string EntityType { get; init; }
    public Guid EntityId { get; init; }
    public int TotalChanges { get; init; }
    public DateTime? FirstChange { get; init; }
    public DateTime? LastChange { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? CreatedAt { get; init; }
    public string? LastModifiedBy { get; init; }
    public DateTime? LastModifiedAt { get; init; }
    public Dictionary<AuditAction, int> ActionCounts { get; init; } = new();
    public IReadOnlyList<AuditLogDto> RecentChanges { get; init; } = new List<AuditLogDto>();
}

/// <summary>
/// User activity summary.
/// </summary>
public record UserActivitySummaryDto
{
    public Guid UserId { get; init; }
    public string? UserName { get; init; }
    public string? UserEmail { get; init; }
    public int TotalActions { get; init; }
    public DateTime? FirstActivity { get; init; }
    public DateTime? LastActivity { get; init; }
    public Dictionary<AuditAction, int> ActionCounts { get; init; } = new();
    public Dictionary<string, int> EntityTypeCounts { get; init; } = new();
    public IReadOnlyList<AuditLogDto> RecentActivity { get; init; } = new List<AuditLogDto>();
}

/// <summary>
/// Compliance report for approvals.
/// </summary>
public record ApprovalComplianceReportDto
{
    public required string Period { get; init; }
    public DateTime GeneratedAt { get; init; }
    public int TotalApprovals { get; init; }
    public int TotalRejections { get; init; }
    public int TotalPending { get; init; }
    public decimal TotalApprovedAmount { get; init; }
    public decimal TotalRejectedAmount { get; init; }
    public double AverageApprovalTimeHours { get; init; }
    public int SlaBreaches { get; init; }
    public IReadOnlyList<ApproverActivityDto> ApproverActivities { get; init; } = new List<ApproverActivityDto>();
    public IReadOnlyList<ApprovalAuditDto> AuditTrail { get; init; } = new List<ApprovalAuditDto>();
}

/// <summary>
/// Approver activity details.
/// </summary>
public record ApproverActivityDto
{
    public Guid ApproverId { get; init; }
    public required string ApproverName { get; init; }
    public int ApprovalsCount { get; init; }
    public int RejectionsCount { get; init; }
    public double AverageResponseTimeHours { get; init; }
    public int SlaBreaches { get; init; }
    public decimal TotalApprovedAmount { get; init; }
}

/// <summary>
/// Approval audit trail entry.
/// </summary>
public record ApprovalAuditDto
{
    public Guid CalculationId { get; init; }
    public required string EmployeeName { get; init; }
    public required string Period { get; init; }
    public decimal Amount { get; init; }
    public required string Status { get; init; }
    public string? ApprovedBy { get; init; }
    public DateTime? ApprovedAt { get; init; }
    public double ResponseTimeHours { get; init; }
    public bool SlaBreach { get; init; }
    public string? Comments { get; init; }
}

/// <summary>
/// Data access report for compliance.
/// </summary>
public record DataAccessReportDto
{
    public required string Period { get; init; }
    public DateTime GeneratedAt { get; init; }
    public int TotalDataAccesses { get; init; }
    public int TotalExports { get; init; }
    public int TotalSensitiveAccesses { get; init; }
    public IReadOnlyList<DataAccessEntryDto> Entries { get; init; } = new List<DataAccessEntryDto>();
    public Dictionary<string, int> AccessByEntityType { get; init; } = new();
    public Dictionary<string, int> AccessByUser { get; init; } = new();
}

/// <summary>
/// Data access entry.
/// </summary>
public record DataAccessEntryDto
{
    public DateTime Timestamp { get; init; }
    public required string UserName { get; init; }
    public required string EntityType { get; init; }
    public Guid EntityId { get; init; }
    public required string Action { get; init; }
    public string? IpAddress { get; init; }
    public bool IsSensitive { get; init; }
}

/// <summary>
/// System change log for configuration changes.
/// </summary>
public record SystemChangeLogDto
{
    public DateTime Timestamp { get; init; }
    public required string ChangeType { get; init; }
    public required string Description { get; init; }
    public required string ChangedBy { get; init; }
    public string? OldValue { get; init; }
    public string? NewValue { get; init; }
    public string? Reason { get; init; }
}

/// <summary>
/// Audit statistics.
/// </summary>
public record AuditStatisticsDto
{
    public int TotalEntries { get; init; }
    public int TodayEntries { get; init; }
    public int ThisWeekEntries { get; init; }
    public int ThisMonthEntries { get; init; }
    public Dictionary<AuditAction, int> ByAction { get; init; } = new();
    public Dictionary<string, int> ByEntityType { get; init; } = new();
    public Dictionary<string, int> TopUsers { get; init; } = new();
    public DateTime? OldestEntry { get; init; }
    public DateTime? NewestEntry { get; init; }
}

/// <summary>
/// Retention policy configuration.
/// </summary>
public record RetentionPolicyDto
{
    public int DefaultRetentionDays { get; init; } = 365;
    public int SensitiveDataRetentionDays { get; init; } = 2555; // 7 years
    public int LoginAuditRetentionDays { get; init; } = 90;
    public int ExportAuditRetentionDays { get; init; } = 365;
    public bool AutoPurgeEnabled { get; init; }
    public DateTime? LastPurgeDate { get; init; }
    public int LastPurgeCount { get; init; }
}
