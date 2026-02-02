using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Dorise.Incentive.Infrastructure.Audit;

/// <summary>
/// EF Core interceptor for automatic audit logging.
/// "I'm Idaho!" - And I'm auditing every change!
/// </summary>
public class AuditSaveChangesInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<AuditSaveChangesInterceptor> _logger;
    private List<AuditEntry> _pendingAuditEntries = new();

    // Entity types to exclude from auditing
    private static readonly HashSet<string> ExcludedEntityTypes = new()
    {
        nameof(AuditLog) // Don't audit audit logs
    };

    public AuditSaveChangesInterceptor(
        IHttpContextAccessor httpContextAccessor,
        ILogger<AuditSaveChangesInterceptor> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            BeforeSaveChanges(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            BeforeSaveChanges(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(
        SaveChangesCompletedEventData eventData,
        int result)
    {
        if (eventData.Context is not null)
        {
            AfterSaveChanges(eventData.Context).GetAwaiter().GetResult();
        }

        return base.SavedChanges(eventData, result);
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            await AfterSaveChanges(eventData.Context);
        }

        return await base.SavedChangesAsync(eventData, result, cancellationToken);
    }

    private void BeforeSaveChanges(DbContext context)
    {
        _pendingAuditEntries.Clear();
        context.ChangeTracker.DetectChanges();

        var httpContext = _httpContextAccessor.HttpContext;
        var userId = GetUserId(httpContext);
        var userName = GetUserName(httpContext);
        var userEmail = GetUserEmail(httpContext);
        var ipAddress = GetIpAddress(httpContext);
        var userAgent = GetUserAgent(httpContext);
        var correlationId = GetCorrelationId(httpContext);

        foreach (var entry in context.ChangeTracker.Entries())
        {
            // Skip excluded entity types
            if (ExcludedEntityTypes.Contains(entry.Entity.GetType().Name))
                continue;

            // Skip unchanged entities
            if (entry.State == EntityState.Unchanged || entry.State == EntityState.Detached)
                continue;

            var auditEntry = new AuditEntry
            {
                EntityType = entry.Entity.GetType().Name,
                Action = entry.State switch
                {
                    EntityState.Added => AuditAction.Create,
                    EntityState.Modified => AuditAction.Update,
                    EntityState.Deleted => AuditAction.Delete,
                    _ => AuditAction.Access
                },
                UserId = userId,
                UserName = userName,
                UserEmail = userEmail,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                CorrelationId = correlationId,
                Timestamp = DateTime.UtcNow,
                Entry = entry
            };

            // For added entities, we need to get the ID after save
            if (entry.State == EntityState.Added)
            {
                auditEntry.TemporaryProperties = entry.Properties
                    .Where(p => p.IsTemporary)
                    .ToList();
            }

            CaptureChanges(auditEntry, entry);
            _pendingAuditEntries.Add(auditEntry);
        }
    }

    private async Task AfterSaveChanges(DbContext context)
    {
        if (_pendingAuditEntries.Count == 0)
            return;

        try
        {
            var auditLogs = new List<AuditLog>();

            foreach (var auditEntry in _pendingAuditEntries)
            {
                // Get the entity ID now that it's been generated
                auditEntry.EntityId = GetEntityId(auditEntry.Entry);

                // For added entities with temporary properties, capture the final values
                if (auditEntry.TemporaryProperties?.Any() == true)
                {
                    foreach (var prop in auditEntry.TemporaryProperties)
                    {
                        if (prop.Metadata.IsPrimaryKey())
                        {
                            auditEntry.EntityId = prop.CurrentValue is Guid guid ? guid : Guid.Empty;
                        }
                        else
                        {
                            auditEntry.NewValues[prop.Metadata.Name] = prop.CurrentValue;
                        }
                    }
                }

                var auditLog = AuditLog.Create(
                    auditEntry.EntityType,
                    auditEntry.EntityId,
                    auditEntry.Action,
                    auditEntry.UserId,
                    auditEntry.UserName,
                    auditEntry.UserEmail,
                    auditEntry.OldValues.Count > 0 ? JsonSerializer.Serialize(auditEntry.OldValues) : null,
                    auditEntry.NewValues.Count > 0 ? JsonSerializer.Serialize(auditEntry.NewValues) : null,
                    auditEntry.ChangedProperties.Count > 0 ? string.Join(",", auditEntry.ChangedProperties) : null,
                    auditEntry.IpAddress,
                    auditEntry.UserAgent,
                    null, // reason
                    auditEntry.CorrelationId,
                    null); // additionalData

                auditLogs.Add(auditLog);
            }

            if (auditLogs.Count > 0)
            {
                context.Set<AuditLog>().AddRange(auditLogs);
                await context.SaveChangesAsync();

                _logger.LogDebug(
                    "Created {Count} audit log entries for correlation {CorrelationId}",
                    auditLogs.Count,
                    _pendingAuditEntries.FirstOrDefault()?.CorrelationId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save audit logs");
            // Don't throw - audit logging failure shouldn't break the main operation
        }
        finally
        {
            _pendingAuditEntries.Clear();
        }
    }

    private void CaptureChanges(AuditEntry auditEntry, EntityEntry entry)
    {
        foreach (var property in entry.Properties)
        {
            // Skip navigation properties and shadow properties
            if (property.Metadata.IsPrimaryKey())
            {
                if (entry.State != EntityState.Added)
                {
                    auditEntry.EntityId = property.CurrentValue is Guid guid ? guid : Guid.Empty;
                }
                continue;
            }

            // Skip properties that are not modified
            if (entry.State == EntityState.Modified && !property.IsModified)
                continue;

            var propertyName = property.Metadata.Name;

            switch (entry.State)
            {
                case EntityState.Added:
                    auditEntry.NewValues[propertyName] = property.CurrentValue;
                    auditEntry.ChangedProperties.Add(propertyName);
                    break;

                case EntityState.Deleted:
                    auditEntry.OldValues[propertyName] = property.OriginalValue;
                    break;

                case EntityState.Modified:
                    if (!Equals(property.OriginalValue, property.CurrentValue))
                    {
                        auditEntry.OldValues[propertyName] = property.OriginalValue;
                        auditEntry.NewValues[propertyName] = property.CurrentValue;
                        auditEntry.ChangedProperties.Add(propertyName);
                    }
                    break;
            }
        }
    }

    private static Guid GetEntityId(EntityEntry entry)
    {
        var primaryKey = entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey());
        return primaryKey?.CurrentValue is Guid guid ? guid : Guid.Empty;
    }

    private static Guid? GetUserId(HttpContext? httpContext)
    {
        var userIdClaim = httpContext?.User?.FindFirst("sub")
            ?? httpContext?.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");

        if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
            return userId;

        return null;
    }

    private static string? GetUserName(HttpContext? httpContext)
    {
        return httpContext?.User?.FindFirst("name")?.Value
            ?? httpContext?.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value
            ?? httpContext?.User?.Identity?.Name;
    }

    private static string? GetUserEmail(HttpContext? httpContext)
    {
        return httpContext?.User?.FindFirst("email")?.Value
            ?? httpContext?.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
    }

    private static string? GetIpAddress(HttpContext? httpContext)
    {
        // Check for forwarded IP (behind proxy/load balancer)
        var forwardedFor = httpContext?.Request?.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').FirstOrDefault()?.Trim();
        }

        return httpContext?.Connection?.RemoteIpAddress?.ToString();
    }

    private static string? GetUserAgent(HttpContext? httpContext)
    {
        return httpContext?.Request?.Headers["User-Agent"].FirstOrDefault();
    }

    private static string? GetCorrelationId(HttpContext? httpContext)
    {
        return httpContext?.Request?.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? httpContext?.TraceIdentifier;
    }

    /// <summary>
    /// Internal class to track audit entry data during save operation.
    /// </summary>
    private class AuditEntry
    {
        public required EntityEntry Entry { get; init; }
        public required string EntityType { get; init; }
        public Guid EntityId { get; set; }
        public required AuditAction Action { get; init; }
        public Dictionary<string, object?> OldValues { get; } = new();
        public Dictionary<string, object?> NewValues { get; } = new();
        public List<string> ChangedProperties { get; } = new();
        public Guid? UserId { get; init; }
        public string? UserName { get; init; }
        public string? UserEmail { get; init; }
        public string? IpAddress { get; init; }
        public string? UserAgent { get; init; }
        public string? CorrelationId { get; init; }
        public DateTime Timestamp { get; init; }
        public List<PropertyEntry>? TemporaryProperties { get; set; }
    }
}
