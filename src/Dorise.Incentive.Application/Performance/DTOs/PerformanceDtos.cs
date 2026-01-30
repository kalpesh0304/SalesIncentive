namespace Dorise.Incentive.Application.Performance.DTOs;

/// <summary>
/// System performance metrics.
/// "I'm learnding!" - Learning about system performance!
/// </summary>
public record SystemPerformanceDto
{
    public DateTime Timestamp { get; init; }
    public CpuMetrics Cpu { get; init; } = new();
    public MemoryMetrics Memory { get; init; } = new();
    public DatabaseMetrics Database { get; init; } = new();
    public CacheMetricsDto Cache { get; init; } = new();
    public RequestMetrics Requests { get; init; } = new();
    public string HealthStatus { get; init; } = "Healthy";
}

public record CpuMetrics
{
    public double UsagePercent { get; init; }
    public int ProcessorCount { get; init; }
    public double ProcessCpuPercent { get; init; }
}

public record MemoryMetrics
{
    public long TotalBytes { get; init; }
    public long UsedBytes { get; init; }
    public long AvailableBytes { get; init; }
    public double UsagePercent { get; init; }
    public long GcTotalMemory { get; init; }
    public int GcGen0Collections { get; init; }
    public int GcGen1Collections { get; init; }
    public int GcGen2Collections { get; init; }
}

public record DatabaseMetrics
{
    public int ActiveConnections { get; init; }
    public int PoolSize { get; init; }
    public double AvgQueryTimeMs { get; init; }
    public int SlowQueryCount { get; init; }
    public long TotalQueries { get; init; }
    public bool IsConnected { get; init; }
    public TimeSpan ConnectionLatency { get; init; }
}

public record CacheMetricsDto
{
    public long TotalKeys { get; init; }
    public long MemoryUsedBytes { get; init; }
    public long Hits { get; init; }
    public long Misses { get; init; }
    public double HitRatePercent { get; init; }
    public bool IsConnected { get; init; }
    public TimeSpan ConnectionLatency { get; init; }
}

public record RequestMetrics
{
    public long TotalRequests { get; init; }
    public long SuccessfulRequests { get; init; }
    public long FailedRequests { get; init; }
    public double AvgResponseTimeMs { get; init; }
    public double P95ResponseTimeMs { get; init; }
    public double P99ResponseTimeMs { get; init; }
    public int ActiveRequests { get; init; }
    public double RequestsPerSecond { get; init; }
}

/// <summary>
/// Query performance metrics.
/// </summary>
public record QueryPerformanceDto
{
    public string QueryName { get; init; } = null!;
    public string QueryType { get; init; } = null!;
    public long ExecutionCount { get; init; }
    public double AvgDurationMs { get; init; }
    public double MinDurationMs { get; init; }
    public double MaxDurationMs { get; init; }
    public double TotalDurationMs { get; init; }
    public int RowsAffected { get; init; }
    public DateTime LastExecuted { get; init; }
    public bool IsSlow { get; init; }
}

/// <summary>
/// Slow query details.
/// </summary>
public record SlowQueryDto
{
    public Guid Id { get; init; }
    public string QueryText { get; init; } = null!;
    public string QueryType { get; init; } = null!;
    public double DurationMs { get; init; }
    public int RowsAffected { get; init; }
    public DateTime ExecutedAt { get; init; }
    public string? Parameters { get; init; }
    public string? StackTrace { get; init; }
    public string? Suggestion { get; init; }
}

/// <summary>
/// Endpoint performance metrics.
/// </summary>
public record EndpointPerformanceDto
{
    public string Endpoint { get; init; } = null!;
    public string HttpMethod { get; init; } = null!;
    public long RequestCount { get; init; }
    public double AvgResponseTimeMs { get; init; }
    public double MinResponseTimeMs { get; init; }
    public double MaxResponseTimeMs { get; init; }
    public double P50ResponseTimeMs { get; init; }
    public double P95ResponseTimeMs { get; init; }
    public double P99ResponseTimeMs { get; init; }
    public long SuccessCount { get; init; }
    public long ErrorCount { get; init; }
    public double ErrorRate { get; init; }
    public DateTime LastAccessed { get; init; }
}

