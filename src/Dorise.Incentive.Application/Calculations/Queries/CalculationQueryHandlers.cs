using AutoMapper;
using Dorise.Incentive.Application.Calculations.DTOs;
using Dorise.Incentive.Application.Common;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.Calculations.Queries;

/// <summary>
/// Handler for GetCalculationsQuery.
/// "Me fail English? That's unpossible!" - Queries always return results!
/// </summary>
public class GetCalculationsQueryHandler : IQueryHandler<GetCalculationsQuery, PagedResult<CalculationSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCalculationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<CalculationSummaryDto>>> Handle(
        GetCalculationsQuery request,
        CancellationToken cancellationToken)
    {
        var calculations = await _unitOfWork.Calculations.GetPagedAsync(
            request.Page,
            request.PageSize,
            request.Period,
            request.EmployeeId,
            request.IncentivePlanId,
            request.Status,
            request.SortBy,
            request.SortDescending,
            cancellationToken);

        var items = _mapper.Map<IReadOnlyList<CalculationSummaryDto>>(calculations.Items);

        var result = new PagedResult<CalculationSummaryDto>(
            items,
            calculations.TotalCount,
            request.Page,
            request.PageSize);

        return Result<PagedResult<CalculationSummaryDto>>.Success(result);
    }
}

/// <summary>
/// Handler for GetCalculationByIdQuery.
/// </summary>
public class GetCalculationByIdQueryHandler : IQueryHandler<GetCalculationByIdQuery, CalculationDto?>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetCalculationByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<CalculationDto?>> Handle(
        GetCalculationByIdQuery request,
        CancellationToken cancellationToken)
    {
        var calculation = await _unitOfWork.Calculations.GetWithDetailsAsync(
            request.CalculationId, cancellationToken);

        if (calculation == null)
        {
            return Result<CalculationDto?>.Success(null);
        }

        return Result<CalculationDto?>.Success(_mapper.Map<CalculationDto>(calculation));
    }
}

/// <summary>
/// Handler for GetEmployeeCalculationsQuery.
/// </summary>
public class GetEmployeeCalculationsQueryHandler : IQueryHandler<GetEmployeeCalculationsQuery, IReadOnlyList<CalculationSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetEmployeeCalculationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<IReadOnlyList<CalculationSummaryDto>>> Handle(
        GetEmployeeCalculationsQuery request,
        CancellationToken cancellationToken)
    {
        var calculations = await _unitOfWork.Calculations.GetByEmployeeAsync(
            request.EmployeeId,
            request.PeriodStart,
            request.PeriodEnd,
            request.Status,
            cancellationToken);

        var result = _mapper.Map<IReadOnlyList<CalculationSummaryDto>>(calculations);
        return Result<IReadOnlyList<CalculationSummaryDto>>.Success(result);
    }
}

/// <summary>
/// Handler for GetCalculationPeriodSummaryQuery.
/// "Super Nintendo Chalmers!" - Super aggregations!
/// </summary>
public class GetCalculationPeriodSummaryQueryHandler : IQueryHandler<GetCalculationPeriodSummaryQuery, CalculationPeriodSummaryDto>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCalculationPeriodSummaryQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CalculationPeriodSummaryDto>> Handle(
        GetCalculationPeriodSummaryQuery request,
        CancellationToken cancellationToken)
    {
        var calculations = await _unitOfWork.Calculations.GetByPeriodAsync(
            request.PeriodStart,
            request.PeriodEnd,
            request.DepartmentId,
            request.IncentivePlanId,
            cancellationToken);

        var calculationList = calculations.ToList();

        // Status counts
        var statusGroups = calculationList.GroupBy(c => c.Status).ToDictionary(g => g.Key, g => g.Count());

        // Plan summaries
        var planGroups = calculationList
            .GroupBy(c => new { c.IncentivePlanId, c.IncentivePlan?.Name })
            .Select(g => new PlanSummaryDto
            {
                PlanId = g.Key.IncentivePlanId,
                PlanName = g.Key.Name ?? "Unknown",
                CalculationCount = g.Count(),
                TotalIncentive = g.Sum(c => c.NetIncentive.Amount),
                AverageAchievement = g.Average(c => c.AchievementPercentage.Value)
            })
            .ToList();

        var summary = new CalculationPeriodSummaryDto
        {
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            TotalCalculations = calculationList.Count,
            PendingCount = statusGroups.GetValueOrDefault(CalculationStatus.Pending, 0),
            CalculatedCount = statusGroups.GetValueOrDefault(CalculationStatus.Calculated, 0),
            PendingApprovalCount = statusGroups.GetValueOrDefault(CalculationStatus.PendingApproval, 0),
            ApprovedCount = statusGroups.GetValueOrDefault(CalculationStatus.Approved, 0),
            RejectedCount = statusGroups.GetValueOrDefault(CalculationStatus.Rejected, 0),
            PaidCount = statusGroups.GetValueOrDefault(CalculationStatus.Paid, 0),
            VoidedCount = statusGroups.GetValueOrDefault(CalculationStatus.Voided, 0),
            TotalGrossIncentive = calculationList.Sum(c => c.GrossIncentive.Amount),
            TotalNetIncentive = calculationList.Sum(c => c.NetIncentive.Amount),
            AverageAchievement = calculationList.Any() ? calculationList.Average(c => c.AchievementPercentage.Value) : 0,
            Currency = calculationList.FirstOrDefault()?.NetIncentive.Currency ?? "INR",
            ByPlan = planGroups
        };

        return Result<CalculationPeriodSummaryDto>.Success(summary);
    }
}

