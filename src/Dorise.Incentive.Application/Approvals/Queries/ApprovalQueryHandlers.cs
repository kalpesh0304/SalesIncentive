using Dorise.Incentive.Application.Approvals.DTOs;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.Approvals.Queries;

/// <summary>
/// Handler for GetPendingApprovalsForUserQuery.
/// "The leprechaun tells me to burn things!" - But queries just return data!
/// </summary>
public class GetPendingApprovalsForUserQueryHandler : IQueryHandler<GetPendingApprovalsForUserQuery, PagedApprovalResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPendingApprovalsForUserQueryHandler> _logger;

    public GetPendingApprovalsForUserQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetPendingApprovalsForUserQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedApprovalResult>> Handle(
        GetPendingApprovalsForUserQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Approvals.GetPagedForApproverAsync(
            request.ApproverId,
            ApprovalStatus.Pending,
            request.Page,
            request.PageSize,
            cancellationToken);

        var summaries = new List<ApprovalSummaryDto>();

        foreach (var approval in items)
        {
            var calculation = await _unitOfWork.Calculations.GetByIdAsync(
                approval.CalculationId,
                cancellationToken);

            if (calculation == null) continue;

            var employee = await _unitOfWork.Employees.GetByIdAsync(
                calculation.EmployeeId,
                cancellationToken);

            var plan = await _unitOfWork.IncentivePlans.GetByIdAsync(
                calculation.IncentivePlanId,
                cancellationToken);

            summaries.Add(new ApprovalSummaryDto
            {
                Id = approval.Id,
                CalculationId = approval.CalculationId,
                EmployeeCode = employee?.EmployeeCode ?? "Unknown",
                EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown",
                PlanName = plan?.Name ?? "Unknown",
                PeriodStart = calculation.CalculationPeriod.StartDate,
                PeriodEnd = calculation.CalculationPeriod.EndDate,
                NetIncentive = calculation.NetIncentive.Amount,
                Currency = calculation.NetIncentive.Currency,
                ApprovalLevel = approval.ApprovalLevel,
                Status = approval.Status,
                CreatedAt = approval.CreatedAt,
                ExpiresAt = approval.ExpiresAt,
                IsOverdue = approval.IsExpired
            });
        }

        return Result<PagedApprovalResult>.Success(new PagedApprovalResult
        {
            Items = summaries,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}

/// <summary>
/// Handler for GetApprovalHistoryQuery.
/// </summary>
public class GetApprovalHistoryQueryHandler : IQueryHandler<GetApprovalHistoryQuery, PagedApprovalResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetApprovalHistoryQueryHandler> _logger;

    public GetApprovalHistoryQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetApprovalHistoryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedApprovalResult>> Handle(
        GetApprovalHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Approvals.GetPagedForApproverAsync(
            request.ApproverId,
            request.Status,
            request.Page,
            request.PageSize,
            cancellationToken);

        var summaries = new List<ApprovalSummaryDto>();

        foreach (var approval in items)
        {
            var calculation = await _unitOfWork.Calculations.GetByIdAsync(
                approval.CalculationId,
                cancellationToken);

            if (calculation == null) continue;

            var employee = await _unitOfWork.Employees.GetByIdAsync(
                calculation.EmployeeId,
                cancellationToken);

            var plan = await _unitOfWork.IncentivePlans.GetByIdAsync(
                calculation.IncentivePlanId,
                cancellationToken);

            summaries.Add(new ApprovalSummaryDto
            {
                Id = approval.Id,
                CalculationId = approval.CalculationId,
                EmployeeCode = employee?.EmployeeCode ?? "Unknown",
                EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown",
                PlanName = plan?.Name ?? "Unknown",
                PeriodStart = calculation.CalculationPeriod.StartDate,
                PeriodEnd = calculation.CalculationPeriod.EndDate,
                NetIncentive = calculation.NetIncentive.Amount,
                Currency = calculation.NetIncentive.Currency,
                ApprovalLevel = approval.ApprovalLevel,
                Status = approval.Status,
                CreatedAt = approval.CreatedAt,
                ExpiresAt = approval.ExpiresAt,
                IsOverdue = approval.IsExpired
            });
        }

        return Result<PagedApprovalResult>.Success(new PagedApprovalResult
        {
            Items = summaries,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}

/// <summary>
/// Handler for GetApprovalDashboardQuery.
/// "Super Nintendo Chalmers!" - Super dashboard data!
/// </summary>
public class GetApprovalDashboardQueryHandler : IQueryHandler<GetApprovalDashboardQuery, ApprovalDashboardDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetApprovalDashboardQueryHandler> _logger;

    public GetApprovalDashboardQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetApprovalDashboardQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ApprovalDashboardDto>> Handle(
        GetApprovalDashboardQuery request,
        CancellationToken cancellationToken)
    {
        // Get counts
        var pendingApprovals = await _unitOfWork.Approvals.GetPendingForApproverAsync(
            request.ApproverId,
            cancellationToken);

        var today = DateTime.UtcNow.Date;
        var allApprovals = await _unitOfWork.Approvals.GetHistoryForApproverAsync(
            request.ApproverId,
            cancellationToken);

        var approvedToday = allApprovals.Count(a =>
            a.Status == ApprovalStatus.Approved &&
            a.ActionDate?.Date == today);

        var rejectedToday = allApprovals.Count(a =>
            a.Status == ApprovalStatus.Rejected &&
            a.ActionDate?.Date == today);

        var overdueCount = pendingApprovals.Count(a => a.IsExpired);

        var delegatedToMe = await _unitOfWork.Approvals.GetDelegatedToUserAsync(
            request.ApproverId,
            cancellationToken);

        // Calculate total pending amount
        var totalPendingAmount = 0m;
        foreach (var approval in pendingApprovals)
        {
            var calculation = await _unitOfWork.Calculations.GetByIdAsync(
                approval.CalculationId,
                cancellationToken);

            if (calculation != null)
            {
                totalPendingAmount += calculation.NetIncentive.Amount;
            }
        }

        // Get recent approvals for display
        var recentApprovals = new List<ApprovalSummaryDto>();
        foreach (var approval in pendingApprovals.Take(5))
        {
            var calculation = await _unitOfWork.Calculations.GetByIdAsync(
                approval.CalculationId,
                cancellationToken);

            if (calculation == null) continue;

            var employee = await _unitOfWork.Employees.GetByIdAsync(
                calculation.EmployeeId,
                cancellationToken);

            var plan = await _unitOfWork.IncentivePlans.GetByIdAsync(
                calculation.IncentivePlanId,
                cancellationToken);

            recentApprovals.Add(new ApprovalSummaryDto
            {
                Id = approval.Id,
                CalculationId = approval.CalculationId,
                EmployeeCode = employee?.EmployeeCode ?? "Unknown",
                EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown",
                PlanName = plan?.Name ?? "Unknown",
                PeriodStart = calculation.CalculationPeriod.StartDate,
                PeriodEnd = calculation.CalculationPeriod.EndDate,
                NetIncentive = calculation.NetIncentive.Amount,
                Currency = calculation.NetIncentive.Currency,
                ApprovalLevel = approval.ApprovalLevel,
                Status = approval.Status,
                CreatedAt = approval.CreatedAt,
                ExpiresAt = approval.ExpiresAt,
                IsOverdue = approval.IsExpired
            });
        }

        return Result<ApprovalDashboardDto>.Success(new ApprovalDashboardDto
        {
            PendingCount = pendingApprovals.Count,
            ApprovedTodayCount = approvedToday,
            RejectedTodayCount = rejectedToday,
            OverdueCount = overdueCount,
            DelegatedToMeCount = delegatedToMe.Count,
            TotalPendingAmount = totalPendingAmount,
            Currency = "INR",
            RecentApprovals = recentApprovals
        });
    }
}

/// <summary>
/// Handler for GetApprovalByIdQuery.
/// </summary>
public class GetApprovalByIdQueryHandler : IQueryHandler<GetApprovalByIdQuery, ApprovalDetailDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetApprovalByIdQueryHandler> _logger;

    public GetApprovalByIdQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetApprovalByIdQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ApprovalDetailDto?>> Handle(
        GetApprovalByIdQuery request,
        CancellationToken cancellationToken)
    {
        var approval = await _unitOfWork.Approvals.GetByIdAsync(request.ApprovalId, cancellationToken);

        if (approval == null)
        {
            return Result<ApprovalDetailDto?>.Success(null);
        }

        var calculation = await _unitOfWork.Calculations.GetByIdAsync(
            approval.CalculationId,
            cancellationToken);

        var approver = await _unitOfWork.Employees.GetByIdAsync(approval.ApproverId, cancellationToken);

        Employee? delegatedTo = null;
        if (approval.DelegatedToId.HasValue)
        {
            delegatedTo = await _unitOfWork.Employees.GetByIdAsync(
                approval.DelegatedToId.Value,
                cancellationToken);
        }

        CalculationSummaryForApproval? calculationSummary = null;
        if (calculation != null)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(
                calculation.EmployeeId,
                cancellationToken);

            var plan = await _unitOfWork.IncentivePlans.GetByIdAsync(
                calculation.IncentivePlanId,
                cancellationToken);

            var department = employee != null
                ? await _unitOfWork.Departments.GetByIdAsync(employee.DepartmentId, cancellationToken)
                : null;

            calculationSummary = new CalculationSummaryForApproval
            {
                Id = calculation.Id,
                EmployeeCode = employee?.EmployeeCode ?? "Unknown",
                EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown",
                Department = department?.Name ?? "Unknown",
                PlanCode = plan?.Code ?? "Unknown",
                PlanName = plan?.Name ?? "Unknown",
                PeriodStart = calculation.CalculationPeriod.StartDate,
                PeriodEnd = calculation.CalculationPeriod.EndDate,
                TargetValue = calculation.TargetValue,
                ActualValue = calculation.ActualValue,
                AchievementPercentage = calculation.AchievementPercentage.Value,
                GrossIncentive = calculation.GrossIncentive.Amount,
                NetIncentive = calculation.NetIncentive.Amount,
                Currency = calculation.NetIncentive.Currency,
                CalculationStatus = calculation.Status
            };
        }

        var levelName = approval.ApprovalLevel switch
        {
            1 => "Manager",
            2 => "Director",
            3 => "VP",
            _ => $"Level {approval.ApprovalLevel}"
        };

        return Result<ApprovalDetailDto?>.Success(new ApprovalDetailDto
        {
            Id = approval.Id,
            CalculationId = approval.CalculationId,
            ApproverId = approval.ApproverId,
            ApproverName = approver != null ? $"{approver.FirstName} {approver.LastName}" : "Unknown",
            ApproverEmail = approver?.Email ?? "Unknown",
            ApprovalLevel = approval.ApprovalLevel,
            ApprovalLevelName = levelName,
            Status = approval.Status,
            StatusDisplay = approval.Status.ToString(),
            ActionDate = approval.ActionDate,
            Comments = approval.Comments,
            CreatedAt = approval.CreatedAt,
            ExpiresAt = approval.ExpiresAt,
            DelegatedToId = approval.DelegatedToId,
            DelegatedToName = delegatedTo != null ? $"{delegatedTo.FirstName} {delegatedTo.LastName}" : null,
            DelegatedAt = approval.DelegatedAt,
            Calculation = calculationSummary!
        });
    }
}

/// <summary>
/// Handler for GetApprovalsForCalculationQuery.
/// </summary>
public class GetApprovalsForCalculationQueryHandler : IQueryHandler<GetApprovalsForCalculationQuery, IReadOnlyList<ApprovalDetailDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetApprovalsForCalculationQueryHandler> _logger;

    public GetApprovalsForCalculationQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetApprovalsForCalculationQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<IReadOnlyList<ApprovalDetailDto>>> Handle(
        GetApprovalsForCalculationQuery request,
        CancellationToken cancellationToken)
    {
        var approvals = await _unitOfWork.Approvals.GetForCalculationAsync(
            request.CalculationId,
            cancellationToken);

        var calculation = await _unitOfWork.Calculations.GetByIdAsync(
            request.CalculationId,
            cancellationToken);

        var results = new List<ApprovalDetailDto>();

        foreach (var approval in approvals)
        {
            var approver = await _unitOfWork.Employees.GetByIdAsync(approval.ApproverId, cancellationToken);

            Employee? delegatedTo = null;
            if (approval.DelegatedToId.HasValue)
            {
                delegatedTo = await _unitOfWork.Employees.GetByIdAsync(
                    approval.DelegatedToId.Value,
                    cancellationToken);
            }

            CalculationSummaryForApproval? calculationSummary = null;
            if (calculation != null)
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(
                    calculation.EmployeeId,
                    cancellationToken);

                var plan = await _unitOfWork.IncentivePlans.GetByIdAsync(
                    calculation.IncentivePlanId,
                    cancellationToken);

                var department = employee != null
                    ? await _unitOfWork.Departments.GetByIdAsync(employee.DepartmentId, cancellationToken)
                    : null;

                calculationSummary = new CalculationSummaryForApproval
                {
                    Id = calculation.Id,
                    EmployeeCode = employee?.EmployeeCode ?? "Unknown",
                    EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown",
                    Department = department?.Name ?? "Unknown",
                    PlanCode = plan?.Code ?? "Unknown",
                    PlanName = plan?.Name ?? "Unknown",
                    PeriodStart = calculation.CalculationPeriod.StartDate,
                    PeriodEnd = calculation.CalculationPeriod.EndDate,
                    TargetValue = calculation.TargetValue,
                    ActualValue = calculation.ActualValue,
                    AchievementPercentage = calculation.AchievementPercentage.Value,
                    GrossIncentive = calculation.GrossIncentive.Amount,
                    NetIncentive = calculation.NetIncentive.Amount,
                    Currency = calculation.NetIncentive.Currency,
                    CalculationStatus = calculation.Status
                };
            }

            var levelName = approval.ApprovalLevel switch
            {
                1 => "Manager",
                2 => "Director",
                3 => "VP",
                _ => $"Level {approval.ApprovalLevel}"
            };

            results.Add(new ApprovalDetailDto
            {
                Id = approval.Id,
                CalculationId = approval.CalculationId,
                ApproverId = approval.ApproverId,
                ApproverName = approver != null ? $"{approver.FirstName} {approver.LastName}" : "Unknown",
                ApproverEmail = approver?.Email ?? "Unknown",
                ApprovalLevel = approval.ApprovalLevel,
                ApprovalLevelName = levelName,
                Status = approval.Status,
                StatusDisplay = approval.Status.ToString(),
                ActionDate = approval.ActionDate,
                Comments = approval.Comments,
                CreatedAt = approval.CreatedAt,
                ExpiresAt = approval.ExpiresAt,
                DelegatedToId = approval.DelegatedToId,
                DelegatedToName = delegatedTo != null ? $"{delegatedTo.FirstName} {delegatedTo.LastName}" : null,
                DelegatedAt = approval.DelegatedAt,
                Calculation = calculationSummary!
            });
        }

        return Result<IReadOnlyList<ApprovalDetailDto>>.Success(results);
    }
}

