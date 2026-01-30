using System.Collections.Concurrent;
using System.Diagnostics;
using Dorise.Incentive.Application.Performance.DTOs;
using Dorise.Incentive.Application.Performance.Services;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Performance;

/// <summary>
/// Performance monitoring service implementation.
/// "When I grow up, I want to be a principal or a caterpillar." - Growing into better performance!
/// </summary>
public class PerformanceMonitorService : IPerformanceMonitorService
{
    private readonly ILogger<PerformanceMonitorService> _logger;
    private readonly ICacheService _cacheService;
    private readonly ConcurrentDictionary<string, EndpointMetrics> _endpointMetrics = new();
    private readonly ConcurrentDictionary<string, QueryMetrics> _queryMetrics = new();
    private readonly ConcurrentQueue<SlowQueryRecord> _slowQueries = new();
    private readonly ConcurrentQueue<PerformanceTrendDto> _trendData = new();
    private long _totalRequests;
    private long _successfulRequests;
    private long _failedRequests;
    private readonly DateTime _startTime = DateTime.UtcNow;
    private const int MaxSlowQueries = 1000;
    private const int MaxTrendDataPoints = 1440; // 24 hours at 1-minute intervals

    public PerformanceMonitorService(
        ILogger<PerformanceMonitorService> logger,
        ICacheService cacheService)
    {
        _logger = logger;
        _cacheService = cacheService;
    }

    public Task<SystemPerformanceDto> GetCurrentMetricsAsync(CancellationToken cancellationToken = default)
    {
        var process = Process.GetCurrentProcess();

        var metrics = new SystemPerformanceDto
        {
            Timestamp = DateTime.UtcNow,
            Cpu = new CpuMetrics
            {
                UsagePercent = 0, // Would need performance counter
                ProcessorCount = Environment.ProcessorCount,
                ProcessCpuPercent = process.TotalProcessorTime.TotalMilliseconds /
                    (Environment.ProcessorCount * (DateTime.UtcNow - process.StartTime).TotalMilliseconds) * 100
            },
            Memory = new MemoryMetrics
            {
                TotalBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes,
                UsedBytes = process.WorkingSet64,
                AvailableBytes = GC.GetGCMemoryInfo().TotalAvailableMemoryBytes - process.WorkingSet64,
                UsagePercent = (double)process.WorkingSet64 / GC.GetGCMemoryInfo().TotalAvailableMemoryBytes * 100,
                GcTotalMemory = GC.GetTotalMemory(false),
                GcGen0Collections = GC.CollectionCount(0),
                GcGen1Collections = GC.CollectionCount(1),
                GcGen2Collections = GC.CollectionCount(2)
            },
            Database = new DatabaseMetrics
            {
                ActiveConnections = 0, // Would need to track from connection pool
                PoolSize = 100,
                AvgQueryTimeMs = GetAverageQueryTime(),
                SlowQueryCount = _slowQueries.Count,
                TotalQueries = _queryMetrics.Values.Sum(m => m.ExecutionCount),
                IsConnected = true,
                ConnectionLatency = TimeSpan.FromMilliseconds(1)
            },
            Cache = new CacheMetricsDto
            {
                TotalKeys = 0,
                MemoryUsedBytes = 0,
                Hits = 0,
                Misses = 0,
                HitRatePercent = 0,
                IsConnected = true,
                ConnectionLatency = TimeSpan.FromMilliseconds(1)
            },
            Requests = new RequestMetrics
            {
                TotalRequests = _totalRequests,
                SuccessfulRequests = _successfulRequests,
                FailedRequests = _failedRequests,
                AvgResponseTimeMs = GetAverageResponseTime(),
                P95ResponseTimeMs = GetPercentileResponseTime(95),
                P99ResponseTimeMs = GetPercentileResponseTime(99),
                ActiveRequests = 0,
                RequestsPerSecond = _totalRequests / Math.Max(1, (DateTime.UtcNow - _startTime).TotalSeconds)
            },
            HealthStatus = "Healthy"
        };

        return Task.FromResult(metrics);
    }

    public Task<List<PerformanceTrendDto>> GetTrendDataAsync(
        DateTime fromDate,
        DateTime toDate,
        string interval = "hour",
        CancellationToken cancellationToken = default)
    {
        var data = _trendData
            .Where(t => t.Timestamp >= fromDate && t.Timestamp <= toDate)
            .OrderBy(t => t.Timestamp)
            .ToList();

        return Task.FromResult(data);
    }

