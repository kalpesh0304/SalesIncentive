using System.Text.Json;
using Dorise.Incentive.Application.Audit.DTOs;
using Dorise.Incentive.Application.Audit.Services;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Audit;

/// <summary>
/// Implementation of audit service for logging and compliance tracking.
/// "I'm learnding!" - And we're learning everything that happens in the system!
/// </summary>
public class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuditService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuditService(
        IAuditLogRepository auditRepository,
        IHttpContextAccessor httpContextAccessor,
        IConfiguration configuration,
        ILogger<AuditService> logger)
    {
        _auditRepository = auditRepository;
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
        _logger = logger;
    }

    #region Audit Logging

    public async Task LogAsync(
        CreateAuditLogRequest request,
        CancellationToken cancellationToken = default)
    {
        var context = GetAuditContext();

        var auditLog = AuditLog.Create(
            entityType: request.EntityType,
            entityId: request.EntityId,
            action: request.Action,
            userId: context.UserId,
            userName: context.UserName,
            userEmail: context.UserEmail,
            oldValues: request.OldValue != null ? JsonSerializer.Serialize(request.OldValue, JsonOptions) : null,
            newValues: request.NewValue != null ? JsonSerializer.Serialize(request.NewValue, JsonOptions) : null,
            ipAddress: context.IpAddress,
            userAgent: context.UserAgent,
            reason: request.Reason,
            correlationId: context.CorrelationId,
            additionalData: request.AdditionalData != null
                ? JsonSerializer.Serialize(request.AdditionalData, JsonOptions)
                : null);

        await _auditRepository.AddAsync(auditLog, cancellationToken);

        _logger.LogDebug(
            "Audit log created: {Action} on {EntityType}/{EntityId} by {UserName}",
            request.Action, request.EntityType, request.EntityId, context.UserName);
    }

    public async Task LogCreateAsync<T>(
        T entity,
        string? reason = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var entityType = typeof(T).Name;
        var entityId = GetEntityId(entity);

        await LogAsync(new CreateAuditLogRequest
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = AuditAction.Create,
            NewValue = entity,
            Reason = reason
        }, cancellationToken);
    }

    public async Task LogUpdateAsync<T>(
        T oldEntity,
        T newEntity,
        string? reason = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var entityType = typeof(T).Name;
        var entityId = GetEntityId(newEntity);
        var changedProperties = GetChangedProperties(oldEntity, newEntity);

        var context = GetAuditContext();

        var auditLog = AuditLog.Create(
            entityType: entityType,
            entityId: entityId,
            action: AuditAction.Update,
            userId: context.UserId,
            userName: context.UserName,
            userEmail: context.UserEmail,
            oldValues: JsonSerializer.Serialize(oldEntity, JsonOptions),
            newValues: JsonSerializer.Serialize(newEntity, JsonOptions),
            changedProperties: string.Join(",", changedProperties),
            ipAddress: context.IpAddress,
            userAgent: context.UserAgent,
            reason: reason,
            correlationId: context.CorrelationId);

        await _auditRepository.AddAsync(auditLog, cancellationToken);
    }

    public async Task LogDeleteAsync<T>(
        T entity,
        string? reason = null,
        CancellationToken cancellationToken = default) where T : class
    {
        var entityType = typeof(T).Name;
        var entityId = GetEntityId(entity);

        await LogAsync(new CreateAuditLogRequest
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = AuditAction.Delete,
            OldValue = entity,
            Reason = reason
        }, cancellationToken);
    }

    public async Task LogAccessAsync(
        string entityType,
        Guid entityId,
        bool isSensitive = false,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(new CreateAuditLogRequest
        {
            EntityType = entityType,
            EntityId = entityId,
            Action = AuditAction.View,
            AdditionalData = new Dictionary<string, object>
            {
                ["IsSensitive"] = isSensitive
            }
        }, cancellationToken);
    }

    public async Task LogExportAsync(
        string entityType,
        int recordCount,
        string format,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(new CreateAuditLogRequest
        {
            EntityType = entityType,
            EntityId = Guid.Empty,
            Action = AuditAction.Export,
            AdditionalData = new Dictionary<string, object>
            {
                ["RecordCount"] = recordCount,
                ["Format"] = format,
                ["ExportedAt"] = DateTime.UtcNow
            }
        }, cancellationToken);
    }

    public async Task LogApprovalAsync(
        Guid entityId,
        bool approved,
        string? comments = null,
        CancellationToken cancellationToken = default)
    {
        await LogAsync(new CreateAuditLogRequest
        {
            EntityType = "Calculation",
            EntityId = entityId,
            Action = approved ? AuditAction.Approve : AuditAction.Reject,
            Reason = comments,
            AdditionalData = new Dictionary<string, object>
            {
                ["Approved"] = approved,
                ["ApprovedAt"] = DateTime.UtcNow
            }
        }, cancellationToken);
    }

    public async Task LogLoginAsync(
        Guid userId,
        string userName,
        bool success,
        string? failureReason = null,
        CancellationToken cancellationToken = default)
    {
        var context = GetAuditContext();

        var auditLog = AuditLog.Create(
            entityType: "User",
            entityId: userId,
            action: AuditAction.Login,
            userId: userId,
            userName: userName,
            ipAddress: context.IpAddress,
            userAgent: context.UserAgent,
            reason: failureReason,
            additionalData: JsonSerializer.Serialize(new
            {
                Success = success,
                FailureReason = failureReason,
                LoginAt = DateTime.UtcNow
            }, JsonOptions));

        await _auditRepository.AddAsync(auditLog, cancellationToken);
    }

    public async Task LogLogoutAsync(
        Guid userId,
        string userName,
        CancellationToken cancellationToken = default)
    {
        var context = GetAuditContext();

        var auditLog = AuditLog.Create(
            entityType: "User",
            entityId: userId,
            action: AuditAction.Logout,
            userId: userId,
            userName: userName,
            ipAddress: context.IpAddress,
            userAgent: context.UserAgent,
            additionalData: JsonSerializer.Serialize(new
            {
                LogoutAt = DateTime.UtcNow
            }, JsonOptions));

        await _auditRepository.AddAsync(auditLog, cancellationToken);
    }

    public async Task LogSystemEventAsync(
        string eventType,
        string description,
        Dictionary<string, object>? data = null,
        CancellationToken cancellationToken = default)
    {
        var context = GetAuditContext();

        var auditLog = AuditLog.Create(
            entityType: "System",
            entityId: Guid.Empty,
            action: AuditAction.SystemEvent,
            userId: context.UserId,
            userName: context.UserName,
            reason: description,
            correlationId: context.CorrelationId,
            additionalData: JsonSerializer.Serialize(new
            {
                EventType = eventType,
                Description = description,
                Data = data,
                Timestamp = DateTime.UtcNow
            }, JsonOptions));

        await _auditRepository.AddAsync(auditLog, cancellationToken);
    }

    #endregion

    #region Audit Queries

    public async Task<AuditLogPagedResult> SearchAsync(
        AuditLogSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _auditRepository.SearchAsync(
            entityType: query.EntityType,
            entityId: query.EntityId,
            action: query.Action,
            userId: query.UserId,
            fromDate: query.FromDate,
            toDate: query.ToDate,
            correlationId: query.CorrelationId,
            page: query.Page,
            pageSize: query.PageSize,
            sortBy: query.SortBy,
            sortDescending: query.SortDescending,
            cancellationToken: cancellationToken);

        return new AuditLogPagedResult
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<AuditLogDto?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var auditLog = await _auditRepository.GetByIdAsync(id, cancellationToken);
        return auditLog != null ? MapToDto(auditLog) : null;
    }

    public async Task<IReadOnlyList<AuditLogDto>> GetEntityHistoryAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditRepository.GetByEntityAsync(entityType, entityId, cancellationToken);
        return logs.Select(MapToDto).ToList();
    }

    public async Task<EntityAuditSummaryDto> GetEntitySummaryAsync(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditRepository.GetByEntityAsync(entityType, entityId, cancellationToken);
        var logList = logs.ToList();

        var createLog = logList.FirstOrDefault(l => l.Action == AuditAction.Create);
        var lastUpdateLog = logList.FirstOrDefault(l => l.Action == AuditAction.Update);

        return new EntityAuditSummaryDto
        {
            EntityType = entityType,
            EntityId = entityId,
            TotalChanges = logList.Count,
            FirstChange = logList.LastOrDefault()?.Timestamp,
            LastChange = logList.FirstOrDefault()?.Timestamp,
            CreatedBy = createLog?.UserName,
            CreatedAt = createLog?.Timestamp,
            LastModifiedBy = lastUpdateLog?.UserName ?? createLog?.UserName,
            LastModifiedAt = lastUpdateLog?.Timestamp ?? createLog?.Timestamp,
            ActionCounts = logList
                .GroupBy(l => l.Action)
                .ToDictionary(g => g.Key, g => g.Count()),
            RecentChanges = logList.Take(10).Select(MapToDto).ToList()
        };
    }

    public async Task<UserActivitySummaryDto> GetUserActivityAsync(
        Guid userId,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditRepository.GetByUserAsync(userId, fromDate, toDate, cancellationToken);
        var logList = logs.ToList();

        var firstLog = logList.LastOrDefault();

        return new UserActivitySummaryDto
        {
            UserId = userId,
            UserName = firstLog?.UserName,
            UserEmail = firstLog?.UserEmail,
            TotalActions = logList.Count,
            FirstActivity = logList.LastOrDefault()?.Timestamp,
            LastActivity = logList.FirstOrDefault()?.Timestamp,
            ActionCounts = logList
                .GroupBy(l => l.Action)
                .ToDictionary(g => g.Key, g => g.Count()),
            EntityTypeCounts = logList
                .GroupBy(l => l.EntityType)
                .ToDictionary(g => g.Key, g => g.Count()),
            RecentActivity = logList.Take(20).Select(MapToDto).ToList()
        };
    }

    public async Task<AuditStatisticsDto> GetStatisticsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var weekStart = now.Date.AddDays(-(int)now.DayOfWeek);
        var monthStart = new DateTime(now.Year, now.Month, 1);

        var totalCount = await _auditRepository.CountAsync(
            fromDate: fromDate, toDate: toDate, cancellationToken: cancellationToken);

        var todayCount = await _auditRepository.CountAsync(
            fromDate: todayStart, cancellationToken: cancellationToken);

        var weekCount = await _auditRepository.CountAsync(
            fromDate: weekStart, cancellationToken: cancellationToken);

        var monthCount = await _auditRepository.CountAsync(
            fromDate: monthStart, cancellationToken: cancellationToken);

        var oldest = await _auditRepository.GetOldestAsync(cancellationToken);
        var newest = await _auditRepository.GetNewestAsync(cancellationToken);

        // Get action breakdown
        var byAction = new Dictionary<AuditAction, int>();
        foreach (AuditAction action in Enum.GetValues<AuditAction>())
        {
            var count = await _auditRepository.CountAsync(
                action: action, fromDate: fromDate, toDate: toDate, cancellationToken: cancellationToken);
            if (count > 0)
                byAction[action] = count;
        }

        // Get entity type breakdown
        var entityTypes = await _auditRepository.GetEntityTypesAsync(cancellationToken);
        var byEntityType = new Dictionary<string, int>();
        foreach (var entityType in entityTypes)
        {
            var count = await _auditRepository.CountAsync(
                entityType: entityType, fromDate: fromDate, toDate: toDate, cancellationToken: cancellationToken);
            byEntityType[entityType] = count;
        }

        return new AuditStatisticsDto
        {
            TotalEntries = totalCount,
            TodayEntries = todayCount,
            ThisWeekEntries = weekCount,
            ThisMonthEntries = monthCount,
            ByAction = byAction,
            ByEntityType = byEntityType,
            OldestEntry = oldest?.Timestamp,
            NewestEntry = newest?.Timestamp
        };
    }

    #endregion

    #region Compliance Reports

    public async Task<ApprovalComplianceReportDto> GetApprovalComplianceReportAsync(
        string period,
        CancellationToken cancellationToken = default)
    {
        // Parse period (format: YYYY-MM)
        var periodDate = DateTime.ParseExact(period + "-01", "yyyy-MM-dd", null);
        var fromDate = periodDate;
        var toDate = periodDate.AddMonths(1).AddDays(-1);

        var approvalLogs = await _auditRepository.GetByActionAsync(
            AuditAction.Approve, fromDate, toDate, cancellationToken);

        var rejectionLogs = await _auditRepository.GetByActionAsync(
            AuditAction.Reject, fromDate, toDate, cancellationToken);

        var allLogs = approvalLogs.Concat(rejectionLogs).ToList();

        // Build approver activities
        var approverActivities = allLogs
            .Where(l => l.UserId.HasValue)
            .GroupBy(l => l.UserId!.Value)
            .Select(g => new ApproverActivityDto
            {
                ApproverId = g.Key,
                ApproverName = g.First().UserName ?? "Unknown",
                ApprovalsCount = g.Count(l => l.Action == AuditAction.Approve),
                RejectionsCount = g.Count(l => l.Action == AuditAction.Reject),
                AverageResponseTimeHours = 0, // Would need submission time to calculate
                SlaBreaches = 0 // Would need SLA configuration
            })
            .ToList();

        return new ApprovalComplianceReportDto
        {
            Period = period,
            GeneratedAt = DateTime.UtcNow,
            TotalApprovals = approvalLogs.Count,
            TotalRejections = rejectionLogs.Count,
            TotalPending = 0, // Would need to query calculations
            TotalApprovedAmount = 0, // Would need to join with calculations
            TotalRejectedAmount = 0,
            AverageApprovalTimeHours = 0,
            SlaBreaches = 0,
            ApproverActivities = approverActivities,
            AuditTrail = new List<ApprovalAuditDto>()
        };
    }

    public async Task<DataAccessReportDto> GetDataAccessReportAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var accessLogs = await _auditRepository.GetByActionAsync(
            AuditAction.View, fromDate, toDate, cancellationToken);

        var exportLogs = await _auditRepository.GetByActionAsync(
            AuditAction.Export, fromDate, toDate, cancellationToken);

        var allLogs = accessLogs.Concat(exportLogs).ToList();

        return new DataAccessReportDto
        {
            Period = $"{fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}",
            GeneratedAt = DateTime.UtcNow,
            TotalDataAccesses = accessLogs.Count,
            TotalExports = exportLogs.Count,
            TotalSensitiveAccesses = allLogs.Count(l =>
                l.AdditionalData?.Contains("\"IsSensitive\":true") == true),
            Entries = allLogs.Take(100).Select(l => new DataAccessEntryDto
            {
                Timestamp = l.Timestamp,
                UserName = l.UserName ?? "Unknown",
                EntityType = l.EntityType,
                EntityId = l.EntityId,
                Action = l.Action.ToString(),
                IpAddress = l.IpAddress,
                IsSensitive = l.AdditionalData?.Contains("\"IsSensitive\":true") == true
            }).ToList(),
            AccessByEntityType = allLogs
                .GroupBy(l => l.EntityType)
                .ToDictionary(g => g.Key, g => g.Count()),
            AccessByUser = allLogs
                .Where(l => l.UserName != null)
                .GroupBy(l => l.UserName!)
                .ToDictionary(g => g.Key, g => g.Count())
        };
    }

    public async Task<IReadOnlyList<SystemChangeLogDto>> GetSystemChangeLogAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditRepository.GetByActionAsync(
            AuditAction.ConfigurationChange, fromDate, toDate, cancellationToken);

        var systemEvents = await _auditRepository.GetByActionAsync(
            AuditAction.SystemEvent, fromDate, toDate, cancellationToken);

        return logs.Concat(systemEvents)
            .OrderByDescending(l => l.Timestamp)
            .Select(l => new SystemChangeLogDto
            {
                Timestamp = l.Timestamp,
                ChangeType = l.Action.ToString(),
                Description = l.Reason ?? "No description",
                ChangedBy = l.UserName ?? "System",
                OldValue = l.OldValues,
                NewValue = l.NewValues,
                Reason = l.Reason
            })
            .ToList();
    }

    #endregion

    #region Retention

    public Task<RetentionPolicyDto> GetRetentionPolicyAsync(
        CancellationToken cancellationToken = default)
    {
        var config = _configuration.GetSection("Audit:Retention");

        return Task.FromResult(new RetentionPolicyDto
        {
            DefaultRetentionDays = config.GetValue<int>("DefaultDays", 365),
            SensitiveDataRetentionDays = config.GetValue<int>("SensitiveDays", 2555),
            LoginAuditRetentionDays = config.GetValue<int>("LoginDays", 90),
            ExportAuditRetentionDays = config.GetValue<int>("ExportDays", 365),
            AutoPurgeEnabled = config.GetValue<bool>("AutoPurge", false)
        });
    }

    public async Task<int> PurgeOldLogsAsync(
        CancellationToken cancellationToken = default)
    {
        var policy = await GetRetentionPolicyAsync(cancellationToken);
        var cutoffDate = DateTime.UtcNow.AddDays(-policy.DefaultRetentionDays);

        var deletedCount = await _auditRepository.DeleteOlderThanAsync(cutoffDate, cancellationToken);

        await LogSystemEventAsync(
            "AuditPurge",
            $"Purged {deletedCount} audit logs older than {cutoffDate:yyyy-MM-dd}",
            new Dictionary<string, object>
            {
                ["DeletedCount"] = deletedCount,
                ["CutoffDate"] = cutoffDate
            },
            cancellationToken);

        _logger.LogInformation("Purged {Count} audit logs older than {CutoffDate}",
            deletedCount, cutoffDate);

        return deletedCount;
    }

    public async Task<string> ArchiveLogsAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken = default)
    {
        var logs = await _auditRepository.GetByDateRangeAsync(fromDate, toDate, cancellationToken);

        var archiveFileName = $"audit_archive_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.json";
        var archivePath = Path.Combine(Path.GetTempPath(), archiveFileName);

        var json = JsonSerializer.Serialize(logs.Select(MapToDto), new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await File.WriteAllTextAsync(archivePath, json, cancellationToken);

        await LogSystemEventAsync(
            "AuditArchive",
            $"Archived {logs.Count} audit logs to {archiveFileName}",
            new Dictionary<string, object>
            {
                ["LogCount"] = logs.Count,
                ["FromDate"] = fromDate,
                ["ToDate"] = toDate,
                ["ArchivePath"] = archivePath
            },
            cancellationToken);

        _logger.LogInformation("Archived {Count} audit logs to {Path}", logs.Count, archivePath);

        return archivePath;
    }

    #endregion

    #region Private Helpers

    private AuditContext GetAuditContext()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        var user = httpContext?.User;

        return new AuditContext
        {
            UserId = Guid.TryParse(user?.FindFirst("sub")?.Value ?? user?.FindFirst("nameid")?.Value, out var id)
                ? id
                : null,
            UserName = user?.Identity?.Name ?? user?.FindFirst("name")?.Value,
            UserEmail = user?.FindFirst("email")?.Value,
            IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request.Headers.UserAgent.ToString(),
            CorrelationId = httpContext?.TraceIdentifier
        };
    }

    private static Guid GetEntityId<T>(T entity) where T : class
    {
        var idProperty = typeof(T).GetProperty("Id");
        if (idProperty?.GetValue(entity) is Guid id)
            return id;
        return Guid.Empty;
    }

    private static List<string> GetChangedProperties<T>(T oldEntity, T newEntity) where T : class
    {
        var changedProperties = new List<string>();
        var properties = typeof(T).GetProperties()
            .Where(p => p.CanRead && p.PropertyType.IsValueType || p.PropertyType == typeof(string));

        foreach (var prop in properties)
        {
            var oldValue = prop.GetValue(oldEntity);
            var newValue = prop.GetValue(newEntity);

            if (!Equals(oldValue, newValue))
            {
                changedProperties.Add(prop.Name);
            }
        }

        return changedProperties;
    }

    private static AuditLogDto MapToDto(AuditLog log)
    {
        return new AuditLogDto
        {
            Id = log.Id,
            EntityType = log.EntityType,
            EntityId = log.EntityId,
            Action = log.Action,
            OldValues = !string.IsNullOrEmpty(log.OldValues)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(log.OldValues)
                : null,
            NewValues = !string.IsNullOrEmpty(log.NewValues)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(log.NewValues)
                : null,
            ChangedProperties = !string.IsNullOrEmpty(log.ChangedProperties)
                ? log.ChangedProperties.Split(',').ToList()
                : null,
            UserId = log.UserId,
            UserName = log.UserName,
            UserEmail = log.UserEmail,
            IpAddress = log.IpAddress,
            UserAgent = log.UserAgent,
            Reason = log.Reason,
            CorrelationId = log.CorrelationId,
            Timestamp = log.Timestamp,
            AdditionalData = !string.IsNullOrEmpty(log.AdditionalData)
                ? JsonSerializer.Deserialize<Dictionary<string, object>>(log.AdditionalData)
                : null
        };
    }

    private record AuditContext
    {
        public Guid? UserId { get; init; }
        public string? UserName { get; init; }
        public string? UserEmail { get; init; }
        public string? IpAddress { get; init; }
        public string? UserAgent { get; init; }
        public string? CorrelationId { get; init; }
    }

    #endregion
}
