using Swashbuckle.AspNetCore.Filters;

namespace Dorise.Incentive.Api.Documentation;

/// <summary>
/// Example request/response models for API documentation.
/// "The doctor said I wouldn't have so many nosebleeds if I kept my finger outta there."
/// - Examples keep documentation accurate!
/// </summary>

// ==================== Employee Examples ====================

public class CreateEmployeeRequestExample : IExamplesProvider<CreateEmployeeRequest>
{
    public CreateEmployeeRequest GetExamples()
    {
        return new CreateEmployeeRequest
        {
            EmployeeNumber = "EMP001",
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@dorise.com",
            HireDate = DateTime.Today.AddMonths(-6),
            DepartmentId = Guid.NewGuid(),
            JobTitle = "Sales Representative"
        };
    }
}

public class EmployeeResponseExample : IExamplesProvider<EmployeeResponse>
{
    public EmployeeResponse GetExamples()
    {
        return new EmployeeResponse
        {
            Id = Guid.NewGuid(),
            EmployeeNumber = "EMP001",
            FullName = "John Doe",
            Email = "john.doe@dorise.com",
            Department = "Sales",
            JobTitle = "Sales Representative",
            Status = "Active",
            HireDate = DateTime.Today.AddMonths(-6),
            CreatedAt = DateTime.UtcNow.AddMonths(-6)
        };
    }
}

// ==================== Calculation Examples ====================

public class CalculationResponseExample : IExamplesProvider<CalculationResponse>
{
    public CalculationResponse GetExamples()
    {
        return new CalculationResponse
        {
            Id = Guid.NewGuid(),
            EmployeeId = Guid.NewGuid(),
            EmployeeName = "John Doe",
            Period = "2025-01",
            SalesAmount = 75000.00m,
            TargetAmount = 50000.00m,
            AchievementPercentage = 150.0,
            IncentiveAmount = 3750.00m,
            Status = "PendingApproval",
            CalculatedAt = DateTime.UtcNow
        };
    }
}

// ==================== Error Response Examples ====================

public class ValidationErrorResponseExample : IExamplesProvider<ValidationErrorResponse>
{
    public ValidationErrorResponse GetExamples()
    {
        return new ValidationErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7807",
            Title = "Validation Error",
            Status = 400,
            Detail = "One or more validation errors occurred.",
            Instance = "/api/v1/employees",
            TraceId = "00-abc123def456-789xyz-00",
            Errors = new Dictionary<string, string[]>
            {
                { "EmployeeNumber", new[] { "Employee number is required." } },
                { "Email", new[] { "Invalid email format.", "Email must be a company email address." } }
            }
        };
    }
}

public class NotFoundErrorResponseExample : IExamplesProvider<NotFoundErrorResponse>
{
    public NotFoundErrorResponse GetExamples()
    {
        return new NotFoundErrorResponse
        {
            Type = "https://tools.ietf.org/html/rfc7807",
            Title = "Not Found",
            Status = 404,
            Detail = "Employee with id '12345678-1234-1234-1234-123456789012' was not found.",
            Instance = "/api/v1/employees/12345678-1234-1234-1234-123456789012",
            TraceId = "00-abc123def456-789xyz-00"
        };
    }
}

// ==================== Request/Response Models ====================

public record CreateEmployeeRequest
{
    /// <summary>
    /// Unique employee number (e.g., EMP001)
    /// </summary>
    /// <example>EMP001</example>
    public string EmployeeNumber { get; init; } = null!;

    /// <summary>
    /// Employee's first name
    /// </summary>
    /// <example>John</example>
    public string FirstName { get; init; } = null!;

    /// <summary>
    /// Employee's last name
    /// </summary>
    /// <example>Doe</example>
    public string LastName { get; init; } = null!;

    /// <summary>
    /// Company email address
    /// </summary>
    /// <example>john.doe@dorise.com</example>
    public string Email { get; init; } = null!;

    /// <summary>
    /// Date of hire
    /// </summary>
    /// <example>2024-07-15</example>
    public DateTime HireDate { get; init; }

    /// <summary>
    /// Department identifier
    /// </summary>
    public Guid? DepartmentId { get; init; }

    /// <summary>
    /// Job title
    /// </summary>
    /// <example>Sales Representative</example>
    public string? JobTitle { get; init; }
}

public record EmployeeResponse
{
    /// <summary>
    /// Unique identifier
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Employee number
    /// </summary>
    /// <example>EMP001</example>
    public string EmployeeNumber { get; init; } = null!;

    /// <summary>
    /// Full name
    /// </summary>
    /// <example>John Doe</example>
    public string FullName { get; init; } = null!;

    /// <summary>
    /// Email address
    /// </summary>
    /// <example>john.doe@dorise.com</example>
    public string Email { get; init; } = null!;

