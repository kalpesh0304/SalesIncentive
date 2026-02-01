using Asp.Versioning;
using Dorise.Incentive.Application.Audit.DTOs;
using Dorise.Incentive.Application.Audit.Services;
using Dorise.Incentive.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for audit log and compliance management.
/// "The doctor said I wouldn't have so many nose bleeds if I kept my finger outta there." - Keep your fingers in the audit logs!
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Roles = "Admin,Auditor,Compliance")]
[Produces("application/json")]
public class AuditController : ControllerBase
{
    private readonly IAuditService _auditService;
    private readonly ILogger<AuditController> _logger;

    public AuditController(
        IAuditService auditService,
        ILogger<AuditController> logger)
    {
        _auditService = auditService;
        _logger = logger;
    }

    /// <summary>
    /// Search audit logs with filtering and pagination.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(AuditLogPagedResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchAuditLogs(
        [FromQuery] AuditLogSearchQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Searching audit logs with query: {@Query}", query);
        var result = await _auditService.SearchAsync(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get a specific audit log entry by ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AuditLogDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetAuditLog(
        Guid id,
        CancellationToken cancellationToken)
    {
        var auditLog = await _auditService.GetByIdAsync(id, cancellationToken);

        if (auditLog == null)
            return NotFound();

        return Ok(auditLog);
    }

    /// <summary>
    /// Get audit history for a specific entity.
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId:guid}")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEntityHistory(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting audit history for {EntityType} {EntityId}",
            entityType, entityId);

        var history = await _auditService.GetEntityHistoryAsync(
            entityType, entityId, cancellationToken);

        return Ok(history);
    }

    /// <summary>
    /// Get audit summary for a specific entity.
    /// </summary>
    [HttpGet("entity/{entityType}/{entityId:guid}/summary")]
    [ProducesResponseType(typeof(EntityAuditSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEntitySummary(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var summary = await _auditService.GetEntitySummaryAsync(
            entityType, entityId, cancellationToken);

        return Ok(summary);
    }

    /// <summary>
    /// Get activity history for a specific user.
    /// </summary>
    [HttpGet("user/{userId:guid}/activity")]
    [ProducesResponseType(typeof(UserActivitySummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserActivity(
        Guid userId,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting activity for user {UserId} from {FromDate} to {ToDate}",
            userId, fromDate, toDate);

        var activity = await _auditService.GetUserActivityAsync(
            userId, fromDate, toDate, cancellationToken);

        return Ok(activity);
    }

    /// <summary>
    /// Get audit statistics.
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(AuditStatisticsDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics(CancellationToken cancellationToken)
    {
        var statistics = await _auditService.GetStatisticsAsync(cancellationToken);
        return Ok(statistics);
    }

    /// <summary>
    /// Get logs by correlation ID.
    /// </summary>
    [HttpGet("correlation/{correlationId}")]
    [ProducesResponseType(typeof(IReadOnlyList<AuditLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByCorrelationId(
        string correlationId,
        CancellationToken cancellationToken)
    {
        var query = new AuditLogSearchQuery { CorrelationId = correlationId };
        var result = await _auditService.SearchAsync(query, cancellationToken);
        return Ok(result.Items);
    }

    /// <summary>
    /// Get approval compliance report.
    /// </summary>
    [HttpGet("reports/approval-compliance")]
    [ProducesResponseType(typeof(ApprovalComplianceReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetApprovalComplianceReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
        var to = toDate ?? DateTime.UtcNow;

        _logger.LogInformation(
            "Generating approval compliance report from {FromDate} to {ToDate}",
            from, to);

        var report = await _auditService.GetApprovalComplianceReportAsync(
            from, to, cancellationToken);

        return Ok(report);
    }

    /// <summary>
    /// Get data access report.
    /// </summary>
    [HttpGet("reports/data-access")]
    [ProducesResponseType(typeof(DataAccessReportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDataAccessReport(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
        var to = toDate ?? DateTime.UtcNow;

        _logger.LogInformation(
            "Generating data access report from {FromDate} to {ToDate}",
            from, to);

        var report = await _auditService.GetDataAccessReportAsync(
            from, to, cancellationToken);

        return Ok(report);
    }

    /// <summary>
    /// Get system change log.
    /// </summary>
    [HttpGet("reports/system-changes")]
    [ProducesResponseType(typeof(IReadOnlyList<SystemChangeLogDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSystemChangeLog(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        CancellationToken cancellationToken)
    {
        var from = fromDate ?? DateTime.UtcNow.AddMonths(-1);
        var to = toDate ?? DateTime.UtcNow;

        var changeLog = await _auditService.GetSystemChangeLogAsync(
            from, to, cancellationToken);

        return Ok(changeLog);
    }

    /// <summary>
    /// Get current retention policy.
    /// </summary>
    [HttpGet("retention/policy")]
    [ProducesResponseType(typeof(RetentionPolicyDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRetentionPolicy(CancellationToken cancellationToken)
    {
        var policy = await _auditService.GetRetentionPolicyAsync(cancellationToken);
        return Ok(policy);
    }

    /// <summary>
    /// Purge old audit logs based on retention policy.
    /// </summary>
    [HttpPost("retention/purge")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(PurgeResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> PurgeOldLogs(
        [FromQuery] int? retentionDays,
        CancellationToken cancellationToken)
    {
        var days = retentionDays ?? 365; // Default 1 year retention

        _logger.LogWarning(
            "Purging audit logs older than {Days} days",
            days);

        var deletedCount = await _auditService.PurgeOldLogsAsync(days, cancellationToken);

        return Ok(new PurgeResultDto
        {
            DeletedCount = deletedCount,
            RetentionDays = days,
            PurgedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Archive audit logs to external storage.
    /// </summary>
    [HttpPost("retention/archive")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ArchiveResultDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ArchiveLogs(
        [FromQuery] DateTime? olderThan,
        CancellationToken cancellationToken)
    {
        var cutoffDate = olderThan ?? DateTime.UtcNow.AddYears(-1);

        _logger.LogInformation(
            "Archiving audit logs older than {CutoffDate}",
            cutoffDate);

        var archivePath = await _auditService.ArchiveLogsAsync(
            cutoffDate, cancellationToken);

        return Ok(new ArchiveResultDto
        {
            ArchivePath = archivePath,
            CutoffDate = cutoffDate,
            ArchivedAt = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Manually log an audit entry.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(AuditLogDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateAuditLog(
        [FromBody] CreateAuditLogRequest request,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var userName = GetCurrentUserName();
        var userEmail = GetCurrentUserEmail();
        var ipAddress = GetClientIpAddress();
        var userAgent = Request.Headers["User-Agent"].FirstOrDefault();
        var correlationId = Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? HttpContext.TraceIdentifier;

        await _auditService.LogAsync(
            request.EntityType,
            request.EntityId,
            request.Action,
            request.OldValue,
            request.NewValue,
            userId,
            userName,
            userEmail,
            ipAddress,
            userAgent,
            request.Reason,
            correlationId,
            request.AdditionalData,
            cancellationToken);

        // Get the created audit log
        var query = new AuditLogSearchQuery
        {
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            CorrelationId = correlationId,
            PageSize = 1
        };
        var result = await _auditService.SearchAsync(query, cancellationToken);
        var created = result.Items.FirstOrDefault();

        if (created == null)
            return BadRequest("Failed to create audit log");

        return CreatedAtAction(nameof(GetAuditLog), new { id = created.Id }, created);
    }

    /// <summary>
    /// Export audit logs to CSV format.
    /// </summary>
    [HttpGet("export")]
    [Produces("text/csv")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportAuditLogs(
        [FromQuery] AuditLogSearchQuery query,
        CancellationToken cancellationToken)
    {
        // Remove pagination limits for export
        var exportQuery = query with { Page = 1, PageSize = 100000 };
        var result = await _auditService.SearchAsync(exportQuery, cancellationToken);

        var csv = GenerateCsv(result.Items);
        var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
        var fileName = $"audit-logs-{DateTime.UtcNow:yyyy-MM-dd-HHmmss}.csv";

        // Log the export action
        await _auditService.LogExportAsync(
            "AuditLog",
            GetCurrentUserId(),
            GetCurrentUserName(),
            GetClientIpAddress(),
            result.Items.Count,
            new Dictionary<string, object> { ["Query"] = query },
            cancellationToken);

        return File(bytes, "text/csv", fileName);
    }

    private static string GenerateCsv(IReadOnlyList<AuditLogDto> items)
    {
        var sb = new System.Text.StringBuilder();

        // Header
        sb.AppendLine("Id,Timestamp,EntityType,EntityId,Action,UserName,UserEmail,IpAddress,Reason,CorrelationId");

        // Data
        foreach (var item in items)
        {
            sb.AppendLine(
                $"\"{item.Id}\"," +
                $"\"{item.Timestamp:O}\"," +
                $"\"{EscapeCsv(item.EntityType)}\"," +
                $"\"{item.EntityId}\"," +
                $"\"{item.ActionName}\"," +
                $"\"{EscapeCsv(item.UserName)}\"," +
                $"\"{EscapeCsv(item.UserEmail)}\"," +
                $"\"{EscapeCsv(item.IpAddress)}\"," +
                $"\"{EscapeCsv(item.Reason)}\"," +
                $"\"{EscapeCsv(item.CorrelationId)}\"");
        }

        return sb.ToString();
    }

    private static string? EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return value;
        return value.Replace("\"", "\"\"");
    }

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst("sub")
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
        return claim != null && Guid.TryParse(claim.Value, out var id) ? id : null;
    }

    private string? GetCurrentUserName()
    {
        return User.FindFirst("name")?.Value
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name")?.Value
            ?? User.Identity?.Name;
    }

    private string? GetCurrentUserEmail()
    {
        return User.FindFirst("email")?.Value
            ?? User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
    }

    private string? GetClientIpAddress()
    {
        var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').FirstOrDefault()?.Trim();
        }
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}

/// <summary>
/// Result of purge operation.
/// </summary>
public record PurgeResultDto
{
    public int DeletedCount { get; init; }
    public int RetentionDays { get; init; }
    public DateTime PurgedAt { get; init; }
}

/// <summary>
/// Result of archive operation.
/// </summary>
public record ArchiveResultDto
{
    public required string ArchivePath { get; init; }
    public DateTime CutoffDate { get; init; }
    public DateTime ArchivedAt { get; init; }
}
