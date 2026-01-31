using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Infrastructure.Persistence;
using Dorise.Incentive.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore.Storage;

namespace Dorise.Incentive.Infrastructure.Services;

/// <summary>
/// Unit of Work implementation for coordinating repository operations.
/// "Hi, Super Nintendo Chalmers!" - Coordinating all the repos like a boss!
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly IncentiveDbContext _context;
    private IDbContextTransaction? _transaction;

    private IEmployeeRepository? _employees;
    private IDepartmentRepository? _departments;
    private IIncentivePlanRepository? _incentivePlans;
    private ICalculationRepository? _calculations;
    private IApprovalRepository? _approvals;
    private IPlanAssignmentRepository? _planAssignments;

    public UnitOfWork(IncentiveDbContext context)
    {
        _context = context;
    }

    public IEmployeeRepository Employees =>
        _employees ??= new EmployeeRepository(_context);

    public IDepartmentRepository Departments =>
        _departments ??= new DepartmentRepository(_context);

    public IIncentivePlanRepository IncentivePlans =>
        _incentivePlans ??= new IncentivePlanRepository(_context);

    public ICalculationRepository Calculations =>
        _calculations ??= new CalculationRepository(_context);

    public IApprovalRepository Approvals =>
        _approvals ??= new ApprovalRepository(_context);

    public IPlanAssignmentRepository PlanAssignments =>
        _planAssignments ??= new PlanAssignmentRepository(_context);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
