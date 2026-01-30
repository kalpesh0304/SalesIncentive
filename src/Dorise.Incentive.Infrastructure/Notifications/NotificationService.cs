using Dorise.Incentive.Application.Notifications.DTOs;
using Dorise.Incentive.Application.Notifications.Services;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Notifications;

/// <summary>
/// Main notification service implementation.
/// "When I grow up, I want to be a principal or a caterpillar." - When I grow up, I want to send notifications!
/// </summary>
public class NotificationService : INotificationService
{
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<NotificationService> _logger;

    // In-memory storage for demo (use database in production)
    private readonly List<NotificationDto> _notifications = new();
    private readonly Dictionary<Guid, NotificationPreferencesDto> _preferences = new();

    public NotificationService(
        IEmailService emailService,
        ITemplateService templateService,
        IEmployeeRepository employeeRepository,
        ILogger<NotificationService> logger)
    {
        _emailService = emailService;
        _templateService = templateService;
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    public async Task<SendNotificationResult> SendAsync(
        SendNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending notification to {RecipientId}: {Title}",
            request.RecipientId, request.Title);

        try
        {
            // Get recipient info
            var employee = await _employeeRepository.GetByIdAsync(request.RecipientId, cancellationToken);
            if (employee == null)
            {
                _logger.LogWarning("Recipient not found: {RecipientId}", request.RecipientId);
                return new SendNotificationResult
                {
                    NotificationId = Guid.Empty,
                    Success = false,
                    ErrorMessage = "Recipient not found",
                    SentAt = DateTime.UtcNow
                };
            }

            // Check preferences
            var preferences = await GetPreferencesAsync(request.RecipientId, cancellationToken);

            // Create notification record
            var notification = new NotificationDto
            {
                Id = Guid.NewGuid(),
                RecipientId = request.RecipientId,
                RecipientName = employee.FullName,
                RecipientEmail = employee.Email,
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                Priority = request.Priority,
                Channel = request.Channel,
                Status = NotificationStatus.Pending,
                Category = request.Category,
                ActionUrl = request.ActionUrl,
                RelatedEntityId = request.RelatedEntityId,
                RelatedEntityType = request.RelatedEntityType,
                CreatedAt = DateTime.UtcNow,
                Metadata = request.Metadata
            };

            // Store in-app notification
            if (request.Channel is NotificationChannel.InApp or NotificationChannel.Both)
            {
                if (preferences.InAppNotificationsEnabled)
                {
                    _notifications.Add(notification);
                    _logger.LogDebug("In-app notification stored: {NotificationId}", notification.Id);
                }
            }

            // Send email notification
            if (request.Channel is NotificationChannel.Email or NotificationChannel.Both)
            {
                if (preferences.EmailNotificationsEnabled)
                {
                    var emailSent = await SendEmailNotificationAsync(
                        employee.Email,
                        request,
                        cancellationToken);

                    if (!emailSent)
                    {
                        _logger.LogWarning("Email notification failed for {RecipientId}", request.RecipientId);
                    }
                }
            }

            // Update status
            var index = _notifications.FindIndex(n => n.Id == notification.Id);
            if (index >= 0)
            {
                _notifications[index] = notification with
                {
                    Status = NotificationStatus.Sent,
                    SentAt = DateTime.UtcNow
                };
            }

            _logger.LogInformation("Notification sent successfully: {NotificationId}", notification.Id);

            return new SendNotificationResult
            {
                NotificationId = notification.Id,
                Success = true,
                SentAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification to {RecipientId}", request.RecipientId);
            return new SendNotificationResult
            {
                NotificationId = Guid.Empty,
                Success = false,
                ErrorMessage = ex.Message,
                SentAt = DateTime.UtcNow
            };
        }
    }

    public async Task<SendBulkNotificationResult> SendBulkAsync(
        SendBulkNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Sending bulk notification to {Count} recipients: {Title}",
            request.RecipientIds.Count, request.Title);

        var results = new List<SendNotificationResult>();
        var successCount = 0;
        var failureCount = 0;

        foreach (var recipientId in request.RecipientIds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var individualRequest = new SendNotificationRequest
            {
                RecipientId = recipientId,
                Title = request.Title,
                Message = request.Message,
                Type = request.Type,
                Priority = request.Priority,
                Channel = request.Channel,
                Category = request.Category,
                ActionUrl = request.ActionUrl,
                TemplateName = request.TemplateName,
                TemplateData = request.TemplateData
            };

            var result = await SendAsync(individualRequest, cancellationToken);
            results.Add(result);

            if (result.Success)
                successCount++;
            else
                failureCount++;
        }

        return new SendBulkNotificationResult
        {
            TotalRecipients = request.RecipientIds.Count,
            SuccessCount = successCount,
            FailureCount = failureCount,
            Results = results,
            SentAt = DateTime.UtcNow
        };
    }

