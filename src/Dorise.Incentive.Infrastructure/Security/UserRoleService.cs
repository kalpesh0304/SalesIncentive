using Dorise.Incentive.Application.Security.DTOs;
using Dorise.Incentive.Application.Security.Services;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Security;

/// <summary>
/// Implementation of user role service for managing role assignments.
/// "Slow down, Bart! My legs don't know how to be as long as yours!" - Users running to get their roles!
/// </summary>
public class UserRoleService : IUserRoleService
{
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISecurityAuditService _auditService;
    private readonly IMemoryCache _cache;
    private readonly ILogger<UserRoleService> _logger;

    public UserRoleService(
        IUserRoleRepository userRoleRepository,
        IRoleRepository roleRepository,
        IUnitOfWork unitOfWork,
        ISecurityAuditService auditService,
        IMemoryCache cache,
        ILogger<UserRoleService> logger)
    {
        _userRoleRepository = userRoleRepository;
        _roleRepository = roleRepository;
        _unitOfWork = unitOfWork;
        _auditService = auditService;
        _cache = cache;
        _logger = logger;
    }

    public async Task<UserRoleDto> AssignRoleAsync(
        AssignRoleRequest request,
        string assignedBy,
        CancellationToken cancellationToken = default)
    {
        // Check if role exists and is active
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken)
            ?? throw new InvalidOperationException($"Role {request.RoleId} not found");

        if (!role.IsActive)
        {
            throw new InvalidOperationException($"Cannot assign inactive role '{role.Name}'");
        }

        // Check for existing assignment
        var existing = await _userRoleRepository.GetByUserAndRoleAsync(
            request.UserId, request.RoleId, cancellationToken);

        if (existing != null && existing.IsEffective)
        {
            throw new InvalidOperationException($"User already has role '{role.Name}'");
        }

        // Create new assignment or reactivate existing
        UserRole userRole;
        if (existing != null)
        {
            existing.Activate();
            if (request.ExpiresAt.HasValue)
            {
                existing.ExtendExpiration(request.ExpiresAt.Value);
            }
            else
            {
                existing.RemoveExpiration();
            }
            existing.SetScope(request.Scope);
            userRole = existing;
        }
        else
        {
            userRole = UserRole.Create(
                request.UserId,
                request.RoleId,
                assignedBy,
                request.ExpiresAt,
                request.Scope);

            await _userRoleRepository.AddAsync(userRole, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Invalidate cache
        InvalidateUserCache(request.UserId);

        await _auditService.LogSecurityEventAsync(
            SecurityEventTypes.UserRoleAssigned,
            request.UserId,
            $"Role '{role.Name}' assigned to user",
            additionalData: new Dictionary<string, object>
            {
                ["RoleId"] = request.RoleId,
                ["ExpiresAt"] = request.ExpiresAt?.ToString() ?? "Never",
                ["Scope"] = request.Scope ?? "Global"
            },
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Role {RoleName} assigned to user {UserId} by {AssignedBy}",
            role.Name, request.UserId, assignedBy);

        return await MapToUserRoleDtoAsync(userRole, cancellationToken);
    }

    public async Task<IReadOnlyList<UserRoleDto>> BulkAssignRoleAsync(
        BulkAssignRoleRequest request,
        string assignedBy,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken)
            ?? throw new InvalidOperationException($"Role {request.RoleId} not found");

        if (!role.IsActive)
        {
            throw new InvalidOperationException($"Cannot assign inactive role '{role.Name}'");
        }

        var results = new List<UserRoleDto>();

        foreach (var userId in request.UserIds)
        {
            try
            {
                var existing = await _userRoleRepository.GetByUserAndRoleAsync(
                    userId, request.RoleId, cancellationToken);

                if (existing != null && existing.IsEffective)
                {
                    _logger.LogWarning("User {UserId} already has role {RoleName}, skipping", userId, role.Name);
                    continue;
                }

                UserRole userRole;
                if (existing != null)
                {
                    existing.Activate();
                    if (request.ExpiresAt.HasValue)
                    {
                        existing.ExtendExpiration(request.ExpiresAt.Value);
                    }
                    existing.SetScope(request.Scope);
                    userRole = existing;
                }
                else
                {
                    userRole = UserRole.Create(
                        userId,
                        request.RoleId,
                        assignedBy,
                        request.ExpiresAt,
                        request.Scope);

                    await _userRoleRepository.AddAsync(userRole, cancellationToken);
                }

                InvalidateUserCache(userId);
                results.Add(await MapToUserRoleDtoAsync(userRole, cancellationToken));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to assign role to user {UserId}", userId);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogSecurityEventAsync(
            SecurityEventTypes.UserRoleAssigned,
            null,
            $"Role '{role.Name}' bulk assigned to {results.Count} users",
            additionalData: new Dictionary<string, object>
            {
                ["RoleId"] = request.RoleId,
                ["UserCount"] = results.Count
            },
            cancellationToken: cancellationToken);

        return results;
    }

    public async Task RevokeRoleAsync(
        Guid userId,
        Guid roleId,
        string revokedBy,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var userRole = await _userRoleRepository.GetByUserAndRoleAsync(userId, roleId, cancellationToken)
            ?? throw new InvalidOperationException("User role assignment not found");

        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);

        userRole.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        InvalidateUserCache(userId);

        await _auditService.LogSecurityEventAsync(
            SecurityEventTypes.UserRoleRevoked,
            userId,
            $"Role '{role?.Name}' revoked from user",
            additionalData: new Dictionary<string, object>
            {
                ["RoleId"] = roleId,
                ["Reason"] = reason ?? "Not specified"
            },
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Role {RoleId} revoked from user {UserId} by {RevokedBy}. Reason: {Reason}",
            roleId, userId, revokedBy, reason ?? "Not specified");
    }

