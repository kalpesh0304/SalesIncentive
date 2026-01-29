using Dorise.Incentive.Domain.Common;
using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Domain.Entities;

/// <summary>
/// Represents a role in the RBAC system with assigned permissions.
/// "Me fail English? That's unpossible!" - Roles make the unpossible possible!
/// </summary>
public class Role : AuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsSystem { get; private set; }
    public bool IsActive { get; private set; }
    public Permission Permissions { get; private set; }

    // Extended permissions stored as JSON or separate table
    private readonly List<ExtendedPermission> _extendedPermissions = new();
    public IReadOnlyCollection<ExtendedPermission> ExtendedPermissions => _extendedPermissions.AsReadOnly();

    // Navigation properties
    private readonly List<UserRole> _userRoles = new();
    public IReadOnlyCollection<UserRole> UserRoles => _userRoles.AsReadOnly();

    private Role() { } // EF Core constructor

    public static Role Create(
        string name,
        string? description = null,
        bool isSystem = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Role name is required", nameof(name));

        var role = new Role
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            IsSystem = isSystem,
            IsActive = true,
            Permissions = Permission.None
        };

        return role;
    }

    public static Role CreateSystemRole(string name, Permission permissions, string? description = null)
    {
        var role = Create(name, description, isSystem: true);
        role.Permissions = permissions;
        return role;
    }

    public void UpdateDetails(string name, string? description)
    {
        if (IsSystem)
            throw new InvalidOperationException("Cannot modify system role details");

        Name = name.Trim();
        Description = description?.Trim();
    }

    public void GrantPermission(Permission permission)
    {
        Permissions |= permission;
    }

    public void RevokePermission(Permission permission)
    {
        if (IsSystem)
            throw new InvalidOperationException("Cannot revoke permissions from system roles");

        Permissions &= ~permission;
    }

    public void SetPermissions(Permission permissions)
    {
        if (IsSystem)
            throw new InvalidOperationException("Cannot set permissions on system roles");

        Permissions = permissions;
    }

    public void GrantExtendedPermission(ExtendedPermission permission)
    {
        if (!_extendedPermissions.Contains(permission))
        {
            _extendedPermissions.Add(permission);
        }
    }

    public void RevokeExtendedPermission(ExtendedPermission permission)
    {
        if (IsSystem)
            throw new InvalidOperationException("Cannot revoke permissions from system roles");

        _extendedPermissions.Remove(permission);
    }

    public bool HasPermission(Permission permission)
    {
        return (Permissions & permission) == permission;
    }

    public bool HasExtendedPermission(ExtendedPermission permission)
    {
        return _extendedPermissions.Contains(permission);
    }

    public bool HasAnyPermission(Permission permissions)
    {
        return (Permissions & permissions) != Permission.None;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        if (IsSystem)
            throw new InvalidOperationException("Cannot deactivate system roles");

        IsActive = false;
    }

    public IReadOnlyList<Permission> GetGrantedPermissions()
    {
        var granted = new List<Permission>();
        foreach (Permission permission in Enum.GetValues(typeof(Permission)))
        {
            if (permission != Permission.None && HasPermission(permission))
            {
                granted.Add(permission);
            }
        }
        return granted;
    }
}

/// <summary>
/// Represents a user's role assignment.
/// "The doctor said I wouldn't have so many nosebleeds if I kept my finger outta there." -
/// Properly assigned roles keep things running smoothly!
/// </summary>
public class UserRole : AuditableEntity
{
    public Guid UserId { get; private set; }
    public Guid RoleId { get; private set; }
    public string? AssignedByUserId { get; private set; }
    public DateTime? ExpiresAt { get; private set; }
    public bool IsActive { get; private set; }
    public string? Scope { get; private set; } // e.g., "Department:123" for scoped permissions

    // Navigation properties
    public Role Role { get; private set; } = null!;

    private UserRole() { } // EF Core constructor

    public static UserRole Create(
        Guid userId,
        Guid roleId,
        string? assignedByUserId = null,
        DateTime? expiresAt = null,
        string? scope = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required", nameof(userId));

        if (roleId == Guid.Empty)
            throw new ArgumentException("Role ID is required", nameof(roleId));

        return new UserRole
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            RoleId = roleId,
            AssignedByUserId = assignedByUserId,
            ExpiresAt = expiresAt,
            IsActive = true,
            Scope = scope
        };
    }

    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;

    public bool IsEffective => IsActive && !IsExpired;

    public void Deactivate()
    {
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void ExtendExpiration(DateTime newExpiresAt)
    {
        if (newExpiresAt <= DateTime.UtcNow)
            throw new ArgumentException("Expiration date must be in the future", nameof(newExpiresAt));

        ExpiresAt = newExpiresAt;
    }

    public void RemoveExpiration()
    {
        ExpiresAt = null;
    }

    public void SetScope(string? scope)
    {
        Scope = scope;
    }
}

