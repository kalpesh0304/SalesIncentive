using Dorise.Incentive.Domain.Common;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Events;
using Dorise.Incentive.Domain.ValueObjects;

namespace Dorise.Incentive.Domain.Entities;

/// <summary>
/// Represents an employee eligible for incentive calculations.
/// "Hi, Super Nintendo Chalmers!" - And hello Employee entity!
/// </summary>
public class Employee : AuditableEntity, IAggregateRoot
{
    public EmployeeCode EmployeeCode { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
    public Guid DepartmentId { get; private set; }
    public Guid? ManagerId { get; private set; }
    public string? Designation { get; private set; }
    public EmployeeStatus Status { get; private set; }
    public DateTime DateOfJoining { get; private set; }
    public DateTime? DateOfLeaving { get; private set; }
    public Money BaseSalary { get; private set; } = null!;
    public string? AzureAdObjectId { get; private set; }

    // Navigation properties
    private readonly List<PlanAssignment> _planAssignments = new();
    public IReadOnlyCollection<PlanAssignment> PlanAssignments => _planAssignments.AsReadOnly();

    private readonly List<Calculation> _calculations = new();
    public IReadOnlyCollection<Calculation> Calculations => _calculations.AsReadOnly();

    private Employee() { } // EF Core constructor

    public static Employee Create(
        string employeeCode,
        string firstName,
        string lastName,
        string email,
        Guid departmentId,
        DateTime dateOfJoining,
        decimal baseSalary,
        string currency = "INR",
        string? designation = null,
        Guid? managerId = null)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name is required", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name is required", nameof(lastName));

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required", nameof(email));

        if (dateOfJoining > DateTime.UtcNow.Date)
            throw new ArgumentException("Date of joining cannot be in the future", nameof(dateOfJoining));

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            EmployeeCode = EmployeeCode.Create(employeeCode),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            DepartmentId = departmentId,
            DateOfJoining = dateOfJoining.Date,
            BaseSalary = Money.Create(baseSalary, currency),
            Status = EmployeeStatus.Active,
            Designation = designation?.Trim(),
            ManagerId = managerId
        };

        employee.AddDomainEvent(new EmployeeCreatedEvent(employee.Id, employee.EmployeeCode.Value));

        return employee;
    }

    public string FullName => $"{FirstName} {LastName}";

    public bool IsActive => Status == EmployeeStatus.Active || Status == EmployeeStatus.Probation;

    public void UpdateProfile(string firstName, string lastName, string email, string? designation)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        Designation = designation?.Trim();
    }

    public void UpdateSalary(decimal newSalary, string currency = "INR")
    {
        var oldSalary = BaseSalary;
        BaseSalary = Money.Create(newSalary, currency);
        AddDomainEvent(new EmployeeSalaryChangedEvent(Id, oldSalary.Amount, newSalary));
    }

    public void TransferToDepartment(Guid newDepartmentId, Guid? newManagerId = null)
    {
        var oldDepartmentId = DepartmentId;
        DepartmentId = newDepartmentId;
        ManagerId = newManagerId;
        AddDomainEvent(new EmployeeTransferredEvent(Id, oldDepartmentId, newDepartmentId));
    }

    public void SetManager(Guid? managerId)
    {
        if (managerId == Id)
            throw new InvalidOperationException("Employee cannot be their own manager");

        ManagerId = managerId;
    }

    public void ChangeStatus(EmployeeStatus newStatus)
    {
        var oldStatus = Status;
        Status = newStatus;

        if (newStatus == EmployeeStatus.Terminated || newStatus == EmployeeStatus.Inactive)
        {
            DateOfLeaving = DateTime.UtcNow.Date;
        }

        AddDomainEvent(new EmployeeStatusChangedEvent(Id, oldStatus, newStatus));
    }

    public void SetAzureAdObjectId(string objectId)
    {
        AzureAdObjectId = objectId;
    }

    public int GetTenureInDays(DateTime asOfDate)
    {
        var endDate = DateOfLeaving ?? asOfDate;
        return (int)(endDate - DateOfJoining).TotalDays;
    }

    public bool IsEligibleForPeriod(DateRange period)
    {
        // Employee must be active and have joined before or during the period
        if (!IsActive && DateOfLeaving.HasValue && DateOfLeaving.Value < period.StartDate)
            return false;

        if (DateOfJoining > period.EndDate)
            return false;

        return true;
    }
}
