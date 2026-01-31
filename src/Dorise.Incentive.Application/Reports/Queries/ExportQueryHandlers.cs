using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.Reports.DTOs;
using Dorise.Incentive.Application.Reports.Services;
using Dorise.Incentive.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

using IMediator = MediatR.IMediator;

namespace Dorise.Incentive.Application.Reports.Queries;

/// <summary>
/// Handler for ExportPayoutReportQuery.
/// "I ated the purple berries!" - Export your data deliciously!
/// </summary>
public class ExportPayoutReportQueryHandler : IQueryHandler<ExportPayoutReportQuery, ExportResultDto>
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    private readonly ILogger<ExportPayoutReportQueryHandler> _logger;

    public ExportPayoutReportQueryHandler(
        IMediator mediator,
        IExportService exportService,
        ILogger<ExportPayoutReportQueryHandler> logger)
    {
        _mediator = mediator;
        _exportService = exportService;
        _logger = logger;
    }

    public async Task<Result<ExportResultDto>> Handle(
        ExportPayoutReportQuery request,
        CancellationToken cancellationToken)
    {
        // First get the report data
        var reportQuery = new GetPayoutReportQuery(
            request.PeriodStart,
            request.PeriodEnd,
            request.DepartmentId,
            request.PlanId);

        var reportResult = await _mediator.Send(reportQuery, cancellationToken);

        if (reportResult.IsFailure)
        {
            return Result<ExportResultDto>.Failure(reportResult.Error!, reportResult.ErrorCode);
        }

        // Export to requested format
        var exportResult = await _exportService.ExportPayoutReportAsync(
            reportResult.Value!,
            request.Format,
            cancellationToken);

        _logger.LogInformation(
            "Exported payout report with {Count} records to {Format}",
            exportResult.RecordCount,
            request.Format);

        return Result<ExportResultDto>.Success(exportResult);
    }
}

/// <summary>
/// Handler for ExportAchievementSummaryQuery.
/// </summary>
public class ExportAchievementSummaryQueryHandler : IQueryHandler<ExportAchievementSummaryQuery, ExportResultDto>
{
    private readonly IMediator _mediator;
    private readonly IExportService _exportService;
    private readonly ILogger<ExportAchievementSummaryQueryHandler> _logger;

    public ExportAchievementSummaryQueryHandler(
        IMediator mediator,
        IExportService exportService,
        ILogger<ExportAchievementSummaryQueryHandler> logger)
    {
        _mediator = mediator;
        _exportService = exportService;
        _logger = logger;
    }

    public async Task<Result<ExportResultDto>> Handle(
        ExportAchievementSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var reportQuery = new GetAchievementSummaryQuery(
            request.PeriodStart,
            request.PeriodEnd,
            request.DepartmentId);

        var reportResult = await _mediator.Send(reportQuery, cancellationToken);

        if (reportResult.IsFailure)
        {
            return Result<ExportResultDto>.Failure(reportResult.Error!, reportResult.ErrorCode);
        }

        var exportResult = await _exportService.ExportAchievementSummaryAsync(
            reportResult.Value!,
            request.Format,
            cancellationToken);

        _logger.LogInformation(
            "Exported achievement summary to {Format}",
            request.Format);

        return Result<ExportResultDto>.Success(exportResult);
    }
}

