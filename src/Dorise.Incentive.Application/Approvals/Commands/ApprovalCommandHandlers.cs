using Dorise.Incentive.Application.Approvals.DTOs;
using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.Services;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.Approvals.Commands;

/// <summary>
/// Handler for ApproveCommand.
/// "I'm a unitard!" - Unified approval handler!
/// </summary>
public class ApproveCommandHandler : ICommandHandler<ApproveCommand, ApprovalResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApprovalWorkflowService _workflowService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<ApproveCommandHandler> _logger;

    public ApproveCommandHandler(
        IUnitOfWork unitOfWork,
        IApprovalWorkflowService workflowService,
        ICurrentUserService currentUser,
        ILogger<ApproveCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _workflowService = workflowService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<ApprovalResultDto>> Handle(
        ApproveCommand request,
        CancellationToken cancellationToken)
    {
        var approval = await _unitOfWork.Approvals.GetByIdAsync(request.ApprovalId, cancellationToken);
        if (approval == null)
        {
            return Result<ApprovalResultDto>.NotFound("Approval", request.ApprovalId);
        }

        if (!approval.IsPending)
        {
            return Result<ApprovalResultDto>.Failure(
                $"Cannot approve - current status is {approval.Status}",
                "INVALID_STATUS");
        }

        // Get the calculation to check if next level approval is needed
        var calculation = await _unitOfWork.Calculations.GetByIdAsync(approval.CalculationId, cancellationToken);
        if (calculation == null)
        {
            return Result<ApprovalResultDto>.Failure("Associated calculation not found", "CALCULATION_NOT_FOUND");
        }

        // Approve the current approval
        approval.Approve(request.Comments);

        // Check if next level approval is required
        var requiresNextLevel = _workflowService.RequiresNextLevelApproval(
            calculation.NetIncentive,
            approval.ApprovalLevel);

        int? nextLevel = null;

        if (requiresNextLevel)
        {
            // Create next level approval
            var employee = await _unitOfWork.Employees.GetByIdAsync(calculation.EmployeeId, cancellationToken);
            if (employee != null)
            {
                var nextApprovalLevel = approval.ApprovalLevel + 1;
                var nextApproverId = await _workflowService.GetApproverForLevelAsync(
                    nextApprovalLevel,
                    employee.DepartmentId,
                    cancellationToken);

                if (nextApproverId.HasValue)
                {
                    var expiresAt = _workflowService.CalculateExpirationTime(nextApprovalLevel);
                    var nextApproval = Approval.Create(
                        calculation.Id,
                        nextApproverId.Value,
                        nextApprovalLevel,
                        expiresAt);

                    await _unitOfWork.Approvals.AddAsync(nextApproval, cancellationToken);
                    nextLevel = nextApprovalLevel;

                    _logger.LogInformation(
                        "Created Level {Level} approval for calculation {CalculationId}",
                        nextApprovalLevel,
                        calculation.Id);
                }
            }
        }
        else
        {
            // All approvals complete - approve the calculation
            var approverName = _currentUser.Email ?? "system";
            calculation.Approve(approverName, request.Comments);

            _logger.LogInformation(
                "Calculation {CalculationId} fully approved",
                calculation.Id);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Approval {ApprovalId} approved by {Approver}",
            approval.Id,
            _currentUser.UserId);

        return Result<ApprovalResultDto>.Success(new ApprovalResultDto
        {
            ApprovalId = approval.Id,
            CalculationId = approval.CalculationId,
            NewStatus = approval.Status,
            Message = requiresNextLevel
                ? $"Approved. Escalated to Level {nextLevel} for final approval."
                : "Approved. Calculation is now fully approved.",
            RequiresNextLevelApproval = requiresNextLevel,
            NextApprovalLevel = nextLevel
        });
    }
}

