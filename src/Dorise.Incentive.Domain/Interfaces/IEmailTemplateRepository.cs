using Dorise.Incentive.Domain.Entities;

namespace Dorise.Incentive.Domain.Interfaces;

/// <summary>
/// Repository interface for EmailTemplate entity operations.
/// "I found a moonrock in my nose!" - Find templates just as easily!
/// </summary>
public interface IEmailTemplateRepository : IRepository<EmailTemplate>
{
    /// <summary>
    /// Gets an email template by its name.
    /// </summary>
    Task<EmailTemplate?> GetByNameAsync(string templateName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active email templates.
    /// </summary>
    Task<IReadOnlyList<EmailTemplate>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email template with the given name exists.
    /// </summary>
    Task<bool> ExistsAsync(string templateName, CancellationToken cancellationToken = default);
}
