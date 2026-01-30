namespace Dorise.Incentive.Application.Performance.Services;

/// <summary>
/// Distributed caching service interface.
/// "I'm Idaho!" - Caching stores data like Idaho stores potatoes!
/// </summary>
public interface ICacheService
{
    // Basic Operations
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default);

    // Get or Set Pattern
    Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default);

    // Bulk Operations
    Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default);
    Task SetManyAsync<T>(IDictionary<string, T> items, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default);

    // Pattern-based Operations
    Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default);
    Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern, CancellationToken cancellationToken = default);

    // Cache Tags/Regions
    Task SetWithTagsAsync<T>(string key, T value, IEnumerable<string> tags, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task InvalidateTagAsync(string tag, CancellationToken cancellationToken = default);

    // Statistics
    Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
    Task ClearAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Cache statistics.
/// </summary>
public record CacheStatistics
{
    public long TotalKeys { get; init; }
    public long MemoryUsedBytes { get; init; }
    public long Hits { get; init; }
    public long Misses { get; init; }
    public double HitRate => Hits + Misses > 0 ? (double)Hits / (Hits + Misses) * 100 : 0;
    public DateTime? LastFlush { get; init; }
    public TimeSpan Uptime { get; init; }
}

/// <summary>
/// Cache key builder for consistent key generation.
/// "The doctor said I wouldn't have so many nosebleeds if I kept my finger outta there."
/// - Consistent keys prevent cache collisions!
/// </summary>
public static class CacheKeys
{
    private const string Separator = ":";
    private const string Prefix = "dsif";

    // Employee Keys
    public static string Employee(Guid id) => Build("employee", id.ToString());
    public static string EmployeeByNumber(string employeeNumber) => Build("employee", "num", employeeNumber);
    public static string EmployeeList(string? filter = null) => Build("employees", filter ?? "all");
    public static string EmployeeCount() => Build("employees", "count");

    // Department Keys
    public static string Department(Guid id) => Build("department", id.ToString());
    public static string DepartmentList() => Build("departments", "all");
    public static string DepartmentHierarchy() => Build("departments", "hierarchy");

    // Plan Keys
    public static string IncentivePlan(Guid id) => Build("plan", id.ToString());
    public static string IncentivePlanList(bool activeOnly = false) => Build("plans", activeOnly ? "active" : "all");
    public static string PlanSlabs(Guid planId) => Build("plan", planId.ToString(), "slabs");

    // Calculation Keys
    public static string Calculation(Guid id) => Build("calculation", id.ToString());
    public static string CalculationsByPeriod(string period) => Build("calculations", "period", period);
    public static string CalculationSummary(string period) => Build("calculations", "summary", period);

    // Configuration Keys
    public static string SystemConfig(string key) => Build("config", key);
    public static string AllSystemConfigs() => Build("config", "all");
    public static string FeatureFlag(string key) => Build("feature", key);
    public static string AllFeatureFlags() => Build("features", "all");

    // User/Role Keys
    public static string UserPermissions(Guid userId) => Build("user", userId.ToString(), "permissions");
    public static string UserRoles(Guid userId) => Build("user", userId.ToString(), "roles");
    public static string Role(Guid id) => Build("role", id.ToString());

    // Dashboard Keys
    public static string DashboardSummary(string period) => Build("dashboard", "summary", period);
    public static string DashboardCharts(string period) => Build("dashboard", "charts", period);

    // Report Keys
    public static string Report(string reportType, string period) => Build("report", reportType, period);

    // Tags for bulk invalidation
    public static class Tags
    {
        public const string Employees = "tag:employees";
        public const string Departments = "tag:departments";
        public const string Plans = "tag:plans";
        public const string Calculations = "tag:calculations";
        public const string Configuration = "tag:config";
        public const string Security = "tag:security";
        public const string Dashboard = "tag:dashboard";
        public const string Reports = "tag:reports";
    }

    private static string Build(params string[] parts)
    {
        return string.Join(Separator, new[] { Prefix }.Concat(parts.Where(p => !string.IsNullOrEmpty(p))));
    }
}

/// <summary>
/// Cache duration presets.
/// </summary>
public static class CacheDurations
{
    public static readonly TimeSpan VeryShort = TimeSpan.FromMinutes(1);
    public static readonly TimeSpan Short = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan Medium = TimeSpan.FromMinutes(15);
    public static readonly TimeSpan Long = TimeSpan.FromHours(1);
    public static readonly TimeSpan VeryLong = TimeSpan.FromHours(4);
    public static readonly TimeSpan Day = TimeSpan.FromDays(1);

    // Specific durations
    public static readonly TimeSpan Configuration = TimeSpan.FromHours(1);
    public static readonly TimeSpan FeatureFlags = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan UserPermissions = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan Dashboard = TimeSpan.FromMinutes(5);
    public static readonly TimeSpan Reports = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan EntityList = TimeSpan.FromMinutes(10);
    public static readonly TimeSpan SingleEntity = TimeSpan.FromMinutes(15);
}

/// <summary>
/// Query caching service for EF Core queries.
/// "Me fail English? That's unpossible!" - Query caching makes slow queries unpossibly fast!
/// </summary>
public interface IQueryCacheService
{
    Task<IReadOnlyList<T>> GetOrQueryAsync<T>(
        string cacheKey,
        Func<Task<IReadOnlyList<T>>> queryFactory,
        TimeSpan? expiration = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    Task<T?> GetOrQuerySingleAsync<T>(
        string cacheKey,
        Func<Task<T?>> queryFactory,
        TimeSpan? expiration = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    Task<int> GetOrQueryCountAsync(
        string cacheKey,
        Func<Task<int>> countFactory,
        TimeSpan? expiration = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default);

    Task InvalidateQueryAsync(string cacheKey, CancellationToken cancellationToken = default);
    Task InvalidateQueriesByTagAsync(string tag, CancellationToken cancellationToken = default);
}

/// <summary>
/// Response caching service for API responses.
/// "I bent my Wookie!" - Response caching bends time to deliver faster!
/// </summary>
public interface IResponseCacheService
{
    Task<string?> GetCachedResponseAsync(string key, CancellationToken cancellationToken = default);
    Task CacheResponseAsync(string key, string response, TimeSpan duration, CancellationToken cancellationToken = default);
    string GenerateCacheKey(string path, IDictionary<string, string>? queryParams = null);
}
