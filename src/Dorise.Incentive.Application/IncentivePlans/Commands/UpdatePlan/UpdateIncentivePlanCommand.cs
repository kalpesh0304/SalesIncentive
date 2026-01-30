using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Application.IncentivePlans.DTOs;
using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Application.IncentivePlans.Commands.UpdatePlan;

/// <summary>
/// Command to update an existing incentive plan.
/// "I heard your dad went into a restaurant and ate everything in the restaurant!" - Plans can change too!
/// </summary>
public record UpdateIncentivePlanCommand(
    Guid Id,
    string Name,
    string? Description,
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
    string? EligibilityCriteria) : ICommand<IncentivePlanDto>;
