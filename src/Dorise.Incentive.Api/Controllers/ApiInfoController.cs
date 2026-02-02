using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace Dorise.Incentive.Api.Controllers;

/// <summary>
/// Controller for API information and metadata.
/// "Hi, Super Nintendo Chalmers!" - Super information about our API!
/// </summary>
[ApiController]
[Route("api")]
[AllowAnonymous]
public class ApiInfoController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;

    public ApiInfoController(
        IWebHostEnvironment environment,
        IConfiguration configuration)
    {
        _environment = environment;
        _configuration = configuration;
    }

    /// <summary>
    /// Get API information and status.
    /// </summary>
    /// <returns>API information</returns>
    [HttpGet]
    [HttpGet("info")]
    [ProducesResponseType(typeof(ApiInfo), StatusCodes.Status200OK)]
    public IActionResult GetApiInfo()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;

        var info = new ApiInfo
        {
            Name = "Dorise Sales Incentive Framework API",
            Version = $"{version?.Major}.{version?.Minor}.{version?.Build}",
            Description = "API for managing sales incentive calculations, employee data, and approval workflows.",
            Environment = _environment.EnvironmentName,
            Documentation = $"{Request.Scheme}://{Request.Host}/api-docs",
            Status = "Running",
            Timestamp = DateTime.UtcNow,
            Endpoints = new EndpointInfo
            {
                Employees = "/api/v1/employees",
                Departments = "/api/v1/departments",
                IncentivePlans = "/api/v1/incentive-plans",
                Calculations = "/api/v1/calculations",
                Approvals = "/api/v1/approvals",
                Reports = "/api/v1/reports",
                Configuration = "/api/v1/configuration",
                Jobs = "/api/v1/jobs",
                DataTransfer = "/api/v1/datatransfer",
                Performance = "/api/v1/performance",
                Audit = "/api/v1/audit"
            },
            Links = new ApiLinks
            {
                Self = $"{Request.Scheme}://{Request.Host}/api",
                Documentation = $"{Request.Scheme}://{Request.Host}/api-docs",
                Health = $"{Request.Scheme}://{Request.Host}/api/performance/health",
                Support = "https://dorise.com/support"
            }
        };

        return Ok(info);
    }

    /// <summary>
    /// Get API version information.
    /// </summary>
    [HttpGet("version")]
    [ProducesResponseType(typeof(VersionInfo), StatusCodes.Status200OK)]
    public IActionResult GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;
        var buildDate = System.IO.File.GetLastWriteTime(assembly.Location);

        return Ok(new VersionInfo
        {
            ApiVersion = $"{version?.Major}.{version?.Minor}",
            FullVersion = version?.ToString() ?? "1.0.0.0",
            BuildNumber = version?.Build ?? 0,
            BuildDate = buildDate,
            Runtime = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription,
            SupportedVersions = new[] { "v1" },
            DeprecatedVersions = Array.Empty<string>()
        });
    }

    /// <summary>
    /// Get available API endpoints.
    /// </summary>
    [HttpGet("endpoints")]
    [ProducesResponseType(typeof(EndpointCatalog), StatusCodes.Status200OK)]
    public IActionResult GetEndpoints()
    {
        var catalog = new EndpointCatalog
        {
            Categories = new List<EndpointCategory>
            {
                new EndpointCategory
                {
                    Name = "Employees",
                    Description = "Employee management operations",
                    Endpoints = new List<EndpointDetail>
                    {
                        new() { Method = "GET", Path = "/api/v1/employees", Description = "Get all employees with pagination" },
                        new() { Method = "GET", Path = "/api/v1/employees/{id}", Description = "Get employee by ID" },
                        new() { Method = "POST", Path = "/api/v1/employees", Description = "Create a new employee" },
                        new() { Method = "PUT", Path = "/api/v1/employees/{id}", Description = "Update an employee" },
                        new() { Method = "DELETE", Path = "/api/v1/employees/{id}", Description = "Delete an employee" }
                    }
                },
                new EndpointCategory
                {
                    Name = "Calculations",
                    Description = "Incentive calculation operations",
                    Endpoints = new List<EndpointDetail>
                    {
                        new() { Method = "GET", Path = "/api/v1/calculations", Description = "Get all calculations" },
                        new() { Method = "GET", Path = "/api/v1/calculations/{id}", Description = "Get calculation by ID" },
                        new() { Method = "POST", Path = "/api/v1/calculations/calculate", Description = "Run calculation for employee" },
                        new() { Method = "POST", Path = "/api/v1/calculations/batch", Description = "Run batch calculation" }
                    }
                },
                new EndpointCategory
                {
                    Name = "Approvals",
                    Description = "Approval workflow operations",
                    Endpoints = new List<EndpointDetail>
                    {
                        new() { Method = "GET", Path = "/api/v1/approvals/pending", Description = "Get pending approvals" },
                        new() { Method = "POST", Path = "/api/v1/approvals/{id}/approve", Description = "Approve a calculation" },
                        new() { Method = "POST", Path = "/api/v1/approvals/{id}/reject", Description = "Reject a calculation" }
                    }
                },
                new EndpointCategory
                {
                    Name = "Data Transfer",
                    Description = "Import and export operations",
                    Endpoints = new List<EndpointDetail>
                    {
                        new() { Method = "POST", Path = "/api/v1/datatransfer/imports/upload", Description = "Upload file for import" },
                        new() { Method = "POST", Path = "/api/v1/datatransfer/exports", Description = "Create an export" },
                        new() { Method = "GET", Path = "/api/v1/datatransfer/exports/{id}/download", Description = "Download an export" }
                    }
                },
                new EndpointCategory
                {
                    Name = "Performance",
                    Description = "Performance monitoring operations",
                    Endpoints = new List<EndpointDetail>
                    {
                        new() { Method = "GET", Path = "/api/v1/performance/metrics", Description = "Get current metrics" },
                        new() { Method = "GET", Path = "/api/v1/performance/health", Description = "Run health checks" },
                        new() { Method = "GET", Path = "/api/v1/performance/cache/metrics", Description = "Get cache metrics" }
                    }
                }
            }
        };

        return Ok(catalog);
    }

    /// <summary>
    /// Get API rate limits.
    /// </summary>
    [HttpGet("rate-limits")]
    [ProducesResponseType(typeof(RateLimitInfo), StatusCodes.Status200OK)]
    public IActionResult GetRateLimits()
    {
        return Ok(new RateLimitInfo
        {
            StandardLimit = new RateLimit { RequestsPerMinute = 100, BurstLimit = 150 },
            BulkOperations = new RateLimit { RequestsPerMinute = 10, BurstLimit = 15 },
            ExportOperations = new RateLimit { RequestsPerMinute = 5, BurstLimit = 10 },
            ImportOperations = new RateLimit { RequestsPerMinute = 5, BurstLimit = 10 },
            Note = "Rate limits are per user/API key. Contact support for increased limits."
        });
    }
}

