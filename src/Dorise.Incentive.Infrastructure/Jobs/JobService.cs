using Dorise.Incentive.Application.Jobs.DTOs;
using Dorise.Incentive.Application.Jobs.Services;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Jobs;

/// <summary>
/// Implementation of background job service.
/// "Go banana!" - Jobs go bananas processing your work!
/// </summary>
public class JobService : IJobService
{
    private readonly IBackgroundJobRepository _jobRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<JobService> _logger;

    public JobService(
        IBackgroundJobRepository jobRepository,
        IUnitOfWork unitOfWork,
        ILogger<JobService> logger)
    {
        _jobRepository = jobRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<BackgroundJobDto> CreateJobAsync(
        CreateJobRequest request,
        CancellationToken cancellationToken = default)
    {
        var job = BackgroundJob.Create(
            request.JobName,
            request.JobType,
            request.Parameters,
            request.Priority,
            request.ScheduledAt,
            request.MaxRetries,
            request.CorrelationId,
            request.ParentJobId);

        await _jobRepository.AddAsync(job, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created job {JobId} of type {JobType} with name {JobName}",
            job.Id, job.JobType, job.JobName);

        return MapToDto(job);
    }

    public async Task<BackgroundJobDto?> GetJobByIdAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
        return job == null ? null : MapToDto(job);
    }

    public async Task<IReadOnlyList<BackgroundJobDto>> GetJobsAsync(
        JobSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var jobs = await _jobRepository.SearchAsync(
            query.JobName,
            query.JobType,
            query.Status,
            query.Priority,
            query.FromDate,
            query.ToDate,
            query.CorrelationId,
            cancellationToken);

        return jobs
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<IReadOnlyList<BackgroundJobSummaryDto>> GetRecentJobsAsync(
        int count = 20,
        CancellationToken cancellationToken = default)
    {
        var jobs = await _jobRepository.GetRecentAsync(count, cancellationToken);
        return jobs.Select(j => new BackgroundJobSummaryDto
        {
            Id = j.Id,
            JobName = j.JobName,
            JobType = j.JobType,
            Status = j.Status,
            ProgressPercentage = j.ProgressPercentage,
            StartedAt = j.StartedAt,
            Duration = j.Duration
        }).ToList();
    }

    public async Task<IReadOnlyList<BackgroundJobDto>> GetJobsByCorrelationIdAsync(
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        var jobs = await _jobRepository.GetByCorrelationIdAsync(correlationId, cancellationToken);
        return jobs.Select(MapToDto).ToList();
    }

    public async Task<BackgroundJobDto> StartJobAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken)
            ?? throw new InvalidOperationException($"Job {jobId} not found");

