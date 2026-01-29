# DORISE Sales Incentive Framework
## Solution Architecture

**Document ID:** DOC-ARCH-001
**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Created:** January 2025
**Last Updated:** January 2025
**Author:** Claude Code
**Status:** Draft

> *"When I grow up, I want to be a principal or a caterpillar."* - Ralph Wiggum
>
> Our architecture can grow into whatever it needs to be - flexible, scalable, and enterprise-ready.

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Architecture Principles](#2-architecture-principles)
3. [Solution Overview](#3-solution-overview)
4. [Logical Architecture](#4-logical-architecture)
5. [Physical Architecture](#5-physical-architecture)
6. [Component Design](#6-component-design)
7. [Integration Architecture](#7-integration-architecture)
8. [Deployment Architecture](#8-deployment-architecture)
9. [Scalability & Performance](#9-scalability--performance)
10. [Disaster Recovery](#10-disaster-recovery)
11. [Technology Stack](#11-technology-stack)

---

## 1. Executive Summary

### 1.1 Purpose

This document describes the solution architecture for the Dorise Sales Incentive Framework (DSIF), a cloud-native application built on Microsoft Azure to automate sales incentive calculations and management.

### 1.2 Scope

The architecture covers:
- Web application for user interaction
- REST APIs for system integration
- Background processing for batch calculations
- Data persistence and caching
- Security and identity management
- Monitoring and operations

### 1.3 Key Architectural Decisions

| Decision | Choice | Rationale |
|----------|--------|-----------|
| Cloud Platform | Microsoft Azure | Enterprise standard, PaaS offerings |
| Runtime | .NET 8.0 | Performance, long-term support |
| Architecture Style | Layered + Event-Driven | Separation of concerns, async processing |
| Data Store | Azure SQL Database | ACID compliance, audit requirements |
| Identity | Azure Entra ID | Enterprise SSO, MFA support |
| Hosting | Azure App Service | Managed PaaS, auto-scaling |

---

## 2. Architecture Principles

### 2.1 Core Principles

| Principle | Description | Application |
|-----------|-------------|-------------|
| **Cloud-Native** | Design for cloud scalability and resilience | Use PaaS services, design for failure |
| **Security by Design** | Security at every layer | Zero trust, encryption, least privilege |
| **Immutability** | Audit trail integrity | Append-only records, temporal tables |
| **Loose Coupling** | Independent components | API contracts, message-based integration |
| **Configuration over Code** | Business rule flexibility | Configurable formulas and workflows |

### 2.2 Design Guidelines

1. **Stateless Services** - No server-side session state; use distributed cache
2. **Idempotent Operations** - Safe to retry any operation
3. **Eventual Consistency** - Accept async processing where appropriate
4. **Defense in Depth** - Multiple security layers
5. **Observable Systems** - Comprehensive logging and monitoring

> *"I'm learnding!"* - Ralph Wiggum
>
> These principles guide our learning journey toward building robust enterprise software.

---

## 3. Solution Overview

### 3.1 Context Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              EXTERNAL ACTORS                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────┐   ┌──────────┐   ┌──────────┐   ┌──────────┐   ┌──────────┐  │
│  │  Sales   │   │ Finance  │   │  Sales   │   │  System  │   │ Auditor  │  │
│  │   Rep    │   │ Manager  │   │ Manager  │   │  Admin   │   │          │  │
│  └────┬─────┘   └────┬─────┘   └────┬─────┘   └────┬─────┘   └────┬─────┘  │
│       │              │              │              │              │         │
└───────┼──────────────┼──────────────┼──────────────┼──────────────┼─────────┘
        │              │              │              │              │
        └──────────────┴──────────────┼──────────────┴──────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                                                                              │
│                    ╔═══════════════════════════════════╗                    │
│                    ║   DORISE SALES INCENTIVE SYSTEM   ║                    │
│                    ║                                   ║                    │
│                    ║  • Employee Management            ║                    │
│                    ║  • Incentive Configuration        ║                    │
│                    ║  • Calculation Engine             ║                    │
│                    ║  • Approval Workflow              ║                    │
│                    ║  • Reporting & Analytics          ║                    │
│                    ║  • Audit Trail                    ║                    │
│                    ╚═══════════════════════════════════╝                    │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
        │              │              │              │
        ▼              ▼              ▼              ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           EXTERNAL SYSTEMS                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐    │
│  │    Payroll   │  │     ERP      │  │   HR System  │  │  Azure AD    │    │
│  │    System    │  │    System    │  │              │  │  (Entra ID)  │    │
│  └──────────────┘  └──────────────┘  └──────────────┘  └──────────────┘    │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Solution Boundaries

| Boundary | In Scope | Out of Scope |
|----------|----------|--------------|
| **Calculations** | Incentive formulas, slabs, proration | Tax calculations, deductions |
| **Users** | Dorise employees with Entra ID accounts | External customers, vendors |
| **Data** | Incentive-related data, audit logs | Full HR records, payroll data |
| **Integration** | API-based data exchange | Direct database connections |

---

## 4. Logical Architecture

### 4.1 Layer Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           PRESENTATION LAYER                                 │
│  ┌─────────────────────────────┐  ┌─────────────────────────────────────┐   │
│  │      Blazor Web App         │  │         REST API (Swagger)          │   │
│  │   • Employee Portal         │  │   • External Integration            │   │
│  │   • Admin Dashboard         │  │   • Mobile Support                  │   │
│  │   • Reports & Analytics     │  │   • Third-Party Access              │   │
│  └─────────────────────────────┘  └─────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           APPLICATION LAYER                                  │
│  ┌───────────────┐ ┌───────────────┐ ┌───────────────┐ ┌───────────────┐   │
│  │   Employee    │ │   Incentive   │ │  Calculation  │ │   Approval    │   │
│  │   Service     │ │   Service     │ │   Service     │ │   Service     │   │
│  └───────────────┘ └───────────────┘ └───────────────┘ └───────────────┘   │
│  ┌───────────────┐ ┌───────────────┐ ┌───────────────┐ ┌───────────────┐   │
│  │   Reporting   │ │    User       │ │  Integration  │ │    Audit      │   │
│  │   Service     │ │   Service     │ │   Service     │ │   Service     │   │
│  └───────────────┘ └───────────────┘ └───────────────┘ └───────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                            DOMAIN LAYER                                      │
│  ┌───────────────┐ ┌───────────────┐ ┌───────────────┐ ┌───────────────┐   │
│  │   Employee    │ │ IncentivePlan │ │  Calculation  │ │   Approval    │   │
│  │   Aggregate   │ │   Aggregate   │ │   Aggregate   │ │   Aggregate   │   │
│  └───────────────┘ └───────────────┘ └───────────────┘ └───────────────┘   │
│  ┌─────────────────────────────────────────────────────────────────────┐   │
│  │                        Domain Events                                 │   │
│  │  CalculationCompleted | ApprovalRequested | PlanActivated | ...      │   │
│  └─────────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         INFRASTRUCTURE LAYER                                 │
│  ┌───────────────┐ ┌───────────────┐ ┌───────────────┐ ┌───────────────┐   │
│  │  Repository   │ │    Cache      │ │   Message     │ │   External    │   │
│  │  (EF Core)    │ │   (Redis)     │ │   Queue       │ │   APIs        │   │
│  └───────────────┘ └───────────────┘ └───────────────┘ └───────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         DATA & SERVICES LAYER                                │
│  ┌───────────────┐ ┌───────────────┐ ┌───────────────┐ ┌───────────────┐   │
│  │   Azure SQL   │ │  Blob Storage │ │  Redis Cache  │ │  Key Vault    │   │
│  │   Database    │ │               │ │               │ │               │   │
│  └───────────────┘ └───────────────┘ └───────────────┘ └───────────────┘   │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Layer Responsibilities

| Layer | Responsibility | Key Technologies |
|-------|----------------|------------------|
| **Presentation** | UI, API endpoints, request handling | Blazor, ASP.NET Core Web API |
| **Application** | Business workflows, orchestration | MediatR, FluentValidation |
| **Domain** | Business logic, rules, entities | DDD patterns, Value Objects |
| **Infrastructure** | Data access, external services | EF Core, HttpClient |
| **Data & Services** | Persistence, caching, secrets | Azure PaaS services |

---

## 5. Physical Architecture

### 5.1 Azure Resource Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        AZURE SUBSCRIPTION                                    │
│                     (sub-dorise-prod-001)                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │                   RESOURCE GROUP: rg-dorise-prod-eastus               │  │
│  │                                                                        │  │
│  │  ┌─────────────────────────────────────────────────────────────────┐  │  │
│  │  │                    NETWORKING                                    │  │  │
│  │  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │  │  │
│  │  │  │   App GW    │  │    WAF      │  │   VNet Integration      │  │  │  │
│  │  │  │   + CDN     │  │             │  │   (Private Endpoints)   │  │  │  │
│  │  │  └─────────────┘  └─────────────┘  └─────────────────────────┘  │  │  │
│  │  └─────────────────────────────────────────────────────────────────┘  │  │
│  │                                                                        │  │
│  │  ┌─────────────────────────────────────────────────────────────────┐  │  │
│  │  │                    COMPUTE                                       │  │  │
│  │  │  ┌─────────────────────┐  ┌─────────────────────────────────┐   │  │  │
│  │  │  │   App Service Plan  │  │      Azure Functions            │   │  │  │
│  │  │  │   (Premium v3)      │  │      (Consumption/Premium)      │   │  │  │
│  │  │  │                     │  │                                 │   │  │  │
│  │  │  │  ┌───────────────┐  │  │  ┌───────────────────────────┐  │   │  │  │
│  │  │  │  │ Web App (API) │  │  │  │ Calculation Trigger       │  │   │  │  │
│  │  │  │  │ + Blazor UI   │  │  │  │ Import Job                │  │   │  │  │
│  │  │  │  └───────────────┘  │  │  │ Export Job                │  │   │  │  │
│  │  │  │                     │  │  │ Notification Service      │  │   │  │  │
│  │  │  └─────────────────────┘  │  └───────────────────────────┘  │   │  │  │
│  │  │                           └─────────────────────────────────┘   │  │  │
│  │  └─────────────────────────────────────────────────────────────────┘  │  │
│  │                                                                        │  │
│  │  ┌─────────────────────────────────────────────────────────────────┐  │  │
│  │  │                    DATA & STORAGE                                │  │  │
│  │  │  ┌───────────────┐  ┌───────────────┐  ┌───────────────────┐    │  │  │
│  │  │  │  Azure SQL    │  │ Blob Storage  │  │   Redis Cache     │    │  │  │
│  │  │  │  (Premium)    │  │ (Hot + Cool)  │  │   (Standard)      │    │  │  │
│  │  │  └───────────────┘  └───────────────┘  └───────────────────┘    │  │  │
│  │  └─────────────────────────────────────────────────────────────────┘  │  │
│  │                                                                        │  │
│  │  ┌─────────────────────────────────────────────────────────────────┐  │  │
│  │  │                    SECURITY & IDENTITY                           │  │  │
│  │  │  ┌───────────────┐  ┌───────────────┐  ┌───────────────────┐    │  │  │
│  │  │  │   Key Vault   │  │ Managed       │  │   Entra ID        │    │  │  │
│  │  │  │               │  │ Identity      │  │   App Registration│    │  │  │
│  │  │  └───────────────┘  └───────────────┘  └───────────────────┘    │  │  │
│  │  └─────────────────────────────────────────────────────────────────┘  │  │
│  │                                                                        │  │
│  │  ┌─────────────────────────────────────────────────────────────────┐  │  │
│  │  │                    MONITORING                                    │  │  │
│  │  │  ┌───────────────┐  ┌───────────────┐  ┌───────────────────┐    │  │  │
│  │  │  │ App Insights  │  │ Log Analytics │  │   Azure Monitor   │    │  │  │
│  │  │  │               │  │  Workspace    │  │   Alerts          │    │  │  │
│  │  │  └───────────────┘  └───────────────┘  └───────────────────┘    │  │  │
│  │  └─────────────────────────────────────────────────────────────────┘  │  │
│  │                                                                        │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5.2 Environment Strategy

| Environment | Purpose | Resource Suffix | SKU Level |
|-------------|---------|-----------------|-----------|
| **Development** | Feature development | `-dev` | Basic/Standard |
| **Staging** | Integration testing, UAT | `-stg` | Standard |
| **Production** | Live system | `-prod` | Premium |

### 5.3 Resource Naming Convention

```
{resource-type}-{project}-{component}-{environment}-{region}

Examples:
- app-dorise-api-prod-eastus
- sql-dorise-main-prod-eastus
- kv-dorise-secrets-prod-eastus
- st-dorise-files-prod-eastus
```

---

## 6. Component Design

### 6.1 Component Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              WEB APPLICATION                                 │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                    Dorise.Incentive.Web                             │    │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐   │    │
│  │  │  Employee   │ │  Incentive  │ │ Calculation │ │  Approval   │   │    │
│  │  │   Pages     │ │   Pages     │ │   Pages     │ │   Pages     │   │    │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘   │    │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐   │    │
│  │  │  Dashboard  │ │  Reports    │ │   Admin     │ │   Shared    │   │    │
│  │  │   Pages     │ │   Pages     │ │   Pages     │ │ Components  │   │    │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘   │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                      │                                       │
└──────────────────────────────────────┼───────────────────────────────────────┘
                                       │ HTTP
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              WEB API                                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                    Dorise.Incentive.Api                             │    │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐   │    │
│  │  │ Employees   │ │ Incentive   │ │ Calculations│ │ Approvals   │   │    │
│  │  │ Controller  │ │ Controller  │ │ Controller  │ │ Controller  │   │    │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘   │    │
│  │  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐   │    │
│  │  │  Reports    │ │   Users     │ │ Integration │ │   Health    │   │    │
│  │  │ Controller  │ │ Controller  │ │ Controller  │ │ Controller  │   │    │
│  │  └─────────────┘ └─────────────┘ └─────────────┘ └─────────────┘   │    │
│  │  ┌─────────────────────────────────────────────────────────────┐   │    │
│  │  │  Middleware: Auth | Logging | Exception | Validation        │   │    │
│  │  └─────────────────────────────────────────────────────────────┘   │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                      │                                       │
└──────────────────────────────────────┼───────────────────────────────────────┘
                                       │ MediatR
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           APPLICATION SERVICES                               │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                 Dorise.Incentive.Application                        │    │
│  │                                                                      │    │
│  │  Commands:                        Queries:                          │    │
│  │  ┌─────────────────────────┐     ┌─────────────────────────┐       │    │
│  │  │ CreateEmployeeCommand   │     │ GetEmployeeQuery        │       │    │
│  │  │ UpdatePlanCommand       │     │ GetCalculationsQuery    │       │    │
│  │  │ RunCalculationCommand   │     │ GetPendingApprovalsQuery│       │    │
│  │  │ ApproveIncentiveCommand │     │ GetDashboardQuery       │       │    │
│  │  └─────────────────────────┘     └─────────────────────────┘       │    │
│  │                                                                      │    │
│  │  Validators:                      Event Handlers:                   │    │
│  │  ┌─────────────────────────┐     ┌─────────────────────────┐       │    │
│  │  │ EmployeeValidator       │     │ CalculationCompletedHandler│    │    │
│  │  │ PlanValidator           │     │ ApprovalRequestedHandler │      │    │
│  │  │ CalculationValidator    │     │ PlanActivatedHandler     │      │    │
│  │  └─────────────────────────┘     └─────────────────────────┘       │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                      │                                       │
└──────────────────────────────────────┼───────────────────────────────────────┘
                                       │
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                              DOMAIN                                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                    Dorise.Incentive.Core                            │    │
│  │                                                                      │    │
│  │  Entities:                        Value Objects:                    │    │
│  │  ┌─────────────────────────┐     ┌─────────────────────────┐       │    │
│  │  │ Employee                │     │ Money                   │       │    │
│  │  │ IncentivePlan           │     │ Percentage              │       │    │
│  │  │ PlanSlab                │     │ DateRange               │       │    │
│  │  │ Calculation             │     │ EmployeeId              │       │    │
│  │  │ CalculationDetail       │     │ PlanId                  │       │    │
│  │  │ Approval                │     │ CalculationId           │       │    │
│  │  │ AuditLog                │     │ ApprovalStatus          │       │    │
│  │  └─────────────────────────┘     └─────────────────────────┘       │    │
│  │                                                                      │    │
│  │  Domain Services:                 Domain Events:                    │    │
│  │  ┌─────────────────────────┐     ┌─────────────────────────┐       │    │
│  │  │ CalculationEngine       │     │ EmployeeCreated         │       │    │
│  │  │ SlabSelector            │     │ CalculationCompleted    │       │    │
│  │  │ ProrationCalculator     │     │ ApprovalRequested       │       │    │
│  │  │ SplitCalculator         │     │ IncentiveApproved       │       │    │
│  │  └─────────────────────────┘     └─────────────────────────┘       │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                      │                                       │
└──────────────────────────────────────┼───────────────────────────────────────┘
                                       │
                                       ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           INFRASTRUCTURE                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │              Dorise.Incentive.Infrastructure                        │    │
│  │                                                                      │    │
│  │  Repositories:                    External Services:                │    │
│  │  ┌─────────────────────────┐     ┌─────────────────────────┐       │    │
│  │  │ EmployeeRepository      │     │ PayrollClient           │       │    │
│  │  │ PlanRepository          │     │ ErpClient               │       │    │
│  │  │ CalculationRepository   │     │ HrSystemClient          │       │    │
│  │  │ ApprovalRepository      │     │ EmailService            │       │    │
│  │  │ AuditRepository         │     │ NotificationService     │       │    │
│  │  └─────────────────────────┘     └─────────────────────────┘       │    │
│  │                                                                      │    │
│  │  DbContext:                       Caching:                          │    │
│  │  ┌─────────────────────────┐     ┌─────────────────────────┐       │    │
│  │  │ IncentiveDbContext      │     │ RedisCacheService       │       │    │
│  │  │ (EF Core + SQL Server)  │     │ DistributedCacheService │       │    │
│  │  └─────────────────────────┘     └─────────────────────────┘       │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

> *"I choo-choo-choose you!"* - Ralph Wiggum
>
> Each component was carefully chosen for its specific role in the architecture.

### 6.2 Project Structure

```
src/
├── Dorise.Incentive.Api/              # Web API project
│   ├── Controllers/
│   ├── Middleware/
│   ├── Filters/
│   └── Program.cs
│
├── Dorise.Incentive.Web/              # Blazor Web App
│   ├── Pages/
│   ├── Components/
│   ├── Services/
│   └── Program.cs
│
├── Dorise.Incentive.Application/      # Application layer (CQRS)
│   ├── Commands/
│   ├── Queries/
│   ├── Validators/
│   ├── EventHandlers/
│   └── DTOs/
│
├── Dorise.Incentive.Core/             # Domain layer
│   ├── Entities/
│   ├── ValueObjects/
│   ├── Services/
│   ├── Events/
│   ├── Interfaces/
│   └── Plugins/
│
├── Dorise.Incentive.Infrastructure/   # Infrastructure layer
│   ├── Data/
│   ├── Repositories/
│   ├── ExternalServices/
│   ├── Caching/
│   └── Migrations/
│
├── Dorise.Incentive.Functions/        # Azure Functions
│   ├── CalculationTrigger/
│   ├── ImportJob/
│   ├── ExportJob/
│   └── NotificationService/
│
└── Dorise.Incentive.Shared/           # Shared DTOs and utilities
    ├── DTOs/
    ├── Constants/
    └── Extensions/
```

---

## 7. Integration Architecture

### 7.1 Integration Overview

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        INTEGRATION ARCHITECTURE                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│                           ┌─────────────────┐                               │
│                           │      DSIF       │                               │
│                           │   REST API      │                               │
│                           └────────┬────────┘                               │
│                                    │                                         │
│        ┌───────────────────────────┼───────────────────────────┐            │
│        │                           │                           │            │
│        ▼                           ▼                           ▼            │
│  ┌───────────┐             ┌───────────┐             ┌───────────┐          │
│  │  INBOUND  │             │ REAL-TIME │             │ OUTBOUND  │          │
│  │  (Import) │             │  (Auth)   │             │ (Export)  │          │
│  └─────┬─────┘             └─────┬─────┘             └─────┬─────┘          │
│        │                         │                         │                │
│        ▼                         ▼                         ▼                │
│  ┌───────────┐             ┌───────────┐             ┌───────────┐          │
│  │    ERP    │             │ Entra ID  │             │  Payroll  │          │
│  │  System   │             │   (SSO)   │             │  System   │          │
│  └───────────┘             └───────────┘             └───────────┘          │
│        │                                                   │                │
│        ▼                                                   ▼                │
│  ┌───────────┐                                       ┌───────────┐          │
│  │    HR     │                                       │   Email   │          │
│  │  System   │                                       │  (SMTP)   │          │
│  └───────────┘                                       └───────────┘          │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Integration Patterns

| Pattern | Use Case | Implementation |
|---------|----------|----------------|
| **Sync Request/Response** | User authentication, real-time queries | REST API + HTTP Client |
| **Async Batch Import** | Daily sales data import | Azure Functions + Timer Trigger |
| **Async Batch Export** | Payroll file generation | Azure Functions + Queue Trigger |
| **Event-Driven** | Notifications, audit logging | Domain Events + Service Bus |

### 7.3 Integration Contracts

#### Inbound: ERP Sales Data

```json
{
  "importBatch": {
    "batchId": "string",
    "importDate": "2025-01-15T00:00:00Z",
    "records": [
      {
        "employeeId": "EMP001",
        "period": "2025-01",
        "salesAmount": 50000.00,
        "unitsSOld": 150,
        "productionValue": 45000.00,
        "sellThroughRate": 0.85
      }
    ]
  }
}
```

#### Outbound: Payroll Export

```json
{
  "exportBatch": {
    "batchId": "string",
    "exportDate": "2025-01-20T00:00:00Z",
    "period": "2025-01",
    "records": [
      {
        "employeeId": "EMP001",
        "employeeName": "John Doe",
        "incentiveAmount": 2500.00,
        "calculationId": "CALC-2025-001",
        "approvalDate": "2025-01-19T10:30:00Z"
      }
    ]
  }
}
```

---

## 8. Deployment Architecture

### 8.1 CI/CD Pipeline

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                           CI/CD PIPELINE                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐   │
│  │  Code   │───▶│  Build  │───▶│  Test   │───▶│ Publish │───▶│ Deploy  │   │
│  │  Push   │    │         │    │         │    │         │    │         │   │
│  └─────────┘    └─────────┘    └─────────┘    └─────────┘    └─────────┘   │
│       │              │              │              │              │         │
│       │              │              │              │              │         │
│       ▼              ▼              ▼              ▼              ▼         │
│  ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐   │
│  │ GitHub  │    │ dotnet  │    │  Unit   │    │ Docker  │    │  Azure  │   │
│  │ Actions │    │ restore │    │  Tests  │    │  Image  │    │ Deploy  │   │
│  │         │    │ + build │    │ + Int.  │    │ + Bicep │    │  Slots  │   │
│  └─────────┘    └─────────┘    └─────────┘    └─────────┘    └─────────┘   │
│                                                                              │
│                      ┌──────────────────────────────┐                       │
│                      │       ENVIRONMENTS           │                       │
│                      │  Dev ─▶ Staging ─▶ Prod     │                       │
│                      │   (auto)   (auto)   (manual) │                       │
│                      └──────────────────────────────┘                       │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 8.2 Deployment Strategy

| Environment | Trigger | Approval | Strategy |
|-------------|---------|----------|----------|
| **Development** | Push to `develop` | None | Direct deploy |
| **Staging** | Push to `release/*` | None | Direct deploy |
| **Production** | Tag `v*.*.*` | Manual | Blue-Green |

### 8.3 Blue-Green Deployment

```
                    Traffic
                       │
                       ▼
              ┌────────────────┐
              │  App Gateway   │
              │  (Load Balancer)│
              └────────┬───────┘
                       │
           ┌───────────┴───────────┐
           │                       │
           ▼                       ▼
    ┌──────────────┐       ┌──────────────┐
    │   BLUE       │       │   GREEN      │
    │   (Current)  │       │   (New)      │
    │   100%       │       │   0%         │
    └──────────────┘       └──────────────┘

After validation, swap slots:
    │   0%         │       │   100%       │
```

---

## 9. Scalability & Performance

### 9.1 Scaling Strategy

| Component | Scaling Type | Trigger | Limits |
|-----------|--------------|---------|--------|
| **Web App** | Horizontal | CPU > 70% | 1-10 instances |
| **Azure Functions** | Auto (Consumption) | Queue depth | Dynamic |
| **Azure SQL** | Vertical | DTU > 80% | P1 to P6 |
| **Redis Cache** | Vertical | Memory > 80% | C1 to P4 |

### 9.2 Performance Targets

| Metric | Target | Measurement |
|--------|--------|-------------|
| **API Response Time** | < 500ms (p95) | Application Insights |
| **Page Load Time** | < 3s | Browser metrics |
| **Batch Calculation** | 10,000 emp/hour | Function metrics |
| **Database Query** | < 200ms (p95) | SQL metrics |

### 9.3 Caching Strategy

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          CACHING LAYERS                                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │  L1: In-Memory Cache (IMemoryCache)                                 │    │
│  │  • App configuration                                                 │    │
│  │  • Lookup tables                                                     │    │
│  │  • TTL: 5 minutes                                                    │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                      │                                       │
│                                      ▼                                       │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │  L2: Distributed Cache (Azure Redis)                                │    │
│  │  • Session data                                                      │    │
│  │  • User preferences                                                  │    │
│  │  • Frequently accessed entities                                      │    │
│  │  • TTL: 15-60 minutes                                                │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                      │                                       │
│                                      ▼                                       │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │  L3: Database (Azure SQL)                                           │    │
│  │  • Source of truth                                                   │    │
│  │  • Query result caching                                              │    │
│  │  • Indexed for performance                                           │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

> *"My cat's breath smells like cat food."* - Ralph Wiggum
>
> Our caching layers are transparent and efficient - data flows naturally.

---

## 10. Disaster Recovery

### 10.1 DR Strategy

| Component | Strategy | RTO | RPO |
|-----------|----------|-----|-----|
| **Azure SQL** | Geo-replication | 1 hour | 5 seconds |
| **Blob Storage** | RA-GRS | 1 hour | 0 (sync) |
| **App Service** | Multi-region | 15 min | N/A |
| **Redis Cache** | Geo-replication | 30 min | 15 min |

### 10.2 Backup Schedule

| Data Type | Frequency | Retention | Location |
|-----------|-----------|-----------|----------|
| Database (Full) | Daily | 35 days | Azure Backup |
| Database (PITR) | Continuous | 7 days | Same region |
| Blob Storage | Real-time | Indefinite | RA-GRS |
| Configuration | On change | 90 days | Git + Key Vault |

### 10.3 Recovery Procedures

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        DISASTER RECOVERY FLOW                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  1. DETECT                                                                   │
│     └─▶ Azure Monitor Alert ─▶ On-call notification                         │
│                                                                              │
│  2. ASSESS                                                                   │
│     └─▶ Determine scope ─▶ Decide failover type                             │
│                                                                              │
│  3. EXECUTE                                                                  │
│     └─▶ Manual: Traffic Manager ─▶ Failover to secondary                    │
│     └─▶ Auto: Database auto-failover group                                  │
│                                                                              │
│  4. VERIFY                                                                   │
│     └─▶ Smoke tests ─▶ Health checks ─▶ User validation                     │
│                                                                              │
│  5. COMMUNICATE                                                              │
│     └─▶ Status page update ─▶ Stakeholder notification                      │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 11. Technology Stack

### 11.1 Complete Technology Stack

| Category | Technology | Version | Purpose |
|----------|------------|---------|---------|
| **Runtime** | .NET | 8.0 LTS | Application runtime |
| **Language** | C# | 12 | Primary language |
| **Web Framework** | ASP.NET Core | 8.0 | API & Web hosting |
| **Frontend** | Blazor | 8.0 | Interactive web UI |
| **ORM** | Entity Framework Core | 8.0 | Data access |
| **Database** | Azure SQL Database | Latest | Relational storage |
| **Cache** | Azure Redis Cache | 6.x | Distributed caching |
| **Storage** | Azure Blob Storage | Latest | File storage |
| **Functions** | Azure Functions | 4.x | Serverless compute |
| **Identity** | Azure Entra ID | Latest | Authentication |
| **Secrets** | Azure Key Vault | Latest | Secrets management |
| **Monitoring** | Application Insights | Latest | APM & logging |
| **IaC** | Bicep | Latest | Infrastructure code |
| **CI/CD** | GitHub Actions | Latest | Automation |

### 11.2 NuGet Package Dependencies

| Package | Purpose |
|---------|---------|
| `MediatR` | CQRS pattern implementation |
| `FluentValidation` | Request validation |
| `AutoMapper` | Object mapping |
| `Serilog` | Structured logging |
| `Polly` | Resilience patterns |
| `Swashbuckle` | OpenAPI/Swagger |
| `Microsoft.Identity.Web` | Entra ID integration |
| `StackExchange.Redis` | Redis client |

---

## Appendix A: Architecture Decision Records (ADRs)

| ADR | Decision | Status | Date |
|-----|----------|--------|------|
| ADR-001 | Use .NET 8.0 for all services | Accepted | Jan 2025 |
| ADR-002 | Choose Azure SQL over Cosmos DB | Accepted | Jan 2025 |
| ADR-003 | Implement CQRS with MediatR | Accepted | Jan 2025 |
| ADR-004 | Use Blazor for web frontend | Accepted | Jan 2025 |
| ADR-005 | Azure Functions for batch processing | Accepted | Jan 2025 |

---

## Appendix B: Glossary

| Term | Definition |
|------|------------|
| **CQRS** | Command Query Responsibility Segregation |
| **DDD** | Domain-Driven Design |
| **PaaS** | Platform as a Service |
| **PITR** | Point-in-Time Recovery |
| **RA-GRS** | Read-Access Geo-Redundant Storage |
| **RTO** | Recovery Time Objective |
| **RPO** | Recovery Point Objective |
| **SSO** | Single Sign-On |
| **TDE** | Transparent Data Encryption |

---

> *"Go Banana!"* - Ralph Wiggum
>
> With this architecture, we're ready to build something great!

---

**Document Approval**

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Solution Architect | Skanda Prasad | _____________ | ______ |
| Product Owner | Skanda Prasad | _____________ | ______ |

---

*This document is part of the DSIF Quality Gate Framework - QG-2 Deliverable*
