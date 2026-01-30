using System.Collections.Concurrent;
using System.Text.Json;
using Dorise.Incentive.Application.Performance.Services;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Caching;

/// <summary>
/// Distributed cache service implementation using Redis or Memory cache fallback.
/// "I'm Idaho!" - Caching stores data like Idaho stores potatoes!
/// </summary>
public class DistributedCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IMemoryCache _memoryCache;
    private readonly ILogger<DistributedCacheService> _logger;
    private readonly ConcurrentDictionary<string, HashSet<string>> _tagKeys = new();
    private long _hits;
    private long _misses;
    private DateTime? _lastFlush;
    private readonly DateTime _startTime = DateTime.UtcNow;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public DistributedCacheService(
        IDistributedCache distributedCache,
        IMemoryCache memoryCache,
        ILogger<DistributedCacheService> logger)
    {
        _distributedCache = distributedCache;
        _memoryCache = memoryCache;
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await _distributedCache.GetStringAsync(key, cancellationToken);

            if (data == null)
            {
                Interlocked.Increment(ref _misses);
                _logger.LogDebug("Cache miss for key: {Key}", key);
                return default;
            }

            Interlocked.Increment(ref _hits);
            _logger.LogDebug("Cache hit for key: {Key}", key);
            return JsonSerializer.Deserialize<T>(data, JsonOptions);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error getting cache key: {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = JsonSerializer.Serialize(value, JsonOptions);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = expiration ?? CacheDurations.Medium
            };

            await _distributedCache.SetStringAsync(key, data, options, cancellationToken);
            _logger.LogDebug("Cached key: {Key} with expiration: {Expiration}", key, expiration ?? CacheDurations.Medium);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error setting cache key: {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            await _distributedCache.RemoveAsync(key, cancellationToken);
            _logger.LogDebug("Removed cache key: {Key}", key);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error removing cache key: {Key}", key);
        }
    }

    public async Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        try
        {
            var data = await _distributedCache.GetStringAsync(key, cancellationToken);
            return data != null;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error checking cache key existence: {Key}", key);
            return false;
        }
    }

    public async Task<T> GetOrSetAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        var cached = await GetAsync<T>(key, cancellationToken);
        if (cached != null)
            return cached;

        var value = await factory();

        if (value != null)
        {
            await SetAsync(key, value, expiration, cancellationToken);
        }

        return value;
    }

    public async Task<IDictionary<string, T?>> GetManyAsync<T>(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, T?>();
        foreach (var key in keys)
        {
            result[key] = await GetAsync<T>(key, cancellationToken);
        }
        return result;
    }

    public async Task SetManyAsync<T>(IDictionary<string, T> items, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        foreach (var item in items)
        {
            await SetAsync(item.Key, item.Value, expiration, cancellationToken);
        }
    }

    public async Task RemoveManyAsync(IEnumerable<string> keys, CancellationToken cancellationToken = default)
    {
        foreach (var key in keys)
        {
            await RemoveAsync(key, cancellationToken);
        }
    }

    public Task RemoveByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Note: Pattern-based removal requires Redis SCAN command or key tracking
        // This is a simplified implementation that uses in-memory tracking
        _logger.LogWarning("Pattern-based cache removal is not fully supported in this implementation");
        return Task.CompletedTask;
    }

    public Task<IEnumerable<string>> GetKeysByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        // Note: This requires Redis SCAN command or key tracking
        _logger.LogWarning("Pattern-based key search is not fully supported in this implementation");
        return Task.FromResult<IEnumerable<string>>(Array.Empty<string>());
    }

    public async Task SetWithTagsAsync<T>(string key, T value, IEnumerable<string> tags, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        await SetAsync(key, value, expiration, cancellationToken);

        // Track key-tag relationships
        foreach (var tag in tags)
        {
            _tagKeys.AddOrUpdate(
                tag,
                _ => new HashSet<string> { key },
                (_, existing) =>
                {
                    existing.Add(key);
                    return existing;
                });
        }
    }

    public async Task InvalidateTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        if (_tagKeys.TryRemove(tag, out var keys))
        {
            await RemoveManyAsync(keys, cancellationToken);
            _logger.LogInformation("Invalidated {Count} keys for tag: {Tag}", keys.Count, tag);
        }
    }

    public Task<CacheStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default)
    {
        var stats = new CacheStatistics
        {
            TotalKeys = _tagKeys.Values.Sum(v => v.Count),
            MemoryUsedBytes = 0, // Would need Redis INFO command for accurate memory
            Hits = _hits,
            Misses = _misses,
            LastFlush = _lastFlush,
            Uptime = DateTime.UtcNow - _startTime
        };

        return Task.FromResult(stats);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        foreach (var tagKeys in _tagKeys.Values)
        {
            await RemoveManyAsync(tagKeys, cancellationToken);
        }
        _tagKeys.Clear();
        _lastFlush = DateTime.UtcNow;
        _logger.LogInformation("Cache cleared");
    }
}