        job.Start();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Started job {JobId}", jobId);
        return MapToDto(job);
    }

    public async Task<BackgroundJobDto> UpdateJobProgressAsync(
        Guid jobId,
        JobProgressUpdate progress,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken)
            ?? throw new InvalidOperationException($"Job {jobId} not found");

        job.UpdateProgress(progress.ProcessedItems, progress.TotalItems);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return MapToDto(job);
    }

    public async Task<BackgroundJobDto> CompleteJobAsync(
        Guid jobId,
        string? result = null,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken)
            ?? throw new InvalidOperationException($"Job {jobId} not found");

        job.Complete(result);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Completed job {JobId} in {Duration}",
            jobId, job.Duration);

        return MapToDto(job);
    }

    public async Task<BackgroundJobDto> FailJobAsync(
        Guid jobId,
        string errorMessage,
        string? stackTrace = null,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken)
            ?? throw new InvalidOperationException($"Job {jobId} not found");

        job.Fail(errorMessage, stackTrace);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogWarning(
            "Job {JobId} failed: {ErrorMessage}. Retry count: {RetryCount}/{MaxRetries}",
            jobId, errorMessage, job.RetryCount, job.MaxRetries);

        return MapToDto(job);
    }

    public async Task<BackgroundJobDto> CancelJobAsync(
        Guid jobId,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken)
            ?? throw new InvalidOperationException($"Job {jobId} not found");

        job.Cancel(reason);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cancelled job {JobId}: {Reason}", jobId, reason);
        return MapToDto(job);
    }

    public async Task<BackgroundJobDto> RetryJobAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken)
            ?? throw new InvalidOperationException($"Job {jobId} not found");

        job.Retry();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Retrying job {JobId}, attempt {RetryCount}", jobId, job.RetryCount);
        return MapToDto(job);
    }

    public async Task<BackgroundJobDto?> DequeueNextJobAsync(
        JobType? jobType = null,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetNextPendingAsync(jobType, cancellationToken);
        return job == null ? null : MapToDto(job);
    }

    public async Task<JobQueueStatusDto> GetQueueStatusAsync(
        CancellationToken cancellationToken = default)
    {
        var pending = await _jobRepository.GetByStatusAsync(JobStatus.Pending, cancellationToken);
        var running = await _jobRepository.GetByStatusAsync(JobStatus.Running, cancellationToken);
        var scheduled = await _jobRepository.GetByStatusAsync(JobStatus.Scheduled, cancellationToken);

        return new JobQueueStatusDto
        {
            PendingCount = pending.Count,
            RunningCount = running.Count,
            ScheduledCount = scheduled.Count,
            RunningJobs = running.Take(10).Select(j => new BackgroundJobSummaryDto
            {
                Id = j.Id,
                JobName = j.JobName,
                JobType = j.JobType,
                Status = j.Status,
                ProgressPercentage = j.ProgressPercentage,
                StartedAt = j.StartedAt,
                Duration = j.Duration
            }).ToList(),
            NextInQueue = pending.Take(10).Select(j => new BackgroundJobSummaryDto
            {
                Id = j.Id,
                JobName = j.JobName,
                JobType = j.JobType,
                Status = j.Status,
                ProgressPercentage = j.ProgressPercentage,
                StartedAt = j.StartedAt,
                Duration = j.Duration
            }).ToList()
        };
    }

    public async Task<int> CleanupCompletedJobsAsync(
        int olderThanDays = 30,
        CancellationToken cancellationToken = default)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);
        var count = await _jobRepository.DeleteCompletedBeforeAsync(cutoffDate, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cleaned up {Count} completed jobs older than {Days} days", count, olderThanDays);
        return count;
    }

    public async Task<JobStatisticsDto> GetStatisticsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddDays(-30);
        var to = toDate ?? DateTime.UtcNow;

        var jobs = await _jobRepository.GetInDateRangeAsync(from, to, cancellationToken);

        var completed = jobs.Where(j => j.Status == JobStatus.Completed).ToList();
        var failed = jobs.Where(j => j.Status == JobStatus.Failed).ToList();

        var avgDuration = completed
            .Where(j => j.Duration.HasValue)
            .Select(j => j.Duration!.Value.TotalSeconds)
            .DefaultIfEmpty(0)
            .Average();

        var successRate = jobs.Count > 0
            ? (double)completed.Count / jobs.Count(j => j.IsTerminal) * 100
            : 0;

        var byType = jobs
            .GroupBy(j => j.JobType)
            .Select(g => new JobTypeStatDto
            {
                JobType = g.Key,
                Count = g.Count(),
                Completed = g.Count(j => j.Status == JobStatus.Completed),
                Failed = g.Count(j => j.Status == JobStatus.Failed),
                AverageDurationSeconds = g
                    .Where(j => j.Duration.HasValue && j.Status == JobStatus.Completed)
                    .Select(j => j.Duration!.Value.TotalSeconds)
                    .DefaultIfEmpty(0)
                    .Average()
            })
            .ToList();

        var trends = jobs
            .GroupBy(j => j.CreatedAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new JobTrendDto
            {
                Date = g.Key,
                TotalJobs = g.Count(),
                Completed = g.Count(j => j.Status == JobStatus.Completed),
                Failed = g.Count(j => j.Status == JobStatus.Failed)
            })
            .ToList();

        return new JobStatisticsDto
        {
            TotalJobs = jobs.Count,
            PendingJobs = jobs.Count(j => j.Status == JobStatus.Pending),
            RunningJobs = jobs.Count(j => j.Status == JobStatus.Running),
            CompletedJobs = completed.Count,
            FailedJobs = failed.Count,
            CancelledJobs = jobs.Count(j => j.Status == JobStatus.Cancelled),
            ScheduledJobs = jobs.Count(j => j.Status == JobStatus.Scheduled),
            AverageDurationSeconds = avgDuration,
            SuccessRate = successRate,
            ByType = byType,
            Trends = trends
        };
    }

    private static BackgroundJobDto MapToDto(BackgroundJob job)
    {
        return new BackgroundJobDto
        {
            Id = job.Id,
            JobName = job.JobName,
            JobType = job.JobType,
            Status = job.Status,
            Priority = job.Priority,
            Parameters = job.Parameters,
            Result = job.Result,
            ErrorMessage = job.ErrorMessage,
            ScheduledAt = job.ScheduledAt,
            StartedAt = job.StartedAt,
            CompletedAt = job.CompletedAt,
            RetryCount = job.RetryCount,
            MaxRetries = job.MaxRetries,
            CorrelationId = job.CorrelationId,
            ParentJobId = job.ParentJobId,
            ProcessedItems = job.ProcessedItems,
            TotalItems = job.TotalItems,
            ProgressPercentage = job.ProgressPercentage,
            Duration = job.Duration,
            CreatedAt = job.CreatedAt
        };
    }
}

