using Dorise.Incentive.Application.Configuration.DTOs;
using Dorise.Incentive.Domain.Entities;

namespace Dorise.Incentive.Application.Configuration.Services;

/// <summary>
/// Service interface for system configuration management.
/// "The doctor said I wouldn't have so many nosebleeds if I kept my finger outta there." -
/// Proper configuration prevents system nosebleeds!
/// </summary>
public interface IConfigurationService
{
    // ============== System Configuration ==============

    /// <summary>
    /// Gets a configuration value by key.
    /// </summary>
    Task<string?> GetValueAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a typed configuration value.
    /// </summary>
    Task<T?> GetValueAsync<T>(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a configuration value with a default fallback.
    /// </summary>
    Task<string> GetValueOrDefaultAsync(
        string key,
        string defaultValue,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a configuration by key.
    /// </summary>
    Task<SystemConfigurationDto?> GetConfigurationAsync(
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all configurations.
    /// </summary>
    Task<IReadOnlyList<SystemConfigurationDto>> GetAllConfigurationsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets configurations by category.
    /// </summary>
    Task<IReadOnlyList<SystemConfigurationDto>> GetConfigurationsByCategoryAsync(
        ConfigurationCategory category,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches configurations.
    /// </summary>
    Task<IReadOnlyList<SystemConfigurationDto>> SearchConfigurationsAsync(
        ConfigurationSearchQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new configuration.
    /// </summary>
    Task<SystemConfigurationDto> CreateConfigurationAsync(
        CreateConfigurationRequest request,
        string createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a configuration value.
    /// </summary>
    Task<SystemConfigurationDto> UpdateConfigurationAsync(
        string key,
        UpdateConfigurationRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Bulk updates configurations.
    /// </summary>
    Task<BulkUpdateResult> BulkUpdateConfigurationsAsync(
        BulkUpdateConfigurationRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a configuration.
    /// </summary>
    Task DeleteConfigurationAsync(
        string key,
        string deletedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets a configuration to its default value.
    /// </summary>
    Task<SystemConfigurationDto> ResetToDefaultAsync(
        string key,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes the configuration cache.
    /// </summary>
    Task RefreshCacheAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for feature flag management.
/// "My cat's breath smells like cat food." - Feature flags smell like flexibility!
/// </summary>
public interface IFeatureFlagService
{
    /// <summary>
    /// Checks if a feature is enabled.
    /// </summary>
    Task<bool> IsEnabledAsync(
        string flagName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a feature is enabled for a specific user.
    /// </summary>
    Task<bool> IsEnabledForUserAsync(
        string flagName,
        Guid userId,
        IEnumerable<string>? userRoles = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks multiple feature flags.
    /// </summary>
    Task<IDictionary<string, bool>> CheckFlagsAsync(
        IEnumerable<string> flagNames,
        Guid? userId = null,
        IEnumerable<string>? userRoles = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a feature flag by name.
    /// </summary>
    Task<FeatureFlagDto?> GetFeatureFlagAsync(
        string name,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all feature flags.
    /// </summary>
    Task<IReadOnlyList<FeatureFlagDto>> GetAllFeatureFlagsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets enabled feature flags.
    /// </summary>
    Task<IReadOnlyList<FeatureFlagSummaryDto>> GetEnabledFlagsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new feature flag.
    /// </summary>
    Task<FeatureFlagDto> CreateFeatureFlagAsync(
        CreateFeatureFlagRequest request,
        string createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a feature flag.
    /// </summary>
    Task<FeatureFlagDto> UpdateFeatureFlagAsync(
        string name,
        UpdateFeatureFlagRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enables a feature flag.
    /// </summary>
    Task EnableFlagAsync(
        string name,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Disables a feature flag.
    /// </summary>
    Task DisableFlagAsync(
        string name,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Toggles a feature flag.
    /// </summary>
    Task<bool> ToggleFlagAsync(
        string name,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a feature flag.
    /// </summary>
    Task DeleteFeatureFlagAsync(
        string name,
        string deletedBy,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for email template management.
/// "Super Nintendo Chalmers!" - Templates are super for consistent emails!
/// </summary>
public interface IEmailTemplateService
{
    /// <summary>
    /// Gets an email template by name.
    /// </summary>
    Task<EmailTemplateDto?> GetTemplateAsync(
        string templateName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all email templates.
    /// </summary>
    Task<IReadOnlyList<EmailTemplateSummaryDto>> GetAllTemplatesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets active email templates.
    /// </summary>
    Task<IReadOnlyList<EmailTemplateSummaryDto>> GetActiveTemplatesAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new email template.
    /// </summary>
    Task<EmailTemplateDto> CreateTemplateAsync(
        CreateEmailTemplateRequest request,
        string createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an email template.
    /// </summary>
    Task<EmailTemplateDto> UpdateTemplateAsync(
        string templateName,
        UpdateEmailTemplateRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Renders an email template with values.
    /// </summary>
    Task<RenderedEmailDto> RenderTemplateAsync(
        RenderEmailTemplateRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Activates an email template.
    /// </summary>
    Task ActivateTemplateAsync(
        string templateName,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deactivates an email template.
    /// </summary>
    Task DeactivateTemplateAsync(
        string templateName,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an email template.
    /// </summary>
    Task DeleteTemplateAsync(
        string templateName,
        string deletedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Previews a template render with sample data.
    /// </summary>
    Task<RenderedEmailDto> PreviewTemplateAsync(
        string templateName,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for calculation parameter management.
/// "I'm Idaho!" - Parameters define the calculations!
/// </summary>
public interface ICalculationParameterService
{
    /// <summary>
    /// Gets a parameter value.
    /// </summary>
    Task<decimal?> GetParameterValueAsync(
        string parameterName,
        ParameterScope scope = ParameterScope.Global,
        Guid? scopeId = null,
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a parameter value with default fallback.
    /// </summary>
    Task<decimal> GetParameterValueOrDefaultAsync(
        string parameterName,
        decimal defaultValue,
        ParameterScope scope = ParameterScope.Global,
        Guid? scopeId = null,
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a calculation parameter.
    /// </summary>
    Task<CalculationParameterDto?> GetParameterAsync(
        string parameterName,
        ParameterScope scope = ParameterScope.Global,
        Guid? scopeId = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all calculation parameters.
    /// </summary>
    Task<IReadOnlyList<CalculationParameterDto>> GetAllParametersAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches calculation parameters.
    /// </summary>
    Task<IReadOnlyList<CalculationParameterDto>> SearchParametersAsync(
        CalculationParameterSearchQuery query,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets effective parameters for a scope.
    /// </summary>
    Task<IReadOnlyList<CalculationParameterDto>> GetEffectiveParametersAsync(
        ParameterScope scope,
        Guid? scopeId = null,
        DateTime? asOfDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new calculation parameter.
    /// </summary>
    Task<CalculationParameterDto> CreateParameterAsync(
        CreateCalculationParameterRequest request,
        string createdBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates a calculation parameter.
    /// </summary>
    Task<CalculationParameterDto> UpdateParameterAsync(
        Guid parameterId,
        UpdateCalculationParameterRequest request,
        string modifiedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a calculation parameter.
    /// </summary>
    Task DeleteParameterAsync(
        Guid parameterId,
        string deletedBy,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Service interface for configuration export/import.
/// "Go banana!" - Export and import configurations like bananas!
/// </summary>
public interface IConfigurationExportService
{
    /// <summary>
    /// Exports all configurations.
    /// </summary>
    Task<ConfigurationExportDto> ExportAllAsync(
        string exportedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports configurations by category.
    /// </summary>
    Task<ConfigurationExportDto> ExportByCategoryAsync(
        ConfigurationCategory category,
        string exportedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Imports configurations.
    /// </summary>
    Task<ConfigurationImportResult> ImportAsync(
        ConfigurationImportRequest request,
        string importedBy,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates an import request without applying changes.
    /// </summary>
    Task<ConfigurationImportResult> ValidateImportAsync(
        ConfigurationImportRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets configuration summary.
    /// </summary>
    Task<ConfigurationSummaryDto> GetSummaryAsync(
        CancellationToken cancellationToken = default);
}