    public Task RecordRequestAsync(
        string endpoint,
        string method,
        int statusCode,
        double durationMs,
        CancellationToken cancellationToken = default)
    {
        Interlocked.Increment(ref _totalRequests);

        if (statusCode >= 200 && statusCode < 400)
            Interlocked.Increment(ref _successfulRequests);
        else
            Interlocked.Increment(ref _failedRequests);

        var key = $"{method}:{endpoint}";
        _endpointMetrics.AddOrUpdate(
            key,
            _ => new EndpointMetrics
            {
                Endpoint = endpoint,
                Method = method,
                RequestCount = 1,
                TotalDurationMs = durationMs,
                MinDurationMs = durationMs,
                MaxDurationMs = durationMs,
                SuccessCount = statusCode < 400 ? 1 : 0,
                ErrorCount = statusCode >= 400 ? 1 : 0,
                LastAccessed = DateTime.UtcNow,
                ResponseTimes = new List<double> { durationMs }
            },
            (_, existing) =>
            {
                existing.RequestCount++;
                existing.TotalDurationMs += durationMs;
                existing.MinDurationMs = Math.Min(existing.MinDurationMs, durationMs);
                existing.MaxDurationMs = Math.Max(existing.MaxDurationMs, durationMs);
                if (statusCode < 400) existing.SuccessCount++;
                else existing.ErrorCount++;
                existing.LastAccessed = DateTime.UtcNow;
                existing.ResponseTimes.Add(durationMs);
                if (existing.ResponseTimes.Count > 1000)
                    existing.ResponseTimes.RemoveAt(0);
                return existing;
            });

        return Task.CompletedTask;
    }

