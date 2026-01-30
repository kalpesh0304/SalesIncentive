using System.Text.Json;
using Dorise.Incentive.Application.DataTransfer.DTOs;
using Dorise.Incentive.Application.DataTransfer.Services;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.DataTransfer;

/// <summary>
/// Implementation of IDataImportService.
/// "I'm learnding!" - Learning how to import data properly!
/// </summary>
public class DataImportService : IDataImportService
{
    private readonly IDataImportRepository _importRepository;
    private readonly IImportFieldMappingRepository _mappingRepository;
    private readonly IDataTransferTemplateRepository _templateRepository;
    private readonly IFileParserService _fileParser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DataImportService> _logger;

    public DataImportService(
        IDataImportRepository importRepository,
        IImportFieldMappingRepository mappingRepository,
        IDataTransferTemplateRepository templateRepository,
        IFileParserService fileParser,
        IUnitOfWork unitOfWork,
        ILogger<DataImportService> logger)
    {
        _importRepository = importRepository;
        _mappingRepository = mappingRepository;
        _templateRepository = templateRepository;
        _fileParser = fileParser;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<FileUploadResponse> UploadFileAsync(
        Stream fileStream,
        string fileName,
        CreateImportRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading file {FileName} for import type {ImportType}",
            fileName, request.ImportType);

        var fileFormat = _fileParser.DetectFileFormat(fileName);
        if (!_fileParser.IsValidFileFormat(fileName, TransferDirection.Import))
            throw new InvalidOperationException($"File format {fileFormat} is not supported for import");

        // Read file to memory for processing
        using var memoryStream = new MemoryStream();
        await fileStream.CopyToAsync(memoryStream, cancellationToken);
        var fileSize = memoryStream.Length;
        memoryStream.Position = 0;

        // Detect columns
        var columns = await DetectColumnsAsync(memoryStream, fileFormat, cancellationToken);
        memoryStream.Position = 0;

        // Get preview rows
        var previewRows = await GetPreviewRowsAsync(memoryStream, fileFormat, 5, cancellationToken);

        // Get mapping configuration from template if provided
        string? mappingConfig = null;
        string? validationRules = null;
        string? options = null;

        if (request.TemplateId.HasValue)
        {
            var template = await _templateRepository.GetByIdAsync(request.TemplateId.Value, cancellationToken);
            if (template != null)
            {
                mappingConfig = template.MappingConfiguration;
                validationRules = template.ValidationRules;
                options = template.DefaultOptions;
            }
        }

        // Override with request mappings if provided
        if (request.FieldMappings?.Any() == true)
        {
            mappingConfig = JsonSerializer.Serialize(request.FieldMappings);
        }

        if (request.Options != null)
        {
            options = JsonSerializer.Serialize(request.Options);
        }

        // Create import record
        var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(fileName)}";
        var dataImport = DataImport.Create(
            request.ImportName,
            request.ImportType,
            storedFileName,
            fileName,
            fileSize,
            fileFormat,
            mappingConfig,
            validationRules,
            options,
            request.DryRun);

        await _importRepository.AddAsync(dataImport, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created import {ImportId} for file {FileName}",
            dataImport.Id, fileName);

