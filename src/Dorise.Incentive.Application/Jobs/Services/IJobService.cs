using Dorise.Incentive.Application.Jobs.DTOs;
using Dorise.Incentive.Domain.Entities;

namespace Dorise.Incentive.Application.Jobs.Services;

/// <summary>
/// Service interface for background job management.
/// "I bent my Wookie!" - Jobs bend to your will!
/// </summary>
public interface IJobService
{
    // ============== Job CRUD ==============

    Task<BackgroundJobDto> CreateJobAsync(
        CreateJobRequest request,
        CancellationToken cancellationToken = default);

    Task<BackgroundJobDto?> GetJobByIdAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BackgroundJobDto>> GetJobsAsync(
        JobSearchQuery query,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BackgroundJobSummaryDto>> GetRecentJobsAsync(
        int count = 20,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BackgroundJobDto>> GetJobsByCorrelationIdAsync(
        string correlationId,
        CancellationToken cancellationToken = default);

    // ============== Job Execution ==============

    Task<BackgroundJobDto> StartJobAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);

    Task<BackgroundJobDto> UpdateJobProgressAsync(
        Guid jobId,
        JobProgressUpdate progress,
        CancellationToken cancellationToken = default);

    Task<BackgroundJobDto> CompleteJobAsync(
        Guid jobId,
        string? result = null,
        CancellationToken cancellationToken = default);

    Task<BackgroundJobDto> FailJobAsync(
        Guid jobId,
        string errorMessage,
        string? stackTrace = null,
        CancellationToken cancellationToken = default);

    Task<BackgroundJobDto> CancelJobAsync(
        Guid jobId,
        string? reason = null,
        CancellationToken cancellationToken = default);

    Task<BackgroundJobDto> RetryJobAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);

    // ============== Job Queue ==============

    Task<BackgroundJobDto?> DequeueNextJobAsync(
        JobType? jobType = null,
        CancellationToken cancellationToken = default);

    Task<JobQueueStatusDto> GetQueueStatusAsync(
        CancellationToken cancellationToken = default);

    Task<int> CleanupCompletedJobsAsync(
        int olderThanDays = 30,
        CancellationToken cancellationToken = default);

    // ============== Statistics ==============

    Task<JobStatisticsDto> GetStatisticsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for job scheduling.
/// "Me fail English? That's unpossible!" - Schedules never fail!
/// </summary>
public interface IJobScheduleService
{
    // ============== Schedule CRUD ==============

    Task<JobScheduleDto> CreateScheduleAsync(
        CreateJobScheduleRequest request,
        CancellationToken cancellationToken = default);

    Task<JobScheduleDto?> GetScheduleByIdAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default);

    Task<JobScheduleDto?> GetScheduleByNameAsync(
        string scheduleName,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobScheduleDto>> GetAllSchedulesAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobScheduleSummaryDto>> GetEnabledSchedulesAsync(
        CancellationToken cancellationToken = default);

    Task<JobScheduleDto> UpdateScheduleAsync(
        Guid scheduleId,
        UpdateJobScheduleRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteScheduleAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default);

    // ============== Schedule Control ==============

    Task EnableScheduleAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default);

    Task DisableScheduleAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default);

    Task<bool> ToggleScheduleAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default);

    // ============== Schedule Execution ==============

    Task<BackgroundJobDto> TriggerScheduleNowAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobScheduleDto>> GetDueSchedulesAsync(
        CancellationToken cancellationToken = default);

    Task RecordScheduleRunAsync(
        Guid scheduleId,
        Guid jobId,
        CancellationToken cancellationToken = default);

    // ============== Cron Helpers ==============

    DateTime? GetNextRunTime(string cronExpression, string? timeZoneId = null);

    bool ValidateCronExpression(string cronExpression);
}

/// <summary>
/// Service interface for batch operations.
/// "I'm Idaho!" - Batch operations process Idaho-sized loads!
/// </summary>
public interface IBatchOperationService
{
    // ============== Calculation Batches ==============

    Task<BatchOperationResult> StartCalculationBatchAsync(
        CalculationBatchRequest request,
        CancellationToken cancellationToken = default);

    Task<CalculationBatchResult?> GetCalculationBatchResultAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);

    // ============== Data Export ==============

    Task<BatchOperationResult> StartDataExportAsync(
        DataExportRequest request,
        CancellationToken cancellationToken = default);

    Task<DataExportResult?> GetDataExportResultAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);

    // ============== Data Import ==============

    Task<BatchOperationResult> StartDataImportAsync(
        DataImportRequest request,
        CancellationToken cancellationToken = default);

    Task<DataImportResult?> GetDataImportResultAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);

    // ============== Generic Batch ==============

    Task<BatchOperationResult> StartBatchOperationAsync(
        BatchOperationRequest request,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Interface for job execution handlers.
/// </summary>
public interface IJobHandler
{
    JobType JobType { get; }

    Task ExecuteAsync(
        JobExecutionContext context,
        CancellationToken cancellationToken = default);
}
