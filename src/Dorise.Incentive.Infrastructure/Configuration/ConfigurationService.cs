using Dorise.Incentive.Application.Configuration.DTOs;
using Dorise.Incentive.Application.Configuration.Services;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Configuration;

/// <summary>
/// Implementation of system configuration service with caching.
/// "Principal Skinner made me?" - This service makes configuration magic happen!
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<ConfigurationService> _logger;

    private const string CacheKeyPrefix = "config:";
    private const string AllConfigsCacheKey = "config:all";
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(15);

    public ConfigurationService(
        IConfigurationRepository repository,
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<ConfigurationService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<string?> GetValueAsync(string key, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{key}";

        if (_cache.TryGetValue(cacheKey, out string? cachedValue))
        {
            return cachedValue;
        }

        var config = await _repository.GetByKeyAsync(key, cancellationToken);
        if (config == null || !config.IsEffective())
        {
            return null;
        }

        _cache.Set(cacheKey, config.Value, DefaultCacheDuration);
        return config.Value;
    }

    public async Task<T?> GetValueAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        var config = await _repository.GetByKeyAsync(key, cancellationToken);
        if (config == null || !config.IsEffective())
        {
            return default;
        }

        return config.GetTypedValue<T>();
    }

    public async Task<string> GetValueOrDefaultAsync(
        string key,
        string defaultValue,
        CancellationToken cancellationToken = default)
    {
        var value = await GetValueAsync(key, cancellationToken);
        return value ?? defaultValue;
    }

    public async Task<SystemConfigurationDto?> GetConfigurationAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var config = await _repository.GetByKeyAsync(key, cancellationToken);
        return config == null ? null : MapToDto(config);
    }

    public async Task<IReadOnlyList<SystemConfigurationDto>> GetAllConfigurationsAsync(
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(AllConfigsCacheKey, out IReadOnlyList<SystemConfigurationDto>? cached) && cached != null)
        {
            return cached;
        }

        var configs = await _repository.GetAllAsync(cancellationToken);
        var result = configs.Select(MapToDto).ToList();

        _cache.Set(AllConfigsCacheKey, (IReadOnlyList<SystemConfigurationDto>)result, DefaultCacheDuration);
        return result;
    }

    public async Task<IReadOnlyList<SystemConfigurationDto>> GetConfigurationsByCategoryAsync(
        ConfigurationCategory category,
        CancellationToken cancellationToken = default)
    {
        var configs = await _repository.GetByCategoryAsync(category, cancellationToken);
        return configs.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<SystemConfigurationDto>> SearchConfigurationsAsync(
        ConfigurationSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        var configs = await _repository.SearchAsync(
            query.Category,
            query.KeyPrefix,
            query.IsEffective,
            cancellationToken);

        return configs
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(MapToDto)
            .ToList();
    }

    public async Task<SystemConfigurationDto> CreateConfigurationAsync(
        CreateConfigurationRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        // Check for duplicate
        if (await _repository.ExistsAsync(request.Key, cancellationToken))
        {
            throw new InvalidOperationException($"Configuration with key '{request.Key}' already exists");
        }

        var config = SystemConfiguration.Create(
            request.Key,
            request.Value,
            request.Category,
            request.DataType,
            request.Description,
            request.IsEncrypted,
            request.IsReadOnly,
            request.ValidationRegex,
            request.DefaultValue);

        if (request.EffectiveFrom.HasValue || request.EffectiveTo.HasValue)
        {
            config.SetEffectivePeriod(request.EffectiveFrom, request.EffectiveTo);
        }

        await _repository.AddAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        InvalidateCache(request.Key);

        _logger.LogInformation(
            "Configuration {Key} created by {CreatedBy}",
            request.Key, createdBy);

        return MapToDto(config);
    }

    public async Task<SystemConfigurationDto> UpdateConfigurationAsync(
        string key,
        UpdateConfigurationRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var config = await _repository.GetByKeyAsync(key, cancellationToken)
            ?? throw new InvalidOperationException($"Configuration with key '{key}' not found");

        config.UpdateValue(request.Value);

        if (request.EffectiveFrom.HasValue || request.EffectiveTo.HasValue)
        {
            config.SetEffectivePeriod(request.EffectiveFrom, request.EffectiveTo);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        InvalidateCache(key);

        _logger.LogInformation(
            "Configuration {Key} updated by {ModifiedBy}",
            key, modifiedBy);

        return MapToDto(config);
    }

    public async Task<BulkUpdateResult> BulkUpdateConfigurationsAsync(
        BulkUpdateConfigurationRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var errors = new List<BulkUpdateError>();
        var succeeded = 0;

        foreach (var update in request.Updates)
        {
            try
            {
                await UpdateConfigurationAsync(
                    update.Key,
                    new UpdateConfigurationRequest { Value = update.Value },
                    modifiedBy,
                    cancellationToken);
                succeeded++;
            }
            catch (Exception ex)
            {
                errors.Add(new BulkUpdateError
                {
                    Key = update.Key,
                    ErrorMessage = ex.Message
                });
            }
        }

        return new BulkUpdateResult
        {
            TotalRequested = request.Updates.Count,
            Succeeded = succeeded,
            Failed = errors.Count,
            Errors = errors
        };
    }

    public async Task DeleteConfigurationAsync(
        string key,
        string deletedBy,
        CancellationToken cancellationToken = default)
    {
        var config = await _repository.GetByKeyAsync(key, cancellationToken)
            ?? throw new InvalidOperationException($"Configuration with key '{key}' not found");

        if (config.IsReadOnly)
        {
            throw new InvalidOperationException($"Configuration '{key}' is read-only and cannot be deleted");
        }

        await _repository.DeleteAsync(config, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        InvalidateCache(key);

        _logger.LogInformation(
            "Configuration {Key} deleted by {DeletedBy}",
            key, deletedBy);
    }

    public async Task<SystemConfigurationDto> ResetToDefaultAsync(
        string key,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var config = await _repository.GetByKeyAsync(key, cancellationToken)
            ?? throw new InvalidOperationException($"Configuration with key '{key}' not found");

        if (string.IsNullOrEmpty(config.DefaultValue))
        {
            throw new InvalidOperationException($"Configuration '{key}' has no default value");
        }

        config.UpdateValue(config.DefaultValue);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        InvalidateCache(key);

        _logger.LogInformation(
            "Configuration {Key} reset to default by {ModifiedBy}",
            key, modifiedBy);

        return MapToDto(config);
    }

    public Task RefreshCacheAsync(CancellationToken cancellationToken = default)
    {
        // Clear all configuration cache entries
        // In a real implementation, you might use a distributed cache with pattern-based invalidation
        _cache.Remove(AllConfigsCacheKey);
        _logger.LogInformation("Configuration cache refreshed");
        return Task.CompletedTask;
    }

    private void InvalidateCache(string key)
    {
        _cache.Remove($"{CacheKeyPrefix}{key}");
        _cache.Remove(AllConfigsCacheKey);
    }

    private static SystemConfigurationDto MapToDto(SystemConfiguration config)
    {
        return new SystemConfigurationDto
        {
            Id = config.Id,
            Key = config.Key,
            Value = config.IsEncrypted ? "********" : config.Value,
            Description = config.Description,
            Category = config.Category,
            DataType = config.DataType,
            IsEncrypted = config.IsEncrypted,
            IsReadOnly = config.IsReadOnly,
            DefaultValue = config.DefaultValue,
            EffectiveFrom = config.EffectiveFrom,
            EffectiveTo = config.EffectiveTo,
            IsEffective = config.IsEffective(),
            CreatedAt = config.CreatedAt,
            CreatedBy = config.CreatedBy,
            ModifiedAt = config.ModifiedAt,
            ModifiedBy = config.ModifiedBy
        };
    }
}

/// <summary>
/// Implementation of feature flag service.
/// "I'm pedaling backwards!" - Feature flags let you pedal in any direction!
/// </summary>
public class FeatureFlagService : IFeatureFlagService
{
    private readonly IFeatureFlagRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<FeatureFlagService> _logger;

    private const string CacheKeyPrefix = "feature:";
    private const string AllFlagsCacheKey = "feature:all";
    private static readonly TimeSpan DefaultCacheDuration = TimeSpan.FromMinutes(5);

    public FeatureFlagService(
        IFeatureFlagRepository repository,
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<FeatureFlagService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<bool> IsEnabledAsync(string flagName, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{CacheKeyPrefix}{flagName}";

        if (_cache.TryGetValue(cacheKey, out bool cachedValue))
        {
            return cachedValue;
        }

        var flag = await _repository.GetByNameAsync(flagName, cancellationToken);
        var isEnabled = flag?.IsEnabled ?? false;

        _cache.Set(cacheKey, isEnabled, DefaultCacheDuration);
        return isEnabled;
    }

    public async Task<bool> IsEnabledForUserAsync(
        string flagName,
        Guid userId,
        IEnumerable<string>? userRoles = null,
        CancellationToken cancellationToken = default)
    {
        var flag = await _repository.GetByNameAsync(flagName, cancellationToken);
        return flag?.IsEnabledForUser(userId, userRoles) ?? false;
    }

    public async Task<IDictionary<string, bool>> CheckFlagsAsync(
        IEnumerable<string> flagNames,
        Guid? userId = null,
        IEnumerable<string>? userRoles = null,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<string, bool>();

        foreach (var flagName in flagNames)
        {
            var isEnabled = userId.HasValue
                ? await IsEnabledForUserAsync(flagName, userId.Value, userRoles, cancellationToken)
                : await IsEnabledAsync(flagName, cancellationToken);

            result[flagName] = isEnabled;
        }

        return result;
    }

    public async Task<FeatureFlagDto?> GetFeatureFlagAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        var flag = await _repository.GetByNameAsync(name, cancellationToken);
        return flag == null ? null : MapToDto(flag);
    }

    public async Task<IReadOnlyList<FeatureFlagDto>> GetAllFeatureFlagsAsync(
        CancellationToken cancellationToken = default)
    {
        var flags = await _repository.GetAllAsync(cancellationToken);
        return flags.Select(MapToDto).ToList();
    }

    public async Task<IReadOnlyList<FeatureFlagSummaryDto>> GetEnabledFlagsAsync(
        CancellationToken cancellationToken = default)
    {
        var flags = await _repository.GetEnabledAsync(cancellationToken);
        return flags.Select(f => new FeatureFlagSummaryDto
        {
            Name = f.Name,
            IsEnabled = f.IsEnabled,
            FlagType = f.FlagType
        }).ToList();
    }

    public async Task<FeatureFlagDto> CreateFeatureFlagAsync(
        CreateFeatureFlagRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        if (await _repository.ExistsAsync(request.Name, cancellationToken))
        {
            throw new InvalidOperationException($"Feature flag '{request.Name}' already exists");
        }

        var flag = FeatureFlag.Create(
            request.Name,
            request.Description,
            request.IsEnabled,
            request.FlagType);

        await _repository.AddAsync(flag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        InvalidateCache(request.Name);

        _logger.LogInformation(
            "Feature flag {FlagName} created by {CreatedBy}",
            request.Name, createdBy);

        return MapToDto(flag);
    }

    public async Task<FeatureFlagDto> UpdateFeatureFlagAsync(
        string name,
        UpdateFeatureFlagRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var flag = await _repository.GetByNameAsync(name, cancellationToken)
            ?? throw new InvalidOperationException($"Feature flag '{name}' not found");

        if (request.IsEnabled.HasValue)
        {
            if (request.IsEnabled.Value) flag.Enable();
            else flag.Disable();
        }

        if (request.RolloutPercentage.HasValue)
        {
            flag.SetRolloutPercentage(request.RolloutPercentage.Value);
        }

        if (request.EnabledForUsers != null)
        {
            flag.EnableForUsers(request.EnabledForUsers);
        }

        if (request.EnabledForRoles != null)
        {
            flag.EnableForRoles(request.EnabledForRoles);
        }

        if (request.EnabledFrom.HasValue || request.EnabledUntil.HasValue)
        {
            flag.SetSchedule(request.EnabledFrom, request.EnabledUntil);
        }

        if (request.Metadata != null)
        {
            flag.SetMetadata(request.Metadata);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        InvalidateCache(name);

        _logger.LogInformation(
            "Feature flag {FlagName} updated by {ModifiedBy}",
            name, modifiedBy);

        return MapToDto(flag);
    }

    public async Task EnableFlagAsync(string name, string modifiedBy, CancellationToken cancellationToken = default)
    {
        var flag = await _repository.GetByNameAsync(name, cancellationToken)
            ?? throw new InvalidOperationException($"Feature flag '{name}' not found");

        flag.Enable();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        InvalidateCache(name);

        _logger.LogInformation("Feature flag {FlagName} enabled by {ModifiedBy}", name, modifiedBy);
    }

    public async Task DisableFlagAsync(string name, string modifiedBy, CancellationToken cancellationToken = default)
    {
        var flag = await _repository.GetByNameAsync(name, cancellationToken)
            ?? throw new InvalidOperationException($"Feature flag '{name}' not found");

        flag.Disable();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        InvalidateCache(name);

        _logger.LogInformation("Feature flag {FlagName} disabled by {ModifiedBy}", name, modifiedBy);
    }

    public async Task<bool> ToggleFlagAsync(string name, string modifiedBy, CancellationToken cancellationToken = default)
    {
        var flag = await _repository.GetByNameAsync(name, cancellationToken)
            ?? throw new InvalidOperationException($"Feature flag '{name}' not found");

        flag.Toggle();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        InvalidateCache(name);

        _logger.LogInformation(
            "Feature flag {FlagName} toggled to {IsEnabled} by {ModifiedBy}",
            name, flag.IsEnabled, modifiedBy);

        return flag.IsEnabled;
    }

    public async Task DeleteFeatureFlagAsync(string name, string deletedBy, CancellationToken cancellationToken = default)
    {
        var flag = await _repository.GetByNameAsync(name, cancellationToken)
            ?? throw new InvalidOperationException($"Feature flag '{name}' not found");

        await _repository.DeleteAsync(flag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        InvalidateCache(name);

        _logger.LogInformation("Feature flag {FlagName} deleted by {DeletedBy}", name, deletedBy);
    }

    private void InvalidateCache(string name)
    {
        _cache.Remove($"{CacheKeyPrefix}{name}");
        _cache.Remove(AllFlagsCacheKey);
    }

    private static FeatureFlagDto MapToDto(FeatureFlag flag)
    {
        return new FeatureFlagDto
        {
            Id = flag.Id,
            Name = flag.Name,
            Description = flag.Description,
            IsEnabled = flag.IsEnabled,
            FlagType = flag.FlagType,
            EnabledForUsers = flag.EnabledForUsers,
            EnabledForRoles = flag.EnabledForRoles,
            RolloutPercentage = flag.RolloutPercentage,
            EnabledFrom = flag.EnabledFrom,
            EnabledUntil = flag.EnabledUntil,
            Metadata = flag.Metadata,
            CreatedAt = flag.CreatedAt,
            CreatedBy = flag.CreatedBy,
            ModifiedAt = flag.ModifiedAt,
            ModifiedBy = flag.ModifiedBy
        };
    }
}

/// <summary>
/// Implementation of email template service.
/// </summary>
public class EmailTemplateService : IEmailTemplateService
{
    private readonly IEmailTemplateRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<EmailTemplateService> _logger;

    public EmailTemplateService(
        IEmailTemplateRepository repository,
        IUnitOfWork unitOfWork,
        ILogger<EmailTemplateService> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<EmailTemplateDto?> GetTemplateAsync(
        string templateName,
        CancellationToken cancellationToken = default)
    {
        var template = await _repository.GetByNameAsync(templateName, cancellationToken);
        return template == null ? null : MapToDto(template);
    }

    public async Task<IReadOnlyList<EmailTemplateSummaryDto>> GetAllTemplatesAsync(
        CancellationToken cancellationToken = default)
    {
        var templates = await _repository.GetAllAsync(cancellationToken);
        return templates.Select(t => new EmailTemplateSummaryDto
        {
            Id = t.Id,
            TemplateName = t.TemplateName,
            Subject = t.Subject,
            IsActive = t.IsActive
        }).ToList();
    }

    public async Task<IReadOnlyList<EmailTemplateSummaryDto>> GetActiveTemplatesAsync(
        CancellationToken cancellationToken = default)
    {
        var templates = await _repository.GetActiveAsync(cancellationToken);
        return templates.Select(t => new EmailTemplateSummaryDto
        {
            Id = t.Id,
            TemplateName = t.TemplateName,
            Subject = t.Subject,
            IsActive = t.IsActive
        }).ToList();
    }

    public async Task<EmailTemplateDto> CreateTemplateAsync(
        CreateEmailTemplateRequest request,
        string createdBy,
        CancellationToken cancellationToken = default)
    {
        if (await _repository.ExistsAsync(request.TemplateName, cancellationToken))
        {
            throw new InvalidOperationException($"Email template '{request.TemplateName}' already exists");
        }

        var template = EmailTemplate.Create(
            request.TemplateName,
            request.Subject,
            request.Body,
            request.IsHtml,
            request.Description);

        if (request.AvailablePlaceholders?.Any() == true)
        {
            template.SetPlaceholders(request.AvailablePlaceholders);
        }

        await _repository.AddAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Email template {TemplateName} created by {CreatedBy}",
            request.TemplateName, createdBy);

        return MapToDto(template);
    }

    public async Task<EmailTemplateDto> UpdateTemplateAsync(
        string templateName,
        UpdateEmailTemplateRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var template = await _repository.GetByNameAsync(templateName, cancellationToken)
            ?? throw new InvalidOperationException($"Email template '{templateName}' not found");

        template.UpdateContent(request.Subject, request.Body);

        if (request.AvailablePlaceholders?.Any() == true)
        {
            template.SetPlaceholders(request.AvailablePlaceholders);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Email template {TemplateName} updated by {ModifiedBy}",
            templateName, modifiedBy);

        return MapToDto(template);
    }

    public async Task<RenderedEmailDto> RenderTemplateAsync(
        RenderEmailTemplateRequest request,
        CancellationToken cancellationToken = default)
    {
        var template = await _repository.GetByNameAsync(request.TemplateName, cancellationToken)
            ?? throw new InvalidOperationException($"Email template '{request.TemplateName}' not found");

        if (!template.IsActive)
        {
            throw new InvalidOperationException($"Email template '{request.TemplateName}' is not active");
        }

        return new RenderedEmailDto
        {
            Subject = template.RenderSubject(request.Values),
            Body = template.RenderBody(request.Values),
            IsHtml = template.IsHtml
        };
    }

    public async Task ActivateTemplateAsync(
        string templateName,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var template = await _repository.GetByNameAsync(templateName, cancellationToken)
            ?? throw new InvalidOperationException($"Email template '{templateName}' not found");

        template.Activate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email template {TemplateName} activated by {ModifiedBy}", templateName, modifiedBy);
    }

    public async Task DeactivateTemplateAsync(
        string templateName,
        string modifiedBy,
        CancellationToken cancellationToken = default)
    {
        var template = await _repository.GetByNameAsync(templateName, cancellationToken)
            ?? throw new InvalidOperationException($"Email template '{templateName}' not found");

        template.Deactivate();
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email template {TemplateName} deactivated by {ModifiedBy}", templateName, modifiedBy);
    }

    public async Task DeleteTemplateAsync(
        string templateName,
        string deletedBy,
        CancellationToken cancellationToken = default)
    {
        var template = await _repository.GetByNameAsync(templateName, cancellationToken)
            ?? throw new InvalidOperationException($"Email template '{templateName}' not found");

        await _repository.DeleteAsync(template, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Email template {TemplateName} deleted by {DeletedBy}", templateName, deletedBy);
    }

    public async Task<RenderedEmailDto> PreviewTemplateAsync(
        string templateName,
        CancellationToken cancellationToken = default)
    {
        var template = await _repository.GetByNameAsync(templateName, cancellationToken)
            ?? throw new InvalidOperationException($"Email template '{templateName}' not found");

        // Generate sample values for placeholders
        var sampleValues = new Dictionary<string, string>
        {
            ["EmployeeName"] = "John Doe",
            ["Period"] = "January 2025",
            ["Amount"] = "$1,500.00",
            ["ApproverName"] = "Jane Smith",
            ["CompanyName"] = "Dorise Inc."
        };

        return new RenderedEmailDto
        {
            Subject = template.RenderSubject(sampleValues),
            Body = template.RenderBody(sampleValues),
            IsHtml = template.IsHtml
        };
    }

    private static EmailTemplateDto MapToDto(EmailTemplate template)
    {
        var placeholders = string.IsNullOrEmpty(template.AvailablePlaceholders)
            ? null
            : System.Text.Json.JsonSerializer.Deserialize<List<string>>(template.AvailablePlaceholders);

        return new EmailTemplateDto
        {
            Id = template.Id,
            TemplateName = template.TemplateName,
            Subject = template.Subject,
            Body = template.Body,
            IsHtml = template.IsHtml,
            IsActive = template.IsActive,
            Description = template.Description,
            AvailablePlaceholders = placeholders,
            CreatedAt = template.CreatedAt,
            CreatedBy = template.CreatedBy,
            ModifiedAt = template.ModifiedAt,
            ModifiedBy = template.ModifiedBy
        };
    }
}
