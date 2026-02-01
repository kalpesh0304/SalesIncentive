# DSIF Developer Guide (CLAUDE.md)

> *"I'm learnding!"* - Ralph Wiggum

**Project:** Dorise Sales Incentive Framework | **.NET 8.0** | **C# 12** | **EF Core 8.0**

---

## Quick Reference

### Build & Test Commands
```bash
dotnet build                                    # Build solution (MUST pass before commit)
dotnet test --no-build                          # Run all tests
dotnet run --project src/Dorise.Incentive.Api  # Run API locally
dotnet format                                   # Format code
```

### EF Core Migrations
```bash
dotnet ef migrations add <Name> -p src/Dorise.Incentive.Infrastructure -s src/Dorise.Incentive.Api
dotnet ef database update -p src/Dorise.Incentive.Infrastructure -s src/Dorise.Incentive.Api
```

### Verification Checklist (REQUIRED after changes)
1. `dotnet build` - **Must pass with 0 errors**
2. `dotnet test --no-build` - **All tests must pass**
3. New repositories registered in `Infrastructure/DependencyInjection.cs`
4. New entities have DbSet in `IncentiveDbContext.cs`

---

## Common Mistakes to Avoid

### Repository Pattern
```csharp
// ❌ WRONG - Repository<T> doesn't exist
public class FooRepository : Repository<Foo>

// ✅ RIGHT - Use RepositoryBase<T>
public class FooRepository : RepositoryBase<Foo>, IFooRepository
{
    public FooRepository(IncentiveDbContext context) : base(context) { }

    // ✅ Use 'Context' not '_context' (inherited from RepositoryBase)
    public async Task<Foo?> GetByNameAsync(string name, CancellationToken ct)
        => await Context.Foos.FirstOrDefaultAsync(f => f.Name == name, ct);
}
```

### Creating New Repositories (Complete Checklist)
1. Create `IFooRepository` in `Domain/Interfaces/` extending `IRepository<Foo>`
2. Create `FooRepository` in `Infrastructure/Persistence/Repositories/` extending `RepositoryBase<Foo>`
3. Register in `Infrastructure/DependencyInjection.cs`: `services.AddScoped<IFooRepository, FooRepository>();`
4. Add `DbSet<Foo> Foos => Set<Foo>();` to `IncentiveDbContext.cs`

### Missing Framework References
When Infrastructure uses ASP.NET Core types (`HttpContext`, `IAuthorizationHandler`, `RequestDelegate`):
```xml
<!-- Add to Dorise.Incentive.Infrastructure.csproj -->
<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```

### Ambiguous Type References
```csharp
// ❌ WRONG - CS0104: Ambiguous between Microsoft.AspNetCore.Authorization and Application.Security.Services
private readonly IAuthorizationService _authService;

// ✅ RIGHT - Use alias
using IAppAuthorizationService = Dorise.Incentive.Application.Security.Services.IAuthorizationService;
private readonly IAppAuthorizationService _authService;
```

### Missing Using Directives
Always include when working with domain code:
```csharp
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
```

### Method Signature Mismatches
```csharp
// ❌ WRONG - CancellationToken passed where Guid? expected
await _repo.GetByIdAsync(id, period, cancellationToken);

// ✅ RIGHT - Use named parameters
await _repo.GetByIdAsync(id, period, cancellationToken: cancellationToken);
```

### Value Object Property Access
```csharp
// ❌ WRONG - Direct property access on value objects
assignment.EffectiveFrom  // DateRange doesn't have this

// ✅ RIGHT - Access through value object structure
assignment.EffectivePeriod.StartDate
assignment.EffectivePeriod.EndDate
assignment.EffectivePeriod.Overlaps(otherPeriod)
```

---

## Project Structure

### Layer Dependencies (Inward Only)
```
Api ──────────────────┐
                      ▼
              ┌─ Application ─┐
              │               │
              ▼               ▼
         Domain ◄──── Infrastructure
```

### Where to Put Things
| Type | Location |
|------|----------|
| Domain entities | `Domain/Entities/` |
| Value objects | `Domain/ValueObjects/` |
| Repository interfaces | `Domain/Interfaces/` |
| Domain events | `Domain/Events/` |
| Repository implementations | `Infrastructure/Persistence/Repositories/` |
| DbContext & configs | `Infrastructure/Persistence/` |
| Command handlers | `Application/Commands/{Entity}/` |
| Query handlers | `Application/Queries/{Entity}/` |
| DTOs | `Application/DTOs/` |
| Validators | `Application/Validators/` |
| API controllers | `Api/Controllers/` |
| DI registration | `Infrastructure/DependencyInjection.cs` |