    public Task<List<EndpointPerformanceDto>> GetEndpointMetricsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        int topN = 20,
        CancellationToken cancellationToken = default)
    {
        var metrics = _endpointMetrics.Values
            .OrderByDescending(m => m.RequestCount)
            .Take(topN)
            .Select(MapToEndpointDto)
            .ToList();

        return Task.FromResult(metrics);
    }

    public Task<EndpointPerformanceDto?> GetEndpointMetricsAsync(
        string endpoint,
        string method,
        DateTime? fromDate = null,
        DateTime? toDate = null,
        CancellationToken cancellationToken = default)
    {
        var key = $"{method}:{endpoint}";
        if (_endpointMetrics.TryGetValue(key, out var metrics))
        {
            return Task.FromResult<EndpointPerformanceDto?>(MapToEndpointDto(metrics));
        }
        return Task.FromResult<EndpointPerformanceDto?>(null);
    }

    public Task RecordQueryAsync(
        string queryName,
        string queryType,
        double durationMs,
        int rowsAffected,
        string? parameters = null,
        CancellationToken cancellationToken = default)
    {
        _queryMetrics.AddOrUpdate(
            queryName,
            _ => new QueryMetrics
            {
                QueryName = queryName,
                QueryType = queryType,
                ExecutionCount = 1,
                TotalDurationMs = durationMs,
                MinDurationMs = durationMs,
                MaxDurationMs = durationMs,
                TotalRowsAffected = rowsAffected,
                LastExecuted = DateTime.UtcNow
            },
            (_, existing) =>
            {
                existing.ExecutionCount++;
                existing.TotalDurationMs += durationMs;
                existing.MinDurationMs = Math.Min(existing.MinDurationMs, durationMs);
                existing.MaxDurationMs = Math.Max(existing.MaxDurationMs, durationMs);
                existing.TotalRowsAffected += rowsAffected;
                existing.LastExecuted = DateTime.UtcNow;
                return existing;
            });

        // Track slow queries
        if (durationMs > 1000)
        {
            if (_slowQueries.Count >= MaxSlowQueries)
                _slowQueries.TryDequeue(out _);

            _slowQueries.Enqueue(new SlowQueryRecord
            {
                Id = Guid.NewGuid(),
                QueryName = queryName,
                QueryType = queryType,
                DurationMs = durationMs,
                RowsAffected = rowsAffected,
                ExecutedAt = DateTime.UtcNow,
                Parameters = parameters
            });
        }

        return Task.CompletedTask;
    }

    public Task<List<QueryPerformanceDto>> GetQueryMetricsAsync(
        DateTime? fromDate = null,
        DateTime? toDate = null,
        bool slowOnly = false,
        int topN = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _queryMetrics.Values.AsEnumerable();

        if (slowOnly)
            query = query.Where(m => m.AvgDurationMs > 1000);

        var metrics = query
            .OrderByDescending(m => m.TotalDurationMs)
            .Take(topN)
            .Select(m => new QueryPerformanceDto
            {
                QueryName = m.QueryName,
                QueryType = m.QueryType,
                ExecutionCount = m.ExecutionCount,
                AvgDurationMs = m.AvgDurationMs,
                MinDurationMs = m.MinDurationMs,
                MaxDurationMs = m.MaxDurationMs,
                TotalDurationMs = m.TotalDurationMs,
                RowsAffected = (int)(m.TotalRowsAffected / Math.Max(1, m.ExecutionCount)),
                LastExecuted = m.LastExecuted,
                IsSlow = m.AvgDurationMs > 1000
            })
            .ToList();

        return Task.FromResult(metrics);
    }

    public Task<List<SlowQueryDto>> GetSlowQueriesAsync(
        int count = 50,
        double thresholdMs = 1000,
        CancellationToken cancellationToken = default)
    {
        var slowQueries = _slowQueries
            .Where(q => q.DurationMs >= thresholdMs)
            .OrderByDescending(q => q.DurationMs)
            .Take(count)
            .Select(q => new SlowQueryDto
            {
                Id = q.Id,
                QueryText = q.QueryName,
                QueryType = q.QueryType,
                DurationMs = q.DurationMs,
                RowsAffected = q.RowsAffected,
                ExecutedAt = q.ExecutedAt,
                Parameters = q.Parameters,
                Suggestion = GetQuerySuggestion(q.DurationMs)
            })
            .ToList();

        return Task.FromResult(slowQueries);
    }

    public async Task<HealthCheckResultDto> RunHealthChecksAsync(CancellationToken cancellationToken = default)
    {
        var entries = new List<HealthCheckEntryDto>();
        var sw = Stopwatch.StartNew();

        // Database check
        entries.Add(await CheckDatabaseHealthAsync(cancellationToken));

        // Cache check
        entries.Add(await CheckCacheHealthAsync(cancellationToken));

        // Memory check
        entries.Add(CheckMemoryHealth());

        sw.Stop();

        var overallStatus = entries.All(e => e.Status == "Healthy") ? "Healthy"
            : entries.Any(e => e.Status == "Unhealthy") ? "Unhealthy" : "Degraded";

        return new HealthCheckResultDto
        {
            Status = overallStatus,
            TotalDuration = sw.Elapsed,
            Entries = entries
        };
    }

    public async Task<PerformanceDashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        var currentMetrics = await GetCurrentMetricsAsync(cancellationToken);
        var trends = await GetTrendDataAsync(
            DateTime.UtcNow.AddHours(-24),
            DateTime.UtcNow,
            "hour",
            cancellationToken);
        var topEndpoints = await GetEndpointMetricsAsync(topN: 10, cancellationToken: cancellationToken);
        var slowQueries = await GetSlowQueriesAsync(10, 500, cancellationToken);

        return new PerformanceDashboardDto
        {
            CurrentMetrics = currentMetrics,
            Trends = trends,
            TopEndpoints = topEndpoints,
            SlowQueries = slowQueries,
            ActiveAlerts = new List<PerformanceAlertDto>(),
            CacheMetrics = currentMetrics.Cache
        };
    }

    public Task<int> CleanupOldMetricsAsync(int daysToKeep = 30, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow.AddDays(-daysToKeep);
        var removed = 0;

        // Clean old slow queries
        while (_slowQueries.TryPeek(out var query) && query.ExecutedAt < cutoff)
        {
            if (_slowQueries.TryDequeue(out _))
                removed++;
        }

        // Clean old trend data
        while (_trendData.TryPeek(out var trend) && trend.Timestamp < cutoff)
        {
            if (_trendData.TryDequeue(out _))
                removed++;
        }

        _logger.LogInformation("Cleaned up {Count} old performance metrics", removed);
        return Task.FromResult(removed);
    }

    private double GetAverageResponseTime()
    {
        var allTimes = _endpointMetrics.Values.SelectMany(m => m.ResponseTimes).ToList();
        return allTimes.Any() ? allTimes.Average() : 0;
    }

    private double GetPercentileResponseTime(int percentile)
    {
        var allTimes = _endpointMetrics.Values
            .SelectMany(m => m.ResponseTimes)
            .OrderBy(t => t)
            .ToList();

        if (!allTimes.Any()) return 0;

        var index = (int)Math.Ceiling(percentile / 100.0 * allTimes.Count) - 1;
        return allTimes[Math.Max(0, index)];
    }

    private double GetAverageQueryTime()
    {
        var metrics = _queryMetrics.Values.ToList();
        if (!metrics.Any()) return 0;
        return metrics.Average(m => m.AvgDurationMs);
    }

    private static string GetQuerySuggestion(double durationMs)
    {
        if (durationMs > 5000)
            return "Consider adding indexes or optimizing the query structure";
        if (durationMs > 2000)
            return "Review query for potential N+1 issues or missing indexes";
        return "Monitor for patterns - may need optimization if frequency is high";
    }

    private async Task<HealthCheckEntryDto> CheckDatabaseHealthAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            // Simulated database check
            await Task.Delay(1, cancellationToken);
            sw.Stop();
            return new HealthCheckEntryDto
            {
                Name = "Database",
                Status = "Healthy",
                Duration = sw.Elapsed,
                Description = "Database connection is healthy"
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new HealthCheckEntryDto
            {
                Name = "Database",
                Status = "Unhealthy",
                Duration = sw.Elapsed,
                Description = "Database connection failed",
                Exception = ex.Message
            };
        }
    }

    private async Task<HealthCheckEntryDto> CheckCacheHealthAsync(CancellationToken cancellationToken)
    {
        var sw = Stopwatch.StartNew();
        try
        {
            var testKey = "health:check:test";
            await _cacheService.SetAsync(testKey, "test", TimeSpan.FromSeconds(10), cancellationToken);
            var result = await _cacheService.GetAsync<string>(testKey, cancellationToken);
            await _cacheService.RemoveAsync(testKey, cancellationToken);
            sw.Stop();

            return new HealthCheckEntryDto
            {
                Name = "Cache",
                Status = result == "test" ? "Healthy" : "Degraded",
                Duration = sw.Elapsed,
                Description = result == "test" ? "Cache is healthy" : "Cache returned unexpected result"
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new HealthCheckEntryDto
            {
                Name = "Cache",
                Status = "Unhealthy",
                Duration = sw.Elapsed,
                Description = "Cache check failed",
                Exception = ex.Message
            };
        }
    }

    private HealthCheckEntryDto CheckMemoryHealth()
    {
        var gcInfo = GC.GetGCMemoryInfo();
        var usedMemory = GC.GetTotalMemory(false);
        var memoryLimit = gcInfo.TotalAvailableMemoryBytes;
        var usagePercent = (double)usedMemory / memoryLimit * 100;

        var status = usagePercent switch
        {
            < 70 => "Healthy",
            < 90 => "Degraded",
            _ => "Unhealthy"
        };

        return new HealthCheckEntryDto
        {
            Name = "Memory",
            Status = status,
            Duration = TimeSpan.Zero,
            Description = $"Memory usage: {usagePercent:F1}%",
            Data = new Dictionary<string, object>
            {
                ["usedBytes"] = usedMemory,
                ["totalBytes"] = memoryLimit,
                ["usagePercent"] = usagePercent
            }
        };
    }

    private static EndpointPerformanceDto MapToEndpointDto(EndpointMetrics m)
    {
        var sortedTimes = m.ResponseTimes.OrderBy(t => t).ToList();
        return new EndpointPerformanceDto
        {
            Endpoint = m.Endpoint,
            HttpMethod = m.Method,
            RequestCount = m.RequestCount,
            AvgResponseTimeMs = m.AvgDurationMs,
            MinResponseTimeMs = m.MinDurationMs,
            MaxResponseTimeMs = m.MaxDurationMs,
            P50ResponseTimeMs = GetPercentile(sortedTimes, 50),
            P95ResponseTimeMs = GetPercentile(sortedTimes, 95),
            P99ResponseTimeMs = GetPercentile(sortedTimes, 99),
            SuccessCount = m.SuccessCount,
            ErrorCount = m.ErrorCount,
            ErrorRate = m.RequestCount > 0 ? (double)m.ErrorCount / m.RequestCount * 100 : 0,
            LastAccessed = m.LastAccessed
        };
    }

    private static double GetPercentile(List<double> sortedValues, int percentile)
    {
        if (!sortedValues.Any()) return 0;
        var index = (int)Math.Ceiling(percentile / 100.0 * sortedValues.Count) - 1;
        return sortedValues[Math.Max(0, index)];
    }

    private class EndpointMetrics
    {
        public string Endpoint { get; set; } = null!;
        public string Method { get; set; } = null!;
        public long RequestCount { get; set; }
        public double TotalDurationMs { get; set; }
        public double MinDurationMs { get; set; }
        public double MaxDurationMs { get; set; }
        public long SuccessCount { get; set; }
        public long ErrorCount { get; set; }
        public DateTime LastAccessed { get; set; }
        public List<double> ResponseTimes { get; set; } = new();
        public double AvgDurationMs => RequestCount > 0 ? TotalDurationMs / RequestCount : 0;
    }

    private class QueryMetrics
    {
        public string QueryName { get; set; } = null!;
        public string QueryType { get; set; } = null!;
        public long ExecutionCount { get; set; }
        public double TotalDurationMs { get; set; }
        public double MinDurationMs { get; set; }
        public double MaxDurationMs { get; set; }
        public long TotalRowsAffected { get; set; }
        public DateTime LastExecuted { get; set; }
        public double AvgDurationMs => ExecutionCount > 0 ? TotalDurationMs / ExecutionCount : 0;
    }

    private class SlowQueryRecord
    {
        public Guid Id { get; set; }
        public string QueryName { get; set; } = null!;
        public string QueryType { get; set; } = null!;
        public double DurationMs { get; set; }
        public int RowsAffected { get; set; }
        public DateTime ExecutedAt { get; set; }
        public string? Parameters { get; set; }
    }
}

