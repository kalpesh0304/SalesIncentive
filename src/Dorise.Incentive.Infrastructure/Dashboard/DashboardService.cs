using Dorise.Incentive.Application.Dashboard.DTOs;
using Dorise.Incentive.Application.Dashboard.Services;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Dashboard;

/// <summary>
/// Dashboard service implementation for analytics and KPIs.
/// "My cat's breath smells like cat food." - My dashboard smells like fresh data!
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly ICalculationRepository _calculationRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IApprovalRepository _approvalRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IIncentivePlanRepository _planRepository;
    private readonly ILogger<DashboardService> _logger;

    private const double DefaultSlaHours = 72;

    public DashboardService(
        ICalculationRepository calculationRepository,
        IEmployeeRepository employeeRepository,
        IApprovalRepository approvalRepository,
        IDepartmentRepository departmentRepository,
        IIncentivePlanRepository planRepository,
        ILogger<DashboardService> logger)
    {
        _calculationRepository = calculationRepository;
        _employeeRepository = employeeRepository;
        _approvalRepository = approvalRepository;
        _departmentRepository = departmentRepository;
        _planRepository = planRepository;
        _logger = logger;
    }

    public async Task<DashboardDto> GetDashboardAsync(
        DashboardFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var period = filter.Period ?? GetCurrentPeriod();

        _logger.LogInformation("Generating dashboard for period {Period}", period);

        var summaryTask = GetSummaryKpisAsync(period, filter.DepartmentId, cancellationToken);
        var calculationMetricsTask = GetCalculationMetricsAsync(period, filter.DepartmentId, cancellationToken);
        var approvalMetricsTask = GetApprovalMetricsAsync(period, cancellationToken);
        var paymentMetricsTask = GetPaymentMetricsAsync(period, cancellationToken);
        var performanceMetricsTask = GetPerformanceMetricsAsync(period, filter.DepartmentId, cancellationToken);

        await Task.WhenAll(
            summaryTask,
            calculationMetricsTask,
            approvalMetricsTask,
            paymentMetricsTask,
            performanceMetricsTask);

        var dashboard = new DashboardDto
        {
            Period = period,
            Summary = await summaryTask,
            CalculationMetrics = await calculationMetricsTask,
            ApprovalMetrics = await approvalMetricsTask,
            PaymentMetrics = await paymentMetricsTask,
            PerformanceMetrics = await performanceMetricsTask
        };

        if (filter.IncludeTrends)
        {
            dashboard = dashboard with
            {
                IncentiveTrends = await GetIncentiveTrendsAsync(12, filter.DepartmentId, cancellationToken)
            };
        }

        if (filter.IncludeTopPerformers)
        {
            dashboard = dashboard with
            {
                TopPerformers = await GetTopPerformersAsync(period, filter.TopPerformersCount, filter.DepartmentId, cancellationToken)
            };
        }

        if (filter.IncludeDepartmentSummaries)
        {
            dashboard = dashboard with
            {
                DepartmentSummaries = await GetDepartmentSummariesAsync(period, cancellationToken)
            };
        }

        if (filter.IncludeAlerts)
        {
            dashboard = dashboard with
            {
                Alerts = await GetAlertsAsync(cancellationToken)
            };
        }

        return dashboard;
    }

    public async Task<QuickStatsDto> GetQuickStatsAsync(
        string period,
        CancellationToken cancellationToken = default)
    {
        var calculations = await _calculationRepository.GetByPeriodAsync(period, cancellationToken);
        var previousPeriod = GetPreviousPeriod(period);
        var previousCalculations = await _calculationRepository.GetByPeriodAsync(previousPeriod, cancellationToken);

        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        var activeEmployees = employees.Where(e => e.Status == EmployeeStatus.Active).ToList();

        var pendingApprovals = await _approvalRepository.GetPendingAsync(cancellationToken);
        var overdueApprovals = pendingApprovals
            .Where(a => (DateTime.UtcNow - a.CreatedAt).TotalHours > DefaultSlaHours)
            .Count();

        var currentTotal = calculations.Sum(c => c.NetIncentive.Amount);
        var previousTotal = previousCalculations.Sum(c => c.NetIncentive.Amount);
        var currentAvgAchievement = calculations.Any()
            ? calculations.Average(c => c.AchievementPercentage.Value)
            : 0;
        var previousAvgAchievement = previousCalculations.Any()
            ? previousCalculations.Average(c => c.AchievementPercentage.Value)
            : 0;

        return new QuickStatsDto
        {
            Period = period,
            TotalIncentives = currentTotal,
            TotalIncentivesChange = currentTotal - previousTotal,
            TotalEmployees = activeEmployees.Count,
            TotalEmployeesChange = 0, // Would need historical employee data
            AverageAchievement = currentAvgAchievement,
            AverageAchievementChange = currentAvgAchievement - previousAvgAchievement,
            PendingApprovals = pendingApprovals.Count,
            OverdueApprovals = overdueApprovals,
            Currency = calculations.FirstOrDefault()?.NetIncentive.Currency ?? "USD"
        };
    }

    public async Task<SummaryKpisDto> GetSummaryKpisAsync(
        string period,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var calculations = await _calculationRepository.GetByPeriodAsync(period, cancellationToken);
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);

        if (departmentId.HasValue)
        {
            employees = employees.Where(e => e.DepartmentId == departmentId.Value).ToList();
            var employeeIds = employees.Select(e => e.Id).ToHashSet();
            calculations = calculations.Where(c => employeeIds.Contains(c.EmployeeId)).ToList();
        }

        var activeEmployees = employees.Where(e => e.Status == EmployeeStatus.Active).ToList();
        var paidCalculations = calculations.Where(c => c.Status == CalculationStatus.Paid).ToList();
        var pendingCalculations = calculations.Where(c =>
            c.Status != CalculationStatus.Paid && c.Status != CalculationStatus.Rejected).ToList();

        var amounts = calculations.Select(c => c.NetIncentive.Amount).OrderBy(a => a).ToList();
        var median = amounts.Any()
            ? amounts[amounts.Count / 2]
            : 0m;

        return new SummaryKpisDto
        {
            TotalEmployees = employees.Count,
            ActiveEmployees = activeEmployees.Count,
            EligibleEmployees = calculations.Select(c => c.EmployeeId).Distinct().Count(),
            TotalCalculations = calculations.Count,
            TotalIncentiveAmount = calculations.Sum(c => c.NetIncentive.Amount),
            AverageIncentive = calculations.Any() ? calculations.Average(c => c.NetIncentive.Amount) : 0,
            MedianIncentive = median,
            TotalPaidAmount = paidCalculations.Sum(c => c.NetIncentive.Amount),
            TotalPendingAmount = pendingCalculations.Sum(c => c.NetIncentive.Amount),
            AverageAchievementPercentage = calculations.Any()
                ? calculations.Average(c => c.AchievementPercentage.Value)
                : 0,
            Currency = calculations.FirstOrDefault()?.NetIncentive.Currency ?? "USD"
        };
    }

    public async Task<CalculationMetricsDto> GetCalculationMetricsAsync(
        string period,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var calculations = await _calculationRepository.GetByPeriodAsync(period, cancellationToken);

        if (departmentId.HasValue)
        {
            var employees = await _employeeRepository.GetAllAsync(cancellationToken);
            var employeeIds = employees
                .Where(e => e.DepartmentId == departmentId.Value)
                .Select(e => e.Id)
                .ToHashSet();
            calculations = calculations.Where(c => employeeIds.Contains(c.EmployeeId)).ToList();
        }

        var byStatus = calculations
            .GroupBy(c => c.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        var plans = await _planRepository.GetAllAsync(cancellationToken);
        var planNames = plans.ToDictionary(p => p.Id, p => p.Name);

        var byPlan = calculations
            .GroupBy(c => c.IncentivePlanId)
            .ToDictionary(
                g => planNames.GetValueOrDefault(g.Key, "Unknown"),
                g => g.Count());

        var completedCount = calculations.Count(c =>
            c.Status == CalculationStatus.Approved ||
            c.Status == CalculationStatus.Paid);

        var failedCount = calculations.Count(c => c.Status == CalculationStatus.Rejected);

        return new CalculationMetricsDto
        {
            TotalCalculations = calculations.Count,
            PendingCalculations = calculations.Count(c => c.Status == CalculationStatus.PendingApproval),
            CompletedCalculations = completedCount,
            FailedCalculations = failedCount,
            CalculationSuccessRate = calculations.Any()
                ? (double)completedCount / calculations.Count * 100
                : 0,
            ByStatus = byStatus,
            ByPlan = byPlan,
            LastCalculationRun = calculations.Any()
                ? calculations.Max(c => c.CalculatedAt)
                : null,
            AverageProcessingTimeMs = 250 // Placeholder - would track actual processing time
        };
    }

    public async Task<ApprovalMetricsDto> GetApprovalMetricsAsync(
        string period,
        CancellationToken cancellationToken = default)
    {
        var pendingApprovals = await _approvalRepository.GetPendingAsync(cancellationToken);
        var calculations = await _calculationRepository.GetByPeriodAsync(period, cancellationToken);
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);

        var approvedThisPeriod = calculations.Count(c => c.Status == CalculationStatus.Approved || c.Status == CalculationStatus.Paid);
        var rejectedThisPeriod = calculations.Count(c => c.Status == CalculationStatus.Rejected);

        var overdueApprovals = pendingApprovals
            .Where(a => (DateTime.UtcNow - a.CreatedAt).TotalHours > DefaultSlaHours)
            .ToList();

        var approachingSla = pendingApprovals
            .Where(a =>
            {
                var hours = (DateTime.UtcNow - a.CreatedAt).TotalHours;
                return hours > DefaultSlaHours * 0.75 && hours <= DefaultSlaHours;
            })
            .ToList();

        var byStatus = pendingApprovals
            .GroupBy(a => a.Status)
            .ToDictionary(g => g.Key, g => g.Count());

        var employeeNames = employees.ToDictionary(e => e.Id, e => e.FullName);

        var urgentApprovals = pendingApprovals
            .OrderBy(a => a.CreatedAt)
            .Take(10)
            .Select(a =>
            {
                var calc = calculations.FirstOrDefault(c => c.Id == a.CalculationId);
                var hoursPending = (DateTime.UtcNow - a.CreatedAt).TotalHours;
                return new PendingApprovalSummaryDto
                {
                    CalculationId = a.CalculationId,
                    EmployeeId = calc?.EmployeeId ?? Guid.Empty,
                    EmployeeName = calc != null && employeeNames.TryGetValue(calc.EmployeeId, out var name)
                        ? name
                        : "Unknown",
                    Period = calc?.CalculationPeriod ?? period,
                    Amount = calc?.NetIncentive.Amount ?? 0,
                    SubmittedAt = a.CreatedAt,
                    HoursPending = hoursPending,
                    SlaHoursRemaining = DefaultSlaHours - hoursPending,
                    IsOverdue = hoursPending > DefaultSlaHours,
                    Currency = calc?.NetIncentive.Currency ?? "USD"
                };
            })
            .ToList();

        return new ApprovalMetricsDto
        {
            TotalPendingApprovals = pendingApprovals.Count,
            ApprovedThisPeriod = approvedThisPeriod,
            RejectedThisPeriod = rejectedThisPeriod,
            ApprovalRate = (approvedThisPeriod + rejectedThisPeriod) > 0
                ? (double)approvedThisPeriod / (approvedThisPeriod + rejectedThisPeriod) * 100
                : 0,
            AverageApprovalTimeHours = 24, // Placeholder - would calculate from actual approval times
            OverduApprovals = overdueApprovals.Count,
            ApproachingSlaApprovals = approachingSla.Count,
            ByStatus = byStatus,
            UrgentApprovals = urgentApprovals
        };
    }

    public async Task<PaymentMetricsDto> GetPaymentMetricsAsync(
        string period,
        CancellationToken cancellationToken = default)
    {
        var calculations = await _calculationRepository.GetByPeriodAsync(period, cancellationToken);

        var paidCalculations = calculations.Where(c => c.Status == CalculationStatus.Paid).ToList();
        var pendingPayment = calculations
            .Where(c => c.Status == CalculationStatus.Approved)
            .ToList();

        // Get trends for past 6 months
        var paymentsByMonth = new Dictionary<string, decimal>();
        var currentDate = DateTime.UtcNow;

        for (var i = 5; i >= 0; i--)
        {
            var monthDate = currentDate.AddMonths(-i);
            var monthPeriod = monthDate.ToString("yyyy-MM");
            var monthCalcs = await _calculationRepository.GetByPeriodAsync(monthPeriod, cancellationToken);
            var monthPaid = monthCalcs.Where(c => c.Status == CalculationStatus.Paid).Sum(c => c.NetIncentive.Amount);
            paymentsByMonth[monthPeriod] = monthPaid;
        }

        return new PaymentMetricsDto
        {
            TotalPaidAmount = paidCalculations.Sum(c => c.NetIncentive.Amount),
            TotalPendingPaymentAmount = pendingPayment.Sum(c => c.NetIncentive.Amount),
            PaidCount = paidCalculations.Count,
            PendingPaymentCount = pendingPayment.Count,
            LastPaymentDate = paidCalculations.Any()
                ? paidCalculations.Max(c => c.ModifiedAt)
                : null,
            PaymentsByMonth = paymentsByMonth,
            Currency = calculations.FirstOrDefault()?.NetIncentive.Currency ?? "USD"
        };
    }

    public async Task<PerformanceMetricsDto> GetPerformanceMetricsAsync(
        string period,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var calculations = await _calculationRepository.GetByPeriodAsync(period, cancellationToken);
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        var departments = await _departmentRepository.GetAllAsync(cancellationToken);
        var plans = await _planRepository.GetAllAsync(cancellationToken);

        if (departmentId.HasValue)
        {
            var deptEmployeeIds = employees
                .Where(e => e.DepartmentId == departmentId.Value)
                .Select(e => e.Id)
                .ToHashSet();
            calculations = calculations.Where(c => deptEmployeeIds.Contains(c.EmployeeId)).ToList();
        }

        if (!calculations.Any())
        {
            return new PerformanceMetricsDto();
        }

        var achievements = calculations.Select(c => c.AchievementPercentage.Value).OrderBy(a => a).ToList();
        var median = achievements[achievements.Count / 2];

        const double targetThreshold = 100.0;
        var aboveTarget = calculations.Count(c => c.AchievementPercentage.Value > targetThreshold);
        var atTarget = calculations.Count(c => Math.Abs(c.AchievementPercentage.Value - targetThreshold) < 5);
        var belowTarget = calculations.Count(c => c.AchievementPercentage.Value < targetThreshold - 5);

        var employeeDepts = employees.ToDictionary(e => e.Id, e => e.DepartmentId);
        var deptNames = departments.ToDictionary(d => d.Id, d => d.Name);

        var achievementByDept = calculations
            .Where(c => employeeDepts.ContainsKey(c.EmployeeId))
            .GroupBy(c => employeeDepts[c.EmployeeId])
            .ToDictionary(
                g => deptNames.GetValueOrDefault(g.Key, "Unknown"),
                g => g.Average(c => c.AchievementPercentage.Value));

        var planNames = plans.ToDictionary(p => p.Id, p => p.Name);
        var achievementByPlan = calculations
            .GroupBy(c => c.IncentivePlanId)
            .ToDictionary(
                g => planNames.GetValueOrDefault(g.Key, "Unknown"),
                g => g.Average(c => c.AchievementPercentage.Value));

        return new PerformanceMetricsDto
        {
            AverageAchievement = achievements.Average(),
            MedianAchievement = median,
            HighestAchievement = achievements.Max(),
            LowestAchievement = achievements.Min(),
            AboveTargetCount = aboveTarget,
            AtTargetCount = atTarget,
            BelowTargetCount = belowTarget,
            AboveTargetPercentage = calculations.Any()
                ? (double)aboveTarget / calculations.Count * 100
                : 0,
            AchievementByDepartment = achievementByDept,
            AchievementByPlan = achievementByPlan
        };
    }

    public async Task<IReadOnlyList<TrendDataPointDto>> GetIncentiveTrendsAsync(
        int months = 12,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var trends = new List<TrendDataPointDto>();
        var currentDate = DateTime.UtcNow;
        var employees = departmentId.HasValue
            ? (await _employeeRepository.GetAllAsync(cancellationToken))
                .Where(e => e.DepartmentId == departmentId.Value)
                .Select(e => e.Id)
                .ToHashSet()
            : null;

        for (var i = months - 1; i >= 0; i--)
        {
            var monthDate = currentDate.AddMonths(-i);
            var period = monthDate.ToString("yyyy-MM");

            var calculations = await _calculationRepository.GetByPeriodAsync(period, cancellationToken);

            if (employees != null)
            {
                calculations = calculations.Where(c => employees.Contains(c.EmployeeId)).ToList();
            }

            trends.Add(new TrendDataPointDto
            {
                Period = period,
                Date = new DateTime(monthDate.Year, monthDate.Month, 1),
                TotalIncentive = calculations.Sum(c => c.NetIncentive.Amount),
                CalculationCount = calculations.Count,
                AverageAchievement = calculations.Any()
                    ? calculations.Average(c => c.AchievementPercentage.Value)
                    : 0,
                AverageIncentive = calculations.Any()
                    ? calculations.Average(c => c.NetIncentive.Amount)
                    : 0
            });
        }

        return trends;
    }

    public async Task<IReadOnlyList<TopPerformerDto>> GetTopPerformersAsync(
        string period,
        int count = 10,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var calculations = await _calculationRepository.GetByPeriodAsync(period, cancellationToken);
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        var departments = await _departmentRepository.GetAllAsync(cancellationToken);

        if (departmentId.HasValue)
        {
            var deptEmployeeIds = employees
                .Where(e => e.DepartmentId == departmentId.Value)
                .Select(e => e.Id)
                .ToHashSet();
            calculations = calculations.Where(c => deptEmployeeIds.Contains(c.EmployeeId)).ToList();
        }

        var employeeDict = employees.ToDictionary(e => e.Id);
        var deptDict = departments.ToDictionary(d => d.Id, d => d.Name);

        // Get previous period for comparison
        var previousPeriod = GetPreviousPeriod(period);
        var previousCalcs = await _calculationRepository.GetByPeriodAsync(previousPeriod, cancellationToken);
        var previousAchievements = previousCalcs.ToDictionary(c => c.EmployeeId, c => c.AchievementPercentage.Value);

        var topPerformers = calculations
            .OrderByDescending(c => c.AchievementPercentage.Value)
            .ThenByDescending(c => c.NetIncentive.Amount)
            .Take(count)
            .Select((c, index) =>
            {
                var employee = employeeDict.GetValueOrDefault(c.EmployeeId);
                var previousAchievement = previousAchievements.GetValueOrDefault(c.EmployeeId, 0);

                return new TopPerformerDto
                {
                    EmployeeId = c.EmployeeId,
                    EmployeeName = employee?.FullName ?? "Unknown",
                    EmployeeCode = employee?.EmployeeCode ?? "N/A",
                    Department = employee != null && deptDict.TryGetValue(employee.DepartmentId, out var dept)
                        ? dept
                        : null,
                    Position = employee?.Position,
                    AchievementPercentage = c.AchievementPercentage.Value,
                    IncentiveAmount = c.NetIncentive.Amount,
                    Rank = index + 1,
                    ChangeFromPreviousPeriod = c.AchievementPercentage.Value - previousAchievement,
                    Currency = c.NetIncentive.Currency
                };
            })
            .ToList();

        return topPerformers;
    }

    public async Task<IReadOnlyList<DepartmentSummaryDto>> GetDepartmentSummariesAsync(
        string period,
        CancellationToken cancellationToken = default)
    {
        var calculations = await _calculationRepository.GetByPeriodAsync(period, cancellationToken);
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        var departments = await _departmentRepository.GetAllAsync(cancellationToken);

        var employeeDepts = employees.ToDictionary(e => e.Id, e => e.DepartmentId);
        var deptEmployeeCounts = employees
            .Where(e => e.Status == EmployeeStatus.Active)
            .GroupBy(e => e.DepartmentId)
            .ToDictionary(g => g.Key, g => g.Count());

        var summaries = departments.Select(dept =>
        {
            var deptCalculations = calculations
                .Where(c => employeeDepts.TryGetValue(c.EmployeeId, out var deptId) && deptId == dept.Id)
                .ToList();

            var aboveTarget = deptCalculations.Count(c => c.AchievementPercentage.Value > 100);

            return new DepartmentSummaryDto
            {
                DepartmentId = dept.Id,
                DepartmentName = dept.Name,
                DepartmentCode = dept.Code,
                EmployeeCount = deptEmployeeCounts.GetValueOrDefault(dept.Id, 0),
                CalculationCount = deptCalculations.Count,
                TotalIncentive = deptCalculations.Sum(c => c.NetIncentive.Amount),
                AverageIncentive = deptCalculations.Any()
                    ? deptCalculations.Average(c => c.NetIncentive.Amount)
                    : 0,
                AverageAchievement = deptCalculations.Any()
                    ? deptCalculations.Average(c => c.AchievementPercentage.Value)
                    : 0,
                AboveTargetCount = aboveTarget,
                AboveTargetPercentage = deptCalculations.Any()
                    ? (double)aboveTarget / deptCalculations.Count * 100
                    : 0,
                Currency = deptCalculations.FirstOrDefault()?.NetIncentive.Currency ?? "USD"
            };
        }).ToList();

        return summaries;
    }

    public async Task<IReadOnlyList<AlertDto>> GetAlertsAsync(
        CancellationToken cancellationToken = default)
    {
        var alerts = new List<AlertDto>();

        // Check for overdue approvals
        var pendingApprovals = await _approvalRepository.GetPendingAsync(cancellationToken);
        var overdueCount = pendingApprovals.Count(a => (DateTime.UtcNow - a.CreatedAt).TotalHours > DefaultSlaHours);

        if (overdueCount > 0)
        {
            alerts.Add(new AlertDto
            {
                Title = "Overdue Approvals",
                Message = $"{overdueCount} approval(s) have exceeded the SLA deadline",
                Severity = overdueCount > 5 ? AlertSeverity.Critical : AlertSeverity.Warning,
                Category = AlertCategory.Approval,
                ActionUrl = "/approvals?status=overdue",
                CreatedAt = DateTime.UtcNow
            });
        }

        // Check for pending approvals approaching SLA
        var approachingSlaCount = pendingApprovals.Count(a =>
        {
            var hours = (DateTime.UtcNow - a.CreatedAt).TotalHours;
            return hours > DefaultSlaHours * 0.75 && hours <= DefaultSlaHours;
        });

        if (approachingSlaCount > 0)
        {
            alerts.Add(new AlertDto
            {
                Title = "Approvals Approaching SLA",
                Message = $"{approachingSlaCount} approval(s) are approaching the SLA deadline",
                Severity = AlertSeverity.Warning,
                Category = AlertCategory.Approval,
                ActionUrl = "/approvals?status=approaching-sla",
                CreatedAt = DateTime.UtcNow
            });
        }

        // Check for large pending payment amounts
        var currentPeriod = GetCurrentPeriod();
        var calculations = await _calculationRepository.GetByPeriodAsync(currentPeriod, cancellationToken);
        var pendingPaymentAmount = calculations
            .Where(c => c.Status == CalculationStatus.Approved)
            .Sum(c => c.NetIncentive.Amount);

        if (pendingPaymentAmount > 100000)
        {
            alerts.Add(new AlertDto
            {
                Title = "Large Pending Payments",
                Message = $"Total pending payment amount: {pendingPaymentAmount:C}",
                Severity = AlertSeverity.Info,
                Category = AlertCategory.Payment,
                ActionUrl = "/payments?status=pending",
                CreatedAt = DateTime.UtcNow,
                Metadata = new Dictionary<string, object> { ["Amount"] = pendingPaymentAmount }
            });
        }

        return alerts.OrderByDescending(a => a.Severity).ToList();
    }

    public async Task<LeaderboardDto> GetLeaderboardAsync(
        string period,
        int topN = 100,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var calculations = await _calculationRepository.GetByPeriodAsync(period, cancellationToken);
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        var departments = await _departmentRepository.GetAllAsync(cancellationToken);

        if (departmentId.HasValue)
        {
            var deptEmployeeIds = employees
                .Where(e => e.DepartmentId == departmentId.Value)
                .Select(e => e.Id)
                .ToHashSet();
            calculations = calculations.Where(c => deptEmployeeIds.Contains(c.EmployeeId)).ToList();
        }

        var employeeDict = employees.ToDictionary(e => e.Id);
        var deptDict = departments.ToDictionary(d => d.Id, d => d.Name);

        // Get previous period rankings
        var previousPeriod = GetPreviousPeriod(period);
        var previousCalcs = await _calculationRepository.GetByPeriodAsync(previousPeriod, cancellationToken);
        var previousRanks = previousCalcs
            .OrderByDescending(c => c.AchievementPercentage.Value)
            .Select((c, i) => new { c.EmployeeId, Rank = i + 1 })
            .ToDictionary(x => x.EmployeeId, x => x.Rank);

        var entries = calculations
            .OrderByDescending(c => c.AchievementPercentage.Value)
            .ThenByDescending(c => c.NetIncentive.Amount)
            .Take(topN)
            .Select((c, index) =>
            {
                var employee = employeeDict.GetValueOrDefault(c.EmployeeId);
                return new LeaderboardEntryDto
                {
                    Rank = index + 1,
                    PreviousRank = previousRanks.GetValueOrDefault(c.EmployeeId, topN + 1),
                    EmployeeId = c.EmployeeId,
                    EmployeeName = employee?.FullName ?? "Unknown",
                    EmployeeCode = employee?.EmployeeCode ?? "N/A",
                    Department = employee != null && deptDict.TryGetValue(employee.DepartmentId, out var dept)
                        ? dept
                        : null,
                    AchievementPercentage = c.AchievementPercentage.Value,
                    IncentiveAmount = c.NetIncentive.Amount,
                    Currency = c.NetIncentive.Currency
                };
            })
            .ToList();

        return new LeaderboardDto
        {
            Period = period,
            TotalParticipants = calculations.Count,
            Entries = entries
        };
    }

    public async Task<PeriodComparisonDto> GetPeriodComparisonAsync(
        string currentPeriod,
        string previousPeriod,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var currentCalcs = await _calculationRepository.GetByPeriodAsync(currentPeriod, cancellationToken);
        var previousCalcs = await _calculationRepository.GetByPeriodAsync(previousPeriod, cancellationToken);

        if (departmentId.HasValue)
        {
            var employees = await _employeeRepository.GetAllAsync(cancellationToken);
            var deptEmployeeIds = employees
                .Where(e => e.DepartmentId == departmentId.Value)
                .Select(e => e.Id)
                .ToHashSet();
            currentCalcs = currentCalcs.Where(c => deptEmployeeIds.Contains(c.EmployeeId)).ToList();
            previousCalcs = previousCalcs.Where(c => deptEmployeeIds.Contains(c.EmployeeId)).ToList();
        }

        var currentTotal = currentCalcs.Sum(c => c.NetIncentive.Amount);
        var previousTotal = previousCalcs.Sum(c => c.NetIncentive.Amount);
        var difference = currentTotal - previousTotal;
        var percentageChange = previousTotal != 0
            ? (double)(difference / previousTotal) * 100
            : 0;

        var currentAvgAchievement = currentCalcs.Any()
            ? currentCalcs.Average(c => c.AchievementPercentage.Value)
            : 0;
        var previousAvgAchievement = previousCalcs.Any()
            ? previousCalcs.Average(c => c.AchievementPercentage.Value)
            : 0;

        return new PeriodComparisonDto
        {
            CurrentPeriod = currentPeriod,
            PreviousPeriod = previousPeriod,
            CurrentTotal = currentTotal,
            PreviousTotal = previousTotal,
            Difference = difference,
            PercentageChange = percentageChange,
            CurrentCount = currentCalcs.Count,
            PreviousCount = previousCalcs.Count,
            CurrentAverageAchievement = currentAvgAchievement,
            PreviousAverageAchievement = previousAvgAchievement,
            AchievementChange = currentAvgAchievement - previousAvgAchievement
        };
    }

    public async Task<WidgetDataDto> GetWidgetDataAsync(
        string widgetId,
        DashboardFilterDto filter,
        CancellationToken cancellationToken = default)
    {
        var period = filter.Period ?? GetCurrentPeriod();

        object? data = widgetId.ToLowerInvariant() switch
        {
            "summary" => await GetSummaryKpisAsync(period, filter.DepartmentId, cancellationToken),
            "calculations" => await GetCalculationMetricsAsync(period, filter.DepartmentId, cancellationToken),
            "approvals" => await GetApprovalMetricsAsync(period, cancellationToken),
            "payments" => await GetPaymentMetricsAsync(period, cancellationToken),
            "performance" => await GetPerformanceMetricsAsync(period, filter.DepartmentId, cancellationToken),
            "trends" => await GetIncentiveTrendsAsync(12, filter.DepartmentId, cancellationToken),
            "top-performers" => await GetTopPerformersAsync(period, filter.TopPerformersCount, filter.DepartmentId, cancellationToken),
            "departments" => await GetDepartmentSummariesAsync(period, cancellationToken),
            "alerts" => await GetAlertsAsync(cancellationToken),
            "leaderboard" => await GetLeaderboardAsync(period, 25, filter.DepartmentId, cancellationToken),
            _ => null
        };

        return new WidgetDataDto
        {
            WidgetId = widgetId,
            Title = GetWidgetTitle(widgetId),
            Type = widgetId,
            Data = data
        };
    }

    public async Task<IReadOnlyList<PendingApprovalSummaryDto>> GetUrgentApprovalsAsync(
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        var pendingApprovals = await _approvalRepository.GetPendingAsync(cancellationToken);
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        var employeeNames = employees.ToDictionary(e => e.Id, e => e.FullName);

        return pendingApprovals
            .OrderBy(a => a.CreatedAt)
            .Take(count)
            .Select(a =>
            {
                var hoursPending = (DateTime.UtcNow - a.CreatedAt).TotalHours;
                return new PendingApprovalSummaryDto
                {
                    CalculationId = a.CalculationId,
                    EmployeeId = a.CalculationId, // Would need calculation lookup for actual employee
                    EmployeeName = "Pending Load",
                    Period = GetCurrentPeriod(),
                    Amount = 0, // Would need calculation lookup
                    SubmittedAt = a.CreatedAt,
                    HoursPending = hoursPending,
                    SlaHoursRemaining = DefaultSlaHours - hoursPending,
                    IsOverdue = hoursPending > DefaultSlaHours
                };
            })
            .ToList();
    }

    private static string GetCurrentPeriod() => DateTime.UtcNow.ToString("yyyy-MM");

    private static string GetPreviousPeriod(string period)
    {
        if (DateTime.TryParseExact(period + "-01", "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None,
            out var date))
        {
            return date.AddMonths(-1).ToString("yyyy-MM");
        }
        return DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM");
    }

    private static string GetWidgetTitle(string widgetId) => widgetId.ToLowerInvariant() switch
    {
        "summary" => "Summary KPIs",
        "calculations" => "Calculation Metrics",
        "approvals" => "Approval Metrics",
        "payments" => "Payment Metrics",
        "performance" => "Performance Metrics",
        "trends" => "Incentive Trends",
        "top-performers" => "Top Performers",
        "departments" => "Department Summary",
        "alerts" => "Alerts",
        "leaderboard" => "Leaderboard",
        _ => widgetId
    };
}
