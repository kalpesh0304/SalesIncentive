namespace Dorise.Incentive.Domain.Enums;

/// <summary>
/// Status of an employee in the system.
/// </summary>
public enum EmployeeStatus
{
    /// <summary>Active and eligible for incentives</summary>
    Active = 1,

    /// <summary>On probation period</summary>
    Probation = 2,

    /// <summary>On leave (temporary)</summary>
    OnLeave = 3,

    /// <summary>Employment terminated</summary>
    Terminated = 4,

    /// <summary>Soft deleted from system</summary>
    Inactive = 5
}
