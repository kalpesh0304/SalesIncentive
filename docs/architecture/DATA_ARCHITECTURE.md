# DORISE Sales Incentive Framework
## Data Architecture

**Document ID:** DOC-ARCH-002
**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Created:** January 2025
**Last Updated:** January 2025
**Author:** Claude Code
**Status:** Draft

> *"I found a moon rock in my nose!"* - Ralph Wiggum
>
> Our data model captures every detail - even the unexpected ones.

---

## Table of Contents

1. [Overview](#1-overview)
2. [Data Principles](#2-data-principles)
3. [Entity Relationship Diagram](#3-entity-relationship-diagram)
4. [Entity Definitions](#4-entity-definitions)
5. [Data Dictionary](#5-data-dictionary)
6. [Temporal Data Model](#6-temporal-data-model)
7. [Audit Trail Design](#7-audit-trail-design)
8. [Data Flow](#8-data-flow)
9. [Data Retention](#9-data-retention)
10. [Database Design](#10-database-design)

---

## 1. Overview

### 1.1 Purpose

This document defines the data architecture for DSIF, including:
- Logical data model and entity relationships
- Physical database design for Azure SQL
- Temporal data patterns for historical accuracy
- Audit trail implementation for compliance

### 1.2 Design Goals

| Goal | Description |
|------|-------------|
| **Immutability** | Calculations and audits cannot be modified |
| **Temporal Accuracy** | Point-in-time queries for any historical date |
| **Referential Integrity** | Foreign key constraints throughout |
| **Performance** | Indexed for common query patterns |
| **Compliance** | 7-year retention, GDPR-ready |

---

## 2. Data Principles

### 2.1 Core Principles

| Principle | Implementation |
|-----------|----------------|
| **Single Source of Truth** | Azure SQL as primary store |
| **Soft Deletes Only** | `IsDeleted` flag, no physical deletes |
| **Temporal Tables** | System-versioned for history |
| **Normalized Design** | 3NF for transactional tables |
| **Denormalized Views** | Materialized views for reporting |

### 2.2 Naming Conventions

| Element | Convention | Example |
|---------|------------|---------|
| **Tables** | PascalCase, singular | `Employee`, `IncentivePlan` |
| **Columns** | PascalCase | `EmployeeId`, `CreatedAt` |
| **Primary Keys** | `{Table}Id` | `EmployeeId`, `PlanId` |
| **Foreign Keys** | `FK_{Child}_{Parent}` | `FK_Assignment_Employee` |
| **Indexes** | `IX_{Table}_{Columns}` | `IX_Calculation_Period` |

---

## 3. Entity Relationship Diagram

### 3.1 High-Level ERD

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     ENTITY RELATIONSHIP DIAGRAM                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│                              ORGANIZATIONAL                                  │
│  ┌──────────────┐       ┌──────────────┐       ┌──────────────┐            │
│  │   Company    │──────▶│     Zone     │──────▶│    Store     │            │
│  │              │  1:N  │              │  1:N  │              │            │
│  └──────────────┘       └──────────────┘       └──────────────┘            │
│                                                       │                      │
│                                                       │ 1:N                  │
│                                                       ▼                      │
│                                                ┌──────────────┐             │
│                                                │  Assignment  │             │
│                                                │ (Temporal)   │             │
│                                                └──────────────┘             │
│                                                       │                      │
│                          ┌────────────────────────────┤                      │
│                          │                            │                      │
│                          ▼                            ▼                      │
│                   ┌──────────────┐            ┌──────────────┐              │
│                   │   Employee   │◀───────────│  SplitShare  │              │
│                   │  (Temporal)  │    N:1     │              │              │
│                   └──────────────┘            └──────────────┘              │
│                          │                                                   │
│                          │ 1:N                                               │
│                          ▼                                                   │
│                   ┌──────────────┐                                          │
│                   │ Calculation  │◀────────────────────────────┐            │
│                   │ (Immutable)  │                              │            │
│                   └──────────────┘                              │            │
│                          │                                      │            │
│            ┌─────────────┼─────────────┐                       │            │
│            │             │             │                       │            │
│            ▼             ▼             ▼                       │            │
│     ┌───────────┐ ┌───────────┐ ┌───────────┐                 │            │
│     │Calculation│ │ Approval  │ │ Amendment │                 │            │
│     │  Detail   │ │           │ │           │─────────────────┘            │
│     └───────────┘ └───────────┘ └───────────┘                              │
│                                                                              │
│                              CONFIGURATION                                   │
│  ┌──────────────┐       ┌──────────────┐       ┌──────────────┐            │
│  │IncentivePlan │──────▶│  PlanSlab    │       │   Formula    │            │
│  │  (Versioned) │  1:N  │              │◀──────│              │            │
│  └──────────────┘       └──────────────┘  N:1  └──────────────┘            │
│         │                                                                    │
│         │ 1:N                                                                │
│         ▼                                                                    │
│  ┌──────────────┐       ┌──────────────┐                                    │
│  │ PlanVersion  │──────▶│   Applies    │                                    │
│  │              │  M:N  │   To Role    │                                    │
│  └──────────────┘       └──────────────┘                                    │
│                                                                              │
│                                AUDIT                                         │
│  ┌──────────────┐       ┌──────────────┐       ┌──────────────┐            │
│  │  AuditLog    │       │  UserSession │       │  SystemLog   │            │
│  │ (Append-only)│       │              │       │              │            │
│  └──────────────┘       └──────────────┘       └──────────────┘            │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Detailed ERD

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        DETAILED DATA MODEL                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌────────────────────┐                    ┌────────────────────┐           │
│  │     Company        │                    │       Zone         │           │
│  ├────────────────────┤                    ├────────────────────┤           │
│  │ PK CompanyId       │───────────────────▶│ PK ZoneId          │           │
│  │    CompanyCode     │         1:N        │ FK CompanyId       │           │
│  │    CompanyName     │                    │    ZoneCode        │           │
│  │    IsActive        │                    │    ZoneName        │           │
│  │    CreatedAt       │                    │    IsActive        │           │
│  │    ModifiedAt      │                    │    CreatedAt       │           │
│  └────────────────────┘                    └────────────────────┘           │
│                                                      │                       │
│                                                      │ 1:N                   │
│                                                      ▼                       │
│                                            ┌────────────────────┐           │
│                                            │       Store        │           │
│                                            ├────────────────────┤           │
│                                            │ PK StoreId         │           │
│                                            │ FK ZoneId          │           │
│                                            │    StoreCode       │           │
│                                            │    StoreName       │           │
│                                            │    IsActive        │           │
│                                            │    CreatedAt       │           │
│                                            └────────────────────┘           │
│                                                      │                       │
│  ┌────────────────────┐                              │ 1:N                   │
│  │     Employee       │                              │                       │
│  ├────────────────────┤         ┌────────────────────┼──────────────────┐   │
│  │ PK EmployeeId      │◀────────│                    │                  │   │
│  │    EmployeeNumber  │   N:1   │   ┌────────────────▼───────┐          │   │
│  │    FirstName       │         │   │    Assignment          │          │   │
│  │    LastName        │         │   ├────────────────────────┤          │   │
│  │    Email           │         │   │ PK AssignmentId        │          │   │
│  │    Status          │         │   │ FK EmployeeId          │          │   │
│  │    HireDate        │         │   │ FK StoreId             │          │   │
│  │    TerminationDate │         │   │ FK RoleId              │          │   │
│  │    IsActive        │         │   │    EffectiveFrom       │          │   │
│  │    CreatedAt       │         │   │    EffectiveTo         │          │   │
│  │    ModifiedAt      │         │   │    IsPrimary           │          │   │
│  │    ValidFrom       │         │   │    CreatedAt           │          │   │
│  │    ValidTo         │         │   └────────────────────────┘          │   │
│  └────────────────────┘                              │                  │   │
│         │                                            │ 1:N              │   │
│         │                                            ▼                  │   │
│         │                               ┌────────────────────┐          │   │
│         │                               │    SplitShare      │          │   │
│         │                               ├────────────────────┤          │   │
│         │                               │ PK SplitShareId    │          │   │
│         │◀──────────────────────────────│ FK AssignmentId    │          │   │
│         │              N:1              │ FK EmployeeId      │◀─────────┘   │
│         │                               │    SharePercentage │              │
│         │                               │    EffectiveFrom   │              │
│         │                               │    EffectiveTo     │              │
│         │                               └────────────────────┘              │
│         │                                                                    │
│         │ 1:N                                                                │
│         ▼                                                                    │
│  ┌────────────────────┐         ┌────────────────────┐                      │
│  │   Calculation      │────────▶│ CalculationDetail  │                      │
│  ├────────────────────┤   1:N   ├────────────────────┤                      │
│  │ PK CalculationId   │         │ PK DetailId        │                      │
│  │ FK EmployeeId      │         │ FK CalculationId   │                      │
│  │ FK PlanVersionId   │         │    MetricType      │                      │
│  │ FK AssignmentId    │         │    MetricValue     │                      │
│  │    Period          │         │    SlabApplied     │                      │
│  │    GrossAmount     │         │    RateApplied     │                      │
│  │    NetAmount       │         │    Amount          │                      │
│  │    Status          │         │    Description     │                      │
│  │    CalculatedAt    │         └────────────────────┘                      │
│  │    BatchId         │                                                      │
│  │    IsAmendment     │         ┌────────────────────┐                      │
│  │ FK OriginalCalcId  │────────▶│    Amendment       │                      │
│  └────────────────────┘   1:N   ├────────────────────┤                      │
│         │                       │ PK AmendmentId     │                      │
│         │                       │ FK CalculationId   │                      │
│         │ 1:N                   │ FK OriginalCalcId  │                      │
│         ▼                       │    Reason          │                      │
│  ┌────────────────────┐         │    AdjustmentAmount│                      │
│  │     Approval       │         │    CreatedBy       │                      │
│  ├────────────────────┤         │    CreatedAt       │                      │
│  │ PK ApprovalId      │         └────────────────────┘                      │
│  │ FK CalculationId   │                                                      │
│  │    ApprovalLevel   │                                                      │
│  │    Status          │                                                      │
│  │    ApprovedBy      │                                                      │
│  │    ApprovedAt      │                                                      │
│  │    Comments        │                                                      │
│  │    DelegatedFrom   │                                                      │
│  └────────────────────┘                                                      │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────────────────────┐
│                       CONFIGURATION TABLES                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌────────────────────┐         ┌────────────────────┐                      │
│  │   IncentivePlan    │────────▶│   PlanVersion      │                      │
│  ├────────────────────┤   1:N   ├────────────────────┤                      │
│  │ PK PlanId          │         │ PK PlanVersionId   │                      │
│  │    PlanCode        │         │ FK PlanId          │                      │
│  │    PlanName        │         │    VersionNumber   │                      │
│  │    Description     │         │    EffectiveFrom   │                      │
│  │    IsActive        │         │    EffectiveTo     │                      │
│  │    CreatedAt       │         │    Status          │                      │
│  │    CreatedBy       │         │    ApprovedBy      │                      │
│  └────────────────────┘         │    ApprovedAt      │                      │
│                                 │    CreatedAt       │                      │
│                                 └────────────────────┘                      │
│                                          │                                   │
│                        ┌─────────────────┼─────────────────┐                │
│                        │                 │                 │                │
│                        ▼                 ▼                 ▼                │
│               ┌──────────────┐  ┌──────────────┐  ┌──────────────┐          │
│               │  PlanSlab    │  │ PlanRoleMap  │  │   Formula    │          │
│               ├──────────────┤  ├──────────────┤  ├──────────────┤          │
│               │ PK SlabId    │  │ PK MapId     │  │ PK FormulaId │          │
│               │ FK VersionId │  │ FK VersionId │  │ FK VersionId │          │
│               │    SlabOrder │  │ FK RoleId    │  │    FormulaName│         │
│               │    MinThreshold│ └──────────────┘  │    Expression │         │
│               │    MaxThreshold│                   │    Variables  │         │
│               │    RateType   │                   └──────────────┘          │
│               │    RateValue  │                                              │
│               │    CapAmount  │         ┌────────────────────┐              │
│               └──────────────┘         │       Role         │              │
│                                        ├────────────────────┤              │
│                                        │ PK RoleId          │              │
│                                        │    RoleCode        │              │
│                                        │    RoleName        │              │
│                                        │    RoleLevel       │              │
│                                        │    IsActive        │              │
│                                        └────────────────────┘              │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

> *"I sleep in a drawer!"* - Ralph Wiggum
>
> Our data fits neatly into well-defined tables - everything has its place.

---

## 4. Entity Definitions

### 4.1 Core Entities

#### Employee
| Attribute | Type | Description |
|-----------|------|-------------|
| EmployeeId | UNIQUEIDENTIFIER | Primary key |
| EmployeeNumber | NVARCHAR(20) | Business identifier |
| FirstName | NVARCHAR(100) | First name |
| LastName | NVARCHAR(100) | Last name |
| Email | NVARCHAR(256) | Email address |
| Status | NVARCHAR(20) | Active/Inactive/Terminated |
| HireDate | DATE | Employment start date |
| TerminationDate | DATE | Employment end date (nullable) |
| ValidFrom | DATETIME2 | Temporal start |
| ValidTo | DATETIME2 | Temporal end |

#### Assignment
| Attribute | Type | Description |
|-----------|------|-------------|
| AssignmentId | UNIQUEIDENTIFIER | Primary key |
| EmployeeId | UNIQUEIDENTIFIER | FK to Employee |
| StoreId | UNIQUEIDENTIFIER | FK to Store |
| RoleId | UNIQUEIDENTIFIER | FK to Role |
| EffectiveFrom | DATE | Assignment start |
| EffectiveTo | DATE | Assignment end (nullable) |
| IsPrimary | BIT | Primary assignment flag |

#### Calculation
| Attribute | Type | Description |
|-----------|------|-------------|
| CalculationId | UNIQUEIDENTIFIER | Primary key |
| EmployeeId | UNIQUEIDENTIFIER | FK to Employee |
| PlanVersionId | UNIQUEIDENTIFIER | FK to PlanVersion |
| AssignmentId | UNIQUEIDENTIFIER | FK to Assignment |
| Period | NVARCHAR(7) | YYYY-MM format |
| GrossAmount | DECIMAL(18,2) | Calculated gross |
| NetAmount | DECIMAL(18,2) | Net after adjustments |
| Status | NVARCHAR(20) | Pending/Approved/Exported |
| CalculatedAt | DATETIME2 | Calculation timestamp |
| BatchId | UNIQUEIDENTIFIER | Batch reference |
| IsAmendment | BIT | Amendment flag |
| OriginalCalcId | UNIQUEIDENTIFIER | FK to original (amendments) |

### 4.2 Configuration Entities

#### IncentivePlan
| Attribute | Type | Description |
|-----------|------|-------------|
| PlanId | UNIQUEIDENTIFIER | Primary key |
| PlanCode | NVARCHAR(20) | Unique code |
| PlanName | NVARCHAR(200) | Display name |
| Description | NVARCHAR(2000) | Plan description |
| IsActive | BIT | Active flag |

#### PlanSlab
| Attribute | Type | Description |
|-----------|------|-------------|
| SlabId | UNIQUEIDENTIFIER | Primary key |
| PlanVersionId | UNIQUEIDENTIFIER | FK to PlanVersion |
| SlabOrder | INT | Slab sequence |
| MinThreshold | DECIMAL(18,2) | Lower bound |
| MaxThreshold | DECIMAL(18,2) | Upper bound (nullable) |
| RateType | NVARCHAR(20) | Percentage/Fixed |
| RateValue | DECIMAL(18,4) | Rate or amount |
| CapAmount | DECIMAL(18,2) | Maximum cap (nullable) |

---

## 5. Data Dictionary

### 5.1 Lookup Values

#### Status Codes

| Entity | Status | Description |
|--------|--------|-------------|
| Employee | Active | Currently employed |
| Employee | Inactive | On leave |
| Employee | Terminated | No longer employed |
| Calculation | Pending | Awaiting approval |
| Calculation | Approved | Approved for payment |
| Calculation | Rejected | Rejected |
| Calculation | Exported | Sent to payroll |
| PlanVersion | Draft | Being edited |
| PlanVersion | Active | Currently in use |
| PlanVersion | Superseded | Replaced by newer version |

#### Rate Types

| Code | Description |
|------|-------------|
| Percentage | Rate as percentage of base |
| Fixed | Fixed amount per unit |
| Tiered | Cumulative tiers |

### 5.2 Validation Rules

| Entity | Field | Rule |
|--------|-------|------|
| Employee | Email | Valid email format |
| Employee | HireDate | <= Today |
| Assignment | EffectiveFrom | <= EffectiveTo |
| SplitShare | SharePercentage | Sum = 100% per assignment |
| PlanSlab | MinThreshold | < MaxThreshold |
| Calculation | GrossAmount | >= 0 |

---

## 6. Temporal Data Model

### 6.1 System-Versioned Temporal Tables

```sql
-- Employee table with temporal support
CREATE TABLE Employee (
    EmployeeId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    EmployeeNumber NVARCHAR(20) NOT NULL,
    FirstName NVARCHAR(100) NOT NULL,
    LastName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(256) NOT NULL,
    Status NVARCHAR(20) NOT NULL,
    HireDate DATE NOT NULL,
    TerminationDate DATE NULL,
    IsDeleted BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    ModifiedAt DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),

    -- Temporal columns
    ValidFrom DATETIME2 GENERATED ALWAYS AS ROW START NOT NULL,
    ValidTo DATETIME2 GENERATED ALWAYS AS ROW END NOT NULL,
    PERIOD FOR SYSTEM_TIME (ValidFrom, ValidTo)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.EmployeeHistory));
```

### 6.2 Point-in-Time Queries

```sql
-- Get employee as of a specific date
SELECT *
FROM Employee
FOR SYSTEM_TIME AS OF '2024-06-15T00:00:00'
WHERE EmployeeId = @EmployeeId;

-- Get complete history for an employee
SELECT *
FROM Employee
FOR SYSTEM_TIME ALL
WHERE EmployeeId = @EmployeeId
ORDER BY ValidFrom;

-- Get employees who were active during a period
SELECT *
FROM Employee
FOR SYSTEM_TIME BETWEEN '2024-01-01' AND '2024-12-31'
WHERE Status = 'Active';
```

### 6.3 Temporal Tables Implementation

| Table | Temporal | History Table | Retention |
|-------|----------|---------------|-----------|
| Employee | Yes | EmployeeHistory | 7 years |
| Assignment | Yes | AssignmentHistory | 7 years |
| IncentivePlan | No | PlanVersion (explicit) | Indefinite |
| Calculation | No | Immutable | 7 years |
| AuditLog | No | Append-only | 7 years |

---

## 7. Audit Trail Design

### 7.1 Audit Log Structure

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          AUDIT LOG TABLE                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌────────────────────────────────────────────────────────────────────┐     │
│  │                        AuditLog                                    │     │
│  ├────────────────────────────────────────────────────────────────────┤     │
│  │  PK  AuditLogId          UNIQUEIDENTIFIER                         │     │
│  │      Timestamp           DATETIME2           (Clustered Index)    │     │
│  │      UserId              NVARCHAR(256)       (Who)                │     │
│  │      UserName            NVARCHAR(200)                            │     │
│  │      Action              NVARCHAR(50)        (Create/Update/Delete)│    │
│  │      EntityType          NVARCHAR(100)       (Table name)         │     │
│  │      EntityId            NVARCHAR(100)       (PK value)           │     │
│  │      OldValues           NVARCHAR(MAX)       (JSON)               │     │
│  │      NewValues           NVARCHAR(MAX)       (JSON)               │     │
│  │      IPAddress           NVARCHAR(50)                             │     │
│  │      UserAgent           NVARCHAR(500)                            │     │
│  │      CorrelationId       UNIQUEIDENTIFIER    (Request tracking)   │     │
│  │      AdditionalInfo      NVARCHAR(MAX)       (JSON, optional)     │     │
│  └────────────────────────────────────────────────────────────────────┘     │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Audit Events

| Event | Trigger | Data Captured |
|-------|---------|---------------|
| Entity Created | INSERT | New values |
| Entity Updated | UPDATE | Old + New values |
| Entity Deleted | Soft DELETE | Old values |
| Login Success | Authentication | User, IP, time |
| Login Failure | Auth failure | Attempted user, IP |
| Calculation Run | Batch process | Parameters, results |
| Approval Action | Workflow | Decision, comments |
| Export Triggered | Integration | Batch details |

### 7.3 Audit Implementation (EF Core)

```csharp
// Audit interceptor for EF Core
public class AuditInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken)
    {
        var context = eventData.Context;
        var auditEntries = new List<AuditLog>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Added ||
                entry.State == EntityState.Modified ||
                entry.State == EntityState.Deleted)
            {
                auditEntries.Add(CreateAuditEntry(entry));
            }
        }

        context.Set<AuditLog>().AddRange(auditEntries);
        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
```

> *"The doctor said I wouldn't have so many nosebleeds if I kept my finger outta there."* - Ralph Wiggum
>
> Our audit trail catches everything - no changes go unnoticed.

---

## 8. Data Flow

### 8.1 Calculation Data Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      CALCULATION DATA FLOW                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────────┐                                                           │
│  │  ERP System  │                                                           │
│  │  (Sales Data)│                                                           │
│  └──────┬───────┘                                                           │
│         │                                                                    │
│         │ 1. Import (Azure Function)                                        │
│         ▼                                                                    │
│  ┌──────────────┐     ┌──────────────┐     ┌──────────────┐                │
│  │  SalesData   │────▶│  Calculation │────▶│  Calculation │                │
│  │  (Staging)   │     │   Engine     │     │   Record     │                │
│  └──────────────┘     └──────────────┘     └──────────────┘                │
│                              │                     │                        │
│                              │ 2. Load             │ 4. Persist             │
│                              ▼                     ▼                        │
│                       ┌──────────────┐     ┌──────────────┐                │
│                       │   Employee   │     │  Azure SQL   │                │
│                       │    + Plan    │     │   Database   │                │
│                       │   + Slab     │     └──────────────┘                │
│                       └──────────────┘            │                        │
│                              │                     │ 5. Approval            │
│                              │ 3. Calculate        ▼                        │
│                              ▼              ┌──────────────┐                │
│                       ┌──────────────┐     │   Approval   │                │
│                       │ Apply Slabs  │     │   Workflow   │                │
│                       │ + Proration  │     └──────────────┘                │
│                       │ + Splits     │            │                        │
│                       └──────────────┘            │ 6. Export               │
│                                                   ▼                        │
│                                            ┌──────────────┐                │
│                                            │   Payroll    │                │
│                                            │   System     │                │
│                                            └──────────────┘                │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 8.2 Data Integration Points

| Source | Direction | Frequency | Data |
|--------|-----------|-----------|------|
| ERP | Inbound | Daily | Sales metrics |
| HR System | Inbound | Real-time | Employee changes |
| Payroll | Outbound | Per period | Approved incentives |
| Entra ID | Both | Real-time | User authentication |

---

## 9. Data Retention

### 9.1 Retention Policy

| Data Type | Online | Archive | Delete |
|-----------|--------|---------|--------|
| Active employees | Indefinite | N/A | Never (soft) |
| Terminated employees | 2 years | 5 years | After 7 years |
| Calculations | 3 years | 4 years | After 7 years |
| Audit logs | 1 year | 6 years | After 7 years |
| Session data | 24 hours | N/A | After 24 hours |
| System logs | 90 days | N/A | After 90 days |

### 9.2 Archive Strategy

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        DATA LIFECYCLE                                        │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  HOT STORAGE (Azure SQL Premium)                                            │
│  └─ Active data, < 3 years                                                  │
│                │                                                             │
│                │ Archive Job (Monthly)                                       │
│                ▼                                                             │
│  WARM STORAGE (Azure SQL Standard)                                          │
│  └─ Archive database, 3-5 years                                             │
│                │                                                             │
│                │ Cold Archive Job (Quarterly)                                │
│                ▼                                                             │
│  COLD STORAGE (Azure Blob - Cool Tier)                                      │
│  └─ Compressed exports, 5-7 years                                           │
│                │                                                             │
│                │ Purge Job (After 7 years)                                   │
│                ▼                                                             │
│  DELETED (Secure wipe, audit record retained)                               │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 9.3 GDPR Compliance

| Right | Implementation |
|-------|----------------|
| **Right to Access** | Export API for user data |
| **Right to Rectification** | Update with audit trail |
| **Right to Erasure** | Anonymization (financial records retained) |
| **Right to Portability** | JSON/CSV export |

---

## 10. Database Design

### 10.1 Physical Database Configuration

| Setting | Development | Staging | Production |
|---------|-------------|---------|------------|
| **SKU** | S0 (10 DTU) | S2 (50 DTU) | P2 (250 DTU) |
| **Max Size** | 250 GB | 500 GB | 1 TB |
| **Backup** | Local | Geo-redundant | Geo-redundant |
| **Zone Redundant** | No | No | Yes |
| **Read Replica** | No | No | Yes |

### 10.2 Index Strategy

```sql
-- Primary performance indexes
CREATE NONCLUSTERED INDEX IX_Calculation_Period_Status
ON Calculation (Period, Status)
INCLUDE (EmployeeId, GrossAmount, NetAmount);

CREATE NONCLUSTERED INDEX IX_Employee_Status_Store
ON Assignment (EmployeeId, StoreId)
WHERE EffectiveTo IS NULL;

CREATE NONCLUSTERED INDEX IX_AuditLog_Entity
ON AuditLog (EntityType, EntityId, Timestamp DESC);

CREATE NONCLUSTERED INDEX IX_Approval_Status_Level
ON Approval (Status, ApprovalLevel)
INCLUDE (CalculationId, ApprovedBy);
```

### 10.3 Partitioning Strategy

| Table | Partition Column | Partition Scheme |
|-------|------------------|------------------|
| Calculation | Period | Monthly |
| AuditLog | Timestamp | Monthly |
| CalculationDetail | CalculationId | Hash (based on Calculation partition) |

```sql
-- Partition function for monthly partitioning
CREATE PARTITION FUNCTION PF_Monthly (NVARCHAR(7))
AS RANGE RIGHT FOR VALUES
('2024-01', '2024-02', '2024-03', /* ... */ '2025-12');

CREATE PARTITION SCHEME PS_Monthly
AS PARTITION PF_Monthly
ALL TO ([PRIMARY]);
```

---

## Appendix A: Sample Data

### Employee Sample
```json
{
  "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "employeeNumber": "EMP001",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@dorise.com",
  "status": "Active",
  "hireDate": "2020-03-15"
}
```

### Calculation Sample
```json
{
  "calculationId": "7fa85f64-5717-4562-b3fc-2c963f66afa9",
  "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "period": "2025-01",
  "grossAmount": 2500.00,
  "netAmount": 2500.00,
  "status": "Approved",
  "details": [
    {
      "metricType": "SalesVolume",
      "metricValue": 75000.00,
      "slabApplied": "Gold",
      "rateApplied": 0.035,
      "amount": 2625.00
    }
  ]
}
```

---

> *"Me fail English? That's unpossible!"* - Ralph Wiggum
>
> Data integrity failures in our system are also unpossible - we've designed for it.

---

**Document Approval**

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Solution Architect | Skanda Prasad | _____________ | ______ |
| Database Administrator | TBD | _____________ | ______ |

---

*This document is part of the DSIF Quality Gate Framework - QG-2 Deliverable*
