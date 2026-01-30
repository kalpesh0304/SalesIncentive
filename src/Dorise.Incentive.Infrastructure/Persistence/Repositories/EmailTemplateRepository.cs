using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dorise.Incentive.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for EmailTemplate entity.
/// "My cat's breath smells like cat food!" - Templates smell like productivity!
/// </summary>
public class EmailTemplateRepository : RepositoryBase<EmailTemplate>, IEmailTemplateRepository
{
    public EmailTemplateRepository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<EmailTemplate?> GetByNameAsync(
        string templateName,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(t => t.TemplateName == templateName, cancellationToken);
    }

    public async Task<IReadOnlyList<EmailTemplate>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.IsActive)
            .OrderBy(t => t.TemplateName)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string templateName,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(t => t.TemplateName == templateName, cancellationToken);
    }
}