---

## Naming Conventions

| Element | Pattern | Example |
|---------|---------|---------|
| Repository interface | `I{Entity}Repository` | `IEmployeeRepository` |
| Repository class | `{Entity}Repository : RepositoryBase<{Entity}>` | `EmployeeRepository` |
| Command | `{Verb}{Entity}Command` | `CreateEmployeeCommand` |
| Command handler | `{Verb}{Entity}CommandHandler` | `CreateEmployeeCommandHandler` |
| Query | `Get{Entity}Query` / `Get{Entities}Query` | `GetEmployeeByIdQuery` |
| Domain event | `{Entity}{PastTenseVerb}Event` | `EmployeeCreatedEvent` |
| DTO | `{Entity}Dto` | `EmployeeDto` |
| Validator | `{Command}Validator` | `CreateEmployeeCommandValidator` |
| Private fields | `_camelCase` | `_employeeRepository` |
| Async methods | `{Method}Async` | `GetEmployeeAsync` |

---

## Architecture Patterns

### CQRS with MediatR
```csharp
// Commands modify state, return Result<T>
public record CreateEmployeeCommand(...) : IRequest<Result<EmployeeDto>>;

// Queries read state, return DTOs
public record GetEmployeeByIdQuery(Guid Id) : IRequest<EmployeeDto?>;
```

### Result Pattern for Commands
```csharp
public async Task<Result<EmployeeDto>> Handle(CreateEmployeeCommand request, CancellationToken ct)
{
    var existing = await _repository.GetByNumberAsync(request.Number, ct);
    if (existing is not null)
        return Result<EmployeeDto>.Failure("Employee already exists.", "DUPLICATE");

    // ... create and save
    return Result<EmployeeDto>.Success(dto);
}
```

### Domain Entity Pattern
```csharp
public class Employee : BaseEntity, IAggregateRoot
{
    private Employee() { } // EF Core constructor

    public Employee(EmployeeId id, string number, string firstName, string lastName)
    {
        // Validate and set properties
        AddDomainEvent(new EmployeeCreatedEvent(Id));
    }

    // Behavior methods that enforce invariants
    public void Terminate(DateTime date) { ... }
}
```

---

## Slash Commands

| Command | Purpose |
|---------|---------|
| `/init` | Generate starter CLAUDE.md from project |
| `/clear` | Reset context between unrelated tasks |
| `/compact` | Compress conversation, preserve key context |
| `/rewind` | Restore previous conversation/code state |
| `/permissions` | Configure allowed tools and domains |
| `/hooks` | Configure automation hooks |
| `/help` | Show available commands |

---

## Git Workflow

### Branch Naming
```
feature/DSIF-123-add-oauth    # New features
bugfix/DSIF-456-fix-calc      # Bug fixes
hotfix/DSIF-789-security      # Production hotfixes
```

### Commit Message Format
```
<type>(<scope>): <subject>

Types: feat, fix, docs, style, refactor, perf, test, chore

Examples:
feat(calculation): add tiered slab support
fix(api): handle null assignments in calculation
```

---

## Code Review Checklist

- [ ] `dotnet build` passes
- [ ] Tests added/updated and passing
- [ ] No hardcoded secrets or connection strings
- [ ] Async/await used correctly with CancellationToken
- [ ] New repositories properly registered
- [ ] Logging uses structured format (not string interpolation)
- [ ] No N+1 query issues (check Includes)

---

## Key Files Reference

| Purpose | File |
|---------|------|
| DI Registration | `Infrastructure/DependencyInjection.cs` |
| DbContext | `Infrastructure/Persistence/IncentiveDbContext.cs` |
| API Entry | `Api/Program.cs` |
| Base Repository | `Infrastructure/Persistence/Repositories/RepositoryBase.cs` |
| Base Entity | `Domain/Common/BaseEntity.cs` |
| Result Type | `Application/Common/Interfaces/Result.cs` |

---

> *"When I grow up, I want to be a principal or a caterpillar."* - Ralph Wiggum
>
> With these guidelines, the codebase grows correctly!

**Document Owner:** Development Team | **Review Cycle:** On error discovery
