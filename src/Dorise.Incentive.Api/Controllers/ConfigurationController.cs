using Dorise.Incentive.Application.Configuration.DTOs;
using Dorise.Incentive.Application.Configuration.Services;
using Dorise.Incentive.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for system configuration, feature flags, and parameters.
/// "I sleep in a drawer!" - Configurations tucked safely in the system!
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize]
[Produces("application/json")]
public class ConfigurationController : ControllerBase
{
    private readonly IConfigurationService _configurationService;
    private readonly IFeatureFlagService _featureFlagService;
    private readonly IEmailTemplateService _emailTemplateService;
    private readonly ICalculationParameterService _parameterService;
    private readonly IConfigurationExportService _exportService;
    private readonly ILogger<ConfigurationController> _logger;

    public ConfigurationController(
        IConfigurationService configurationService,
        IFeatureFlagService featureFlagService,
        IEmailTemplateService emailTemplateService,
        ICalculationParameterService parameterService,
        IConfigurationExportService exportService,
        ILogger<ConfigurationController> logger)
    {
        _configurationService = configurationService;
        _featureFlagService = featureFlagService;
        _emailTemplateService = emailTemplateService;
        _parameterService = parameterService;
        _exportService = exportService;
        _logger = logger;
    }

    // ============== System Configuration ==============

