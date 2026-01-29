# DSIF Development Plan

**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Created:** January 2025
**Author:** Claude Code (Lead Developer)

> *"I'm Idaho!"* - Ralph Wiggum
>
> And we're CODING - building the incentive system piece by piece!

---

## Table of Contents

1. [Overview](#overview)
2. [Technology Stack](#technology-stack)
3. [Solution Architecture](#solution-architecture)
4. [Sprint Plan](#sprint-plan)
5. [Development Phases](#development-phases)
6. [Coding Standards](#coding-standards)
7. [Implementation Order](#implementation-order)

---

## 1. Overview

### Project Summary

The Dorise Sales Incentive Framework (DSIF) is a cloud-native enterprise application for managing sales incentive calculations, approvals, and payouts. This document outlines the development plan for implementing the system based on completed documentation from QG-1 through QG-5.

### Key Features

| Feature | Priority | Sprint |
|---------|----------|--------|
| Employee Management | High | 1-2 |
| Incentive Plan Configuration | Critical | 2-3 |
| Calculation Engine | Critical | 3-4 |
| Approval Workflows | High | 4-5 |
| Reporting & Analytics | Medium | 5-6 |
| External Integrations | Medium | 6-7 |

---

## 2. Technology Stack

> *"Me fail English? That's unpossible!"* - Ralph Wiggum
>
> Our tech stack is very possible - industry-proven and enterprise-ready!

### Backend

| Technology | Version | Purpose |
|------------|---------|---------|
| .NET | 8.0 | Runtime framework |
| C# | 12.0 | Primary language |
| ASP.NET Core | 8.0 | Web API framework |
| Entity Framework Core | 8.0 | ORM |
| MediatR | 12.x | CQRS mediator |
| FluentValidation | 11.x | Input validation |
| AutoMapper | 12.x | Object mapping |
| Serilog | 3.x | Structured logging |

### Database

| Technology | Purpose |
|------------|---------|
| SQL Server 2022 | Primary database |
| Azure SQL Database | Cloud hosting |
| Temporal Tables | Point-in-time queries |

### Infrastructure

| Technology | Purpose |
|------------|---------|
| Azure App Service | API hosting |
| Azure Key Vault | Secrets management |
| Azure Application Insights | Monitoring |
| Azure Redis Cache | Distributed caching |
| GitHub Actions | CI/CD |

---

## 3. Solution Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                             │
│         (Controllers, DTOs, Middleware, Filters)            │
├─────────────────────────────────────────────────────────────┤
│                    Application Layer                         │
│      (Commands, Queries, Handlers, Validators, DTOs)        │
├─────────────────────────────────────────────────────────────┤
│                      Domain Layer                            │
│    (Entities, Value Objects, Domain Services, Events)       │
├─────────────────────────────────────────────────────────────┤
│                   Infrastructure Layer                       │
│  (Repositories, External Services, Database, Caching)       │
└─────────────────────────────────────────────────────────────┘
```

### Project Structure

```
src/
├── Dorise.Incentive.Domain/           # Domain layer
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Enums/
│   ├── Events/
│   ├── Exceptions/
│   ├── Interfaces/
│   └── Services/
│
├── Dorise.Incentive.Application/      # Application layer
│   ├── Common/
│   │   ├── Behaviors/
│   │   ├── Interfaces/
│   │   └── Mappings/
│   ├── Employees/
│   │   ├── Commands/
│   │   ├── Queries/
│   │   └── DTOs/
│   ├── Plans/
│   ├── Calculations/
│   └── Approvals/
│
├── Dorise.Incentive.Infrastructure/   # Infrastructure layer
│   ├── Persistence/
│   │   ├── Configurations/
│   │   ├── Repositories/
│   │   └── Migrations/
│   ├── Services/
│   └── Caching/
│
├── Dorise.Incentive.Api/              # API layer
│   ├── Controllers/
│   ├── Middleware/
│   ├── Filters/
│   └── Extensions/
│
└── tests/
    ├── Dorise.Incentive.Domain.Tests/
    ├── Dorise.Incentive.Application.Tests/
    ├── Dorise.Incentive.Infrastructure.Tests/
    └── Dorise.Incentive.Api.Tests/
```

---

## 4. Sprint Plan

> *"The doctor said I wouldn't have so many nose bleeds if I kept my finger outta there."* - Ralph Wiggum
>
> Our sprint plan keeps us on track - no bleeding deadlines here!

### Sprint Overview (2-week sprints)

| Sprint | Focus | Deliverables |
|--------|-------|--------------|
| Sprint 1 | Foundation | Solution setup, Domain entities, Database schema |
| Sprint 2 | Employee Module | Employee CRUD, Plan assignment, Eligibility |
| Sprint 3 | Plan Module | Plan configuration, Slabs, Rules engine |
| Sprint 4 | Calculation Engine | Core calculations, Proration, Batch processing |
| Sprint 5 | Approval Workflow | Submission, Approval chain, Notifications |
| Sprint 6 | Reporting | Reports, Exports, Dashboards |
| Sprint 7 | Integration | ERP, HR, Payroll integrations |
| Sprint 8 | Polish & Deploy | Bug fixes, Performance, Production deployment |

---

## 5. Development Phases

### Phase 1: Foundation (Sprint 1)

#### Week 1: Solution Setup
- [x] Create solution structure
- [x] Configure project references
- [x] Set up dependency injection
- [x] Configure Entity Framework Core
- [x] Create base entities and interfaces

#### Week 2: Domain Layer
- [x] Implement Employee entity
- [x] Implement IncentivePlan entity
- [x] Implement Calculation entity
- [x] Create value objects (Money, Percentage, DateRange)
- [x] Define domain events

### Phase 2: Core Modules (Sprints 2-4)

#### Sprint 2: Employee Management
- Employee CRUD operations
- Plan assignment workflow
- Eligibility determination
- Bulk import functionality

#### Sprint 3: Incentive Plans
- Plan CRUD operations
- Slab configuration
- Business rules engine
- Plan activation workflow

#### Sprint 4: Calculation Engine
- Single employee calculation
- Slab application logic
- Proration handling
- Batch calculation processing
- Adjustment workflows

### Phase 3: Workflows (Sprint 5)

#### Approval System
- Submission workflow
- Multi-level approval chain
- Delegation and escalation
- Notification system

### Phase 4: Reporting (Sprint 6)

#### Reports & Analytics
- Payout reports
- Achievement summaries
- Variance analysis
- Export functionality

### Phase 5: Integration (Sprint 7)

#### External Systems
- ERP integration (sales data)
- HR integration (employee sync)
- Payroll integration (payout export)

### Phase 6: Production (Sprint 8)

#### Final Preparation
- Performance optimization
- Security hardening
- Production deployment
- Documentation updates

---

## 6. Coding Standards

> *"I bent my Wookiee!"* - Ralph Wiggum
>
> We won't bend our code - following strict standards keeps it strong!

### Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| Class | PascalCase | `IncentiveCalculation` |
| Interface | IPascalCase | `ICalculationService` |
| Method | PascalCase | `CalculateIncentive()` |
| Property | PascalCase | `EmployeeId` |
| Private field | _camelCase | `_repository` |
| Parameter | camelCase | `employeeId` |
| Constant | UPPER_CASE | `MAX_SLAB_COUNT` |

### File Organization

```csharp
// 1. Namespace
namespace Dorise.Incentive.Domain.Entities;

// 2. Using statements (inside namespace)
// 3. Class definition
public class Employee : BaseEntity, IAggregateRoot
{
    // 4. Constants
    // 5. Private fields
    // 6. Constructors
    // 7. Properties
    // 8. Public methods
    // 9. Private methods
}
```

### CQRS Pattern

```csharp
// Command
public record CreateEmployeeCommand(
    string EmployeeCode,
    string FirstName,
    string LastName,
    string Email,
    Guid DepartmentId) : IRequest<Guid>;

// Handler
public class CreateEmployeeCommandHandler
    : IRequestHandler<CreateEmployeeCommand, Guid>
{
    public async Task<Guid> Handle(
        CreateEmployeeCommand request,
        CancellationToken cancellationToken)
    {
        // Implementation
    }
}
```

---

## 7. Implementation Order

### Domain Layer (First)

1. **Base Classes**
   - `BaseEntity` - Common entity properties
   - `AuditableEntity` - Audit fields
   - `IAggregateRoot` - Aggregate marker

2. **Value Objects**
   - `Money` - Currency amounts
   - `Percentage` - Percentage values
   - `DateRange` - Period definitions
   - `EmployeeCode` - Validated employee code

3. **Enumerations**
   - `EmployeeStatus`
   - `PlanStatus`
   - `CalculationStatus`
   - `ApprovalStatus`

4. **Core Entities**
   - `Employee`
   - `Department`
   - `IncentivePlan`
   - `Slab`
   - `Calculation`
   - `Approval`

5. **Domain Services**
   - `ICalculationDomainService`
   - `ISlabSelectionService`
   - `IProrationService`

### Application Layer (Second)

1. **Common Infrastructure**
   - `IApplicationDbContext`
   - `ValidationBehavior`
   - `LoggingBehavior`

2. **Employee Features**
   - `CreateEmployeeCommand`
   - `UpdateEmployeeCommand`
   - `GetEmployeeQuery`
   - `GetEmployeesQuery`

3. **Plan Features**
   - `CreatePlanCommand`
   - `ActivatePlanCommand`
   - `GetPlanQuery`

4. **Calculation Features**
   - `CalculateIncentiveCommand`
   - `BatchCalculateCommand`
   - `GetCalculationQuery`

### Infrastructure Layer (Third)

1. **Database**
   - `ApplicationDbContext`
   - Entity configurations
   - Migrations

2. **Repositories**
   - `EmployeeRepository`
   - `PlanRepository`
   - `CalculationRepository`

3. **Services**
   - `DateTimeService`
   - `CurrentUserService`

### API Layer (Fourth)

1. **Controllers**
   - `EmployeesController`
   - `PlansController`
   - `CalculationsController`
   - `ApprovalsController`

2. **Middleware**
   - Exception handling
   - Request logging
   - Authentication

---

## Development Checklist

### Sprint 1 Deliverables

- [ ] Solution structure created
- [ ] Domain entities implemented
- [ ] Value objects implemented
- [ ] Database context configured
- [ ] Initial migration created
- [ ] Unit tests for domain layer
- [ ] API scaffolding complete

---

## Sign-off

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Lead Developer | Claude Code | ✅ Approved | January 2025 |
| Solution Architect | Skanda Prasad | _____________ | ______ |

---

*This document is part of the DSIF Development Documentation.*
