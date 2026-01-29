using System.Security.Claims;
using Dorise.Incentive.Application.Security.DTOs;
using Dorise.Incentive.Application.Security.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Security;

/// <summary>
/// Middleware for enriching the security context and validating requests.
/// "Me fail English? That's unpossible!" - Security failures are unpossible with proper middleware!
/// </summary>
public class SecurityContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityContextMiddleware> _logger;

    public SecurityContextMiddleware(
        RequestDelegate next,
        ILogger<SecurityContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuthorizationService authorizationService)
    {
        // Skip for unauthenticated requests
        if (context.User.Identity?.IsAuthenticated != true)
        {
            await _next(context);
            return;
        }

        var userId = GetUserId(context.User);
        if (userId == null)
        {
            _logger.LogWarning("Authenticated user without valid user ID claim");
            await _next(context);
            return;
        }

        try
        {
            // Load security context
            var securityContext = await authorizationService.GetSecurityContextAsync(userId.Value);

            // Store in HttpContext for later use
            context.Items["SecurityContext"] = securityContext;

            // Add permissions as claims for compatibility with standard [Authorize] attributes
            if (context.User.Identity is ClaimsIdentity identity)
            {
                foreach (var role in securityContext.Roles)
                {
                    if (!identity.HasClaim(ClaimTypes.Role, role))
                    {
                        identity.AddClaim(new Claim(ClaimTypes.Role, role));
                    }
                }
            }

            _logger.LogDebug(
                "Security context loaded for user {UserId} with {RoleCount} roles",
                userId, securityContext.Roles.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load security context for user {UserId}", userId);
        }

        await _next(context);
    }

    private static Guid? GetUserId(ClaimsPrincipal user)
    {
        var claim = user.FindFirst("sub") ?? user.FindFirst("oid") ?? user.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && Guid.TryParse(claim.Value, out var userId) ? userId : null;
    }
}

/// <summary>
/// Middleware for rate limiting security-sensitive operations.
/// "The doctor told me that my nose would stop bleeding if I just kept my finger out of there." -
/// Rate limiting keeps troublemakers out!
/// </summary>
public class SecurityRateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityRateLimitMiddleware> _logger;
    private readonly Dictionary<string, RateLimitEntry> _rateLimits = new();
    private readonly object _lock = new();

    private const int MaxFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan WindowDuration = TimeSpan.FromMinutes(5);

    public SecurityRateLimitMiddleware(
        RequestDelegate next,
        ILogger<SecurityRateLimitMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ISecurityAuditService auditService)
    {
        var clientKey = GetClientKey(context);

        // Check if client is currently locked out
        if (IsLockedOut(clientKey))
        {
            _logger.LogWarning("Request blocked due to rate limiting: {ClientKey}", clientKey);

            await auditService.LogSecurityEventAsync(
                SecurityEventTypes.AccessDenied,
                null,
                "Request blocked due to rate limiting",
                isSuccess: false,
                failureReason: "Too many failed attempts",
                additionalData: new Dictionary<string, object>
                {
                    ["ClientKey"] = clientKey,
                    ["Path"] = context.Request.Path.Value ?? ""
                });

            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.RetryAfter = LockoutDuration.TotalSeconds.ToString();
            await context.Response.WriteAsJsonAsync(new
            {
                Error = "Too many requests. Please try again later.",
                RetryAfter = LockoutDuration.TotalSeconds
            });
            return;
        }

        await _next(context);

        // Track failed authentication/authorization attempts
        if (context.Response.StatusCode is 401 or 403)
        {
            RecordFailedAttempt(clientKey);
        }
        else if (context.Response.StatusCode >= 200 && context.Response.StatusCode < 300)
        {
            // Reset on successful request
            ResetAttempts(clientKey);
        }
    }

    private string GetClientKey(HttpContext context)
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var userId = context.User.FindFirst("sub")?.Value ?? "anonymous";
        return $"{ip}:{userId}";
    }

    private bool IsLockedOut(string clientKey)
    {
        lock (_lock)
        {
            if (_rateLimits.TryGetValue(clientKey, out var entry))
            {
                if (entry.LockedUntil.HasValue && entry.LockedUntil > DateTime.UtcNow)
                {
                    return true;
                }

                // Clean up expired entries
                if (entry.WindowStart.Add(WindowDuration) < DateTime.UtcNow)
                {
                    _rateLimits.Remove(clientKey);
                }
            }
        }
        return false;
    }

    private void RecordFailedAttempt(string clientKey)
    {
        lock (_lock)
        {
            if (!_rateLimits.TryGetValue(clientKey, out var entry))
            {
                entry = new RateLimitEntry { WindowStart = DateTime.UtcNow };
                _rateLimits[clientKey] = entry;
            }

            // Reset window if expired
            if (entry.WindowStart.Add(WindowDuration) < DateTime.UtcNow)
            {
                entry.WindowStart = DateTime.UtcNow;
                entry.FailedAttempts = 0;
                entry.LockedUntil = null;
            }

            entry.FailedAttempts++;

            if (entry.FailedAttempts >= MaxFailedAttempts)
            {
                entry.LockedUntil = DateTime.UtcNow.Add(LockoutDuration);
                _logger.LogWarning(
                    "Client {ClientKey} locked out until {LockedUntil} after {Attempts} failed attempts",
                    clientKey, entry.LockedUntil, entry.FailedAttempts);
            }
        }
    }

    private void ResetAttempts(string clientKey)
    {
        lock (_lock)
        {
            _rateLimits.Remove(clientKey);
        }
    }

    private class RateLimitEntry
    {
        public DateTime WindowStart { get; set; }
        public int FailedAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
    }
}

