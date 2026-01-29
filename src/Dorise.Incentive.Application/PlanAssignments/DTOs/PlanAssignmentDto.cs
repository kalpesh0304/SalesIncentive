namespace Dorise.Incentive.Application.PlanAssignments.DTOs;

/// <summary>
/// DTO for plan assignment.
/// "Sleep! That's where I'm a Viking!" - And assignments are where plans meet employees!
/// </summary>
public record PlanAssignmentDto
{
    public Guid Id { get; init; }
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = null!;
    public string EmployeeName { get; init; } = null!;
    public Guid IncentivePlanId { get; init; }
    public string PlanCode { get; init; } = null!;
    public string PlanName { get; init; } = null!;
    public DateTime EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public decimal? CustomTarget { get; init; }
    public string? CustomTargetUnit { get; init; }
    public bool IsActive { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public string CreatedBy { get; init; } = null!;
}

/// <summary>
/// Result of bulk assignment operation.
/// </summary>
public record BulkAssignmentResultDto
{
    public int TotalRequested { get; init; }
    public int SuccessCount { get; init; }
    public int FailedCount { get; init; }
    public IReadOnlyList<PlanAssignmentDto> SuccessfulAssignments { get; init; } = Array.Empty<PlanAssignmentDto>();
    public IReadOnlyList<BulkAssignmentErrorDto> Errors { get; init; } = Array.Empty<BulkAssignmentErrorDto>();
}

/// <summary>
/// Error details for failed bulk assignment.
/// </summary>
public record BulkAssignmentErrorDto
{
    public Guid EmployeeId { get; init; }
    public string EmployeeCode { get; init; } = null!;
    public string ErrorMessage { get; init; } = null!;
}

/// <summary>
/// Result of eligibility check.
/// </summary>
public record EligibilityCheckResultDto
{
    public Guid EmployeeId { get; init; }
    public Guid PlanId { get; init; }
    public bool IsEligible { get; init; }
    public string? Reason { get; init; }
    public IReadOnlyList<string> EligibilityCriteriaMet { get; init; } = Array.Empty<string>();
    public IReadOnlyList<string> EligibilityCriteriaNotMet { get; init; } = Array.Empty<string>();
    public bool HasExistingAssignment { get; init; }
    public DateTime? ExistingAssignmentEndDate { get; init; }
}
