using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Application.IncentivePlans.DTOs;

/// <summary>
/// Full DTO for Incentive Plan.
/// "That's where I saw the leprechaun!" - And this is where we see the plan details!
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

    // Effective Period
    public DateTime EffectiveFrom { get; init; }
    public DateTime EffectiveTo { get; init; }
    public bool IsCurrentlyEffective { get; init; }

    // Target Configuration
    public decimal TargetValue { get; init; }
    public decimal MinimumThreshold { get; init; }
    public AchievementType AchievementType { get; init; }
    public string? MetricUnit { get; init; }

    // Payout Configuration
    public decimal? MaximumPayout { get; init; }
    public decimal? MinimumPayout { get; init; }
    public string Currency { get; init; } = "INR";

    // Approval Settings
    public bool RequiresApproval { get; init; }
    public int ApprovalLevels { get; init; }

    // Eligibility
    public int? MinimumTenureDays { get; init; }
    public string? EligibilityCriteria { get; init; }

    // Metadata
    public int Version { get; init; }
    public int SlabCount { get; init; }
    public int AssignmentCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? ModifiedAt { get; init; }
    public string? ModifiedBy { get; init; }

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
    public decimal TargetValue { get; init; }
    public int AssignmentCount { get; init; }
}

/// <summary>
/// Plan with all slabs included.
/// </summary>
public record IncentivePlanWithSlabsDto : IncentivePlanDto
{
    public new IReadOnlyList<SlabDto> Slabs { get; init; } = Array.Empty<SlabDto>();
}

/// <summary>
/// DTO for Slab configuration.
/// </summary>
public record SlabDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = null!;
    public string? Description { get; init; }
    public int Order { get; init; }
    public decimal FromPercentage { get; init; }
    public decimal ToPercentage { get; init; }
    public decimal PayoutPercentage { get; init; }
    public decimal? FixedAmount { get; init; }
    public string Currency { get; init; } = "INR";
    public bool IsActive { get; init; }
}

/// <summary>
/// Result of plan validation.
/// </summary>
public record PlanValidationResultDto
{
    public Guid PlanId { get; init; }
    public bool IsValid { get; init; }
    public bool CanBeActivated { get; init; }
    public IReadOnlyList<ValidationIssueDto> Errors { get; init; } = Array.Empty<ValidationIssueDto>();
    public IReadOnlyList<ValidationIssueDto> Warnings { get; init; } = Array.Empty<ValidationIssueDto>();
}

/// <summary>
/// A single validation issue.
/// </summary>
public record ValidationIssueDto
{
    public string Code { get; init; } = null!;
    public string Message { get; init; } = null!;
    public string? Field { get; init; }
    public string Severity { get; init; } = null!; // "Error" or "Warning"
}

/// <summary>
/// Paged result wrapper.
/// </summary>
public record PagedResult<T>
{
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
    public int Page { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
