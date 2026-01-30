using Dorise.Incentive.Domain.Common;

namespace Dorise.Incentive.Domain.Entities;

/// <summary>
/// Represents a data import operation.
/// "I'm learnding!" - Importing data teaches the system new things!
/// </summary>
public class DataImport : AuditableEntity
{
    public string ImportName { get; private set; } = null!;
    public ImportType ImportType { get; private set; }
    public ImportStatus Status { get; private set; }
    public string FileName { get; private set; } = null!;
    public string? OriginalFileName { get; private set; }
    public long FileSize { get; private set; }
    public string FileFormat { get; private set; } = null!;
    public string? MappingConfiguration { get; private set; } // JSON mapping config
    public string? ValidationRules { get; private set; } // JSON validation rules
    public int TotalRecords { get; private set; }
    public int ProcessedRecords { get; private set; }
    public int SuccessfulRecords { get; private set; }
    public int FailedRecords { get; private set; }
    public int SkippedRecords { get; private set; }
    public string? ErrorLog { get; private set; } // JSON array of errors
    public string? WarningLog { get; private set; } // JSON array of warnings
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid? BackgroundJobId { get; private set; }
    public string? Options { get; private set; } // JSON import options
    public bool DryRun { get; private set; }
    public string? DryRunResults { get; private set; } // JSON preview results

    private DataImport() { } // EF Core constructor

    public static DataImport Create(
        string importName,
        ImportType importType,
        string fileName,
        string? originalFileName,
        long fileSize,
        string fileFormat,
        string? mappingConfiguration = null,
        string? validationRules = null,
        string? options = null,
        bool dryRun = false)
    {
        if (string.IsNullOrWhiteSpace(importName))
            throw new ArgumentException("Import name is required", nameof(importName));

        if (string.IsNullOrWhiteSpace(fileName))
            throw new ArgumentException("File name is required", nameof(fileName));

        return new DataImport
        {
            Id = Guid.NewGuid(),
            ImportName = importName.Trim(),
            ImportType = importType,
            Status = ImportStatus.Pending,
            FileName = fileName,
            OriginalFileName = originalFileName,
            FileSize = fileSize,
            FileFormat = fileFormat.ToLowerInvariant(),
            MappingConfiguration = mappingConfiguration,
            ValidationRules = validationRules,
            Options = options,
            DryRun = dryRun,
            TotalRecords = 0,
            ProcessedRecords = 0,
            SuccessfulRecords = 0,
            FailedRecords = 0,
            SkippedRecords = 0
        };
    }

    public void Start(Guid? backgroundJobId = null)
    {
        if (Status != ImportStatus.Pending && Status != ImportStatus.Validated)
            throw new InvalidOperationException($"Cannot start import in {Status} status");

        Status = ImportStatus.Processing;
        StartedAt = DateTime.UtcNow;
        BackgroundJobId = backgroundJobId;
    }

    public void SetTotalRecords(int totalRecords)
    {
        TotalRecords = totalRecords;
    }

    public void UpdateProgress(int processed, int successful, int failed, int skipped)
    {
        ProcessedRecords = processed;
        SuccessfulRecords = successful;
        FailedRecords = failed;
        SkippedRecords = skipped;
    }

    public void MarkAsValidating()
    {
        Status = ImportStatus.Validating;
    }

    public void MarkAsValidated(string? dryRunResults = null)
    {
        Status = ImportStatus.Validated;
        DryRunResults = dryRunResults;
    }

    public void Complete()
    {
        Status = FailedRecords > 0 ? ImportStatus.CompletedWithErrors : ImportStatus.Completed;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        Status = ImportStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        AppendError(errorMessage);
    }

    public void Cancel(string? reason = null)
    {
        if (Status == ImportStatus.Completed || Status == ImportStatus.Failed)
            throw new InvalidOperationException($"Cannot cancel import in {Status} status");

        Status = ImportStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(reason))
            AppendError($"Cancelled: {reason}");
    }

    public void AppendError(string error)
    {
        var errors = string.IsNullOrEmpty(ErrorLog)
            ? new List<string>()
            : System.Text.Json.JsonSerializer.Deserialize<List<string>>(ErrorLog) ?? new List<string>();
        errors.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {error}");
        ErrorLog = System.Text.Json.JsonSerializer.Serialize(errors);
    }

    public void AppendWarning(string warning)
    {
        var warnings = string.IsNullOrEmpty(WarningLog)
            ? new List<string>()
            : System.Text.Json.JsonSerializer.Deserialize<List<string>>(WarningLog) ?? new List<string>();
        warnings.Add($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {warning}");
        WarningLog = System.Text.Json.JsonSerializer.Serialize(warnings);
    }

    public double ProgressPercentage => TotalRecords > 0 ? (double)ProcessedRecords / TotalRecords * 100 : 0;
    public TimeSpan? Duration => StartedAt.HasValue
        ? (CompletedAt ?? DateTime.UtcNow) - StartedAt.Value
        : null;
    public bool IsTerminal => Status is ImportStatus.Completed or ImportStatus.CompletedWithErrors
        or ImportStatus.Failed or ImportStatus.Cancelled;
}

