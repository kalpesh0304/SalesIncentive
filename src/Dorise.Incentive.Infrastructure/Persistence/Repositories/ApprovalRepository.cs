using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dorise.Incentive.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Approval entities.
/// "I choo-choo-choose you!" - For approval!
/// </summary>
public class ApprovalRepository : AggregateRepositoryBase<Approval>, IApprovalRepository
{
    public ApprovalRepository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Approval>> GetPendingForApproverAsync(
        Guid approverId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Calculation)
                .ThenInclude(c => c!.Employee)
            .Include(a => a.Calculation)
                .ThenInclude(c => c!.IncentivePlan)
            .Where(a => a.ApproverId == approverId && a.Status == ApprovalStatus.Pending)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Approval>> GetForCalculationAsync(
        Guid calculationId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Approver)
            .Include(a => a.DelegatedTo)
            .Where(a => a.CalculationId == calculationId)
            .OrderBy(a => a.ApprovalLevel)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Approval>> GetExpiredAsync(
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        return await DbSet
            .Include(a => a.Calculation)
                .ThenInclude(c => c!.Employee)
            .Include(a => a.Approver)
            .Where(a =>
                a.Status == ApprovalStatus.Pending &&
                a.ExpiresAt.HasValue &&
                a.ExpiresAt.Value < now)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Approval>> GetForEscalationAsync(
        int slaHours,
        CancellationToken cancellationToken = default)
    {
        var cutoffTime = DateTime.UtcNow.AddHours(-slaHours);
        return await DbSet
            .Include(a => a.Calculation)
                .ThenInclude(c => c!.Employee)
            .Include(a => a.Approver)
            .Where(a =>
                a.Status == ApprovalStatus.Pending &&
                a.CreatedAt < cutoffTime)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<Approval> Items, int TotalCount)> GetPagedForApproverAsync(
        Guid approverId,
        ApprovalStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(a => a.Calculation)
                .ThenInclude(c => c!.Employee)
            .Include(a => a.Calculation)
                .ThenInclude(c => c!.IncentivePlan)
            .Where(a => a.ApproverId == approverId);

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(IReadOnlyList<Approval> Items, int TotalCount)> GetAllPendingPagedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Include(a => a.Calculation)
                .ThenInclude(c => c!.Employee)
            .Include(a => a.Calculation)
                .ThenInclude(c => c!.IncentivePlan)
            .Include(a => a.Approver)
            .Where(a => a.Status == ApprovalStatus.Pending);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<Dictionary<ApprovalStatus, int>> GetStatusCountsForApproverAsync(
        Guid approverId,
        CancellationToken cancellationToken = default)
    {
        var counts = await DbSet
            .Where(a => a.ApproverId == approverId)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return counts.ToDictionary(x => x.Status, x => x.Count);
    }

    public async Task<Approval?> GetCurrentPendingAsync(
        Guid calculationId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Approver)
            .Where(a =>
                a.CalculationId == calculationId &&
                a.Status == ApprovalStatus.Pending)
            .OrderBy(a => a.ApprovalLevel)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> HasPendingApprovalsAsync(
        Guid approverId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(
            a => a.ApproverId == approverId && a.Status == ApprovalStatus.Pending,
            cancellationToken);
    }

    public async Task<IReadOnlyList<Approval>> GetDelegatedToUserAsync(
        Guid delegatedToId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Calculation)
                .ThenInclude(c => c!.Employee)
            .Include(a => a.Approver)
            .Where(a =>
                a.DelegatedToId == delegatedToId &&
                a.Status == ApprovalStatus.Delegated)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Approval>> GetHistoryForApproverAsync(
        Guid approverId,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Calculation)
                .ThenInclude(c => c!.Employee)
            .Include(a => a.Calculation)
                .ThenInclude(c => c!.IncentivePlan)
            .Where(a => a.ApproverId == approverId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<ApprovalStatistics> GetStatisticsAsync(
        DateTime? startDate,
        DateTime? endDate,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet.AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(a => a.CreatedAt <= endDate.Value);
        }

        var approvals = await query.ToListAsync(cancellationToken);

        var totalCount = approvals.Count;
        var pendingCount = approvals.Count(a => a.Status == ApprovalStatus.Pending);
        var approvedCount = approvals.Count(a => a.Status == ApprovalStatus.Approved);
        var rejectedCount = approvals.Count(a => a.Status == ApprovalStatus.Rejected);
        var escalatedCount = approvals.Count(a => a.Status == ApprovalStatus.Escalated);
        var expiredCount = approvals.Count(a => a.Status == ApprovalStatus.Expired);

        // Calculate average approval time for approved items
        var completedApprovals = approvals.Where(a =>
            a.Status == ApprovalStatus.Approved &&
            a.ActionDate.HasValue).ToList();

        var averageTimeHours = completedApprovals.Any()
            ? (decimal)completedApprovals.Average(a =>
                (a.ActionDate!.Value - a.CreatedAt).TotalHours)
            : 0m;

        // Group by level
        var byLevel = approvals
            .GroupBy(a => a.ApprovalLevel)
            .Select(g =>
            {
                var levelApprovals = g.ToList();
                var levelCompleted = levelApprovals.Where(a =>
                    a.Status == ApprovalStatus.Approved &&
                    a.ActionDate.HasValue).ToList();

                var avgTime = levelCompleted.Any()
                    ? (decimal)levelCompleted.Average(a =>
                        (a.ActionDate!.Value - a.CreatedAt).TotalHours)
                    : 0m;

                return new ApprovalLevelStatistics
                {
                    Level = g.Key,
                    Count = levelApprovals.Count,
                    AverageTimeHours = avgTime
                };
            })
            .OrderBy(l => l.Level)
            .ToList();

        return new ApprovalStatistics
        {
            TotalCount = totalCount,
            PendingCount = pendingCount,
            ApprovedCount = approvedCount,
            RejectedCount = rejectedCount,
            EscalatedCount = escalatedCount,
            ExpiredCount = expiredCount,
            AverageApprovalTimeHours = averageTimeHours,
            ByLevel = byLevel
        };
    }

    public async Task<IReadOnlyList<Approval>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet
            .Include(a => a.Calculation)
                .ThenInclude(c => c!.Employee)
            .Include(a => a.Calculation)
                .ThenInclude(c => c!.IncentivePlan)
            .Include(a => a.Approver)
            .Where(a => a.Status == ApprovalStatus.Pending)
            .OrderBy(a => a.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