    public async Task RevokeAllRolesAsync(
        Guid userId,
        string revokedBy,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        var userRoles = await _userRoleRepository.GetActiveByUserIdAsync(userId, cancellationToken);

        foreach (var userRole in userRoles)
        {
            userRole.Deactivate();
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        InvalidateUserCache(userId);

        await _auditService.LogSecurityEventAsync(
            SecurityEventTypes.UserRoleRevoked,
            userId,
            $"All {userRoles.Count} roles revoked from user",
            additionalData: new Dictionary<string, object>
            {
                ["RoleCount"] = userRoles.Count,
                ["Reason"] = reason ?? "Not specified"
            },
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "All {Count} roles revoked from user {UserId} by {RevokedBy}. Reason: {Reason}",
            userRoles.Count, userId, revokedBy, reason ?? "Not specified");
    }

    public async Task<UserRoleDto> ExtendRoleAssignmentAsync(
        Guid userId,
        Guid roleId,
        ExtendRoleAssignmentRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var userRole = await _userRoleRepository.GetByUserAndRoleAsync(userId, roleId, cancellationToken)
            ?? throw new InvalidOperationException("User role assignment not found");

        var oldExpiration = userRole.ExpiresAt;
        userRole.ExtendExpiration(request.NewExpiresAt);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _auditService.LogSecurityEventAsync(
            SecurityEventTypes.UserRoleExtended,
            userId,
            $"Role assignment extended",
            additionalData: new Dictionary<string, object>
            {
                ["RoleId"] = roleId,
                ["OldExpiration"] = oldExpiration?.ToString() ?? "Never",
                ["NewExpiration"] = request.NewExpiresAt.ToString()
            },
            cancellationToken: cancellationToken);

        _logger.LogInformation(
            "Role {RoleId} assignment extended for user {UserId} to {NewExpiration} by {ModifiedBy}",
            roleId, userId, request.NewExpiresAt, modifiedBy);

        return await MapToUserRoleDtoAsync(userRole, cancellationToken);
    }

    public async Task<IReadOnlyList<UserRoleDto>> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var userRoles = await _userRoleRepository.GetByUserIdAsync(userId, cancellationToken);
        var results = new List<UserRoleDto>();

        foreach (var ur in userRoles)
        {
            results.Add(await MapToUserRoleDtoAsync(ur, cancellationToken));
        }

        return results;
    }

    public async Task<IReadOnlyList<UserRoleDto>> GetRoleUsersAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var userRoles = await _userRoleRepository.GetByRoleIdAsync(roleId, cancellationToken);
        var results = new List<UserRoleDto>();

        foreach (var ur in userRoles.Where(ur => ur.IsEffective))
        {
            results.Add(await MapToUserRoleDtoAsync(ur, cancellationToken));
        }

