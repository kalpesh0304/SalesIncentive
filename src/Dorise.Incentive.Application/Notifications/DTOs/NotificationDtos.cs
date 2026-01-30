namespace Dorise.Incentive.Application.Notifications.DTOs;

/// <summary>
/// Represents a notification in the system.
/// "I bent my Wookie." - But we won't bend your notifications!
/// </summary>
public record NotificationDto
{
    public Guid Id { get; init; }
    public Guid RecipientId { get; init; }
    public string RecipientName { get; init; } = string.Empty;
    public string RecipientEmail { get; init; } = string.Empty;
    public required string Title { get; init; }
    public required string Message { get; init; }
    public NotificationType Type { get; init; }
    public NotificationPriority Priority { get; init; }
    public NotificationChannel Channel { get; init; }
    public NotificationStatus Status { get; init; }
    public string? Category { get; init; }
    public string? ActionUrl { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public string? RelatedEntityType { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ReadAt { get; init; }
    public DateTime? SentAt { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Types of notifications.
/// </summary>
public enum NotificationType
{
    Information,
    Success,
    Warning,
    Error,
    Action
}

/// <summary>
/// Priority levels for notifications.
/// </summary>
public enum NotificationPriority
{
    Low,
    Normal,
    High,
    Urgent
}

/// <summary>
/// Notification delivery channels.
/// </summary>
public enum NotificationChannel
{
    InApp,
    Email,
    Both
}

/// <summary>
/// Status of a notification.
/// </summary>
public enum NotificationStatus
{
    Pending,
    Sent,
    Delivered,
    Read,
    Failed,
    Expired
}

/// <summary>
/// Request to send a notification.
/// </summary>
public record SendNotificationRequest
{
    public Guid RecipientId { get; init; }
    public required string Title { get; init; }
    public required string Message { get; init; }
    public NotificationType Type { get; init; } = NotificationType.Information;
    public NotificationPriority Priority { get; init; } = NotificationPriority.Normal;
    public NotificationChannel Channel { get; init; } = NotificationChannel.InApp;
    public string? Category { get; init; }
    public string? ActionUrl { get; init; }
    public Guid? RelatedEntityId { get; init; }
    public string? RelatedEntityType { get; init; }
    public string? TemplateName { get; init; }
    public Dictionary<string, object>? TemplateData { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Request to send bulk notifications.
/// </summary>
public record SendBulkNotificationRequest
{
    public List<Guid> RecipientIds { get; init; } = new();
    public required string Title { get; init; }
    public required string Message { get; init; }
    public NotificationType Type { get; init; } = NotificationType.Information;
    public NotificationPriority Priority { get; init; } = NotificationPriority.Normal;
    public NotificationChannel Channel { get; init; } = NotificationChannel.InApp;
    public string? Category { get; init; }
    public string? ActionUrl { get; init; }
    public string? TemplateName { get; init; }
    public Dictionary<string, object>? TemplateData { get; init; }
}

/// <summary>
/// Result of sending a notification.
/// </summary>
public record SendNotificationResult
{
    public Guid NotificationId { get; init; }
    public bool Success { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime SentAt { get; init; }
}

/// <summary>
/// Result of sending bulk notifications.
/// </summary>
public record SendBulkNotificationResult
{
    public int TotalRecipients { get; init; }
    public int SuccessCount { get; init; }
    public int FailureCount { get; init; }
    public List<SendNotificationResult> Results { get; init; } = new();
    public DateTime SentAt { get; init; }
}

/// <summary>
/// Notification preferences for a user.
/// </summary>
public record NotificationPreferencesDto
{
    public Guid UserId { get; init; }
    public bool EmailNotificationsEnabled { get; init; } = true;
    public bool InAppNotificationsEnabled { get; init; } = true;
    public bool ApprovalAlerts { get; init; } = true;
    public bool CalculationAlerts { get; init; } = true;
    public bool PaymentAlerts { get; init; } = true;
    public bool SystemAlerts { get; init; } = true;
    public bool DailyDigest { get; init; }
    public bool WeeklyDigest { get; init; }
    public string? PreferredEmailTime { get; init; }
    public List<string> MutedCategories { get; init; } = new();
}

/// <summary>
/// Email notification details.
/// </summary>
public record EmailNotificationDto
{
    public required string To { get; init; }
    public List<string>? Cc { get; init; }
    public List<string>? Bcc { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public bool IsHtml { get; init; } = true;
    public List<EmailAttachmentDto>? Attachments { get; init; }
    public Dictionary<string, string>? Headers { get; init; }
}

/// <summary>
/// Email attachment.
/// </summary>
public record EmailAttachmentDto
{
    public required string FileName { get; init; }
    public required byte[] Content { get; init; }
    public required string ContentType { get; init; }
}

/// <summary>
/// Notification template.
/// </summary>
public record NotificationTemplateDto
{
    public required string Name { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public bool IsHtml { get; init; } = true;
    public NotificationChannel DefaultChannel { get; init; }
    public NotificationType DefaultType { get; init; }
    public NotificationPriority DefaultPriority { get; init; }
    public string? Category { get; init; }
    public List<string> RequiredPlaceholders { get; init; } = new();
}

/// <summary>
/// Notification summary/statistics.
/// </summary>
public record NotificationSummaryDto
{
    public int TotalNotifications { get; init; }
    public int UnreadCount { get; init; }
    public int TodayCount { get; init; }
    public int PendingCount { get; init; }
    public Dictionary<string, int> ByCategory { get; init; } = new();
    public Dictionary<NotificationType, int> ByType { get; init; } = new();
    public DateTime? LastNotificationAt { get; init; }
}

/// <summary>
/// Alert configuration for system events.
/// </summary>
public record AlertConfigurationDto
{
    public required string EventType { get; init; }
    public bool IsEnabled { get; init; } = true;
    public NotificationChannel Channel { get; init; }
    public NotificationPriority Priority { get; init; }
    public string? TemplateName { get; init; }
    public List<Guid>? RecipientIds { get; init; }
    public List<string>? RecipientRoles { get; init; }
    public int? ThresholdValue { get; init; }
    public string? ThresholdCondition { get; init; }
}
