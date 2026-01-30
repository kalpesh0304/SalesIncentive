using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.Services;
using Dorise.Incentive.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.Approvals.Services;

/// <summary>
/// Implementation of approval workflow service.
/// "I bent my Wookiee!" - But the workflow stays straight!
/// </summary>
public class ApprovalWorkflowService : IApprovalWorkflowService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ApprovalWorkflowService> _logger;

    // Approval thresholds (in INR)
    private const decimal Level1Threshold = 50_000m;     // Up to ₹50,000 - Manager
    private const decimal Level2Threshold = 200_000m;    // ₹50,001 - ₹200,000 - Director
    // Above ₹200,000 - VP (Level 3)

    public ApprovalWorkflowService(
        IUnitOfWork unitOfWork,
        ILogger<ApprovalWorkflowService> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public ApprovalRequirement DetermineApprovalLevel(Money incentiveAmount)
    {
        var amount = incentiveAmount.Amount;

        if (amount <= Level1Threshold)
        {
            _logger.LogDebug(
                "Amount {Amount} requires Level 1 (Manager) approval",
                amount);
            return ApprovalRequirement.Level1;
        }

        if (amount <= Level2Threshold)
        {
            _logger.LogDebug(
                "Amount {Amount} requires Level 2 (Director) approval",
                amount);
            return ApprovalRequirement.Level2;
        }

        _logger.LogDebug(
            "Amount {Amount} requires Level 3 (VP) approval",
            amount);
        return ApprovalRequirement.Level3;
    }

    public async Task<Guid?> GetApproverForLevelAsync(
        int approvalLevel,
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        // Get department to find hierarchy
        var department = await _unitOfWork.Departments.GetByIdAsync(departmentId, cancellationToken);
        if (department == null)
        {
            _logger.LogWarning("Department {DepartmentId} not found", departmentId);
            return null;
        }

        // For now, use a simplified approach:
        // Level 1 - Department manager (stored in department)
        // Level 2 - Parent department manager
        // Level 3 - Top-level department manager

        return approvalLevel switch
        {
            1 => department.ManagerId,
            2 => await GetParentDepartmentManagerAsync(department.ParentId, cancellationToken),
            3 => await GetTopLevelManagerAsync(departmentId, cancellationToken),
            _ => null
        };
    }

    public async Task<Guid?> GetNextLevelApproverAsync(
        int currentLevel,
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        var nextLevel = currentLevel + 1;
        if (nextLevel > 3)
        {
            return null; // No more levels
        }

        return await GetApproverForLevelAsync(nextLevel, departmentId, cancellationToken);
    }

    public bool RequiresNextLevelApproval(Money incentiveAmount, int currentLevel)
    {
        var required = DetermineApprovalLevel(incentiveAmount);
        return currentLevel < required.RequiredLevel;
    }

    public async Task<Guid?> GetEscalationTargetAsync(
        Guid currentApproverId,
        int currentLevel,
        Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        // First try to get the same level approver from parent department
        var department = await _unitOfWork.Departments.GetByIdAsync(departmentId, cancellationToken);
        if (department?.ParentId != null)
        {
            var parentManager = await GetParentDepartmentManagerAsync(department.ParentId, cancellationToken);
            if (parentManager.HasValue && parentManager.Value != currentApproverId)
            {
                return parentManager;
            }
        }

        // If no parent manager, escalate to next level
        return await GetNextLevelApproverAsync(currentLevel, departmentId, cancellationToken);
    }

    public DateTime CalculateExpirationTime(int approvalLevel)
    {
        var slaHours = approvalLevel switch
        {
            1 => 72, // 3 business days
            2 => 48, // 2 business days
            3 => 24, // 1 business day
            _ => 72
        };

        // Simple calculation - in production, should consider business hours
        return DateTime.UtcNow.AddHours(slaHours);
    }

    private async Task<Guid?> GetParentDepartmentManagerAsync(
        Guid? parentId,
        CancellationToken cancellationToken)
    {
        if (!parentId.HasValue)
        {
            return null;
        }

        var parent = await _unitOfWork.Departments.GetByIdAsync(parentId.Value, cancellationToken);
        return parent?.ManagerId;
    }

    private async Task<Guid?> GetTopLevelManagerAsync(
        Guid departmentId,
        CancellationToken cancellationToken)
    {
        var hierarchy = await _unitOfWork.Departments.GetHierarchyAsync(departmentId, cancellationToken);
        var topLevel = hierarchy.LastOrDefault();
        return topLevel?.ManagerId;
    }
}
