using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dorise.Incentive.Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Role entity.
/// "I dress myself!" - Roles dress up users with permissions!
/// </summary>
public class RoleRepository : Repository<Role>, IRoleRepository
{
    public RoleRepository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.UserRoles)
            .FirstOrDefaultAsync(r => r.Name == name, cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetActiveRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Where(r => r.IsActive)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetSystemRolesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Where(r => r.IsSystem)
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Role>> GetRolesWithPermissionAsync(
        Permission permission,
        CancellationToken cancellationToken = default)
    {
        var roles = await _context.Roles
            .Where(r => r.IsActive)
            .ToListAsync(cancellationToken);

        // Filter in memory since Permissions is a flags enum
        return roles.Where(r => r.HasPermission(permission)).ToList();
    }

    public async Task<bool> ExistsWithNameAsync(
        string name,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Roles.Where(r => r.Name == name);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}

/// <summary>
/// Repository implementation for UserRole entity.
/// "That's my sandbox. I'm not allowed to go in the deep end." - Each user has their role boundaries!
/// </summary>
public class UserRoleRepository : Repository<UserRole>, IUserRoleRepository
{
    public UserRoleRepository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<UserRole>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserRole>> GetActiveByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.UserId == userId && ur.IsActive)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserRole>> GetByRoleIdAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.RoleId == roleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<UserRole?> GetByUserAndRoleAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId, cancellationToken);
    }

    public async Task<bool> UserHasRoleAsync(
        Guid userId,
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserRoles
            .AnyAsync(ur =>
                ur.UserId == userId &&
                ur.RoleId == roleId &&
                ur.IsActive &&
                (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow),
                cancellationToken);
    }

    public async Task<bool> UserHasRoleByNameAsync(
        Guid userId,
        string roleName,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .AnyAsync(ur =>
                ur.UserId == userId &&
                ur.Role.Name == roleName &&
                ur.IsActive &&
                (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow),
                cancellationToken);
    }

    public async Task<IReadOnlyList<UserRole>> GetExpiredAssignmentsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur => ur.ExpiresAt != null && ur.ExpiresAt < DateTime.UtcNow)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<UserRole>> GetExpiringAssignmentsAsync(
        DateTime expiringBefore,
        CancellationToken cancellationToken = default)
    {
        return await _context.UserRoles
            .Include(ur => ur.Role)
            .Where(ur =>
                ur.IsActive &&
                ur.ExpiresAt != null &&
                ur.ExpiresAt > DateTime.UtcNow &&
                ur.ExpiresAt <= expiringBefore)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Guid>> GetUserIdsWithPermissionAsync(
        Permission permission,
        CancellationToken cancellationToken = default)
    {
        var roles = await _context.Roles
            .Where(r => r.IsActive)
            .ToListAsync(cancellationToken);

        var roleIds = roles.Where(r => r.HasPermission(permission)).Select(r => r.Id).ToHashSet();

        return await _context.UserRoles
            .Where(ur =>
                ur.IsActive &&
                (ur.ExpiresAt == null || ur.ExpiresAt > DateTime.UtcNow) &&
                roleIds.Contains(ur.RoleId))
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task RemoveAllUserRolesAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var userRoles = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .ToListAsync(cancellationToken);

        _context.UserRoles.RemoveRange(userRoles);
    }
}
