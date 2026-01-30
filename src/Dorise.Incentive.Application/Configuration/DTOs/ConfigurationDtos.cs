using Dorise.Incentive.Domain.Entities;

namespace Dorise.Incentive.Application.Configuration.DTOs;

/// <summary>
/// DTOs for system configuration management.
/// "I choo-choo-choose you!" - Choose the right configuration!
/// </summary>

// ============== System Configuration DTOs ==============

public record SystemConfigurationDto
{
    public Guid Id { get; init; }
    public required string Key { get; init; }
    public required string Value { get; init; }
    public string? Description { get; init; }
    public ConfigurationCategory Category { get; init; }
    public ConfigurationDataType DataType { get; init; }
    public bool IsEncrypted { get; init; }
    public bool IsReadOnly { get; init; }
    public string? DefaultValue { get; init; }
    public DateTime? EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public bool IsEffective { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? ModifiedAt { get; init; }
    public string? ModifiedBy { get; init; }
}

public record SystemConfigurationSummaryDto
{
    public required string Key { get; init; }
    public required string Value { get; init; }
    public ConfigurationCategory Category { get; init; }
    public bool IsReadOnly { get; init; }
}

public record CreateConfigurationRequest
{
    public required string Key { get; init; }
    public required string Value { get; init; }
    public string? Description { get; init; }
    public ConfigurationCategory Category { get; init; } = ConfigurationCategory.General;
    public ConfigurationDataType DataType { get; init; } = ConfigurationDataType.String;
    public bool IsEncrypted { get; init; }
    public bool IsReadOnly { get; init; }
    public string? ValidationRegex { get; init; }
    public string? DefaultValue { get; init; }
    public DateTime? EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
}

public record UpdateConfigurationRequest
{
    public required string Value { get; init; }
    public DateTime? EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
}

public record ConfigurationSearchQuery
{
    public ConfigurationCategory? Category { get; init; }
    public string? KeyPrefix { get; init; }
    public bool? IsEffective { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}

// ============== Feature Flag DTOs ==============

public record FeatureFlagDto
{
    public Guid Id { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool IsEnabled { get; init; }
    public FeatureFlagType FlagType { get; init; }
    public string? EnabledForUsers { get; init; }
    public string? EnabledForRoles { get; init; }
    public int? RolloutPercentage { get; init; }
    public DateTime? EnabledFrom { get; init; }
    public DateTime? EnabledUntil { get; init; }
    public string? Metadata { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? ModifiedAt { get; init; }
    public string? ModifiedBy { get; init; }
}

public record FeatureFlagSummaryDto
{
    public required string Name { get; init; }
    public bool IsEnabled { get; init; }
    public FeatureFlagType FlagType { get; init; }
}

public record CreateFeatureFlagRequest
{
    public required string Name { get; init; }
    public string? Description { get; init; }
    public bool IsEnabled { get; init; }
    public FeatureFlagType FlagType { get; init; } = FeatureFlagType.Boolean;
}

public record UpdateFeatureFlagRequest
{
    public string? Description { get; init; }
    public bool? IsEnabled { get; init; }
    public FeatureFlagType? FlagType { get; init; }
    public IReadOnlyList<Guid>? EnabledForUsers { get; init; }
    public IReadOnlyList<string>? EnabledForRoles { get; init; }
    public int? RolloutPercentage { get; init; }
    public DateTime? EnabledFrom { get; init; }
    public DateTime? EnabledUntil { get; init; }
    public string? Metadata { get; init; }
}

public record FeatureFlagCheckRequest
{
    public required string FlagName { get; init; }
    public Guid? UserId { get; init; }
    public IReadOnlyList<string>? UserRoles { get; init; }
}

public record FeatureFlagCheckResult
{
    public required string FlagName { get; init; }
    public bool IsEnabled { get; init; }
    public string? Reason { get; init; }
}

// ============== Email Template DTOs ==============

public record EmailTemplateDto
{
    public Guid Id { get; init; }
    public required string TemplateName { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public bool IsHtml { get; init; }
    public bool IsActive { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<string>? AvailablePlaceholders { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? ModifiedAt { get; init; }
    public string? ModifiedBy { get; init; }
}

public record EmailTemplateSummaryDto
{
    public Guid Id { get; init; }
    public required string TemplateName { get; init; }
    public required string Subject { get; init; }
    public bool IsActive { get; init; }
}

public record CreateEmailTemplateRequest
{
    public required string TemplateName { get; init; }
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public bool IsHtml { get; init; } = true;
    public string? Description { get; init; }
    public IReadOnlyList<string>? AvailablePlaceholders { get; init; }
}

public record UpdateEmailTemplateRequest
{
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public string? Description { get; init; }
    public IReadOnlyList<string>? AvailablePlaceholders { get; init; }
}

public record RenderEmailTemplateRequest
{
    public required string TemplateName { get; init; }
    public required IDictionary<string, string> Values { get; init; }
}

public record RenderedEmailDto
{
    public required string Subject { get; init; }
    public required string Body { get; init; }
    public bool IsHtml { get; init; }
}

// ============== Calculation Parameter DTOs ==============

public record CalculationParameterDto
{
    public Guid Id { get; init; }
    public required string ParameterName { get; init; }
    public decimal Value { get; init; }
    public string? Description { get; init; }
    public ParameterScope Scope { get; init; }
    public Guid? ScopeId { get; init; }
    public string? ScopeName { get; init; }
    public DateTime EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
    public bool IsEffective { get; init; }
    public DateTime CreatedAt { get; init; }
    public string? CreatedBy { get; init; }
    public DateTime? ModifiedAt { get; init; }
    public string? ModifiedBy { get; init; }
}

public record CreateCalculationParameterRequest
{
    public required string ParameterName { get; init; }
    public decimal Value { get; init; }
    public string? Description { get; init; }
    public ParameterScope Scope { get; init; } = ParameterScope.Global;
    public Guid? ScopeId { get; init; }
    public DateTime? EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
}

public record UpdateCalculationParameterRequest
{
    public decimal Value { get; init; }
    public DateTime? EffectiveFrom { get; init; }
    public DateTime? EffectiveTo { get; init; }
}

public record CalculationParameterSearchQuery
{
    public string? ParameterName { get; init; }
    public ParameterScope? Scope { get; init; }
    public Guid? ScopeId { get; init; }
    public bool? IsEffective { get; init; }
    public DateTime? AsOfDate { get; init; }
}

// ============== Configuration Category DTOs ==============

public record ConfigurationCategoryDto
{
    public ConfigurationCategory Category { get; init; }
    public required string Name { get; init; }
    public string? Description { get; init; }
    public int SettingsCount { get; init; }
}

// ============== Bulk Operations DTOs ==============

public record BulkUpdateConfigurationRequest
{
    public IReadOnlyList<ConfigurationUpdateItem> Updates { get; init; } = new List<ConfigurationUpdateItem>();
}

public record ConfigurationUpdateItem
{
    public required string Key { get; init; }
    public required string Value { get; init; }
}

public record BulkUpdateResult
{
    public int TotalRequested { get; init; }
    public int Succeeded { get; init; }
    public int Failed { get; init; }
    public IReadOnlyList<BulkUpdateError> Errors { get; init; } = new List<BulkUpdateError>();
}

public record BulkUpdateError
{
    public required string Key { get; init; }
    public required string ErrorMessage { get; init; }
}

// ============== Configuration Export/Import DTOs ==============

public record ConfigurationExportDto
{
    public DateTime ExportedAt { get; init; }
    public string? ExportedBy { get; init; }
    public IReadOnlyList<SystemConfigurationDto> Configurations { get; init; } = new List<SystemConfigurationDto>();
    public IReadOnlyList<FeatureFlagDto> FeatureFlags { get; init; } = new List<FeatureFlagDto>();
    public IReadOnlyList<CalculationParameterDto> CalculationParameters { get; init; } = new List<CalculationParameterDto>();
}

public record ConfigurationImportRequest
{
    public IReadOnlyList<CreateConfigurationRequest> Configurations { get; init; } = new List<CreateConfigurationRequest>();
    public IReadOnlyList<CreateFeatureFlagRequest> FeatureFlags { get; init; } = new List<CreateFeatureFlagRequest>();
    public IReadOnlyList<CreateCalculationParameterRequest> CalculationParameters { get; init; } = new List<CreateCalculationParameterRequest>();
    public bool OverwriteExisting { get; init; }
}

public record ConfigurationImportResult
{
    public int ConfigurationsImported { get; init; }
    public int FeatureFlagsImported { get; init; }
    public int CalculationParametersImported { get; init; }
    public int Skipped { get; init; }
    public IReadOnlyList<string> Errors { get; init; } = new List<string>();
}

// ============== Configuration Summary DTOs ==============

public record ConfigurationSummaryDto
{
    public int TotalConfigurations { get; init; }
    public int TotalFeatureFlags { get; init; }
    public int EnabledFeatureFlags { get; init; }
    public int TotalEmailTemplates { get; init; }
    public int ActiveEmailTemplates { get; init; }
    public int TotalCalculationParameters { get; init; }
    public int EffectiveCalculationParameters { get; init; }
    public IReadOnlyList<ConfigurationCategoryDto> ByCategory { get; init; } = new List<ConfigurationCategoryDto>();
    public DateTime? LastModified { get; init; }
}

// ============== Configuration History DTOs ==============

public record ConfigurationHistoryDto
{
    public Guid Id { get; init; }
    public required string Key { get; init; }
    public string? OldValue { get; init; }
    public required string NewValue { get; init; }
    public required string ChangedBy { get; init; }
    public DateTime ChangedAt { get; init; }
    public string? Reason { get; init; }
}

public record ConfigurationHistorySearchQuery
{
    public string? Key { get; init; }
    public string? ChangedBy { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 50;
}
