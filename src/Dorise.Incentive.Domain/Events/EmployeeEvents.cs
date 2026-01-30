using Dorise.Incentive.Domain.Common;
using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Domain.Events;

/// <summary>
/// Event raised when a new employee is created.
/// </summary>
public sealed class EmployeeCreatedEvent : DomainEvent
{
    public Guid EmployeeId { get; }
    public string EmployeeCode { get; }

    public EmployeeCreatedEvent(Guid employeeId, string employeeCode)
    {
        EmployeeId = employeeId;
        EmployeeCode = employeeCode;
    }
}

/// <summary>
/// Event raised when an employee's salary is changed.
/// </summary>
public sealed class EmployeeSalaryChangedEvent : DomainEvent
{
    public Guid EmployeeId { get; }
    public decimal OldSalary { get; }
    public decimal NewSalary { get; }

    public EmployeeSalaryChangedEvent(Guid employeeId, decimal oldSalary, decimal newSalary)
    {
        EmployeeId = employeeId;
        OldSalary = oldSalary;
        NewSalary = newSalary;
    }
}

/// <summary>
/// Event raised when an employee is transferred to a new department.
/// </summary>
public sealed class EmployeeTransferredEvent : DomainEvent
{
    public Guid EmployeeId { get; }
    public Guid OldDepartmentId { get; }
    public Guid NewDepartmentId { get; }

    public EmployeeTransferredEvent(Guid employeeId, Guid oldDepartmentId, Guid newDepartmentId)
    {
        EmployeeId = employeeId;
        OldDepartmentId = oldDepartmentId;
        NewDepartmentId = newDepartmentId;
    }
}

/// <summary>
/// Event raised when an employee's status changes.
/// </summary>
public sealed class EmployeeStatusChangedEvent : DomainEvent
{
    public Guid EmployeeId { get; }
    public EmployeeStatus OldStatus { get; }
    public EmployeeStatus NewStatus { get; }

    public EmployeeStatusChangedEvent(Guid employeeId, EmployeeStatus oldStatus, EmployeeStatus newStatus)
    {
        EmployeeId = employeeId;
        OldStatus = oldStatus;
        NewStatus = newStatus;
    }
}
