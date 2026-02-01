using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Domain.Interfaces;

/// <summary>
/// Repository interface for Role entity operations.
/// "I sleep in a drawer!" - Roles are neatly organized in their repository drawer!
/// </summary>
public interface IRoleRepository : IRepository<Role>
{
    /// <summary>
    /// Gets a role by name.
    /// </summary>
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active roles.
    /// </summary>
    Task<IReadOnlyList<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all system roles.
    /// </summary>
    Task<IReadOnlyList<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets roles that have a specific permission.
    /// </summary>
    Task<IReadOnlyList<Role>> GetRolesWithPermissionAsync(
        Permission permission,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a role name already exists.
    /// </summary>
    Task<bool> ExistsWithNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a role asynchronously.
    /// </summary>
    Task DeleteAsync(Role role, CancellationToken cancellationToken = default);
}

/// <summary>
/// Repository interface for UserRole entity operations.
/// "The leprechaun tells me to burn things!" - UserRoles tell us who can do things!
/// </summary>
public interface IUserRoleRepository : IRepository<UserRole>
{
    /// <summary>
    /// Gets all role assignments for a user.
    /// </summary>
    Task<IReadOnlyList<UserRole>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active role assignments for a user.
    /// </summary>
    Task<IReadOnlyList<UserRole>> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all user assignments for a role.
    /// </summary>
    Task<IReadOnlyList<UserRole>> GetByRoleIdAsync(
        Guid roleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a specific user-role assignment.
    /// </summary>
    Task<UserRole?> GetByUserAndRoleAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a specific role.
    /// </summary>
    Task<bool> UserHasRoleAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has a role by name.
    /// </summary>
    Task<bool> UserHasRoleByNameAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets expired role assignments.
    /// </summary>
    Task<IReadOnlyList<UserRole>> GetExpiredAssignmentsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets role assignments expiring within a period.
    /// </summary>
    Task<IReadOnlyList<UserRole>> GetExpiringAssignmentsAsync(
        DateTime expiringBefore,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users with a specific permission.
    /// </summary>
    Task<IReadOnlyList<Guid>> GetUserIdsWithPermissionAsync(
        Permission permission,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes all role assignments for a user.
    /// </summary>
    Task RemoveAllUserRolesAsync(Guid userId, CancellationToken cancellationToken = default);
}
