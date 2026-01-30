using Dorise.Incentive.Application.DataTransfer.DTOs;
using Dorise.Incentive.Domain.Entities;

namespace Dorise.Incentive.Application.DataTransfer.Services;

/// <summary>
/// Service interface for data import operations.
/// "I bent my Wookie!" - Bending data into the right shape for import!
/// </summary>
public interface IDataImportService
{
    // Import Operations
    Task<FileUploadResponse> UploadFileAsync(
        Stream fileStream,
        string fileName,
        CreateImportRequest request,
        CancellationToken cancellationToken = default);

    Task<ImportValidationResponse> ValidateImportAsync(
        Guid importId,
        CancellationToken cancellationToken = default);

    Task<DataImportDto> StartImportAsync(
        Guid importId,
        CancellationToken cancellationToken = default);

    Task<ImportResultDto> GetImportResultAsync(
        Guid importId,
        CancellationToken cancellationToken = default);

    Task<DataImportDto> CancelImportAsync(
        Guid importId,
        string? reason = null,
        CancellationToken cancellationToken = default);

    // Import Queries
    Task<DataImportDto?> GetImportByIdAsync(
        Guid importId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DataImportDto>> GetImportsAsync(
        ImportType? importType = null,
        ImportStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DataImportDto>> GetRecentImportsAsync(
        int count = 10,
        CancellationToken cancellationToken = default);

    // Field Mapping
    Task<EntityFieldsDto> GetImportFieldsAsync(
        ImportType importType,
        CancellationToken cancellationToken = default);

    Task<List<string>> DetectColumnsAsync(
        Stream fileStream,
        string fileFormat,
        CancellationToken cancellationToken = default);

    Task<List<ImportFieldMappingDto>> SuggestMappingsAsync(
        ImportType importType,
        List<string> sourceColumns,
        CancellationToken cancellationToken = default);

    // Processing
    Task ProcessImportAsync(
        Guid importId,
        CancellationToken cancellationToken = default);

    Task<byte[]> GetImportErrorReportAsync(
        Guid importId,
        CancellationToken cancellationToken = default);

    // Cleanup
    Task<int> DeleteOldImportsAsync(
        int daysOld,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for data export operations.
/// "The doctor said I wouldn't have so many nosebleeds if I kept my finger outta there."
/// - Exports pull data out cleanly!
/// </summary>
public interface IDataExportService
{
    // Export Operations
    Task<DataExportDto> CreateExportAsync(
        CreateExportRequest request,
        CancellationToken cancellationToken = default);

    Task<DataExportDto> StartExportAsync(
        Guid exportId,
        CancellationToken cancellationToken = default);

    Task<ExportResultDto> GetExportResultAsync(
        Guid exportId,
        CancellationToken cancellationToken = default);

    Task<DataExportDto> CancelExportAsync(
        Guid exportId,
        string? reason = null,
        CancellationToken cancellationToken = default);

    // Export Queries
    Task<DataExportDto?> GetExportByIdAsync(
        Guid exportId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DataExportDto>> GetExportsAsync(
        ExportType? exportType = null,
        ExportStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DataExportDto>> GetRecentExportsAsync(
        int count = 10,
        CancellationToken cancellationToken = default);

    // Download
    Task<Stream?> DownloadExportAsync(
        Guid exportId,
        CancellationToken cancellationToken = default);

    Task<string> RefreshDownloadUrlAsync(
        Guid exportId,
        CancellationToken cancellationToken = default);

    // Field Configuration
    Task<EntityFieldsDto> GetExportFieldsAsync(
        ExportType exportType,
        CancellationToken cancellationToken = default);

    Task<List<ExportColumnRequest>> GetDefaultColumnsAsync(
        ExportType exportType,
        CancellationToken cancellationToken = default);

    // Preview
    Task<List<Dictionary<string, object?>>> PreviewExportAsync(
        CreateExportRequest request,
        int maxRows = 10,
        CancellationToken cancellationToken = default);

    // Processing
    Task ProcessExportAsync(
        Guid exportId,
        CancellationToken cancellationToken = default);

    // Cleanup
    Task<int> DeleteExpiredExportsAsync(
        CancellationToken cancellationToken = default);

    Task<int> DeleteOldExportsAsync(
        int daysOld,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for data transfer templates.
/// "Hi, Super Nintendo Chalmers!" - Templates are super powerful for reusable transfers!
/// </summary>
public interface IDataTransferTemplateService
{
    // Template CRUD
    Task<DataTransferTemplateDto> CreateTemplateAsync(
        DataTransferTemplateRequest request,
        CancellationToken cancellationToken = default);

    Task<DataTransferTemplateDto> UpdateTemplateAsync(
        Guid templateId,
        DataTransferTemplateRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken = default);

    Task<DataTransferTemplateDto?> GetTemplateByIdAsync(
        Guid templateId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DataTransferTemplateDto>> GetTemplatesAsync(
        TransferDirection? direction = null,
        string? entityType = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    Task<DataTransferTemplateDto?> GetDefaultTemplateAsync(
        TransferDirection direction,
        string entityType,
        CancellationToken cancellationToken = default);

    // Template Management
    Task SetDefaultTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken = default);

    Task ActivateTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken = default);

    Task DeactivateTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken = default);

    // Template Configuration
    Task<string> GetMappingConfigurationAsync(
        Guid templateId,
        CancellationToken cancellationToken = default);

    Task UpdateMappingConfigurationAsync(
        Guid templateId,
        string mappingConfiguration,
        CancellationToken cancellationToken = default);

    Task<DataTransferTemplateDto> DuplicateTemplateAsync(
        Guid templateId,
        string newName,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for file parsing and generation.
/// "Me fail English? That's unpossible!" - File handling is unpossibly important!
/// </summary>
public interface IFileParserService
{
    // CSV Operations
    Task<List<string>> GetCsvHeadersAsync(
        Stream stream,
        string delimiter = ",",
        CancellationToken cancellationToken = default);

    Task<List<Dictionary<string, string>>> ParseCsvAsync(
        Stream stream,
        string delimiter = ",",
        bool hasHeader = true,
        int? maxRows = null,
        CancellationToken cancellationToken = default);

    Task<Stream> GenerateCsvAsync<T>(
        IEnumerable<T> data,
        List<ExportColumnRequest>? columns = null,
        ExportOptionsRequest? options = null,
        CancellationToken cancellationToken = default);

    // Excel Operations
    Task<List<string>> GetExcelSheetsAsync(
        Stream stream,
        CancellationToken cancellationToken = default);

    Task<List<string>> GetExcelHeadersAsync(
        Stream stream,
        string? sheetName = null,
        CancellationToken cancellationToken = default);

    Task<List<Dictionary<string, object?>>> ParseExcelAsync(
        Stream stream,
        string? sheetName = null,
        bool hasHeader = true,
        int? maxRows = null,
        CancellationToken cancellationToken = default);

    Task<Stream> GenerateExcelAsync<T>(
        IEnumerable<T> data,
        List<ExportColumnRequest>? columns = null,
        ExportOptionsRequest? options = null,
        CancellationToken cancellationToken = default);

    // JSON Operations
    Task<List<Dictionary<string, object?>>> ParseJsonAsync(
        Stream stream,
        string? rootPath = null,
        int? maxItems = null,
        CancellationToken cancellationToken = default);

    Task<Stream> GenerateJsonAsync<T>(
        IEnumerable<T> data,
        ExportOptionsRequest? options = null,
        CancellationToken cancellationToken = default);

    // XML Operations
    Task<List<Dictionary<string, object?>>> ParseXmlAsync(
        Stream stream,
        string? rootElement = null,
        string? itemElement = null,
        int? maxItems = null,
        CancellationToken cancellationToken = default);

    Task<Stream> GenerateXmlAsync<T>(
        IEnumerable<T> data,
        string rootElement = "Data",
        string itemElement = "Item",
        ExportOptionsRequest? options = null,
        CancellationToken cancellationToken = default);

    // PDF Operations (Export only)
    Task<Stream> GeneratePdfAsync<T>(
        IEnumerable<T> data,
        List<ExportColumnRequest>? columns = null,
        ExportOptionsRequest? options = null,
        CancellationToken cancellationToken = default);

    // File Detection
    string DetectFileFormat(string fileName);
    bool IsValidFileFormat(string fileName, TransferDirection direction);
    long GetMaxFileSize(string fileFormat);
}

/// <summary>
/// Service interface for data transfer statistics.
/// "I'm a unitard!" - Statistics are unified metrics for all transfers!
/// </summary>
public interface IDataTransferStatisticsService
{
    Task<DataTransferStatisticsDto> GetStatisticsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<Dictionary<string, int>> GetImportsByTypeAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<Dictionary<string, int>> GetExportsByTypeAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    Task<List<RecentTransferDto>> GetRecentTransfersAsync(
        int count = 20,
        CancellationToken cancellationToken = default);

    Task<Dictionary<string, long>> GetTransferVolumeTrendAsync(
        int days = 30,
        CancellationToken cancellationToken = default);
}
