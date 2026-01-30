using Asp.Versioning;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Dorise.Incentive.Api.Documentation;

/// <summary>
/// API versioning configuration.
/// "When I grow up, I want to be a principal or a caterpillar." - APIs grow through versions!
/// </summary>
public static class ApiVersioningConfiguration
{
    /// <summary>
    /// Adds API versioning services.
    /// </summary>
    public static IServiceCollection AddApiVersioningServices(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            // Default version when not specified
            options.DefaultApiVersion = new ApiVersion(1, 0);

            // Assume default version when not specified
            options.AssumeDefaultVersionWhenUnspecified = true;

            // Report supported versions in response headers
            options.ReportApiVersions = true;

            // Read version from URL segment, query string, or header
            options.ApiVersionReader = ApiVersionReader.Combine(
                new UrlSegmentApiVersionReader(),
                new QueryStringApiVersionReader("api-version"),
                new HeaderApiVersionReader("X-Api-Version")
            );
        })
        .AddApiExplorer(options =>
        {
            // Format version as 'v'major[.minor][-status]
            options.GroupNameFormat = "'v'VVV";

            // Substitute the version in route templates
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}

/// <summary>
/// Swagger options configuration for API versioning.
/// </summary>
public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider? _provider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider? provider = null)
    {
        _provider = provider;
    }

    public void Configure(SwaggerGenOptions options)
    {
        // Add a swagger document for each discovered API version
        if (_provider != null)
        {
            foreach (var description in _provider.ApiVersionDescriptions)
            {
                options.SwaggerDoc(description.GroupName, CreateInfoForApiVersion(description));
            }
        }
    }

    private static Microsoft.OpenApi.Models.OpenApiInfo CreateInfoForApiVersion(
        ApiVersionDescription description)
    {
        var info = new Microsoft.OpenApi.Models.OpenApiInfo
        {
            Title = "Dorise Sales Incentive Framework API",
            Version = description.ApiVersion.ToString(),
            Description = "API for the Dorise Sales Incentive Framework.",
            Contact = new Microsoft.OpenApi.Models.OpenApiContact
            {
                Name = "DSIF Support",
                Email = "support@dorise.com"
            }
        };

        if (description.IsDeprecated)
        {
            info.Description += " **This API version has been deprecated.**";
        }

        return info;
    }
}

/// <summary>
/// API version constants.
/// </summary>
public static class ApiVersions
{
    /// <summary>
    /// Version 1.0
    /// </summary>
    public const string V1 = "1.0";

    /// <summary>
    /// Version 2.0 (future)
    /// </summary>
    public const string V2 = "2.0";
}

/// <summary>
/// API route constants.
/// </summary>
public static class ApiRoutes
{
    private const string Root = "api";
    private const string Version = "v{version:apiVersion}";
    private const string Base = $"{Root}/{Version}";

    public static class Employees
    {
        public const string GetAll = $"{Base}/employees";
        public const string GetById = $"{Base}/employees/{{id}}";
        public const string Create = $"{Base}/employees";
        public const string Update = $"{Base}/employees/{{id}}";
        public const string Delete = $"{Base}/employees/{{id}}";
        public const string Search = $"{Base}/employees/search";
    }

    public static class Departments
    {
        public const string GetAll = $"{Base}/departments";
        public const string GetById = $"{Base}/departments/{{id}}";
        public const string GetHierarchy = $"{Base}/departments/hierarchy";
    }

    public static class IncentivePlans
    {
        public const string GetAll = $"{Base}/incentive-plans";
        public const string GetById = $"{Base}/incentive-plans/{{id}}";
        public const string GetSlabs = $"{Base}/incentive-plans/{{id}}/slabs";
    }

    public static class Calculations
    {
        public const string GetAll = $"{Base}/calculations";
        public const string GetById = $"{Base}/calculations/{{id}}";
        public const string Calculate = $"{Base}/calculations/calculate";
        public const string BatchCalculate = $"{Base}/calculations/batch";
    }

    public static class Approvals
    {
        public const string GetPending = $"{Base}/approvals/pending";
        public const string Approve = $"{Base}/approvals/{{id}}/approve";
        public const string Reject = $"{Base}/approvals/{{id}}/reject";
    }
}
