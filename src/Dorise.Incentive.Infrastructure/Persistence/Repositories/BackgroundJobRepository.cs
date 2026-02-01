using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dorise.Incentive.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for BackgroundJob entity.
/// "My cat's breath smells like cat food!" - Jobs process data like cats process food!
/// </summary>
public class BackgroundJobRepository : RepositoryBase<BackgroundJob>, IBackgroundJobRepository
{
    public BackgroundJobRepository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<BackgroundJob>> SearchAsync(
        string? jobName,
        JobType? jobType,
        JobStatus? status,
        JobPriority? priority,
        DateTime? fromDate,
        DateTime? toDate,
        string? correlationId,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(jobName))
            query = query.Where(j => j.JobName.Contains(jobName));

        if (jobType.HasValue)
            query = query.Where(j => j.JobType == jobType.Value);

        if (status.HasValue)
            query = query.Where(j => j.Status == status.Value);

        if (priority.HasValue)
            query = query.Where(j => j.Priority == priority.Value);

        if (fromDate.HasValue)
            query = query.Where(j => j.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(j => j.CreatedAt <= toDate.Value);

        if (!string.IsNullOrWhiteSpace(correlationId))
            query = query.Where(j => j.CorrelationId == correlationId);

        return await query
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BackgroundJob>> GetRecentAsync(
        int count,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .OrderByDescending(j => j.CreatedAt)
            .Take(count)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BackgroundJob>> GetByCorrelationIdAsync(
        string correlationId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(j => j.CorrelationId == correlationId)
            .OrderBy(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BackgroundJob>> GetByStatusAsync(
        JobStatus status,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(j => j.Status == status)
            .OrderByDescending(j => j.Priority)
            .ThenBy(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<BackgroundJob?> GetNextPendingAsync(
        JobType? jobType,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Where(j => j.Status == JobStatus.Pending ||
                       (j.Status == JobStatus.Scheduled && j.ScheduledAt <= DateTime.UtcNow));

        if (jobType.HasValue)
            query = query.Where(j => j.JobType == jobType.Value);

        return await query
            .OrderByDescending(j => j.Priority)
            .ThenBy(j => j.ScheduledAt ?? j.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<BackgroundJob>> GetInDateRangeAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(j => j.CreatedAt >= fromDate && j.CreatedAt <= toDate)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> DeleteCompletedBeforeAsync(
        DateTime cutoffDate,
        CancellationToken cancellationToken = default)
    {
        var jobsToDelete = await DbSet
            .Where(j => j.IsTerminal && j.CompletedAt < cutoffDate)
            .ToListAsync(cancellationToken);

        DbSet.RemoveRange(jobsToDelete);
        return jobsToDelete.Count;
    }
}

/// <summary>
/// Repository implementation for JobSchedule entity.
/// "I bent my Wookie!" - Schedules bend time to run jobs!
/// </summary>
public class JobScheduleRepository : RepositoryBase<JobSchedule>, IJobScheduleRepository
{
    public JobScheduleRepository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<JobSchedule?> GetByNameAsync(
        string scheduleName,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(s => s.ScheduleName == scheduleName, cancellationToken);
    }

    public async Task<IReadOnlyList<JobSchedule>> GetEnabledAsync(
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.IsEnabled)
            .OrderBy(s => s.NextRunAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<JobSchedule>> GetDueAsync(
        DateTime asOf,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Where(s => s.IsEnabled && s.NextRunAt <= asOf && s.CanRun)
            .OrderBy(s => s.NextRunAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(
        string scheduleName,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .AnyAsync(s => s.ScheduleName == scheduleName, cancellationToken);
    }

    public Task DeleteAsync(JobSchedule schedule, CancellationToken cancellationToken = default)
    {
        Remove(schedule);
        return Task.CompletedTask;
    }
}
