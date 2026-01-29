using Dorise.Incentive.Application.Reports.DTOs;
using Dorise.Incentive.Application.Reports.Services;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Dorise.Incentive.Infrastructure.Reports;

/// <summary>
/// Report generation service implementation.
/// "I'm learnding!" - I'm generating reports!
/// </summary>
public class ReportGenerationService : IReportGenerationService
{
    private readonly ICalculationRepository _calculationRepository;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IApprovalRepository _approvalRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IIncentivePlanRepository _planRepository;
    private readonly ILogger<ReportGenerationService> _logger;

    private readonly List<ReportScheduleDto> _schedules = new();

    public ReportGenerationService(
        ICalculationRepository calculationRepository,
        IEmployeeRepository employeeRepository,
        IApprovalRepository approvalRepository,
        IDepartmentRepository departmentRepository,
        IIncentivePlanRepository planRepository,
        ILogger<ReportGenerationService> logger)
    {
        _calculationRepository = calculationRepository;
        _employeeRepository = employeeRepository;
        _approvalRepository = approvalRepository;
        _departmentRepository = departmentRepository;
        _planRepository = planRepository;
        _logger = logger;
    }

    public async Task<GeneratedReportDto> GenerateReportAsync(
        ReportRequestDto request,
        string generatedBy,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Generating {ReportType} report for period {Period}",
            request.ReportType, request.Period);

        var metadata = new ReportMetadataDto
        {
            ReportName = GetReportName(request.ReportType),
            ReportType = request.ReportType,
            GeneratedBy = generatedBy,
            Period = request.Period,
            FromDate = request.FromDate,
            ToDate = request.ToDate,
            Parameters = request.AdditionalParameters ?? new Dictionary<string, object>()
        };

        object? jsonData = request.ReportType switch
        {
            ReportTypes.IncentiveSummary => await GenerateIncentiveSummaryAsync(
                request.Period, request.DepartmentId, cancellationToken),
            ReportTypes.Performance => await GenerateAchievementSummaryAsync(
                request.Period, request.DepartmentId, cancellationToken),
            ReportTypes.Variance => await GenerateVarianceAnalysisAsync(
                request.Period, GetPreviousPeriod(request.Period), request.DepartmentId, cancellationToken),
            ReportTypes.Forecast => await GenerateForecastAsync(6, request.DepartmentId, cancellationToken),
            _ => await GenerateIncentiveSummaryAsync(request.Period, request.DepartmentId, cancellationToken)
        };

        byte[]? content = null;
        string? contentType = null;
        string? fileName = null;

        if (request.Format != ReportFormats.Json)
        {
            var export = await ExportReportAsync(request, request.Format, cancellationToken);
            content = export.Content;
            contentType = export.ContentType;
            fileName = export.FileName;
        }

