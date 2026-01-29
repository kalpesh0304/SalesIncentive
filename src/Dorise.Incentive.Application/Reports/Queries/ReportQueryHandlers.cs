using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.Reports.DTOs;
using Dorise.Incentive.Application.Reports.Services;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.Reports.Queries;

/// <summary>
/// Handler for GetPayoutReportQuery.
/// "My cat's breath smells like cat food." - Reports smell like success!
/// </summary>
public class GetPayoutReportQueryHandler : IQueryHandler<GetPayoutReportQuery, PayoutReportDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPayoutReportQueryHandler> _logger;

    public GetPayoutReportQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetPayoutReportQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PayoutReportDto>> Handle(
        GetPayoutReportQuery request,
        CancellationToken cancellationToken)
    {
        var calculations = await _unitOfWork.Calculations.GetByPeriodAsync(
            request.PeriodStart,
            request.PeriodEnd,
            cancellationToken);

        // Filter by department if specified
        if (request.DepartmentId.HasValue)
        {
            var employeeIds = (await _unitOfWork.Employees.GetByDepartmentAsync(
                request.DepartmentId.Value,
                cancellationToken)).Select(e => e.Id).ToHashSet();

            calculations = calculations.Where(c => employeeIds.Contains(c.EmployeeId)).ToList();
        }

        // Filter by plan if specified
        if (request.PlanId.HasValue)
        {
            calculations = calculations.Where(c => c.IncentivePlanId == request.PlanId.Value).ToList();
        }

        var details = new List<PayoutDetailDto>();

        foreach (var calc in calculations)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(calc.EmployeeId, cancellationToken);
            var plan = await _unitOfWork.IncentivePlans.GetByIdAsync(calc.IncentivePlanId, cancellationToken);
            var department = employee != null
                ? await _unitOfWork.Departments.GetByIdAsync(employee.DepartmentId, cancellationToken)
                : null;

            var slab = calc.AppliedSlabId.HasValue
                ? await _unitOfWork.IncentivePlans.GetSlabByIdAsync(calc.AppliedSlabId.Value, cancellationToken)
                : null;

            details.Add(new PayoutDetailDto
            {
                EmployeeId = calc.EmployeeId,
                EmployeeCode = employee?.EmployeeCode ?? "Unknown",
                EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown",
                Department = department?.Name ?? "Unknown",
                PlanCode = plan?.Code ?? "Unknown",
                PlanName = plan?.Name ?? "Unknown",
                TargetValue = calc.TargetValue,
                ActualValue = calc.ActualValue,
                AchievementPercentage = calc.AchievementPercentage.Value,
                GrossIncentive = calc.GrossIncentive.Amount,
                NetIncentive = calc.NetIncentive.Amount,
                Currency = calc.NetIncentive.Currency,
                SlabApplied = slab?.Name ?? "N/A",
                Status = calc.Status,
                PaidDate = calc.Status == CalculationStatus.Paid ? calc.ModifiedAt : null
            });
        }

        // Calculate summary
        var netIncentives = details.Select(d => d.NetIncentive).ToList();
        var sortedIncentives = netIncentives.OrderBy(x => x).ToList();

        var summary = new PayoutSummaryDto
        {
            TotalEmployees = details.Count,
            EligibleEmployees = details.Count(d => d.Status != CalculationStatus.Ineligible),
            PaidEmployees = details.Count(d => d.Status == CalculationStatus.Paid),
            TotalGrossIncentive = details.Sum(d => d.GrossIncentive),
            TotalNetIncentive = details.Sum(d => d.NetIncentive),
            TotalDeductions = details.Sum(d => d.GrossIncentive - d.NetIncentive),
            Currency = "INR",
            AverageIncentive = netIncentives.Any() ? netIncentives.Average() : 0,
            MedianIncentive = sortedIncentives.Any()
                ? sortedIncentives[sortedIncentives.Count / 2]
                : 0,
            MinIncentive = netIncentives.Any() ? netIncentives.Min() : 0,
            MaxIncentive = netIncentives.Any() ? netIncentives.Max() : 0
        };

        var periodLabel = $"{request.PeriodStart:yyyy-MM} to {request.PeriodEnd:yyyy-MM}";

        _logger.LogInformation(
            "Generated payout report for period {Period} with {Count} employees",
            periodLabel,
            details.Count);

        return Result<PayoutReportDto>.Success(new PayoutReportDto
        {
            GeneratedAt = DateTime.UtcNow,
            Period = periodLabel,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            Summary = summary,
            Details = details.OrderBy(d => d.Department).ThenBy(d => d.EmployeeName).ToList()
        });
    }
}

