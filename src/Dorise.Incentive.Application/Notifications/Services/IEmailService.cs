using Dorise.Incentive.Application.Notifications.DTOs;

namespace Dorise.Incentive.Application.Notifications.Services;

/// <summary>
/// Email service interface for sending email notifications.
/// "Me fail English? That's unpossible!" - Email delivery is possible!
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends an email.
    /// </summary>
    Task<bool> SendEmailAsync(
        EmailNotificationDto email,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends an email using a template.
    /// </summary>
    Task<bool> SendTemplatedEmailAsync(
        string to,
        string templateName,
        Dictionary<string, object> templateData,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends bulk emails.
    /// </summary>
    Task<SendBulkNotificationResult> SendBulkEmailAsync(
        IEnumerable<EmailNotificationDto> emails,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an email address.
    /// </summary>
    bool IsValidEmail(string email);

    /// <summary>
    /// Tests email configuration.
    /// </summary>
    Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default);
}