    public async Task<SendNotificationResult> SendFromTemplateAsync(
        Guid recipientId,
        string templateName,
        Dictionary<string, object> templateData,
        CancellationToken cancellationToken = default)
    {
        var template = await _templateService.GetTemplateAsync(templateName, cancellationToken);
        if (template == null)
        {
            return new SendNotificationResult
            {
                NotificationId = Guid.Empty,
                Success = false,
                ErrorMessage = $"Template '{templateName}' not found",
                SentAt = DateTime.UtcNow
            };
        }

        var (subject, body) = await _templateService.RenderTemplateAsync(
            templateName, templateData, cancellationToken);

        var request = new SendNotificationRequest
        {
            RecipientId = recipientId,
            Title = subject,
            Message = body,
            Type = template.DefaultType,
            Priority = template.DefaultPriority,
            Channel = template.DefaultChannel,
            Category = template.Category,
            TemplateName = templateName,
            TemplateData = templateData
        };

        return await SendAsync(request, cancellationToken);
    }

    public Task<IReadOnlyList<NotificationDto>> GetForUserAsync(
        Guid userId,
        bool unreadOnly = false,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _notifications
            .Where(n => n.RecipientId == userId)
            .OrderByDescending(n => n.CreatedAt);

        if (unreadOnly)
        {
            query = query.Where(n => n.ReadAt == null).OrderByDescending(n => n.CreatedAt);
        }

        var result = query.Take(limit).ToList();
        return Task.FromResult<IReadOnlyList<NotificationDto>>(result);
    }

    public Task<NotificationDto?> GetByIdAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        return Task.FromResult(notification);
    }

    public Task MarkAsReadAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var index = _notifications.FindIndex(n => n.Id == notificationId);
        if (index >= 0)
        {
            _notifications[index] = _notifications[index] with
            {
                Status = NotificationStatus.Read,
                ReadAt = DateTime.UtcNow
            };
            _logger.LogDebug("Notification marked as read: {NotificationId}", notificationId);
        }

        return Task.CompletedTask;
    }

    public Task MarkAllAsReadAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        for (var i = 0; i < _notifications.Count; i++)
        {
            if (_notifications[i].RecipientId == userId && _notifications[i].ReadAt == null)
            {
                _notifications[i] = _notifications[i] with
                {
                    Status = NotificationStatus.Read,
                    ReadAt = DateTime.UtcNow
                };
            }
        }

        _logger.LogDebug("All notifications marked as read for user: {UserId}", userId);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        var removed = _notifications.RemoveAll(n => n.Id == notificationId);
        if (removed > 0)
        {
            _logger.LogDebug("Notification deleted: {NotificationId}", notificationId);
        }

        return Task.CompletedTask;
    }

    public Task DeleteOldNotificationsAsync(
        int daysOld = 30,
        CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-daysOld);
        var removed = _notifications.RemoveAll(n => n.CreatedAt < cutoff);
        _logger.LogInformation("Deleted {Count} old notifications", removed);
        return Task.CompletedTask;
    }

    public Task<NotificationSummaryDto> GetSummaryAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userNotifications = _notifications.Where(n => n.RecipientId == userId).ToList();
        var today = DateTime.UtcNow.Date;

        var summary = new NotificationSummaryDto
        {
            TotalNotifications = userNotifications.Count,
            UnreadCount = userNotifications.Count(n => n.ReadAt == null),
            TodayCount = userNotifications.Count(n => n.CreatedAt.Date == today),
            PendingCount = userNotifications.Count(n => n.Status == NotificationStatus.Pending),
            ByCategory = userNotifications
                .Where(n => n.Category != null)
                .GroupBy(n => n.Category!)
                .ToDictionary(g => g.Key, g => g.Count()),
            ByType = userNotifications
                .GroupBy(n => n.Type)
                .ToDictionary(g => g.Key, g => g.Count()),
            LastNotificationAt = userNotifications
                .OrderByDescending(n => n.CreatedAt)
                .FirstOrDefault()?.CreatedAt
        };

        return Task.FromResult(summary);
    }

    public Task<NotificationPreferencesDto> GetPreferencesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (_preferences.TryGetValue(userId, out var prefs))
        {
            return Task.FromResult(prefs);
        }

        // Return default preferences
        var defaultPrefs = new NotificationPreferencesDto
        {
            UserId = userId,
            EmailNotificationsEnabled = true,
            InAppNotificationsEnabled = true,
            ApprovalAlerts = true,
            CalculationAlerts = true,
            PaymentAlerts = true,
            SystemAlerts = true
        };

        return Task.FromResult(defaultPrefs);
    }

    public Task UpdatePreferencesAsync(
        NotificationPreferencesDto preferences,
        CancellationToken cancellationToken = default)
    {
        _preferences[preferences.UserId] = preferences;
        _logger.LogInformation("Updated notification preferences for user: {UserId}", preferences.UserId);
        return Task.CompletedTask;
    }

    private async Task<bool> SendEmailNotificationAsync(
        string email,
        SendNotificationRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(request.TemplateName) && request.TemplateData != null)
        {
            return await _emailService.SendTemplatedEmailAsync(
                email, request.TemplateName, request.TemplateData, cancellationToken);
        }

        var emailDto = new EmailNotificationDto
        {
            To = email,
            Subject = request.Title,
            Body = request.Message,
            IsHtml = true
        };

        return await _emailService.SendEmailAsync(emailDto, cancellationToken);
    }
}