/// <summary>
/// Handler for RejectCommand.
/// </summary>
public class RejectCommandHandler : ICommandHandler<RejectCommand, ApprovalResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<RejectCommandHandler> _logger;

    public RejectCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ILogger<RejectCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<ApprovalResultDto>> Handle(
        RejectCommand request,
        CancellationToken cancellationToken)
    {
        var approval = await _unitOfWork.Approvals.GetByIdAsync(request.ApprovalId, cancellationToken);
        if (approval == null)
        {
            return Result<ApprovalResultDto>.NotFound("Approval", request.ApprovalId);
        }

        if (!approval.IsPending)
        {
            return Result<ApprovalResultDto>.Failure(
                $"Cannot reject - current status is {approval.Status}",
                "INVALID_STATUS");
        }

        // Reject the approval
        approval.Reject(request.Reason);

        // Also reject the calculation
        var calculation = await _unitOfWork.Calculations.GetByIdAsync(approval.CalculationId, cancellationToken);
        if (calculation != null)
        {
            var rejectorName = _currentUser.Email ?? "system";
            calculation.Reject(rejectorName, request.Reason);
        }

        // Cancel any other pending approvals for this calculation
        var otherApprovals = await _unitOfWork.Approvals.GetForCalculationAsync(
            approval.CalculationId,
            cancellationToken);

        foreach (var other in otherApprovals.Where(a => a.Id != approval.Id && a.IsPending))
        {
            other.Cancel("Parent approval rejected");
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Approval {ApprovalId} rejected by {Rejector}. Reason: {Reason}",
            approval.Id,
            _currentUser.UserId,
            request.Reason);

        return Result<ApprovalResultDto>.Success(new ApprovalResultDto
        {
            ApprovalId = approval.Id,
            CalculationId = approval.CalculationId,
            NewStatus = ApprovalStatus.Rejected,
            Message = "Calculation rejected.",
            RequiresNextLevelApproval = false
        });
    }
}

