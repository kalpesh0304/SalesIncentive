using Dorise.Incentive.Domain.Common;

namespace Dorise.Incentive.Domain.Entities;

/// <summary>
/// Represents a system configuration setting.
/// "I bent my Wookie!" - Configure everything so nothing bends!
/// </summary>
public class SystemConfiguration : AuditableEntity
{
    public string Key { get; private set; } = null!;
    public string Value { get; private set; } = null!;
    public string? Description { get; private set; }
    public ConfigurationCategory Category { get; private set; }
    public ConfigurationDataType DataType { get; private set; }
    public bool IsEncrypted { get; private set; }
    public bool IsReadOnly { get; private set; }
    public string? ValidationRegex { get; private set; }
    public string? DefaultValue { get; private set; }
    public DateTime? EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }

    private SystemConfiguration() { } // EF Core constructor

    public static SystemConfiguration Create(
        string key,
        string value,
        ConfigurationCategory category,
        ConfigurationDataType dataType = ConfigurationDataType.String,
        string? description = null,
        bool isEncrypted = false,
        bool isReadOnly = false,
        string? validationRegex = null,
        string? defaultValue = null)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Configuration key is required", nameof(key));

        return new SystemConfiguration
        {
            Id = Guid.NewGuid(),
            Key = key.Trim(),
            Value = value ?? string.Empty,
            Category = category,
            DataType = dataType,
            Description = description?.Trim(),
            IsEncrypted = isEncrypted,
            IsReadOnly = isReadOnly,
            ValidationRegex = validationRegex,
            DefaultValue = defaultValue
        };
    }

    public void UpdateValue(string newValue)
    {
        if (IsReadOnly)
            throw new InvalidOperationException($"Configuration '{Key}' is read-only");

        Value = newValue ?? string.Empty;
    }

    public void SetEffectivePeriod(DateTime? from, DateTime? to)
    {
        if (from.HasValue && to.HasValue && from > to)
            throw new ArgumentException("Effective from date must be before effective to date");

        EffectiveFrom = from;
        EffectiveTo = to;
    }

    public bool IsEffective(DateTime? asOf = null)
    {
        var checkDate = asOf ?? DateTime.UtcNow;

        if (EffectiveFrom.HasValue && checkDate < EffectiveFrom.Value)
            return false;

        if (EffectiveTo.HasValue && checkDate > EffectiveTo.Value)
            return false;

        return true;
    }

    public T GetTypedValue<T>()
    {
        return DataType switch
        {
            ConfigurationDataType.Boolean => (T)(object)bool.Parse(Value),
            ConfigurationDataType.Integer => (T)(object)int.Parse(Value),
            ConfigurationDataType.Decimal => (T)(object)decimal.Parse(Value),
            ConfigurationDataType.DateTime => (T)(object)DateTime.Parse(Value),
            ConfigurationDataType.Json => System.Text.Json.JsonSerializer.Deserialize<T>(Value)!,
            _ => (T)(object)Value
        };
    }
}

/// <summary>
/// Represents a feature flag for enabling/disabling features.
/// "Me fail English? That's unpossible!" - Feature flags make the unpossible toggleable!
/// </summary>
public class FeatureFlag : AuditableEntity
{
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public bool IsEnabled { get; private set; }
    public FeatureFlagType FlagType { get; private set; }
    public string? EnabledForUsers { get; private set; } // Comma-separated user IDs
    public string? EnabledForRoles { get; private set; } // Comma-separated role names
    public int? RolloutPercentage { get; private set; } // For gradual rollout
    public DateTime? EnabledFrom { get; private set; }
    public DateTime? EnabledUntil { get; private set; }
    public string? Metadata { get; private set; } // JSON for additional config

    private FeatureFlag() { } // EF Core constructor

    public static FeatureFlag Create(
        string name,
        string? description = null,
        bool isEnabled = false,
        FeatureFlagType flagType = FeatureFlagType.Boolean)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Feature flag name is required", nameof(name));