/// <summary>
/// Handler for PreviewCalculationQuery.
/// "That's where I saw the leprechaun!" - Preview before commit!
/// </summary>
public class PreviewCalculationQueryHandler : IQueryHandler<PreviewCalculationQuery, CalculationPreviewDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IIncentiveCalculationService _calculationService;
    private readonly IEligibilityService _eligibilityService;
    private readonly ILogger<PreviewCalculationQueryHandler> _logger;

    public PreviewCalculationQueryHandler(
        IUnitOfWork unitOfWork,
        IIncentiveCalculationService calculationService,
        IEligibilityService eligibilityService,
        ILogger<PreviewCalculationQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _calculationService = calculationService;
        _eligibilityService = eligibilityService;
        _logger = logger;
    }

    public async Task<Result<CalculationPreviewDto>> Handle(
        PreviewCalculationQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await _unitOfWork.Employees.GetByIdAsync(request.EmployeeId, cancellationToken);
        if (employee == null)
        {
            return Result<CalculationPreviewDto>.NotFound("Employee", request.EmployeeId);
        }

        var plan = await _unitOfWork.IncentivePlans.GetWithSlabsAsync(request.IncentivePlanId, cancellationToken);
        if (plan == null)
        {
            return Result<CalculationPreviewDto>.NotFound("IncentivePlan", request.IncentivePlanId);
        }

        // Check eligibility
        var eligibility = _eligibilityService.CheckEligibility(employee, plan, request.PeriodEnd);

        // Run calculation
        var calcResult = _calculationService.Calculate(
            employee,
            plan,
            request.ActualValue,
            Domain.ValueObjects.DateRange.Create(request.PeriodStart, request.PeriodEnd));

        var breakdown = new List<CalculationBreakdownDto>
        {
            new() { Component = "Actual Sales", Description = "Reported sales value", Value = request.ActualValue },
            new() { Component = "Target", Description = "Plan target value", Value = plan.Target.Value },
            new()
            {
                Component = "Achievement",
                Description = "Achievement percentage",
                Value = calcResult.Achievement.Value,
                Formula = $"{request.ActualValue} / {plan.Target.Value} × 100"
            },
            new() { Component = "Gross Incentive", Description = "Before adjustments", Value = calcResult.GrossIncentive.Amount },
        };

        // Check proration
        decimal? prorataFactor = null;
        var netIncentive = calcResult.NetIncentive.Amount;

        if (eligibility.ProrataFactor.Value < 100)
        {
            prorataFactor = eligibility.ProrataFactor.Value;
            netIncentive = netIncentive * prorataFactor.Value / 100;
            breakdown.Add(new()
            {
                Component = "Proration",
                Description = $"Prorated at {prorataFactor:F2}%",
                Value = prorataFactor.Value,
                Formula = $"{calcResult.NetIncentive.Amount} × {prorataFactor:F2}%"
            });
        }

        // Check cap
        var isCapped = false;
        decimal? capAmount = null;
        if (plan.MaximumPayout != null && netIncentive > plan.MaximumPayout.Amount)
        {
            isCapped = true;
            capAmount = plan.MaximumPayout.Amount;
            netIncentive = capAmount.Value;
            breakdown.Add(new()
            {
                Component = "Cap Applied",
                Description = "Maximum payout cap",
                Value = capAmount.Value
            });
        }

        breakdown.Add(new() { Component = "Net Incentive", Description = "Final payout amount", Value = netIncentive });

        var preview = new CalculationPreviewDto
        {
            EmployeeId = employee.Id,
            EmployeeCode = employee.EmployeeCode,
            EmployeeName = employee.FullName,
            IncentivePlanId = plan.Id,
            PlanName = plan.Name,
            PeriodStart = request.PeriodStart,
            PeriodEnd = request.PeriodEnd,
            TargetValue = plan.Target.Value,
            ActualValue = request.ActualValue,
            AchievementPercentage = calcResult.Achievement.Value,
            GrossIncentive = calcResult.GrossIncentive.Amount,
            NetIncentive = netIncentive,
            Currency = calcResult.NetIncentive.Currency,
            AppliedSlabId = calcResult.AppliedSlab?.Id,
            AppliedSlabDescription = calcResult.AppliedSlab?.Name,
            ProrataFactor = prorataFactor,
            IsCapped = isCapped,
            CapAmount = capAmount,
            IsBelowThreshold = !plan.Target.MeetsMinimumThreshold(request.ActualValue),
            IsEligible = eligibility.IsEligible,
            IneligibilityReason = eligibility.IsEligible ? null : string.Join("; ", eligibility.Reasons),
            Breakdown = breakdown
        };

        _logger.LogDebug(
            "Preview calculated for employee {EmployeeId}: Achievement={Achievement}%, NetIncentive={Amount}",
            employee.Id, preview.AchievementPercentage, preview.NetIncentive);

        return Result<CalculationPreviewDto>.Success(preview);
    }
}

