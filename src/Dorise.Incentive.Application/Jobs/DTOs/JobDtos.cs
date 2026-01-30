using Dorise.Incentive.Domain.Entities;

namespace Dorise.Incentive.Application.Jobs.DTOs;

/// <summary>
/// DTOs for background job management.
/// "My cat's breath smells like cat food!" - Jobs smell like productivity!
/// </summary>

// ============== Background Job DTOs ==============

public record BackgroundJobDto
{
    public Guid Id { get; init; }
    public required string JobName { get; init; }
    public JobType JobType { get; init; }
    public JobStatus Status { get; init; }
    public JobPriority Priority { get; init; }
    public string? Parameters { get; init; }
    public string? Result { get; init; }
    public string? ErrorMessage { get; init; }
    public DateTime? ScheduledAt { get; init; }
    public DateTime? StartedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public int RetryCount { get; init; }
    public int MaxRetries { get; init; }
    public string? CorrelationId { get; init; }
    public Guid? ParentJobId { get; init; }
    public long? ProcessedItems { get; init; }
    public long? TotalItems { get; init; }
    public double? ProgressPercentage { get; init; }
    public TimeSpan? Duration { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record BackgroundJobSummaryDto
{
    public Guid Id { get; init; }
    public required string JobName { get; init; }
    public JobType JobType { get; init; }
    public JobStatus Status { get; init; }
    public double? ProgressPercentage { get; init; }
    public DateTime? StartedAt { get; init; }
    public TimeSpan? Duration { get; init; }
}

public record CreateJobRequest
{
    public required string JobName { get; init; }
    public JobType JobType { get; init; }
    public string? Parameters { get; init; }
    public JobPriority Priority { get; init; } = JobPriority.Normal;
    public DateTime? ScheduledAt { get; init; }
    public int MaxRetries { get; init; } = 3;
    public string? CorrelationId { get; init; }
    public Guid? ParentJobId { get; init; }
}

public record JobSearchQuery
{
    public string? JobName { get; init; }
    public JobType? JobType { get; init; }
    public JobStatus? Status { get; init; }
    public JobPriority? Priority { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public string? CorrelationId { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

public record JobProgressUpdate
{
    public long ProcessedItems { get; init; }
    public long TotalItems { get; init; }
}

// ============== Job Schedule DTOs ==============

public record JobScheduleDto
{
    public Guid Id { get; init; }
    public required string ScheduleName { get; init; }
    public required string JobName { get; init; }
    public JobType JobType { get; init; }
    public string? Parameters { get; init; }
    public required string CronExpression { get; init; }
    public string? Description { get; init; }
    public bool IsEnabled { get; init; }
    public DateTime? LastRunAt { get; init; }
    public DateTime? NextRunAt { get; init; }
    public Guid? LastJobId { get; init; }
    public string? TimeZoneId { get; init; }
    public int MaxConcurrentRuns { get; init; }
    public int CurrentRunCount { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record JobScheduleSummaryDto
{
    public Guid Id { get; init; }
    public required string ScheduleName { get; init; }
    public required string JobName { get; init; }
    public bool IsEnabled { get; init; }
    public DateTime? NextRunAt { get; init; }
}

public record CreateJobScheduleRequest
{
    public required string ScheduleName { get; init; }
    public required string JobName { get; init; }
    public JobType JobType { get; init; }
    public required string CronExpression { get; init; }
    public string? Parameters { get; init; }
    public string? Description { get; init; }
    public string? TimeZoneId { get; init; }
    public int MaxConcurrentRuns { get; init; } = 1;
}

public record UpdateJobScheduleRequest
{
    public string? CronExpression { get; init; }
    public string? Parameters { get; init; }
    public string? Description { get; init; }
    public int? MaxConcurrentRuns { get; init; }
}

// ============== Job Statistics DTOs ==============

public record JobStatisticsDto
{
    public int TotalJobs { get; init; }
    public int PendingJobs { get; init; }
    public int RunningJobs { get; init; }
    public int CompletedJobs { get; init; }
    public int FailedJobs { get; init; }
    public int CancelledJobs { get; init; }
    public int ScheduledJobs { get; init; }
    public double AverageDurationSeconds { get; init; }
    public double SuccessRate { get; init; }
    public IReadOnlyList<JobTypeStatDto> ByType { get; init; } = new List<JobTypeStatDto>();
    public IReadOnlyList<JobTrendDto> Trends { get; init; } = new List<JobTrendDto>();
}

public record JobTypeStatDto
{
    public JobType JobType { get; init; }
    public int Count { get; init; }
    public int Completed { get; init; }
    public int Failed { get; init; }
    public double AverageDurationSeconds { get; init; }
}

public record JobTrendDto
{
    public DateTime Date { get; init; }
    public int TotalJobs { get; init; }
    public int Completed { get; init; }
    public int Failed { get; init; }
}

// ============== Job Queue DTOs ==============

public record JobQueueStatusDto
{
    public int PendingCount { get; init; }
    public int RunningCount { get; init; }
    public int ScheduledCount { get; init; }
    public IReadOnlyList<BackgroundJobSummaryDto> RunningJobs { get; init; } = new List<BackgroundJobSummaryDto>();
    public IReadOnlyList<BackgroundJobSummaryDto> NextInQueue { get; init; } = new List<BackgroundJobSummaryDto>();
}

// ============== Batch Operation DTOs ==============

public record BatchOperationRequest
{
    public required string OperationType { get; init; }
    public required IDictionary<string, object> Parameters { get; init; }
    public JobPriority Priority { get; init; } = JobPriority.Normal;
}

public record BatchOperationResult
{
    public Guid JobId { get; init; }
    public required string OperationType { get; init; }
    public JobStatus Status { get; init; }
    public string? Message { get; init; }
}

// ============== Calculation Batch DTOs ==============

public record CalculationBatchRequest
{
    public required string Period { get; init; }
    public Guid? DepartmentId { get; init; }
    public Guid? PlanId { get; init; }
    public IReadOnlyList<Guid>? EmployeeIds { get; init; }
    public bool Recalculate { get; init; }
}

public record CalculationBatchResult
{
    public Guid JobId { get; init; }
    public required string Period { get; init; }
    public int TotalEmployees { get; init; }
    public int Processed { get; init; }
    public int Succeeded { get; init; }
    public int Failed { get; init; }
    public decimal TotalAmount { get; init; }
    public IReadOnlyList<CalculationBatchError> Errors { get; init; } = new List<CalculationBatchError>();
}

public record CalculationBatchError
{
    public Guid EmployeeId { get; init; }
    public required string EmployeeName { get; init; }
    public required string ErrorMessage { get; init; }
}

// ============== Data Export Batch DTOs ==============

public record DataExportRequest
{
    public required string ExportType { get; init; }
    public required string Format { get; init; } // CSV, Excel, JSON
    public IDictionary<string, object>? Filters { get; init; }
    public IReadOnlyList<string>? Columns { get; init; }
}

public record DataExportResult
{
    public Guid JobId { get; init; }
    public required string ExportType { get; init; }
    public required string Format { get; init; }
    public string? FilePath { get; init; }
    public string? DownloadUrl { get; init; }
    public long? FileSizeBytes { get; init; }
    public int RecordCount { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

// ============== Data Import Batch DTOs ==============

public record DataImportRequest
{
    public required string ImportType { get; init; }
    public required string FilePath { get; init; }
    public bool ValidateOnly { get; init; }
    public bool SkipErrors { get; init; }
    public IDictionary<string, string>? ColumnMappings { get; init; }
}

public record DataImportResult
{
    public Guid JobId { get; init; }
    public required string ImportType { get; init; }
    public int TotalRecords { get; init; }
    public int Imported { get; init; }
    public int Skipped { get; init; }
    public int Failed { get; init; }
    public IReadOnlyList<DataImportError> Errors { get; init; } = new List<DataImportError>();
}

public record DataImportError
{
    public int RowNumber { get; init; }
    public string? ColumnName { get; init; }
    public string? Value { get; init; }
    public required string ErrorMessage { get; init; }
}

// ============== Job Execution Context ==============

public record JobExecutionContext
{
    public Guid JobId { get; init; }
    public required string JobName { get; init; }
    public JobType JobType { get; init; }
    public string? Parameters { get; init; }
    public string? CorrelationId { get; init; }
    public int RetryCount { get; init; }
    public CancellationToken CancellationToken { get; init; }
}