/// <summary>
/// Middleware for logging security-related requests.
/// </summary>
public class SecurityLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SecurityLoggingMiddleware> _logger;

    // Paths that should have enhanced security logging
    private static readonly string[] SecuritySensitivePaths =
    {
        "/api/v1/security",
        "/api/v1/roles",
        "/api/v1/auth",
        "/api/v1/users"
    };

    public SecurityLoggingMiddleware(
        RequestDelegate next,
        ILogger<SecurityLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, ISecurityAuditService auditService)
    {
        var path = context.Request.Path.Value ?? "";
        var isSecuritySensitive = SecuritySensitivePaths.Any(p =>
            path.StartsWith(p, StringComparison.OrdinalIgnoreCase));

        if (!isSecuritySensitive)
        {
            await _next(context);
            return;
        }

        var startTime = DateTime.UtcNow;
        var userId = context.User.FindFirst("sub")?.Value;

        try
        {
            await _next(context);

            var duration = DateTime.UtcNow - startTime;

            if (context.Response.StatusCode >= 400)
            {
                await auditService.LogSecurityEventAsync(
                    context.Response.StatusCode == 401 ? SecurityEventTypes.AccessDenied : SecurityEventTypes.AccessDenied,
                    userId != null && Guid.TryParse(userId, out var uid) ? uid : null,
                    $"Security-sensitive request failed: {context.Request.Method} {path}",
                    isSuccess: false,
                    failureReason: $"HTTP {context.Response.StatusCode}",
                    additionalData: new Dictionary<string, object>
                    {
                        ["Method"] = context.Request.Method,
                        ["Path"] = path,
                        ["StatusCode"] = context.Response.StatusCode,
                        ["DurationMs"] = duration.TotalMilliseconds
                    });
            }
            else
            {
                _logger.LogInformation(
                    "Security-sensitive request: {Method} {Path} -> {StatusCode} ({Duration}ms)",
                    context.Request.Method, path, context.Response.StatusCode, duration.TotalMilliseconds);
            }
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;

            await auditService.LogSecurityEventAsync(
                SecurityEventTypes.AccessDenied,
                userId != null && Guid.TryParse(userId, out var uid) ? uid : null,
                $"Security-sensitive request threw exception: {context.Request.Method} {path}",
                isSuccess: false,
                failureReason: ex.Message,
                additionalData: new Dictionary<string, object>
                {
                    ["Method"] = context.Request.Method,
                    ["Path"] = path,
                    ["ExceptionType"] = ex.GetType().Name,
                    ["DurationMs"] = duration.TotalMilliseconds
                });

            throw;
        }
    }
}

/// <summary>
/// Extension methods for HttpContext security features.
/// </summary>
public static class SecurityContextExtensions
{
    /// <summary>
    /// Gets the current security context from HttpContext.
    /// </summary>
    public static SecurityContextDto? GetSecurityContext(this HttpContext context)
    {
        return context.Items.TryGetValue("SecurityContext", out var securityContext)
            ? securityContext as SecurityContextDto
            : null;
    }

    /// <summary>
    /// Checks if the current user has a specific permission.
    /// </summary>
    public static bool HasPermission(this HttpContext context, Domain.Enums.Permission permission)
    {
        var securityContext = context.GetSecurityContext();
        if (securityContext == null) return false;

        return (securityContext.Permissions & permission) == permission;
    }

    /// <summary>
    /// Checks if the current user has an extended permission.
    /// </summary>
    public static bool HasExtendedPermission(this HttpContext context, Domain.Enums.ExtendedPermission permission)
    {
        var securityContext = context.GetSecurityContext();
        return securityContext?.ExtendedPermissions.Contains(permission) ?? false;
    }

    /// <summary>
    /// Checks if the current user is an administrator.
    /// </summary>
    public static bool IsAdmin(this HttpContext context)
    {
        var securityContext = context.GetSecurityContext();
        return securityContext?.IsAdmin ?? false;
    }
}