/// <summary>
/// Handler for GetPendingApprovalsQuery.
/// </summary>
public class GetPendingApprovalsQueryHandler : IQueryHandler<GetPendingApprovalsQuery, PagedResult<CalculationSummaryDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public GetPendingApprovalsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<Result<PagedResult<CalculationSummaryDto>>> Handle(
        GetPendingApprovalsQuery request,
        CancellationToken cancellationToken)
    {
        var calculations = await _unitOfWork.Calculations.GetPendingApprovalsAsync(
            request.ApproverId,
            request.Page,
            request.PageSize,
            cancellationToken);

        var items = _mapper.Map<IReadOnlyList<CalculationSummaryDto>>(calculations.Items);

        var result = new PagedResult<CalculationSummaryDto>(
            items,
            calculations.TotalCount,
            request.Page,
            request.PageSize);

        return Result<PagedResult<CalculationSummaryDto>>.Success(result);
    }
}

/// <summary>
/// Handler for GetCalculationHistoryQuery.
/// </summary>
public class GetCalculationHistoryQueryHandler : IQueryHandler<GetCalculationHistoryQuery, IReadOnlyList<CalculationAuditDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCalculationHistoryQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<IReadOnlyList<CalculationAuditDto>>> Handle(
        GetCalculationHistoryQuery request,
        CancellationToken cancellationToken)
    {
        var calculation = await _unitOfWork.Calculations.GetWithDetailsAsync(
            request.CalculationId, cancellationToken);

        if (calculation == null)
        {
            return Result<IReadOnlyList<CalculationAuditDto>>.NotFound("Calculation", request.CalculationId);
        }

        // Build audit trail from calculation history
        var auditTrail = new List<CalculationAuditDto>
        {
            new()
            {
                Timestamp = calculation.CreatedAt,
                Action = "Created",
                PerformedBy = calculation.CalculatedBy ?? "System",
                Details = $"Initial calculation created for period {calculation.CalculationPeriod.StartDate:d} - {calculation.CalculationPeriod.EndDate:d}",
                NewStatus = CalculationStatus.Pending,
                NewAmount = calculation.GrossIncentive.Amount
            }
        };

        // Add approval history
        if (calculation.Approvals != null)
        {
            foreach (var approval in calculation.Approvals.OrderBy(a => a.ActionDate ?? a.CreatedAt))
            {
                auditTrail.Add(new CalculationAuditDto
                {
                    Timestamp = approval.ActionDate ?? approval.CreatedAt,
                    Action = approval.Status.ToString(),
                    PerformedBy = approval.ApproverEmail ?? "Unknown",
                    Details = approval.Comments,
                    NewStatus = approval.Status == ApprovalStatus.Approved
                        ? CalculationStatus.Approved
                        : approval.Status == ApprovalStatus.Rejected
                            ? CalculationStatus.Rejected
                            : null
                });
            }
        }

        // Add adjustment if present
        if (!string.IsNullOrEmpty(calculation.AdjustmentReason))
        {
            auditTrail.Add(new CalculationAuditDto
            {
                Timestamp = calculation.ModifiedAt ?? calculation.CreatedAt,
                Action = "Adjusted",
                PerformedBy = "System",
                Details = calculation.AdjustmentReason,
                NewAmount = calculation.NetIncentive.Amount
            });
        }

        // Add void if present
        if (calculation.Status == CalculationStatus.Voided)
        {
            auditTrail.Add(new CalculationAuditDto
            {
                Timestamp = calculation.ModifiedAt ?? calculation.CreatedAt,
                Action = "Voided",
                PerformedBy = calculation.VoidedBy ?? "System",
                Details = calculation.VoidReason,
                NewStatus = CalculationStatus.Voided
            });
        }

        return Result<IReadOnlyList<CalculationAuditDto>>.Success(
            auditTrail.OrderBy(a => a.Timestamp).ToList());
    }
}
