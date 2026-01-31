using Dorise.Incentive.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using IAppAuthorizationService = Dorise.Incentive.Application.Security.Services.IAuthorizationService;

namespace Dorise.Incentive.Infrastructure.Security;

/// <summary>
/// Authorization requirement for permission-based access control.
/// "Principal Skinner made me?" - Requirements define who can do what!
/// </summary>
public class PermissionRequirement : IAuthorizationRequirement
{
    public Permission Permission { get; }

    public PermissionRequirement(Permission permission)
    {
        Permission = permission;
    }
}

/// <summary>
/// Authorization requirement for extended permission-based access control.
/// </summary>
public class ExtendedPermissionRequirement : IAuthorizationRequirement
{
    public ExtendedPermission Permission { get; }

    public ExtendedPermissionRequirement(ExtendedPermission permission)
    {
        Permission = permission;
    }
}

/// <summary>
/// Authorization requirement for multiple permissions (any).
/// </summary>
public class AnyPermissionRequirement : IAuthorizationRequirement
{
    public Permission Permissions { get; }

    public AnyPermissionRequirement(Permission permissions)
    {
        Permissions = permissions;
    }
}

/// <summary>
/// Authorization handler for permission-based requirements.
/// "I'm learnding!" - Learning who has which permissions!
/// </summary>
public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IAppAuthorizationService _authorizationService;
    private readonly ILogger<PermissionAuthorizationHandler> _logger;

    public PermissionAuthorizationHandler(
        IAppAuthorizationService authorizationService,
        ILogger<PermissionAuthorizationHandler> logger)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst("oid");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("User ID claim not found in context");
            return;
        }

        try
        {
            var hasPermission = await _authorizationService.HasPermissionAsync(userId, requirement.Permission);

            if (hasPermission)
            {
                context.Succeed(requirement);
                _logger.LogDebug(
                    "User {UserId} authorized for permission {Permission}",
                    userId, requirement.Permission);
            }
            else
            {
                _logger.LogWarning(
                    "User {UserId} denied permission {Permission}",
                    userId, requirement.Permission);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permission for user {UserId}", userId);
        }
    }
}

/// <summary>
/// Authorization handler for extended permission-based requirements.
/// </summary>
public class ExtendedPermissionAuthorizationHandler : AuthorizationHandler<ExtendedPermissionRequirement>
{
    private readonly IAppAuthorizationService _authorizationService;
    private readonly ILogger<ExtendedPermissionAuthorizationHandler> _logger;

    public ExtendedPermissionAuthorizationHandler(
        IAppAuthorizationService authorizationService,
        ILogger<ExtendedPermissionAuthorizationHandler> logger)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ExtendedPermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst("oid");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("User ID claim not found in context");
            return;
        }

        try
        {
            var hasPermission = await _authorizationService.HasExtendedPermissionAsync(userId, requirement.Permission);

            if (hasPermission)
            {
                context.Succeed(requirement);
                _logger.LogDebug(
                    "User {UserId} authorized for extended permission {Permission}",
                    userId, requirement.Permission);
            }
            else
            {
                _logger.LogWarning(
                    "User {UserId} denied extended permission {Permission}",
                    userId, requirement.Permission);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking extended permission for user {UserId}", userId);
        }
    }
}

/// <summary>
/// Authorization handler for any permission requirements.
/// </summary>
public class AnyPermissionAuthorizationHandler : AuthorizationHandler<AnyPermissionRequirement>
{
    private readonly IAppAuthorizationService _authorizationService;
    private readonly ILogger<AnyPermissionAuthorizationHandler> _logger;

    public AnyPermissionAuthorizationHandler(
        IAppAuthorizationService authorizationService,
        ILogger<AnyPermissionAuthorizationHandler> logger)
    {
        _authorizationService = authorizationService;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AnyPermissionRequirement requirement)
    {
        var userIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst("oid");

        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("User ID claim not found in context");
            return;
        }

        try
        {
            var hasAnyPermission = await _authorizationService.HasAnyPermissionAsync(userId, requirement.Permissions);

            if (hasAnyPermission)
            {
                context.Succeed(requirement);
                _logger.LogDebug(
                    "User {UserId} authorized for any of permissions {Permissions}",
                    userId, requirement.Permissions);
            }
            else
            {
                _logger.LogWarning(
                    "User {UserId} denied any of permissions {Permissions}",
                    userId, requirement.Permissions);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking permissions for user {UserId}", userId);
        }
    }
}

/// <summary>
/// Attribute to require a specific permission.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequirePermissionAttribute : AuthorizeAttribute
{
    public RequirePermissionAttribute(Permission permission)
        : base($"Permission:{permission}")
    {
    }
}

/// <summary>
/// Attribute to require an extended permission.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireExtendedPermissionAttribute : AuthorizeAttribute
{
    public RequireExtendedPermissionAttribute(ExtendedPermission permission)
        : base($"ExtendedPermission:{permission}")
    {
    }
}

/// <summary>
/// Authorization policy provider that dynamically creates permission-based policies.
/// "It tastes like burning!" - Permission checks burn through unauthorized access!
/// </summary>
public class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private const string PermissionPrefix = "Permission:";
    private const string ExtendedPermissionPrefix = "ExtendedPermission:";

    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public PermissionPolicyProvider(Microsoft.Extensions.Options.IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(PermissionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissionName = policyName[PermissionPrefix.Length..];
            if (Enum.TryParse<Permission>(permissionName, out var permission))
            {
                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new PermissionRequirement(permission))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }
        }

        if (policyName.StartsWith(ExtendedPermissionPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var permissionName = policyName[ExtendedPermissionPrefix.Length..];
            if (Enum.TryParse<ExtendedPermission>(permissionName, out var permission))
            {
                var policy = new AuthorizationPolicyBuilder()
                    .AddRequirements(new ExtendedPermissionRequirement(permission))
                    .Build();

                return Task.FromResult<AuthorizationPolicy?>(policy);
            }
        }

        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
