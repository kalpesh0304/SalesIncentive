using Dorise.Incentive.Domain.Common;

namespace Dorise.Incentive.Domain.Entities;

/// <summary>
/// Represents a scheduled or executed background job.
/// "I'm a unitard!" - Jobs are singular units of work!
/// </summary>
public class BackgroundJob : AuditableEntity
{
    public string JobName { get; private set; } = null!;
    public JobType JobType { get; private set; }
    public JobStatus Status { get; private set; }
    public JobPriority Priority { get; private set; }
    public string? Parameters { get; private set; } // JSON serialized parameters
    public string? Result { get; private set; } // JSON serialized result
    public string? ErrorMessage { get; private set; }
    public string? StackTrace { get; private set; }
    public DateTime? ScheduledAt { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int RetryCount { get; private set; }
    public int MaxRetries { get; private set; }
    public string? CorrelationId { get; private set; }
    public Guid? ParentJobId { get; private set; }
    public string? TriggerInfo { get; private set; }
    public long? ProcessedItems { get; private set; }
    public long? TotalItems { get; private set; }
    public double? ProgressPercentage { get; private set; }

    private BackgroundJob() { } // EF Core constructor

    public static BackgroundJob Create(
        string jobName,
        JobType jobType,
        string? parameters = null,
        JobPriority priority = JobPriority.Normal,
        DateTime? scheduledAt = null,
        int maxRetries = 3,
        string? correlationId = null,
        Guid? parentJobId = null)
    {
        if (string.IsNullOrWhiteSpace(jobName))
            throw new ArgumentException("Job name is required", nameof(jobName));

        return new BackgroundJob
        {
            Id = Guid.NewGuid(),
            JobName = jobName.Trim(),
            JobType = jobType,
            Status = scheduledAt.HasValue ? JobStatus.Scheduled : JobStatus.Pending,
            Priority = priority,
            Parameters = parameters,
            ScheduledAt = scheduledAt,
            MaxRetries = maxRetries,
            RetryCount = 0,
            CorrelationId = correlationId ?? Guid.NewGuid().ToString(),
            ParentJobId = parentJobId
        };
    }

    public void Start()
    {
        if (Status != JobStatus.Pending && Status != JobStatus.Scheduled && Status != JobStatus.Retrying)
            throw new InvalidOperationException($"Cannot start job in {Status} status");

        Status = JobStatus.Running;
        StartedAt = DateTime.UtcNow;
    }

    public void UpdateProgress(long processedItems, long totalItems)
    {
        ProcessedItems = processedItems;
        TotalItems = totalItems;
        ProgressPercentage = totalItems > 0 ? (double)processedItems / totalItems * 100 : 0;
    }

    public void Complete(string? result = null)
    {
        Status = JobStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        Result = result;
        ProgressPercentage = 100;
    }

    public void Fail(string errorMessage, string? stackTrace = null)
    {
        if (RetryCount < MaxRetries)
        {
            Status = JobStatus.Retrying;
            RetryCount++;
        }
        else
        {
            Status = JobStatus.Failed;
            CompletedAt = DateTime.UtcNow;
        }

        ErrorMessage = errorMessage;
        StackTrace = stackTrace;
    }

    public void Cancel(string? reason = null)
    {
        if (Status == JobStatus.Completed || Status == JobStatus.Failed)
            throw new InvalidOperationException($"Cannot cancel job in {Status} status");

        Status = JobStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
        ErrorMessage = reason ?? "Job was cancelled";
    }

    public void Retry()
    {
        if (Status != JobStatus.Failed && Status != JobStatus.Retrying)
            throw new InvalidOperationException($"Cannot retry job in {Status} status");

        if (RetryCount >= MaxRetries)
            throw new InvalidOperationException("Maximum retry count exceeded");

        Status = JobStatus.Retrying;
        ErrorMessage = null;
        StackTrace = null;
    }

    public void SetTriggerInfo(string triggerInfo)
    {
        TriggerInfo = triggerInfo;
    }

    public TimeSpan? Duration => StartedAt.HasValue && CompletedAt.HasValue
        ? CompletedAt.Value - StartedAt.Value
        : StartedAt.HasValue
            ? DateTime.UtcNow - StartedAt.Value
            : null;

    public bool IsTerminal => Status is JobStatus.Completed or JobStatus.Failed or JobStatus.Cancelled;
}

/// <summary>
/// Represents a recurring job schedule.
/// "The leprechaun tells me to burn things!" - Schedules tell jobs when to run!
/// </summary>
public class JobSchedule : AuditableEntity
{
    public string ScheduleName { get; private set; } = null!;
    public string JobName { get; private set; } = null!;
    public JobType JobType { get; private set; }
    public string? Parameters { get; private set; }
    public string CronExpression { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsEnabled { get; private set; }
    public DateTime? LastRunAt { get; private set; }
    public DateTime? NextRunAt { get; private set; }
    public Guid? LastJobId { get; private set; }
    public string? TimeZoneId { get; private set; }
    public int MaxConcurrentRuns { get; private set; }
    public int CurrentRunCount { get; private set; }

    private JobSchedule() { } // EF Core constructor

    public static JobSchedule Create(
        string scheduleName,
        string jobName,
        JobType jobType,
        string cronExpression,
        string? parameters = null,
        string? description = null,
        string? timeZoneId = null,
        int maxConcurrentRuns = 1)
    {
        if (string.IsNullOrWhiteSpace(scheduleName))
            throw new ArgumentException("Schedule name is required", nameof(scheduleName));

        if (string.IsNullOrWhiteSpace(cronExpression))
            throw new ArgumentException("Cron expression is required", nameof(cronExpression));

        return new JobSchedule
        {
            Id = Guid.NewGuid(),
            ScheduleName = scheduleName.Trim(),
            JobName = jobName.Trim(),
            JobType = jobType,
            CronExpression = cronExpression.Trim(),
            Parameters = parameters,
            Description = description?.Trim(),
            TimeZoneId = timeZoneId ?? "UTC",
            MaxConcurrentRuns = maxConcurrentRuns,
            IsEnabled = true,
            CurrentRunCount = 0
        };
    }

    public void Enable() => IsEnabled = true;
    public void Disable() => IsEnabled = false;
    public void Toggle() => IsEnabled = !IsEnabled;

    public void UpdateCronExpression(string cronExpression)
    {
        CronExpression = cronExpression.Trim();
        // NextRunAt should be recalculated by the scheduler
    }

    public void UpdateParameters(string? parameters)
    {
        Parameters = parameters;
    }

    public void RecordRun(Guid jobId, DateTime? nextRun)
    {
        LastRunAt = DateTime.UtcNow;
        LastJobId = jobId;
        NextRunAt = nextRun;
    }

    public void IncrementRunCount() => CurrentRunCount++;
    public void DecrementRunCount() => CurrentRunCount = Math.Max(0, CurrentRunCount - 1);

    public bool CanRun => IsEnabled && CurrentRunCount < MaxConcurrentRuns;
}

// ============== Enums ==============

public enum JobType
{
    CalculationBatch,
    ReportGeneration,
    DataImport,
    DataExport,
    PayrollExport,
    NotificationBatch,
    DataCleanup,
    AuditArchive,
    CacheRefresh,
    IntegrationSync,
    RecalculatePeriod,
    ApprovalReminder,
    ExpirationCheck,
    SystemMaintenance,
    Custom
}

public enum JobStatus
{
    Pending,
    Scheduled,
    Running,
    Completed,
    Failed,
    Cancelled,
    Retrying
}

public enum JobPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Critical = 3
}

// ============== Well-Known Jobs ==============

public static class WellKnownJobs
{
    // Calculation Jobs
    public const string DailyCalculationBatch = "Job.DailyCalculationBatch";
    public const string MonthlyCalculationBatch = "Job.MonthlyCalculationBatch";
    public const string RecalculatePeriod = "Job.RecalculatePeriod";