/// <summary>
/// Implementation of job schedule service.
/// "Super Nintendo Chalmers!" - Schedules are super!
/// </summary>
public class JobScheduleService : IJobScheduleService
{
    private readonly IJobScheduleRepository _scheduleRepository;
    private readonly IJobService _jobService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<JobScheduleService> _logger;

    public JobScheduleService(
        IJobScheduleRepository scheduleRepository,
        IJobService jobService,
        IUnitOfWork unitOfWork,
        ILogger<JobScheduleService> logger)
    {
        _scheduleRepository = scheduleRepository;
        _jobService = jobService;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<JobScheduleDto> CreateScheduleAsync(
        CreateJobScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!ValidateCronExpression(request.CronExpression))
        {
            throw new ArgumentException("Invalid cron expression", nameof(request.CronExpression));
        }

        var schedule = JobSchedule.Create(
            request.ScheduleName,
            request.JobName,
            request.JobType,
            request.CronExpression,
            request.Parameters,
            request.Description,
            request.TimeZoneId,
            request.MaxConcurrentRuns);

        await _scheduleRepository.AddAsync(schedule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created schedule {ScheduleId} for job {JobName} with cron {CronExpression}",
            schedule.Id, schedule.JobName, schedule.CronExpression);

        return MapToDto(schedule);
    }