/// <summary>
/// Handler for ExportCalculationsQuery.
/// </summary>
public class ExportCalculationsQueryHandler : IQueryHandler<ExportCalculationsQuery, ExportResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IExportService _exportService;
    private readonly ILogger<ExportCalculationsQueryHandler> _logger;

    public ExportCalculationsQueryHandler(
        IUnitOfWork unitOfWork,
        IExportService exportService,
        ILogger<ExportCalculationsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _exportService = exportService;
        _logger = logger;
    }

    public async Task<Result<ExportResultDto>> Handle(
        ExportCalculationsQuery request,
        CancellationToken cancellationToken)
    {
        var calculations = await _unitOfWork.Calculations.GetByPeriodAsync(
            request.PeriodStart,
            request.PeriodEnd,
            cancellationToken: cancellationToken);

        // Filter by department if specified
        if (request.DepartmentId.HasValue)
        {
            var employeeIds = (await _unitOfWork.Employees.GetByDepartmentAsync(
                request.DepartmentId.Value,
                cancellationToken: cancellationToken)).Select(e => e.Id).ToHashSet();

            calculations = calculations.Where(c => employeeIds.Contains(c.EmployeeId)).ToList();
        }

        // Filter by plan if specified
        if (request.PlanId.HasValue)
        {
            calculations = calculations.Where(c => c.IncentivePlanId == request.PlanId.Value).ToList();
        }

        // Build export data
        var exportData = new List<CalculationExportRow>();

        foreach (var calc in calculations)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(calc.EmployeeId, cancellationToken);
            var plan = await _unitOfWork.IncentivePlans.GetByIdAsync(calc.IncentivePlanId, cancellationToken);
            var department = employee != null
                ? await _unitOfWork.Departments.GetByIdAsync(employee.DepartmentId, cancellationToken)
                : null;

            exportData.Add(new CalculationExportRow
            {
                CalculationId = calc.Id.ToString(),
                EmployeeCode = employee?.EmployeeCode ?? "Unknown",
                EmployeeName = employee != null ? $"{employee.FirstName} {employee.LastName}" : "Unknown",
                Department = department?.Name ?? "Unknown",
                PlanCode = plan?.Code ?? "Unknown",
                PlanName = plan?.Name ?? "Unknown",
                PeriodStart = calc.CalculationPeriod.StartDate.ToString("yyyy-MM-dd"),
                PeriodEnd = calc.CalculationPeriod.EndDate.ToString("yyyy-MM-dd"),
                TargetValue = calc.TargetValue,
                ActualValue = calc.ActualValue,
                AchievementPercentage = calc.AchievementPercentage.Value,
                GrossIncentive = calc.GrossIncentive.Amount,
                NetIncentive = calc.NetIncentive.Amount,
                Currency = calc.NetIncentive.Currency,
                Status = calc.Status.ToString(),
                CalculatedAt = calc.CalculatedAt.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }

        var fileName = $"Calculations_{request.PeriodStart:yyyyMM}_{request.PeriodEnd:yyyyMM}";

        var exportResult = request.Format switch
        {
            ExportFormat.Csv => await _exportService.ExportToCsvAsync(exportData, fileName, cancellationToken),
            ExportFormat.Excel => await _exportService.ExportToExcelAsync(exportData, fileName, "Calculations", cancellationToken),
            _ => throw new ArgumentException($"Unsupported format: {request.Format}")
        };

        _logger.LogInformation(
            "Exported {Count} calculations to {Format}",
            exportData.Count,
            request.Format);

        return Result<ExportResultDto>.Success(exportResult);
    }
}

/// <summary>
/// Handler for GetPeriodComparisonQuery.
/// </summary>
public class GetPeriodComparisonQueryHandler : IQueryHandler<GetPeriodComparisonQuery, PeriodComparisonDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPeriodComparisonQueryHandler> _logger;

    public GetPeriodComparisonQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetPeriodComparisonQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<PeriodComparisonDto>> Handle(
        GetPeriodComparisonQuery request,
        CancellationToken cancellationToken)
    {
        var periodData = new List<PeriodDataDto>();

        foreach (var period in request.Periods)
        {
            var calculations = await _unitOfWork.Calculations.GetByPeriodAsync(
                period.StartDate,
                period.EndDate,
                cancellationToken: cancellationToken);

            // Filter by department if specified
            if (request.DepartmentId.HasValue)
            {
                var employeeIds = (await _unitOfWork.Employees.GetByDepartmentAsync(
                    request.DepartmentId.Value,
                    cancellationToken: cancellationToken)).Select(e => e.Id).ToHashSet();

                calculations = calculations.Where(c => employeeIds.Contains(c.EmployeeId)).ToList();
            }

            var totalIncentive = calculations.Sum(c => c.NetIncentive.Amount);
            var avgIncentive = calculations.Any() ? calculations.Average(c => c.NetIncentive.Amount) : 0;
            var avgAchievement = calculations.Any() ? calculations.Average(c => c.AchievementPercentage.Value) : 0;

            periodData.Add(new PeriodDataDto
            {
                Label = period.Label,
                StartDate = period.StartDate,
                EndDate = period.EndDate,
                EmployeeCount = calculations.Count,
                TotalIncentive = totalIncentive,
                AverageIncentive = avgIncentive,
                AverageAchievement = avgAchievement,
                Currency = "INR"
            });
        }

        _logger.LogInformation(
            "Generated period comparison for {Count} periods",
            request.Periods.Count);

        return Result<PeriodComparisonDto>.Success(new PeriodComparisonDto
        {
            GeneratedAt = DateTime.UtcNow,
            Periods = periodData
        });
    }
}

/// <summary>
/// Row structure for calculation export.
/// </summary>
internal record CalculationExportRow
{
    public string CalculationId { get; init; } = null!;
    public string EmployeeCode { get; init; } = null!;
    public string EmployeeName { get; init; } = null!;
    public string Department { get; init; } = null!;
    public string PlanCode { get; init; } = null!;
    public string PlanName { get; init; } = null!;
    public string PeriodStart { get; init; } = null!;
    public string PeriodEnd { get; init; } = null!;
    public decimal TargetValue { get; init; }
    public decimal ActualValue { get; init; }
    public decimal AchievementPercentage { get; init; }
    public decimal GrossIncentive { get; init; }
    public decimal NetIncentive { get; init; }
    public string Currency { get; init; } = null!;
    public string Status { get; init; } = null!;
    public string CalculatedAt { get; init; } = null!;
}
