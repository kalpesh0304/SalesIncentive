using Dorise.Incentive.Application.Audit.Services;
using Dorise.Incentive.Application.Security.DTOs;
using Dorise.Incentive.Application.Security.Services;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Security;

/// <summary>
/// Implementation of security audit service.
/// "I picked it, and I licked it, and I stuck it in a drawer!" - Security events stored safely!
/// </summary>
public class SecurityAuditService : ISecurityAuditService
{
    private readonly IAuditService _auditService;
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRoleRepository _userRoleRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SecurityAuditService> _logger;

    public SecurityAuditService(
        IAuditService auditService,
        IRoleRepository roleRepository,
        IUserRoleRepository userRoleRepository,
        IHttpContextAccessor httpContextAccessor,
        ILogger<SecurityAuditService> logger)
    {
        _auditService = auditService;
        _roleRepository = roleRepository;
        _userRoleRepository = userRoleRepository;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public async Task LogSecurityEventAsync(
        string eventType,
        Guid? userId,
        string description,
        bool isSuccess = true,
        string? failureReason = null,
        IDictionary<string, object>? additionalData = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var context = _httpContextAccessor.HttpContext;
            var ipAddress = context?.Connection.RemoteIpAddress?.ToString();
            var userAgent = context?.Request.Headers.UserAgent.ToString();
            var currentUser = context?.User?.Identity?.Name;

            var oldValues = new Dictionary<string, object?>
            {
                ["EventType"] = eventType,
                ["IsSuccess"] = isSuccess,
                ["FailureReason"] = failureReason,
                ["IpAddress"] = ipAddress,
                ["UserAgent"] = userAgent,
                ["TargetUserId"] = userId?.ToString()
            };

            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    oldValues[kvp.Key] = kvp.Value;
                }
            }

            await _auditService.LogActivityAsync(
                entityType: "SecurityEvent",
                entityId: Guid.NewGuid().ToString(),
                action: eventType,
                description: description,
                oldValues: null,
                newValues: oldValues,
                cancellationToken: cancellationToken);

            if (!isSuccess)
            {
                _logger.LogWarning(
                    "Security event {EventType} failed for user {UserId}: {FailureReason}",
                    eventType, userId, failureReason);
            }
            else
            {
                _logger.LogInformation(
                    "Security event {EventType} logged for user {UserId}: {Description}",
                    eventType, userId, description);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to log security event {EventType}", eventType);
        }
    }

    public async Task<IReadOnlyList<SecurityEventDto>> GetSecurityEventsAsync(
        SecurityEventSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var auditLogs = await _auditService.GetAuditLogsAsync(
            new Application.Audit.DTOs.AuditLogSearchQuery
            {
                EntityType = "SecurityEvent",
                Action = query.EventType,
                UserId = query.UserId?.ToString(),
                FromDate = query.FromDate,
                ToDate = query.ToDate,
                Page = query.Page,
                PageSize = query.PageSize
            },
            cancellationToken);

        return auditLogs.Items.Select(log => new SecurityEventDto
        {
            Id = log.Id,
            EventType = log.Action,
            UserId = Guid.TryParse(log.NewValues?.GetValueOrDefault("TargetUserId")?.ToString(), out var uid) ? uid : null,
            UserName = log.UserName,
            TargetUserId = log.NewValues?.GetValueOrDefault("TargetUserId")?.ToString(),
            TargetRoleId = log.NewValues?.GetValueOrDefault("RoleId")?.ToString(),
            Description = log.Description ?? log.Action,
            IpAddress = log.NewValues?.GetValueOrDefault("IpAddress")?.ToString(),
            UserAgent = log.NewValues?.GetValueOrDefault("UserAgent")?.ToString(),
            IsSuccess = (bool?)log.NewValues?.GetValueOrDefault("IsSuccess") ?? true,
            FailureReason = log.NewValues?.GetValueOrDefault("FailureReason")?.ToString(),
            Timestamp = log.Timestamp,
            AdditionalData = log.NewValues?.ToDictionary(
                kvp => kvp.Key,
                kvp => (object)kvp.Value!)
        }).ToList();
    }

    public async Task<SecuritySummaryDto> GetSecuritySummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var allRoles = await _roleRepository.GetAllAsync(cancellationToken);
        var activeRoles = allRoles.Where(r => r.IsActive).ToList();
        var systemRoles = allRoles.Where(r => r.IsSystem).ToList();

        var allUserRoles = new List<UserRole>();
        foreach (var role in allRoles)
        {
            var userRoles = await _userRoleRepository.GetByRoleIdAsync(role.Id, cancellationToken);
            allUserRoles.AddRange(userRoles);
        }

        var expiringAssignments = await _userRoleRepository.GetExpiringAssignmentsAsync(
            DateTime.UtcNow.AddDays(7), cancellationToken);

        var expiredAssignments = await _userRoleRepository.GetExpiredAssignmentsAsync(cancellationToken);

        var recentEvents = await GetRecentSecurityEventsAsync(10, cancellationToken);

        // Calculate top roles by user count
        var roleUserCounts = new Dictionary<Guid, int>();
        foreach (var userRole in allUserRoles.Where(ur => ur.IsEffective))
        {
            if (!roleUserCounts.ContainsKey(userRole.RoleId))
            {
                roleUserCounts[userRole.RoleId] = 0;
            }
            roleUserCounts[userRole.RoleId]++;
        }

        var topRoles = roleUserCounts
            .OrderByDescending(kvp => kvp.Value)
            .Take(5)
            .Select(kvp =>
            {
                var role = allRoles.First(r => r.Id == kvp.Key);
                return new RoleSummaryDto
                {
                    Id = role.Id,
                    Name = role.Name,
                    Description = role.Description,
                    IsSystem = role.IsSystem,
                    IsActive = role.IsActive,
                    PermissionCount = role.GetGrantedPermissions().Count,
                    UserCount = kvp.Value
                };
            })
            .ToList();

        return new SecuritySummaryDto
        {
            TotalRoles = allRoles.Count,
            ActiveRoles = activeRoles.Count,
            SystemRoles = systemRoles.Count,
            TotalUserRoleAssignments = allUserRoles.Count,
            ActiveAssignments = allUserRoles.Count(ur => ur.IsEffective),
            ExpiringAssignments = expiringAssignments.Count,
            ExpiredAssignments = expiredAssignments.Count,
            TopRolesByUsers = topRoles,
            RecentSecurityEvents = recentEvents
        };
    }

    public async Task<IReadOnlyList<SecurityEventDto>> GetRecentSecurityEventsAsync(
        int count = 20,
        CancellationToken cancellationToken = default)
    {
        return await GetSecurityEventsAsync(
            new SecurityEventSearchQuery
            {
                Page = 1,
                PageSize = count
            },
            cancellationToken);
    }
}