    public async Task<JobScheduleDto?> GetScheduleByIdAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId, cancellationToken);
        return schedule == null ? null : MapToDto(schedule);
    }

    public async Task<JobScheduleDto?> GetScheduleByNameAsync(
        string scheduleName,
        CancellationToken cancellationToken = default)
    {
        var schedule = await _scheduleRepository.GetByNameAsync(scheduleName, cancellationToken);
        return schedule == null ? null : MapToDto(schedule);
    }

    public async Task<IReadOnlyList<JobScheduleDto>> GetAllSchedulesAsync(
        CancellationToken cancellationToken = default)
    {
        var schedules = await _scheduleRepository.GetAllAsync(cancellationToken);
        return schedules.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<JobScheduleSummaryDto>> GetEnabledSchedulesAsync(
        CancellationToken cancellationToken = default)
    {
        var schedules = await _scheduleRepository.GetEnabledAsync(cancellationToken);
        return schedules.Select(s => new JobScheduleSummaryDto
        {
            Id = s.Id,
            ScheduleName = s.ScheduleName,
            JobName = s.JobName,
            IsEnabled = s.IsEnabled,
            NextRunAt = s.NextRunAt
        }).ToList();
    }

    public async Task<JobScheduleDto> UpdateScheduleAsync(
        Guid scheduleId,
        UpdateJobScheduleRequest request,
        CancellationToken cancellationToken = default)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId, cancellationToken)
            ?? throw new InvalidOperationException($"Schedule {scheduleId} not found");

        if (request.CronExpression != null)
        {
            if (!ValidateCronExpression(request.CronExpression))
            {
                throw new ArgumentException("Invalid cron expression", nameof(request.CronExpression));
            }
            schedule.UpdateCronExpression(request.CronExpression);
        }

        if (request.Parameters != null)
        {
            schedule.UpdateParameters(request.Parameters);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Updated schedule {ScheduleId}", scheduleId);
        return MapToDto(schedule);
    }

    public async Task DeleteScheduleAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId, cancellationToken)
            ?? throw new InvalidOperationException($"Schedule {scheduleId} not found");

        await _scheduleRepository.DeleteAsync(schedule, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Deleted schedule {ScheduleId}", scheduleId);
    }

    public async Task EnableScheduleAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId, cancellationToken)
            ?? throw new InvalidOperationException($"Schedule {scheduleId} not found");

        schedule.Enable();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Enabled schedule {ScheduleId}", scheduleId);
    }

    public async Task DisableScheduleAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId, cancellationToken)
            ?? throw new InvalidOperationException($"Schedule {scheduleId} not found");

        schedule.Disable();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Disabled schedule {ScheduleId}", scheduleId);
    }

    public async Task<bool> ToggleScheduleAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId, cancellationToken)
            ?? throw new InvalidOperationException($"Schedule {scheduleId} not found");

        schedule.Toggle();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Toggled schedule {ScheduleId} to {IsEnabled}", scheduleId, schedule.IsEnabled);
        return schedule.IsEnabled;
    }

    public async Task<BackgroundJobDto> TriggerScheduleNowAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId, cancellationToken)
            ?? throw new InvalidOperationException($"Schedule {scheduleId} not found");

        var job = await _jobService.CreateJobAsync(new CreateJobRequest
        {
            JobName = schedule.JobName,
            JobType = schedule.JobType,
            Parameters = schedule.Parameters,
            Priority = JobPriority.Normal
        }, cancellationToken);

        schedule.RecordRun(job.Id, GetNextRunTime(schedule.CronExpression, schedule.TimeZoneId));
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Triggered schedule {ScheduleId} manually, created job {JobId}",
            scheduleId, job.Id);

        return job;
    }

    public async Task<IReadOnlyList<JobScheduleDto>> GetDueSchedulesAsync(
        CancellationToken cancellationToken = default)
    {
        var schedules = await _scheduleRepository.GetDueAsync(DateTime.UtcNow, cancellationToken);
        return schedules.Select(MapToDto).ToList();
    }

    public async Task RecordScheduleRunAsync(
        Guid scheduleId,
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var schedule = await _scheduleRepository.GetByIdAsync(scheduleId, cancellationToken)
            ?? throw new InvalidOperationException($"Schedule {scheduleId} not found");

        var nextRun = GetNextRunTime(schedule.CronExpression, schedule.TimeZoneId);
        schedule.RecordRun(jobId, nextRun);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public DateTime? GetNextRunTime(string cronExpression, string? timeZoneId = null)
    {
        // Simplified cron parsing - in production use NCrontab or similar
        try
        {
            // For now, return next hour as placeholder
            return DateTime.UtcNow.AddHours(1);
        }
        catch
        {
            return null;
        }
    }

    public bool ValidateCronExpression(string cronExpression)
    {
        if (string.IsNullOrWhiteSpace(cronExpression))
            return false;

        var parts = cronExpression.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 5 || parts.Length == 6;
    }

    private static JobScheduleDto MapToDto(JobSchedule schedule)
    {
        return new JobScheduleDto
        {
            Id = schedule.Id,
            ScheduleName = schedule.ScheduleName,
            JobName = schedule.JobName,
            JobType = schedule.JobType,
            Parameters = schedule.Parameters,
            CronExpression = schedule.CronExpression,
            Description = schedule.Description,
            IsEnabled = schedule.IsEnabled,
            LastRunAt = schedule.LastRunAt,
            NextRunAt = schedule.NextRunAt,
            LastJobId = schedule.LastJobId,
            TimeZoneId = schedule.TimeZoneId,
            MaxConcurrentRuns = schedule.MaxConcurrentRuns,
            CurrentRunCount = schedule.CurrentRunCount,
            CreatedAt = schedule.CreatedAt
        };
    }
}

/// <summary>
/// Implementation of batch operation service.
/// </summary>
public class BatchOperationService : IBatchOperationService
{
    private readonly IJobService _jobService;
    private readonly IBackgroundJobRepository _jobRepository;
    private readonly ILogger<BatchOperationService> _logger;

    public BatchOperationService(
        IJobService jobService,
        IBackgroundJobRepository jobRepository,
        ILogger<BatchOperationService> logger)
    {
        _jobService = jobService;
        _jobRepository = jobRepository;
        _logger = logger;
    }

    public async Task<BatchOperationResult> StartCalculationBatchAsync(
        CalculationBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        var parameters = System.Text.Json.JsonSerializer.Serialize(request);

        var job = await _jobService.CreateJobAsync(new CreateJobRequest
        {
            JobName = request.Recalculate
                ? WellKnownJobs.RecalculatePeriod
                : WellKnownJobs.MonthlyCalculationBatch,
            JobType = JobType.CalculationBatch,
            Parameters = parameters,
            Priority = JobPriority.High
        }, cancellationToken);

        _logger.LogInformation(
            "Started calculation batch for period {Period}, job {JobId}",
            request.Period, job.Id);

        return new BatchOperationResult
        {
            JobId = job.Id,
            OperationType = "CalculationBatch",
            Status = job.Status,
            Message = $"Calculation batch started for period {request.Period}"
        };
    }

