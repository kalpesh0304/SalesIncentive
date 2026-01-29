# DORISE Sales Incentive Framework
## Developer Guide & Coding Standards (CLAUDE.md)

**Document ID:** DOC-TECH-001
**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Created:** January 2025
**Last Updated:** January 2025
**Author:** Claude Code
**Status:** Active

> *"I'm learnding!"* - Ralph Wiggum
>
> This guide helps every developer learn and follow our coding standards consistently.

---

## Table of Contents

1. [Overview](#1-overview)
2. [Project Structure](#2-project-structure)
3. [Coding Standards](#3-coding-standards)
4. [Architecture Patterns](#4-architecture-patterns)
5. [Error Handling](#5-error-handling)
6. [Logging Standards](#6-logging-standards)
7. [Testing Strategy](#7-testing-strategy)
8. [Security Guidelines](#8-security-guidelines)
9. [Performance Guidelines](#9-performance-guidelines)
10. [Database Guidelines](#10-database-guidelines)
11. [API Guidelines](#11-api-guidelines)
12. [Git Workflow](#12-git-workflow)

---

## 1. Overview

### 1.1 Purpose

This document provides comprehensive coding standards, architectural guidelines, and best practices for all developers working on the DSIF project. Following these standards ensures code quality, maintainability, and consistency across the codebase.

### 1.2 Technology Stack

| Component | Technology | Version |
|-----------|------------|---------|
| Runtime | .NET | 8.0 LTS |
| Language | C# | 12 |
| Web Framework | ASP.NET Core | 8.0 |
| Frontend | Blazor Server | 8.0 |
| ORM | Entity Framework Core | 8.0 |
| Database | Azure SQL Database | Latest |
| Cache | Azure Redis Cache | 6.x |
| IDE | Visual Studio 2022 / VS Code | Latest |

### 1.3 Required Tools

```bash
# Required installations
dotnet --version  # .NET 8.0 SDK
az --version      # Azure CLI
git --version     # Git 2.x+
docker --version  # Docker Desktop (optional for local dev)
```

---

## 2. Project Structure

### 2.1 Solution Organization

```
Dorise.Incentive/
├── src/
│   ├── Dorise.Incentive.Api/              # Web API (REST endpoints)
│   │   ├── Controllers/                    # API controllers
│   │   ├── Middleware/                     # Custom middleware
│   │   ├── Filters/                        # Action filters
│   │   └── Program.cs                      # Entry point
│   │
│   ├── Dorise.Incentive.Web/              # Blazor Web Application
│   │   ├── Pages/                          # Razor pages
│   │   ├── Components/                     # Reusable components
│   │   ├── Services/                       # UI services
│   │   └── wwwroot/                        # Static files
│   │
│   ├── Dorise.Incentive.Application/      # Application Layer (CQRS)
│   │   ├── Commands/                       # Command handlers
│   │   │   ├── Employee/
│   │   │   ├── IncentivePlan/
│   │   │   ├── Calculation/
│   │   │   └── Approval/
│   │   ├── Queries/                        # Query handlers
│   │   │   ├── Employee/
│   │   │   ├── IncentivePlan/
│   │   │   ├── Calculation/
│   │   │   └── Reports/
│   │   ├── Validators/                     # FluentValidation validators
│   │   ├── EventHandlers/                  # Domain event handlers
│   │   ├── DTOs/                           # Data transfer objects
│   │   └── Behaviors/                      # MediatR pipeline behaviors
│   │
│   ├── Dorise.Incentive.Core/             # Domain Layer
│   │   ├── Entities/                       # Domain entities
│   │   ├── ValueObjects/                   # Value objects
│   │   ├── Services/                       # Domain services
│   │   ├── Events/                         # Domain events
│   │   ├── Interfaces/                     # Repository interfaces
│   │   ├── Specifications/                 # Query specifications
│   │   └── Exceptions/                     # Domain exceptions
│   │
│   ├── Dorise.Incentive.Infrastructure/   # Infrastructure Layer
│   │   ├── Data/                           # EF Core DbContext
│   │   │   ├── IncentiveDbContext.cs
│   │   │   ├── Configurations/             # Entity configurations
│   │   │   └── Migrations/                 # EF migrations
│   │   ├── Repositories/                   # Repository implementations
│   │   ├── ExternalServices/               # External API clients
│   │   ├── Caching/                        # Cache implementations
│   │   └── Identity/                       # Identity services
│   │
│   ├── Dorise.Incentive.Functions/        # Azure Functions
│   │   ├── CalculationTrigger/             # Calculation batch jobs
│   │   ├── ImportJob/                      # Data import functions
│   │   ├── ExportJob/                      # Payroll export functions
│   │   └── NotificationService/            # Email/notification functions
│   │
│   └── Dorise.Incentive.Shared/           # Shared Library
│       ├── DTOs/                           # Shared DTOs
│       ├── Constants/                      # Application constants
│       ├── Extensions/                     # Extension methods
│       └── Utilities/                      # Common utilities
│
├── tests/
│   ├── Dorise.Incentive.UnitTests/        # Unit tests
│   ├── Dorise.Incentive.IntegrationTests/ # Integration tests
│   └── Dorise.Incentive.E2ETests/         # End-to-end tests
│
├── infra/
│   ├── bicep/                              # Infrastructure as Code
│   └── scripts/                            # Deployment scripts
│
├── docs/                                   # Documentation
│
├── .github/                                # GitHub Actions
│   └── workflows/
│
├── Dorise.Incentive.sln                    # Solution file
├── Directory.Build.props                   # Shared MSBuild properties
├── Directory.Packages.props                # Central package management
├── .editorconfig                           # Editor configuration
└── CLAUDE.md                               # This file
```

### 2.2 Namespace Conventions

```csharp
// Root namespace
namespace Dorise.Incentive;

// Layer namespaces
namespace Dorise.Incentive.Api.Controllers;
namespace Dorise.Incentive.Application.Commands.Employee;
namespace Dorise.Incentive.Core.Entities;
namespace Dorise.Incentive.Infrastructure.Data;
```

---

## 3. Coding Standards

### 3.1 Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| **Namespaces** | PascalCase | `Dorise.Incentive.Core` |
| **Classes** | PascalCase, noun | `EmployeeService`, `CalculationEngine` |
| **Interfaces** | I + PascalCase | `IEmployeeRepository`, `ICalculationService` |
| **Methods** | PascalCase, verb | `GetEmployeeById`, `CalculateIncentive` |
| **Properties** | PascalCase | `FirstName`, `CalculatedAmount` |
| **Private fields** | _camelCase | `_employeeRepository`, `_logger` |
| **Parameters** | camelCase | `employeeId`, `calculationPeriod` |
| **Constants** | PascalCase | `MaxRetryCount`, `DefaultPageSize` |
| **Async methods** | *Async suffix | `GetEmployeeAsync`, `SaveChangesAsync` |

### 3.2 File Organization

```csharp
// File: Employee.cs
// Order: usings, namespace, class

using System;
using System.Collections.Generic;
using Dorise.Incentive.Core.ValueObjects;
using Dorise.Incentive.Core.Events;

namespace Dorise.Incentive.Core.Entities;

/// <summary>
/// Represents a sales employee in the incentive system.
/// </summary>
public class Employee : BaseEntity, IAggregateRoot
{
    // 1. Constants
    public const int MaxNameLength = 100;

    // 2. Private fields
    private readonly List<Assignment> _assignments = new();

    // 3. Constructors
    private Employee() { } // EF Core constructor

    public Employee(EmployeeId id, string employeeNumber, string firstName, string lastName)
    {
        Id = id ?? throw new ArgumentNullException(nameof(id));
        EmployeeNumber = employeeNumber ?? throw new ArgumentNullException(nameof(employeeNumber));
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        Status = EmployeeStatus.Active;
        CreatedAt = DateTime.UtcNow;

        AddDomainEvent(new EmployeeCreatedEvent(Id));
    }

    // 4. Properties
    public EmployeeId Id { get; private set; }
    public string EmployeeNumber { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string FullName => $"{FirstName} {LastName}";
    public string Email { get; private set; }
    public EmployeeStatus Status { get; private set; }
    public DateTime HireDate { get; private set; }
    public DateTime? TerminationDate { get; private set; }
    public IReadOnlyCollection<Assignment> Assignments => _assignments.AsReadOnly();

    // 5. Public methods
    public void UpdateName(string firstName, string lastName)
    {
        FirstName = firstName ?? throw new ArgumentNullException(nameof(firstName));
        LastName = lastName ?? throw new ArgumentNullException(nameof(lastName));
        ModifiedAt = DateTime.UtcNow;
    }

    public void Terminate(DateTime terminationDate)
    {
        if (Status == EmployeeStatus.Terminated)
            throw new DomainException("Employee is already terminated.");

        TerminationDate = terminationDate;
        Status = EmployeeStatus.Terminated;
        ModifiedAt = DateTime.UtcNow;

        AddDomainEvent(new EmployeeTerminatedEvent(Id, terminationDate));
    }

    // 6. Private methods
    private void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (name.Length > MaxNameLength)
            throw new ArgumentException($"Name cannot exceed {MaxNameLength} characters.", nameof(name));
    }
}
```

### 3.3 Code Style Rules

```csharp
// ✅ DO: Use expression-bodied members for simple operations
public string FullName => $"{FirstName} {LastName}";
public bool IsActive => Status == EmployeeStatus.Active;

// ✅ DO: Use pattern matching
if (employee is { Status: EmployeeStatus.Active, Assignments.Count: > 0 })
{
    // Process active employee with assignments
}

// ✅ DO: Use collection expressions (C# 12)
List<string> names = [firstName, lastName];
int[] numbers = [1, 2, 3, 4, 5];

// ✅ DO: Use primary constructors (C# 12) for simple classes
public class EmployeeDto(Guid id, string name, string email);

// ✅ DO: Use required properties for initialization
public class CreateEmployeeCommand
{
    public required string EmployeeNumber { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
}

// ❌ DON'T: Use var when type isn't obvious
var x = GetSomething(); // What type is x?

// ✅ DO: Be explicit when type isn't obvious
Employee employee = await _repository.GetByIdAsync(id);

// ✅ DO: Use var when type is obvious
var employees = new List<Employee>();
var result = await _mediator.Send(command);
```

### 3.4 Async/Await Guidelines

```csharp
// ✅ DO: Always use async/await for I/O operations
public async Task<Employee> GetEmployeeAsync(EmployeeId id, CancellationToken cancellationToken)
{
    return await _dbContext.Employees
        .AsNoTracking()
        .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
}

// ✅ DO: Accept CancellationToken and pass it through
public async Task<IEnumerable<CalculationDto>> GetCalculationsAsync(
    string period,
    CancellationToken cancellationToken = default)
{
    var calculations = await _repository.GetByPeriodAsync(period, cancellationToken);
    return _mapper.Map<IEnumerable<CalculationDto>>(calculations);
}

// ✅ DO: Use ConfigureAwait(false) in library code
public async Task<T> ReadFromCacheAsync<T>(string key)
{
    var cached = await _cache.GetStringAsync(key).ConfigureAwait(false);
    return cached is null ? default : JsonSerializer.Deserialize<T>(cached);
}

// ❌ DON'T: Block on async code
var result = GetDataAsync().Result; // Deadlock risk!
var result = GetDataAsync().GetAwaiter().GetResult(); // Still blocking!

// ❌ DON'T: Use async void (except event handlers)
public async void ProcessEmployee(Employee employee) // Bad!

// ✅ DO: Return Task for async methods without return value
public async Task ProcessEmployeeAsync(Employee employee)
```

> *"Me fail English? That's unpossible!"* - Ralph Wiggum
>
> Following these naming conventions makes our code readable - failures are unpossible!

---

## 4. Architecture Patterns

### 4.1 CQRS with MediatR

```csharp
// Command definition
public record CreateEmployeeCommand(
    string EmployeeNumber,
    string FirstName,
    string LastName,
    string Email,
    DateTime HireDate) : IRequest<Result<EmployeeDto>>;

// Command handler
public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, Result<EmployeeDto>>
{
    private readonly IEmployeeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<CreateEmployeeCommandHandler> _logger;

    public CreateEmployeeCommandHandler(
        IEmployeeRepository repository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<CreateEmployeeCommandHandler> logger)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<EmployeeDto>> Handle(
        CreateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        // Check for duplicates
        var existing = await _repository.GetByEmployeeNumberAsync(
            request.EmployeeNumber,
            cancellationToken);

        if (existing is not null)
        {
            return Result<EmployeeDto>.Failure(
                $"Employee with number {request.EmployeeNumber} already exists.");
        }

        // Create domain entity
        var employee = new Employee(
            EmployeeId.New(),
            request.EmployeeNumber,
            request.FirstName,
            request.LastName);

        employee.SetEmail(request.Email);
        employee.SetHireDate(request.HireDate);

        // Persist
        await _repository.AddAsync(employee, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Created employee {EmployeeId} with number {EmployeeNumber}",
            employee.Id,
            employee.EmployeeNumber);

        return Result<EmployeeDto>.Success(_mapper.Map<EmployeeDto>(employee));
    }
}

// Query definition
public record GetEmployeeByIdQuery(Guid EmployeeId) : IRequest<EmployeeDto?>;

// Query handler
public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeDto?>
{
    private readonly IReadOnlyRepository<Employee> _repository;
    private readonly IMapper _mapper;

    public GetEmployeeByIdQueryHandler(
        IReadOnlyRepository<Employee> repository,
        IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<EmployeeDto?> Handle(
        GetEmployeeByIdQuery request,
        CancellationToken cancellationToken)
    {
        var employee = await _repository.GetByIdAsync(
            new EmployeeId(request.EmployeeId),
            cancellationToken);

        return employee is null ? null : _mapper.Map<EmployeeDto>(employee);
    }
}
```

### 4.2 FluentValidation

```csharp
public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeNumber)
            .NotEmpty().WithMessage("Employee number is required.")
            .MaximumLength(20).WithMessage("Employee number cannot exceed 20 characters.")
            .Matches(@"^EMP\d+$").WithMessage("Employee number must be in format EMP###.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .Must(BeCompanyEmail).WithMessage("Email must be a company email address.");

        RuleFor(x => x.HireDate)
            .NotEmpty().WithMessage("Hire date is required.")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Hire date cannot be in the future.");
    }

    private bool BeCompanyEmail(string email)
    {
        return email.EndsWith("@dorise.com", StringComparison.OrdinalIgnoreCase);
    }
}
```

### 4.3 Repository Pattern

```csharp
// Interface (in Core layer)
public interface IEmployeeRepository : IRepository<Employee>
{
    Task<Employee?> GetByEmployeeNumberAsync(string employeeNumber, CancellationToken cancellationToken);
    Task<IEnumerable<Employee>> GetByStoreAsync(StoreId storeId, CancellationToken cancellationToken);
    Task<IEnumerable<Employee>> GetActiveForPeriodAsync(string period, CancellationToken cancellationToken);
}

// Implementation (in Infrastructure layer)
public class EmployeeRepository : Repository<Employee>, IEmployeeRepository
{
    public EmployeeRepository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<Employee?> GetByEmployeeNumberAsync(
        string employeeNumber,
        CancellationToken cancellationToken)
    {
        return await _context.Employees
            .Include(e => e.Assignments)
            .FirstOrDefaultAsync(e => e.EmployeeNumber == employeeNumber, cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetByStoreAsync(
        StoreId storeId,
        CancellationToken cancellationToken)
    {
        return await _context.Employees
            .Where(e => e.Assignments.Any(a =>
                a.StoreId == storeId &&
                a.EffectiveTo == null))
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<Employee>> GetActiveForPeriodAsync(
        string period,
        CancellationToken cancellationToken)
    {
        var periodStart = DateOnly.ParseExact(period + "-01", "yyyy-MM-dd");
        var periodEnd = periodStart.AddMonths(1).AddDays(-1);

        return await _context.Employees
            .AsNoTracking()
            .Where(e => e.Status == EmployeeStatus.Active)
            .Where(e => e.HireDate <= periodEnd.ToDateTime(TimeOnly.MinValue))
            .Where(e => e.TerminationDate == null || e.TerminationDate >= periodStart.ToDateTime(TimeOnly.MinValue))
            .Include(e => e.Assignments.Where(a =>
                a.EffectiveFrom <= periodEnd.ToDateTime(TimeOnly.MinValue) &&
                (a.EffectiveTo == null || a.EffectiveTo >= periodStart.ToDateTime(TimeOnly.MinValue))))
            .ToListAsync(cancellationToken);
    }
}
```

### 4.4 Domain Events

```csharp
// Domain event definition
public record CalculationCompletedEvent(
    CalculationId CalculationId,
    EmployeeId EmployeeId,
    string Period,
    Money Amount) : IDomainEvent;

// Event handler
public class CalculationCompletedEventHandler : INotificationHandler<CalculationCompletedEvent>
{
    private readonly IApprovalService _approvalService;
    private readonly INotificationService _notificationService;
    private readonly ILogger<CalculationCompletedEventHandler> _logger;

    public CalculationCompletedEventHandler(
        IApprovalService approvalService,
        INotificationService notificationService,
        ILogger<CalculationCompletedEventHandler> logger)
    {
        _approvalService = approvalService;
        _notificationService = notificationService;
        _logger = logger;
    }

    public async Task Handle(
        CalculationCompletedEvent notification,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Calculation {CalculationId} completed for employee {EmployeeId} with amount {Amount}",
            notification.CalculationId,
            notification.EmployeeId,
            notification.Amount);

        // Create approval request
        await _approvalService.CreateApprovalRequestAsync(
            notification.CalculationId,
            cancellationToken);

        // Send notification
        await _notificationService.SendCalculationCompletedNotificationAsync(
            notification.EmployeeId,
            notification.Period,
            notification.Amount,
            cancellationToken);
    }
}
```

---

## 5. Error Handling

### 5.1 Exception Hierarchy

```csharp
// Base exception
public abstract class DoriseException : Exception
{
    public string ErrorCode { get; }
    public IDictionary<string, object> Metadata { get; } = new Dictionary<string, object>();

    protected DoriseException(string message, string errorCode) : base(message)
    {
        ErrorCode = errorCode;
    }

    protected DoriseException(string message, string errorCode, Exception innerException)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}

// Domain exception (business rule violation)
public class DomainException : DoriseException
{
    public DomainException(string message)
        : base(message, "DOMAIN_ERROR") { }
}

// Not found exception
public class NotFoundException : DoriseException
{
    public string EntityType { get; }
    public object EntityId { get; }

    public NotFoundException(string entityType, object entityId)
        : base($"{entityType} with id '{entityId}' was not found.", "NOT_FOUND")
    {
        EntityType = entityType;
        EntityId = entityId;
        Metadata["EntityType"] = entityType;
        Metadata["EntityId"] = entityId;
    }
}

// Validation exception
public class ValidationException : DoriseException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.", "VALIDATION_ERROR")
    {
        Errors = new ReadOnlyDictionary<string, string[]>(errors);
    }
}

// Conflict exception
public class ConflictException : DoriseException
{
    public ConflictException(string message)
        : base(message, "CONFLICT_ERROR") { }
}
```

### 5.2 Global Exception Handler

```csharp
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, response) = exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Validation Error",
                    Detail = validationEx.Message,
                    Extensions = { ["errors"] = validationEx.Errors }
                }),

            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                new ProblemDetails
                {
                    Status = StatusCodes.Status404NotFound,
                    Title = "Not Found",
                    Detail = notFoundEx.Message
                }),

            DomainException domainEx => (
                StatusCodes.Status400BadRequest,
                new ProblemDetails
                {
                    Status = StatusCodes.Status400BadRequest,
                    Title = "Business Rule Violation",
                    Detail = domainEx.Message
                }),

            ConflictException conflictEx => (
                StatusCodes.Status409Conflict,
                new ProblemDetails
                {
                    Status = StatusCodes.Status409Conflict,
                    Title = "Conflict",
                    Detail = conflictEx.Message
                }),

            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                new ProblemDetails
                {
                    Status = StatusCodes.Status403Forbidden,
                    Title = "Forbidden",
                    Detail = "You do not have permission to perform this action."
                }),

            _ => (
                StatusCodes.Status500InternalServerError,
                new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Detail = "An unexpected error occurred."
                })
        };

        // Log the exception
        if (statusCode >= 500)
        {
            _logger.LogError(exception,
                "Unhandled exception occurred. TraceId: {TraceId}",
                context.TraceIdentifier);
        }
        else
        {
            _logger.LogWarning(exception,
                "Client error occurred. TraceId: {TraceId}",
                context.TraceIdentifier);
        }

        response.Instance = context.Request.Path;
        response.Extensions["traceId"] = context.TraceIdentifier;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(response);
    }
}
```

### 5.3 Result Pattern

```csharp
// Result type for operations that can fail
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public string? ErrorCode { get; }

    protected Result(bool isSuccess, string? error, string? errorCode)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorCode = errorCode;
    }

    public static Result Success() => new(true, null, null);
    public static Result Failure(string error, string? errorCode = null) => new(false, error, errorCode);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string? error, string? errorCode)
        : base(isSuccess, error, errorCode)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, null, null);
    public new static Result<T> Failure(string error, string? errorCode = null) => new(false, default, error, errorCode);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<string, TResult> onFailure)
        => IsSuccess ? onSuccess(Value!) : onFailure(Error!);
}

// Usage in handlers
public async Task<Result<EmployeeDto>> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
{
    var existing = await _repository.GetByEmployeeNumberAsync(request.EmployeeNumber, cancellationToken);
    if (existing is not null)
    {
        return Result<EmployeeDto>.Failure(
            $"Employee with number {request.EmployeeNumber} already exists.",
            "EMPLOYEE_DUPLICATE");
    }

    // ... create employee
    return Result<EmployeeDto>.Success(dto);
}
```

> *"My cat's breath smells like cat food."* - Ralph Wiggum
>
> Our error messages are clear and helpful - never leave developers guessing!

---

## 6. Logging Standards

### 6.1 Structured Logging with Serilog

```csharp
// Configuration in Program.cs
builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.ApplicationInsights(
        services.GetRequiredService<TelemetryConfiguration>(),
        TelemetryConverter.Traces));
```

### 6.2 Logging Best Practices

```csharp
public class CalculationService
{
    private readonly ILogger<CalculationService> _logger;

    public CalculationService(ILogger<CalculationService> logger)
    {
        _logger = logger;
    }

    public async Task<CalculationResult> CalculateAsync(CalculationRequest request)
    {
        // ✅ DO: Use structured logging with meaningful properties
        _logger.LogInformation(
            "Starting calculation for employee {EmployeeId} period {Period}",
            request.EmployeeId,
            request.Period);

        using var activity = _logger.BeginScope(new Dictionary<string, object>
        {
            ["EmployeeId"] = request.EmployeeId,
            ["Period"] = request.Period,
            ["BatchId"] = request.BatchId
        });

        try
        {
            var result = await PerformCalculationAsync(request);

            // ✅ DO: Log significant business events
            _logger.LogInformation(
                "Calculation completed for employee {EmployeeId}. Amount: {Amount:C}, Slab: {SlabName}",
                request.EmployeeId,
                result.Amount,
                result.SlabApplied);

            return result;
        }
        catch (Exception ex)
        {
            // ✅ DO: Log errors with context
            _logger.LogError(ex,
                "Calculation failed for employee {EmployeeId} period {Period}",
                request.EmployeeId,
                request.Period);
            throw;
        }
    }
}

// ❌ DON'T: Log sensitive information
_logger.LogInformation("User {UserId} logged in with password {Password}", userId, password);

// ❌ DON'T: Use string interpolation in log messages
_logger.LogInformation($"Processing employee {employeeId}"); // No structured properties!

// ✅ DO: Use log level appropriately
_logger.LogTrace("Detailed diagnostic info for debugging");
_logger.LogDebug("Debug info useful during development");
_logger.LogInformation("Normal operational events");
_logger.LogWarning("Abnormal but recoverable situations");
_logger.LogError(ex, "Errors that need attention");
_logger.LogCritical(ex, "System is in unusable state");
```

### 6.3 Correlation IDs

```csharp
// Middleware to propagate correlation ID
public class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[CorrelationIdHeader].FirstOrDefault()
            ?? Guid.NewGuid().ToString();

        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}
```

---

## 7. Testing Strategy

### 7.1 Test Project Structure

```
tests/
├── Dorise.Incentive.UnitTests/
│   ├── Core/
│   │   ├── Entities/
│   │   │   └── EmployeeTests.cs
│   │   └── Services/
│   │       └── CalculationEngineTests.cs
│   ├── Application/
│   │   ├── Commands/
│   │   │   └── CreateEmployeeCommandHandlerTests.cs
│   │   └── Validators/
│   │       └── CreateEmployeeCommandValidatorTests.cs
│   └── _Fixtures/
│       └── TestDataBuilder.cs
│
├── Dorise.Incentive.IntegrationTests/
│   ├── Api/
│   │   └── EmployeesControllerTests.cs
│   ├── Data/
│   │   └── EmployeeRepositoryTests.cs
│   └── _Infrastructure/
│       ├── TestWebApplicationFactory.cs
│       └── DatabaseFixture.cs
│
└── Dorise.Incentive.E2ETests/
    └── Scenarios/
        └── CalculationWorkflowTests.cs
```

### 7.2 Unit Testing Patterns

```csharp
// Test class using xUnit
public class CalculationEngineTests
{
    private readonly CalculationEngine _sut;
    private readonly Mock<IPlanRepository> _planRepositoryMock;
    private readonly Mock<ILogger<CalculationEngine>> _loggerMock;

    public CalculationEngineTests()
    {
        _planRepositoryMock = new Mock<IPlanRepository>();
        _loggerMock = new Mock<ILogger<CalculationEngine>>();
        _sut = new CalculationEngine(_planRepositoryMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task Calculate_WithValidInput_ReturnsCorrectAmount()
    {
        // Arrange
        var employee = new EmployeeBuilder()
            .WithStatus(EmployeeStatus.Active)
            .Build();

        var plan = new PlanBuilder()
            .WithSlab(minThreshold: 0, maxThreshold: 50000, rate: 0.02m)
            .WithSlab(minThreshold: 50000, maxThreshold: 100000, rate: 0.03m)
            .Build();

        var salesData = new SalesData
        {
            SalesAmount = 75000m,
            Period = "2025-01"
        };

        _planRepositoryMock
            .Setup(x => x.GetActivePlanForRoleAsync(It.IsAny<RoleId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        // Act
        var result = await _sut.CalculateAsync(employee, salesData, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.GrossAmount.Should().Be(1750m); // (50000 * 0.02) + (25000 * 0.03)
        result.SlabApplied.Should().Be("Slab2");
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(25000, 500)]      // 25000 * 0.02
    [InlineData(50000, 1000)]     // 50000 * 0.02
    [InlineData(75000, 1750)]     // 1000 + (25000 * 0.03)
    public async Task Calculate_WithVariousSalesAmounts_ReturnsExpectedIncentive(
        decimal salesAmount,
        decimal expectedIncentive)
    {
        // Arrange
        var employee = new EmployeeBuilder().Build();
        var plan = CreateTieredPlan();
        var salesData = new SalesData { SalesAmount = salesAmount, Period = "2025-01" };

        _planRepositoryMock
            .Setup(x => x.GetActivePlanForRoleAsync(It.IsAny<RoleId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(plan);

        // Act
        var result = await _sut.CalculateAsync(employee, salesData, CancellationToken.None);

        // Assert
        result.GrossAmount.Should().Be(expectedIncentive);
    }

    [Fact]
    public async Task Calculate_WithTerminatedEmployee_ThrowsDomainException()
    {
        // Arrange
        var employee = new EmployeeBuilder()
            .WithStatus(EmployeeStatus.Terminated)
            .Build();

        // Act
        var act = () => _sut.CalculateAsync(employee, new SalesData(), CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*terminated*");
    }
}

// Test data builder
public class EmployeeBuilder
{
    private EmployeeId _id = EmployeeId.New();
    private string _employeeNumber = "EMP001";
    private string _firstName = "John";
    private string _lastName = "Doe";
    private EmployeeStatus _status = EmployeeStatus.Active;

    public EmployeeBuilder WithId(EmployeeId id) { _id = id; return this; }
    public EmployeeBuilder WithEmployeeNumber(string number) { _employeeNumber = number; return this; }
    public EmployeeBuilder WithStatus(EmployeeStatus status) { _status = status; return this; }

    public Employee Build()
    {
        var employee = new Employee(_id, _employeeNumber, _firstName, _lastName);
        // Set status via reflection for testing
        typeof(Employee).GetProperty(nameof(Employee.Status))!.SetValue(employee, _status);
        return employee;
    }
}
```

### 7.3 Integration Testing

```csharp
// Test web application factory
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove production database
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<IncentiveDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add test database
            services.AddDbContext<IncentiveDbContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // Seed test data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IncentiveDbContext>();
            db.Database.EnsureCreated();
            SeedTestData(db);
        });
    }

    private void SeedTestData(IncentiveDbContext db)
    {
        db.Employees.Add(new EmployeeBuilder().WithEmployeeNumber("TEST001").Build());
        db.SaveChanges();
    }
}

// Integration test
public class EmployeesControllerTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly HttpClient _client;

    public EmployeesControllerTests(TestWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetEmployee_WithValidId_ReturnsEmployee()
    {
        // Arrange
        var employeeId = Guid.Parse("..."); // Known test data ID

        // Act
        var response = await _client.GetAsync($"/api/v1/employees/{employeeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var employee = await response.Content.ReadFromJsonAsync<EmployeeDto>();
        employee.Should().NotBeNull();
        employee!.EmployeeNumber.Should().Be("TEST001");
    }

    [Fact]
    public async Task CreateEmployee_WithValidData_ReturnsCreated()
    {
        // Arrange
        var command = new
        {
            EmployeeNumber = "NEW001",
            FirstName = "Jane",
            LastName = "Doe",
            Email = "jane.doe@dorise.com",
            HireDate = DateTime.Today.AddDays(-30)
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/employees", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }
}
```

### 7.4 Test Coverage Requirements

| Layer | Minimum Coverage | Focus Areas |
|-------|------------------|-------------|
| **Core (Domain)** | 90% | Entities, Value Objects, Domain Services |
| **Application** | 85% | Command/Query Handlers, Validators |
| **Infrastructure** | 70% | Repositories, External Services |
| **API** | 75% | Controllers, Middleware |

---

## 8. Security Guidelines

### 8.1 Authentication & Authorization

```csharp
// Configure Azure AD authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        options.TokenValidationParameters.NameClaimType = "name";
    },
    options => builder.Configuration.Bind("AzureAd", options));

// Configure authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdmin", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("RequireApprover", policy =>
        policy.RequireRole("Admin", "Manager", "Approver"));

    options.AddPolicy("RequireZoneAccess", policy =>
        policy.Requirements.Add(new ZoneAccessRequirement()));
});

// Custom authorization handler for zone-based access
public class ZoneAccessHandler : AuthorizationHandler<ZoneAccessRequirement, ZoneResource>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ZoneAccessRequirement requirement,
        ZoneResource resource)
    {
        var userZones = context.User.FindAll("zone").Select(c => c.Value);

        if (context.User.IsInRole("Admin") || userZones.Contains(resource.ZoneId.ToString()))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

### 8.2 Input Validation

```csharp
// Always validate input at controller level
[HttpPost]
[ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeCommand command)
{
    // FluentValidation runs automatically via MediatR pipeline
    var result = await _mediator.Send(command);

    return result.Match(
        employee => CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, employee),
        error => BadRequest(new { Error = error }));
}

// Sanitize user input
public static class InputSanitizer
{
    public static string SanitizeString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        // Remove potential XSS characters
        return HtmlEncoder.Default.Encode(input.Trim());
    }
}
```

### 8.3 Data Protection

```csharp
// Use Key Vault for secrets
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://{builder.Configuration["KeyVault:VaultName"]}.vault.azure.net/"),
    new DefaultAzureCredential());

// Mask sensitive data in logs
public class SensitiveDataMaskingEnricher : ILogEventEnricher
{
    private static readonly string[] SensitiveProperties =
        { "password", "ssn", "creditcard", "token" };

    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        foreach (var property in logEvent.Properties.ToList())
        {
            if (SensitiveProperties.Any(s =>
                property.Key.Contains(s, StringComparison.OrdinalIgnoreCase)))
            {
                logEvent.AddOrUpdateProperty(
                    propertyFactory.CreateProperty(property.Key, "***REDACTED***"));
            }
        }
    }
}

// Encrypt sensitive data at rest
public class EncryptedStringConverter : ValueConverter<string, string>
{
    public EncryptedStringConverter(IDataProtector protector)
        : base(
            v => protector.Protect(v),
            v => protector.Unprotect(v))
    {
    }
}
```

> *"I bent my Wookie."* - Ralph Wiggum
>
> Security vulnerabilities can break things in unexpected ways - always follow security guidelines!

---

## 9. Performance Guidelines

### 9.1 Database Query Optimization

```csharp
// ✅ DO: Use AsNoTracking for read-only queries
public async Task<IEnumerable<EmployeeDto>> GetEmployeesAsync(CancellationToken cancellationToken)
{
    return await _context.Employees
        .AsNoTracking()
        .Select(e => new EmployeeDto
        {
            Id = e.Id.Value,
            Name = e.FullName,
            Email = e.Email
        })
        .ToListAsync(cancellationToken);
}

// ✅ DO: Use pagination for large result sets
public async Task<PagedResult<EmployeeDto>> GetEmployeesPagedAsync(
    int page,
    int pageSize,
    CancellationToken cancellationToken)
{
    var query = _context.Employees.AsNoTracking();

    var totalCount = await query.CountAsync(cancellationToken);

    var items = await query
        .OrderBy(e => e.LastName)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(e => new EmployeeDto { /* ... */ })
        .ToListAsync(cancellationToken);

    return new PagedResult<EmployeeDto>(items, totalCount, page, pageSize);
}

// ✅ DO: Use Include strategically
public async Task<Employee?> GetEmployeeWithAssignmentsAsync(
    EmployeeId id,
    CancellationToken cancellationToken)
{
    return await _context.Employees
        .Include(e => e.Assignments.Where(a => a.EffectiveTo == null))
            .ThenInclude(a => a.Store)
        .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
}

// ❌ DON'T: Load more data than needed
var employee = await _context.Employees
    .Include(e => e.Assignments)
        .ThenInclude(a => a.Store)
            .ThenInclude(s => s.Zone)
                .ThenInclude(z => z.Company) // Over-fetching!
    .FirstOrDefaultAsync(e => e.Id == id);
```

### 9.2 Caching Strategies

```csharp
// Cache service interface
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
}

// Cache-aside pattern implementation
public class CachedPlanRepository : IPlanRepository
{
    private readonly IPlanRepository _inner;
    private readonly ICacheService _cache;
    private readonly ILogger<CachedPlanRepository> _logger;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(30);

    public CachedPlanRepository(
        IPlanRepository inner,
        ICacheService cache,
        ILogger<CachedPlanRepository> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<IncentivePlan?> GetByIdAsync(PlanId id, CancellationToken cancellationToken)
    {
        var cacheKey = $"plan:{id.Value}";

        var cached = await _cache.GetAsync<IncentivePlan>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for plan {PlanId}", id);
            return cached;
        }

        _logger.LogDebug("Cache miss for plan {PlanId}", id);
        var plan = await _inner.GetByIdAsync(id, cancellationToken);

        if (plan is not null)
        {
            await _cache.SetAsync(cacheKey, plan, CacheDuration, cancellationToken);
        }

        return plan;
    }

    // Invalidate cache on updates
    public async Task UpdateAsync(IncentivePlan plan, CancellationToken cancellationToken)
    {
        await _inner.UpdateAsync(plan, cancellationToken);
        await _cache.RemoveAsync($"plan:{plan.Id.Value}", cancellationToken);
    }
}
```

### 9.3 Async Best Practices

```csharp
// ✅ DO: Run independent operations in parallel
public async Task<DashboardData> GetDashboardDataAsync(CancellationToken cancellationToken)
{
    var employeeCountTask = _employeeRepository.CountAsync(cancellationToken);
    var pendingApprovalsTask = _approvalRepository.CountPendingAsync(cancellationToken);
    var recentCalculationsTask = _calculationRepository.GetRecentAsync(10, cancellationToken);

    await Task.WhenAll(employeeCountTask, pendingApprovalsTask, recentCalculationsTask);

    return new DashboardData
    {
        EmployeeCount = await employeeCountTask,
        PendingApprovals = await pendingApprovalsTask,
        RecentCalculations = await recentCalculationsTask
    };
}

// ✅ DO: Use proper batch processing
public async Task ProcessEmployeesInBatchesAsync(
    IEnumerable<Employee> employees,
    CancellationToken cancellationToken)
{
    const int batchSize = 100;
    var batches = employees.Chunk(batchSize);

    foreach (var batch in batches)
    {
        var tasks = batch.Select(e => ProcessEmployeeAsync(e, cancellationToken));
        await Task.WhenAll(tasks);

        // Allow other tasks to run
        await Task.Yield();
    }
}
```

---

## 10. Database Guidelines

### 10.1 Entity Framework Core Configuration

```csharp
// Entity configuration
public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employee");

        // Primary key with value object
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .HasConversion(
                v => v.Value,
                v => new EmployeeId(v))
            .HasColumnName("EmployeeId");

        // Required properties
        builder.Property(e => e.EmployeeNumber)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        // Computed column
        builder.Property(e => e.FullName)
            .HasComputedColumnSql("[FirstName] + ' ' + [LastName]");

        // Enum conversion
        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        // Relationships
        builder.HasMany(e => e.Assignments)
            .WithOne(a => a.Employee)
            .HasForeignKey(a => a.EmployeeId)
            .OnDelete(DeleteBehavior.Restrict);

        // Indexes
        builder.HasIndex(e => e.EmployeeNumber)
            .IsUnique()
            .HasDatabaseName("IX_Employee_EmployeeNumber");

        builder.HasIndex(e => e.Status)
            .HasDatabaseName("IX_Employee_Status");

        // Temporal table configuration
        builder.ToTable(tb => tb.IsTemporal(ttb =>
        {
            ttb.HasPeriodStart("ValidFrom");
            ttb.HasPeriodEnd("ValidTo");
            ttb.UseHistoryTable("EmployeeHistory");
        }));

        // Soft delete query filter
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}
```

### 10.2 Migration Guidelines

```bash
# Create a migration
dotnet ef migrations add AddEmployeeEmail \
    --project src/Dorise.Incentive.Infrastructure \
    --startup-project src/Dorise.Incentive.Api \
    --context IncentiveDbContext

# Apply migrations
dotnet ef database update \
    --project src/Dorise.Incentive.Infrastructure \
    --startup-project src/Dorise.Incentive.Api

# Generate SQL script for production
dotnet ef migrations script \
    --project src/Dorise.Incentive.Infrastructure \
    --startup-project src/Dorise.Incentive.Api \
    --idempotent \
    --output migrations.sql
```

### 10.3 Unit of Work Pattern

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}

public class UnitOfWork : IUnitOfWork
{
    private readonly IncentiveDbContext _context;
    private readonly IDomainEventDispatcher _eventDispatcher;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(IncentiveDbContext context, IDomainEventDispatcher eventDispatcher)
    {
        _context = context;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        var entities = _context.ChangeTracker
            .Entries<IAggregateRoot>()
            .Where(e => e.Entity.DomainEvents.Any())
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities
            .SelectMany(e => e.DomainEvents)
            .ToList();

        entities.ForEach(e => e.ClearDomainEvents());

        // Save changes
        var result = await _context.SaveChangesAsync(cancellationToken);

        // Dispatch events after successful save
        foreach (var domainEvent in domainEvents)
        {
            await _eventDispatcher.DispatchAsync(domainEvent, cancellationToken);
        }

        return result;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.CommitAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(cancellationToken);
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }
}
```

---

## 11. API Guidelines

### 11.1 RESTful API Design

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Produces("application/json")]
public class EmployeesController : ControllerBase
{
    private readonly IMediator _mediator;

    public EmployeesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all employees with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<EmployeeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEmployees(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetEmployeesQuery(page, pageSize, status);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get employee by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEmployee(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetEmployeeByIdQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>
    /// Create a new employee
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateEmployee(
        [FromBody] CreateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            employee => CreatedAtAction(
                nameof(GetEmployee),
                new { id = employee.Id },
                employee),
            error => BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Update an existing employee
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(EmployeeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateEmployee(
        Guid id,
        [FromBody] UpdateEmployeeCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(new { Error = "ID mismatch" });

        var result = await _mediator.Send(command, cancellationToken);

        return result.Match(
            employee => Ok(employee),
            error => error.Contains("not found") ? NotFound() : BadRequest(new { Error = error }));
    }

    /// <summary>
    /// Delete (soft) an employee
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteEmployee(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteEmployeeCommand(id);
        var result = await _mediator.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : NotFound();
    }
}
```

### 11.2 API Response Standards

```csharp
// Standard success response
{
    "data": { /* response data */ },
    "meta": {
        "timestamp": "2025-01-15T10:30:00Z",
        "requestId": "abc123"
    }
}

// Paginated response
{
    "data": [ /* items */ ],
    "pagination": {
        "page": 1,
        "pageSize": 20,
        "totalCount": 150,
        "totalPages": 8,
        "hasNextPage": true,
        "hasPreviousPage": false
    }
}

// Error response (RFC 7807 Problem Details)
{
    "type": "https://tools.ietf.org/html/rfc7807",
    "title": "Validation Error",
    "status": 400,
    "detail": "One or more validation errors occurred.",
    "instance": "/api/v1/employees",
    "traceId": "00-abc123-def456-00",
    "errors": {
        "EmployeeNumber": ["Employee number is required."],
        "Email": ["Invalid email format."]
    }
}
```

---

## 12. Git Workflow

### 12.1 Branch Strategy

```
main                    # Production-ready code
├── develop             # Integration branch
├── feature/DSIF-123    # Feature branches
├── bugfix/DSIF-456     # Bug fix branches
├── hotfix/DSIF-789     # Production hotfixes
└── release/1.0.0       # Release branches
```

### 12.2 Commit Message Format

```
<type>(<scope>): <subject>

<body>

<footer>

# Types:
# feat:     New feature
# fix:      Bug fix
# docs:     Documentation
# style:    Formatting, missing semicolons
# refactor: Code change that neither fixes a bug nor adds a feature
# perf:     Performance improvement
# test:     Adding tests
# chore:    Maintenance tasks

# Examples:
feat(calculation): add support for tiered incentive slabs

- Implement SlabSelector service
- Add PlanSlab entity with threshold logic
- Update CalculationEngine to use tiered calculation

Closes DSIF-123

fix(api): handle null employee assignments in calculation

Previously, the calculation would throw NullReferenceException
when an employee had no active assignments. Now returns empty
result with appropriate warning.

Fixes DSIF-456
```

### 12.3 Code Review Checklist

- [ ] Code follows naming conventions
- [ ] Unit tests added/updated
- [ ] No hardcoded secrets or connection strings
- [ ] Async/await used correctly
- [ ] Error handling is appropriate
- [ ] Logging is meaningful and structured
- [ ] No N+1 query issues
- [ ] API follows REST conventions
- [ ] Documentation updated if needed
- [ ] No breaking changes (or documented if intentional)

---

## Appendix A: Quick Reference

### Common Commands

```bash
# Build solution
dotnet build

# Run tests
dotnet test

# Run API locally
dotnet run --project src/Dorise.Incentive.Api

# Add EF migration
dotnet ef migrations add <MigrationName> -p src/Dorise.Incentive.Infrastructure -s src/Dorise.Incentive.Api

# Update database
dotnet ef database update -p src/Dorise.Incentive.Infrastructure -s src/Dorise.Incentive.Api

# Format code
dotnet format

# Run code analysis
dotnet build /p:TreatWarningsAsErrors=true
```

### Useful Extensions (VS Code)

- C# Dev Kit
- .NET Core Test Explorer
- GitLens
- EditorConfig
- Azure Functions

---

> *"When I grow up, I want to be a principal or a caterpillar."* - Ralph Wiggum
>
> With these guidelines, our codebase can grow into whatever it needs to be!

---

**Document Owner:** Lead Developer (Claude Code)
**Review Cycle:** Quarterly
**Last Review:** January 2025

---

*This document is part of the DSIF Quality Gate Framework - QG-3 Deliverable*
