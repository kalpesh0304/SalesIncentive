using Dorise.Incentive.Domain.Entities;

namespace Dorise.Incentive.Application.DataTransfer.DTOs;

/// <summary>
/// DTO for data import operations.
/// "My worm went in my mouth and then I ate it!" - Data goes in and gets processed!
/// </summary>
public record DataImportDto
{
    public Guid Id { get; init; }
    public string ImportName { get; init; } = null!;
    public ImportType ImportType { get; init; }
    public ImportStatus Status { get; init; }
    public string FileName { get; init; } = null!;
    public string? OriginalFileName { get; init; }
    public long FileSize { get; init; }
    public string FileFormat { get; init; } = null!;
    public int TotalRecords { get; init; }
    public int ProcessedRecords { get; init; }
    public int SuccessfulRecords { get; init; }
    public int FailedRecords { get; init; }
    public int SkippedRecords { get; init; }
    public double ProgressPercentage { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public TimeSpan? Duration { get; init; }
    public Guid? BackgroundJobId { get; init; }
    public bool DryRun { get; init; }
    public List<string>? Errors { get; init; }
    public List<string>? Warnings { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
}

/// <summary>
/// DTO for data export operations.
/// </summary>
public record DataExportDto
{
    public Guid Id { get; init; }
    public string ExportName { get; init; } = null!;
    public ExportType ExportType { get; init; }
    public ExportStatus Status { get; init; }
    public string FileFormat { get; init; } = null!;
    public string? FileName { get; init; }
    public long? FileSize { get; init; }
    public string? DownloadUrl { get; init; }
    public DateTime? DownloadUrlExpiry { get; init; }
    public bool IsDownloadAvailable { get; init; }
    public int TotalRecords { get; init; }
    public int ExportedRecords { get; init; }
    public double ProgressPercentage { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public TimeSpan? Duration { get; init; }
    public Guid? BackgroundJobId { get; init; }
    public int DownloadCount { get; init; }
    public DateTime? LastDownloadedAt { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
}

/// <summary>
/// DTO for data transfer templates.
/// </summary>
public record DataTransferTemplateDto
{
    public Guid Id { get; init; }
    public string TemplateName { get; init; } = null!;
    public string? Description { get; init; }
    public TransferDirection Direction { get; init; }
    public string EntityType { get; init; } = null!;
    public string FileFormat { get; init; } = null!;
    public bool IsDefault { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }
}

/// <summary>
/// DTO for import field mapping.
/// </summary>
public record ImportFieldMappingDto
{
    public Guid Id { get; init; }
    public string SourceField { get; init; } = null!;
    public string TargetField { get; init; } = null!;
    public string? DataType { get; init; }
    public string? DefaultValue { get; init; }
    public bool IsRequired { get; init; }
    public string? TransformExpression { get; init; }
    public string? ValidationRegex { get; init; }
    public int Order { get; init; }
}

// ============== Request DTOs ==============

/// <summary>
/// Request to create a new data import.
/// </summary>
public record CreateImportRequest
{
    public required string ImportName { get; init; }
    public required ImportType ImportType { get; init; }
    public Guid? TemplateId { get; init; }
    public List<ImportFieldMappingRequest>? FieldMappings { get; init; }
    public ImportOptionsRequest? Options { get; init; }
    public bool DryRun { get; init; }
}

/// <summary>
/// Request for import field mapping.
/// </summary>
public record ImportFieldMappingRequest
{
    public required string SourceField { get; init; }
    public required string TargetField { get; init; }
    public string? DataType { get; init; }
    public string? DefaultValue { get; init; }
    public bool IsRequired { get; init; }
    public string? TransformExpression { get; init; }
    public string? ValidationRegex { get; init; }
}

/// <summary>
/// Import options configuration.
/// </summary>
public record ImportOptionsRequest
{
    public bool HasHeaderRow { get; init; } = true;
    public string? Delimiter { get; init; } = ",";
    public string? DateFormat { get; init; } = "yyyy-MM-dd";
    public string? NumberFormat { get; init; }
    public bool SkipEmptyRows { get; init; } = true;
    public bool TrimWhitespace { get; init; } = true;
    public int? StartRow { get; init; }
    public int? EndRow { get; init; }
    public string? SheetName { get; init; } // For Excel files
    public ImportDuplicateHandling DuplicateHandling { get; init; } = ImportDuplicateHandling.Skip;
    public string? DuplicateKeyField { get; init; }
    public bool StopOnFirstError { get; init; }
    public int? MaxErrors { get; init; }
}

/// <summary>
/// Request to create a new data export.
/// </summary>
public record CreateExportRequest
{
    public required string ExportName { get; init; }
    public required ExportType ExportType { get; init; }
    public required string FileFormat { get; init; }
    public Guid? TemplateId { get; init; }
    public ExportFilterRequest? Filters { get; init; }
    public List<ExportColumnRequest>? Columns { get; init; }
    public ExportOptionsRequest? Options { get; init; }
}

/// <summary>
/// Export filter criteria.
/// </summary>
public record ExportFilterRequest
{
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? Period { get; init; }
    public List<Guid>? DepartmentIds { get; init; }
    public List<Guid>? EmployeeIds { get; init; }
    public List<string>? Statuses { get; init; }
    public Dictionary<string, string>? CustomFilters { get; init; }
}

/// <summary>
/// Export column configuration.
/// </summary>
public record ExportColumnRequest
{
    public required string FieldName { get; init; }
    public string? DisplayName { get; init; }
    public int Order { get; init; }
    public string? Format { get; init; }
    public int? Width { get; init; }
}

/// <summary>
/// Export options configuration.
/// </summary>
public record ExportOptionsRequest
{
    public bool IncludeHeaders { get; init; } = true;
    public string? Delimiter { get; init; } = ",";
    public string? DateFormat { get; init; } = "yyyy-MM-dd";
    public string? NumberFormat { get; init; } = "#,##0.00";
    public string? SheetName { get; init; } = "Export";
    public bool AutoFitColumns { get; init; } = true;
    public bool FreezeHeaderRow { get; init; } = true;
    public string? Title { get; init; }
    public bool IncludeSummary { get; init; }
    public int? MaxRecords { get; init; }
    public ExportSortRequest? Sort { get; init; }
}

/// <summary>
/// Export sort configuration.
/// </summary>
public record ExportSortRequest
{
    public required string Field { get; init; }
    public bool Descending { get; init; }
}

/// <summary>
/// Request to create/update a template.
/// </summary>
public record DataTransferTemplateRequest
{
    public required string TemplateName { get; init; }
    public string? Description { get; init; }
    public required TransferDirection Direction { get; init; }
    public required string EntityType { get; init; }
    public required string FileFormat { get; init; }
    public List<ImportFieldMappingRequest>? FieldMappings { get; init; }
    public List<ExportColumnRequest>? Columns { get; init; }
    public ImportOptionsRequest? ImportOptions { get; init; }
    public ExportOptionsRequest? ExportOptions { get; init; }
    public bool IsDefault { get; init; }
}

// ============== Response DTOs ==============

/// <summary>
/// Response for file upload.
/// </summary>
public record FileUploadResponse
{
    public Guid ImportId { get; init; }
    public string FileName { get; init; } = null!;
    public long FileSize { get; init; }
    public string FileFormat { get; init; } = null!;
    public List<string> DetectedColumns { get; init; } = new();
    public int? EstimatedRowCount { get; init; }
    public List<Dictionary<string, object?>>? PreviewRows { get; init; }
}

/// <summary>
/// Response for import validation.
/// </summary>
public record ImportValidationResponse
{
    public Guid ImportId { get; init; }
    public bool IsValid { get; init; }
    public int TotalRecords { get; init; }
    public int ValidRecords { get; init; }
    public int InvalidRecords { get; init; }
    public List<ValidationErrorDto> Errors { get; init; } = new();
    public List<ValidationWarningDto> Warnings { get; init; } = new();
    public List<Dictionary<string, object?>>? PreviewData { get; init; }
}

/// <summary>
/// Validation error details.
/// </summary>
public record ValidationErrorDto
{
    public int Row { get; init; }
    public string? Field { get; init; }
    public string Message { get; init; } = null!;
    public string? Value { get; init; }
    public string ErrorCode { get; init; } = null!;
}

/// <summary>
/// Validation warning details.
/// </summary>
public record ValidationWarningDto
{
    public int Row { get; init; }
    public string? Field { get; init; }
    public string Message { get; init; } = null!;
    public string? Value { get; init; }
    public string WarningCode { get; init; } = null!;
}

/// <summary>
/// Import result summary.
/// </summary>
public record ImportResultDto
{
    public Guid ImportId { get; init; }
    public ImportStatus Status { get; init; }
    public int TotalRecords { get; init; }
    public int SuccessfulRecords { get; init; }
    public int FailedRecords { get; init; }
    public int SkippedRecords { get; init; }
    public TimeSpan Duration { get; init; }
    public List<ImportRecordResultDto>? FailedRecordDetails { get; init; }
    public Dictionary<string, int>? CreatedEntities { get; init; }
    public Dictionary<string, int>? UpdatedEntities { get; init; }
}

/// <summary>
/// Individual record import result.
/// </summary>
public record ImportRecordResultDto
{
    public int Row { get; init; }
    public string Status { get; init; } = null!;
    public string? ErrorMessage { get; init; }
    public Dictionary<string, object?>? Data { get; init; }
}

/// <summary>
/// Export result with download information.
/// </summary>
public record ExportResultDto
{
    public Guid ExportId { get; init; }
    public ExportStatus Status { get; init; }
    public string? FileName { get; init; }
    public long? FileSize { get; init; }
    public string? DownloadUrl { get; init; }
    public DateTime? DownloadUrlExpiry { get; init; }
    public int TotalRecords { get; init; }
    public int ExportedRecords { get; init; }
    public TimeSpan Duration { get; init; }
    public string? ErrorMessage { get; init; }
}

/// <summary>
/// Available fields for import/export for a given entity type.
/// </summary>
public record EntityFieldsDto
{
    public string EntityType { get; init; } = null!;
    public List<FieldDefinitionDto> Fields { get; init; } = new();
}

/// <summary>
/// Field definition for mapping.
/// </summary>
public record FieldDefinitionDto
{
    public string FieldName { get; init; } = null!;
    public string DisplayName { get; init; } = null!;
    public string DataType { get; init; } = null!;
    public bool IsRequired { get; init; }
    public bool IsKey { get; init; }
    public string? Description { get; init; }
    public string? Format { get; init; }
    public List<string>? AllowedValues { get; init; }
    public string? DefaultValue { get; init; }
}

/// <summary>
/// Import/export statistics.
/// </summary>
public record DataTransferStatisticsDto
{
    public int TotalImports { get; init; }
    public int SuccessfulImports { get; init; }
    public int FailedImports { get; init; }
    public int TotalExports { get; init; }
    public int SuccessfulExports { get; init; }
    public int FailedExports { get; init; }
    public long TotalRecordsImported { get; init; }
    public long TotalRecordsExported { get; init; }
    public Dictionary<string, int> ImportsByType { get; init; } = new();
    public Dictionary<string, int> ExportsByType { get; init; } = new();
    public List<RecentTransferDto> RecentTransfers { get; init; } = new();
}

/// <summary>
/// Recent transfer summary.
/// </summary>
public record RecentTransferDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public TransferDirection Direction { get; init; }
    public string Type { get; init; } = null!;
    public string Status { get; init; } = null!;
    public int RecordCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
}

// ============== Enums ==============

public enum ImportDuplicateHandling
{
    Skip,
    Update,
    Error,
    CreateNew
}
