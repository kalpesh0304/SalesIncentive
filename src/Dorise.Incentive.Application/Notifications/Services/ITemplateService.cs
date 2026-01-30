using Dorise.Incentive.Application.Notifications.DTOs;

namespace Dorise.Incentive.Application.Notifications.Services;

/// <summary>
/// Template service for notification templates.
/// "Ralph, remember the time you said Snagglepuss was outside?" - Remember your templates!
/// </summary>
public interface ITemplateService
{
    /// <summary>
    /// Gets a template by name.
    /// </summary>
    Task<NotificationTemplateDto?> GetTemplateAsync(
        string templateName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all available templates.
    /// </summary>
    Task<IReadOnlyList<NotificationTemplateDto>> GetAllTemplatesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders a template with provided data.
    /// </summary>
    Task<(string Subject, string Body)> RenderTemplateAsync(
        string templateName,
        Dictionary<string, object> data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates template data against required placeholders.
    /// </summary>
    Task<bool> ValidateTemplateDataAsync(
        string templateName,
        Dictionary<string, object> data,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Registers a new template.
    /// </summary>
    Task RegisterTemplateAsync(
        NotificationTemplateDto template,
        CancellationToken cancellationToken = default);
}