/// <summary>
/// Handler for GetAchievementSummaryQuery.
/// </summary>
public class GetAchievementSummaryQueryHandler : IQueryHandler<GetAchievementSummaryQuery, AchievementSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetAchievementSummaryQueryHandler> _logger;

    public GetAchievementSummaryQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetAchievementSummaryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<AchievementSummaryDto>> Handle(
        GetAchievementSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var calculations = await _unitOfWork.Calculations.GetByPeriodAsync(
            request.PeriodStart,
            request.PeriodEnd,
            cancellationToken);

        // Filter by department if specified
        if (request.DepartmentId.HasValue)
        {
            var employeeIds = (await _unitOfWork.Employees.GetByDepartmentAsync(
                request.DepartmentId.Value,
                cancellationToken)).Select(e => e.Id).ToHashSet();

            calculations = calculations.Where(c => employeeIds.Contains(c.EmployeeId)).ToList();
        }

        var achievements = calculations.Select(c => c.AchievementPercentage.Value).ToList();
        var sortedAchievements = achievements.OrderBy(x => x).ToList();

        // Overall statistics
        var overall = new OverallAchievementDto
        {
            TotalEmployees = calculations.Count,
            AverageAchievement = achievements.Any() ? achievements.Average() : 0,
            MedianAchievement = sortedAchievements.Any()
                ? sortedAchievements[sortedAchievements.Count / 2]
                : 0,
            AboveTargetCount = achievements.Count(a => a > 100),
            AtTargetCount = achievements.Count(a => a >= 95 && a <= 100),
            BelowTargetCount = achievements.Count(a => a < 95),
            AboveTargetPercentage = achievements.Any()
                ? (decimal)achievements.Count(a => a > 100) / achievements.Count * 100
                : 0
        };

        // By department
        var byDepartment = new List<DepartmentAchievementDto>();
        var employeeDepartments = new Dictionary<Guid, Guid>();

        foreach (var calc in calculations)
        {
            if (!employeeDepartments.ContainsKey(calc.EmployeeId))
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(calc.EmployeeId, cancellationToken);
                if (employee != null)
                {
                    employeeDepartments[calc.EmployeeId] = employee.DepartmentId;
                }
            }
        }

        var deptGroups = calculations
            .Where(c => employeeDepartments.ContainsKey(c.EmployeeId))
            .GroupBy(c => employeeDepartments[c.EmployeeId]);

        foreach (var group in deptGroups)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(group.Key, cancellationToken);
            var deptCalcs = group.ToList();

            byDepartment.Add(new DepartmentAchievementDto
            {
                DepartmentId = group.Key,
                DepartmentName = department?.Name ?? "Unknown",
                EmployeeCount = deptCalcs.Count,
                AverageAchievement = deptCalcs.Average(c => c.AchievementPercentage.Value),
                TotalTarget = deptCalcs.Sum(c => c.TargetValue),
                TotalActual = deptCalcs.Sum(c => c.ActualValue),
                TotalIncentive = deptCalcs.Sum(c => c.NetIncentive.Amount),
                Currency = "INR"
            });
        }

        // By plan
        var planGroups = calculations.GroupBy(c => c.IncentivePlanId);
        var byPlan = new List<PlanAchievementDto>();

        foreach (var group in planGroups)
        {
            var plan = await _unitOfWork.IncentivePlans.GetByIdAsync(group.Key, cancellationToken);
            var planCalcs = group.ToList();

            byPlan.Add(new PlanAchievementDto
            {
                PlanId = group.Key,
                PlanCode = plan?.Code ?? "Unknown",
                PlanName = plan?.Name ?? "Unknown",
                EmployeeCount = planCalcs.Count,
                AverageAchievement = planCalcs.Average(c => c.AchievementPercentage.Value),
                TotalIncentive = planCalcs.Sum(c => c.NetIncentive.Amount),
                Currency = "INR"
            });
        }

        // By achievement band
        var bands = new List<AchievementBandDto>
        {
            CreateBand("Below 50%", 0, 50, achievements),
            CreateBand("50% - 75%", 50, 75, achievements),
            CreateBand("75% - 90%", 75, 90, achievements),
            CreateBand("90% - 100%", 90, 100, achievements),
            CreateBand("100% - 120%", 100, 120, achievements),
            CreateBand("Above 120%", 120, decimal.MaxValue, achievements)
        };

        var periodLabel = $"{request.PeriodStart:yyyy-MM} to {request.PeriodEnd:yyyy-MM}";

        _logger.LogInformation(
            "Generated achievement summary for period {Period} with {Count} employees",
            periodLabel,
            calculations.Count);

        return Result<AchievementSummaryDto>.Success(new AchievementSummaryDto
        {
            GeneratedAt = DateTime.UtcNow,
            Period = periodLabel,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            Overall = overall,
            ByDepartment = byDepartment.OrderByDescending(d => d.AverageAchievement).ToList(),
            ByPlan = byPlan.OrderByDescending(p => p.AverageAchievement).ToList(),
            ByAchievementBand = bands
        });
    }

    private static AchievementBandDto CreateBand(string name, decimal min, decimal max, List<decimal> achievements)
    {
        var count = achievements.Count(a => a >= min && a < max);
        return new AchievementBandDto
        {
            BandName = name,
            MinPercentage = min,
            MaxPercentage = max == decimal.MaxValue ? 999 : max,
            EmployeeCount = count,
            Percentage = achievements.Any() ? (decimal)count / achievements.Count * 100 : 0
        };
    }
}