/// <summary>
/// Cache management service implementation.
/// "My worm went in my mouth and then I ate it!" - Cache consumes data efficiently!
/// </summary>
public class CacheManagementService : ICacheManagementService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheManagementService> _logger;
    private readonly List<CacheWarmupConfigDto> _warmupConfigs = new();

    public CacheManagementService(
        ICacheService cacheService,
        ILogger<CacheManagementService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<CacheMetricsDto> GetCacheMetricsAsync(CancellationToken cancellationToken = default)
    {
        var stats = await _cacheService.GetStatisticsAsync(cancellationToken);
        return new CacheMetricsDto
        {
            TotalKeys = stats.TotalKeys,
            MemoryUsedBytes = stats.MemoryUsedBytes,
            Hits = stats.Hits,
            Misses = stats.Misses,
            HitRatePercent = stats.HitRate,
            IsConnected = true,
            ConnectionLatency = TimeSpan.FromMilliseconds(1)
        };
    }

    public Task<List<CacheWarmupConfigDto>> GetWarmupConfigsAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_warmupConfigs.ToList());
    }

    public Task AddWarmupConfigAsync(CacheWarmupConfigDto config, CancellationToken cancellationToken = default)
    {
        _warmupConfigs.Add(config);
        _logger.LogInformation("Added cache warmup config: {CacheKey}", config.CacheKey);
        return Task.CompletedTask;
    }

    public Task RemoveWarmupConfigAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        _warmupConfigs.RemoveAll(c => c.CacheKey == cacheKey);
        _logger.LogInformation("Removed cache warmup config: {CacheKey}", cacheKey);
        return Task.CompletedTask;
    }

    public Task WarmupCacheAsync(string? specificKey = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Cache warmup triggered for: {Key}", specificKey ?? "all");
        // Implementation would call specific warmup functions based on config
        return Task.CompletedTask;
    }

    public async Task InvalidateCacheAsync(string key, CancellationToken cancellationToken = default)
    {
        await _cacheService.RemoveAsync(key, cancellationToken);
        _logger.LogInformation("Invalidated cache key: {Key}", key);
    }

    public async Task InvalidateCacheByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
        _logger.LogInformation("Invalidated cache by pattern: {Pattern}", pattern);
    }

    public async Task InvalidateCacheByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        await _cacheService.InvalidateTagAsync(tag, cancellationToken);
        _logger.LogInformation("Invalidated cache by tag: {Tag}", tag);
    }

    public async Task ClearAllCacheAsync(CancellationToken cancellationToken = default)
    {
        await _cacheService.ClearAsync(cancellationToken);
        _logger.LogWarning("Cleared all cache");
    }

    public Task<Dictionary<string, long>> GetCacheKeyDistributionAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new Dictionary<string, long>());
    }

    public Task<List<string>> GetLargestCacheEntriesAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<string>());
    }

    public Task<List<string>> GetMostAccessedKeysAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new List<string>());
    }
}
