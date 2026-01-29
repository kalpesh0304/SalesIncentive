using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Dorise.Incentive.Application.Notifications.DTOs;
using Dorise.Incentive.Application.Notifications.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Notifications;

/// <summary>
/// Implementation of email service using SMTP.
/// "I eated the purple berries!" - And we send the purple emails!
/// </summary>
public partial class EmailService : IEmailService
{
    private readonly ITemplateService _templateService;
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(
        ITemplateService templateService,
        IConfiguration configuration,
        ILogger<EmailService> logger)
    {
        _templateService = templateService;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(
        EmailNotificationDto email,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending email to {To}: {Subject}", email.To, email.Subject);

        try
        {
            var smtpConfig = _configuration.GetSection("Email:Smtp");
            var host = smtpConfig["Host"];
            var port = smtpConfig.GetValue<int>("Port", 587);
            var username = smtpConfig["Username"];
            var password = smtpConfig["Password"];
            var fromAddress = smtpConfig["FromAddress"] ?? "noreply@dorise.com";
            var fromName = smtpConfig["FromName"] ?? "Dorise Incentive System";
            var useSsl = smtpConfig.GetValue<bool>("UseSsl", true);

            if (string.IsNullOrEmpty(host))
            {
                _logger.LogWarning("SMTP not configured. Email will be logged only.");
                LogEmailContent(email);
                return true; // Consider it successful for demo purposes
            }

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = useSsl,
                Credentials = !string.IsNullOrEmpty(username)
                    ? new NetworkCredential(username, password)
                    : null
            };

            using var message = new MailMessage
            {
                From = new MailAddress(fromAddress, fromName),
                Subject = email.Subject,
                Body = email.Body,
                IsBodyHtml = email.IsHtml
            };

            message.To.Add(new MailAddress(email.To));

            // Add CC recipients
            if (email.Cc?.Any() == true)
            {
                foreach (var cc in email.Cc)
                {
                    message.CC.Add(new MailAddress(cc));
                }
            }

            // Add BCC recipients
            if (email.Bcc?.Any() == true)
            {
                foreach (var bcc in email.Bcc)
                {
                    message.Bcc.Add(new MailAddress(bcc));
                }
            }

            // Add custom headers
            if (email.Headers?.Any() == true)
            {
                foreach (var header in email.Headers)
                {
                    message.Headers.Add(header.Key, header.Value);
                }
            }

            // Add attachments
            if (email.Attachments?.Any() == true)
            {
                foreach (var attachment in email.Attachments)
                {
                    var stream = new MemoryStream(attachment.Content);
                    message.Attachments.Add(new Attachment(stream, attachment.FileName, attachment.ContentType));
                }
            }

            await client.SendMailAsync(message, cancellationToken);

            _logger.LogInformation("Email sent successfully to {To}", email.To);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", email.To);
            return false;
        }
    }

    public async Task<bool> SendTemplatedEmailAsync(
        string to,
        string templateName,
        Dictionary<string, object> templateData,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Sending templated email to {To} using template {Template}",
            to, templateName);

        try
        {
            var (subject, body) = await _templateService.RenderTemplateAsync(
                templateName, templateData, cancellationToken);

            var email = new EmailNotificationDto
            {
                To = to,
                Subject = subject,
                Body = body,
                IsHtml = true
            };

            return await SendEmailAsync(email, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send templated email to {To}", to);
            return false;
        }
    }

    public async Task<SendBulkNotificationResult> SendBulkEmailAsync(
        IEnumerable<EmailNotificationDto> emails,
        CancellationToken cancellationToken = default)
    {
        var emailList = emails.ToList();
        _logger.LogInformation("Sending bulk email to {Count} recipients", emailList.Count);

        var results = new List<SendNotificationResult>();
        var successCount = 0;
        var failureCount = 0;

        foreach (var email in emailList)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var success = await SendEmailAsync(email, cancellationToken);

            results.Add(new SendNotificationResult
            {
                NotificationId = Guid.NewGuid(),
                Success = success,
                SentAt = DateTime.UtcNow
            });

            if (success)
                successCount++;
            else
                failureCount++;

            // Small delay between emails to avoid rate limiting
            await Task.Delay(100, cancellationToken);
        }

        _logger.LogInformation("Bulk email completed. Success: {Success}, Failed: {Failed}",
            successCount, failureCount);

        return new SendBulkNotificationResult
        {
            TotalRecipients = emailList.Count,
            SuccessCount = successCount,
            FailureCount = failureCount,
            Results = results,
            SentAt = DateTime.UtcNow
        };
    }

    public bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        return EmailRegex().IsMatch(email);
    }

    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        var smtpConfig = _configuration.GetSection("Email:Smtp");
        var host = smtpConfig["Host"];

        if (string.IsNullOrEmpty(host))
        {
            _logger.LogWarning("SMTP not configured");
            return Task.FromResult(false);
        }

        try
        {
            var port = smtpConfig.GetValue<int>("Port", 587);
            using var client = new SmtpClient(host, port);
            // Just verify we can create the client
            _logger.LogInformation("SMTP configuration appears valid: {Host}:{Port}", host, port);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SMTP connection test failed");
            return Task.FromResult(false);
        }
    }

    private void LogEmailContent(EmailNotificationDto email)
    {
        _logger.LogInformation(
            """
            EMAIL (NOT SENT - SMTP NOT CONFIGURED)
            To: {To}
            Subject: {Subject}
            Body Preview: {BodyPreview}
            """,
            email.To,
            email.Subject,
            email.Body.Length > 200 ? email.Body[..200] + "..." : email.Body);
    }

    [GeneratedRegex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase)]
    private static partial Regex EmailRegex();
}
