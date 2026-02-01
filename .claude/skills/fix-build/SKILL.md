---
name: fix-build
description: Systematic approach to fixing build errors
---

# Fix Build Errors Workflow

When fixing build errors, follow this systematic approach:

## Step 1: Categorize Errors

Group errors by type:

| Error Code | Meaning | Common Fix |
|------------|---------|------------|
| CS0234 | Namespace not found | Add using directive or package reference |
| CS0246 | Type not found | Add using directive or create missing type |
| CS0104 | Ambiguous reference | Use type alias or fully qualified name |
| CS0535 | Interface not implemented | Add missing methods or fix base class |
| CS1061 | Member not found | Check property/method name, use correct accessor |
| CS1503 | Argument type mismatch | Use named parameters or fix types |

## Step 2: Fix in Dependency Order

Fix errors starting from lowest-level projects:
1. Domain (no dependencies)
2. Application (depends on Domain)
3. Infrastructure (depends on Application, Domain)
4. Api (depends on all)

## Step 3: Common Fixes

### CS0234: Missing ASP.NET Core types in Infrastructure
```xml
<!-- Add to Infrastructure.csproj -->
<ItemGroup>
  <FrameworkReference Include="Microsoft.AspNetCore.App" />
</ItemGroup>
```

### CS0104: Ambiguous IAuthorizationService
```csharp
using IAppAuthorizationService = Dorise.Incentive.Application.Security.Services.IAuthorizationService;
```

### CS0535: Repository not implementing IRepository<T>
```csharp
// Use RepositoryBase<T> not Repository<T>
public class FooRepository : RepositoryBase<Foo>, IFooRepository
```

### CS1061: Property not found on value object
```csharp
// Access through value object structure
assignment.EffectivePeriod.StartDate  // Not assignment.EffectiveFrom
```

### CS1503: CancellationToken in wrong position
```csharp
// Use named parameters
await _repo.GetAsync(id, period, cancellationToken: cancellationToken);
```

## Step 4: Verify Fix

After each group of fixes:
```bash
dotnet build
```

Continue until 0 errors.
