using System.Text.RegularExpressions;
using Dorise.Incentive.Application.Notifications.DTOs;
using Dorise.Incentive.Application.Notifications.Services;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Notifications;

/// <summary>
/// Implementation of template service for notification templates.
/// "That's where I saw the leprechaun. He tells me to burn things." - Templates tell us to send things!
/// </summary>
public partial class TemplateService : ITemplateService
{
    private readonly ILogger<TemplateService> _logger;
    private readonly Dictionary<string, NotificationTemplateDto> _templates;

    public TemplateService(ILogger<TemplateService> logger)
    {
        _logger = logger;
        _templates = InitializeDefaultTemplates();
    }

    public Task<NotificationTemplateDto?> GetTemplateAsync(
        string templateName,
        CancellationToken cancellationToken = default)
    {
        _templates.TryGetValue(templateName, out var template);
        return Task.FromResult(template);
    }

    public Task<IReadOnlyList<NotificationTemplateDto>> GetAllTemplatesAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<NotificationTemplateDto>>(_templates.Values.ToList());
    }

    public async Task<(string Subject, string Body)> RenderTemplateAsync(
        string templateName,
        Dictionary<string, object> data,
        CancellationToken cancellationToken = default)
    {
        var template = await GetTemplateAsync(templateName, cancellationToken);

        if (template == null)
        {
            _logger.LogWarning("Template not found: {TemplateName}", templateName);
            throw new InvalidOperationException($"Template '{templateName}' not found");
        }

        var subject = RenderContent(template.Subject, data);
        var body = RenderContent(template.Body, data);

        return (subject, body);
    }

    public async Task<bool> ValidateTemplateDataAsync(
        string templateName,
        Dictionary<string, object> data,
        CancellationToken cancellationToken = default)
    {
        var template = await GetTemplateAsync(templateName, cancellationToken);

        if (template == null)
            return false;

        foreach (var placeholder in template.RequiredPlaceholders)
        {
            if (!data.ContainsKey(placeholder))
            {
                _logger.LogWarning(
                    "Missing required placeholder {Placeholder} for template {Template}",
                    placeholder, templateName);
                return false;
            }
        }

        return true;
    }

    public Task RegisterTemplateAsync(
        NotificationTemplateDto template,
        CancellationToken cancellationToken = default)
    {
        _templates[template.Name] = template;
        _logger.LogInformation("Registered template: {TemplateName}", template.Name);
        return Task.CompletedTask;
    }

    private static string RenderContent(string template, Dictionary<string, object> data)
    {
        var result = template;

        foreach (var (key, value) in data)
        {
            var placeholder = $"{{{{{key}}}}}";
            result = result.Replace(placeholder, value?.ToString() ?? string.Empty);
        }

        return result;
    }

    private static Dictionary<string, NotificationTemplateDto> InitializeDefaultTemplates()
    {
        return new Dictionary<string, NotificationTemplateDto>
        {
            ["calculation-completed"] = new NotificationTemplateDto
            {
                Name = "calculation-completed",
                Subject = "Incentive Calculation Completed - {{Period}}",
                Body = """
                    <html>
                    <body style="font-family: Arial, sans-serif; padding: 20px;">
                        <h2 style="color: #2c3e50;">Incentive Calculation Completed</h2>
                        <p>Dear {{EmployeeName}},</p>
                        <p>Your incentive calculation for <strong>{{Period}}</strong> has been completed.</p>
                        <table style="border-collapse: collapse; margin: 20px 0;">
                            <tr>
                                <td style="padding: 8px; border: 1px solid #ddd;"><strong>Plan:</strong></td>
                                <td style="padding: 8px; border: 1px solid #ddd;">{{PlanName}}</td>
                            </tr>
                            <tr>
                                <td style="padding: 8px; border: 1px solid #ddd;"><strong>Achievement:</strong></td>
                                <td style="padding: 8px; border: 1px solid #ddd;">{{Achievement}}%</td>
                            </tr>
                            <tr>
                                <td style="padding: 8px; border: 1px solid #ddd;"><strong>Incentive Amount:</strong></td>
                                <td style="padding: 8px; border: 1px solid #ddd;">{{Currency}} {{Amount}}</td>
                            </tr>
                        </table>
                        <p>The calculation is now pending approval.</p>
                        <p>Best regards,<br/>Dorise Incentive System</p>
                    </body>
                    </html>
                    """,
                IsHtml = true,
                DefaultChannel = NotificationChannel.Both,
                DefaultType = NotificationType.Success,
                DefaultPriority = NotificationPriority.Normal,
                Category = "Calculation",
                RequiredPlaceholders = new List<string>
                {
                    "EmployeeName", "Period", "PlanName", "Achievement", "Currency", "Amount"
                }
            },

            ["approval-pending"] = new NotificationTemplateDto
            {
                Name = "approval-pending",
                Subject = "Approval Required - {{EmployeeName}} ({{Period}})",
                Body = """
                    <html>
                    <body style="font-family: Arial, sans-serif; padding: 20px;">
                        <h2 style="color: #e74c3c;">Approval Required</h2>
                        <p>Dear {{ApproverName}},</p>
                        <p>An incentive calculation requires your approval:</p>
                        <table style="border-collapse: collapse; margin: 20px 0;">
                            <tr>
                                <td style="padding: 8px; border: 1px solid #ddd;"><strong>Employee:</strong></td>
                                <td style="padding: 8px; border: 1px solid #ddd;">{{EmployeeName}}</td>
                            </tr>
                            <tr>
                                <td style="padding: 8px; border: 1px solid #ddd;"><strong>Period:</strong></td>
                                <td style="padding: 8px; border: 1px solid #ddd;">{{Period}}</td>
                            </tr>
                            <tr>
                                <td style="padding: 8px; border: 1px solid #ddd;"><strong>Amount:</strong></td>
                                <td style="padding: 8px; border: 1px solid #ddd;">{{Currency}} {{Amount}}</td>
                            </tr>
                            <tr>
                                <td style="padding: 8px; border: 1px solid #ddd;"><strong>Approval Level:</strong></td>
                                <td style="padding: 8px; border: 1px solid #ddd;">{{ApprovalLevel}}</td>
                            </tr>
                        </table>
                        <p><a href="{{ActionUrl}}" style="background-color: #3498db; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">Review Now</a></p>
                        <p>Please review and take action within {{SlaHours}} hours.</p>
                        <p>Best regards,<br/>Dorise Incentive System</p>
                    </body>
                    </html>
                    """,
                IsHtml = true,
                DefaultChannel = NotificationChannel.Both,
                DefaultType = NotificationType.Action,
                DefaultPriority = NotificationPriority.High,
                Category = "Approval",
                RequiredPlaceholders = new List<string>
                {
                    "ApproverName", "EmployeeName", "Period", "Currency", "Amount", "ApprovalLevel", "ActionUrl", "SlaHours"
                }
            },

            ["approval-approved"] = new NotificationTemplateDto
            {
                Name = "approval-approved",
                Subject = "Incentive Approved - {{Period}}",
                Body = """
                    <html>
                    <body style="font-family: Arial, sans-serif; padding: 20px;">
                        <h2 style="color: #27ae60;">Incentive Approved</h2>
                        <p>Dear {{EmployeeName}},</p>
                        <p>Great news! Your incentive for <strong>{{Period}}</strong> has been approved.</p>
                        <table style="border-collapse: collapse; margin: 20px 0;">
                            <tr>
                                <td style="padding: 8px; border: 1px solid #ddd;"><strong>Approved Amount:</strong></td>
                                <td style="padding: 8px; border: 1px solid #ddd;">{{Currency}} {{Amount}}</td>
                            </tr>
                            <tr>
                                <td style="padding: 8px; border: 1px solid #ddd;"><strong>Approved By:</strong></td>
                                <td style="padding: 8px; border: 1px solid #ddd;">{{ApprovedBy}}</td>
                            </tr>
                            <tr>
                                <td style="padding: 8px; border: 1px solid #ddd;"><strong>Approved On:</strong></td>
                                <td style="padding: 8px; border: 1px solid #ddd;">{{ApprovedDate}}</td>
                            </tr>
                        </table>
                        <p>The payout will be processed in the next payroll cycle.</p>
                        <p>Best regards,<br/>Dorise Incentive System</p>
                    </body>
                    </html>
                    """,
                IsHtml = true,
                DefaultChannel = NotificationChannel.Both,
                DefaultType = NotificationType.Success,
                DefaultPriority = NotificationPriority.Normal,
                Category = "Approval",
                RequiredPlaceholders = new List<string>
                {
                    "EmployeeName", "Period", "Currency", "Amount", "ApprovedBy", "ApprovedDate"
                }
            },

            ["approval-rejected"] = new NotificationTemplateDto
            {
                Name = "approval-rejected",
                Subject = "Incentive Rejected - {{Period}}",
                Body = """
                    <html>
                    <body style="font-family: Arial, sans-serif; padding: 20px;">
                        <h2 style="color: #e74c3c;">Incentive Rejected</h2>
                        <p>Dear {{EmployeeName}},</p>
                        <p>Your incentive for <strong>{{Period}}</strong> has been rejected.</p>
                        <table style="border-collapse: collapse; margin: 20px 0;">
                            <tr>
                                <td style="padding: 8px; border: 1px solid #ddd;"><strong>Rejected By:</strong></td>
                                <td style="padding: 8px; border: 1px solid #ddd;">{{RejectedBy}}</td>
                            </tr>
                            <tr>
                                <td style="padding: 8px; border: 1px solid #ddd;"><strong>Reason:</strong></td>
                                <td style="padding: 8px; border: 1px solid #ddd;">{{Reason}}</td>
                            </tr>
                        </table>
                        <p>Please contact your manager for more information.</p>
                        <p>Best regards,<br/>Dorise Incentive System</p>
                    </body>
                    </html>
                    """,
                IsHtml = true,
                DefaultChannel = NotificationChannel.Both,
                DefaultType = NotificationType.Warning,
                DefaultPriority = NotificationPriority.Normal,
                Category = "Approval",
                RequiredPlaceholders = new List<string>
                {
                    "EmployeeName", "Period", "RejectedBy", "Reason"
                }
            },

            ["payment-processed"] = new NotificationTemplateDto
            {
                Name = "payment-processed",
                Subject = "Incentive Payment Processed - {{Period}}",
                Body = """
                    <html>
                    <body style="font-family: Arial, sans-serif; padding: 20px;">
                        <h2 style="color: #27ae60;">Payment Processed</h2>
                        <p>Dear {{EmployeeName}},</p>
                        <p>Your incentive payment for <strong>{{Period}}</strong> has been processed.</p>
                        <table style="border-collapse: collapse; margin: 20px 0;">
                            <tr>
                                <td style="padding: 8px; border: 1px solid #ddd;"><strong>Net Amount:</strong></td>
                                <td style="padding: 8px; border: 1px solid #ddd;">{{Currency}} {{NetAmount}}</td>
                            </tr>
                            <tr>
                                <td style="padding: 8px; border: 1px solid #ddd;"><strong>Payment Date:</strong></td>
                                <td style="padding: 8px; border: 1px solid #ddd;">{{PaymentDate}}</td>
                            </tr>
                            <tr>
                                <td style="padding: 8px; border: 1px solid #ddd;"><strong>Reference:</strong></td>
                                <td style="padding: 8px; border: 1px solid #ddd;">{{PaymentReference}}</td>
                            </tr>
                        </table>
                        <p>The amount will be credited to your salary account.</p>
                        <p>Best regards,<br/>Dorise Incentive System</p>
                    </body>
                    </html>
                    """,
                IsHtml = true,
                DefaultChannel = NotificationChannel.Both,
                DefaultType = NotificationType.Success,
                DefaultPriority = NotificationPriority.Normal,
                Category = "Payment",
                RequiredPlaceholders = new List<string>
                {
                    "EmployeeName", "Period", "Currency", "NetAmount", "PaymentDate", "PaymentReference"
                }
            },

            ["sla-warning"] = new NotificationTemplateDto
            {
                Name = "sla-warning",
                Subject = "⚠️ Approval SLA Warning - {{EmployeeName}}",
                Body = """
                    <html>
                    <body style="font-family: Arial, sans-serif; padding: 20px;">
                        <h2 style="color: #f39c12;">⚠️ SLA Warning</h2>
                        <p>Dear {{ApproverName}},</p>
                        <p>The following approval is approaching its SLA deadline:</p>
                        <table style="border-collapse: collapse; margin: 20px 0;">
                            <tr>
                                <td style="padding: 8px; border: 1px solid #ddd;"><strong>Employee:</strong></td>
                                <td style="padding: 8px; border: 1px solid #ddd;">{{EmployeeName}}</td>
                            </tr>
                            <tr>
                                <td style="padding: 8px; border: 1px solid #ddd;"><strong>Amount:</strong></td>
                                <td style="padding: 8px; border: 1px solid #ddd;">{{Currency}} {{Amount}}</td>
                            </tr>
                            <tr>
                                <td style="padding: 8px; border: 1px solid #ddd;"><strong>Time Remaining:</strong></td>
                                <td style="padding: 8px; border: 1px solid #ddd; color: #e74c3c;"><strong>{{TimeRemaining}}</strong></td>
                            </tr>
                        </table>
                        <p><a href="{{ActionUrl}}" style="background-color: #e74c3c; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">Take Action Now</a></p>
                        <p>Best regards,<br/>Dorise Incentive System</p>
                    </body>
                    </html>
                    """,
                IsHtml = true,
                DefaultChannel = NotificationChannel.Both,
                DefaultType = NotificationType.Warning,
                DefaultPriority = NotificationPriority.Urgent,
                Category = "Approval",
                RequiredPlaceholders = new List<string>
                {
                    "ApproverName", "EmployeeName", "Currency", "Amount", "TimeRemaining", "ActionUrl"
                }
            },

            ["welcome"] = new NotificationTemplateDto
            {
                Name = "welcome",
                Subject = "Welcome to Dorise Incentive System",
                Body = """
                    <html>
                    <body style="font-family: Arial, sans-serif; padding: 20px;">
                        <h2 style="color: #3498db;">Welcome to Dorise Incentive System</h2>
                        <p>Dear {{EmployeeName}},</p>
                        <p>Welcome to the Dorise Sales Incentive System! Your account has been created successfully.</p>
                        <p>You can now:</p>
                        <ul>
                            <li>View your incentive plans</li>
                            <li>Track your sales performance</li>
                            <li>Monitor your incentive calculations</li>
                            <li>View payment history</li>
                        </ul>
                        <p><a href="{{LoginUrl}}" style="background-color: #3498db; color: white; padding: 10px 20px; text-decoration: none; border-radius: 5px;">Login Now</a></p>
                        <p>Best regards,<br/>Dorise Incentive System</p>
                    </body>
                    </html>
                    """,
                IsHtml = true,
                DefaultChannel = NotificationChannel.Email,
                DefaultType = NotificationType.Information,
                DefaultPriority = NotificationPriority.Normal,
                Category = "System",
                RequiredPlaceholders = new List<string> { "EmployeeName", "LoginUrl" }
            }
        };
    }
}
