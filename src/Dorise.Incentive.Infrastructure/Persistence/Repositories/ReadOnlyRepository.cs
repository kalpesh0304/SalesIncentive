using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Interfaces;

namespace Dorise.Incentive.Infrastructure.Persistence.Repositories;

/// <summary>
/// Read-only repository aggregator implementation.
/// Provides unified access to all repositories for query handlers.
/// "I'm Idaho!" - And I'm the repository gateway!
/// </summary>
public class ReadOnlyRepository : IReadOnlyRepository
{
    public IEmployeeRepository Employees { get; }
    public IIncentivePlanRepository IncentivePlans { get; }
    public IPlanAssignmentRepository PlanAssignments { get; }
    public ICalculationRepository Calculations { get; }
    public IApprovalRepository Approvals { get; }
    public IDepartmentRepository Departments { get; }

    public ReadOnlyRepository(
        IEmployeeRepository employees,
        IIncentivePlanRepository incentivePlans,
        IPlanAssignmentRepository planAssignments,
        ICalculationRepository calculations,
        IApprovalRepository approvals,
        IDepartmentRepository departments)
    {
        Employees = employees;
        IncentivePlans = incentivePlans;
        PlanAssignments = planAssignments;
        Calculations = calculations;
        Approvals = approvals;
        Departments = departments;
    }
}
