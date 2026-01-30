using Dorise.Incentive.Application.Security.DTOs;
using Dorise.Incentive.Application.Security.Services;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Security;

/// <summary>
/// Implementation of authorization service for permission checks.
/// "I found a moonrock in my nose!" - Finding permissions in the right places!
/// </summary>
public class AuthorizationService : IAuthorizationService
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AuthorizationService> _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    public AuthorizationService(
        IUserRoleRepository userRoleRepository,
        IRoleRepository roleRepository,
        IMemoryCache cache,
        ILogger<AuthorizationService> logger)
    {
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(
        Guid userId,
        Permission permission,
        CancellationToken cancellationToken = default)
    {
        var effectivePermissions = await GetEffectivePermissionsAsync(userId, cancellationToken);
        return (effectivePermissions & permission) == permission;
    }

    public async Task<bool> HasExtendedPermissionAsync(
        Guid userId,
        ExtendedPermission permission,
        CancellationToken cancellationToken = default)
    {
        var extendedPermissions = await GetEffectiveExtendedPermissionsAsync(userId, cancellationToken);
        return extendedPermissions.Contains(permission);
    }

    public async Task<bool> HasAnyPermissionAsync(
        Guid userId,
        Permission permissions,
        CancellationToken cancellationToken = default)
    {
        var effectivePermissions = await GetEffectivePermissionsAsync(userId, cancellationToken);
        return (effectivePermissions & permissions) != Permission.None;
    }

    public async Task<bool> HasAllPermissionsAsync(
        Guid userId,
        Permission permissions,
        CancellationToken cancellationToken = default)
    {
        var effectivePermissions = await GetEffectivePermissionsAsync(userId, cancellationToken);
        return (effectivePermissions & permissions) == permissions;
    }

    public async Task<PermissionCheckResult> CheckPermissionAsync(
        PermissionCheckRequest request,
        CancellationToken cancellationToken = default)
    {
        var userRoles = await GetActiveUserRolesAsync(request.UserId, cancellationToken);
        var grantingRoles = new List<string>();
        var isAllowed = false;

        foreach (var userRole in userRoles)
        {
            if (userRole.Role.HasPermission(request.Permission))
            {
                // Check scope if specified
                if (string.IsNullOrEmpty(request.Scope) ||
                    string.IsNullOrEmpty(userRole.Scope) ||
                    userRole.Scope == request.Scope ||
                    userRole.Scope == "*")
                {
                    grantingRoles.Add(userRole.Role.Name);
                    isAllowed = true;
                }
            }
        }

        return new PermissionCheckResult
        {
            IsAllowed = isAllowed,
            DeniedReason = isAllowed ? null : $"User does not have permission: {request.Permission}",
            GrantingRoles = grantingRoles
        };
    }

    public async Task<PermissionCheckResult> CheckExtendedPermissionAsync(
        ExtendedPermissionCheckRequest request,
        CancellationToken cancellationToken = default)
    {
        var userRoles = await GetActiveUserRolesAsync(request.UserId, cancellationToken);
        var grantingRoles = new List<string>();
        var isAllowed = false;

        foreach (var userRole in userRoles)
        {
            if (userRole.Role.HasExtendedPermission(request.Permission))
            {
                if (string.IsNullOrEmpty(request.Scope) ||
                    string.IsNullOrEmpty(userRole.Scope) ||
                    userRole.Scope == request.Scope ||
                    userRole.Scope == "*")
                {
                    grantingRoles.Add(userRole.Role.Name);
                    isAllowed = true;
                }
            }
        }

        return new PermissionCheckResult
        {
            IsAllowed = isAllowed,
            DeniedReason = isAllowed ? null : $"User does not have extended permission: {request.Permission}",
            GrantingRoles = grantingRoles
        };
    }

    public async Task<UserPermissionsDto> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userRoles = await GetActiveUserRolesAsync(userId, cancellationToken);

        var effectivePermissions = Permission.None;
        var effectiveExtendedPermissions = new HashSet<ExtendedPermission>();
        var roles = new List<RoleSummaryDto>();

        foreach (var userRole in userRoles)
        {
            effectivePermissions |= userRole.Role.Permissions;
            foreach (var ep in userRole.Role.ExtendedPermissions)
            {
                effectiveExtendedPermissions.Add(ep);
            }

            roles.Add(new RoleSummaryDto
            {
                Id = userRole.RoleId,
                Name = userRole.Role.Name,
                Description = userRole.Role.Description,
                IsSystem = userRole.Role.IsSystem,
                IsActive = userRole.Role.IsActive,
                PermissionCount = userRole.Role.GetGrantedPermissions().Count,
                UserCount = 0 // Not needed here
            });
        }

        var permissionNames = GetPermissionNames(effectivePermissions);

        return new UserPermissionsDto
        {
            UserId = userId,
            UserName = "Unknown", // Would need employee repo to get name
            Email = null,
            Roles = roles,
            EffectivePermissions = effectivePermissions,
            EffectiveExtendedPermissions = effectiveExtendedPermissions.ToList(),
            PermissionNames = permissionNames,
            IsAdmin = roles.Any(r => r.Name == SystemRoles.Administrator)
        };
    }

    public async Task<SecurityContextDto> GetSecurityContextAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var permissions = await GetUserPermissionsAsync(userId, cancellationToken);
        var userRoles = await GetActiveUserRolesAsync(userId, cancellationToken);

        var scopes = userRoles
            .Where(ur => !string.IsNullOrEmpty(ur.Scope))
            .GroupBy(ur => ur.Role.Name)
            .ToDictionary(g => g.Key, g => g.First().Scope);

        return new SecurityContextDto
        {
            UserId = userId,
            UserName = permissions.UserName,
            Email = permissions.Email,
            Roles = permissions.Roles.Select(r => r.Name).ToList(),
            Permissions = permissions.EffectivePermissions,
            ExtendedPermissions = permissions.EffectiveExtendedPermissions,
            Scopes = scopes!,
            IsAuthenticated = true,
            IsAdmin = permissions.IsAdmin,
            SessionExpiresAt = null
        };
    }

    public async Task<UserAccessMatrixDto> GetUserAccessMatrixAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var permissions = await GetUserPermissionsAsync(userId, cancellationToken);
        var accessByCategory = BuildAccessMatrix(permissions.EffectivePermissions, permissions.EffectiveExtendedPermissions);

        return new UserAccessMatrixDto
        {
            UserId = userId,
            UserName = permissions.UserName,
            AccessByCategory = accessByCategory
        };
    }

    public async Task<bool> IsInRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        return await _userRoleRepository.UserHasRoleByNameAsync(userId, roleName, cancellationToken);
    }

    public async Task<bool> IsInAnyRoleAsync(
        Guid userId,
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken = default)
    {
        foreach (var roleName in roleNames)
        {
            if (await _userRoleRepository.UserHasRoleByNameAsync(userId, roleName, cancellationToken))
            {
                return true;
            }
        }
        return false;
    }

    public async Task<bool> IsAdminAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await IsInRoleAsync(userId, SystemRoles.Administrator, cancellationToken);
    }

    // ============== Private Helper Methods ==============

    private async Task<Permission> GetEffectivePermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var cacheKey = $"permissions:{userId}";

        if (_cache.TryGetValue(cacheKey, out Permission cached))
        {
            return cached;
        }

        var userRoles = await GetActiveUserRolesAsync(userId, cancellationToken);
        var effectivePermissions = userRoles.Aggregate(
            Permission.None,
            (current, userRole) => current | userRole.Role.Permissions);

        _cache.Set(cacheKey, effectivePermissions, CacheDuration);
        return effectivePermissions;
    }

    private async Task<IReadOnlyList<ExtendedPermission>> GetEffectiveExtendedPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"extended-permissions:{userId}";

        if (_cache.TryGetValue(cacheKey, out IReadOnlyList<ExtendedPermission>? cached) && cached != null)
        {
            return cached;
        }

        var userRoles = await GetActiveUserRolesAsync(userId, cancellationToken);
        var effectiveExtendedPermissions = userRoles
            .SelectMany(ur => ur.Role.ExtendedPermissions)
            .Distinct()
            .ToList();

        _cache.Set(cacheKey, (IReadOnlyList<ExtendedPermission>)effectiveExtendedPermissions, CacheDuration);
        return effectiveExtendedPermissions;
    }

    private async Task<IReadOnlyList<UserRole>> GetActiveUserRolesAsync(Guid userId, CancellationToken cancellationToken)
    {
        var userRoles = await _userRoleRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        return userRoles.Where(ur => ur.IsEffective && ur.Role.IsActive).ToList();
    }

    private static IReadOnlyList<string> GetPermissionNames(Permission permissions)
    {
        var names = new List<string>();
        foreach (Permission permission in Enum.GetValues(typeof(Permission)))
        {
            if (permission != Permission.None && (permissions & permission) == permission)
            {
                names.Add(permission.ToString());
            }
        }
        return names;
    }

    private static IReadOnlyDictionary<string, IReadOnlyList<string>> BuildAccessMatrix(
        Permission permissions,
        IReadOnlyList<ExtendedPermission> extendedPermissions)
    {
        var matrix = new Dictionary<string, IReadOnlyList<string>>();

        // Group standard permissions by category
        var permissionsByCategory = new Dictionary<string, List<string>>
        {
            ["Employee"] = new(),
            ["Department"] = new(),
            ["IncentivePlan"] = new(),
            ["Calculation"] = new(),
            ["Approval"] = new(),
            ["Report"] = new(),
            ["Dashboard"] = new(),
            ["Audit"] = new(),
            ["System"] = new(),
            ["Integration"] = new(),
            ["Notification"] = new(),
            ["DataManagement"] = new()
        };

        foreach (Permission permission in Enum.GetValues(typeof(Permission)))
        {
            if (permission == Permission.None) continue;
            if ((permissions & permission) != permission) continue;

            var name = permission.ToString();
            if (name.StartsWith("Employee")) permissionsByCategory["Employee"].Add(name);
            else if (name.StartsWith("Department")) permissionsByCategory["Department"].Add(name);
            else if (name.StartsWith("Plan")) permissionsByCategory["IncentivePlan"].Add(name);
            else if (name.StartsWith("Calculation")) permissionsByCategory["Calculation"].Add(name);
            else if (name.StartsWith("Approval")) permissionsByCategory["Approval"].Add(name);
            else if (name.StartsWith("Report")) permissionsByCategory["Report"].Add(name);
            else if (name.StartsWith("Dashboard")) permissionsByCategory["Dashboard"].Add(name);
            else if (name.StartsWith("Audit")) permissionsByCategory["Audit"].Add(name);
        }

        // Add extended permissions
        foreach (var ep in extendedPermissions)
        {
            var name = ep.ToString();
            if (name.StartsWith("System") || name.StartsWith("User") || name.StartsWith("Role") || name.StartsWith("Security"))
                permissionsByCategory["System"].Add(name);
            else if (name.StartsWith("Integration"))
                permissionsByCategory["Integration"].Add(name);
            else if (name.StartsWith("Notification"))
                permissionsByCategory["Notification"].Add(name);
            else if (name.StartsWith("Data"))
                permissionsByCategory["DataManagement"].Add(name);
        }

        // Only include categories that have permissions
        foreach (var kvp in permissionsByCategory.Where(kvp => kvp.Value.Count > 0))
        {
            matrix[kvp.Key] = kvp.Value;
        }

        return matrix;
    }
}