        return new FeatureFlag
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Description = description?.Trim(),
            IsEnabled = isEnabled,
            FlagType = flagType
        };
    }

    public void Enable()
    {
        IsEnabled = true;
    }

    public void Disable()
    {
        IsEnabled = false;
    }

    public void Toggle()
    {
        IsEnabled = !IsEnabled;
    }

    public void SetRolloutPercentage(int percentage)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be between 0 and 100");

        RolloutPercentage = percentage;
        FlagType = FeatureFlagType.Percentage;
    }

    public void EnableForUsers(IEnumerable<Guid> userIds)
    {
        EnabledForUsers = string.Join(",", userIds);
        FlagType = FeatureFlagType.UserBased;
    }

    public void EnableForRoles(IEnumerable<string> roles)
    {
        EnabledForRoles = string.Join(",", roles);
        FlagType = FeatureFlagType.RoleBased;
    }

    public void SetSchedule(DateTime? from, DateTime? until)
    {
        if (from.HasValue && until.HasValue && from > until)
            throw new ArgumentException("Enabled from date must be before enabled until date");

        EnabledFrom = from;
        EnabledUntil = until;
        if (from.HasValue || until.HasValue)
            FlagType = FeatureFlagType.TimeBased;
    }

    public void SetMetadata(string json)
    {
        Metadata = json;
    }

    public bool IsEnabledForUser(Guid userId, IEnumerable<string>? userRoles = null)
    {
        if (!IsEnabled)
            return false;

        // Check time-based conditions
        var now = DateTime.UtcNow;
        if (EnabledFrom.HasValue && now < EnabledFrom.Value)
            return false;
        if (EnabledUntil.HasValue && now > EnabledUntil.Value)
            return false;

        return FlagType switch
        {
            FeatureFlagType.Boolean => true,
            FeatureFlagType.UserBased => IsUserInList(userId),
            FeatureFlagType.RoleBased => IsRoleInList(userRoles),
            FeatureFlagType.Percentage => IsInRolloutPercentage(userId),
            FeatureFlagType.TimeBased => true, // Already checked above
            _ => true
        };
    }

    private bool IsUserInList(Guid userId)
    {
        if (string.IsNullOrEmpty(EnabledForUsers))
            return false;

        var userIds = EnabledForUsers.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return userIds.Contains(userId.ToString());
    }

    private bool IsRoleInList(IEnumerable<string>? userRoles)
    {
        if (string.IsNullOrEmpty(EnabledForRoles) || userRoles == null)
            return false;

        var enabledRoles = EnabledForRoles.Split(',', StringSplitOptions.RemoveEmptyEntries);
        return userRoles.Any(r => enabledRoles.Contains(r, StringComparer.OrdinalIgnoreCase));
    }

    private bool IsInRolloutPercentage(Guid userId)
    {
        if (!RolloutPercentage.HasValue)
            return false;

        // Use hash of user ID for consistent rollout
        var hash = Math.Abs(userId.GetHashCode());
        var bucket = hash % 100;
        return bucket < RolloutPercentage.Value;
    }
}

/// <summary>
/// Represents an email template configuration.
/// "I'm learnding!" - Templates help emails learn their content!
/// </summary>
public class EmailTemplate : AuditableEntity
{
    public string TemplateName { get; private set; } = null!;
    public string Subject { get; private set; } = null!;
    public string Body { get; private set; } = null!;
    public bool IsHtml { get; private set; }
    public bool IsActive { get; private set; }
    public string? Description { get; private set; }
    public string? AvailablePlaceholders { get; private set; } // JSON array of placeholder names

    private EmailTemplate() { } // EF Core constructor

    public static EmailTemplate Create(
        string templateName,
        string subject,
        string body,
        bool isHtml = true,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(templateName))
            throw new ArgumentException("Template name is required", nameof(templateName));

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject is required", nameof(subject));

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body is required", nameof(body));

