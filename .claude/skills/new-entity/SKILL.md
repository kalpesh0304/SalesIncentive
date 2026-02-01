---
name: new-entity
description: Create a new domain entity with all required components
---

# Create New Domain Entity Workflow

When asked to create a new entity `{EntityName}`:

## Step 1: Create Domain Entity

Create `src/Dorise.Incentive.Domain/Entities/{EntityName}.cs`:

```csharp
using Dorise.Incentive.Domain.Common;
using Dorise.Incentive.Domain.Events;

namespace Dorise.Incentive.Domain.Entities;

public class {EntityName} : BaseEntity, IAggregateRoot
{
    // Required properties
    public string Name { get; private set; } = null!;
    public bool IsActive { get; private set; }

    // EF Core constructor
    private {EntityName}() { }

    // Factory method
    public static {EntityName} Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required", nameof(name));

        var entity = new {EntityName}
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            IsActive = true
        };

        entity.AddDomainEvent(new {EntityName}CreatedEvent(entity.Id));
        return entity;
    }

    // Behavior methods
    public void Deactivate()
    {
        IsActive = false;
        ModifiedAt = DateTime.UtcNow;
    }
}
```

## Step 2: Create Domain Event (if needed)

Create `src/Dorise.Incentive.Domain/Events/{EntityName}CreatedEvent.cs`:

```csharp
namespace Dorise.Incentive.Domain.Events;

public record {EntityName}CreatedEvent(Guid {EntityName}Id) : IDomainEvent;
```

## Step 3: Create Repository Interface

See @new-repository skill.

## Step 4: Create EF Core Configuration

Create `src/Dorise.Incentive.Infrastructure/Persistence/Configurations/{EntityName}Configuration.cs`:

```csharp
using Dorise.Incentive.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dorise.Incentive.Infrastructure.Persistence.Configurations;

public class {EntityName}Configuration : IEntityTypeConfiguration<{EntityName}>
{
    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        builder.ToTable("{EntityName}");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(e => e.Name)
            .IsUnique();
    }
}
```

## Step 5: Add DbSet to Context

Add to `IncentiveDbContext.cs`:
```csharp
public DbSet<{EntityName}> {EntityName}s => Set<{EntityName}>();
```

## Step 6: Create DTO

Create `src/Dorise.Incentive.Application/DTOs/{EntityName}Dto.cs`:

```csharp
namespace Dorise.Incentive.Application.DTOs;

public record {EntityName}Dto(
    Guid Id,
    string Name,
    bool IsActive,
    DateTime CreatedAt);
```

## Step 7: Verify

```bash
dotnet build
dotnet ef migrations add Add{EntityName} -p src/Dorise.Incentive.Infrastructure -s src/Dorise.Incentive.Api
```
