using Dorise.Incentive.Domain.Common;

namespace Dorise.Incentive.Domain.Events;

/// <summary>
/// Event raised when a calculation is completed.
/// </summary>
public sealed class CalculationCompletedEvent : DomainEvent
{
    public Guid CalculationId { get; }
    public Guid EmployeeId { get; }
    public decimal IncentiveAmount { get; }

    public CalculationCompletedEvent(Guid calculationId, Guid employeeId, decimal incentiveAmount)
    {
        CalculationId = calculationId;
        EmployeeId = employeeId;
        IncentiveAmount = incentiveAmount;
    }
}

/// <summary>
/// Event raised when a calculation is submitted for approval.
/// </summary>
public sealed class CalculationSubmittedForApprovalEvent : DomainEvent
{
    public Guid CalculationId { get; }
    public Guid EmployeeId { get; }

    public CalculationSubmittedForApprovalEvent(Guid calculationId, Guid employeeId)
    {
        CalculationId = calculationId;
        EmployeeId = employeeId;
    }
}

/// <summary>
/// Event raised when a calculation is approved.
/// </summary>
public sealed class CalculationApprovedEvent : DomainEvent
{
    public Guid CalculationId { get; }
    public Guid EmployeeId { get; }
    public string ApprovedBy { get; }

    public CalculationApprovedEvent(Guid calculationId, Guid employeeId, string approvedBy)
    {
        CalculationId = calculationId;
        EmployeeId = employeeId;
        ApprovedBy = approvedBy;
    }
}

/// <summary>
/// Event raised when a calculation is rejected.
/// </summary>
public sealed class CalculationRejectedEvent : DomainEvent
{
    public Guid CalculationId { get; }
    public Guid EmployeeId { get; }
    public string RejectedBy { get; }
    public string Reason { get; }

    public CalculationRejectedEvent(Guid calculationId, Guid employeeId, string rejectedBy, string reason)
    {
        CalculationId = calculationId;
        EmployeeId = employeeId;
        RejectedBy = rejectedBy;
        Reason = reason;
    }
}

/// <summary>
/// Event raised when a calculation is marked as paid.
/// </summary>
public sealed class CalculationPaidEvent : DomainEvent
{
    public Guid CalculationId { get; }
    public Guid EmployeeId { get; }
    public decimal Amount { get; }

    public CalculationPaidEvent(Guid calculationId, Guid employeeId, decimal amount)
    {
        CalculationId = calculationId;
        EmployeeId = employeeId;
        Amount = amount;
    }
}

/// <summary>
/// Event raised when a calculation is voided.
/// </summary>
public sealed class CalculationVoidedEvent : DomainEvent
{
    public Guid CalculationId { get; }
    public Guid EmployeeId { get; }
    public string VoidedBy { get; }
    public string Reason { get; }

    public CalculationVoidedEvent(Guid calculationId, Guid employeeId, string voidedBy, string reason)
    {
        CalculationId = calculationId;
        EmployeeId = employeeId;
        VoidedBy = voidedBy;
        Reason = reason;
    }
}