    public async Task<CalculationBatchResult?> GetCalculationBatchResultAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job == null) return null;

        // Parse result if completed
        if (job.Status == JobStatus.Completed && !string.IsNullOrEmpty(job.Result))
        {
            return System.Text.Json.JsonSerializer.Deserialize<CalculationBatchResult>(job.Result);
        }

        // Return progress status
        var request = !string.IsNullOrEmpty(job.Parameters)
            ? System.Text.Json.JsonSerializer.Deserialize<CalculationBatchRequest>(job.Parameters)
            : null;

        return new CalculationBatchResult
        {
            JobId = jobId,
            Period = request?.Period ?? "Unknown",
            TotalEmployees = (int)(job.TotalItems ?? 0),
            Processed = (int)(job.ProcessedItems ?? 0),
            Succeeded = 0,
            Failed = 0,
            TotalAmount = 0,
            Errors = new List<CalculationBatchError>()
        };
    }

    public async Task<BatchOperationResult> StartDataExportAsync(
        DataExportRequest request,
        CancellationToken cancellationToken = default)
    {
        var parameters = System.Text.Json.JsonSerializer.Serialize(request);

        var job = await _jobService.CreateJobAsync(new CreateJobRequest
        {
            JobName = $"Export.{request.ExportType}",
            JobType = JobType.DataExport,
            Parameters = parameters,
            Priority = JobPriority.Normal
        }, cancellationToken);

        _logger.LogInformation(
            "Started data export {ExportType} in format {Format}, job {JobId}",
            request.ExportType, request.Format, job.Id);

        return new BatchOperationResult
        {
            JobId = job.Id,
            OperationType = "DataExport",
            Status = job.Status,
            Message = $"Data export started for {request.ExportType}"
        };
    }

    public async Task<DataExportResult?> GetDataExportResultAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job == null) return null;

        if (job.Status == JobStatus.Completed && !string.IsNullOrEmpty(job.Result))
        {
            return System.Text.Json.JsonSerializer.Deserialize<DataExportResult>(job.Result);
        }

        var request = !string.IsNullOrEmpty(job.Parameters)
            ? System.Text.Json.JsonSerializer.Deserialize<DataExportRequest>(job.Parameters)
            : null;

        return new DataExportResult
        {
            JobId = jobId,
            ExportType = request?.ExportType ?? "Unknown",
            Format = request?.Format ?? "Unknown",
            RecordCount = 0
        };
    }

    public async Task<BatchOperationResult> StartDataImportAsync(
        DataImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var parameters = System.Text.Json.JsonSerializer.Serialize(request);

        var job = await _jobService.CreateJobAsync(new CreateJobRequest
        {
            JobName = $"Import.{request.ImportType}",
            JobType = JobType.DataImport,
            Parameters = parameters,
            Priority = JobPriority.Normal
        }, cancellationToken);

        _logger.LogInformation(
            "Started data import {ImportType} from {FilePath}, job {JobId}",
            request.ImportType, request.FilePath, job.Id);

        return new BatchOperationResult
        {
            JobId = job.Id,
            OperationType = "DataImport",
            Status = job.Status,
            Message = $"Data import started for {request.ImportType}"
        };
    }

    public async Task<DataImportResult?> GetDataImportResultAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
        if (job == null) return null;

        if (job.Status == JobStatus.Completed && !string.IsNullOrEmpty(job.Result))
        {
            return System.Text.Json.JsonSerializer.Deserialize<DataImportResult>(job.Result);
        }

        var request = !string.IsNullOrEmpty(job.Parameters)
            ? System.Text.Json.JsonSerializer.Deserialize<DataImportRequest>(job.Parameters)
            : null;

        return new DataImportResult
        {
            JobId = jobId,
            ImportType = request?.ImportType ?? "Unknown",
            TotalRecords = (int)(job.TotalItems ?? 0),
            Imported = (int)(job.ProcessedItems ?? 0),
            Skipped = 0,
            Failed = 0,
            Errors = new List<DataImportError>()
        };
    }

    public async Task<BatchOperationResult> StartBatchOperationAsync(
        BatchOperationRequest request,
        CancellationToken cancellationToken = default)
    {
        var parameters = System.Text.Json.JsonSerializer.Serialize(request.Parameters);

        var job = await _jobService.CreateJobAsync(new CreateJobRequest
        {
            JobName = $"Batch.{request.OperationType}",
            JobType = JobType.Custom,
            Parameters = parameters,
            Priority = request.Priority
        }, cancellationToken);

        _logger.LogInformation(
            "Started batch operation {OperationType}, job {JobId}",
            request.OperationType, job.Id);

        return new BatchOperationResult
        {
            JobId = job.Id,
            OperationType = request.OperationType,
            Status = job.Status,
            Message = $"Batch operation {request.OperationType} started"
        };
    }
}