/// <summary>
/// Performance trend data.
/// </summary>
public record PerformanceTrendDto
{
    public DateTime Timestamp { get; init; }
    public double CpuPercent { get; init; }
    public double MemoryPercent { get; init; }
    public double AvgResponseTimeMs { get; init; }
    public double RequestsPerSecond { get; init; }
    public double ErrorRate { get; init; }
    public double CacheHitRate { get; init; }
}

/// <summary>
/// Health check result.
/// </summary>
public record HealthCheckResultDto
{
    public string Status { get; init; } = "Healthy";
    public TimeSpan TotalDuration { get; init; }
    public List<HealthCheckEntryDto> Entries { get; init; } = new();
}

public record HealthCheckEntryDto
{
    public string Name { get; init; } = null!;
    public string Status { get; init; } = null!;
    public TimeSpan Duration { get; init; }
    public string? Description { get; init; }
    public Dictionary<string, object>? Data { get; init; }
    public string? Exception { get; init; }
}

/// <summary>
/// Performance alert configuration.
/// </summary>
public record PerformanceAlertConfigDto
{
    public Guid Id { get; init; }
    public string AlertName { get; init; } = null!;
    public string MetricType { get; init; } = null!;
    public double ThresholdValue { get; init; }
    public string ComparisonOperator { get; init; } = null!;
    public int DurationMinutes { get; init; }
    public bool IsEnabled { get; init; }
    public string? NotificationChannels { get; init; }
}

/// <summary>
/// Performance alert event.
/// </summary>
public record PerformanceAlertDto
{
    public Guid Id { get; init; }
    public string AlertName { get; init; } = null!;
    public string MetricType { get; init; } = null!;
    public double CurrentValue { get; init; }
    public double ThresholdValue { get; init; }
    public string Severity { get; init; } = null!;
    public DateTime TriggeredAt { get; init; }
    public DateTime? ResolvedAt { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// Database index analysis.
/// </summary>
public record IndexAnalysisDto
{
    public string TableName { get; init; } = null!;
    public string IndexName { get; init; } = null!;
    public string IndexType { get; init; } = null!;
    public long SizeBytes { get; init; }
    public long Reads { get; init; }
    public long Writes { get; init; }
    public double FragmentationPercent { get; init; }
    public bool IsUnused { get; init; }
    public string? Recommendation { get; init; }
}

/// <summary>
/// Query optimization suggestion.
/// </summary>
public record QueryOptimizationSuggestionDto
{
    public string QueryPattern { get; init; } = null!;
    public string IssueType { get; init; } = null!;
    public string Severity { get; init; } = null!;
    public string Description { get; init; } = null!;
    public string Suggestion { get; init; } = null!;
    public double EstimatedImprovement { get; init; }
    public string? ExampleQuery { get; init; }
}

/// <summary>
/// Cache warm-up configuration.
/// </summary>
public record CacheWarmupConfigDto
{
    public string CacheKey { get; init; } = null!;
    public string Description { get; init; } = null!;
    public int Priority { get; init; }
    public bool IsEnabled { get; init; }
    public TimeSpan? RefreshInterval { get; init; }
    public DateTime? LastWarmedAt { get; init; }
}

/// <summary>
/// Performance dashboard summary.
/// </summary>
public record PerformanceDashboardDto
{
    public SystemPerformanceDto CurrentMetrics { get; init; } = new();
    public List<PerformanceTrendDto> Trends { get; init; } = new();
    public List<EndpointPerformanceDto> TopEndpoints { get; init; } = new();
    public List<SlowQueryDto> SlowQueries { get; init; } = new();
    public List<PerformanceAlertDto> ActiveAlerts { get; init; } = new();
    public CacheMetricsDto CacheMetrics { get; init; } = new();
}

/// <summary>
/// Performance report request.
/// </summary>
public record PerformanceReportRequest
{
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }
    public string? MetricTypes { get; init; }
    public string? Endpoints { get; init; }
    public bool IncludeSlowQueries { get; init; } = true;
    public bool IncludeCacheMetrics { get; init; } = true;
    public bool IncludeAlerts { get; init; } = true;
    public string? AggregationInterval { get; init; } = "hour";
}