    /// <summary>
    /// Get all configurations.
    /// </summary>
    [HttpGet("settings")]
    [ProducesResponseType(typeof(IReadOnlyList<SystemConfigurationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllConfigurations(CancellationToken cancellationToken)
    {
        var configurations = await _configurationService.GetAllConfigurationsAsync(cancellationToken);
        return Ok(configurations);
    }

    /// <summary>
    /// Get configuration by key.
    /// </summary>
    [HttpGet("settings/{key}")]
    [ProducesResponseType(typeof(SystemConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConfiguration(string key, CancellationToken cancellationToken)
    {
        var config = await _configurationService.GetConfigurationAsync(key, cancellationToken);
        return config == null ? NotFound() : Ok(config);
    }

    /// <summary>
    /// Get configuration value by key.
    /// </summary>
    [HttpGet("settings/{key}/value")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConfigurationValue(string key, CancellationToken cancellationToken)
    {
        var value = await _configurationService.GetValueAsync(key, cancellationToken);
        return value == null ? NotFound() : Ok(new { Key = key, Value = value });
    }

    /// <summary>
    /// Get configurations by category.
    /// </summary>
    [HttpGet("settings/category/{category}")]
    [ProducesResponseType(typeof(IReadOnlyList<SystemConfigurationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfigurationsByCategory(
        ConfigurationCategory category,
        CancellationToken cancellationToken)
    {
        var configurations = await _configurationService.GetConfigurationsByCategoryAsync(
            category, cancellationToken);
        return Ok(configurations);
    }

    /// <summary>
    /// Search configurations.
    /// </summary>
    [HttpGet("settings/search")]
    [ProducesResponseType(typeof(IReadOnlyList<SystemConfigurationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchConfigurations(
        [FromQuery] ConfigurationSearchQuery query,
        CancellationToken cancellationToken)
    {
        var configurations = await _configurationService.SearchConfigurationsAsync(
            query, cancellationToken);
        return Ok(configurations);
    }

    /// <summary>
    /// Create a new configuration.
    /// </summary>
    [HttpPost("settings")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(SystemConfigurationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateConfiguration(
        [FromBody] CreateConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var createdBy = User.Identity?.Name ?? "System";
            var config = await _configurationService.CreateConfigurationAsync(
                request, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetConfiguration), new { key = config.Key }, config);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Update a configuration.
    /// </summary>
    [HttpPut("settings/{key}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(SystemConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateConfiguration(
        string key,
        [FromBody] UpdateConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var modifiedBy = User.Identity?.Name ?? "System";
            var config = await _configurationService.UpdateConfigurationAsync(
                key, request, modifiedBy, cancellationToken);
            return Ok(config);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Bulk update configurations.
    /// </summary>
    [HttpPut("settings/bulk")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(BulkUpdateResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> BulkUpdateConfigurations(
        [FromBody] BulkUpdateConfigurationRequest request,
        CancellationToken cancellationToken)
    {
        var modifiedBy = User.Identity?.Name ?? "System";
        var result = await _configurationService.BulkUpdateConfigurationsAsync(
            request, modifiedBy, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Delete a configuration.
    /// </summary>
    [HttpDelete("settings/{key}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConfiguration(
        string key,
        CancellationToken cancellationToken)
    {
        try
        {
            var deletedBy = User.Identity?.Name ?? "System";
            await _configurationService.DeleteConfigurationAsync(key, deletedBy, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Reset configuration to default value.
    /// </summary>
    [HttpPost("settings/{key}/reset")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(SystemConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResetToDefault(string key, CancellationToken cancellationToken)
    {
        try
        {
            var modifiedBy = User.Identity?.Name ?? "System";
            var config = await _configurationService.ResetToDefaultAsync(key, modifiedBy, cancellationToken);
            return Ok(config);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(new { Error = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Refresh configuration cache.
    /// </summary>
    [HttpPost("settings/refresh-cache")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RefreshCache(CancellationToken cancellationToken)
    {
        await _configurationService.RefreshCacheAsync(cancellationToken);
        return NoContent();
    }

    // ============== Feature Flags ==============

    /// <summary>
    /// Get all feature flags.
    /// </summary>
    [HttpGet("features")]
    [ProducesResponseType(typeof(IReadOnlyList<FeatureFlagDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllFeatureFlags(CancellationToken cancellationToken)
    {
        var flags = await _featureFlagService.GetAllFeatureFlagsAsync(cancellationToken);
        return Ok(flags);
    }

    /// <summary>
    /// Get enabled feature flags.
    /// </summary>
    [HttpGet("features/enabled")]
    [ProducesResponseType(typeof(IReadOnlyList<FeatureFlagSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEnabledFeatureFlags(CancellationToken cancellationToken)
    {
        var flags = await _featureFlagService.GetEnabledFlagsAsync(cancellationToken);
        return Ok(flags);
    }

    /// <summary>
    /// Get feature flag by name.
    /// </summary>
    [HttpGet("features/{name}")]
    [ProducesResponseType(typeof(FeatureFlagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFeatureFlag(string name, CancellationToken cancellationToken)
    {
        var flag = await _featureFlagService.GetFeatureFlagAsync(name, cancellationToken);
        return flag == null ? NotFound() : Ok(flag);
    }

    /// <summary>
    /// Check if feature is enabled.
    /// </summary>
    [HttpGet("features/{name}/check")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckFeatureFlag(string name, CancellationToken cancellationToken)
    {
        var isEnabled = await _featureFlagService.IsEnabledAsync(name, cancellationToken);
        return Ok(new { Name = name, IsEnabled = isEnabled });
    }

    /// <summary>
    /// Check multiple feature flags.
    /// </summary>
    [HttpPost("features/check")]
    [ProducesResponseType(typeof(IDictionary<string, bool>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckMultipleFeatureFlags(
        [FromBody] IEnumerable<string> flagNames,
        CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        var userRoles = User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value);

        var results = await _featureFlagService.CheckFlagsAsync(
            flagNames, userId, userRoles, cancellationToken);
        return Ok(results);
    }

    /// <summary>
    /// Create a new feature flag.
    /// </summary>
    [HttpPost("features")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(FeatureFlagDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateFeatureFlag(
        [FromBody] CreateFeatureFlagRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var createdBy = User.Identity?.Name ?? "System";
            var flag = await _featureFlagService.CreateFeatureFlagAsync(
                request, createdBy, cancellationToken);
            return CreatedAtAction(nameof(GetFeatureFlag), new { name = flag.Name }, flag);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Update a feature flag.
    /// </summary>
    [HttpPut("features/{name}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(FeatureFlagDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateFeatureFlag(
        string name,
        [FromBody] UpdateFeatureFlagRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var modifiedBy = User.Identity?.Name ?? "System";
            var flag = await _featureFlagService.UpdateFeatureFlagAsync(
                name, request, modifiedBy, cancellationToken);
            return Ok(flag);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Enable a feature flag.
    /// </summary>
    [HttpPost("features/{name}/enable")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EnableFeatureFlag(string name, CancellationToken cancellationToken)
    {
        try
        {
            var modifiedBy = User.Identity?.Name ?? "System";
            await _featureFlagService.EnableFlagAsync(name, modifiedBy, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Disable a feature flag.
    /// </summary>
    [HttpPost("features/{name}/disable")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DisableFeatureFlag(string name, CancellationToken cancellationToken)
    {
        try
        {
            var modifiedBy = User.Identity?.Name ?? "System";
            await _featureFlagService.DisableFlagAsync(name, modifiedBy, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Toggle a feature flag.
    /// </summary>
    [HttpPost("features/{name}/toggle")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleFeatureFlag(string name, CancellationToken cancellationToken)
    {
        try
        {
            var modifiedBy = User.Identity?.Name ?? "System";
            var isEnabled = await _featureFlagService.ToggleFlagAsync(name, modifiedBy, cancellationToken);
            return Ok(new { Name = name, IsEnabled = isEnabled });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a feature flag.
    /// </summary>
    [HttpDelete("features/{name}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteFeatureFlag(string name, CancellationToken cancellationToken)
    {
        try
        {
            var deletedBy = User.Identity?.Name ?? "System";
            await _featureFlagService.DeleteFeatureFlagAsync(name, deletedBy, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    // ============== Email Templates ==============

    /// <summary>
    /// Get all email templates.
    /// </summary>
    [HttpGet("email-templates")]
    [ProducesResponseType(typeof(IReadOnlyList<EmailTemplateSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllEmailTemplates(CancellationToken cancellationToken)
    {
        var templates = await _emailTemplateService.GetAllTemplatesAsync(cancellationToken);
        return Ok(templates);
    }

    /// <summary>
    /// Get email template by name.
    /// </summary>
    [HttpGet("email-templates/{templateName}")]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmailTemplate(
        string templateName,
        CancellationToken cancellationToken)
    {
        var template = await _emailTemplateService.GetTemplateAsync(templateName, cancellationToken);
        return template == null ? NotFound() : Ok(template);
    }

    /// <summary>
    /// Create a new email template.
    /// </summary>
    [HttpPost("email-templates")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEmailTemplate(
        [FromBody] CreateEmailTemplateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var createdBy = User.Identity?.Name ?? "System";
            var template = await _emailTemplateService.CreateTemplateAsync(
                request, createdBy, cancellationToken);
            return CreatedAtAction(
                nameof(GetEmailTemplate),
                new { templateName = template.TemplateName },
                template);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Update an email template.
    /// </summary>
    [HttpPut("email-templates/{templateName}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(EmailTemplateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateEmailTemplate(
        string templateName,
        [FromBody] UpdateEmailTemplateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var modifiedBy = User.Identity?.Name ?? "System";
            var template = await _emailTemplateService.UpdateTemplateAsync(
                templateName, request, modifiedBy, cancellationToken);
            return Ok(template);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Preview an email template.
    /// </summary>
    [HttpGet("email-templates/{templateName}/preview")]
    [ProducesResponseType(typeof(RenderedEmailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PreviewEmailTemplate(
        string templateName,
        CancellationToken cancellationToken)
    {
        try
        {
            var preview = await _emailTemplateService.PreviewTemplateAsync(
                templateName, cancellationToken);
            return Ok(preview);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Render an email template with values.
    /// </summary>
    [HttpPost("email-templates/render")]
    [ProducesResponseType(typeof(RenderedEmailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RenderEmailTemplate(
        [FromBody] RenderEmailTemplateRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var rendered = await _emailTemplateService.RenderTemplateAsync(
                request, cancellationToken);
            return Ok(rendered);
        }
        catch (InvalidOperationException ex)
        {
            if (ex.Message.Contains("not found"))
                return NotFound(new { Error = ex.Message });
            return BadRequest(new { Error = ex.Message });
        }
    }

    // ============== Calculation Parameters ==============

    /// <summary>
    /// Get all calculation parameters.
    /// </summary>
    [HttpGet("parameters")]
    [ProducesResponseType(typeof(IReadOnlyList<CalculationParameterDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllParameters(CancellationToken cancellationToken)
    {
        var parameters = await _parameterService.GetAllParametersAsync(cancellationToken);
        return Ok(parameters);
    }

    /// <summary>
    /// Get parameter value.
    /// </summary>
    [HttpGet("parameters/{name}/value")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetParameterValue(
        string name,
        [FromQuery] ParameterScope scope = ParameterScope.Global,
        [FromQuery] Guid? scopeId = null,
        [FromQuery] DateTime? asOfDate = null,
        CancellationToken cancellationToken = default)
    {
        var value = await _parameterService.GetParameterValueAsync(
            name, scope, scopeId, asOfDate, cancellationToken);

        return value == null
            ? NotFound()
            : Ok(new { Name = name, Value = value, Scope = scope, ScopeId = scopeId });
    }

    /// <summary>
    /// Search calculation parameters.
    /// </summary>
    [HttpGet("parameters/search")]
    [ProducesResponseType(typeof(IReadOnlyList<CalculationParameterDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchParameters(
        [FromQuery] CalculationParameterSearchQuery query,
        CancellationToken cancellationToken)
    {
        var parameters = await _parameterService.SearchParametersAsync(query, cancellationToken);
        return Ok(parameters);
    }

    /// <summary>
    /// Create a new calculation parameter.
    /// </summary>
    [HttpPost("parameters")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(CalculationParameterDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateParameter(
        [FromBody] CreateCalculationParameterRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var createdBy = User.Identity?.Name ?? "System";
            var parameter = await _parameterService.CreateParameterAsync(
                request, createdBy, cancellationToken);
            return CreatedAtAction(
                nameof(GetParameterValue),
                new { name = parameter.ParameterName },
                parameter);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Update a calculation parameter.
    /// </summary>
    [HttpPut("parameters/{id:guid}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(CalculationParameterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateParameter(
        Guid id,
        [FromBody] UpdateCalculationParameterRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var modifiedBy = User.Identity?.Name ?? "System";
            var parameter = await _parameterService.UpdateParameterAsync(
                id, request, modifiedBy, cancellationToken);
            return Ok(parameter);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    /// <summary>
    /// Delete a calculation parameter.
    /// </summary>
    [HttpDelete("parameters/{id:guid}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteParameter(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var deletedBy = User.Identity?.Name ?? "System";
            await _parameterService.DeleteParameterAsync(id, deletedBy, cancellationToken);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { Error = ex.Message });
        }
    }

    // ============== Export/Import ==============

    /// <summary>
    /// Export all configurations.
    /// </summary>
    [HttpGet("export")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ConfigurationExportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportConfigurations(CancellationToken cancellationToken)
    {
        var exportedBy = User.Identity?.Name ?? "System";
        var export = await _exportService.ExportAllAsync(exportedBy, cancellationToken);
        return Ok(export);
    }

    /// <summary>
    /// Export configurations by category.
    /// </summary>
    [HttpGet("export/{category}")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ConfigurationExportDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportByCategory(
        ConfigurationCategory category,
        CancellationToken cancellationToken)
    {
        var exportedBy = User.Identity?.Name ?? "System";
        var export = await _exportService.ExportByCategoryAsync(category, exportedBy, cancellationToken);
        return Ok(export);
    }

    /// <summary>
    /// Import configurations.
    /// </summary>
    [HttpPost("import")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ConfigurationImportResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ImportConfigurations(
        [FromBody] ConfigurationImportRequest request,
        CancellationToken cancellationToken)
    {
        var importedBy = User.Identity?.Name ?? "System";
        var result = await _exportService.ImportAsync(request, importedBy, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Validate import without applying.
    /// </summary>
    [HttpPost("import/validate")]
    [Authorize(Roles = "Administrator")]
    [ProducesResponseType(typeof(ConfigurationImportResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> ValidateImport(
        [FromBody] ConfigurationImportRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _exportService.ValidateImportAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get configuration summary.
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ConfigurationSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSummary(CancellationToken cancellationToken)
    {
        var summary = await _exportService.GetSummaryAsync(cancellationToken);
        return Ok(summary);
    }

    // ============== Helper Methods ==============

    private Guid? GetCurrentUserId()
    {
        var claim = User.FindFirst("sub") ?? User.FindFirst("oid");
        return claim != null && Guid.TryParse(claim.Value, out var userId) ? userId : null;
    }
}