/// <summary>
/// Handler for GetVarianceAnalysisQuery.
/// </summary>
public class GetVarianceAnalysisQueryHandler : IQueryHandler<GetVarianceAnalysisQuery, VarianceAnalysisDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetVarianceAnalysisQueryHandler> _logger;

    public GetVarianceAnalysisQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetVarianceAnalysisQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<VarianceAnalysisDto>> Handle(
        GetVarianceAnalysisQuery request,
        CancellationToken cancellationToken)
    {
        var currentCalcs = await _unitOfWork.Calculations.GetByPeriodAsync(
            request.CurrentPeriodStart,
            request.CurrentPeriodEnd,
            cancellationToken);

        var previousCalcs = await _unitOfWork.Calculations.GetByPeriodAsync(
            request.PreviousPeriodStart,
            request.PreviousPeriodEnd,
            cancellationToken);

        // Filter by department if specified
        if (request.DepartmentId.HasValue)
        {
            var employeeIds = (await _unitOfWork.Employees.GetByDepartmentAsync(
                request.DepartmentId.Value,
                cancellationToken)).Select(e => e.Id).ToHashSet();

            currentCalcs = currentCalcs.Where(c => employeeIds.Contains(c.EmployeeId)).ToList();
            previousCalcs = previousCalcs.Where(c => employeeIds.Contains(c.EmployeeId)).ToList();
        }

        // Summary
        var currentTotal = currentCalcs.Sum(c => c.NetIncentive.Amount);
        var previousTotal = previousCalcs.Sum(c => c.NetIncentive.Amount);
        var absoluteVariance = currentTotal - previousTotal;
        var percentageVariance = previousTotal != 0
            ? (absoluteVariance / previousTotal) * 100
            : 0;

        var currentAvgAchievement = currentCalcs.Any()
            ? currentCalcs.Average(c => c.AchievementPercentage.Value)
            : 0;
        var previousAvgAchievement = previousCalcs.Any()
            ? previousCalcs.Average(c => c.AchievementPercentage.Value)
            : 0;

        var summary = new VarianceSummaryDto
        {
            CurrentPeriodTotal = currentTotal,
            PreviousPeriodTotal = previousTotal,
            AbsoluteVariance = absoluteVariance,
            PercentageVariance = percentageVariance,
            CurrentAverageAchievement = currentAvgAchievement,
            PreviousAverageAchievement = previousAvgAchievement,
            AchievementVariance = currentAvgAchievement - previousAvgAchievement,
            Currency = "INR"
        };

        // Employee variances
        var previousByEmployee = previousCalcs.ToDictionary(c => c.EmployeeId, c => c);
        var employeeVariances = new List<EmployeeVarianceDto>();

        foreach (var current in currentCalcs)
        {
            previousByEmployee.TryGetValue(current.EmployeeId, out var previous);

            var employee = await _unitOfWork.Employees.GetByIdAsync(current.EmployeeId, cancellationToken);
            var department = employee != null
                ? await _unitOfWork.Departments.GetByIdAsync(employee.DepartmentId, cancellationToken)
                : null;

            var prevIncentive = previous?.NetIncentive.Amount ?? 0;
            var absVar = current.NetIncentive.Amount - prevIncentive;
            var pctVar = prevIncentive != 0 ? (absVar / prevIncentive) * 100 : 0;

            employeeVariances.Add(new EmployeeVarianceDto
            {
                EmployeeId = current.EmployeeId,
                EmployeeCode = employee?.EmployeeCode ?? "Unknown",
                EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown",
                Department = department?.Name ?? "Unknown",
                CurrentIncentive = current.NetIncentive.Amount,
                PreviousIncentive = prevIncentive,
                AbsoluteVariance = absVar,
                PercentageVariance = pctVar,
                CurrentAchievement = current.AchievementPercentage.Value,
                PreviousAchievement = previous?.AchievementPercentage.Value ?? 0,
                Currency = "INR"
            });
        }

        var topGainers = employeeVariances
            .OrderByDescending(v => v.AbsoluteVariance)
            .Take(request.TopCount)
            .ToList();

        var topDecliners = employeeVariances
            .OrderBy(v => v.AbsoluteVariance)
            .Take(request.TopCount)
            .ToList();

        // Department variances
        var employeeDepartments = new Dictionary<Guid, Guid>();
        foreach (var calc in currentCalcs.Concat(previousCalcs))
        {
            if (!employeeDepartments.ContainsKey(calc.EmployeeId))
            {
                var employee = await _unitOfWork.Employees.GetByIdAsync(calc.EmployeeId, cancellationToken);
                if (employee != null)
                {
                    employeeDepartments[calc.EmployeeId] = employee.DepartmentId;
                }
            }
        }

        var allDeptIds = employeeDepartments.Values.Distinct();
        var byDepartment = new List<DepartmentVarianceDto>();

        foreach (var deptId in allDeptIds)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(deptId, cancellationToken);
            var deptEmployees = employeeDepartments.Where(kv => kv.Value == deptId).Select(kv => kv.Key).ToHashSet();

            var currentDept = currentCalcs.Where(c => deptEmployees.Contains(c.EmployeeId)).ToList();
            var previousDept = previousCalcs.Where(c => deptEmployees.Contains(c.EmployeeId)).ToList();

            var currTotal = currentDept.Sum(c => c.NetIncentive.Amount);
            var prevTotal = previousDept.Sum(c => c.NetIncentive.Amount);
            var deptAbsVar = currTotal - prevTotal;
            var deptPctVar = prevTotal != 0 ? (deptAbsVar / prevTotal) * 100 : 0;

            byDepartment.Add(new DepartmentVarianceDto
            {
                DepartmentId = deptId,
                DepartmentName = department?.Name ?? "Unknown",
                CurrentTotal = currTotal,
                PreviousTotal = prevTotal,
                AbsoluteVariance = deptAbsVar,
                PercentageVariance = deptPctVar,
                CurrentEmployeeCount = currentDept.Count,
                PreviousEmployeeCount = previousDept.Count,
                Currency = "INR"
            });
        }

        var currentPeriodLabel = $"{request.CurrentPeriodStart:yyyy-MM}";
        var previousPeriodLabel = $"{request.PreviousPeriodStart:yyyy-MM}";

        _logger.LogInformation(
            "Generated variance analysis comparing {Current} vs {Previous}",
            currentPeriodLabel,
            previousPeriodLabel);

        return Result<VarianceAnalysisDto>.Success(new VarianceAnalysisDto
        {
            GeneratedAt = DateTime.UtcNow,
            CurrentPeriod = currentPeriodLabel,
            PreviousPeriod = previousPeriodLabel,
            Summary = summary,
            TopGainers = topGainers,
            TopDecliners = topDecliners,
            ByDepartment = byDepartment.OrderByDescending(d => d.AbsoluteVariance).ToList()
        });
    }
}