    // Report Jobs
    public const string DailyReportGeneration = "Job.DailyReportGeneration";
    public const string WeeklyReportGeneration = "Job.WeeklyReportGeneration";
    public const string MonthlyReportGeneration = "Job.MonthlyReportGeneration";

    // Integration Jobs
    public const string ErpSyncJob = "Job.ErpSync";
    public const string HrSyncJob = "Job.HrSync";
    public const string PayrollExportJob = "Job.PayrollExport";

    // Notification Jobs
    public const string ApprovalReminderJob = "Job.ApprovalReminder";
    public const string NotificationDigestJob = "Job.NotificationDigest";

    // Maintenance Jobs
    public const string AuditArchiveJob = "Job.AuditArchive";
    public const string DataCleanupJob = "Job.DataCleanup";
    public const string CacheRefreshJob = "Job.CacheRefresh";
    public const string SessionCleanupJob = "Job.SessionCleanup";
    public const string ExpiredRoleCleanup = "Job.ExpiredRoleCleanup";
}

// ============== Cron Expressions ==============

public static class CronExpressions
{
    public const string EveryMinute = "* * * * *";
    public const string Every5Minutes = "*/5 * * * *";
    public const string Every15Minutes = "*/15 * * * *";
    public const string Every30Minutes = "*/30 * * * *";
    public const string EveryHour = "0 * * * *";
    public const string Every6Hours = "0 */6 * * *";
    public const string Daily = "0 0 * * *";
    public const string DailyAt6AM = "0 6 * * *";
    public const string DailyAt9AM = "0 9 * * *";
    public const string DailyAtMidnight = "0 0 * * *";
    public const string WeeklyMonday = "0 0 * * 1";
    public const string WeeklySunday = "0 0 * * 0";
    public const string MonthlyFirst = "0 0 1 * *";
    public const string MonthlyLast = "0 0 L * *";
    public const string Quarterly = "0 0 1 1,4,7,10 *";
    public const string Yearly = "0 0 1 1 *";
}
