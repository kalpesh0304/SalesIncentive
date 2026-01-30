using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Application.Security.DTOs;

/// <summary>
/// DTOs for role management and user authorization.
/// "Go banana!" - Go secure with RBAC!
/// </summary>

// ============== Role DTOs ==============

public record RoleDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool IsSystem { get; init; }
    public bool IsActive { get; init; }
    public Permission Permissions { get; init; }
    public IReadOnlyList<ExtendedPermission> ExtendedPermissions { get; init; } = new List<ExtendedPermission>();
    public IReadOnlyList<string> PermissionNames { get; init; } = new List<string>();
    public int UserCount { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? ModifiedAt { get; init; }
    public string? ModifiedBy { get; init; }
}

public record RoleSummaryDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool IsSystem { get; init; }
    public bool IsActive { get; init; }
    public int PermissionCount { get; init; }
    public int UserCount { get; init; }
}

public record CreateRoleRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public Permission Permissions { get; init; } = Permission.None;
    public IReadOnlyList<ExtendedPermission> ExtendedPermissions { get; init; } = new List<ExtendedPermission>();
}

public record UpdateRoleRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
}

public record UpdateRolePermissionsRequest
{
    public Permission Permissions { get; init; }
    public IReadOnlyList<ExtendedPermission> ExtendedPermissions { get; init; } = new List<ExtendedPermission>();
}

// ============== User Role DTOs ==============

public record UserRoleDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public Guid RoleId { get; init; }
    public required string RoleName { get; init; }
    public string? RoleDescription { get; init; }
    public string? AssignedByUserId { get; init; }
    public string? AssignedByUserName { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public bool IsActive { get; init; }
    public bool IsExpired { get; init; }
    public bool IsEffective { get; init; }
    public string? Scope { get; init; }
    public DateTime CreatedAt { get; init; }
}