/// <summary>
/// Handler for GetDashboardQuery.
/// </summary>
public class GetDashboardQueryHandler : IQueryHandler<GetDashboardQuery, DashboardDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetDashboardQueryHandler> _logger;

    public GetDashboardQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetDashboardQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<DashboardDto>> Handle(
        GetDashboardQuery request,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var currentMonthStart = new DateTime(now.Year, now.Month, 1);
        var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
        var yearStart = new DateTime(now.Year, 1, 1);

        // Get current month calculations
        var currentMonthCalcs = await _unitOfWork.Calculations.GetByPeriodAsync(
            currentMonthStart,
            currentMonthEnd,
            cancellationToken);

        // Get YTD calculations
        var ytdCalcs = await _unitOfWork.Calculations.GetByPeriodAsync(
            yearStart,
            now,
            cancellationToken);

        // Get all employees
        var allEmployees = await _unitOfWork.Employees.GetAllActiveAsync(cancellationToken);

        // Get pending approvals
        var pendingApprovals = await _unitOfWork.Approvals.GetAllPendingPagedAsync(1, 1, cancellationToken);

        // KPIs
        var kpis = new DashboardKpisDto
        {
            TotalIncentiveYtd = ytdCalcs.Sum(c => c.NetIncentive.Amount),
            TotalIncentiveCurrentMonth = currentMonthCalcs.Sum(c => c.NetIncentive.Amount),
            TotalEmployees = allEmployees.Count,
            EligibleEmployees = currentMonthCalcs.Count,
            AverageAchievement = currentMonthCalcs.Any()
                ? currentMonthCalcs.Average(c => c.AchievementPercentage.Value)
                : 0,
            PendingApprovals = pendingApprovals.TotalCount,
            BudgetUtilization = 0, // Would need budget data
            Currency = "INR"
        };

        // Monthly trend (last 12 months)
        var monthlyTrend = new List<MonthlyTrendDto>();
        for (int i = 11; i >= 0; i--)
        {
            var monthStart = now.AddMonths(-i);
            monthStart = new DateTime(monthStart.Year, monthStart.Month, 1);
            var monthEnd = monthStart.AddMonths(1).AddDays(-1);

            var monthCalcs = await _unitOfWork.Calculations.GetByPeriodAsync(
                monthStart,
                monthEnd,
                cancellationToken);

            monthlyTrend.Add(new MonthlyTrendDto
            {
                Month = monthStart.ToString("MMM"),
                Year = monthStart.Year,
                TotalIncentive = monthCalcs.Sum(c => c.NetIncentive.Amount),
                EmployeeCount = monthCalcs.Count,
                AverageAchievement = monthCalcs.Any()
                    ? monthCalcs.Average(c => c.AchievementPercentage.Value)
                    : 0,
                Currency = "INR"
            });
        }

        // Top performers
        var topPerformers = new List<TopPerformerDto>();
        var rankedCalcs = currentMonthCalcs
            .OrderByDescending(c => c.AchievementPercentage.Value)
            .Take(request.TopPerformerCount)
            .ToList();

        var rank = 1;
        foreach (var calc in rankedCalcs)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(calc.EmployeeId, cancellationToken);
            var department = employee != null
                ? await _unitOfWork.Departments.GetByIdAsync(employee.DepartmentId, cancellationToken)
                : null;

            topPerformers.Add(new TopPerformerDto
            {
                EmployeeId = calc.EmployeeId,
                EmployeeCode = employee?.EmployeeCode ?? "Unknown",
                EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown",
                Department = department?.Name ?? "Unknown",
                Achievement = calc.AchievementPercentage.Value,
                Incentive = calc.NetIncentive.Amount,
                Currency = "INR",
                Rank = rank++
            });
        }

        // Pending actions
        var pendingActions = new List<PendingActionDto>
        {
            new()
            {
                ActionType = "Approvals",
                Count = pendingApprovals.TotalCount,
                Description = "Pending approval requests",
                Url = "/approvals/pending"
            },
            new()
            {
                ActionType = "Calculations",
                Count = currentMonthCalcs.Count(c => c.Status == CalculationStatus.Pending),
                Description = "Pending calculations",
                Url = "/calculations?status=pending"
            }
        };

        _logger.LogInformation("Generated executive dashboard");

        return Result<DashboardDto>.Success(new DashboardDto
        {
            GeneratedAt = DateTime.UtcNow,
            CurrentPeriod = $"{now:yyyy-MM}",
            Kpis = kpis,
            MonthlyTrend = monthlyTrend,
            TopPerformers = topPerformers,
            PendingActions = pendingActions
        });
    }
}
