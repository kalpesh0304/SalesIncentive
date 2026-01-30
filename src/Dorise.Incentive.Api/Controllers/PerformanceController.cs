using Dorise.Incentive.Application.Performance.DTOs;
using Dorise.Incentive.Application.Performance.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for performance monitoring and optimization.
/// "I'm learnding!" - Learning about system performance!
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class PerformanceController : ControllerBase
{
    private readonly IPerformanceMonitorService _performanceService;
    private readonly ICacheManagementService _cacheManagementService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<PerformanceController> _logger;

    public PerformanceController(
        IPerformanceMonitorService performanceService,
        ICacheManagementService cacheManagementService,
        ICacheService cacheService,
        ILogger<PerformanceController> logger)
    {
        _performanceService = performanceService;
        _cacheManagementService = cacheManagementService;
        _cacheService = cacheService;
        _logger = logger;
    }

    // ==================== System Metrics ====================

    /// <summary>
    /// Get current system performance metrics.
    /// </summary>
    [HttpGet("metrics")]
    public async Task<ActionResult<SystemPerformanceDto>> GetCurrentMetrics(
        CancellationToken cancellationToken = default)
    {
        var metrics = await _performanceService.GetCurrentMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Get performance trend data.
    /// </summary>
    [HttpGet("trends")]
    public async Task<ActionResult<List<PerformanceTrendDto>>> GetTrends(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] string interval = "hour",
        CancellationToken cancellationToken = default)
    {
        var from = fromDate ?? DateTime.UtcNow.AddHours(-24);
        var to = toDate ?? DateTime.UtcNow;

        var trends = await _performanceService.GetTrendDataAsync(from, to, interval, cancellationToken);
        return Ok(trends);
    }

    /// <summary>
    /// Get performance dashboard summary.
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<PerformanceDashboardDto>> GetDashboard(
        CancellationToken cancellationToken = default)
    {
        var dashboard = await _performanceService.GetDashboardAsync(cancellationToken);
        return Ok(dashboard);
    }

    // ==================== Endpoint Metrics ====================

    /// <summary>
    /// Get endpoint performance metrics.
    /// </summary>
    [HttpGet("endpoints")]
    public async Task<ActionResult<List<EndpointPerformanceDto>>> GetEndpointMetrics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] int topN = 20,
        CancellationToken cancellationToken = default)
    {
        var metrics = await _performanceService.GetEndpointMetricsAsync(
            fromDate, toDate, topN, cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Get metrics for a specific endpoint.
    /// </summary>
    [HttpGet("endpoints/{method}/{*endpoint}")]
    public async Task<ActionResult<EndpointPerformanceDto>> GetEndpointMetrics(
        string method,
        string endpoint,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var metrics = await _performanceService.GetEndpointMetricsAsync(
            $"/{endpoint}", method.ToUpper(), fromDate, toDate, cancellationToken);

        if (metrics == null)
            return NotFound();

        return Ok(metrics);
    }

    // ==================== Query Metrics ====================

    /// <summary>
    /// Get query performance metrics.
    /// </summary>
    [HttpGet("queries")]
    public async Task<ActionResult<List<QueryPerformanceDto>>> GetQueryMetrics(
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        [FromQuery] bool slowOnly = false,
        [FromQuery] int topN = 20,
        CancellationToken cancellationToken = default)
    {
        var metrics = await _performanceService.GetQueryMetricsAsync(
            fromDate, toDate, slowOnly, topN, cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Get slow queries.
    /// </summary>
    [HttpGet("queries/slow")]
    public async Task<ActionResult<List<SlowQueryDto>>> GetSlowQueries(
        [FromQuery] int count = 50,
        [FromQuery] double thresholdMs = 1000,
        CancellationToken cancellationToken = default)
    {
        var queries = await _performanceService.GetSlowQueriesAsync(count, thresholdMs, cancellationToken);
        return Ok(queries);
    }

    // ==================== Health Checks ====================

    /// <summary>
    /// Run health checks.
    /// </summary>
    [HttpGet("health")]
    [AllowAnonymous]
    public async Task<ActionResult<HealthCheckResultDto>> RunHealthChecks(
        CancellationToken cancellationToken = default)
    {
        var result = await _performanceService.RunHealthChecksAsync(cancellationToken);

        var statusCode = result.Status switch
        {
            "Healthy" => 200,
            "Degraded" => 200,
            _ => 503
        };

        return StatusCode(statusCode, result);
    }

    /// <summary>
    /// Simple liveness probe.
    /// </summary>
    [HttpGet("health/live")]
    [AllowAnonymous]
    public IActionResult LivenessProbe()
    {
        return Ok(new { status = "alive", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Readiness probe.
    /// </summary>
    [HttpGet("health/ready")]
    [AllowAnonymous]
    public async Task<IActionResult> ReadinessProbe(CancellationToken cancellationToken = default)
    {
        var health = await _performanceService.RunHealthChecksAsync(cancellationToken);
        if (health.Status == "Unhealthy")
            return StatusCode(503, new { status = "not ready", details = health });

        return Ok(new { status = "ready", timestamp = DateTime.UtcNow });
    }

    // ==================== Cache Management ====================

    /// <summary>
    /// Get cache metrics.
    /// </summary>
    [HttpGet("cache/metrics")]
    public async Task<ActionResult<CacheMetricsDto>> GetCacheMetrics(
        CancellationToken cancellationToken = default)
    {
        var metrics = await _cacheManagementService.GetCacheMetricsAsync(cancellationToken);
        return Ok(metrics);
    }

    /// <summary>
    /// Get cache statistics.
    /// </summary>
    [HttpGet("cache/statistics")]
    public async Task<ActionResult<CacheStatistics>> GetCacheStatistics(
        CancellationToken cancellationToken = default)
    {
        var stats = await _cacheService.GetStatisticsAsync(cancellationToken);
        return Ok(stats);
    }

    /// <summary>
    /// Get a specific cache value.
    /// </summary>
    [HttpGet("cache/keys/{key}")]
    public async Task<ActionResult<object>> GetCacheValue(
        string key,
        CancellationToken cancellationToken = default)
    {
        var value = await _cacheService.GetAsync<object>(key, cancellationToken);
        if (value == null)
            return NotFound();
        return Ok(value);
    }

    /// <summary>
    /// Check if a cache key exists.
    /// </summary>
    [HttpHead("cache/keys/{key}")]
    public async Task<IActionResult> CheckCacheKey(
        string key,
        CancellationToken cancellationToken = default)
    {
        var exists = await _cacheService.ExistsAsync(key, cancellationToken);
        return exists ? Ok() : NotFound();
    }

    /// <summary>
    /// Invalidate a specific cache key.
    /// </summary>
    [HttpDelete("cache/keys/{key}")]
    public async Task<IActionResult> InvalidateCacheKey(
        string key,
        CancellationToken cancellationToken = default)
    {
        await _cacheManagementService.InvalidateCacheAsync(key, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Invalidate cache by pattern.
    /// </summary>
    [HttpDelete("cache/pattern")]
    public async Task<IActionResult> InvalidateCacheByPattern(
        [FromQuery] string pattern,
        CancellationToken cancellationToken = default)
    {
        await _cacheManagementService.InvalidateCacheByPatternAsync(pattern, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Invalidate cache by tag.
    /// </summary>
    [HttpDelete("cache/tags/{tag}")]
    public async Task<IActionResult> InvalidateCacheByTag(
        string tag,
        CancellationToken cancellationToken = default)
    {
        await _cacheManagementService.InvalidateCacheByTagAsync(tag, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Clear all cache.
    /// </summary>
    [HttpDelete("cache/all")]
    public async Task<IActionResult> ClearAllCache(
        CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Full cache clear requested by user");
        await _cacheManagementService.ClearAllCacheAsync(cancellationToken);
        return NoContent();
    }

    // ==================== Cache Warmup ====================

    /// <summary>
    /// Get cache warmup configurations.
    /// </summary>
    [HttpGet("cache/warmup")]
    public async Task<ActionResult<List<CacheWarmupConfigDto>>> GetWarmupConfigs(
        CancellationToken cancellationToken = default)
    {
        var configs = await _cacheManagementService.GetWarmupConfigsAsync(cancellationToken);
        return Ok(configs);
    }

    /// <summary>
    /// Add cache warmup configuration.
    /// </summary>
    [HttpPost("cache/warmup")]
    public async Task<IActionResult> AddWarmupConfig(
        [FromBody] CacheWarmupConfigDto config,
        CancellationToken cancellationToken = default)
    {
        await _cacheManagementService.AddWarmupConfigAsync(config, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Remove cache warmup configuration.
    /// </summary>
    [HttpDelete("cache/warmup/{cacheKey}")]
    public async Task<IActionResult> RemoveWarmupConfig(
        string cacheKey,
        CancellationToken cancellationToken = default)
    {
        await _cacheManagementService.RemoveWarmupConfigAsync(cacheKey, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Trigger cache warmup.
    /// </summary>
    [HttpPost("cache/warmup/trigger")]
    public async Task<IActionResult> TriggerWarmup(
        [FromQuery] string? cacheKey = null,
        CancellationToken cancellationToken = default)
    {
        await _cacheManagementService.WarmupCacheAsync(cacheKey, cancellationToken);
        return Ok(new { message = "Cache warmup triggered", key = cacheKey ?? "all" });
    }

    // ==================== Cleanup ====================

    /// <summary>
    /// Cleanup old performance metrics.
    /// </summary>
    [HttpPost("cleanup")]
    public async Task<ActionResult<int>> CleanupOldMetrics(
        [FromQuery] int daysToKeep = 30,
        CancellationToken cancellationToken = default)
    {
        var removed = await _performanceService.CleanupOldMetricsAsync(daysToKeep, cancellationToken);
        return Ok(new { removedCount = removed });
    }

    // ==================== Cache Tags ====================

    /// <summary>
    /// Get available cache tags.
    /// </summary>
    [HttpGet("cache/tags")]
    public IActionResult GetCacheTags()
    {
        var tags = new
        {
            Employees = CacheKeys.Tags.Employees,
            Departments = CacheKeys.Tags.Departments,
            Plans = CacheKeys.Tags.Plans,
            Calculations = CacheKeys.Tags.Calculations,
            Configuration = CacheKeys.Tags.Configuration,
            Security = CacheKeys.Tags.Security,
            Dashboard = CacheKeys.Tags.Dashboard,
            Reports = CacheKeys.Tags.Reports
        };

        return Ok(tags);
    }

    /// <summary>
    /// Get cache duration presets.
    /// </summary>
    [HttpGet("cache/durations")]
    public IActionResult GetCacheDurations()
    {
        var durations = new
        {
            VeryShort = CacheDurations.VeryShort.TotalMinutes,
            Short = CacheDurations.Short.TotalMinutes,
            Medium = CacheDurations.Medium.TotalMinutes,
            Long = CacheDurations.Long.TotalMinutes,
            VeryLong = CacheDurations.VeryLong.TotalMinutes,
            Day = CacheDurations.Day.TotalHours,
            Configuration = CacheDurations.Configuration.TotalMinutes,
            FeatureFlags = CacheDurations.FeatureFlags.TotalMinutes,
            UserPermissions = CacheDurations.UserPermissions.TotalMinutes,
            Dashboard = CacheDurations.Dashboard.TotalMinutes
        };

        return Ok(durations);
    }
}
