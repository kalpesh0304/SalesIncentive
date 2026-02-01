using Dorise.Incentive.Application.Notifications.DTOs;
using Dorise.Incentive.Application.Notifications.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for notification management.
/// "Hi, Lisa! Hi, Super Nintendo Chalmers!" - Hi, Notifications!
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly INotificationService _notificationService;
    private readonly IEmailService _emailService;
    private readonly ITemplateService _templateService;
    private readonly ILogger<NotificationsController> _logger;

    public NotificationsController(
        INotificationService notificationService,
        IEmailService emailService,
        ITemplateService templateService,
        ILogger<NotificationsController> logger)
    {
        _notificationService = notificationService;
        _emailService = emailService;
        _templateService = templateService;
        _logger = logger;
    }

    private Guid CurrentUserId =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out var id)
            ? id
            : Guid.Empty;

    #region User Notifications

    /// <summary>
    /// Gets notifications for the current user.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationService.GetForUserAsync(
            CurrentUserId, unreadOnly, limit, cancellationToken);

        return Ok(notifications);
    }

    /// <summary>
    /// Gets a specific notification.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(NotificationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNotification(
        Guid id,
        CancellationToken cancellationToken)
    {
        var notification = await _notificationService.GetByIdAsync(id, cancellationToken);

        if (notification == null)
            return NotFound();

        // Ensure user can only access their own notifications
        if (notification.RecipientId != CurrentUserId && !User.IsInRole("Admin"))
            return Forbid();

        return Ok(notification);
    }

    /// <summary>
    /// Gets notification summary for current user.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(NotificationSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _notificationService.GetSummaryAsync(CurrentUserId, cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Marks a notification as read.
    /// </summary>
    [HttpPost("{id:guid}/read")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(
        Guid id,
        CancellationToken cancellationToken)
    {
        var notification = await _notificationService.GetByIdAsync(id, cancellationToken);

        if (notification == null)
            return NotFound();

        if (notification.RecipientId != CurrentUserId)
            return Forbid();

        await _notificationService.MarkAsReadAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Marks all notifications as read for current user.
    /// </summary>
    [HttpPost("read-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> MarkAllAsRead(CancellationToken cancellationToken)
    {
        await _notificationService.MarkAllAsReadAsync(CurrentUserId, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Deletes a notification.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var notification = await _notificationService.GetByIdAsync(id, cancellationToken);

        if (notification == null)
            return NotFound();

        if (notification.RecipientId != CurrentUserId && !User.IsInRole("Admin"))
            return Forbid();

        await _notificationService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    #endregion

    #region Preferences

    /// <summary>
    /// Gets notification preferences for current user.
    /// </summary>
    [HttpGet("preferences")]
    [ProducesResponseType(typeof(NotificationPreferencesDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPreferences(CancellationToken cancellationToken)
    {
        var preferences = await _notificationService.GetPreferencesAsync(
            CurrentUserId, cancellationToken);

        return Ok(preferences);
    }

    /// <summary>
    /// Updates notification preferences for current user.
    /// </summary>
    [HttpPut("preferences")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> UpdatePreferences(
        [FromBody] NotificationPreferencesDto preferences,
        CancellationToken cancellationToken)
    {
        // Ensure user can only update their own preferences
        var updatedPreferences = preferences with { UserId = CurrentUserId };

        await _notificationService.UpdatePreferencesAsync(updatedPreferences, cancellationToken);
        return NoContent();
    }

    #endregion

    #region Admin Operations

    /// <summary>
    /// Sends a notification to a user (Admin only).
    /// </summary>
    [HttpPost("send")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SendNotificationResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendNotification(
        [FromBody] SendNotificationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Admin {AdminId} sending notification to {RecipientId}",
            CurrentUserId, request.RecipientId);

        var result = await _notificationService.SendAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Sends bulk notifications (Admin only).
    /// </summary>
    [HttpPost("send-bulk")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SendBulkNotificationResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendBulkNotification(
        [FromBody] SendBulkNotificationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Admin {AdminId} sending bulk notification to {Count} recipients",
            CurrentUserId, request.RecipientIds.Count);

        var result = await _notificationService.SendBulkAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Sends a templated notification (Admin only).
    /// </summary>
    [HttpPost("send-template")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SendNotificationResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendTemplatedNotification(
        [FromBody] SendTemplatedNotificationRequest request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Admin {AdminId} sending templated notification to {RecipientId}",
            CurrentUserId, request.RecipientId);

        var result = await _notificationService.SendFromTemplateAsync(
            request.RecipientId,
            request.TemplateName,
            request.TemplateData,
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Deletes old notifications (Admin only).
    /// </summary>
    [HttpDelete("cleanup")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> CleanupOldNotifications(
        [FromQuery] int daysOld = 30,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Admin {AdminId} cleaning up notifications older than {Days} days",
            CurrentUserId, daysOld);

        await _notificationService.DeleteOldNotificationsAsync(daysOld, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Gets notifications for a specific user (Admin only).
    /// </summary>
    [HttpGet("user/{userId:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserNotifications(
        Guid userId,
        [FromQuery] bool unreadOnly = false,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var notifications = await _notificationService.GetForUserAsync(
            userId, unreadOnly, limit, cancellationToken);

        return Ok(notifications);
    }

    #endregion

    #region Templates

    /// <summary>
    /// Gets all available templates.
    /// </summary>
    [HttpGet("templates")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(IReadOnlyList<NotificationTemplateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTemplates(CancellationToken cancellationToken)
    {
        var templates = await _templateService.GetAllTemplatesAsync(cancellationToken);
        return Ok(templates);
    }

    /// <summary>
    /// Gets a specific template.
    /// </summary>
    [HttpGet("templates/{name}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(NotificationTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTemplate(
        string name,
        CancellationToken cancellationToken)
    {
        var template = await _templateService.GetTemplateAsync(name, cancellationToken);

        if (template == null)
            return NotFound();

        return Ok(template);
    }

    /// <summary>
    /// Registers a new template.
    /// </summary>
    [HttpPost("templates")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<IActionResult> RegisterTemplate(
        [FromBody] NotificationTemplateDto template,
        CancellationToken cancellationToken)
    {
        await _templateService.RegisterTemplateAsync(template, cancellationToken);

        return CreatedAtAction(
            nameof(GetTemplate),
            new { name = template.Name },
            template);
    }

    /// <summary>
    /// Previews a rendered template.
    /// </summary>
    [HttpPost("templates/{name}/preview")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PreviewTemplate(
        string name,
        [FromBody] Dictionary<string, object> data,
        CancellationToken cancellationToken)
    {
        try
        {
            var (subject, body) = await _templateService.RenderTemplateAsync(
                name, data, cancellationToken);

            return Ok(new { Subject = subject, Body = body });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    #endregion

    #region Email Testing

    /// <summary>
    /// Tests email configuration (Admin only).
    /// </summary>
    [HttpPost("email/test")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> TestEmailConnection(CancellationToken cancellationToken)
    {
        var result = await _emailService.TestConnectionAsync(cancellationToken);
        return Ok(new { Connected = result, TestedAt = DateTime.UtcNow });
    }

    /// <summary>
    /// Sends a test email (Admin only).
    /// </summary>
    [HttpPost("email/send-test")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    public async Task<IActionResult> SendTestEmail(
        [FromBody] SendTestEmailRequest request,
        CancellationToken cancellationToken)
    {
        var email = new EmailNotificationDto
        {
            To = request.To,
            Subject = "Test Email from Dorise Incentive System",
            Body = $"""
                <html>
                <body style="font-family: Arial, sans-serif; padding: 20px;">
                    <h2>Test Email</h2>
                    <p>This is a test email from the Dorise Incentive System.</p>
                    <p>If you received this email, your email configuration is working correctly.</p>
                    <p>Sent at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>
                </body>
                </html>
                """,
            IsHtml = true
        };

        var result = await _emailService.SendEmailAsync(email, cancellationToken);
        return Ok(new { Success = result, SentAt = DateTime.UtcNow });
    }

    #endregion
}

/// <summary>
/// Request to send a templated notification.
/// </summary>
public record SendTemplatedNotificationRequest
{
    public Guid RecipientId { get; init; }
    public required string TemplateName { get; init; }
    public Dictionary<string, object> TemplateData { get; init; } = new();
}

/// <summary>
/// Request to send a test email.
/// </summary>
public record SendTestEmailRequest
{
    public required string To { get; init; }
}