/// <summary>
/// Predefined system roles.
/// "I bent my Wookie." - These roles won't bend under pressure!
/// </summary>
public static class SystemRoles
{
    public const string Administrator = "Administrator";
    public const string Manager = "Manager";
    public const string Approver = "Approver";
    public const string Analyst = "Analyst";
    public const string Viewer = "Viewer";
    public const string HrAdmin = "HRAdmin";
    public const string FinanceAdmin = "FinanceAdmin";

    /// <summary>
    /// Gets permissions for Administrator role (full access).
    /// </summary>
    public static Permission AdministratorPermissions =>
        Permission.EmployeeView | Permission.EmployeeCreate | Permission.EmployeeEdit | Permission.EmployeeDelete |
        Permission.EmployeeExport | Permission.EmployeeImport |
        Permission.DepartmentView | Permission.DepartmentCreate | Permission.DepartmentEdit | Permission.DepartmentDelete |
        Permission.PlanView | Permission.PlanCreate | Permission.PlanEdit | Permission.PlanDelete |
        Permission.PlanActivate | Permission.PlanDeactivate |
        Permission.CalculationView | Permission.CalculationRun | Permission.CalculationRecalculate | Permission.CalculationExport |
        Permission.ApprovalView | Permission.ApprovalApprove | Permission.ApprovalReject | Permission.ApprovalBulkProcess |
        Permission.ReportView | Permission.ReportGenerate | Permission.ReportExport | Permission.ReportSchedule |
        Permission.DashboardView | Permission.DashboardExport |
        Permission.AuditView | Permission.AuditExport;

    /// <summary>
    /// Gets permissions for Manager role.
    /// </summary>
    public static Permission ManagerPermissions =>
        Permission.EmployeeView | Permission.EmployeeEdit | Permission.EmployeeExport |
        Permission.DepartmentView |
        Permission.PlanView |
        Permission.CalculationView | Permission.CalculationRun | Permission.CalculationExport |
        Permission.ApprovalView | Permission.ApprovalApprove | Permission.ApprovalReject |
        Permission.ReportView | Permission.ReportGenerate | Permission.ReportExport |
        Permission.DashboardView | Permission.DashboardExport;

    /// <summary>
    /// Gets permissions for Approver role.
    /// </summary>
    public static Permission ApproverPermissions =>
        Permission.EmployeeView |
        Permission.CalculationView |
        Permission.ApprovalView | Permission.ApprovalApprove | Permission.ApprovalReject |
        Permission.ReportView |
        Permission.DashboardView;

    /// <summary>
    /// Gets permissions for Analyst role.
    /// </summary>
    public static Permission AnalystPermissions =>
        Permission.EmployeeView | Permission.EmployeeExport |
        Permission.DepartmentView |
        Permission.PlanView |
        Permission.CalculationView | Permission.CalculationExport |
        Permission.ApprovalView |
        Permission.ReportView | Permission.ReportGenerate | Permission.ReportExport | Permission.ReportSchedule |
        Permission.DashboardView | Permission.DashboardExport |
        Permission.AuditView;

    /// <summary>
    /// Gets permissions for Viewer role (read-only).
    /// </summary>
    public static Permission ViewerPermissions =>
        Permission.EmployeeView |
        Permission.DepartmentView |
        Permission.PlanView |
        Permission.CalculationView |
        Permission.ApprovalView |
        Permission.ReportView |
        Permission.DashboardView;

    /// <summary>
    /// Gets permissions for HR Admin role.
    /// </summary>
    public static Permission HrAdminPermissions =>
        Permission.EmployeeView | Permission.EmployeeCreate | Permission.EmployeeEdit | Permission.EmployeeDelete |
        Permission.EmployeeExport | Permission.EmployeeImport |
        Permission.DepartmentView | Permission.DepartmentCreate | Permission.DepartmentEdit |
        Permission.PlanView |
        Permission.CalculationView |
        Permission.ReportView | Permission.ReportGenerate | Permission.ReportExport |
        Permission.DashboardView;

    /// <summary>
    /// Gets permissions for Finance Admin role.
    /// </summary>
    public static Permission FinanceAdminPermissions =>
        Permission.EmployeeView | Permission.EmployeeExport |
        Permission.DepartmentView |
        Permission.PlanView | Permission.PlanCreate | Permission.PlanEdit |
        Permission.CalculationView | Permission.CalculationRun | Permission.CalculationRecalculate | Permission.CalculationExport |
        Permission.ApprovalView | Permission.ApprovalApprove | Permission.ApprovalReject | Permission.ApprovalBulkProcess |
        Permission.ReportView | Permission.ReportGenerate | Permission.ReportExport | Permission.ReportSchedule |
        Permission.DashboardView | Permission.DashboardExport;
}