        return new FileUploadResponse
        {
            ImportId = dataImport.Id,
            FileName = fileName,
            FileSize = fileSize,
            FileFormat = fileFormat,
            DetectedColumns = columns,
            EstimatedRowCount = previewRows?.Count,
            PreviewRows = previewRows
        };
    }

    public async Task<ImportValidationResponse> ValidateImportAsync(
        Guid importId,
        CancellationToken cancellationToken = default)
    {
        var dataImport = await _importRepository.GetByIdAsync(importId, cancellationToken)
            ?? throw new InvalidOperationException($"Import {importId} not found");

        _logger.LogInformation("Validating import {ImportId}", importId);

        dataImport.MarkAsValidating();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var errors = new List<ValidationErrorDto>();
        var warnings = new List<ValidationWarningDto>();
        var validRecords = 0;
        var invalidRecords = 0;

        // Simulate validation logic - in real implementation this would parse and validate
        var totalRecords = 100; // Would be determined from file
        validRecords = 95;
        invalidRecords = 5;

        dataImport.SetTotalRecords(totalRecords);
        dataImport.MarkAsValidated(dataImport.DryRun ? JsonSerializer.Serialize(new { ValidRecords = validRecords }) : null);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Validation complete for import {ImportId}. Valid: {Valid}, Invalid: {Invalid}",
            importId, validRecords, invalidRecords);

        return new ImportValidationResponse
        {
            ImportId = importId,
            IsValid = invalidRecords == 0,
            TotalRecords = totalRecords,
            ValidRecords = validRecords,
            InvalidRecords = invalidRecords,
            Errors = errors,
            Warnings = warnings
        };
    }

    public async Task<DataImportDto> StartImportAsync(
        Guid importId,
        CancellationToken cancellationToken = default)
    {
        var dataImport = await _importRepository.GetByIdAsync(importId, cancellationToken)
            ?? throw new InvalidOperationException($"Import {importId} not found");

        _logger.LogInformation("Starting import {ImportId}", importId);

        dataImport.Start();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // In real implementation, this would queue a background job
        // For now, simulate immediate processing
        await ProcessImportAsync(importId, cancellationToken);

        return MapToDto(dataImport);
    }

    public async Task<ImportResultDto> GetImportResultAsync(
        Guid importId,
        CancellationToken cancellationToken = default)
    {
        var dataImport = await _importRepository.GetByIdAsync(importId, cancellationToken)
            ?? throw new InvalidOperationException($"Import {importId} not found");

        return new ImportResultDto
        {
            ImportId = importId,
            Status = dataImport.Status,
            TotalRecords = dataImport.TotalRecords,
            SuccessfulRecords = dataImport.SuccessfulRecords,
            FailedRecords = dataImport.FailedRecords,
            SkippedRecords = dataImport.SkippedRecords,
            Duration = dataImport.Duration ?? TimeSpan.Zero
        };
    }

    public async Task<DataImportDto> CancelImportAsync(
        Guid importId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var dataImport = await _importRepository.GetByIdAsync(importId, cancellationToken)
            ?? throw new InvalidOperationException($"Import {importId} not found");

        _logger.LogInformation("Cancelling import {ImportId}. Reason: {Reason}", importId, reason);

        dataImport.Cancel(reason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(dataImport);
    }

    public async Task<DataImportDto?> GetImportByIdAsync(
        Guid importId,
        CancellationToken cancellationToken = default)
    {
        var dataImport = await _importRepository.GetByIdAsync(importId, cancellationToken);
        return dataImport == null ? null : MapToDto(dataImport);
    }

    public async Task<IReadOnlyList<DataImportDto>> GetImportsAsync(
        ImportType? importType = null,
        ImportStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var imports = await _importRepository.SearchAsync(
            importType, status, fromDate, toDate, page, pageSize, cancellationToken);

        return imports.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<DataImportDto>> GetRecentImportsAsync(
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        var imports = await _importRepository.GetRecentAsync(count, cancellationToken);
        return imports.Select(MapToDto).ToList();
    }

    public Task<EntityFieldsDto> GetImportFieldsAsync(
        ImportType importType,
        CancellationToken cancellationToken = default)
    {
        var fields = importType switch
        {
            ImportType.Employees => GetEmployeeFields(),
            ImportType.Departments => GetDepartmentFields(),
            ImportType.SalesData => GetSalesDataFields(),
            ImportType.Targets => GetTargetFields(),
            _ => new List<FieldDefinitionDto>()
        };

        return Task.FromResult(new EntityFieldsDto
        {
            EntityType = importType.ToString(),
            Fields = fields
        });
    }

    public async Task<List<string>> DetectColumnsAsync(
        Stream fileStream,
        string fileFormat,
        CancellationToken cancellationToken = default)
    {
        return fileFormat.ToLowerInvariant() switch
        {
            "csv" => await _fileParser.GetCsvHeadersAsync(fileStream, ",", cancellationToken),
            "xlsx" or "xls" => await _fileParser.GetExcelHeadersAsync(fileStream, null, cancellationToken),
            _ => new List<string>()
        };
    }

    public Task<List<ImportFieldMappingDto>> SuggestMappingsAsync(
        ImportType importType,
        List<string> sourceColumns,
        CancellationToken cancellationToken = default)
    {
        var entityFields = importType switch
        {
            ImportType.Employees => GetEmployeeFields(),
            ImportType.Departments => GetDepartmentFields(),
            _ => new List<FieldDefinitionDto>()
        };

        var suggestions = new List<ImportFieldMappingDto>();
        var order = 0;

        foreach (var sourceColumn in sourceColumns)
        {
            var matchedField = entityFields.FirstOrDefault(f =>
                f.FieldName.Equals(sourceColumn, StringComparison.OrdinalIgnoreCase) ||
                f.DisplayName.Equals(sourceColumn, StringComparison.OrdinalIgnoreCase));

            if (matchedField != null)
            {
                suggestions.Add(new ImportFieldMappingDto
                {
                    Id = Guid.NewGuid(),
                    SourceField = sourceColumn,
                    TargetField = matchedField.FieldName,
                    DataType = matchedField.DataType,
                    IsRequired = matchedField.IsRequired,
                    Order = order++
                });
            }
        }

        return Task.FromResult(suggestions);
    }

    public async Task ProcessImportAsync(
        Guid importId,
        CancellationToken cancellationToken = default)
    {
        var dataImport = await _importRepository.GetByIdAsync(importId, cancellationToken)
            ?? throw new InvalidOperationException($"Import {importId} not found");

        _logger.LogInformation("Processing import {ImportId}", importId);

        try
        {
            // Simulate processing
            var totalRecords = dataImport.TotalRecords > 0 ? dataImport.TotalRecords : 100;
            var successful = 0;
            var failed = 0;
            var skipped = 0;

            for (int i = 0; i < totalRecords; i++)
            {
                // Simulate record processing
                if (i % 20 == 0) // 5% failure rate
                    failed++;
                else if (i % 50 == 0) // 2% skip rate
                    skipped++;
                else
                    successful++;

                if (i % 10 == 0)
                {
                    dataImport.UpdateProgress(i + 1, successful, failed, skipped);
                }
            }

            dataImport.UpdateProgress(totalRecords, successful, failed, skipped);
            dataImport.Complete();

            _logger.LogInformation("Import {ImportId} completed. Success: {Success}, Failed: {Failed}, Skipped: {Skipped}",
                importId, successful, failed, skipped);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import {ImportId} failed", importId);
            dataImport.Fail(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public Task<byte[]> GetImportErrorReportAsync(
        Guid importId,
        CancellationToken cancellationToken = default)
    {
        // Generate error report as CSV
        var report = "Row,Field,Error,Value\n";
        return Task.FromResult(System.Text.Encoding.UTF8.GetBytes(report));
    }

    public async Task<int> DeleteOldImportsAsync(
        int daysOld,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var count = await _importRepository.DeleteCompletedBeforeAsync(cutoffDate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted {Count} old imports older than {Days} days", count, daysOld);
        return count;
    }

    private async Task<List<Dictionary<string, object?>>?> GetPreviewRowsAsync(
        Stream stream,
        string fileFormat,
        int maxRows,
        CancellationToken cancellationToken)
    {
        return fileFormat.ToLowerInvariant() switch
        {
            "csv" => (await _fileParser.ParseCsvAsync(stream, ",", true, maxRows, cancellationToken))
                .Select(r => r.ToDictionary(k => k.Key, v => (object?)v.Value)).ToList(),
            "xlsx" or "xls" => await _fileParser.ParseExcelAsync(stream, null, true, maxRows, cancellationToken),
            "json" => await _fileParser.ParseJsonAsync(stream, null, maxRows, cancellationToken),
            _ => null
        };
    }

    private static List<FieldDefinitionDto> GetEmployeeFields() => new()
    {
        new() { FieldName = "EmployeeNumber", DisplayName = "Employee Number", DataType = "string", IsRequired = true, IsKey = true },
        new() { FieldName = "FirstName", DisplayName = "First Name", DataType = "string", IsRequired = true },
        new() { FieldName = "LastName", DisplayName = "Last Name", DataType = "string", IsRequired = true },
        new() { FieldName = "Email", DisplayName = "Email", DataType = "string", IsRequired = true },
        new() { FieldName = "HireDate", DisplayName = "Hire Date", DataType = "date", IsRequired = true, Format = "yyyy-MM-dd" },
        new() { FieldName = "DepartmentCode", DisplayName = "Department Code", DataType = "string" },
        new() { FieldName = "JobTitle", DisplayName = "Job Title", DataType = "string" },
        new() { FieldName = "ManagerEmployeeNumber", DisplayName = "Manager Employee Number", DataType = "string" }
    };

    private static List<FieldDefinitionDto> GetDepartmentFields() => new()
    {
        new() { FieldName = "DepartmentCode", DisplayName = "Department Code", DataType = "string", IsRequired = true, IsKey = true },
        new() { FieldName = "DepartmentName", DisplayName = "Department Name", DataType = "string", IsRequired = true },
        new() { FieldName = "ParentDepartmentCode", DisplayName = "Parent Department Code", DataType = "string" },
        new() { FieldName = "ManagerEmployeeNumber", DisplayName = "Manager Employee Number", DataType = "string" }
    };

    private static List<FieldDefinitionDto> GetSalesDataFields() => new()
    {
        new() { FieldName = "EmployeeNumber", DisplayName = "Employee Number", DataType = "string", IsRequired = true },
        new() { FieldName = "Period", DisplayName = "Period", DataType = "string", IsRequired = true, Format = "yyyy-MM" },
        new() { FieldName = "SalesAmount", DisplayName = "Sales Amount", DataType = "decimal", IsRequired = true },
        new() { FieldName = "TargetAmount", DisplayName = "Target Amount", DataType = "decimal" },
        new() { FieldName = "Category", DisplayName = "Category", DataType = "string" }
    };

    private static List<FieldDefinitionDto> GetTargetFields() => new()
    {
        new() { FieldName = "EmployeeNumber", DisplayName = "Employee Number", DataType = "string", IsRequired = true },
        new() { FieldName = "Period", DisplayName = "Period", DataType = "string", IsRequired = true, Format = "yyyy-MM" },
        new() { FieldName = "TargetAmount", DisplayName = "Target Amount", DataType = "decimal", IsRequired = true },
        new() { FieldName = "TargetType", DisplayName = "Target Type", DataType = "string" }
    };

    private static DataImportDto MapToDto(DataImport import) => new()
    {
        Id = import.Id,
        ImportName = import.ImportName,
        ImportType = import.ImportType,
        Status = import.Status,
        FileName = import.FileName,
        OriginalFileName = import.OriginalFileName,
        FileSize = import.FileSize,
        FileFormat = import.FileFormat,
        TotalRecords = import.TotalRecords,
        ProcessedRecords = import.ProcessedRecords,
        SuccessfulRecords = import.SuccessfulRecords,
        FailedRecords = import.FailedRecords,
        SkippedRecords = import.SkippedRecords,
        ProgressPercentage = import.ProgressPercentage,
        StartedAt = import.StartedAt,
        CompletedAt = import.CompletedAt,
        Duration = import.Duration,
        BackgroundJobId = import.BackgroundJobId,
        DryRun = import.DryRun,
        Errors = string.IsNullOrEmpty(import.ErrorLog) ? null
            : JsonSerializer.Deserialize<List<string>>(import.ErrorLog),
        Warnings = string.IsNullOrEmpty(import.WarningLog) ? null
            : JsonSerializer.Deserialize<List<string>>(import.WarningLog),
        CreatedAt = import.CreatedAt,
        CreatedBy = import.CreatedBy
    };
}

/// <summary>
/// Implementation of IDataExportService.
/// "Me fail English? That's unpossible!" - Exporting data is unpossibly easy!
/// </summary>
public class DataExportService : IDataExportService
{
    private readonly IDataExportRepository _exportRepository;
    private readonly IDataTransferTemplateRepository _templateRepository;
    private readonly IFileParserService _fileParser;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DataExportService> _logger;

    public DataExportService(
        IDataExportRepository exportRepository,
        IDataTransferTemplateRepository templateRepository,
        IFileParserService fileParser,
        IUnitOfWork unitOfWork,
        ILogger<DataExportService> logger)
    {
        _exportRepository = exportRepository;
        _templateRepository = templateRepository;
        _fileParser = fileParser;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DataExportDto> CreateExportAsync(
        CreateExportRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating export {ExportName} of type {ExportType}",
            request.ExportName, request.ExportType);

        if (!SupportedFileFormats.IsValidExportFormat(request.FileFormat))
            throw new InvalidOperationException($"File format {request.FileFormat} is not supported for export");

        string? filterCriteria = null;
        string? columnConfig = null;
        string? options = null;

        if (request.TemplateId.HasValue)
        {
            var template = await _templateRepository.GetByIdAsync(request.TemplateId.Value, cancellationToken);
            if (template != null)
            {
                columnConfig = template.ColumnConfiguration;
                options = template.DefaultOptions;
            }
        }

        if (request.Filters != null)
            filterCriteria = JsonSerializer.Serialize(request.Filters);

        if (request.Columns?.Any() == true)
            columnConfig = JsonSerializer.Serialize(request.Columns);

        if (request.Options != null)
            options = JsonSerializer.Serialize(request.Options);

        var dataExport = DataExport.Create(
            request.ExportName,
            request.ExportType,
            request.FileFormat,
            filterCriteria,
            columnConfig,
            options);

        await _exportRepository.AddAsync(dataExport, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created export {ExportId}", dataExport.Id);

        return MapToDto(dataExport);
    }

    public async Task<DataExportDto> StartExportAsync(
        Guid exportId,
        CancellationToken cancellationToken = default)
    {
        var dataExport = await _exportRepository.GetByIdAsync(exportId, cancellationToken)
            ?? throw new InvalidOperationException($"Export {exportId} not found");

        _logger.LogInformation("Starting export {ExportId}", exportId);

        dataExport.Start();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // In real implementation, this would queue a background job
        await ProcessExportAsync(exportId, cancellationToken);

        dataExport = await _exportRepository.GetByIdAsync(exportId, cancellationToken);
        return MapToDto(dataExport!);
    }

    public async Task<ExportResultDto> GetExportResultAsync(
        Guid exportId,
        CancellationToken cancellationToken = default)
    {
        var dataExport = await _exportRepository.GetByIdAsync(exportId, cancellationToken)
            ?? throw new InvalidOperationException($"Export {exportId} not found");

        return new ExportResultDto
        {
            ExportId = exportId,
            Status = dataExport.Status,
            FileName = dataExport.FileName,
            FileSize = dataExport.FileSize,
            DownloadUrl = dataExport.DownloadUrl,
            DownloadUrlExpiry = dataExport.DownloadUrlExpiry,
            TotalRecords = dataExport.TotalRecords,
            ExportedRecords = dataExport.ExportedRecords,
            Duration = dataExport.Duration ?? TimeSpan.Zero,
            ErrorMessage = dataExport.ErrorMessage
        };
    }

    public async Task<DataExportDto> CancelExportAsync(
        Guid exportId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var dataExport = await _exportRepository.GetByIdAsync(exportId, cancellationToken)
            ?? throw new InvalidOperationException($"Export {exportId} not found");

        _logger.LogInformation("Cancelling export {ExportId}. Reason: {Reason}", exportId, reason);

        dataExport.Cancel(reason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(dataExport);
    }

    public async Task<DataExportDto?> GetExportByIdAsync(
        Guid exportId,
        CancellationToken cancellationToken = default)
    {
        var dataExport = await _exportRepository.GetByIdAsync(exportId, cancellationToken);
        return dataExport == null ? null : MapToDto(dataExport);
    }

    public async Task<IReadOnlyList<DataExportDto>> GetExportsAsync(
        ExportType? exportType = null,
        ExportStatus? status = null,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var exports = await _exportRepository.SearchAsync(
            exportType, status, fromDate, toDate, page, pageSize, cancellationToken);

        return exports.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<DataExportDto>> GetRecentExportsAsync(
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        var exports = await _exportRepository.GetRecentAsync(count, cancellationToken);
        return exports.Select(MapToDto).ToList();
    }

    public Task<Stream?> DownloadExportAsync(
        Guid exportId,
        CancellationToken cancellationToken = default)
    {
        // In real implementation, this would return the file stream from storage
        _logger.LogInformation("Download requested for export {ExportId}", exportId);
        return Task.FromResult<Stream?>(null);
    }

    public Task<string> RefreshDownloadUrlAsync(
        Guid exportId,
        CancellationToken cancellationToken = default)
    {
        // In real implementation, this would generate a new SAS URL
        var newUrl = $"https://storage.example.com/exports/{exportId}?token={Guid.NewGuid()}";
        return Task.FromResult(newUrl);
    }

    public Task<EntityFieldsDto> GetExportFieldsAsync(
        ExportType exportType,
        CancellationToken cancellationToken = default)
    {
        var fields = exportType switch
        {
            ExportType.Employees => GetEmployeeExportFields(),
            ExportType.Calculations => GetCalculationExportFields(),
            ExportType.AuditLogs => GetAuditLogExportFields(),
            _ => new List<FieldDefinitionDto>()
        };

        return Task.FromResult(new EntityFieldsDto
        {
            EntityType = exportType.ToString(),
            Fields = fields
        });
    }

    public Task<List<ExportColumnRequest>> GetDefaultColumnsAsync(
        ExportType exportType,
        CancellationToken cancellationToken = default)
    {
        var fields = exportType switch
        {
            ExportType.Employees => GetEmployeeExportFields(),
            ExportType.Calculations => GetCalculationExportFields(),
            _ => new List<FieldDefinitionDto>()
        };

        var columns = fields.Select((f, i) => new ExportColumnRequest
        {
            FieldName = f.FieldName,
            DisplayName = f.DisplayName,
            Order = i,
            Format = f.Format
        }).ToList();

        return Task.FromResult(columns);
    }

    public Task<List<Dictionary<string, object?>>> PreviewExportAsync(
        CreateExportRequest request,
        int maxRows = 10,
        CancellationToken cancellationToken = default)
    {
        // In real implementation, this would query and return preview data
        var previewData = new List<Dictionary<string, object?>>();
        return Task.FromResult(previewData);
    }

    public async Task ProcessExportAsync(
        Guid exportId,
        CancellationToken cancellationToken = default)
    {
        var dataExport = await _exportRepository.GetByIdAsync(exportId, cancellationToken)
            ?? throw new InvalidOperationException($"Export {exportId} not found");

        _logger.LogInformation("Processing export {ExportId}", exportId);

        try
        {
            // Simulate processing
            var totalRecords = 100;
            dataExport.SetTotalRecords(totalRecords);

            for (int i = 0; i < totalRecords; i += 10)
            {
                dataExport.UpdateProgress(Math.Min(i + 10, totalRecords));
            }

            var fileName = $"{dataExport.ExportName}_{DateTime.UtcNow:yyyyMMddHHmmss}.{dataExport.FileFormat}";
            var filePath = $"/exports/{exportId}/{fileName}";
            var fileSize = 1024L * totalRecords; // Simulated file size
            var downloadUrl = $"https://storage.example.com{filePath}?token={Guid.NewGuid()}";
            var urlExpiry = DateTime.UtcNow.AddHours(24);

            dataExport.Complete(fileName, filePath, fileSize, downloadUrl, urlExpiry);

            _logger.LogInformation("Export {ExportId} completed. Records: {Records}, Size: {Size}",
                exportId, totalRecords, fileSize);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Export {ExportId} failed", exportId);
            dataExport.Fail(ex.Message);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> DeleteExpiredExportsAsync(CancellationToken cancellationToken = default)
    {
        var count = await _exportRepository.DeleteExpiredAsync(cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted {Count} expired exports", count);
        return count;
    }

    public async Task<int> DeleteOldExportsAsync(
        int daysOld,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        var count = await _exportRepository.DeleteCompletedBeforeAsync(cutoffDate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted {Count} old exports older than {Days} days", count, daysOld);
        return count;
    }

    private static List<FieldDefinitionDto> GetEmployeeExportFields() => new()
    {
        new() { FieldName = "EmployeeNumber", DisplayName = "Employee Number", DataType = "string" },
        new() { FieldName = "FullName", DisplayName = "Full Name", DataType = "string" },
        new() { FieldName = "Email", DisplayName = "Email", DataType = "string" },
        new() { FieldName = "Department", DisplayName = "Department", DataType = "string" },
        new() { FieldName = "JobTitle", DisplayName = "Job Title", DataType = "string" },
        new() { FieldName = "HireDate", DisplayName = "Hire Date", DataType = "date", Format = "yyyy-MM-dd" },
        new() { FieldName = "Status", DisplayName = "Status", DataType = "string" }
    };

    private static List<FieldDefinitionDto> GetCalculationExportFields() => new()
    {
        new() { FieldName = "EmployeeNumber", DisplayName = "Employee Number", DataType = "string" },
        new() { FieldName = "EmployeeName", DisplayName = "Employee Name", DataType = "string" },
        new() { FieldName = "Period", DisplayName = "Period", DataType = "string" },
        new() { FieldName = "SalesAmount", DisplayName = "Sales Amount", DataType = "decimal", Format = "#,##0.00" },
        new() { FieldName = "IncentiveAmount", DisplayName = "Incentive Amount", DataType = "decimal", Format = "#,##0.00" },
        new() { FieldName = "Status", DisplayName = "Status", DataType = "string" }
    };

    private static List<FieldDefinitionDto> GetAuditLogExportFields() => new()
    {
        new() { FieldName = "Timestamp", DisplayName = "Timestamp", DataType = "datetime", Format = "yyyy-MM-dd HH:mm:ss" },
        new() { FieldName = "Action", DisplayName = "Action", DataType = "string" },
        new() { FieldName = "EntityType", DisplayName = "Entity Type", DataType = "string" },
        new() { FieldName = "EntityId", DisplayName = "Entity ID", DataType = "string" },
        new() { FieldName = "UserName", DisplayName = "User Name", DataType = "string" },
        new() { FieldName = "Changes", DisplayName = "Changes", DataType = "string" }
    };

    private static DataExportDto MapToDto(DataExport export) => new()
    {
        Id = export.Id,
        ExportName = export.ExportName,
        ExportType = export.ExportType,
        Status = export.Status,
        FileFormat = export.FileFormat,
        FileName = export.FileName,
        FileSize = export.FileSize,
        DownloadUrl = export.DownloadUrl,
        DownloadUrlExpiry = export.DownloadUrlExpiry,
        IsDownloadAvailable = export.IsDownloadAvailable,
        TotalRecords = export.TotalRecords,
        ExportedRecords = export.ExportedRecords,
        ProgressPercentage = export.ProgressPercentage,
        StartedAt = export.StartedAt,
        CompletedAt = export.CompletedAt,
        Duration = export.Duration,
        BackgroundJobId = export.BackgroundJobId,
        DownloadCount = export.DownloadCount,
        LastDownloadedAt = export.LastDownloadedAt,
        ErrorMessage = export.ErrorMessage,
        CreatedAt = export.CreatedAt,
        CreatedBy = export.CreatedBy
    };
}

/// <summary>
/// Implementation of IDataTransferTemplateService.
/// "Hi, Super Nintendo Chalmers!" - Templates are super for reusable transfers!
/// </summary>
public class DataTransferTemplateService : IDataTransferTemplateService
{
    private readonly IDataTransferTemplateRepository _templateRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<DataTransferTemplateService> _logger;

    public DataTransferTemplateService(
        IDataTransferTemplateRepository templateRepository,
        IUnitOfWork unitOfWork,
        ILogger<DataTransferTemplateService> logger)
    {
        _templateRepository = templateRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DataTransferTemplateDto> CreateTemplateAsync(
        DataTransferTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        if (await _templateRepository.ExistsAsync(request.TemplateName, cancellationToken))
            throw new InvalidOperationException($"Template with name '{request.TemplateName}' already exists");

        var mappingConfig = request.FieldMappings != null
            ? JsonSerializer.Serialize(request.FieldMappings)
            : null;

        var columnConfig = request.Columns != null
            ? JsonSerializer.Serialize(request.Columns)
            : null;

        var defaultOptions = request.Direction == TransferDirection.Import
            ? (request.ImportOptions != null ? JsonSerializer.Serialize(request.ImportOptions) : null)
            : (request.ExportOptions != null ? JsonSerializer.Serialize(request.ExportOptions) : null);

        var template = DataTransferTemplate.Create(
            request.TemplateName,
            request.Direction,
            request.EntityType,
            request.FileFormat,
            request.Description,
            mappingConfig,
            null,
            columnConfig,
            defaultOptions,
            request.IsDefault);

        if (request.IsDefault)
        {
            await _templateRepository.ClearDefaultAsync(request.Direction, request.EntityType, cancellationToken);
        }

        await _templateRepository.AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created template {TemplateId} - {TemplateName}", template.Id, template.TemplateName);

        return MapToDto(template);
    }

    public async Task<DataTransferTemplateDto> UpdateTemplateAsync(
        Guid templateId,
        DataTransferTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken)
            ?? throw new InvalidOperationException($"Template {templateId} not found");

        var mappingConfig = request.FieldMappings != null
            ? JsonSerializer.Serialize(request.FieldMappings)
            : null;

        var columnConfig = request.Columns != null
            ? JsonSerializer.Serialize(request.Columns)
            : null;

        var defaultOptions = request.Direction == TransferDirection.Import
            ? (request.ImportOptions != null ? JsonSerializer.Serialize(request.ImportOptions) : null)
            : (request.ExportOptions != null ? JsonSerializer.Serialize(request.ExportOptions) : null);

        template.Update(
            request.TemplateName,
            request.Description,
            mappingConfig,
            null,
            columnConfig,
            defaultOptions);

        if (request.IsDefault && !template.IsDefault)
        {
            await _templateRepository.ClearDefaultAsync(request.Direction, request.EntityType, cancellationToken);
            template.SetAsDefault();
        }
        else if (!request.IsDefault && template.IsDefault)
        {
            template.ClearDefault();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated template {TemplateId}", templateId);

        return MapToDto(template);
    }

    public async Task DeleteTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken)
            ?? throw new InvalidOperationException($"Template {templateId} not found");

        _templateRepository.Remove(template);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted template {TemplateId}", templateId);
    }

    public async Task<DataTransferTemplateDto?> GetTemplateByIdAsync(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken);
        return template == null ? null : MapToDto(template);
    }

    public async Task<IReadOnlyList<DataTransferTemplateDto>> GetTemplatesAsync(
        TransferDirection? direction = null,
        string? entityType = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<DataTransferTemplate> templates;

        if (direction.HasValue)
            templates = await _templateRepository.GetByDirectionAsync(direction.Value, cancellationToken);
        else if (!string.IsNullOrEmpty(entityType))
            templates = await _templateRepository.GetByEntityTypeAsync(entityType, cancellationToken);
        else if (isActive == true)
            templates = await _templateRepository.GetActiveAsync(cancellationToken);
        else
            templates = await _templateRepository.GetAllAsync(cancellationToken);

        return templates.Select(MapToDto).ToList();
    }

    public async Task<DataTransferTemplateDto?> GetDefaultTemplateAsync(
        TransferDirection direction,
        string entityType,
        CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetDefaultAsync(direction, entityType, cancellationToken);
        return template == null ? null : MapToDto(template);
    }

    public async Task SetDefaultTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken)
            ?? throw new InvalidOperationException($"Template {templateId} not found");

        await _templateRepository.ClearDefaultAsync(template.Direction, template.EntityType, cancellationToken);
        template.SetAsDefault();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Set template {TemplateId} as default", templateId);
    }

    public async Task ActivateTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken)
            ?? throw new InvalidOperationException($"Template {templateId} not found");

        template.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateTemplateAsync(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken)
            ?? throw new InvalidOperationException($"Template {templateId} not found");

        template.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<string> GetMappingConfigurationAsync(
        Guid templateId,
        CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken)
            ?? throw new InvalidOperationException($"Template {templateId} not found");

        return template.MappingConfiguration ?? "{}";
    }

    public async Task UpdateMappingConfigurationAsync(
        Guid templateId,
        string mappingConfiguration,
        CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken)
            ?? throw new InvalidOperationException($"Template {templateId} not found");

        template.Update(
            template.TemplateName,
            template.Description,
            mappingConfiguration,
            template.ValidationRules,
            template.ColumnConfiguration,
            template.DefaultOptions);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<DataTransferTemplateDto> DuplicateTemplateAsync(
        Guid templateId,
        string newName,
        CancellationToken cancellationToken = default)
    {
        var template = await _templateRepository.GetByIdAsync(templateId, cancellationToken)
            ?? throw new InvalidOperationException($"Template {templateId} not found");

        if (await _templateRepository.ExistsAsync(newName, cancellationToken))
            throw new InvalidOperationException($"Template with name '{newName}' already exists");

        var newTemplate = DataTransferTemplate.Create(
            newName,
            template.Direction,
            template.EntityType,
            template.FileFormat,
            template.Description,
            template.MappingConfiguration,
            template.ValidationRules,
            template.ColumnConfiguration,
            template.DefaultOptions,
            false); // Duplicate is never default

        await _templateRepository.AddAsync(newTemplate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Duplicated template {TemplateId} to {NewTemplateId}", templateId, newTemplate.Id);

        return MapToDto(newTemplate);
    }

    private static DataTransferTemplateDto MapToDto(DataTransferTemplate template) => new()
    {
        Id = template.Id,
        TemplateName = template.TemplateName,
        Description = template.Description,
        Direction = template.Direction,
        EntityType = template.EntityType,
        FileFormat = template.FileFormat,
        IsDefault = template.IsDefault,
        IsActive = template.IsActive,
        CreatedAt = template.CreatedAt,
        ModifiedAt = template.ModifiedAt
    };
}