        return new EmailTemplate
        {
            Id = Guid.NewGuid(),
            TemplateName = templateName.Trim(),
            Subject = subject,
            Body = body,
            IsHtml = isHtml,
            IsActive = true,
            Description = description?.Trim()
        };
    }

    public void UpdateContent(string subject, string body)
    {
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        Body = body ?? throw new ArgumentNullException(nameof(body));
    }

    public void SetPlaceholders(IEnumerable<string> placeholders)
    {
        AvailablePlaceholders = System.Text.Json.JsonSerializer.Serialize(placeholders);
    }

    public void Activate() => IsActive = true;
    public void Deactivate() => IsActive = false;

    public string RenderSubject(IDictionary<string, string> values)
    {
        return ReplacePlaceholders(Subject, values);
    }

    public string RenderBody(IDictionary<string, string> values)
    {
        return ReplacePlaceholders(Body, values);
    }

    private static string ReplacePlaceholders(string template, IDictionary<string, string> values)
    {
        var result = template;
        foreach (var kvp in values)
        {
            result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }
        return result;
    }
}

/// <summary>
/// Represents calculation parameters configuration.
/// "When I grow up, I'm going to Bovine University!" - Parameters grow your calculations!
/// </summary>
public class CalculationParameter : AuditableEntity
{
    public string ParameterName { get; private set; } = null!;
    public decimal Value { get; private set; }
    public string? Description { get; private set; }
    public ParameterScope Scope { get; private set; }
    public Guid? ScopeId { get; private set; } // Department ID, Plan ID, etc.
    public DateTime EffectiveFrom { get; private set; }
    public DateTime? EffectiveTo { get; private set; }

    private CalculationParameter() { } // EF Core constructor

    public static CalculationParameter Create(
        string parameterName,
        decimal value,
        ParameterScope scope = ParameterScope.Global,
        Guid? scopeId = null,
        DateTime? effectiveFrom = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
            throw new ArgumentException("Parameter name is required", nameof(parameterName));

        return new CalculationParameter
        {
            Id = Guid.NewGuid(),
            ParameterName = parameterName.Trim(),
            Value = value,
            Scope = scope,
            ScopeId = scopeId,
            EffectiveFrom = effectiveFrom ?? DateTime.UtcNow,
            Description = description?.Trim()
        };
    }

    public void UpdateValue(decimal newValue)
    {
        Value = newValue;
    }

    public void SetEffectivePeriod(DateTime from, DateTime? to = null)
    {
        if (to.HasValue && from > to.Value)
            throw new ArgumentException("Effective from date must be before effective to date");

        EffectiveFrom = from;
        EffectiveTo = to;
    }

    public bool IsEffective(DateTime? asOf = null)
    {
        var checkDate = asOf ?? DateTime.UtcNow;
        if (checkDate < EffectiveFrom) return false;
        if (EffectiveTo.HasValue && checkDate > EffectiveTo.Value) return false;
        return true;
    }
}

// ============== Enums ==============

public enum ConfigurationCategory
{
    General,
    Calculation,
    Approval,
    Notification,
    Integration,
    Security,
    Reporting,
    Display,
    Email,
    Performance
}

public enum ConfigurationDataType
{
    String,
    Boolean,
    Integer,
    Decimal,
    DateTime,
    Json,
    ConnectionString,
    Url
}

public enum FeatureFlagType
{
    Boolean,
    UserBased,
    RoleBased,
    Percentage,
    TimeBased
}

public enum ParameterScope
{
    Global,
    Department,
    Plan,
    Role,
    Employee
}

// ============== Well-Known Configuration Keys ==============

public static class ConfigurationKeys
{
    // General
    public const string CompanyName = "General.CompanyName";
    public const string FiscalYearStartMonth = "General.FiscalYearStartMonth";
    public const string DefaultCurrency = "General.DefaultCurrency";
    public const string DefaultTimezone = "General.DefaultTimezone";
    public const string DefaultLanguage = "General.DefaultLanguage";

    // Calculation
    public const string CalculationPrecision = "Calculation.Precision";
    public const string RoundingMode = "Calculation.RoundingMode";
    public const string MinimumPayout = "Calculation.MinimumPayout";
    public const string MaximumPayout = "Calculation.MaximumPayout";
    public const string ProrationMethod = "Calculation.ProrationMethod";
    public const string BatchSize = "Calculation.BatchSize";