/// <summary>
/// Handler for BulkApproveCommand.
/// "Me fail English? That's unpossible!" - Bulk success is very possible!
/// </summary>
public class BulkApproveCommandHandler : ICommandHandler<BulkApproveCommand, BulkApprovalResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApprovalWorkflowService _workflowService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<BulkApproveCommandHandler> _logger;

    public BulkApproveCommandHandler(
        IUnitOfWork unitOfWork,
        IApprovalWorkflowService workflowService,
        ICurrentUserService currentUser,
        ILogger<BulkApproveCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _workflowService = workflowService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<BulkApprovalResultDto>> Handle(
        BulkApproveCommand request,
        CancellationToken cancellationToken)
    {
        var results = new List<ApprovalResultDto>();
        var errors = new List<ApprovalErrorDto>();
        var totalApprovedAmount = 0m;

        foreach (var approvalId in request.ApprovalIds)
        {
            try
            {
                var approval = await _unitOfWork.Approvals.GetByIdAsync(approvalId, cancellationToken);
                if (approval == null)
                {
                    errors.Add(new ApprovalErrorDto
                    {
                        ApprovalId = approvalId,
                        ErrorMessage = "Approval not found",
                        ErrorCode = "NOT_FOUND"
                    });
                    continue;
                }

                if (!approval.IsPending)
                {
                    errors.Add(new ApprovalErrorDto
                    {
                        ApprovalId = approvalId,
                        ErrorMessage = $"Cannot approve - status is {approval.Status}",
                        ErrorCode = "INVALID_STATUS"
                    });
                    continue;
                }

                var calculation = await _unitOfWork.Calculations.GetByIdAsync(
                    approval.CalculationId,
                    cancellationToken);

                if (calculation == null)
                {
                    errors.Add(new ApprovalErrorDto
                    {
                        ApprovalId = approvalId,
                        ErrorMessage = "Calculation not found",
                        ErrorCode = "CALCULATION_NOT_FOUND"
                    });
                    continue;
                }

                // Approve
                approval.Approve(request.Comments);

                var requiresNextLevel = _workflowService.RequiresNextLevelApproval(
                    calculation.NetIncentive,
                    approval.ApprovalLevel);

                int? nextLevel = null;

                if (requiresNextLevel)
                {
                    var employee = await _unitOfWork.Employees.GetByIdAsync(
                        calculation.EmployeeId,
                        cancellationToken);

                    if (employee != null)
                    {
                        var nextApprovalLevel = approval.ApprovalLevel + 1;
                        var nextApproverId = await _workflowService.GetApproverForLevelAsync(
                            nextApprovalLevel,
                            employee.DepartmentId,
                            cancellationToken);

                        if (nextApproverId.HasValue)
                        {
                            var expiresAt = _workflowService.CalculateExpirationTime(nextApprovalLevel);
                            var nextApproval = Approval.Create(
                                calculation.Id,
                                nextApproverId.Value,
                                nextApprovalLevel,
                                expiresAt);

                            await _unitOfWork.Approvals.AddAsync(nextApproval, cancellationToken);
                            nextLevel = nextApprovalLevel;
                        }
                    }
                }
                else
                {
                    var approverName = _currentUser.Email ?? "system";
                    calculation.Approve(approverName, request.Comments);
                    totalApprovedAmount += calculation.NetIncentive.Amount;
                }

                results.Add(new ApprovalResultDto
                {
                    ApprovalId = approval.Id,
                    CalculationId = approval.CalculationId,
                    NewStatus = approval.Status,
                    Message = requiresNextLevel ? "Escalated to next level" : "Fully approved",
                    RequiresNextLevelApproval = requiresNextLevel,
                    NextApprovalLevel = nextLevel
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing bulk approval for {ApprovalId}", approvalId);
                errors.Add(new ApprovalErrorDto
                {
                    ApprovalId = approvalId,
                    ErrorMessage = ex.Message,
                    ErrorCode = "PROCESSING_ERROR"
                });
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Bulk approval completed: {Success} succeeded, {Failed} failed",
            results.Count,
            errors.Count);

        return Result<BulkApprovalResultDto>.Success(new BulkApprovalResultDto
        {
            TotalProcessed = request.ApprovalIds.Count,
            SuccessCount = results.Count,
            FailedCount = errors.Count,
            TotalApprovedAmount = totalApprovedAmount,
            Currency = "INR",
            Results = results,
            Errors = errors
        });
    }
}

/// <summary>
/// Handler for DelegateApprovalCommand.
/// </summary>
public class DelegateApprovalCommandHandler : ICommandHandler<DelegateApprovalCommand, ApprovalResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<DelegateApprovalCommandHandler> _logger;

    public DelegateApprovalCommandHandler(
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUser,
        ILogger<DelegateApprovalCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<ApprovalResultDto>> Handle(
        DelegateApprovalCommand request,
        CancellationToken cancellationToken)
    {
        var approval = await _unitOfWork.Approvals.GetByIdAsync(request.ApprovalId, cancellationToken);
        if (approval == null)
        {
            return Result<ApprovalResultDto>.NotFound("Approval", request.ApprovalId);
        }

        if (!approval.IsPending)
        {
            return Result<ApprovalResultDto>.Failure(
                $"Cannot delegate - current status is {approval.Status}",
                "INVALID_STATUS");
        }

        // Verify delegate exists
        var delegate_ = await _unitOfWork.Employees.GetByIdAsync(request.DelegateToId, cancellationToken);
        if (delegate_ == null)
        {
            return Result<ApprovalResultDto>.NotFound("Employee (delegate)", request.DelegateToId);
        }

        // Mark current as delegated
        approval.Delegate(request.DelegateToId);

        // Create new approval for the delegate
        var calculation = await _unitOfWork.Calculations.GetByIdAsync(
            approval.CalculationId,
            cancellationToken);

        if (calculation == null)
        {
            return Result<ApprovalResultDto>.Failure("Calculation not found", "CALCULATION_NOT_FOUND");
        }

        var newApproval = Approval.Create(
            calculation.Id,
            request.DelegateToId,
            approval.ApprovalLevel,
            approval.ExpiresAt);

        await _unitOfWork.Approvals.AddAsync(newApproval, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Approval {ApprovalId} delegated to {DelegateId} by {Delegator}",
            approval.Id,
            request.DelegateToId,
            _currentUser.UserId);

        return Result<ApprovalResultDto>.Success(new ApprovalResultDto
        {
            ApprovalId = newApproval.Id,
            CalculationId = approval.CalculationId,
            NewStatus = ApprovalStatus.Pending,
            Message = $"Delegated to {delegate_.FirstName} {delegate_.LastName}",
            RequiresNextLevelApproval = false
        });
    }
}

/// <summary>
/// Handler for EscalateApprovalCommand.
/// </summary>
public class EscalateApprovalCommandHandler : ICommandHandler<EscalateApprovalCommand, ApprovalResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApprovalWorkflowService _workflowService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<EscalateApprovalCommandHandler> _logger;

    public EscalateApprovalCommandHandler(
        IUnitOfWork unitOfWork,
        IApprovalWorkflowService workflowService,
        ICurrentUserService currentUser,
        ILogger<EscalateApprovalCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _workflowService = workflowService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<ApprovalResultDto>> Handle(
        EscalateApprovalCommand request,
        CancellationToken cancellationToken)
    {
        var approval = await _unitOfWork.Approvals.GetByIdAsync(request.ApprovalId, cancellationToken);
        if (approval == null)
        {
            return Result<ApprovalResultDto>.NotFound("Approval", request.ApprovalId);
        }

        if (!approval.IsPending)
        {
            return Result<ApprovalResultDto>.Failure(
                $"Cannot escalate - current status is {approval.Status}",
                "INVALID_STATUS");
        }

        var calculation = await _unitOfWork.Calculations.GetByIdAsync(
            approval.CalculationId,
            cancellationToken);

        if (calculation == null)
        {
            return Result<ApprovalResultDto>.Failure("Calculation not found", "CALCULATION_NOT_FOUND");
        }

        var employee = await _unitOfWork.Employees.GetByIdAsync(
            calculation.EmployeeId,
            cancellationToken);

        if (employee == null)
        {
            return Result<ApprovalResultDto>.Failure("Employee not found", "EMPLOYEE_NOT_FOUND");
        }

        // Get escalation target
        var escalationTarget = await _workflowService.GetEscalationTargetAsync(
            approval.ApproverId,
            approval.ApprovalLevel,
            employee.DepartmentId,
            cancellationToken);

        if (!escalationTarget.HasValue)
        {
            return Result<ApprovalResultDto>.Failure(
                "No escalation target available",
                "NO_ESCALATION_TARGET");
        }

        // Mark current approval as escalated
        approval.Escalate(request.Reason);

        // Create new approval for escalation target
        var nextLevel = approval.ApprovalLevel + 1;
        var expiresAt = _workflowService.CalculateExpirationTime(nextLevel);
        var newApproval = Approval.Create(
            calculation.Id,
            escalationTarget.Value,
            nextLevel,
            expiresAt);

        await _unitOfWork.Approvals.AddAsync(newApproval, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Approval {ApprovalId} escalated to Level {Level} by {Escalator}. Reason: {Reason}",
            approval.Id,
            nextLevel,
            _currentUser.UserId,
            request.Reason);

        return Result<ApprovalResultDto>.Success(new ApprovalResultDto
        {
            ApprovalId = newApproval.Id,
            CalculationId = approval.CalculationId,
            NewStatus = ApprovalStatus.Pending,
            Message = $"Escalated to Level {nextLevel}",
            RequiresNextLevelApproval = false,
            NextApprovalLevel = nextLevel
        });
    }
}

/// <summary>
/// Handler for SubmitForApprovalCommand.
/// "That's where I saw the leprechaun!" - Submit and watch the magic!
/// </summary>
public class SubmitForApprovalCommandHandler : ICommandHandler<SubmitForApprovalCommand, SubmissionResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApprovalWorkflowService _workflowService;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<SubmitForApprovalCommandHandler> _logger;

    public SubmitForApprovalCommandHandler(
        IUnitOfWork unitOfWork,
        IApprovalWorkflowService workflowService,
        ICurrentUserService currentUser,
        ILogger<SubmitForApprovalCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _workflowService = workflowService;
        _currentUser = currentUser;
        _logger = logger;
    }

    public async Task<Result<SubmissionResultDto>> Handle(
        SubmitForApprovalCommand request,
        CancellationToken cancellationToken)
    {
        var results = new List<SubmissionItemResultDto>();
        var submitter = _currentUser.Email ?? "system";

        foreach (var calculationId in request.CalculationIds)
        {
            try
            {
                var calculation = await _unitOfWork.Calculations.GetByIdAsync(calculationId, cancellationToken);
                if (calculation == null)
                {
                    results.Add(new SubmissionItemResultDto
                    {
                        CalculationId = calculationId,
                        Success = false,
                        ErrorMessage = "Calculation not found"
                    });
                    continue;
                }

                // Validate status
                var validStatuses = new[]
                {
                    CalculationStatus.Calculated,
                    CalculationStatus.Prorated,
                    CalculationStatus.Capped
                };

                if (!validStatuses.Contains(calculation.Status))
                {
                    results.Add(new SubmissionItemResultDto
                    {
                        CalculationId = calculationId,
                        Success = false,
                        ErrorMessage = $"Cannot submit - status is {calculation.Status}"
                    });
                    continue;
                }

                // Determine approval level
                var requirement = _workflowService.DetermineApprovalLevel(calculation.NetIncentive);

                // Get employee to find department
                var employee = await _unitOfWork.Employees.GetByIdAsync(
                    calculation.EmployeeId,
                    cancellationToken);

                if (employee == null)
                {
                    results.Add(new SubmissionItemResultDto
                    {
                        CalculationId = calculationId,
                        Success = false,
                        ErrorMessage = "Employee not found"
                    });
                    continue;
                }

                // Get first level approver
                var approverId = await _workflowService.GetApproverForLevelAsync(
                    1, // Start at level 1
                    employee.DepartmentId,
                    cancellationToken);

                if (!approverId.HasValue)
                {
                    results.Add(new SubmissionItemResultDto
                    {
                        CalculationId = calculationId,
                        Success = false,
                        ErrorMessage = "No approver found for department"
                    });
                    continue;
                }

                // Submit the calculation
                calculation.SubmitForApproval(submitter);

                // Create approval request
                var expiresAt = _workflowService.CalculateExpirationTime(1);
                var approval = Approval.Create(
                    calculation.Id,
                    approverId.Value,
                    1,
                    expiresAt);

                await _unitOfWork.Approvals.AddAsync(approval, cancellationToken);

                var approver = await _unitOfWork.Employees.GetByIdAsync(approverId.Value, cancellationToken);

                results.Add(new SubmissionItemResultDto
                {
                    CalculationId = calculationId,
                    Success = true,
                    ApprovalId = approval.Id,
                    ApprovalLevel = 1,
                    AssignedApproverId = approverId.Value,
                    ApproverName = approver != null ? $"{approver.FirstName} {approver.LastName}" : null
                });

                _logger.LogInformation(
                    "Calculation {CalculationId} submitted for approval. Assigned to {ApproverId}",
                    calculationId,
                    approverId.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting calculation {CalculationId}", calculationId);
                results.Add(new SubmissionItemResultDto
                {
                    CalculationId = calculationId,
                    Success = false,
                    ErrorMessage = ex.Message
                });
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var successCount = results.Count(r => r.Success);

        return Result<SubmissionResultDto>.Success(new SubmissionResultDto
        {
            TotalSubmitted = request.CalculationIds.Count,
            SuccessCount = successCount,
            FailedCount = results.Count - successCount,
            Results = results
        });
    }
}

/// <summary>
/// Handler for ProcessExpiredApprovalsCommand.
/// Background job handler for processing expired approvals.
/// </summary>
public class ProcessExpiredApprovalsCommandHandler : ICommandHandler<ProcessExpiredApprovalsCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ProcessExpiredApprovalsCommandHandler> _logger;

    public ProcessExpiredApprovalsCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<ProcessExpiredApprovalsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(
        ProcessExpiredApprovalsCommand request,
        CancellationToken cancellationToken)
    {
        var expiredApprovals = await _unitOfWork.Approvals.GetExpiredAsync(cancellationToken);
        var processedCount = 0;

        foreach (var approval in expiredApprovals)
        {
            try
            {
                approval.MarkExpired();
                processedCount++;

                _logger.LogWarning(
                    "Approval {ApprovalId} marked as expired",
                    approval.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing expired approval {ApprovalId}", approval.Id);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Processed {Count} expired approvals",
            processedCount);

        return Result<int>.Success(processedCount);
    }
}

/// <summary>
/// Handler for AutoEscalateOverdueApprovalsCommand.
/// Background job handler for auto-escalating overdue approvals.
/// </summary>
public class AutoEscalateOverdueApprovalsCommandHandler : ICommandHandler<AutoEscalateOverdueApprovalsCommand, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IApprovalWorkflowService _workflowService;
    private readonly ILogger<AutoEscalateOverdueApprovalsCommandHandler> _logger;

    public AutoEscalateOverdueApprovalsCommandHandler(
        IUnitOfWork unitOfWork,
        IApprovalWorkflowService workflowService,
        ILogger<AutoEscalateOverdueApprovalsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _workflowService = workflowService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(
        AutoEscalateOverdueApprovalsCommand request,
        CancellationToken cancellationToken)
    {
        var overdueApprovals = await _unitOfWork.Approvals.GetForEscalationAsync(
            request.SlaHours,
            cancellationToken);

        var escalatedCount = 0;

        foreach (var approval in overdueApprovals)
        {
            try
            {
                var calculation = await _unitOfWork.Calculations.GetByIdAsync(
                    approval.CalculationId,
                    cancellationToken);

                if (calculation == null)
                {
                    continue;
                }

                var employee = await _unitOfWork.Employees.GetByIdAsync(
                    calculation.EmployeeId,
                    cancellationToken);

                if (employee == null)
                {
                    continue;
                }

                // Get escalation target
                var escalationTarget = await _workflowService.GetEscalationTargetAsync(
                    approval.ApproverId,
                    approval.ApprovalLevel,
                    employee.DepartmentId,
                    cancellationToken);

                if (!escalationTarget.HasValue)
                {
                    _logger.LogWarning(
                        "No escalation target for approval {ApprovalId}",
                        approval.Id);
                    continue;
                }

                // Mark as escalated
                approval.Escalate("Auto-escalated due to SLA breach");

                // Create new approval
                var nextLevel = approval.ApprovalLevel + 1;
                var expiresAt = _workflowService.CalculateExpirationTime(nextLevel);
                var newApproval = Approval.Create(
                    calculation.Id,
                    escalationTarget.Value,
                    nextLevel,
                    expiresAt);

                await _unitOfWork.Approvals.AddAsync(newApproval, cancellationToken);
                escalatedCount++;

                _logger.LogInformation(
                    "Auto-escalated approval {ApprovalId} to Level {Level}",
                    approval.Id,
                    nextLevel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error auto-escalating approval {ApprovalId}", approval.Id);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Auto-escalated {Count} overdue approvals",
            escalatedCount);

        return Result<int>.Success(escalatedCount);
    }
}