        return new GeneratedReportDto
        {
            Metadata = metadata,
            Format = request.Format,
            Content = content,
            ContentType = contentType,
            FileName = fileName,
            JsonData = jsonData
        };
    }

    public async Task<PayoutReportDto> GenerateIncentiveSummaryAsync(
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

        var employeeDict = employees.ToDictionary(e => e.Id);
        var deptDict = departments.ToDictionary(d => d.Id, d => d.Name);
        var planDict = plans.ToDictionary(p => p.Id);

        var details = calculations.Select(c =>
        {
            var emp = employeeDict.GetValueOrDefault(c.EmployeeId);
            var plan = planDict.GetValueOrDefault(c.IncentivePlanId);
            return new PayoutDetailDto
            {
                EmployeeId = c.EmployeeId,
                EmployeeCode = emp?.EmployeeCode ?? "N/A",
                EmployeeName = emp?.FullName ?? "Unknown",
                Department = emp != null && deptDict.TryGetValue(emp.DepartmentId, out var dept) ? dept : "Unknown",
                PlanCode = plan?.Code ?? "N/A",
                PlanName = plan?.Name ?? "Unknown",
                TargetValue = c.TargetValue.Amount,
                ActualValue = c.ActualValue.Amount,
                AchievementPercentage = (decimal)c.AchievementPercentage.Value,
                GrossIncentive = c.GrossIncentive.Amount,
                NetIncentive = c.NetIncentive.Amount,
                Currency = c.NetIncentive.Currency,
                SlabApplied = c.AppliedSlabName ?? "N/A",
                Status = c.Status,
                PaidDate = c.Status == CalculationStatus.Paid ? c.ModifiedAt : null
            };
        }).ToList();

        var amounts = calculations.Select(c => c.NetIncentive.Amount).OrderBy(a => a).ToList();

        return new PayoutReportDto
        {
            GeneratedAt = DateTime.UtcNow,
            Period = period,
            PeriodStart = GetPeriodStartDate(period),
            PeriodEnd = GetPeriodEndDate(period),
            Summary = new PayoutSummaryDto
            {
                TotalEmployees = employees.Count(e => e.Status == EmployeeStatus.Active),
                EligibleEmployees = calculations.Select(c => c.EmployeeId).Distinct().Count(),
                PaidEmployees = calculations.Count(c => c.Status == CalculationStatus.Paid),
                TotalGrossIncentive = calculations.Sum(c => c.GrossIncentive.Amount),
                TotalNetIncentive = calculations.Sum(c => c.NetIncentive.Amount),
                TotalDeductions = calculations.Sum(c => c.GrossIncentive.Amount - c.NetIncentive.Amount),
                Currency = calculations.FirstOrDefault()?.NetIncentive.Currency ?? "USD",
                AverageIncentive = amounts.Any() ? amounts.Average() : 0,
                MedianIncentive = amounts.Any() ? amounts[amounts.Count / 2] : 0,
                MinIncentive = amounts.Any() ? amounts.Min() : 0,
                MaxIncentive = amounts.Any() ? amounts.Max() : 0
            },
            Details = details
        };
    }

    public async Task<AchievementSummaryDto> GenerateAchievementSummaryAsync(
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

        var achievements = calculations.Select(c => (decimal)c.AchievementPercentage.Value).OrderBy(a => a).ToList();
        var aboveTarget = calculations.Count(c => c.AchievementPercentage.Value > 100);
        var atTarget = calculations.Count(c => Math.Abs(c.AchievementPercentage.Value - 100) <= 5);
        var belowTarget = calculations.Count(c => c.AchievementPercentage.Value < 95);

        var employeeDepts = employees.ToDictionary(e => e.Id, e => e.DepartmentId);
        var deptNames = departments.ToDictionary(d => d.Id, d => d.Name);

        var byDepartment = calculations
            .Where(c => employeeDepts.ContainsKey(c.EmployeeId))
            .GroupBy(c => employeeDepts[c.EmployeeId])
            .Select(g =>
            {
                var dept = departments.FirstOrDefault(d => d.Id == g.Key);
                return new DepartmentAchievementDto
                {
                    DepartmentId = g.Key,
                    DepartmentName = deptNames.GetValueOrDefault(g.Key, "Unknown"),
                    EmployeeCount = g.Count(),
                    AverageAchievement = g.Average(c => (decimal)c.AchievementPercentage.Value),
                    TotalTarget = g.Sum(c => c.TargetValue.Amount),
                    TotalActual = g.Sum(c => c.ActualValue.Amount),
                    TotalIncentive = g.Sum(c => c.NetIncentive.Amount),
                    Currency = g.First().NetIncentive.Currency
                };
            })
            .ToList();

        var planNames = plans.ToDictionary(p => p.Id, p => (p.Code, p.Name));
        var byPlan = calculations
            .GroupBy(c => c.IncentivePlanId)
            .Select(g =>
            {
                var planInfo = planNames.GetValueOrDefault(g.Key, ("N/A", "Unknown"));
                return new PlanAchievementDto
                {
                    PlanId = g.Key,
                    PlanCode = planInfo.Code,
                    PlanName = planInfo.Name,
                    EmployeeCount = g.Count(),
                    AverageAchievement = g.Average(c => (decimal)c.AchievementPercentage.Value),
                    TotalIncentive = g.Sum(c => c.NetIncentive.Amount),
                    Currency = g.First().NetIncentive.Currency
                };
            })
            .ToList();

        var bands = new List<AchievementBandDto>
        {
            CreateBand("Below 50%", 0, 50, calculations),
            CreateBand("50-75%", 50, 75, calculations),
            CreateBand("75-95%", 75, 95, calculations),
            CreateBand("95-105%", 95, 105, calculations),
            CreateBand("105-120%", 105, 120, calculations),
            CreateBand("Above 120%", 120, 1000, calculations)
        };

        return new AchievementSummaryDto
        {
            GeneratedAt = DateTime.UtcNow,
            Period = period,
            PeriodStart = GetPeriodStartDate(period),
            PeriodEnd = GetPeriodEndDate(period),
            Overall = new OverallAchievementDto
            {
                TotalEmployees = calculations.Count,
                AverageAchievement = achievements.Any() ? achievements.Average() : 0,
                MedianAchievement = achievements.Any() ? achievements[achievements.Count / 2] : 0,
                AboveTargetCount = aboveTarget,
                AtTargetCount = atTarget,
                BelowTargetCount = belowTarget,
                AboveTargetPercentage = calculations.Any()
                    ? (decimal)aboveTarget / calculations.Count * 100
                    : 0
            },
            ByDepartment = byDepartment,
            ByPlan = byPlan,
            ByAchievementBand = bands
        };
    }

    public async Task<VarianceAnalysisDto> GenerateVarianceAnalysisAsync(
        string currentPeriod,
        string previousPeriod,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var currentCalcs = await _calculationRepository.GetByPeriodAsync(currentPeriod, cancellationToken);
        var previousCalcs = await _calculationRepository.GetByPeriodAsync(previousPeriod, cancellationToken);
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        var departments = await _departmentRepository.GetAllAsync(cancellationToken);

        if (departmentId.HasValue)
        {
            var deptEmployeeIds = employees
                .Where(e => e.DepartmentId == departmentId.Value)
                .Select(e => e.Id)
                .ToHashSet();
            currentCalcs = currentCalcs.Where(c => deptEmployeeIds.Contains(c.EmployeeId)).ToList();
            previousCalcs = previousCalcs.Where(c => deptEmployeeIds.Contains(c.EmployeeId)).ToList();
        }

        var employeeDict = employees.ToDictionary(e => e.Id);
        var deptDict = departments.ToDictionary(d => d.Id, d => d.Name);

        var previousByEmployee = previousCalcs.ToDictionary(c => c.EmployeeId);

        var variances = currentCalcs
            .Where(c => previousByEmployee.ContainsKey(c.EmployeeId))
            .Select(c =>
            {
                var prev = previousByEmployee[c.EmployeeId];
                var emp = employeeDict.GetValueOrDefault(c.EmployeeId);
                return new EmployeeVarianceDto
                {
                    EmployeeId = c.EmployeeId,
                    EmployeeCode = emp?.EmployeeCode ?? "N/A",
                    EmployeeName = emp?.FullName ?? "Unknown",
                    Department = emp != null && deptDict.TryGetValue(emp.DepartmentId, out var dept) ? dept : "Unknown",
                    CurrentIncentive = c.NetIncentive.Amount,
                    PreviousIncentive = prev.NetIncentive.Amount,
                    AbsoluteVariance = c.NetIncentive.Amount - prev.NetIncentive.Amount,
                    PercentageVariance = prev.NetIncentive.Amount != 0
                        ? (c.NetIncentive.Amount - prev.NetIncentive.Amount) / prev.NetIncentive.Amount * 100
                        : 0,
                    CurrentAchievement = (decimal)c.AchievementPercentage.Value,
                    PreviousAchievement = (decimal)prev.AchievementPercentage.Value,
                    Currency = c.NetIncentive.Currency
                };
            })
            .ToList();

        var topGainers = variances.OrderByDescending(v => v.PercentageVariance).Take(10).ToList();
        var topDecliners = variances.OrderBy(v => v.PercentageVariance).Take(10).ToList();

        var employeeDepts = employees.ToDictionary(e => e.Id, e => e.DepartmentId);
        var currentTotal = currentCalcs.Sum(c => c.NetIncentive.Amount);
        var previousTotal = previousCalcs.Sum(c => c.NetIncentive.Amount);

        var byDepartment = departments.Select(dept =>
        {
            var deptEmployeeIds = employees.Where(e => e.DepartmentId == dept.Id).Select(e => e.Id).ToHashSet();
            var currentDeptCalcs = currentCalcs.Where(c => deptEmployeeIds.Contains(c.EmployeeId)).ToList();
            var previousDeptCalcs = previousCalcs.Where(c => deptEmployeeIds.Contains(c.EmployeeId)).ToList();

            var currentDeptTotal = currentDeptCalcs.Sum(c => c.NetIncentive.Amount);
            var previousDeptTotal = previousDeptCalcs.Sum(c => c.NetIncentive.Amount);

            return new DepartmentVarianceDto
            {
                DepartmentId = dept.Id,
                DepartmentName = dept.Name,
                CurrentTotal = currentDeptTotal,
                PreviousTotal = previousDeptTotal,
                AbsoluteVariance = currentDeptTotal - previousDeptTotal,
                PercentageVariance = previousDeptTotal != 0
                    ? (currentDeptTotal - previousDeptTotal) / previousDeptTotal * 100
                    : 0,
                CurrentEmployeeCount = currentDeptCalcs.Count,
                PreviousEmployeeCount = previousDeptCalcs.Count,
                Currency = currentCalcs.FirstOrDefault()?.NetIncentive.Currency ?? "USD"
            };
        }).ToList();

        return new VarianceAnalysisDto
        {
            GeneratedAt = DateTime.UtcNow,
            CurrentPeriod = currentPeriod,
            PreviousPeriod = previousPeriod,
            Summary = new VarianceSummaryDto
            {
                CurrentPeriodTotal = currentTotal,
                PreviousPeriodTotal = previousTotal,
                AbsoluteVariance = currentTotal - previousTotal,
                PercentageVariance = previousTotal != 0
                    ? (currentTotal - previousTotal) / previousTotal * 100
                    : 0,
                CurrentAverageAchievement = currentCalcs.Any()
                    ? currentCalcs.Average(c => (decimal)c.AchievementPercentage.Value)
                    : 0,
                PreviousAverageAchievement = previousCalcs.Any()
                    ? previousCalcs.Average(c => (decimal)c.AchievementPercentage.Value)
                    : 0,
                AchievementVariance = currentCalcs.Any() && previousCalcs.Any()
                    ? currentCalcs.Average(c => (decimal)c.AchievementPercentage.Value) -
                      previousCalcs.Average(c => (decimal)c.AchievementPercentage.Value)
                    : 0,
                Currency = currentCalcs.FirstOrDefault()?.NetIncentive.Currency ?? "USD"
            },
            TopGainers = topGainers,
            TopDecliners = topDecliners,
            ByDepartment = byDepartment
        };
    }

    public async Task<ForecastReportDto> GenerateForecastAsync(
        int monthsAhead = 6,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var historicalData = new List<decimal>();
        var currentDate = DateTime.UtcNow;

        // Get last 12 months of data
        for (var i = 11; i >= 0; i--)
        {
            var period = currentDate.AddMonths(-i).ToString("yyyy-MM");
            var calculations = await _calculationRepository.GetByPeriodAsync(period, cancellationToken);

            if (departmentId.HasValue)
            {
                var employees = await _employeeRepository.GetAllAsync(cancellationToken);
                var deptEmployeeIds = employees
                    .Where(e => e.DepartmentId == departmentId.Value)
                    .Select(e => e.Id)
                    .ToHashSet();
                calculations = calculations.Where(c => deptEmployeeIds.Contains(c.EmployeeId)).ToList();
            }

            historicalData.Add(calculations.Sum(c => c.NetIncentive.Amount));
        }

        // Simple linear forecast
        var forecasts = new List<ForecastDataPointDto>();
        var avgGrowth = historicalData.Count > 1
            ? (historicalData.Last() - historicalData.First()) / (historicalData.Count - 1)
            : 0;

        var lastValue = historicalData.LastOrDefault();

        for (var i = 1; i <= monthsAhead; i++)
        {
            var projected = lastValue + (avgGrowth * i);
            var confidence = Math.Max(0.5, 1.0 - (i * 0.08)); // Decreasing confidence

            forecasts.Add(new ForecastDataPointDto
            {
                Period = currentDate.AddMonths(i).ToString("yyyy-MM"),
                ProjectedIncentive = Math.Max(0, projected),
                LowerBound = Math.Max(0, projected * 0.85m),
                UpperBound = projected * 1.15m,
                ConfidenceLevel = confidence,
                Currency = "USD"
            });
        }

        var avgHistorical = historicalData.Any() ? historicalData.Average() : 0;
        var trend = avgGrowth >= 0 ? "up" : "down";

        return new ForecastReportDto
        {
            Metadata = new ReportMetadataDto
            {
                ReportName = "Incentive Forecast Report",
                ReportType = ReportTypes.Forecast,
                GeneratedBy = "System",
                Period = currentDate.ToString("yyyy-MM")
            },
            ForecastData = forecasts,
            Summary = new ForecastSummaryDto
            {
                ProjectedAnnualTotal = forecasts.Take(12).Sum(f => f.ProjectedIncentive),
                ProjectedNextQuarter = forecasts.Take(3).Sum(f => f.ProjectedIncentive),
                GrowthTrend = historicalData.Any() && lastValue != 0
                    ? (double)((lastValue - avgHistorical) / avgHistorical * 100)
                    : 0,
                TrendDirection = trend,
                Currency = "USD"
            }
        };
    }

    public async Task<DashboardDto> GenerateDashboardReportAsync(
        string period,
        CancellationToken cancellationToken = default)
    {
        var calculations = await _calculationRepository.GetByPeriodAsync(period, cancellationToken);
        var employees = await _employeeRepository.GetAllAsync(cancellationToken);
        var pendingApprovals = await _approvalRepository.GetPendingAsync(cancellationToken);
        var departments = await _departmentRepository.GetAllAsync(cancellationToken);

        var employeeDict = employees.ToDictionary(e => e.Id);
        var deptDict = departments.ToDictionary(d => d.Id, d => d.Name);

        // Get YTD totals
        var currentYear = DateTime.UtcNow.Year;
        decimal ytdTotal = 0;
        for (var month = 1; month <= DateTime.UtcNow.Month; month++)
        {
            var monthPeriod = $"{currentYear}-{month:D2}";
            var monthCalcs = await _calculationRepository.GetByPeriodAsync(monthPeriod, cancellationToken);
            ytdTotal += monthCalcs.Sum(c => c.NetIncentive.Amount);
        }

        // Monthly trends
        var trends = new List<MonthlyTrendDto>();
        for (var i = 11; i >= 0; i--)
        {
            var monthDate = DateTime.UtcNow.AddMonths(-i);
            var monthPeriod = monthDate.ToString("yyyy-MM");
            var monthCalcs = await _calculationRepository.GetByPeriodAsync(monthPeriod, cancellationToken);

            trends.Add(new MonthlyTrendDto
            {
                Month = monthDate.ToString("MMM"),
                Year = monthDate.Year,
                TotalIncentive = monthCalcs.Sum(c => c.NetIncentive.Amount),
                EmployeeCount = monthCalcs.Select(c => c.EmployeeId).Distinct().Count(),
                AverageAchievement = monthCalcs.Any()
                    ? monthCalcs.Average(c => (decimal)c.AchievementPercentage.Value)
                    : 0,
                Currency = monthCalcs.FirstOrDefault()?.NetIncentive.Currency ?? "USD"
            });
        }

        // Top performers
        var topPerformers = calculations
            .OrderByDescending(c => c.AchievementPercentage.Value)
            .Take(10)
            .Select((c, i) =>
            {
                var emp = employeeDict.GetValueOrDefault(c.EmployeeId);
                return new TopPerformerDto
                {
                    EmployeeId = c.EmployeeId,
                    EmployeeCode = emp?.EmployeeCode ?? "N/A",
                    EmployeeName = emp?.FullName ?? "Unknown",
                    Department = emp != null && deptDict.TryGetValue(emp.DepartmentId, out var dept) ? dept : "Unknown",
                    Achievement = (decimal)c.AchievementPercentage.Value,
                    Incentive = c.NetIncentive.Amount,
                    Currency = c.NetIncentive.Currency,
                    Rank = i + 1
                };
            })
            .ToList();

        return new DashboardDto
        {
            GeneratedAt = DateTime.UtcNow,
            CurrentPeriod = period,
            Kpis = new DashboardKpisDto
            {
                TotalIncentiveYtd = ytdTotal,
                TotalIncentiveCurrentMonth = calculations.Sum(c => c.NetIncentive.Amount),
                TotalEmployees = employees.Count(e => e.Status == EmployeeStatus.Active),
                EligibleEmployees = calculations.Select(c => c.EmployeeId).Distinct().Count(),
                AverageAchievement = calculations.Any()
                    ? calculations.Average(c => (decimal)c.AchievementPercentage.Value)
                    : 0,
                PendingApprovals = pendingApprovals.Count,
                BudgetUtilization = 75, // Placeholder
                Currency = calculations.FirstOrDefault()?.NetIncentive.Currency ?? "USD"
            },
            MonthlyTrend = trends,
            TopPerformers = topPerformers,
            PendingActions = new List<PendingActionDto>
            {
                new()
                {
                    ActionType = "Approvals",
                    Count = pendingApprovals.Count,
                    Description = $"{pendingApprovals.Count} calculations pending approval",
                    Url = "/approvals"
                }
            }
        };
    }

    public async Task<ExportResultDto> ExportReportAsync(
        ReportRequestDto request,
        string format,
        CancellationToken cancellationToken = default)
    {
        var report = await GenerateIncentiveSummaryAsync(request.Period, request.DepartmentId, cancellationToken);

        var content = format switch
        {
            ReportFormats.Csv => GenerateCsv(report),
            ReportFormats.Excel => GenerateExcelPlaceholder(report),
            ReportFormats.Pdf => GeneratePdfPlaceholder(report),
            _ => Encoding.UTF8.GetBytes(JsonSerializer.Serialize(report))
        };

        var contentType = format switch
        {
            ReportFormats.Csv => "text/csv",
            ReportFormats.Excel => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ReportFormats.Pdf => "application/pdf",
            _ => "application/json"
        };

        var extension = format switch
        {
            ReportFormats.Csv => "csv",
            ReportFormats.Excel => "xlsx",
            ReportFormats.Pdf => "pdf",
            _ => "json"
        };

        return new ExportResultDto
        {
            FileName = $"incentive-report-{request.Period}.{extension}",
            ContentType = contentType,
            Content = content,
            RecordCount = report.Details.Count,
            GeneratedAt = DateTime.UtcNow
        };
    }

    public IReadOnlyList<ReportTypeInfoDto> GetAvailableReportTypes()
    {
        return new List<ReportTypeInfoDto>
        {
            new()
            {
                TypeId = ReportTypes.IncentiveSummary,
                Name = "Incentive Summary",
                Description = "Comprehensive summary of incentive payouts",
                SupportedFormats = new[] { "json", "csv", "excel", "pdf" },
                Parameters = new List<ReportParameterDto>
                {
                    new() { Name = "period", Type = "string", Required = true, Description = "Period (YYYY-MM)" },
                    new() { Name = "departmentId", Type = "guid", Required = false, Description = "Filter by department" }
                }
            },
            new()
            {
                TypeId = ReportTypes.Performance,
                Name = "Performance Report",
                Description = "Achievement analysis and performance metrics",
                SupportedFormats = new[] { "json", "csv", "excel" },
                Parameters = new List<ReportParameterDto>
                {
                    new() { Name = "period", Type = "string", Required = true, Description = "Period (YYYY-MM)" }
                }
            },
            new()
            {
                TypeId = ReportTypes.Variance,
                Name = "Variance Analysis",
                Description = "Period-over-period comparison",
                SupportedFormats = new[] { "json", "csv" },
                Parameters = new List<ReportParameterDto>
                {
                    new() { Name = "currentPeriod", Type = "string", Required = true },
                    new() { Name = "previousPeriod", Type = "string", Required = true }
                }
            },
            new()
            {
                TypeId = ReportTypes.Forecast,
                Name = "Forecast Report",
                Description = "Projected incentive trends",
                SupportedFormats = new[] { "json" },
                Parameters = new List<ReportParameterDto>
                {
                    new() { Name = "monthsAhead", Type = "int", Required = false, DefaultValue = "6" }
                }
            }
        };
    }

    public Task<ReportScheduleDto> ScheduleReportAsync(
        ReportScheduleDto schedule,
        CancellationToken cancellationToken = default)
    {
        var newSchedule = schedule with { ScheduleId = Guid.NewGuid(), CreatedAt = DateTime.UtcNow };
        _schedules.Add(newSchedule);
        _logger.LogInformation("Report scheduled: {ScheduleId}", newSchedule.ScheduleId);
        return Task.FromResult(newSchedule);
    }

    public Task<IReadOnlyList<ReportScheduleDto>> GetScheduledReportsAsync(
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyList<ReportScheduleDto>>(_schedules);
    }

    public Task CancelScheduledReportAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default)
    {
        _schedules.RemoveAll(s => s.ScheduleId == scheduleId);
        _logger.LogInformation("Report schedule cancelled: {ScheduleId}", scheduleId);
        return Task.CompletedTask;
    }

    private static byte[] GenerateCsv(PayoutReportDto report)
    {
        var sb = new StringBuilder();
        sb.AppendLine("EmployeeCode,EmployeeName,Department,PlanName,Target,Actual,Achievement%,GrossIncentive,NetIncentive,Status");

        foreach (var detail in report.Details)
        {
            sb.AppendLine($"\"{detail.EmployeeCode}\",\"{detail.EmployeeName}\",\"{detail.Department}\"," +
                         $"\"{detail.PlanName}\",{detail.TargetValue},{detail.ActualValue}," +
                         $"{detail.AchievementPercentage},{detail.GrossIncentive},{detail.NetIncentive}," +
                         $"\"{detail.Status}\"");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    private static byte[] GenerateExcelPlaceholder(PayoutReportDto report)
    {
        // Placeholder - would use a library like ClosedXML or EPPlus
        return GenerateCsv(report);
    }

    private static byte[] GeneratePdfPlaceholder(PayoutReportDto report)
    {
        // Placeholder - would use a library like QuestPDF or iTextSharp
        return Encoding.UTF8.GetBytes($"PDF Report - {report.Details.Count} records");
    }

    private static AchievementBandDto CreateBand(string name, double min, double max, IReadOnlyList<Calculation> calculations)
    {
        var count = calculations.Count(c => c.AchievementPercentage.Value >= min && c.AchievementPercentage.Value < max);
        return new AchievementBandDto
        {
            BandName = name,
            MinPercentage = (decimal)min,
            MaxPercentage = (decimal)max,
            EmployeeCount = count,
            Percentage = calculations.Any() ? (decimal)count / calculations.Count * 100 : 0
        };
    }

    private static string GetReportName(string reportType) => reportType switch
    {
        ReportTypes.IncentiveSummary => "Incentive Summary Report",
        ReportTypes.Performance => "Performance Report",
        ReportTypes.TrendAnalysis => "Trend Analysis Report",
        ReportTypes.Variance => "Variance Analysis Report",
        ReportTypes.Forecast => "Forecast Report",
        _ => "Report"
    };

    private static string GetPreviousPeriod(string period)
    {
        if (DateTime.TryParseExact(period + "-01", "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var date))
        {
            return date.AddMonths(-1).ToString("yyyy-MM");
        }
        return DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM");
    }

    private static DateTime GetPeriodStartDate(string period)
    {
        if (DateTime.TryParseExact(period + "-01", "yyyy-MM-dd",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.None, out var date))
        {
            return date;
        }
        return new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
    }

    private static DateTime GetPeriodEndDate(string period)
    {
        var start = GetPeriodStartDate(period);
        return start.AddMonths(1).AddDays(-1);
    }
}