        return results;
    }

    public async Task<IReadOnlyList<UserRoleDto>> GetExpiringAssignmentsAsync(
        int daysUntilExpiration,
        CancellationToken cancellationToken = default)
    {
        var expiringBefore = DateTime.UtcNow.AddDays(daysUntilExpiration);
        var userRoles = await _userRoleRepository.GetExpiringAssignmentsAsync(expiringBefore, cancellationToken);
        var results = new List<UserRoleDto>();

        foreach (var ur in userRoles)
        {
            results.Add(await MapToUserRoleDtoAsync(ur, cancellationToken));
        }

        return results;
    }

    public async Task<IReadOnlyList<UserRoleDto>> GetExpiredAssignmentsAsync(
        CancellationToken cancellationToken = default)
    {
        var userRoles = await _userRoleRepository.GetExpiredAssignmentsAsync(cancellationToken);
        var results = new List<UserRoleDto>();

        foreach (var ur in userRoles)
        {
            results.Add(await MapToUserRoleDtoAsync(ur, cancellationToken));
        }

        return results;
    }

    public async Task<int> CleanupExpiredAssignmentsAsync(CancellationToken cancellationToken = default)
    {
        var expired = await _userRoleRepository.GetExpiredAssignmentsAsync(cancellationToken);
        var count = 0;

        foreach (var userRole in expired.Where(ur => ur.IsActive))
        {
            userRole.Deactivate();
            InvalidateUserCache(userRole.UserId);
            count++;

            await _auditService.LogSecurityEventAsync(
                SecurityEventTypes.UserRoleExpired,
                userRole.UserId,
                $"Role assignment expired",
                additionalData: new Dictionary<string, object>
                {
                    ["RoleId"] = userRole.RoleId,
                    ["ExpiredAt"] = userRole.ExpiresAt?.ToString() ?? "Unknown"
                },
                cancellationToken: cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cleaned up {Count} expired role assignments", count);

        return count;
    }

    public async Task<IReadOnlyList<RoleAssignmentHistoryDto>> GetUserRoleHistoryAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        // This would typically query an audit log or history table
        // For now, we return the current assignments with their creation dates
        var userRoles = await _userRoleRepository.GetByUserIdAsync(userId, cancellationToken);
        var history = new List<RoleAssignmentHistoryDto>();

        foreach (var ur in userRoles)
        {
            var role = await _roleRepository.GetByIdAsync(ur.RoleId, cancellationToken);

            history.Add(new RoleAssignmentHistoryDto
            {
                Id = ur.Id,
                UserId = ur.UserId,
                UserName = "Unknown", // Would need employee repository
                RoleId = ur.RoleId,
                RoleName = role?.Name ?? "Unknown",
                Action = ur.IsActive ? "Assigned" : "Revoked",
                PerformedByUserId = ur.AssignedByUserId,
                PerformedByUserName = null,
                Reason = null,
                Timestamp = ur.CreatedAt
            });
        }

        return history.OrderByDescending(h => h.Timestamp).ToList();
    }

    public async Task<IReadOnlyList<RoleAssignmentHistoryDto>> GetRoleAssignmentHistoryAsync(
        Guid roleId,
        CancellationToken cancellationToken = default)
    {
        var role = await _roleRepository.GetByIdAsync(roleId, cancellationToken);
        var userRoles = await _userRoleRepository.GetByRoleIdAsync(roleId, cancellationToken);
        var history = new List<RoleAssignmentHistoryDto>();

        foreach (var ur in userRoles)
        {
            history.Add(new RoleAssignmentHistoryDto
            {
                Id = ur.Id,
                UserId = ur.UserId,
                UserName = "Unknown",
                RoleId = ur.RoleId,
                RoleName = role?.Name ?? "Unknown",
                Action = ur.IsActive ? "Assigned" : "Revoked",
                PerformedByUserId = ur.AssignedByUserId,
                PerformedByUserName = null,
                Reason = null,
                Timestamp = ur.CreatedAt
            });
        }

        return history.OrderByDescending(h => h.Timestamp).ToList();
    }

    // ============== Private Helper Methods ==============

    private async Task<UserRoleDto> MapToUserRoleDtoAsync(UserRole userRole, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(userRole.RoleId, cancellationToken);

        return new UserRoleDto
        {
            Id = userRole.Id,
            UserId = userRole.UserId,
            RoleId = userRole.RoleId,
            RoleName = role?.Name ?? "Unknown",
            RoleDescription = role?.Description,
            AssignedByUserId = userRole.AssignedByUserId,
            AssignedByUserName = null, // Would need to lookup
            ExpiresAt = userRole.ExpiresAt,
            IsActive = userRole.IsActive,
            IsExpired = userRole.IsExpired,
            IsEffective = userRole.IsEffective,
            Scope = userRole.Scope,
            CreatedAt = userRole.CreatedAt
        };
    }

    private void InvalidateUserCache(Guid userId)
    {
        _cache.Remove($"permissions:{userId}");
        _cache.Remove($"extended-permissions:{userId}");
    }
}
