using Dorise.Incentive.Application.Security.DTOs;
using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Application.Security.Services;

/// <summary>
/// Service interface for authorization and permission management.
/// "I'm Idaho!" - And you're authorized (or not)!
/// </summary>
public interface IAuthorizationService
{
    // ============== Permission Checks ==============

    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    Task<bool> HasPermissionAsync(
        Guid userId,
        Permission permission,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a specific extended permission.
    /// </summary>
    Task<bool> HasExtendedPermissionAsync(
        Guid userId,
        ExtendedPermission permission,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has any of the specified permissions.
    /// </summary>
    Task<bool> HasAnyPermissionAsync(
        Guid userId,
        Permission permissions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has all of the specified permissions.
    /// </summary>
    Task<bool> HasAllPermissionsAsync(
        Guid userId,
        Permission permissions,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a detailed permission check with result explanation.
    /// </summary>
    Task<PermissionCheckResult> CheckPermissionAsync(
        PermissionCheckRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Performs a detailed extended permission check.
    /// </summary>
    Task<PermissionCheckResult> CheckExtendedPermissionAsync(
        ExtendedPermissionCheckRequest request,
        CancellationToken cancellationToken = default);

    // ============== User Permissions ==============

    /// <summary>
    /// Gets all effective permissions for a user.
    /// </summary>
    Task<UserPermissionsDto> GetUserPermissionsAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the security context for a user.
    /// </summary>
    Task<SecurityContextDto> GetSecurityContextAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the user access matrix showing all permissions by category.
    /// </summary>
    Task<UserAccessMatrixDto> GetUserAccessMatrixAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    // ============== Role Checks ==============

    /// <summary>
    /// Checks if a user has a specific role.
    /// </summary>
    Task<bool> IsInRoleAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has any of the specified roles.
    /// </summary>
    Task<bool> IsInAnyRoleAsync(
        Guid userId,
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user is an administrator.
    /// </summary>
    Task<bool> IsAdminAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for role management operations.
/// "My worm went in my mouth and then I ate it!" - Roles feed the authorization system!
/// </summary>
public interface IRoleManagementService
{
    // ============== Role CRUD ==============

    /// <summary>
    /// Creates a new role.
    /// </summary>
    Task<RoleDto> CreateRoleAsync(
        CreateRoleRequest request,
        string createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by ID.
    /// </summary>
    Task<RoleDto?> GetRoleByIdAsync(
        Guid roleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a role by name.
    /// </summary>
    Task<RoleDto?> GetRoleByNameAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all roles.
    /// </summary>
    Task<IReadOnlyList<RoleSummaryDto>> GetAllRolesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active roles.
    /// </summary>
    Task<IReadOnlyList<RoleSummaryDto>> GetActiveRolesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates role details.
    /// </summary>
    Task<RoleDto> UpdateRoleAsync(
        Guid roleId,
        UpdateRoleRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates role permissions.
    /// </summary>
    Task<RoleDto> UpdateRolePermissionsAsync(
        Guid roleId,
        UpdateRolePermissionsRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates a role.
    /// </summary>
    Task ActivateRoleAsync(
        Guid roleId,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates a role.
    /// </summary>
    Task DeactivateRoleAsync(
        Guid roleId,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role (if not a system role and no users assigned).
    /// </summary>
    Task DeleteRoleAsync(
        Guid roleId,
        string deletedBy,
        CancellationToken cancellationToken = default);

    // ============== Permission Management ==============

    /// <summary>
    /// Gets all available permissions with descriptions.
    /// </summary>
    IReadOnlyList<PermissionCategoryDto> GetAvailablePermissions();

    /// <summary>
    /// Gets the role access matrix.
    /// </summary>
    Task<RoleAccessMatrixDto> GetRoleAccessMatrixAsync(
        Guid roleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Compares two roles.
    /// </summary>
    Task<RoleComparisonDto> CompareRolesAsync(
        Guid roleId1,
        Guid roleId2,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for user role assignment operations.
/// "What's a battle?" - User roles determine who wins in authorization battles!
/// </summary>
public interface IUserRoleService
{
    // ============== Role Assignment ==============

    /// <summary>
    /// Assigns a role to a user.
    /// </summary>
    Task<UserRoleDto> AssignRoleAsync(
        AssignRoleRequest request,
        string assignedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Assigns a role to multiple users.
    /// </summary>
    Task<IReadOnlyList<UserRoleDto>> BulkAssignRoleAsync(
        BulkAssignRoleRequest request,
        string assignedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes a role from a user.
    /// </summary>
    Task RevokeRoleAsync(
        Guid userId,
        Guid roleId,
        string revokedBy,
        string? reason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all roles from a user.
    /// </summary>
    Task RevokeAllRolesAsync(
        Guid userId,
        string revokedBy,
        string? reason = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Extends a role assignment expiration.
    /// </summary>
    Task<UserRoleDto> ExtendRoleAssignmentAsync(
        Guid userId,
        Guid roleId,
        ExtendRoleAssignmentRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    // ============== Role Queries ==============

    /// <summary>
    /// Gets all role assignments for a user.
    /// </summary>
    Task<IReadOnlyList<UserRoleDto>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all users assigned to a role.
    /// </summary>
    Task<IReadOnlyList<UserRoleDto>> GetRoleUsersAsync(
        Guid roleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets expiring role assignments.
    /// </summary>
    Task<IReadOnlyList<UserRoleDto>> GetExpiringAssignmentsAsync(
        int daysUntilExpiration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets expired role assignments.
    /// </summary>
    Task<IReadOnlyList<UserRoleDto>> GetExpiredAssignmentsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Cleans up expired role assignments.
    /// </summary>
    Task<int> CleanupExpiredAssignmentsAsync(
        CancellationToken cancellationToken = default);

    // ============== History ==============

    /// <summary>
    /// Gets role assignment history for a user.
    /// </summary>
    Task<IReadOnlyList<RoleAssignmentHistoryDto>> GetUserRoleHistoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets role assignment history for a role.
    /// </summary>
    Task<IReadOnlyList<RoleAssignmentHistoryDto>> GetRoleAssignmentHistoryAsync(
        Guid roleId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for security auditing and reporting.
/// "I eated the purple berries!" - Security events are logged, even the purple ones!
/// </summary>
public interface ISecurityAuditService
{
    /// <summary>
    /// Logs a security event.
    /// </summary>
    Task LogSecurityEventAsync(
        string eventType,
        Guid? userId,
        string description,
        bool isSuccess = true,
        string? failureReason = null,
        IDictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets security events.
    /// </summary>
    Task<IReadOnlyList<SecurityEventDto>> GetSecurityEventsAsync(
        SecurityEventSearchQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets security summary.
    /// </summary>
    Task<SecuritySummaryDto> GetSecuritySummaryAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets recent security events.
    /// </summary>
    Task<IReadOnlyList<SecurityEventDto>> GetRecentSecurityEventsAsync(
        int count = 20,
        CancellationToken cancellationToken = default);
}