/// <summary>
/// Represents a data export operation.
/// "Me fail English? That's unpossible!" - Exports make data sharing unpossibly easy!
/// </summary>
public class DataExport : AuditableEntity
{
    public string ExportName { get; private set; } = null!;
    public ExportType ExportType { get; private set; }
    public ExportStatus Status { get; private set; }
    public string FileFormat { get; private set; } = null!;
    public string? FileName { get; private set; }
    public long? FileSize { get; private set; }
    public string? FilePath { get; private set; }
    public string? DownloadUrl { get; private set; }
    public DateTime? DownloadUrlExpiry { get; private set; }
    public string? FilterCriteria { get; private set; } // JSON filter criteria
    public string? ColumnConfiguration { get; private set; } // JSON column config
    public string? Options { get; private set; } // JSON export options
    public int TotalRecords { get; private set; }
    public int ExportedRecords { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public Guid? BackgroundJobId { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int DownloadCount { get; private set; }
    public DateTime? LastDownloadedAt { get; private set; }

    private DataExport() { } // EF Core constructor

    public static DataExport Create(
        string exportName,
        ExportType exportType,
        string fileFormat,
        string? filterCriteria = null,
        string? columnConfiguration = null,
        string? options = null)
    {
        if (string.IsNullOrWhiteSpace(exportName))
            throw new ArgumentException("Export name is required", nameof(exportName));

        return new DataExport
        {
            Id = Guid.NewGuid(),
            ExportName = exportName.Trim(),
            ExportType = exportType,
            Status = ExportStatus.Pending,
            FileFormat = fileFormat.ToLowerInvariant(),
            FilterCriteria = filterCriteria,
            ColumnConfiguration = columnConfiguration,
            Options = options,
            TotalRecords = 0,
            ExportedRecords = 0,
            DownloadCount = 0
        };
    }

    public void Start(Guid? backgroundJobId = null)
    {
        if (Status != ExportStatus.Pending)
            throw new InvalidOperationException($"Cannot start export in {Status} status");

        Status = ExportStatus.Processing;
        StartedAt = DateTime.UtcNow;
        BackgroundJobId = backgroundJobId;
    }

    public void SetTotalRecords(int totalRecords)
    {
        TotalRecords = totalRecords;
    }

    public void UpdateProgress(int exportedRecords)
    {
        ExportedRecords = exportedRecords;
    }

    public void Complete(string fileName, string filePath, long fileSize, string? downloadUrl = null, DateTime? urlExpiry = null)
    {
        Status = ExportStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        FileName = fileName;
        FilePath = filePath;
        FileSize = fileSize;
        DownloadUrl = downloadUrl;
        DownloadUrlExpiry = urlExpiry;
    }

    public void Fail(string errorMessage)
    {
        Status = ExportStatus.Failed;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = errorMessage;
    }

    public void Cancel(string? reason = null)
    {
        if (Status == ExportStatus.Completed || Status == ExportStatus.Failed)
            throw new InvalidOperationException($"Cannot cancel export in {Status} status");

        Status = ExportStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = reason ?? "Export was cancelled";
    }

    public void RecordDownload()
    {
        DownloadCount++;
        LastDownloadedAt = DateTime.UtcNow;
    }

    public void RefreshDownloadUrl(string downloadUrl, DateTime expiry)
    {
        DownloadUrl = downloadUrl;
        DownloadUrlExpiry = expiry;
    }

    public void MarkAsExpired()
    {
        Status = ExportStatus.Expired;
        DownloadUrl = null;
    }

    public double ProgressPercentage => TotalRecords > 0 ? (double)ExportedRecords / TotalRecords * 100 : 0;
    public TimeSpan? Duration => StartedAt.HasValue
        ? (CompletedAt ?? DateTime.UtcNow) - StartedAt.Value
        : null;
    public bool IsTerminal => Status is ExportStatus.Completed or ExportStatus.Failed
        or ExportStatus.Cancelled or ExportStatus.Expired;
    public bool IsDownloadAvailable => Status == ExportStatus.Completed
        && !string.IsNullOrEmpty(DownloadUrl)
        && (!DownloadUrlExpiry.HasValue || DownloadUrlExpiry > DateTime.UtcNow);
}

/// <summary>
/// Represents a field mapping for data import.
/// "The doctor said I wouldn't have so many nosebleeds if I kept my finger outta there."
/// - Field mappings keep data in the right places!
/// </summary>
public class ImportFieldMapping : BaseEntity
{
    public Guid DataImportId { get; private set; }
    public string SourceField { get; private set; } = null!;
    public string TargetField { get; private set; } = null!;
    public string? DataType { get; private set; }
    public string? DefaultValue { get; private set; }
    public bool IsRequired { get; private set; }
    public string? TransformExpression { get; private set; }
    public string? ValidationRegex { get; private set; }
    public int Order { get; private set; }

    private ImportFieldMapping() { } // EF Core constructor

    public static ImportFieldMapping Create(
        Guid dataImportId,
        string sourceField,
        string targetField,
        string? dataType = null,
        string? defaultValue = null,
        bool isRequired = false,
        string? transformExpression = null,
        string? validationRegex = null,
        int order = 0)
    {
        return new ImportFieldMapping
        {
            Id = Guid.NewGuid(),
            DataImportId = dataImportId,
            SourceField = sourceField,
            TargetField = targetField,
            DataType = dataType,
            DefaultValue = defaultValue,
            IsRequired = isRequired,
            TransformExpression = transformExpression,
            ValidationRegex = validationRegex,
            Order = order
        };
    }
}

/// <summary>
/// Represents a template for data import/export operations.
/// "When I grow up, I want to be a principal or a caterpillar." - Templates grow into full imports!
/// </summary>
public class DataTransferTemplate : AuditableEntity
{
    public string TemplateName { get; private set; } = null!;
    public string? Description { get; private set; }
    public TransferDirection Direction { get; private set; }
    public string EntityType { get; private set; } = null!;
    public string FileFormat { get; private set; } = null!;
    public string? MappingConfiguration { get; private set; }
    public string? ValidationRules { get; private set; }
    public string? ColumnConfiguration { get; private set; }
    public string? DefaultOptions { get; private set; }
    public bool IsDefault { get; private set; }
    public bool IsActive { get; private set; }

    private DataTransferTemplate() { } // EF Core constructor

    public static DataTransferTemplate Create(
        string templateName,
        TransferDirection direction,
        string entityType,
        string fileFormat,
        string? description = null,
        string? mappingConfiguration = null,
        string? validationRules = null,
        string? columnConfiguration = null,
        string? defaultOptions = null,
        bool isDefault = false)
    {
        return new DataTransferTemplate
        {
            Id = Guid.NewGuid(),
            TemplateName = templateName.Trim(),
            Description = description?.Trim(),
            Direction = direction,
            EntityType = entityType,
            FileFormat = fileFormat.ToLowerInvariant(),
            MappingConfiguration = mappingConfiguration,
            ValidationRules = validationRules,
            ColumnConfiguration = columnConfiguration,
            DefaultOptions = defaultOptions,
            IsDefault = isDefault,
            IsActive = true
        };
    }

    public void Update(
        string templateName,
        string? description,
        string? mappingConfiguration,
        string? validationRules,
        string? columnConfiguration,
        string? defaultOptions)
    {
        TemplateName = templateName.Trim();
        Description = description?.Trim();
        MappingConfiguration = mappingConfiguration;
        ValidationRules = validationRules;
        ColumnConfiguration = columnConfiguration;
        DefaultOptions = defaultOptions;
    }

    public void SetAsDefault() => IsDefault = true;
    public void ClearDefault() => IsDefault = false;
    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;
}

// ============== Enums ==============

public enum ImportType
{
    Employees,
    Departments,
    IncentivePlans,
    SalesData,
    Targets,
    Adjustments,
    Assignments,
    Calculations,
    Custom
}

public enum ImportStatus
{
    Pending,
    Validating,
    Validated,
    Processing,
    Completed,
    CompletedWithErrors,
    Failed,
    Cancelled
}

public enum ExportType
{
    Employees,
    Departments,
    IncentivePlans,
    Calculations,
    Approvals,
    AuditLogs,
    Reports,
    Payroll,
    Custom
}

public enum ExportStatus
{
    Pending,
    Processing,
    Completed,
    Failed,
    Cancelled,
    Expired
}

public enum TransferDirection
{
    Import,
    Export
}

// ============== Supported File Formats ==============

public static class SupportedFileFormats
{
    // Import formats
    public const string Csv = "csv";
    public const string Xlsx = "xlsx";
    public const string Xls = "xls";
    public const string Json = "json";
    public const string Xml = "xml";

    // Export formats
    public const string Pdf = "pdf";

    public static readonly string[] ImportFormats = { Csv, Xlsx, Xls, Json, Xml };
    public static readonly string[] ExportFormats = { Csv, Xlsx, Json, Xml, Pdf };

    public static bool IsValidImportFormat(string format)
        => ImportFormats.Contains(format.ToLowerInvariant());

    public static bool IsValidExportFormat(string format)
        => ExportFormats.Contains(format.ToLowerInvariant());

    public static string GetContentType(string format) => format.ToLowerInvariant() switch
    {
        Csv => "text/csv",
        Xlsx => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        Xls => "application/vnd.ms-excel",
        Json => "application/json",
        Xml => "application/xml",
        Pdf => "application/pdf",
        _ => "application/octet-stream"
    };

    public static string GetFileExtension(string format) => $".{format.ToLowerInvariant()}";
}
