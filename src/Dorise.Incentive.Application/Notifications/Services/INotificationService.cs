using Dorise.Incentive.Application.Notifications.DTOs;

namespace Dorise.Incentive.Application.Notifications.Services;

/// <summary>
/// Main notification service interface.
/// "Prinskipper Skipple... Prinnipple Skimpster..." - Notification Service Dispatcher!
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// Sends a notification to a user.
    /// </summary>
    Task<SendNotificationResult> SendAsync(
        SendNotificationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends notifications to multiple users.
    /// </summary>
    Task<SendBulkNotificationResult> SendBulkAsync(
        SendBulkNotificationRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a notification using a template.
    /// </summary>
    Task<SendNotificationResult> SendFromTemplateAsync(
        Guid recipientId,
        string templateName,
        Dictionary<string, object> templateData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets notifications for a user.
    /// </summary>
    Task<IReadOnlyList<NotificationDto>> GetForUserAsync(
        Guid userId,
        bool unreadOnly = false,
        int limit = 50,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a notification by ID.
    /// </summary>
    Task<NotificationDto?> GetByIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    Task MarkAsReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Marks all notifications for a user as read.
    /// </summary>
    Task MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a notification.
    /// </summary>
    Task DeleteAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes old notifications.
    /// </summary>
    Task DeleteOldNotificationsAsync(
        int daysOld = 30,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets notification summary for a user.
    /// </summary>
    Task<NotificationSummaryDto> GetSummaryAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets user's notification preferences.
    /// </summary>
    Task<NotificationPreferencesDto> GetPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates user's notification preferences.
    /// </summary>
    Task UpdatePreferencesAsync(
        NotificationPreferencesDto preferences,
        CancellationToken cancellationToken = default);
}