    // Approval
    public const string ApprovalLevels = "Approval.Levels";
    public const string AutoApproveThreshold = "Approval.AutoApproveThreshold";
    public const string ApprovalTimeoutDays = "Approval.TimeoutDays";
    public const string RequireCommentsOnReject = "Approval.RequireCommentsOnReject";

    // Notification
    public const string EmailSenderAddress = "Notification.EmailSenderAddress";
    public const string EmailSenderName = "Notification.EmailSenderName";
    public const string EnableEmailNotifications = "Notification.EnableEmail";
    public const string EnablePushNotifications = "Notification.EnablePush";
    public const string DigestFrequency = "Notification.DigestFrequency";

    // Integration
    public const string ErpApiUrl = "Integration.ErpApiUrl";
    public const string HrApiUrl = "Integration.HrApiUrl";
    public const string PayrollApiUrl = "Integration.PayrollApiUrl";
    public const string IntegrationRetryCount = "Integration.RetryCount";
    public const string IntegrationTimeoutSeconds = "Integration.TimeoutSeconds";

    // Security
    public const string SessionTimeoutMinutes = "Security.SessionTimeoutMinutes";
    public const string MaxLoginAttempts = "Security.MaxLoginAttempts";
    public const string PasswordMinLength = "Security.PasswordMinLength";
    public const string RequireMfa = "Security.RequireMfa";

    // Reporting
    public const string DefaultExportFormat = "Reporting.DefaultExportFormat";
    public const string MaxExportRows = "Reporting.MaxExportRows";
    public const string ReportRetentionDays = "Reporting.RetentionDays";

    // Performance
    public const string CacheDurationMinutes = "Performance.CacheDurationMinutes";
    public const string EnableQueryCaching = "Performance.EnableQueryCaching";
    public const string MaxConcurrentCalculations = "Performance.MaxConcurrentCalculations";
}

// ============== Well-Known Feature Flags ==============

public static class FeatureFlags
{
    public const string NewDashboard = "Feature.NewDashboard";
    public const string AdvancedReporting = "Feature.AdvancedReporting";
    public const string BulkOperations = "Feature.BulkOperations";
    public const string AutoApproval = "Feature.AutoApproval";
    public const string MultiCurrency = "Feature.MultiCurrency";
    public const string RealTimeCalculation = "Feature.RealTimeCalculation";
    public const string ExternalIntegrations = "Feature.ExternalIntegrations";
    public const string AuditDashboard = "Feature.AuditDashboard";
    public const string AdvancedSecurity = "Feature.AdvancedSecurity";
    public const string ExportToPdf = "Feature.ExportToPdf";
    public const string MobileApp = "Feature.MobileApp";
    public const string ApiV2 = "Feature.ApiV2";
}

// ============== Well-Known Email Templates ==============

public static class EmailTemplates
{
    public const string CalculationComplete = "Email.CalculationComplete";
    public const string ApprovalRequired = "Email.ApprovalRequired";
    public const string ApprovalApproved = "Email.ApprovalApproved";
    public const string ApprovalRejected = "Email.ApprovalRejected";
    public const string PaymentProcessed = "Email.PaymentProcessed";
    public const string WelcomeEmployee = "Email.WelcomeEmployee";
    public const string PasswordReset = "Email.PasswordReset";
    public const string RoleAssigned = "Email.RoleAssigned";
    public const string ReportGenerated = "Email.ReportGenerated";
    public const string SystemAlert = "Email.SystemAlert";
}

// ============== Well-Known Calculation Parameters ==============

public static class CalculationParameters
{
    public const string TaxRate = "Param.TaxRate";
    public const string SocialSecurityRate = "Param.SocialSecurityRate";
    public const string PensionContributionRate = "Param.PensionContributionRate";
    public const string MinimumWage = "Param.MinimumWage";
    public const string OvertimeMultiplier = "Param.OvertimeMultiplier";
    public const string BonusCapPercentage = "Param.BonusCapPercentage";
    public const string ClawbackPeriodDays = "Param.ClawbackPeriodDays";
}
