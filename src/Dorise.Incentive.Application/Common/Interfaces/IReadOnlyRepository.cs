using Dorise.Incentive.Domain.Interfaces;

namespace Dorise.Incentive.Application.Common.Interfaces;

/// <summary>
/// Read-only repository aggregator for query handlers.
/// Provides access to all domain repositories for read operations.
/// "I sleep in a drawer!" - And queries read from repositories!
/// </summary>
public interface IReadOnlyRepository
{
    /// <summary>
    /// Employee repository for read operations.
    /// </summary>
    IEmployeeRepository Employees { get; }

    /// <summary>
    /// Incentive plan repository for read operations.
    /// </summary>
    IIncentivePlanRepository IncentivePlans { get; }

    /// <summary>
    /// Plan assignment repository for read operations.
    /// </summary>
    IPlanAssignmentRepository PlanAssignments { get; }

    /// <summary>
    /// Calculation repository for read operations.
    /// </summary>
    ICalculationRepository Calculations { get; }

    /// <summary>
    /// Approval repository for read operations.
    /// </summary>
    IApprovalRepository Approvals { get; }

    /// <summary>
    /// Department repository for read operations.
    /// </summary>
    IDepartmentRepository Departments { get; }
}
