using Asp.Versioning;
using Dorise.Incentive.Application.Security.DTOs;
using Dorise.Incentive.Application.Security.Services;
using Dorise.Incentive.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IAppAuthorizationService = Dorise.Incentive.Application.Security.Services.IAuthorizationService;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for security management - roles, permissions, and user assignments.
/// "I'm a brick!" - Building blocks of security with RBAC!
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json")]
public class SecurityController : ControllerBase
{
    private readonly IAppAuthorizationService _authorizationService;
    private readonly IRoleManagementService _roleManagementService;
    private readonly IUserRoleService _userRoleService;
    private readonly ISecurityAuditService _auditService;
    private readonly ILogger<SecurityController> _logger;

    public SecurityController(
        IAppAuthorizationService authorizationService,
        IRoleManagementService roleManagementService,
        IUserRoleService userRoleService,
        ISecurityAuditService auditService,
        ILogger<SecurityController> logger)
    {
        _authorizationService = authorizationService;
        _roleManagementService = roleManagementService;
        _userRoleService = userRoleService;
        _auditService = auditService;
        _logger = logger;
    }

    // ============== Role Management ==============

    /// <summary>
    /// Get all roles.
    /// </summary>
    [HttpGet("roles")]
    [ProducesResponseType(typeof(IReadOnlyList<RoleSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRoles(CancellationToken cancellationToken)
    {
        var roles = await _roleManagementService.GetAllRolesAsync(cancellationToken);
        return Ok(roles);
    }

    /// <summary>
    /// Get active roles.
    /// </summary>
    [HttpGet("roles/active")]
    [ProducesResponseType(typeof(IReadOnlyList<RoleSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetActiveRoles(CancellationToken cancellationToken)
    {
        var roles = await _roleManagementService.GetActiveRolesAsync(cancellationToken);
        return Ok(roles);
    }

    /// <summary>
    /// Get role by ID.
    /// </summary>
    [HttpGet("roles/{roleId:guid}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleById(Guid roleId, CancellationToken cancellationToken)
    {
        var role = await _roleManagementService.GetRoleByIdAsync(roleId, cancellationToken);
        return role == null ? NotFound() : Ok(role);
    }

    /// <summary>
    /// Get role by name.
    /// </summary>
    [HttpGet("roles/by-name/{name}")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleByName(string name, CancellationToken cancellationToken)
    {
        var role = await _roleManagementService.GetRoleByNameAsync(name, cancellationToken);
        return role == null ? NotFound() : Ok(role);
    }

    /// <summary>
    /// Create a new role.
    /// </summary>
    [HttpPost("roles")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRole(
        [FromBody] CreateRoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var createdBy = User.Identity?.Name ?? "System";
            var role = await _roleManagementService.CreateRoleAsync(request, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetRoleById), new { roleId = role.Id }, role);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Update role details.
    /// </summary>
    [HttpPut("roles/{roleId:guid}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRole(
        Guid roleId,
        [FromBody] UpdateRoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var modifiedBy = User.Identity?.Name ?? "System";
            var role = await _roleManagementService.UpdateRoleAsync(roleId, request, modifiedBy, cancellationToken);
            return Ok(role);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Update role permissions.
    /// </summary>
    [HttpPut("roles/{roleId:guid}/permissions")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(RoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRolePermissions(
        Guid roleId,
        [FromBody] UpdateRolePermissionsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var modifiedBy = User.Identity?.Name ?? "System";
            var role = await _roleManagementService.UpdateRolePermissionsAsync(roleId, request, modifiedBy, cancellationToken);
            return Ok(role);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Activate a role.
    /// </summary>
    [HttpPost("roles/{roleId:guid}/activate")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ActivateRole(Guid roleId, CancellationToken cancellationToken)
    {
        try
        {
            var modifiedBy = User.Identity?.Name ?? "System";
            await _roleManagementService.ActivateRoleAsync(roleId, modifiedBy, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Deactivate a role.
    /// </summary>
    [HttpPost("roles/{roleId:guid}/deactivate")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeactivateRole(Guid roleId, CancellationToken cancellationToken)
    {
        try
        {
            var modifiedBy = User.Identity?.Name ?? "System";
            await _roleManagementService.DeactivateRoleAsync(roleId, modifiedBy, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a role.
    /// </summary>
    [HttpDelete("roles/{roleId:guid}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRole(Guid roleId, CancellationToken cancellationToken)
    {
        try
        {
            var deletedBy = User.Identity?.Name ?? "System";
            await _roleManagementService.DeleteRoleAsync(roleId, deletedBy, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    // ============== Permission Information ==============

    /// <summary>
    /// Get all available permissions.
    /// </summary>
    [HttpGet("permissions")]
    [ProducesResponseType(typeof(IReadOnlyList<PermissionCategoryDto>), StatusCodes.Status200OK)]
    public IActionResult GetAvailablePermissions()
    {
        var permissions = _roleManagementService.GetAvailablePermissions();
        return Ok(permissions);
    }

    /// <summary>
    /// Get role access matrix.
    /// </summary>
    [HttpGet("roles/{roleId:guid}/access-matrix")]
    [ProducesResponseType(typeof(RoleAccessMatrixDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRoleAccessMatrix(Guid roleId, CancellationToken cancellationToken)
    {
        try
        {
            var matrix = await _roleManagementService.GetRoleAccessMatrixAsync(roleId, cancellationToken);
            return Ok(matrix);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Compare two roles.
    /// </summary>
    [HttpGet("roles/compare")]
    [ProducesResponseType(typeof(RoleComparisonDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CompareRoles(
        [FromQuery] Guid roleId1,
        [FromQuery] Guid roleId2,
        CancellationToken cancellationToken)
    {
        try
        {
            var comparison = await _roleManagementService.CompareRolesAsync(roleId1, roleId2, cancellationToken);
            return Ok(comparison);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    // ============== User Role Assignments ==============

    /// <summary>
    /// Assign a role to a user.
    /// </summary>
    [HttpPost("users/roles")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(UserRoleDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AssignRole(
        [FromBody] AssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var assignedBy = User.Identity?.Name ?? "System";
            var userRole = await _userRoleService.AssignRoleAsync(request, assignedBy, cancellationToken);
            return CreatedAtAction(nameof(GetUserRoles), new { userId = request.UserId }, userRole);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Bulk assign a role to multiple users.
    /// </summary>
    [HttpPost("users/roles/bulk")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(IReadOnlyList<UserRoleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BulkAssignRole(
        [FromBody] BulkAssignRoleRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var assignedBy = User.Identity?.Name ?? "System";
            var userRoles = await _userRoleService.BulkAssignRoleAsync(request, assignedBy, cancellationToken);
            return Ok(userRoles);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Revoke a role from a user.
    /// </summary>
    [HttpDelete("users/{userId:guid}/roles/{roleId:guid}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RevokeRole(
        Guid userId,
        Guid roleId,
        [FromQuery] string? reason,
        CancellationToken cancellationToken)
    {
        try
        {
            var revokedBy = User.Identity?.Name ?? "System";
            await _userRoleService.RevokeRoleAsync(userId, roleId, revokedBy, reason, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Revoke all roles from a user.
    /// </summary>
    [HttpDelete("users/{userId:guid}/roles")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RevokeAllRoles(
        Guid userId,
        [FromQuery] string? reason,
        CancellationToken cancellationToken)
    {
        var revokedBy = User.Identity?.Name ?? "System";
        await _userRoleService.RevokeAllRolesAsync(userId, revokedBy, reason, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Extend a role assignment.
    /// </summary>
    [HttpPost("users/{userId:guid}/roles/{roleId:guid}/extend")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(UserRoleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExtendRoleAssignment(
        Guid userId,
        Guid roleId,
        [FromBody] ExtendRoleAssignmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var modifiedBy = User.Identity?.Name ?? "System";
            var userRole = await _userRoleService.ExtendRoleAssignmentAsync(userId, roleId, request, modifiedBy, cancellationToken);
            return Ok(userRole);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Get user roles.
    /// </summary>
    [HttpGet("users/{userId:guid}/roles")]
    [ProducesResponseType(typeof(IReadOnlyList<UserRoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserRoles(Guid userId, CancellationToken cancellationToken)
    {
        var roles = await _userRoleService.GetUserRolesAsync(userId, cancellationToken);
        return Ok(roles);
    }

    /// <summary>
    /// Get users with a specific role.
    /// </summary>
    [HttpGet("roles/{roleId:guid}/users")]
    [ProducesResponseType(typeof(IReadOnlyList<UserRoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoleUsers(Guid roleId, CancellationToken cancellationToken)
    {
        var users = await _userRoleService.GetRoleUsersAsync(roleId, cancellationToken);
        return Ok(users);
    }

    /// <summary>
    /// Get expiring role assignments.
    /// </summary>
    [HttpGet("users/roles/expiring")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(IReadOnlyList<UserRoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiringAssignments(
        [FromQuery] int daysUntilExpiration = 7,
        CancellationToken cancellationToken = default)
    {
        var expiring = await _userRoleService.GetExpiringAssignmentsAsync(daysUntilExpiration, cancellationToken);
        return Ok(expiring);
    }

    /// <summary>
    /// Get expired role assignments.
    /// </summary>
    [HttpGet("users/roles/expired")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(IReadOnlyList<UserRoleDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExpiredAssignments(CancellationToken cancellationToken)
    {
        var expired = await _userRoleService.GetExpiredAssignmentsAsync(cancellationToken);
        return Ok(expired);
    }

    /// <summary>
    /// Cleanup expired role assignments.
    /// </summary>
    [HttpPost("users/roles/cleanup")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> CleanupExpiredAssignments(CancellationToken cancellationToken)
    {
        var count = await _userRoleService.CleanupExpiredAssignmentsAsync(cancellationToken);
        return Ok(new { CleanedUp = count });
    }

    // ============== User Permissions ==============

    /// <summary>
    /// Get current user's permissions.
    /// </summary>
    [HttpGet("me/permissions")]
    [ProducesResponseType(typeof(UserPermissionsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyPermissions(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var permissions = await _authorizationService.GetUserPermissionsAsync(userId.Value, cancellationToken);
        return Ok(permissions);
    }

    /// <summary>
    /// Get current user's security context.
    /// </summary>
    [HttpGet("me/context")]
    [ProducesResponseType(typeof(SecurityContextDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMySecurityContext(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var context = await _authorizationService.GetSecurityContextAsync(userId.Value, cancellationToken);
        return Ok(context);
    }

    /// <summary>
    /// Check if current user has a permission.
    /// </summary>
    [HttpGet("me/check-permission")]
    [ProducesResponseType(typeof(PermissionCheckResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckMyPermission(
        [FromQuery] Permission permission,
        [FromQuery] string? scope = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var result = await _authorizationService.CheckPermissionAsync(
            new PermissionCheckRequest
            {
                UserId = userId.Value,
                Permission = permission,
                Scope = scope
            },
            cancellationToken);

        return Ok(result);
    }

    /// <summary>
    /// Get user permissions.
    /// </summary>
    [HttpGet("users/{userId:guid}/permissions")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(UserPermissionsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserPermissions(Guid userId, CancellationToken cancellationToken)
    {
        var permissions = await _authorizationService.GetUserPermissionsAsync(userId, cancellationToken);
        return Ok(permissions);
    }

    /// <summary>
    /// Get user access matrix.
    /// </summary>
    [HttpGet("users/{userId:guid}/access-matrix")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(UserAccessMatrixDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserAccessMatrix(Guid userId, CancellationToken cancellationToken)
    {
        var matrix = await _authorizationService.GetUserAccessMatrixAsync(userId, cancellationToken);
        return Ok(matrix);
    }

    // ============== Security Audit ==============

    /// <summary>
    /// Get security summary.
    /// </summary>
    [HttpGet("summary")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(SecuritySummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSecuritySummary(CancellationToken cancellationToken)
    {
        var summary = await _auditService.GetSecuritySummaryAsync(cancellationToken);
        return Ok(summary);
    }

    /// <summary>
    /// Get security events.
    /// </summary>
    [HttpGet("events")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(IReadOnlyList<SecurityEventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSecurityEvents(
        [FromQuery] SecurityEventSearchQuery query,
        CancellationToken cancellationToken)
    {
        var events = await _auditService.GetSecurityEventsAsync(query, cancellationToken);
        return Ok(events);
    }

    /// <summary>
    /// Get recent security events.
    /// </summary>
    [HttpGet("events/recent")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(IReadOnlyList<SecurityEventDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRecentSecurityEvents(
        [FromQuery] int count = 20,
        CancellationToken cancellationToken = default)
    {
        var events = await _auditService.GetRecentSecurityEventsAsync(count, cancellationToken);
        return Ok(events);
    }

    /// <summary>
    /// Get user role history.
    /// </summary>
    [HttpGet("users/{userId:guid}/history")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(IReadOnlyList<RoleAssignmentHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserRoleHistory(Guid userId, CancellationToken cancellationToken)
    {
        var history = await _userRoleService.GetUserRoleHistoryAsync(userId, cancellationToken);
        return Ok(history);
    }

    /// <summary>
    /// Get role assignment history.
    /// </summary>
    [HttpGet("roles/{roleId:guid}/history")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(IReadOnlyList<RoleAssignmentHistoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRoleAssignmentHistory(Guid roleId, CancellationToken cancellationToken)
    {
        var history = await _userRoleService.GetRoleAssignmentHistoryAsync(roleId, cancellationToken);
        return Ok(history);
    }

    // ============== Helper Methods ==============

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst("sub") ?? User.FindFirst("oid");
        return claim != null && Guid.TryParse(claim.Value, out var userId) ? userId : null;
    }
}
