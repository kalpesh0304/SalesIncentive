using Dorise.Incentive.Application.Security.DTOs;
using Dorise.Incentive.Application.Security.Services;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Security;

/// <summary>
/// Implementation of role management service.
/// "When I grow up, I'm going to Bovine University!" - Roles grow your access privileges!
/// </summary>
public class RoleManagementService : IRoleManagementService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<RoleManagementService> _logger;

    public RoleManagementService(
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        IMemoryCache cache,
        ILogger<RoleManagementService> logger)
    {
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<RoleDto> CreateRoleAsync(
        CreateRoleRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        // Check for duplicate name
        if (await _roleRepository.ExistsWithNameAsync(request.Name, cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException($"Role with name '{request.Name}' already exists");
        }

        var role = Role.Create(request.Name, request.Description);
        role.SetPermissions(request.Permissions);

        foreach (var ep in request.ExtendedPermissions)
        {
            role.GrantExtendedPermission(ep);
        }

        await _roleRepository.AddAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogSecurityEventAsync(
            SecurityEventTypes.RoleCreated,
            null,
            $"Role '{role.Name}' created",
            additionalData: new Dictionary<string, object> { ["RoleId"] = role.Id },
            cancellationToken: cancellationToken);

        _logger.LogInformation("Role {RoleName} created by {CreatedBy}", role.Name, createdBy);

        return MapToDto(role);
    }

    public async Task<RoleDto?> GetRoleByIdAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
        if (role == null) return null;

        var userCount = (await _userRoleRepository.GetByRoleIdAsync(roleId, cancellationToken))
            .Count(ur => ur.IsEffective);

        return MapToDto(role, userCount);
    }

    public async Task<RoleDto?> GetRoleByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByNameAsync(name, cancellationToken);
        if (role == null) return null;

        var userCount = (await _userRoleRepository.GetByRoleIdAsync(role.Id, cancellationToken))
            .Count(ur => ur.IsEffective);

        return MapToDto(role, userCount);
    }

    public async Task<IReadOnlyList<RoleSummaryDto>> GetAllRolesAsync(
        CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetAllAsync(cancellationToken);
        var result = new List<RoleSummaryDto>();

        foreach (var role in roles)
        {
            var userCount = (await _userRoleRepository.GetByRoleIdAsync(role.Id, cancellationToken))
                .Count(ur => ur.IsEffective);

            result.Add(new RoleSummaryDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsSystem = role.IsSystem,
                IsActive = role.IsActive,
                PermissionCount = role.GetGrantedPermissions().Count + role.ExtendedPermissions.Count,
                UserCount = userCount
            });
        }

        return result;
    }

    public async Task<IReadOnlyList<RoleSummaryDto>> GetActiveRolesAsync(
        CancellationToken cancellationToken = default)
    {
        var roles = await _roleRepository.GetActiveRolesAsync(cancellationToken);
        var result = new List<RoleSummaryDto>();

        foreach (var role in roles)
        {
            var userCount = (await _userRoleRepository.GetByRoleIdAsync(role.Id, cancellationToken))
                .Count(ur => ur.IsEffective);

            result.Add(new RoleSummaryDto
            {
                Id = role.Id,
                Name = role.Name,
                Description = role.Description,
                IsSystem = role.IsSystem,
                IsActive = role.IsActive,
                PermissionCount = role.GetGrantedPermissions().Count + role.ExtendedPermissions.Count,
                UserCount = userCount
            });
        }

        return result;
    }

    public async Task<RoleDto> UpdateRoleAsync(
        Guid roleId,
        UpdateRoleRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken)
            ?? throw new InvalidOperationException($"Role {roleId} not found");

        if (await _roleRepository.ExistsWithNameAsync(request.Name, roleId, cancellationToken))
        {
            throw new InvalidOperationException($"Role with name '{request.Name}' already exists");
        }

        role.UpdateDetails(request.Name, request.Description);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogSecurityEventAsync(
            SecurityEventTypes.RoleModified,
            null,
            $"Role '{role.Name}' details updated",
            additionalData: new Dictionary<string, object> { ["RoleId"] = role.Id },
            cancellationToken: cancellationToken);

        _logger.LogInformation("Role {RoleId} updated by {ModifiedBy}", roleId, modifiedBy);

        return MapToDto(role);
    }

    public async Task<RoleDto> UpdateRolePermissionsAsync(
        Guid roleId,
        UpdateRolePermissionsRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken)
            ?? throw new InvalidOperationException($"Role {roleId} not found");

        var oldPermissions = role.Permissions;
        role.SetPermissions(request.Permissions);

        // Update extended permissions
        var currentExtended = role.ExtendedPermissions.ToList();
        foreach (var ep in currentExtended)
        {
            if (!request.ExtendedPermissions.Contains(ep))
            {
                role.RevokeExtendedPermission(ep);
            }
        }
        foreach (var ep in request.ExtendedPermissions)
        {
            role.GrantExtendedPermission(ep);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate permission cache for all users with this role
        var userRoles = await _userRoleRepository.GetByRoleIdAsync(roleId, cancellationToken);
        foreach (var ur in userRoles)
        {
            _cache.Remove($"permissions:{ur.UserId}");
            _cache.Remove($"extended-permissions:{ur.UserId}");
        }

        await _auditService.LogSecurityEventAsync(
            SecurityEventTypes.PermissionGranted,
            null,
            $"Role '{role.Name}' permissions updated",
            additionalData: new Dictionary<string, object>
            {
                ["RoleId"] = role.Id,
                ["OldPermissions"] = oldPermissions.ToString(),
                ["NewPermissions"] = request.Permissions.ToString()
            },
            cancellationToken: cancellationToken);

        _logger.LogInformation("Role {RoleId} permissions updated by {ModifiedBy}", roleId, modifiedBy);

        return MapToDto(role);
    }

    public async Task ActivateRoleAsync(
        Guid roleId,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken)
            ?? throw new InvalidOperationException($"Role {roleId} not found");

        role.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogSecurityEventAsync(
            SecurityEventTypes.RoleActivated,
            null,
            $"Role '{role.Name}' activated",
            additionalData: new Dictionary<string, object> { ["RoleId"] = role.Id },
            cancellationToken: cancellationToken);

        _logger.LogInformation("Role {RoleId} activated by {ModifiedBy}", roleId, modifiedBy);
    }

    public async Task DeactivateRoleAsync(
        Guid roleId,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken)
            ?? throw new InvalidOperationException($"Role {roleId} not found");

        role.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate permission cache for all users with this role
        var userRoles = await _userRoleRepository.GetByRoleIdAsync(roleId, cancellationToken);
        foreach (var ur in userRoles)
        {
            _cache.Remove($"permissions:{ur.UserId}");
            _cache.Remove($"extended-permissions:{ur.UserId}");
        }

        await _auditService.LogSecurityEventAsync(
            SecurityEventTypes.RoleDeactivated,
            null,
            $"Role '{role.Name}' deactivated",
            additionalData: new Dictionary<string, object> { ["RoleId"] = role.Id },
            cancellationToken: cancellationToken);

        _logger.LogInformation("Role {RoleId} deactivated by {ModifiedBy}", roleId, modifiedBy);
    }

    public async Task DeleteRoleAsync(
        Guid roleId,
        string deletedBy,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken)
            ?? throw new InvalidOperationException($"Role {roleId} not found");

        if (role.IsSystem)
        {
            throw new InvalidOperationException("Cannot delete system roles");
        }

        var userRoles = await _userRoleRepository.GetByRoleIdAsync(roleId, cancellationToken);
        if (userRoles.Any(ur => ur.IsEffective))
        {
            throw new InvalidOperationException("Cannot delete role with active user assignments");
        }

        await _roleRepository.DeleteAsync(role, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogSecurityEventAsync(
            SecurityEventTypes.RoleDeleted,
            null,
            $"Role '{role.Name}' deleted",
            additionalData: new Dictionary<string, object> { ["RoleId"] = role.Id },
            cancellationToken: cancellationToken);

        _logger.LogInformation("Role {RoleId} deleted by {DeletedBy}", roleId, deletedBy);
    }

    public IReadOnlyList<PermissionCategoryDto> GetAvailablePermissions()
    {
        var categories = new Dictionary<string, PermissionCategoryDto>();

        // Add standard permissions
        foreach (Permission permission in Enum.GetValues(typeof(Permission)))
        {
            if (permission == Permission.None) continue;

            var name = permission.ToString();
            var category = GetPermissionCategory(name);

            if (!categories.ContainsKey(category))
            {
                categories[category] = new PermissionCategoryDto
                {
                    Name = category,
                    Permissions = new List<PermissionInfoDto>(),
                    ExtendedPermissions = new List<ExtendedPermissionInfoDto>()
                };
            }

            ((List<PermissionInfoDto>)categories[category].Permissions).Add(new PermissionInfoDto
            {
                Name = name,
                Category = category,
                Description = GetPermissionDescription(name),
                Value = permission
            });
        }

        // Add extended permissions
        foreach (ExtendedPermission permission in Enum.GetValues(typeof(ExtendedPermission)))
        {
            var name = permission.ToString();
            var category = GetExtendedPermissionCategory(name);

            if (!categories.ContainsKey(category))
            {
                categories[category] = new PermissionCategoryDto
                {
                    Name = category,
                    Permissions = new List<PermissionInfoDto>(),
                    ExtendedPermissions = new List<ExtendedPermissionInfoDto>()
                };
            }

            ((List<ExtendedPermissionInfoDto>)categories[category].ExtendedPermissions).Add(new ExtendedPermissionInfoDto
            {
                Name = name,
                Category = category,
                Description = GetExtendedPermissionDescription(name),
                Value = permission
            });
        }

        return categories.Values.ToList();
    }

    public async Task<RoleAccessMatrixDto> GetRoleAccessMatrixAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken)
            ?? throw new InvalidOperationException($"Role {roleId} not found");

        var accessByCategory = new Dictionary<string, List<string>>();

        foreach (Permission permission in Enum.GetValues(typeof(Permission)))
        {
            if (permission == Permission.None) continue;
            if (!role.HasPermission(permission)) continue;

            var name = permission.ToString();
            var category = GetPermissionCategory(name);

            if (!accessByCategory.ContainsKey(category))
            {
                accessByCategory[category] = new List<string>();
            }
            accessByCategory[category].Add(name);
        }

        foreach (var ep in role.ExtendedPermissions)
        {
            var name = ep.ToString();
            var category = GetExtendedPermissionCategory(name);

            if (!accessByCategory.ContainsKey(category))
            {
                accessByCategory[category] = new List<string>();
            }
            accessByCategory[category].Add(name);
        }

        return new RoleAccessMatrixDto
        {
            RoleId = roleId,
            RoleName = role.Name,
            AccessByCategory = accessByCategory.ToDictionary(
                kvp => kvp.Key,
                kvp => (IReadOnlyList<string>)kvp.Value)
        };
    }

    public async Task<RoleComparisonDto> CompareRolesAsync(
        Guid roleId1,
        Guid roleId2,
        CancellationToken cancellationToken = default)
    {
        var role1 = await _roleRepository.GetByIdAsync(roleId1, cancellationToken)
            ?? throw new InvalidOperationException($"Role {roleId1} not found");

        var role2 = await _roleRepository.GetByIdAsync(roleId2, cancellationToken)
            ?? throw new InvalidOperationException($"Role {roleId2} not found");

        var permissions1 = role1.GetGrantedPermissions().Select(p => p.ToString()).ToHashSet();
        var permissions2 = role2.GetGrantedPermissions().Select(p => p.ToString()).ToHashSet();

        var common = permissions1.Intersect(permissions2).ToList();
        var onlyIn1 = permissions1.Except(permissions2).ToList();
        var onlyIn2 = permissions2.Except(permissions1).ToList();

        return new RoleComparisonDto
        {
            Role1 = new RoleSummaryDto
            {
                Id = role1.Id,
                Name = role1.Name,
                Description = role1.Description,
                IsSystem = role1.IsSystem,
                IsActive = role1.IsActive,
                PermissionCount = permissions1.Count
            },
            Role2 = new RoleSummaryDto
            {
                Id = role2.Id,
                Name = role2.Name,
                Description = role2.Description,
                IsSystem = role2.IsSystem,
                IsActive = role2.IsActive,
                PermissionCount = permissions2.Count
            },
            CommonPermissions = common,
            OnlyInRole1 = onlyIn1,
            OnlyInRole2 = onlyIn2
        };
    }

    // ============== Private Helper Methods ==============

    private static RoleDto MapToDto(Role role, int userCount = 0)
    {
        return new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Description = role.Description,
            IsSystem = role.IsSystem,
            IsActive = role.IsActive,
            Permissions = role.Permissions,
            ExtendedPermissions = role.ExtendedPermissions.ToList(),
            PermissionNames = role.GetGrantedPermissions().Select(p => p.ToString()).ToList(),
            UserCount = userCount,
            CreatedAt = role.CreatedAt,
            CreatedBy = role.CreatedBy,
            ModifiedAt = role.ModifiedAt,
            ModifiedBy = role.ModifiedBy
        };
    }

    private static string GetPermissionCategory(string permissionName)
    {
        if (permissionName.StartsWith("Employee")) return "Employee";
        if (permissionName.StartsWith("Department")) return "Department";
        if (permissionName.StartsWith("Plan")) return "IncentivePlan";
        if (permissionName.StartsWith("Calculation")) return "Calculation";
        if (permissionName.StartsWith("Approval")) return "Approval";
        if (permissionName.StartsWith("Report")) return "Report";
        if (permissionName.StartsWith("Dashboard")) return "Dashboard";
        if (permissionName.StartsWith("Audit")) return "Audit";
        return "Other";
    }

    private static string GetExtendedPermissionCategory(string permissionName)
    {
        if (permissionName.StartsWith("System") || permissionName.StartsWith("User") ||
            permissionName.StartsWith("Role") || permissionName.StartsWith("Security"))
            return "System";
        if (permissionName.StartsWith("Integration")) return "Integration";
        if (permissionName.StartsWith("Notification")) return "Notification";
        if (permissionName.StartsWith("Data")) return "DataManagement";
        return "Other";
    }

    private static string GetPermissionDescription(string permissionName)
    {
        return permissionName switch
        {
            "EmployeeView" => "View employee information",
            "EmployeeCreate" => "Create new employees",
            "EmployeeEdit" => "Edit employee information",
            "EmployeeDelete" => "Delete employees",
            "EmployeeExport" => "Export employee data",
            "EmployeeImport" => "Import employee data",
            "DepartmentView" => "View departments",
            "DepartmentCreate" => "Create departments",
            "DepartmentEdit" => "Edit departments",
            "DepartmentDelete" => "Delete departments",
            "PlanView" => "View incentive plans",
            "PlanCreate" => "Create incentive plans",
            "PlanEdit" => "Edit incentive plans",
            "PlanDelete" => "Delete incentive plans",
            "PlanActivate" => "Activate incentive plans",
            "PlanDeactivate" => "Deactivate incentive plans",
            "CalculationView" => "View calculations",
            "CalculationRun" => "Run calculations",
            "CalculationRecalculate" => "Recalculate incentives",
            "CalculationExport" => "Export calculation data",
            "ApprovalView" => "View approvals",
            "ApprovalApprove" => "Approve calculations",
            "ApprovalReject" => "Reject calculations",
            "ApprovalBulkProcess" => "Bulk process approvals",
            "ReportView" => "View reports",
            "ReportGenerate" => "Generate reports",
            "ReportExport" => "Export reports",
            "ReportSchedule" => "Schedule report generation",
            "DashboardView" => "View dashboard",
            "DashboardExport" => "Export dashboard data",
            "AuditView" => "View audit logs",
            "AuditExport" => "Export audit logs",
            _ => permissionName
        };
    }

    private static string GetExtendedPermissionDescription(string permissionName)
    {
        return permissionName switch
        {
            "SystemConfig" => "Configure system settings",
            "UserManage" => "Manage user accounts",
            "RoleManage" => "Manage roles and permissions",
            "SecurityManage" => "Manage security settings",
            "IntegrationErp" => "Access ERP integration",
            "IntegrationHr" => "Access HR integration",
            "IntegrationPayroll" => "Access payroll integration",
            "IntegrationConfig" => "Configure integrations",
            "NotificationManage" => "Manage notifications",
            "NotificationTemplateEdit" => "Edit notification templates",
            "DataPurge" => "Purge data",
            "DataBackup" => "Create data backups",
            "DataRestore" => "Restore data from backups",
            _ => permissionName
        };
    }
}
