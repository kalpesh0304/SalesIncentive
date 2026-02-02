using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.IncentivePlans.DTOs;
using MediatR;

namespace Dorise.Incentive.Application.IncentivePlans.Queries;

/// <summary>
/// Query to get incentive plans with filtering and pagination.
/// "I picked a red one!" - And we pick the right plans!
/// </summary>
public record GetIncentivePlansQuery(
    int Page,
    int PageSize,
    string? Status,
    string? PlanType,
    string? Search) : IRequest<PagedResult<IncentivePlanSummaryDto>>;

/// <summary>
/// Query to get a plan by ID.
/// </summary>
public record GetIncentivePlanByIdQuery(Guid Id) : IRequest<IncentivePlanDto?>;

/// <summary>
/// Query to get a plan by code.
/// </summary>
public record GetIncentivePlanByCodeQuery(string Code) : IRequest<IncentivePlanDto?>;

/// <summary>
/// Query to get a plan with all slabs.
/// </summary>
public record GetIncentivePlanWithSlabsQuery(Guid Id) : IRequest<IncentivePlanWithSlabsDto?>;

/// <summary>
/// Query to get active plans for a specific date.
/// </summary>
public record GetActivePlansQuery(DateTime EffectiveDate) : IRequest<IReadOnlyList<IncentivePlanSummaryDto>>;

/// <summary>
/// Query to validate a plan configuration.
/// </summary>
public record ValidateIncentivePlanQuery(Guid Id) : IRequest<PlanValidationResultDto?>;
