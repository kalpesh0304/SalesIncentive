using Dorise.Incentive.Domain.Entities;

namespace Dorise.Incentive.Domain.Interfaces;

/// <summary>
/// Repository interface for background jobs.
/// "The doctor said I wouldn't have so many nosebleeds if I kept my finger outta there." -
/// Repositories keep jobs organized properly!
/// </summary>
public interface IBackgroundJobRepository : IRepository<BackgroundJob>
{
    Task<IReadOnlyList<BackgroundJob>> SearchAsync(
        string? jobName,
        JobType? jobType,
        JobStatus? status,
        JobPriority? priority,
        DateTime? fromDate,
        DateTime? toDate,
        string? correlationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BackgroundJob>> GetRecentAsync(
        int count,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BackgroundJob>> GetByCorrelationIdAsync(
        string correlationId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BackgroundJob>> GetByStatusAsync(
        JobStatus status,
        CancellationToken cancellationToken = default);

    Task<BackgroundJob?> GetNextPendingAsync(
        JobType? jobType,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<BackgroundJob>> GetInDateRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default);

    Task<int> DeleteCompletedBeforeAsync(
        DateTime cutoffDate,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for job schedules.
/// "I'm learnding!" - Schedules help jobs learn when to run!
/// </summary>
public interface IJobScheduleRepository : IRepository<JobSchedule>
{
    Task<JobSchedule?> GetByNameAsync(
        string scheduleName,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobSchedule>> GetEnabledAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<JobSchedule>> GetDueAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string scheduleName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a job schedule.
    /// </summary>
    Task DeleteAsync(JobSchedule schedule, CancellationToken cancellationToken = default);
}
