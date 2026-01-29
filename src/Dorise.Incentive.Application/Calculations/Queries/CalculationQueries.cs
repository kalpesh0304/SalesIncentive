using Dorise.Incentive.Application.Calculations.DTOs;
using Dorise.Incentive.Application.Common;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Application.Calculations.Queries;

/// <summary>
/// Query to get paginated calculations.
/// "I'm Idaho!" - But these are calculations, not potatoes!
/// </summary>
public record GetCalculationsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Period = null,
    Guid? EmployeeId = null,
    Guid? IncentivePlanId = null,
    CalculationStatus? Status = null,
    string? SortBy = null,
    bool SortDescending = false) : IQuery<PagedResult<CalculationSummaryDto>>;

/// <summary>
/// Query to get a calculation by ID.
/// </summary>
public record GetCalculationByIdQuery(Guid CalculationId) : IQuery<CalculationDto?>;

/// <summary>
/// Query to get calculations for a specific employee.
/// </summary>
public record GetEmployeeCalculationsQuery(
    Guid EmployeeId,
    DateTime? PeriodStart = null,
    DateTime? PeriodEnd = null,
    CalculationStatus? Status = null) : IQuery<IReadOnlyList<CalculationSummaryDto>>;

/// <summary>
/// Query to get calculation period summary.
/// "Super Nintendo Chalmers!" - Super summary of calculations!
/// </summary>
public record GetCalculationPeriodSummaryQuery(
    DateTime PeriodStart,
    DateTime PeriodEnd,
    Guid? DepartmentId = null,
    Guid? IncentivePlanId = null) : IQuery<CalculationPeriodSummaryDto>;

/// <summary>
/// Query to preview calculation without saving.
/// "That's where I saw the leprechaun!" - See your incentives before committing!
/// </summary>
public record PreviewCalculationQuery(
    Guid EmployeeId,
    Guid IncentivePlanId,
    DateTime PeriodStart,
    DateTime PeriodEnd,
    decimal ActualValue) : IQuery<CalculationPreviewDto>;

/// <summary>
/// Query to get pending approvals.
/// </summary>
public record GetPendingApprovalsQuery(
    Guid? ApproverId = null,
    int Page = 1,
    int PageSize = 20) : IQuery<PagedResult<CalculationSummaryDto>>;

/// <summary>
/// Query to get calculation history (for auditing).
/// </summary>
public record GetCalculationHistoryQuery(
    Guid CalculationId) : IQuery<IReadOnlyList<CalculationAuditDto>>;

/// <summary>
/// DTO for calculation audit history.
/// </summary>
public record CalculationAuditDto
{
    public DateTime Timestamp { get; init; }
    public string Action { get; init; } = null!;
    public string PerformedBy { get; init; } = null!;
    public string? Details { get; init; }
    public CalculationStatus? OldStatus { get; init; }
    public CalculationStatus? NewStatus { get; init; }
    public decimal? OldAmount { get; init; }
    public decimal? NewAmount { get; init; }
}