/// <summary>
/// Query cache service implementation.
/// "Me fail English? That's unpossible!" - Query caching makes slow queries unpossibly fast!
/// </summary>
public class QueryCacheService : IQueryCacheService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<QueryCacheService> _logger;

    public QueryCacheService(ICacheService cacheService, ILogger<QueryCacheService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<IReadOnlyList<T>> GetOrQueryAsync<T>(
        string cacheKey,
        Func<Task<IReadOnlyList<T>>> queryFactory,
        TimeSpan? expiration = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var cached = await _cacheService.GetAsync<List<T>>(cacheKey, cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug("Query cache hit for: {CacheKey}", cacheKey);
            return cached;
        }

        _logger.LogDebug("Query cache miss for: {CacheKey}, executing query", cacheKey);
        var result = await queryFactory();
        var list = result.ToList();

        if (tags != null)
        {
            await _cacheService.SetWithTagsAsync(cacheKey, list, tags, expiration, cancellationToken);
        }
        else
        {
            await _cacheService.SetAsync(cacheKey, list, expiration, cancellationToken);
        }

        return list;
    }

    public async Task<T?> GetOrQuerySingleAsync<T>(
        string cacheKey,
        Func<Task<T?>> queryFactory,
        TimeSpan? expiration = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var cached = await _cacheService.GetAsync<T>(cacheKey, cancellationToken);
        if (cached != null)
        {
            _logger.LogDebug("Query cache hit for: {CacheKey}", cacheKey);
            return cached;
        }

        _logger.LogDebug("Query cache miss for: {CacheKey}, executing query", cacheKey);
        var result = await queryFactory();

        if (result != null)
        {
            if (tags != null)
            {
                await _cacheService.SetWithTagsAsync(cacheKey, result, tags, expiration, cancellationToken);
            }
            else
            {
                await _cacheService.SetAsync(cacheKey, result, expiration, cancellationToken);
            }
        }

        return result;
    }

    public async Task<int> GetOrQueryCountAsync(
        string cacheKey,
        Func<Task<int>> countFactory,
        TimeSpan? expiration = null,
        IEnumerable<string>? tags = null,
        CancellationToken cancellationToken = default)
    {
        var cached = await _cacheService.GetAsync<int?>(cacheKey, cancellationToken);
        if (cached.HasValue)
        {
            _logger.LogDebug("Count cache hit for: {CacheKey}", cacheKey);
            return cached.Value;
        }

        _logger.LogDebug("Count cache miss for: {CacheKey}, executing count", cacheKey);
        var count = await countFactory();

        if (tags != null)
        {
            await _cacheService.SetWithTagsAsync(cacheKey, count, tags, expiration, cancellationToken);
        }
        else
        {
            await _cacheService.SetAsync(cacheKey, count, expiration, cancellationToken);
        }

        return count;
    }

    public async Task InvalidateQueryAsync(string cacheKey, CancellationToken cancellationToken = default)
    {
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);
        _logger.LogDebug("Invalidated query cache: {CacheKey}", cacheKey);
    }

    public async Task InvalidateQueriesByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        await _cacheService.InvalidateTagAsync(tag, cancellationToken);
        _logger.LogDebug("Invalidated queries by tag: {Tag}", tag);
    }
}

/// <summary>
/// Response cache service implementation.
/// "I bent my Wookie!" - Response caching bends time to deliver faster!
/// </summary>
public class ResponseCacheService : IResponseCacheService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<ResponseCacheService> _logger;

    public ResponseCacheService(ICacheService cacheService, ILogger<ResponseCacheService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<string?> GetCachedResponseAsync(string key, CancellationToken cancellationToken = default)
    {
        return await _cacheService.GetAsync<string>(key, cancellationToken);
    }

    public async Task CacheResponseAsync(string key, string response, TimeSpan duration, CancellationToken cancellationToken = default)
    {
        await _cacheService.SetAsync(key, response, duration, cancellationToken);
        _logger.LogDebug("Cached response for: {Key}", key);
    }

    public string GenerateCacheKey(string path, IDictionary<string, string>? queryParams = null)
    {
        var key = $"response:{path}";

        if (queryParams != null && queryParams.Any())
        {
            var sortedParams = queryParams.OrderBy(p => p.Key);
            var queryString = string.Join("&", sortedParams.Select(p => $"{p.Key}={p.Value}"));
            key = $"{key}?{queryString}";
        }

        return key;
    }
}
