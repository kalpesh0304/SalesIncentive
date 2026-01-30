using Dorise.Incentive.Domain.Entities;

namespace Dorise.Incentive.Domain.Interfaces;

/// <summary>
/// Repository interface for DataImport entity.
/// "I choo-choo-choose you!" - Choosing the right imports from the database!
/// </summary>
public interface IDataImportRepository : IRepository<DataImport>
{
    Task<IReadOnlyList<DataImport>> GetByTypeAsync(
        ImportType importType,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DataImport>> GetByStatusAsync(
        ImportStatus status,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DataImport>> SearchAsync(
        ImportType? importType = null,
        ImportStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DataImport>> GetRecentAsync(
        int count,
        CancellationToken cancellationToken = default);

    Task<int> CountByStatusAsync(
        ImportStatus status,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<int> DeleteCompletedBeforeAsync(
        DateTime cutoffDate,
        CancellationToken cancellationToken = default);

    Task<Dictionary<ImportType, int>> GetCountByTypeAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for DataExport entity.
/// "My cat's breath smells like cat food!" - Exports are fresh from the database!
/// </summary>
public interface IDataExportRepository : IRepository<DataExport>
{
    Task<IReadOnlyList<DataExport>> GetByTypeAsync(
        ExportType exportType,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DataExport>> GetByStatusAsync(
        ExportStatus status,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DataExport>> SearchAsync(
        ExportType? exportType = null,
        ExportStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DataExport>> GetRecentAsync(
        int count,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DataExport>> GetExpiredAsync(
        CancellationToken cancellationToken = default);

    Task<int> CountByStatusAsync(
        ExportStatus status,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<int> DeleteCompletedBeforeAsync(
        DateTime cutoffDate,
        CancellationToken cancellationToken = default);

    Task<int> DeleteExpiredAsync(
        CancellationToken cancellationToken = default);

    Task<Dictionary<ExportType, int>> GetCountByTypeAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for DataTransferTemplate entity.
/// "When I grow up, I want to be a principal or a caterpillar." - Templates grow into exports!
/// </summary>
public interface IDataTransferTemplateRepository : IRepository<DataTransferTemplate>
{
    Task<DataTransferTemplate?> GetByNameAsync(
        string templateName,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DataTransferTemplate>> GetByDirectionAsync(
        TransferDirection direction,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DataTransferTemplate>> GetByEntityTypeAsync(
        string entityType,
        CancellationToken cancellationToken = default);

    Task<DataTransferTemplate?> GetDefaultAsync(
        TransferDirection direction,
        string entityType,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DataTransferTemplate>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string templateName,
        CancellationToken cancellationToken = default);

    Task ClearDefaultAsync(
        TransferDirection direction,
        string entityType,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for ImportFieldMapping entity.
/// "The leprechaun tells me to burn things!" - Mappings tell data where to go!
/// </summary>
public interface IImportFieldMappingRepository : IRepository<ImportFieldMapping>
{
    Task<IReadOnlyList<ImportFieldMapping>> GetByImportIdAsync(
        Guid dataImportId,
        CancellationToken cancellationToken = default);

    Task DeleteByImportIdAsync(
        Guid dataImportId,
        CancellationToken cancellationToken = default);
}