    /// <summary>
    /// Department name
    /// </summary>
    /// <example>Sales</example>
    public string? Department { get; init; }

    /// <summary>
    /// Job title
    /// </summary>
    /// <example>Sales Representative</example>
    public string? JobTitle { get; init; }

    /// <summary>
    /// Employment status
    /// </summary>
    /// <example>Active</example>
    public string Status { get; init; } = null!;

    /// <summary>
    /// Hire date
    /// </summary>
    public DateTime HireDate { get; init; }

    /// <summary>
    /// Record creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; init; }
}

public record CalculationResponse
{
    /// <summary>
    /// Calculation identifier
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Employee identifier
    /// </summary>
    public Guid EmployeeId { get; init; }

    /// <summary>
    /// Employee name
    /// </summary>
    /// <example>John Doe</example>
    public string EmployeeName { get; init; } = null!;

    /// <summary>
    /// Calculation period (YYYY-MM format)
    /// </summary>
    /// <example>2025-01</example>
    public string Period { get; init; } = null!;

    /// <summary>
    /// Total sales amount
    /// </summary>
    /// <example>75000.00</example>
    public decimal SalesAmount { get; init; }

    /// <summary>
    /// Target amount for the period
    /// </summary>
    /// <example>50000.00</example>
    public decimal TargetAmount { get; init; }

    /// <summary>
    /// Achievement percentage (sales/target * 100)
    /// </summary>
    /// <example>150.0</example>
    public double AchievementPercentage { get; init; }

    /// <summary>
    /// Calculated incentive amount
    /// </summary>
    /// <example>3750.00</example>
    public decimal IncentiveAmount { get; init; }

    /// <summary>
    /// Calculation status
    /// </summary>
    /// <example>PendingApproval</example>
    public string Status { get; init; } = null!;

    /// <summary>
    /// Calculation timestamp
    /// </summary>
    public DateTime CalculatedAt { get; init; }
}

public record ValidationErrorResponse
{
    /// <summary>
    /// Error type URI
    /// </summary>
    public string Type { get; init; } = null!;

    /// <summary>
    /// Error title
    /// </summary>
    public string Title { get; init; } = null!;

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int Status { get; init; }

    /// <summary>
    /// Detailed error message
    /// </summary>
    public string Detail { get; init; } = null!;

    /// <summary>
    /// Request path
    /// </summary>
    public string Instance { get; init; } = null!;

    /// <summary>
    /// Request trace identifier
    /// </summary>
    public string TraceId { get; init; } = null!;

    /// <summary>
    /// Validation errors by field
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; init; }
}

public record NotFoundErrorResponse
{
    /// <summary>
    /// Error type URI
    /// </summary>
    public string Type { get; init; } = null!;

    /// <summary>
    /// Error title
    /// </summary>
    public string Title { get; init; } = null!;

    /// <summary>
    /// HTTP status code
    /// </summary>
    public int Status { get; init; }

    /// <summary>
    /// Detailed error message
    /// </summary>
    public string Detail { get; init; } = null!;

    /// <summary>
    /// Request path
    /// </summary>
    public string Instance { get; init; } = null!;

    /// <summary>
    /// Request trace identifier
    /// </summary>
    public string TraceId { get; init; } = null!;
}

// ==================== Paginated Response ====================

/// <summary>
/// Generic paginated response wrapper.
/// </summary>
public record PagedResponse<T>
{
    /// <summary>
    /// Page items
    /// </summary>
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    /// <example>1</example>
    public int Page { get; init; }

    /// <summary>
    /// Items per page
    /// </summary>
    /// <example>20</example>
    public int PageSize { get; init; }

    /// <summary>
    /// Total number of items
    /// </summary>
    /// <example>150</example>
    public int TotalCount { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    /// <example>8</example>
    public int TotalPages { get; init; }

    /// <summary>
    /// Whether there is a next page
    /// </summary>
    public bool HasNextPage { get; init; }

    /// <summary>
    /// Whether there is a previous page
    /// </summary>
    public bool HasPreviousPage { get; init; }
}

// ==================== API Response Wrapper ====================

/// <summary>
/// Standard API response wrapper.
/// </summary>
public record ApiResponse<T>
{
    /// <summary>
    /// Response data
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Whether the request was successful
    /// </summary>
    public bool Success { get; init; }

    /// <summary>
    /// Error message if not successful
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Response metadata
    /// </summary>
    public ResponseMetadata Meta { get; init; } = new();
}

/// <summary>
/// Response metadata.
/// </summary>
public record ResponseMetadata
{
    /// <summary>
    /// Response timestamp
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Request correlation ID
    /// </summary>
    public string? CorrelationId { get; init; }

    /// <summary>
    /// API version
    /// </summary>
    /// <example>1.0</example>
    public string Version { get; init; } = "1.0";
}
