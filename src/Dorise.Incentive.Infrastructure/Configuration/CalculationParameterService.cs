using Dorise.Incentive.Application.Configuration.DTOs;
using Dorise.Incentive.Application.Configuration.Services;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Configuration;

/// <summary>
/// Implementation of calculation parameter service.
/// "Hi, I'm Ralph!" - Hi, I'm your calculation parameter helper!
/// </summary>
public class CalculationParameterService : ICalculationParameterService
{
    private readonly ICalculationParameterRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<CalculationParameterService> _logger;

    private const string CacheKeyPrefix = "param:";
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(30);

    public CalculationParameterService(
        ICalculationParameterRepository repository,
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<CalculationParameterService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<decimal?> GetParameterValueAsync(
        string parameterName,
        ParameterScope scope = ParameterScope.Global,
        Guid? scopeId = null,
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default)
    {
        var cacheKey = BuildCacheKey(parameterName, scope, scopeId, asOfDate);

        if (_cache.TryGetValue(cacheKey, out decimal? cachedValue))
        {
            return cachedValue;
        }

        var param = await _repository.GetEffectiveParameterAsync(
            parameterName, scope, scopeId, asOfDate ?? DateTime.UtcNow, cancellationToken);

        if (param == null)
        {
            return null;
        }

        _cache.Set(cacheKey, (decimal?)param.Value, DefaultCacheDuration);
        return param.Value;
    }

    public async Task<decimal> GetParameterValueOrDefaultAsync(
        string parameterName,
        decimal defaultValue,
        ParameterScope scope = ParameterScope.Global,
        Guid? scopeId = null,
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default)
    {
        var value = await GetParameterValueAsync(
            parameterName, scope, scopeId, asOfDate, cancellationToken);

        return value ?? defaultValue;
    }

    public async Task<CalculationParameterDto?> GetParameterAsync(
        string parameterName,
        ParameterScope scope = ParameterScope.Global,
        Guid? scopeId = null,
        CancellationToken cancellationToken = default)
    {
        var param = await _repository.GetByNameAndScopeAsync(
            parameterName, scope, scopeId, cancellationToken);

        return param == null ? null : MapToDto(param);
    }

    public async Task<IReadOnlyList<CalculationParameterDto>> GetAllParametersAsync(
        CancellationToken cancellationToken = default)
    {
        var parameters = await _repository.GetAllAsync(cancellationToken);
        return parameters.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<CalculationParameterDto>> SearchParametersAsync(
        CalculationParameterSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        // Search using the repository's date-based filtering
        // When IsEffective is specified, we use AsOfDate as the effective date range
        var effectiveFrom = query.IsEffective == true ? query.AsOfDate : null;
        var effectiveTo = query.IsEffective == true ? query.AsOfDate : null;

        var parameters = await _repository.SearchAsync(
            query.ParameterName,
            query.Scope,
            query.ScopeId,
            effectiveFrom,
            effectiveTo,
            cancellationToken);

        // If IsEffective filter is specified, further filter results
        if (query.IsEffective.HasValue)
        {
            var asOfDate = query.AsOfDate ?? DateTime.UtcNow;
            parameters = parameters
                .Where(p => p.IsEffective(asOfDate) == query.IsEffective.Value)
                .ToList();
        }

        return parameters.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<CalculationParameterDto>> GetEffectiveParametersAsync(
        ParameterScope scope,
        Guid? scopeId = null,
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = await _repository.GetEffectiveForScopeAsync(
            scope, scopeId, asOfDate ?? DateTime.UtcNow, cancellationToken);

        return parameters.Select(MapToDto).ToList();
    }

    public async Task<CalculationParameterDto> CreateParameterAsync(
        CreateCalculationParameterRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        // Check for overlapping parameter
        var existing = await _repository.GetByNameAndScopeAsync(
            request.ParameterName, request.Scope, request.ScopeId, cancellationToken);

        if (existing != null && existing.IsEffective(request.EffectiveFrom))
        {
            throw new InvalidOperationException(
                $"Parameter '{request.ParameterName}' already exists for this scope with overlapping effective period");
        }

        var param = CalculationParameter.Create(
            request.ParameterName,
            request.Value,
            request.Scope,
            request.ScopeId,
            request.EffectiveFrom,
            request.Description);

        if (request.EffectiveTo.HasValue)
        {
            param.SetEffectivePeriod(request.EffectiveFrom ?? DateTime.UtcNow, request.EffectiveTo);
        }

        await _repository.AddAsync(param, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        InvalidateCache(request.ParameterName, request.Scope, request.ScopeId);

        _logger.LogInformation(
            "Calculation parameter {ParameterName} created for scope {Scope} by {CreatedBy}",
            request.ParameterName, request.Scope, createdBy);

        return MapToDto(param);
    }

    public async Task<CalculationParameterDto> UpdateParameterAsync(
        Guid parameterId,
        UpdateCalculationParameterRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var param = await _repository.GetByIdAsync(parameterId, cancellationToken)
            ?? throw new InvalidOperationException($"Calculation parameter {parameterId} not found");

        param.UpdateValue(request.Value);

        if (request.EffectiveFrom.HasValue)
        {
            param.SetEffectivePeriod(request.EffectiveFrom.Value, request.EffectiveTo);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        InvalidateCache(param.ParameterName, param.Scope, param.ScopeId);

        _logger.LogInformation(
            "Calculation parameter {ParameterId} updated by {ModifiedBy}",
            parameterId, modifiedBy);

        return MapToDto(param);
    }

    public async Task DeleteParameterAsync(
        Guid parameterId,
        string deletedBy,
        CancellationToken cancellationToken = default)
    {
        var param = await _repository.GetByIdAsync(parameterId, cancellationToken)
            ?? throw new InvalidOperationException($"Calculation parameter {parameterId} not found");

        await _repository.DeleteAsync(param, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        InvalidateCache(param.ParameterName, param.Scope, param.ScopeId);

        _logger.LogInformation(
            "Calculation parameter {ParameterId} deleted by {DeletedBy}",
            parameterId, deletedBy);
    }

    private static string BuildCacheKey(
        string parameterName,
        ParameterScope scope,
        Guid? scopeId,
        DateTime? asOfDate)
    {
        var dateKey = asOfDate?.ToString("yyyyMMdd") ?? "current";
        return $"{CacheKeyPrefix}{parameterName}:{scope}:{scopeId}:{dateKey}";
    }

    private void InvalidateCache(string parameterName, ParameterScope scope, Guid? scopeId)
    {
        // Pattern-based cache invalidation would be better in production
        // For now, we just log that cache should be invalidated
        _logger.LogDebug(
            "Cache invalidated for parameter {ParameterName} scope {Scope}",
            parameterName, scope);
    }

    private static CalculationParameterDto MapToDto(CalculationParameter param)
    {
        return new CalculationParameterDto
        {
            Id = param.Id,
            ParameterName = param.ParameterName,
            Value = param.Value,
            Description = param.Description,
            Scope = param.Scope,
            ScopeId = param.ScopeId,
            ScopeName = null, // Would need to resolve from other repositories
            EffectiveFrom = param.EffectiveFrom,
            EffectiveTo = param.EffectiveTo,
            IsEffective = param.IsEffective(),
            CreatedAt = param.CreatedAt,
            CreatedBy = param.CreatedBy,
            ModifiedAt = param.ModifiedAt,
            ModifiedBy = param.ModifiedBy
        };
    }
}

/// <summary>
/// Implementation of configuration export service.
/// "That's where I saw the leprechaun!" - Export your configurations like magical finds!
/// </summary>
public class ConfigurationExportService : IConfigurationExportService
{
    private readonly IConfigurationService _configurationService;
    private readonly IFeatureFlagService _featureFlagService;
    private readonly ICalculationParameterService _parameterService;
    private readonly IConfigurationRepository _configRepository;
    private readonly IFeatureFlagRepository _flagRepository;
    private readonly ICalculationParameterRepository _paramRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConfigurationExportService> _logger;

    public ConfigurationExportService(
        IConfigurationService configurationService,
        IFeatureFlagService featureFlagService,
        ICalculationParameterService parameterService,
        IConfigurationRepository configRepository,
        IFeatureFlagRepository flagRepository,
        ICalculationParameterRepository paramRepository,
        IUnitOfWork unitOfWork,
        ILogger<ConfigurationExportService> logger)
    {
        _configurationService = configurationService;
        _featureFlagService = featureFlagService;
        _parameterService = parameterService;
        _configRepository = configRepository;
        _flagRepository = flagRepository;
        _paramRepository = paramRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ConfigurationExportDto> ExportAllAsync(
        string exportedBy,
        CancellationToken cancellationToken = default)
    {
        var configurations = await _configurationService.GetAllConfigurationsAsync(cancellationToken);
        var featureFlags = await _featureFlagService.GetAllFeatureFlagsAsync(cancellationToken);
        var parameters = await _parameterService.GetAllParametersAsync(cancellationToken);

        _logger.LogInformation(
            "Configuration exported by {ExportedBy}: {ConfigCount} configs, {FlagCount} flags, {ParamCount} params",
            exportedBy, configurations.Count, featureFlags.Count, parameters.Count);

        return new ConfigurationExportDto
        {
            ExportedAt = DateTime.UtcNow,
            ExportedBy = exportedBy,
            Configurations = configurations,
            FeatureFlags = featureFlags,
            CalculationParameters = parameters
        };
    }

    public async Task<ConfigurationExportDto> ExportByCategoryAsync(
        ConfigurationCategory category,
        string exportedBy,
        CancellationToken cancellationToken = default)
    {
        var configurations = await _configurationService.GetConfigurationsByCategoryAsync(
            category, cancellationToken);

        return new ConfigurationExportDto
        {
            ExportedAt = DateTime.UtcNow,
            ExportedBy = exportedBy,
            Configurations = configurations,
            FeatureFlags = new List<FeatureFlagDto>(),
            CalculationParameters = new List<CalculationParameterDto>()
        };
    }

    public async Task<ConfigurationImportResult> ImportAsync(
        ConfigurationImportRequest request,
        string importedBy,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        var configurationsImported = 0;
        var featureFlagsImported = 0;
        var parametersImported = 0;
        var skipped = 0;

        // Import configurations
        foreach (var config in request.Configurations)
        {
            try
            {
                var exists = await _configRepository.ExistsAsync(config.Key, cancellationToken);
                if (exists && !request.OverwriteExisting)
                {
                    skipped++;
                    continue;
                }

                if (exists)
                {
                    await _configurationService.UpdateConfigurationAsync(
                        config.Key,
                        new UpdateConfigurationRequest { Value = config.Value },
                        importedBy,
                        cancellationToken);
                }
                else
                {
                    await _configurationService.CreateConfigurationAsync(
                        config, importedBy, cancellationToken);
                }

                configurationsImported++;
            }
            catch (Exception ex)
            {
                errors.Add($"Configuration '{config.Key}': {ex.Message}");
            }
        }

        // Import feature flags
        foreach (var flag in request.FeatureFlags)
        {
            try
            {
                var exists = await _flagRepository.ExistsAsync(flag.Name, cancellationToken);
                if (exists && !request.OverwriteExisting)
                {
                    skipped++;
                    continue;
                }

                if (exists)
                {
                    await _featureFlagService.UpdateFeatureFlagAsync(
                        flag.Name,
                        new UpdateFeatureFlagRequest { IsEnabled = flag.IsEnabled },
                        importedBy,
                        cancellationToken);
                }
                else
                {
                    await _featureFlagService.CreateFeatureFlagAsync(
                        flag, importedBy, cancellationToken);
                }

                featureFlagsImported++;
            }
            catch (Exception ex)
            {
                errors.Add($"Feature flag '{flag.Name}': {ex.Message}");
            }
        }

        // Import calculation parameters
        foreach (var param in request.CalculationParameters)
        {
            try
            {
                var existing = await _paramRepository.GetByNameAndScopeAsync(
                    param.ParameterName, param.Scope, param.ScopeId, cancellationToken);

                if (existing != null && !request.OverwriteExisting)
                {
                    skipped++;
                    continue;
                }

                if (existing != null)
                {
                    await _parameterService.UpdateParameterAsync(
                        existing.Id,
                        new UpdateCalculationParameterRequest { Value = param.Value },
                        importedBy,
                        cancellationToken);
                }
                else
                {
                    await _parameterService.CreateParameterAsync(
                        param, importedBy, cancellationToken);
                }

                parametersImported++;
            }
            catch (Exception ex)
            {
                errors.Add($"Parameter '{param.ParameterName}': {ex.Message}");
            }
        }

        _logger.LogInformation(
            "Configuration imported by {ImportedBy}: {ConfigCount} configs, {FlagCount} flags, {ParamCount} params, {Skipped} skipped",
            importedBy, configurationsImported, featureFlagsImported, parametersImported, skipped);

        return new ConfigurationImportResult
        {
            ConfigurationsImported = configurationsImported,
            FeatureFlagsImported = featureFlagsImported,
            CalculationParametersImported = parametersImported,
            Skipped = skipped,
            Errors = errors
        };
    }

    public async Task<ConfigurationImportResult> ValidateImportAsync(
        ConfigurationImportRequest request,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<string>();
        var skipped = 0;

        // Validate configurations
        foreach (var config in request.Configurations)
        {
            if (string.IsNullOrWhiteSpace(config.Key))
            {
                errors.Add("Configuration key is required");
            }

            var exists = await _configRepository.ExistsAsync(config.Key, cancellationToken);
            if (exists && !request.OverwriteExisting)
            {
                skipped++;
            }
        }

        // Validate feature flags
        foreach (var flag in request.FeatureFlags)
        {
            if (string.IsNullOrWhiteSpace(flag.Name))
            {
                errors.Add("Feature flag name is required");
            }

            var exists = await _flagRepository.ExistsAsync(flag.Name, cancellationToken);
            if (exists && !request.OverwriteExisting)
            {
                skipped++;
            }
        }

        // Validate calculation parameters
        foreach (var param in request.CalculationParameters)
        {
            if (string.IsNullOrWhiteSpace(param.ParameterName))
            {
                errors.Add("Parameter name is required");
            }

            var existing = await _paramRepository.GetByNameAndScopeAsync(
                param.ParameterName, param.Scope, param.ScopeId, cancellationToken);

            if (existing != null && !request.OverwriteExisting)
            {
                skipped++;
            }
        }

        return new ConfigurationImportResult
        {
            ConfigurationsImported = request.Configurations.Count - skipped,
            FeatureFlagsImported = request.FeatureFlags.Count,
            CalculationParametersImported = request.CalculationParameters.Count,
            Skipped = skipped,
            Errors = errors
        };
    }

    public async Task<ConfigurationSummaryDto> GetSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var configurations = await _configurationService.GetAllConfigurationsAsync(cancellationToken);
        var featureFlags = await _featureFlagService.GetAllFeatureFlagsAsync(cancellationToken);
        var parameters = await _parameterService.GetAllParametersAsync(cancellationToken);

        var byCategory = configurations
            .GroupBy(c => c.Category)
            .Select(g => new ConfigurationCategoryDto
            {
                Category = g.Key,
                Name = g.Key.ToString(),
                Description = null,
                SettingsCount = g.Count()
            })
            .ToList();

        var lastModified = configurations
            .Where(c => c.ModifiedAt.HasValue)
            .OrderByDescending(c => c.ModifiedAt)
            .Select(c => c.ModifiedAt)
            .FirstOrDefault();

        return new ConfigurationSummaryDto
        {
            TotalConfigurations = configurations.Count,
            TotalFeatureFlags = featureFlags.Count,
            EnabledFeatureFlags = featureFlags.Count(f => f.IsEnabled),
            TotalEmailTemplates = 0, // Would need email template repository
            ActiveEmailTemplates = 0,
            TotalCalculationParameters = parameters.Count,
            EffectiveCalculationParameters = parameters.Count(p => p.IsEffective),
            ByCategory = byCategory,
            LastModified = lastModified
        };
    }
}
