using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.IncentivePlans.DTOs;
using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Application.IncentivePlans.Commands.CreatePlan;

/// <summary>
/// Command to create a new incentive plan.
/// "I'm a brick!" - And plans are the building blocks of incentives!
/// </summary>
public record CreateIncentivePlanCommand(
    string Code,
    string Name,
    string? Description,
    PlanType PlanType,
    PaymentFrequency Frequency,
    DateTime EffectiveFrom,
    DateTime EffectiveTo,
    decimal TargetValue,
    decimal MinimumThreshold,
    AchievementType AchievementType,
    string? MetricUnit,
    decimal? MaximumPayout,
    decimal? MinimumPayout,
    string Currency,
    bool RequiresApproval,
    int ApprovalLevels,
    int? MinimumTenureDays,
    string? EligibilityCriteria,
    IReadOnlyList<CreateSlabDto>? Slabs = null) : ICommand<IncentivePlanDto>;

/// <summary>
/// DTO for creating a slab within a plan.
/// </summary>
public record CreateSlabDto(
    string Name,
    string? Description,
    int Order,
    decimal FromPercentage,
    decimal ToPercentage,
    decimal PayoutPercentage,
    decimal? FixedAmount = null);
