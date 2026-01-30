using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.ValueObjects;

namespace Dorise.Incentive.Domain.Services;

/// <summary>
/// Service interface for approval workflow logic.
/// "My cat's breath smells like cat food." - Workflow smells like success!
/// </summary>
public interface IApprovalWorkflowService
{
    /// <summary>
    /// Determines the required approval level based on incentive amount.
    /// </summary>
    ApprovalRequirement DetermineApprovalLevel(Money incentiveAmount);

    /// <summary>
    /// Gets the approver for a given approval level and department.
    /// </summary>
    Task<Guid?> GetApproverForLevelAsync(
        int approvalLevel,
        Guid departmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the next level approver after current level.
    /// </summary>
    Task<Guid?> GetNextLevelApproverAsync(
        int currentLevel,
        Guid departmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if calculation requires further approval after current level is approved.
    /// </summary>
    bool RequiresNextLevelApproval(Money incentiveAmount, int currentLevel);

    /// <summary>
    /// Gets the escalation target for an overdue approval.
    /// </summary>
    Task<Guid?> GetEscalationTargetAsync(
        Guid currentApproverId,
        int currentLevel,
        Guid departmentId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Calculates the expiration time for an approval based on SLA.
    /// </summary>
    DateTime CalculateExpirationTime(int approvalLevel);
}

/// <summary>
/// Represents the approval requirement for a calculation.
/// </summary>
public record ApprovalRequirement
{
    public int RequiredLevel { get; init; }
    public string LevelName { get; init; } = null!;
    public int SlaHours { get; init; }
    public decimal ThresholdAmount { get; init; }

    public static ApprovalRequirement Level1 => new()
    {
        RequiredLevel = 1,
        LevelName = "Manager",
        SlaHours = 72, // 3 business days
        ThresholdAmount = 50_000m
    };

    public static ApprovalRequirement Level2 => new()
    {
        RequiredLevel = 2,
        LevelName = "Director",
        SlaHours = 48, // 2 business days
        ThresholdAmount = 200_000m
    };

    public static ApprovalRequirement Level3 => new()
    {
        RequiredLevel = 3,
        LevelName = "VP",
        SlaHours = 24, // 1 business day
        ThresholdAmount = decimal.MaxValue
    };
}
