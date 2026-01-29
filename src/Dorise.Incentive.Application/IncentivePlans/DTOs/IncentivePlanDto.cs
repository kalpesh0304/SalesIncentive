using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Application.IncentivePlans.DTOs;

/// <summary>
/// Data transfer object for IncentivePlan.
/// "I'm pedaling backwards!" - But plans move forward!
/// </summary>
public record IncentivePlanDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public PlanType PlanType { get; init; }
    public string PlanTypeDisplay { get; init; } = null!;
    public PlanStatus Status { get; init; }
    public string StatusDisplay { get; init; } = null!;
    public PaymentFrequency Frequency { get; init; }
    public string FrequencyDisplay { get; init; } = null!;
    public DateTime EffectiveFrom { get; init; }
    public DateTime EffectiveTo { get; init; }

    // Target
    public decimal TargetValue { get; init; }
    public decimal MinimumThreshold { get; init; }
    public AchievementType AchievementType { get; init; }
    public string? MetricUnit { get; init; }

    // Limits
    public decimal? MaximumPayout { get; init; }
    public decimal? MinimumPayout { get; init; }
    public string Currency { get; init; } = "INR";

    // Approval
    public bool RequiresApproval { get; init; }
    public int ApprovalLevels { get; init; }

    public string? EligibilityCriteria { get; init; }
    public int Version { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? ModifiedAt { get; init; }

    public IReadOnlyList<SlabDto> Slabs { get; init; } = Array.Empty<SlabDto>();
}

/// <summary>
/// Summary DTO for plan lists.
/// </summary>
public record IncentivePlanSummaryDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public PlanType PlanType { get; init; }
    public PlanStatus Status { get; init; }
    public PaymentFrequency Frequency { get; init; }
    public DateTime EffectiveFrom { get; init; }
    public DateTime EffectiveTo { get; init; }
    public int AssignedEmployeesCount { get; init; }
}

/// <summary>
/// DTO for slab information.
/// </summary>
public record SlabDto
{
    public Guid Id { get; init; }
    public int Order { get; init; }
    public decimal FromPercentage { get; init; }
    public decimal ToPercentage { get; init; }
    public decimal PayoutRate { get; init; }
    public string? Description { get; init; }
}

/// <summary>
/// Request DTO for creating a plan.
/// </summary>
public record CreateIncentivePlanRequest
{
    public string Code { get; init; } = null!;
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public PlanType PlanType { get; init; }
    public PaymentFrequency Frequency { get; init; }
    public DateTime EffectiveFrom { get; init; }
    public DateTime EffectiveTo { get; init; }
    public decimal TargetValue { get; init; }
    public decimal MinimumThreshold { get; init; }
    public AchievementType AchievementType { get; init; }
    public string? MetricUnit { get; init; }
    public decimal? MaximumPayout { get; init; }
    public decimal? MinimumPayout { get; init; }
    public string Currency { get; init; } = "INR";
    public bool RequiresApproval { get; init; } = true;
    public int ApprovalLevels { get; init; } = 1;
    public IReadOnlyList<CreateSlabRequest>? Slabs { get; init; }
}

/// <summary>
/// Request DTO for creating a slab.
/// </summary>
public record CreateSlabRequest
{
    public decimal FromPercentage { get; init; }
    public decimal ToPercentage { get; init; }
    public decimal PayoutRate { get; init; }
    public string? Description { get; init; }
}
