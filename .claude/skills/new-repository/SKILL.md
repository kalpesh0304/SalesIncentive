---
name: new-repository
description: Create a new repository with interface and implementation
---

# Create New Repository Workflow

When asked to create a new repository for entity `{EntityName}`:

## Step 1: Create Interface in Domain Layer

Create `src/Dorise.Incentive.Domain/Interfaces/I{EntityName}Repository.cs`:

```csharp
using Dorise.Incentive.Domain.Entities;

namespace Dorise.Incentive.Domain.Interfaces;

public interface I{EntityName}Repository : IRepository<{EntityName}>
{
    // Add entity-specific query methods
    Task<{EntityName}?> GetByNameAsync(string name, CancellationToken cancellationToken = default);
}
```

## Step 2: Create Implementation in Infrastructure Layer

Create `src/Dorise.Incentive.Infrastructure/Persistence/Repositories/{EntityName}Repository.cs`:

```csharp
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dorise.Incentive.Infrastructure.Persistence.Repositories;

public class {EntityName}Repository : RepositoryBase<{EntityName}>, I{EntityName}Repository
{
    public {EntityName}Repository(IncentiveDbContext context) : base(context)
    {
    }

    public async Task<{EntityName}?> GetByNameAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return await DbSet
            .FirstOrDefaultAsync(e => e.Name == name, cancellationToken);
    }
}
```

## Step 3: Register in DependencyInjection.cs

Add to `src/Dorise.Incentive.Infrastructure/DependencyInjection.cs`:

```csharp
services.AddScoped<I{EntityName}Repository, {EntityName}Repository>();
```

## Step 4: Verify DbSet Exists

Ensure `IncentiveDbContext.cs` has:
```csharp
public DbSet<{EntityName}> {EntityName}s => Set<{EntityName}>();
```

## Step 5: Verify Build

Run `dotnet build` to confirm no errors.