// ==================== Response Models ====================

public record ApiInfo
{
    public string Name { get; init; } = null!;
    public string Version { get; init; } = null!;
    public string Description { get; init; } = null!;
    public string Environment { get; init; } = null!;
    public string Documentation { get; init; } = null!;
    public string Status { get; init; } = null!;
    public DateTime Timestamp { get; init; }
    public EndpointInfo Endpoints { get; init; } = new();
    public ApiLinks Links { get; init; } = new();
}

public record EndpointInfo
{
    public string Employees { get; init; } = null!;
    public string Departments { get; init; } = null!;
    public string IncentivePlans { get; init; } = null!;
    public string Calculations { get; init; } = null!;
    public string Approvals { get; init; } = null!;
    public string Reports { get; init; } = null!;
    public string Configuration { get; init; } = null!;
    public string Jobs { get; init; } = null!;
    public string DataTransfer { get; init; } = null!;
    public string Performance { get; init; } = null!;
    public string Audit { get; init; } = null!;
}

public record ApiLinks
{
    public string Self { get; init; } = null!;
    public string Documentation { get; init; } = null!;
    public string Health { get; init; } = null!;
    public string Support { get; init; } = null!;
}

public record VersionInfo
{
    public string ApiVersion { get; init; } = null!;
    public string FullVersion { get; init; } = null!;
    public int BuildNumber { get; init; }
    public DateTime BuildDate { get; init; }
    public string Runtime { get; init; } = null!;
    public string[] SupportedVersions { get; init; } = Array.Empty<string>();
    public string[] DeprecatedVersions { get; init; } = Array.Empty<string>();
}

public record EndpointCatalog
{
    public List<EndpointCategory> Categories { get; init; } = new();
}

public record EndpointCategory
{
    public string Name { get; init; } = null!;
    public string Description { get; init; } = null!;
    public List<EndpointDetail> Endpoints { get; init; } = new();
}

public record EndpointDetail
{
    public string Method { get; init; } = null!;
    public string Path { get; init; } = null!;
    public string Description { get; init; } = null!;
}

public record RateLimitInfo
{
    public RateLimit StandardLimit { get; init; } = new();
    public RateLimit BulkOperations { get; init; } = new();
    public RateLimit ExportOperations { get; init; } = new();
    public RateLimit ImportOperations { get; init; } = new();
    public string Note { get; init; } = null!;
}

public record RateLimit
{
    public int RequestsPerMinute { get; init; }
    public int BurstLimit { get; init; }
}
