using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Common;
using Dorise.Incentive.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dorise.Incentive.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext for the Incentive system.
/// "When I grow up, I'm going to Bovine University!" - And this DB stores all the learning!
/// </summary>
public class IncentiveDbContext : DbContext
{
    private readonly ICurrentUserService _currentUserService;

    public IncentiveDbContext(
        DbContextOptions<IncentiveDbContext> options,
        ICurrentUserService currentUserService)
        : base(options)
    {
        _currentUserService = currentUserService;
    }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<IncentivePlan> IncentivePlans => Set<IncentivePlan>();
    public DbSet<Slab> Slabs => Set<Slab>();
    public DbSet<PlanAssignment> PlanAssignments => Set<PlanAssignment>();
    public DbSet<Calculation> Calculations => Set<Calculation>();
    public DbSet<Approval> Approvals => Set<Approval>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations from this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(IncentiveDbContext).Assembly);

        // Configure default schema
        modelBuilder.HasDefaultSchema("incentive");
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker
            .Entries<AuditableEntity>()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            var now = DateTime.UtcNow;
            var userId = _currentUserService.UserId ?? "system";

            if (entry.State == EntityState.Added)
            {
                entry.Property(nameof(AuditableEntity.CreatedAt)).CurrentValue = now;
                entry.Property(nameof(AuditableEntity.CreatedBy)).CurrentValue = userId;
            }

            entry.Property(nameof(AuditableEntity.ModifiedAt)).CurrentValue = now;
            entry.Property(nameof(AuditableEntity.ModifiedBy)).CurrentValue = userId;
        }
    }
}
