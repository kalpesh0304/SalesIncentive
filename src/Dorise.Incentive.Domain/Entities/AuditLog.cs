using Dorise.Incentive.Domain.Common;

namespace Dorise.Incentive.Domain.Entities;

/// <summary>
/// Represents an audit log entry for tracking system changes.
/// "I found a moonrock in my nose!" - And we found every change in the audit log!
/// </summary>
public class AuditLog : BaseEntity
{
    public string EntityType { get; private set; } = null!;
    public Guid EntityId { get; private set; }
    public AuditAction Action { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string? ChangedProperties { get; private set; }
    public Guid? UserId { get; private set; }
    public string? UserName { get; private set; }
    public string? UserEmail { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? Reason { get; private set; }
    public string? CorrelationId { get; private set; }
    public DateTime Timestamp { get; private set; }
    public string? AdditionalData { get; private set; }

    private AuditLog() { } // EF Core constructor

    public static AuditLog Create(
        string entityType,
        Guid entityId,
        AuditAction action,
        Guid? userId = null,
        string? userName = null,
        string? userEmail = null,
        string? oldValues = null,
        string? newValues = null,
        string? changedProperties = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? reason = null,
        string? correlationId = null,
        string? additionalData = null)
    {
        return new AuditLog
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            Action = action,
            UserId = userId,
            UserName = userName,
            UserEmail = userEmail,
            OldValues = oldValues,
            NewValues = newValues,
            ChangedProperties = changedProperties,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            Reason = reason,
            CorrelationId = correlationId,
            Timestamp = DateTime.UtcNow,
            AdditionalData = additionalData
        };
    }
}

/// <summary>
/// Types of audit actions.
/// </summary>
public enum AuditAction
{
    Create,
    Update,
    Delete,
    View,
    Export,
    Import,
    Approve,
    Reject,
    Submit,
    Login,
    Logout,
    PasswordChange,
    PermissionChange,
    ConfigurationChange,
    BulkOperation,
    SystemEvent
}