/// <summary>
/// Handler for GetAllPendingApprovalsQuery.
/// Admin view for all pending approvals.
/// </summary>
public class GetAllPendingApprovalsQueryHandler : IQueryHandler<GetAllPendingApprovalsQuery, PagedApprovalResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAllPendingApprovalsQueryHandler> _logger;

    public GetAllPendingApprovalsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAllPendingApprovalsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedApprovalResult>> Handle(
        GetAllPendingApprovalsQuery request,
        CancellationToken cancellationToken)
    {
        var (items, totalCount) = await _unitOfWork.Approvals.GetAllPendingPagedAsync(
            request.Page,
            request.PageSize,
            cancellationToken);

        var summaries = new List<ApprovalSummaryDto>();

        foreach (var approval in items)
        {
            var calculation = await _unitOfWork.Calculations.GetByIdAsync(
                approval.CalculationId,
                cancellationToken);

            if (calculation == null) continue;

            var employee = await _unitOfWork.Employees.GetByIdAsync(
                calculation.EmployeeId,
                cancellationToken);

            var plan = await _unitOfWork.IncentivePlans.GetByIdAsync(
                calculation.IncentivePlanId,
                cancellationToken);

            summaries.Add(new ApprovalSummaryDto
            {
                Id = approval.Id,
                CalculationId = approval.CalculationId,
                EmployeeCode = employee?.EmployeeCode ?? "Unknown",
                EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown",
                PlanName = plan?.Name ?? "Unknown",
                PeriodStart = calculation.CalculationPeriod.StartDate,
                PeriodEnd = calculation.CalculationPeriod.EndDate,
                NetIncentive = calculation.NetIncentive.Amount,
                Currency = calculation.NetIncentive.Currency,
                ApprovalLevel = approval.ApprovalLevel,
                Status = approval.Status,
                CreatedAt = approval.CreatedAt,
                ExpiresAt = approval.ExpiresAt,
                IsOverdue = approval.IsExpired
            });
        }

        return Result<PagedApprovalResult>.Success(new PagedApprovalResult
        {
            Items = summaries,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}

/// <summary>
/// Handler for GetOverdueApprovalsQuery.
/// </summary>
public class GetOverdueApprovalsQueryHandler : IQueryHandler<GetOverdueApprovalsQuery, PagedApprovalResult>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetOverdueApprovalsQueryHandler> _logger;

    public GetOverdueApprovalsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetOverdueApprovalsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PagedApprovalResult>> Handle(
        GetOverdueApprovalsQuery request,
        CancellationToken cancellationToken)
    {
        var expiredApprovals = await _unitOfWork.Approvals.GetExpiredAsync(cancellationToken);

        var summaries = new List<ApprovalSummaryDto>();

        foreach (var approval in expiredApprovals)
        {
            var calculation = await _unitOfWork.Calculations.GetByIdAsync(
                approval.CalculationId,
                cancellationToken);

            if (calculation == null) continue;

            var employee = await _unitOfWork.Employees.GetByIdAsync(
                calculation.EmployeeId,
                cancellationToken);

            var plan = await _unitOfWork.IncentivePlans.GetByIdAsync(
                calculation.IncentivePlanId,
                cancellationToken);

            summaries.Add(new ApprovalSummaryDto
            {
                Id = approval.Id,
                CalculationId = approval.CalculationId,
                EmployeeCode = employee?.EmployeeCode ?? "Unknown",
                EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown",
                PlanName = plan?.Name ?? "Unknown",
                PeriodStart = calculation.CalculationPeriod.StartDate,
                PeriodEnd = calculation.CalculationPeriod.EndDate,
                NetIncentive = calculation.NetIncentive.Amount,
                Currency = calculation.NetIncentive.Currency,
                ApprovalLevel = approval.ApprovalLevel,
                Status = approval.Status,
                CreatedAt = approval.CreatedAt,
                ExpiresAt = approval.ExpiresAt,
                IsOverdue = true
            });
        }

        // Apply pagination
        var totalCount = summaries.Count;
        var pagedItems = summaries
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        return Result<PagedApprovalResult>.Success(new PagedApprovalResult
        {
            Items = pagedItems,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}

/// <summary>
/// Handler for GetApprovalStatisticsQuery.
/// </summary>
public class GetApprovalStatisticsQueryHandler : IQueryHandler<GetApprovalStatisticsQuery, ApprovalStatisticsDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetApprovalStatisticsQueryHandler> _logger;

    public GetApprovalStatisticsQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetApprovalStatisticsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<ApprovalStatisticsDto>> Handle(
        GetApprovalStatisticsQuery request,
        CancellationToken cancellationToken)
    {
        var statistics = await _unitOfWork.Approvals.GetStatisticsAsync(
            request.StartDate,
            request.EndDate,
            cancellationToken);

        return Result<ApprovalStatisticsDto>.Success(new ApprovalStatisticsDto
        {
            TotalApprovals = statistics.TotalCount,
            PendingCount = statistics.PendingCount,
            ApprovedCount = statistics.ApprovedCount,
            RejectedCount = statistics.RejectedCount,
            EscalatedCount = statistics.EscalatedCount,
            ExpiredCount = statistics.ExpiredCount,
            AverageApprovalTimeHours = statistics.AverageApprovalTimeHours,
            ByLevel = statistics.ByLevel.Select(l => new ApprovalsByLevelDto
            {
                Level = l.Level,
                LevelName = l.Level switch
                {
                    1 => "Manager",
                    2 => "Director",
                    3 => "VP",
                    _ => $"Level {l.Level}"
                },
                Count = l.Count,
                AverageTimeHours = l.AverageTimeHours
            }).ToList()
        });
    }
}
