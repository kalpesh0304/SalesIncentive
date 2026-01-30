namespace Dorise.Incentive.Application.Integrations.DTOs;

/// <summary>
/// DTO for sales data imported from ERP system.
/// "Hi, Super Nintendo Chalmers!" - Hi, Super Sales Data!
/// </summary>
public record SalesDataImportDto
{
    public required string EmployeeCode { get; init; }
    public required string Period { get; init; }
    public required decimal SalesAmount { get; init; }
    public required decimal TargetAmount { get; init; }
    public string? ProductCategory { get; init; }
    public string? Region { get; init; }
    public string? Channel { get; init; }
    public DateTime TransactionDate { get; init; }
    public string? ExternalReference { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Result of sales data import operation.
/// </summary>
public record SalesDataImportResultDto
{
    public int TotalRecords { get; init; }
    public int SuccessfulRecords { get; init; }
    public int FailedRecords { get; init; }
    public int SkippedRecords { get; init; }
    public List<ImportErrorDto> Errors { get; init; } = new();
    public DateTime ImportedAt { get; init; }
    public string? BatchId { get; init; }
}

/// <summary>
/// Import error details.
/// </summary>
public record ImportErrorDto
{
    public int RowNumber { get; init; }
    public string? EmployeeCode { get; init; }
    public required string ErrorMessage { get; init; }
    public string? FieldName { get; init; }
}

/// <summary>
/// Employee data from HR system.
/// </summary>
public record HrEmployeeDto
{
    public required string EmployeeCode { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public string? Department { get; init; }
    public string? Designation { get; init; }
    public string? ManagerCode { get; init; }
    public DateTime DateOfJoining { get; init; }
    public DateTime? DateOfLeaving { get; init; }
    public string? Status { get; init; }
    public string? AzureAdObjectId { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Result of HR sync operation.
/// </summary>
public record HrSyncResultDto
{
    public int TotalRecords { get; init; }
    public int NewEmployees { get; init; }
    public int UpdatedEmployees { get; init; }
    public int DeactivatedEmployees { get; init; }
    public int FailedRecords { get; init; }
    public List<SyncErrorDto> Errors { get; init; } = new();
    public DateTime SyncedAt { get; init; }
    public string? SyncId { get; init; }
}

/// <summary>
/// Sync error details.
/// </summary>
public record SyncErrorDto
{
    public required string EmployeeCode { get; init; }
    public required string ErrorMessage { get; init; }
    public string? Operation { get; init; }
}

/// <summary>
/// Payout export data for payroll system.
/// </summary>
public record PayrollPayoutDto
{
    public required string EmployeeCode { get; init; }
    public required string EmployeeName { get; init; }
    public required string Period { get; init; }
    public required decimal GrossAmount { get; init; }
    public required decimal NetAmount { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal DeductionAmount { get; init; }
    public required string PlanCode { get; init; }
    public required string PlanName { get; init; }
    public Guid CalculationId { get; init; }
    public DateTime ApprovedAt { get; init; }
    public string? ApprovedBy { get; init; }
    public string? CostCenter { get; init; }
    public string? Department { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

/// <summary>
/// Result of payroll export operation.
/// </summary>
public record PayrollExportResultDto
{
    public int TotalRecords { get; init; }
    public int ExportedRecords { get; init; }
    public int FailedRecords { get; init; }
    public decimal TotalAmount { get; init; }
    public List<ExportErrorDto> Errors { get; init; } = new();
    public DateTime ExportedAt { get; init; }
    public string? BatchId { get; init; }
    public string? ExportFilePath { get; init; }
}

/// <summary>
/// Export error details.
/// </summary>
public record ExportErrorDto
{
    public Guid CalculationId { get; init; }
    public string? EmployeeCode { get; init; }
    public required string ErrorMessage { get; init; }
}

/// <summary>
/// Integration sync status.
/// </summary>
public record IntegrationSyncStatusDto
{
    public required string IntegrationType { get; init; }
    public DateTime? LastSyncAt { get; init; }
    public string? LastSyncStatus { get; init; }
    public int? LastSyncRecordCount { get; init; }
    public string? LastSyncError { get; init; }
    public DateTime? NextScheduledSync { get; init; }
    public bool IsEnabled { get; init; }
}

/// <summary>
/// Integration configuration.
/// </summary>
public record IntegrationConfigDto
{
    public required string IntegrationType { get; init; }
    public required string Endpoint { get; init; }
    public bool IsEnabled { get; init; }
    public int SyncIntervalMinutes { get; init; }
    public int RetryCount { get; init; }
    public int TimeoutSeconds { get; init; }
    public Dictionary<string, string>? Headers { get; init; }
    public Dictionary<string, object>? Settings { get; init; }
}