public record AssignRoleRequest
{
    public Guid UserId { get; init; }
    public Guid RoleId { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public string? Scope { get; init; }
}

public record BulkAssignRoleRequest
{
    public IReadOnlyList<Guid> UserIds { get; init; } = new List<Guid>();
    public Guid RoleId { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public string? Scope { get; init; }
}

public record ExtendRoleAssignmentRequest
{
    public DateTime NewExpiresAt { get; init; }
}

// ============== User Permissions DTOs ==============

public record UserPermissionsDto
{
    public Guid UserId { get; init; }
    public required string UserName { get; init; }
    public string? Email { get; init; }
    public IReadOnlyList<RoleSummaryDto> Roles { get; init; } = new List<RoleSummaryDto>();
    public Permission EffectivePermissions { get; init; }
    public IReadOnlyList<ExtendedPermission> EffectiveExtendedPermissions { get; init; } = new List<ExtendedPermission>();
    public IReadOnlyList<string> PermissionNames { get; init; } = new List<string>();
    public bool IsAdmin { get; init; }
}

public record PermissionCheckRequest
{
    public Guid UserId { get; init; }
    public Permission Permission { get; init; }
    public string? Scope { get; init; }
}

public record PermissionCheckResult
{
    public bool IsAllowed { get; init; }
    public string? DeniedReason { get; init; }
    public IReadOnlyList<string> GrantingRoles { get; init; } = new List<string>();
}

public record ExtendedPermissionCheckRequest
{
    public Guid UserId { get; init; }
    public ExtendedPermission Permission { get; init; }
    public string? Scope { get; init; }
}

// ============== Permission Info DTOs ==============

public record PermissionInfoDto
{
    public required string Name { get; init; }
    public required string Category { get; init; }
    public string? Description { get; init; }
    public Permission Value { get; init; }
}

public record ExtendedPermissionInfoDto
{
    public required string Name { get; init; }
    public required string Category { get; init; }
    public string? Description { get; init; }
    public ExtendedPermission Value { get; init; }
}

public record PermissionCategoryDto
{
    public required string Name { get; init; }
    public IReadOnlyList<PermissionInfoDto> Permissions { get; init; } = new List<PermissionInfoDto>();
    public IReadOnlyList<ExtendedPermissionInfoDto> ExtendedPermissions { get; init; } = new List<ExtendedPermissionInfoDto>();
}

// ============== Security Context DTOs ==============

public record SecurityContextDto
{
    public Guid UserId { get; init; }
    public required string UserName { get; init; }
    public string? Email { get; init; }
    public IReadOnlyList<string> Roles { get; init; } = new List<string>();
    public Permission Permissions { get; init; }
    public IReadOnlyList<ExtendedPermission> ExtendedPermissions { get; init; } = new List<ExtendedPermission>();
    public IReadOnlyDictionary<string, string?> Scopes { get; init; } = new Dictionary<string, string?>();
    public bool IsAuthenticated { get; init; }
    public bool IsAdmin { get; init; }
    public DateTime? SessionExpiresAt { get; init; }
}

// ============== Security Audit DTOs ==============

public record SecurityEventDto
{
    public Guid Id { get; init; }
    public required string EventType { get; init; }
    public Guid? UserId { get; init; }
    public string? UserName { get; init; }
    public string? TargetUserId { get; init; }
    public string? TargetRoleId { get; init; }
    public required string Description { get; init; }
    public string? IpAddress { get; init; }
    public string? UserAgent { get; init; }
    public bool IsSuccess { get; init; }
    public string? FailureReason { get; init; }
    public DateTime Timestamp { get; init; }
    public IReadOnlyDictionary<string, object>? AdditionalData { get; init; }
}

public record SecurityEventSearchQuery
{
    public string? EventType { get; init; }
    public Guid? UserId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public bool? IsSuccess { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

// ============== Role Assignment History DTOs ==============

public record RoleAssignmentHistoryDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public required string UserName { get; init; }
    public Guid RoleId { get; init; }
    public required string RoleName { get; init; }
    public required string Action { get; init; } // Assigned, Revoked, Expired, Modified
    public string? PerformedByUserId { get; init; }
    public string? PerformedByUserName { get; init; }
    public string? Reason { get; init; }
    public DateTime Timestamp { get; init; }
}

// ============== Role Comparison DTOs ==============

public record RoleComparisonDto
{
    public required RoleSummaryDto Role1 { get; init; }
    public required RoleSummaryDto Role2 { get; init; }
    public IReadOnlyList<string> CommonPermissions { get; init; } = new List<string>();
    public IReadOnlyList<string> OnlyInRole1 { get; init; } = new List<string>();
    public IReadOnlyList<string> OnlyInRole2 { get; init; } = new List<string>();
}

// ============== Security Report DTOs ==============

public record SecuritySummaryDto
{
    public int TotalRoles { get; init; }
    public int ActiveRoles { get; init; }
    public int SystemRoles { get; init; }
    public int TotalUserRoleAssignments { get; init; }
    public int ActiveAssignments { get; init; }
    public int ExpiringAssignments { get; init; }
    public int ExpiredAssignments { get; init; }
    public IReadOnlyList<RoleSummaryDto> TopRolesByUsers { get; init; } = new List<RoleSummaryDto>();
    public IReadOnlyList<SecurityEventDto> RecentSecurityEvents { get; init; } = new List<SecurityEventDto>();
}

public record UserAccessMatrixDto
{
    public Guid UserId { get; init; }
    public required string UserName { get; init; }
    public IReadOnlyDictionary<string, IReadOnlyList<string>> AccessByCategory { get; init; } =
        new Dictionary<string, IReadOnlyList<string>>();
}

public record RoleAccessMatrixDto
{
    public Guid RoleId { get; init; }
    public required string RoleName { get; init; }
    public IReadOnlyDictionary<string, IReadOnlyList<string>> AccessByCategory { get; init; } =
        new Dictionary<string, IReadOnlyList<string>>();
}

// ============== Security Event Types ==============

public static class SecurityEventTypes
{
    public const string RoleCreated = "RoleCreated";
    public const string RoleModified = "RoleModified";
    public const string RoleDeleted = "RoleDeleted";
    public const string RoleActivated = "RoleActivated";
    public const string RoleDeactivated = "RoleDeactivated";
    public const string PermissionGranted = "PermissionGranted";
    public const string PermissionRevoked = "PermissionRevoked";
    public const string UserRoleAssigned = "UserRoleAssigned";
    public const string UserRoleRevoked = "UserRoleRevoked";
    public const string UserRoleExpired = "UserRoleExpired";
    public const string UserRoleExtended = "UserRoleExtended";
    public const string AccessDenied = "AccessDenied";
    public const string AccessGranted = "AccessGranted";
    public const string LoginSuccess = "LoginSuccess";
    public const string LoginFailed = "LoginFailed";
    public const string Logout = "Logout";
    public const string PasswordChanged = "PasswordChanged";
    public const string SessionExpired = "SessionExpired";
}
