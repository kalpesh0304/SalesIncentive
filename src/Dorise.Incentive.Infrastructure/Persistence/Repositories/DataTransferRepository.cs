using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dorise.Incentive.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for DataImport entity.
/// "I'm learnding!" - Learning to store import data!
/// </summary>
public class DataImportRepository : RepositoryBase<DataImport>, IDataImportRepository
{
    public DataImportRepository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<DataImport>> GetByTypeAsync(
        ImportType importType,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(i => i.ImportType == importType)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DataImport>> GetByStatusAsync(
        ImportStatus status,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(i => i.Status == status)
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DataImport>> SearchAsync(
        ImportType? importType = null,
        ImportStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (importType.HasValue)
            query = query.Where(i => i.ImportType == importType.Value);

        if (status.HasValue)
            query = query.Where(i => i.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(i => i.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(i => i.CreatedAt <= toDate.Value);

        return await query
            .OrderByDescending(i => i.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DataImport>> GetRecentAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .OrderByDescending(i => i.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByStatusAsync(
        ImportStatus status,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(i => i.Status == status);

        if (fromDate.HasValue)
            query = query.Where(i => i.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(i => i.CreatedAt <= toDate.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> DeleteCompletedBeforeAsync(
        DateTime cutoffDate,
        CancellationToken cancellationToken = default)
    {
        var toDelete = await DbSet
            .Where(i => i.IsTerminal && i.CompletedAt < cutoffDate)
            .ToListAsync(cancellationToken);

        DbSet.RemoveRange(toDelete);
        return toDelete.Count;
    }

    public async Task<Dictionary<ImportType, int>> GetCountByTypeAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(i => i.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(i => i.CreatedAt <= toDate.Value);

        return await query
            .GroupBy(i => i.ImportType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, cancellationToken);
    }
}

/// <summary>
/// Repository implementation for DataExport entity.
/// "Me fail English? That's unpossible!" - Exports never fail... unpossibly!
/// </summary>
public class DataExportRepository : RepositoryBase<DataExport>, IDataExportRepository
{
    public DataExportRepository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<DataExport>> GetByTypeAsync(
        ExportType exportType,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.ExportType == exportType)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DataExport>> GetByStatusAsync(
        ExportStatus status,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.Status == status)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DataExport>> SearchAsync(
        ExportType? exportType = null,
        ExportStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (exportType.HasValue)
            query = query.Where(e => e.ExportType == exportType.Value);

        if (status.HasValue)
            query = query.Where(e => e.Status == status.Value);

        if (fromDate.HasValue)
            query = query.Where(e => e.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(e => e.CreatedAt <= toDate.Value);

        return await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DataExport>> GetRecentAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .OrderByDescending(e => e.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DataExport>> GetExpiredAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(e => e.Status == ExportStatus.Completed &&
                       e.DownloadUrlExpiry.HasValue &&
                       e.DownloadUrlExpiry < DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> CountByStatusAsync(
        ExportStatus status,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(e => e.Status == status);

        if (fromDate.HasValue)
            query = query.Where(e => e.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(e => e.CreatedAt <= toDate.Value);

        return await query.CountAsync(cancellationToken);
    }

    public async Task<int> DeleteCompletedBeforeAsync(
        DateTime cutoffDate,
        CancellationToken cancellationToken = default)
    {
        var toDelete = await DbSet
            .Where(e => e.IsTerminal && e.CompletedAt < cutoffDate)
            .ToListAsync(cancellationToken);

        DbSet.RemoveRange(toDelete);
        return toDelete.Count;
    }

    public async Task<int> DeleteExpiredAsync(
        CancellationToken cancellationToken = default)
    {
        var expired = await GetExpiredAsync(cancellationToken);
        DbSet.RemoveRange(expired);
        return expired.Count;
    }

    public async Task<Dictionary<ExportType, int>> GetCountByTypeAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (fromDate.HasValue)
            query = query.Where(e => e.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(e => e.CreatedAt <= toDate.Value);

        return await query
            .GroupBy(e => e.ExportType)
            .Select(g => new { Type = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Type, x => x.Count, cancellationToken);
    }
}

/// <summary>
/// Repository implementation for DataTransferTemplate entity.
/// "Hi, Super Nintendo Chalmers!" - Templates are super for organizing transfers!
/// </summary>
public class DataTransferTemplateRepository : RepositoryBase<DataTransferTemplate>, IDataTransferTemplateRepository
{
    public DataTransferTemplateRepository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<DataTransferTemplate?> GetByNameAsync(
        string templateName,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(t => t.TemplateName == templateName, cancellationToken);
    }

    public async Task<IReadOnlyList<DataTransferTemplate>> GetByDirectionAsync(
        TransferDirection direction,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.Direction == direction)
            .OrderBy(t => t.TemplateName)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<DataTransferTemplate>> GetByEntityTypeAsync(
        string entityType,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(t => t.EntityType == entityType)
            .OrderBy(t => t.TemplateName)
            .ToListAsync(cancellationToken);
    }

    public async Task<DataTransferTemplate?> GetDefaultAsync(
        TransferDirection direction,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(t =>
                t.Direction == direction &&
                t.EntityType == entityType &&
                t.IsDefault &&
                t.IsActive,
                cancellationToken);
    }

    public async Task<IReadOnlyList<DataTransferTemplate>> GetActiveAsync(
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

    public async Task ClearDefaultAsync(
        TransferDirection direction,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        var defaultTemplates = await DbSet
            .Where(t =>
                t.Direction == direction &&
                t.EntityType == entityType &&
                t.IsDefault)
            .ToListAsync(cancellationToken);

        foreach (var template in defaultTemplates)
        {
            template.ClearDefault();
        }
    }
}

/// <summary>
/// Repository implementation for ImportFieldMapping entity.
/// "The leprechaun tells me to burn things!" - Mappings tell fields where to go!
/// </summary>
public class ImportFieldMappingRepository : RepositoryBase<ImportFieldMapping>, IImportFieldMappingRepository
{
    public ImportFieldMappingRepository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<ImportFieldMapping>> GetByImportIdAsync(
        Guid dataImportId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(m => m.DataImportId == dataImportId)
            .OrderBy(m => m.Order)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteByImportIdAsync(
        Guid dataImportId,
        CancellationToken cancellationToken = default)
    {
        var mappings = await DbSet
            .Where(m => m.DataImportId == dataImportId)
            .ToListAsync(cancellationToken);

        DbSet.RemoveRange(mappings);
    }
}
