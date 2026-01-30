using Dorise.Incentive.Application.Performance.DTOs;

namespace Dorise.Incentive.Application.Performance.Services;

/// <summary>
/// Service interface for performance monitoring.
/// "When I grow up, I want to be a principal or a caterpillar." - Growing into better performance!
/// </summary>
public interface IPerformanceMonitorService
{
    // System Metrics
    Task<SystemPerformanceDto> GetCurrentMetricsAsync(CancellationToken cancellationToken = default);
    Task<List<PerformanceTrendDto>> GetTrendDataAsync(
        DateTime fromDate,
        DateTime toDate,
        string interval = "hour",
        CancellationToken cancellationToken = default);

    // Request Metrics
    Task RecordRequestAsync(
        string endpoint,
        string method,
        int statusCode,
        double durationMs,
        CancellationToken cancellationToken = default);

    Task<List<EndpointPerformanceDto>> GetEndpointMetricsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int topN = 20,
        CancellationToken cancellationToken = default);

    Task<EndpointPerformanceDto?> GetEndpointMetricsAsync(
        string endpoint,
        string method,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default);

    // Query Metrics
    Task RecordQueryAsync(
        string queryName,
        string queryType,
        double durationMs,
        int rowsAffected,
        string? parameters = null,
        CancellationToken cancellationToken = default);

    Task<List<QueryPerformanceDto>> GetQueryMetricsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        bool slowOnly = false,
        int topN = 20,
        CancellationToken cancellationToken = default);

    Task<List<SlowQueryDto>> GetSlowQueriesAsync(
        int count = 50,
        double thresholdMs = 1000,
        CancellationToken cancellationToken = default);

    // Health Checks
    Task<HealthCheckResultDto> RunHealthChecksAsync(CancellationToken cancellationToken = default);

    // Dashboard
    Task<PerformanceDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);

    // Cleanup
    Task<int> CleanupOldMetricsAsync(int daysToKeep = 30, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for performance alerts.
/// "Hi, Super Nintendo Chalmers!" - Super alerts for super problems!
/// </summary>
public interface IPerformanceAlertService
{
    // Alert Configuration
    Task<PerformanceAlertConfigDto> CreateAlertConfigAsync(
        PerformanceAlertConfigDto config,
        CancellationToken cancellationToken = default);

    Task<PerformanceAlertConfigDto> UpdateAlertConfigAsync(
        Guid id,
        PerformanceAlertConfigDto config,
        CancellationToken cancellationToken = default);

    Task DeleteAlertConfigAsync(Guid id, CancellationToken cancellationToken = default);

    Task<List<PerformanceAlertConfigDto>> GetAlertConfigsAsync(
        bool? enabledOnly = null,
        CancellationToken cancellationToken = default);

    Task EnableAlertAsync(Guid id, CancellationToken cancellationToken = default);
    Task DisableAlertAsync(Guid id, CancellationToken cancellationToken = default);

    // Alert Events
    Task<List<PerformanceAlertDto>> GetActiveAlertsAsync(CancellationToken cancellationToken = default);

    Task<List<PerformanceAlertDto>> GetAlertHistoryAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        string? metricType = null,
        int maxResults = 100,
        CancellationToken cancellationToken = default);

    Task AcknowledgeAlertAsync(Guid alertId, CancellationToken cancellationToken = default);

    // Alert Evaluation
    Task EvaluateAlertsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for database optimization.
/// "The leprechaun tells me to burn things!" - Optimizing slow queries!
/// </summary>
public interface IDatabaseOptimizationService
{
    // Index Analysis
    Task<List<IndexAnalysisDto>> AnalyzeIndexesAsync(CancellationToken cancellationToken = default);
    Task<List<IndexAnalysisDto>> GetUnusedIndexesAsync(CancellationToken cancellationToken = default);
    Task<List<IndexAnalysisDto>> GetFragmentedIndexesAsync(
        double thresholdPercent = 30,
        CancellationToken cancellationToken = default);

    // Query Analysis
    Task<List<QueryOptimizationSuggestionDto>> AnalyzeQueriesAsync(
        CancellationToken cancellationToken = default);

    Task<QueryOptimizationSuggestionDto?> AnalyzeQueryAsync(
        string queryText,
        CancellationToken cancellationToken = default);

    // Statistics
    Task UpdateStatisticsAsync(string? tableName = null, CancellationToken cancellationToken = default);

    // Connection Pool
    Task<DatabaseMetrics> GetConnectionPoolMetricsAsync(CancellationToken cancellationToken = default);
    Task ResetConnectionPoolAsync(CancellationToken cancellationToken = default);

    // Maintenance
    Task<bool> RebuildIndexAsync(string tableName, string indexName, CancellationToken cancellationToken = default);
    Task<bool> ReorganizeIndexAsync(string tableName, string indexName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for cache management.
/// "My worm went in my mouth and then I ate it!" - Cache consumes data efficiently!
/// </summary>
public interface ICacheManagementService
{
    // Cache Statistics
    Task<CacheMetricsDto> GetCacheMetricsAsync(CancellationToken cancellationToken = default);

    // Cache Warmup
    Task<List<CacheWarmupConfigDto>> GetWarmupConfigsAsync(CancellationToken cancellationToken = default);
    Task AddWarmupConfigAsync(CacheWarmupConfigDto config, CancellationToken cancellationToken = default);
    Task RemoveWarmupConfigAsync(string cacheKey, CancellationToken cancellationToken = default);
    Task WarmupCacheAsync(string? specificKey = null, CancellationToken cancellationToken = default);

    // Cache Invalidation
    Task InvalidateCacheAsync(string key, CancellationToken cancellationToken = default);
    Task InvalidateCacheByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    Task InvalidateCacheByTagAsync(string tag, CancellationToken cancellationToken = default);
    Task ClearAllCacheAsync(CancellationToken cancellationToken = default);

    // Cache Analysis
    Task<Dictionary<string, long>> GetCacheKeyDistributionAsync(CancellationToken cancellationToken = default);
    Task<List<string>> GetLargestCacheEntriesAsync(int count = 20, CancellationToken cancellationToken = default);
    Task<List<string>> GetMostAccessedKeysAsync(int count = 20, CancellationToken cancellationToken = default);
}

/// <summary>
/// Middleware for tracking request performance.
/// "I bent my Wookie!" - Bending time to measure performance!
/// </summary>
public interface IRequestPerformanceTracker
{
    IDisposable BeginRequest(string endpoint, string method);
    void EndRequest(int statusCode);
}

/// <summary>
/// Query performance interceptor for EF Core.
/// </summary>
public interface IQueryPerformanceInterceptor
{
    void OnQueryExecuting(string queryText, string queryType, object? parameters);
    void OnQueryExecuted(string queryText, string queryType, double durationMs, int rowsAffected);
}
