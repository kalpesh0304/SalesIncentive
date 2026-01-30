# Data Migration Plan

**Document ID:** RB-MIGRATE-001
**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Last Updated:** January 2025
**Owner:** DBA Team + Business Analysts

> *"Hi, Super Nintendo Chalmers!"* - Ralph Wiggum
>
> We're migrating data like a super-powered system - carefully and completely!

---

## Table of Contents

1. [Overview](#1-overview)
2. [Migration Scope](#2-migration-scope)
3. [Migration Strategy](#3-migration-strategy)
4. [Data Mapping](#4-data-mapping)
5. [Migration Scripts](#5-migration-scripts)
6. [Validation Rules](#6-validation-rules)
7. [Execution Plan](#7-execution-plan)
8. [Rollback Procedures](#8-rollback-procedures)

---

## 1. Overview

### 1.1 Purpose

This document outlines the plan for migrating data from legacy systems to the new DSIF platform. It ensures data integrity, completeness, and minimal disruption to business operations.

### 1.2 Migration Objectives

- Migrate all historical employee data
- Migrate active incentive plans and configurations
- Migrate historical calculations (last 2 years)
- Ensure zero data loss
- Maintain audit trail integrity
- Complete within the maintenance window

### 1.3 Source Systems

| System | Type | Data |
|--------|------|------|
| Legacy HR System | Oracle DB | Employee data |
| Excel Spreadsheets | Files | Incentive plans, rates |
| Legacy Incentive App | SQL Server | Historical calculations |
| Active Directory | LDAP | User accounts |

### 1.4 Target System

| Component | Technology | Purpose |
|-----------|------------|---------|
| DSIF Database | Azure SQL | Primary data store |
| Blob Storage | Azure Storage | Document attachments |
| Key Vault | Azure KV | Sensitive configurations |

---

## 2. Migration Scope

### 2.1 In Scope

| Entity | Records (Est.) | Priority | Source |
|--------|----------------|----------|--------|
| Employees | 5,000 | P1 | HR System |
| Departments | 50 | P1 | HR System |
| Stores/Locations | 200 | P1 | HR System |
| Zones | 10 | P1 | Excel |
| Incentive Plans | 25 | P1 | Excel |
| Plan Slabs | 150 | P1 | Excel |
| Roles | 30 | P1 | Excel |
| Historical Calculations | 100,000 | P2 | Legacy App |
| Approval History | 50,000 | P2 | Legacy App |
| Audit Logs | 500,000 | P3 | Legacy App |

### 2.2 Out of Scope

- Archived data older than 2 years
- Test/training data
- Duplicate records
- Invalid/orphaned records

### 2.3 Data Quality Requirements

| Metric | Target | Measurement |
|--------|--------|-------------|
| Completeness | 100% | All required fields populated |
| Accuracy | 99.9% | Post-migration validation |
| Consistency | 100% | Referential integrity maintained |
| Timeliness | < 4 hours | Within maintenance window |

---

## 3. Migration Strategy

### 3.1 Approach

**Hybrid Migration:**
1. **Pre-Migration (T-7):** Reference data, static lookups
2. **Cut-Over (T-0):** Transactional data with application offline
3. **Delta Sync:** Changes during migration window

### 3.2 Migration Phases

```
Phase 1: Reference Data (T-7 to T-3)
├── Departments
├── Zones
├── Stores
├── Roles
└── Incentive Plans

Phase 2: Master Data (T-1)
├── Employees (full load)
├── Assignments
└── Role mappings

Phase 3: Transactional Data (T-0)
├── Calculations (last 2 years)
├── Approvals
└── Audit logs

Phase 4: Validation (T-0)
├── Record counts
├── Data integrity
└── Business rules
```

### 3.3 Migration Tools

| Tool | Purpose |
|------|---------|
| Azure Data Factory | ETL orchestration |
| SQL Server Migration Assistant | Schema migration |
| PowerShell Scripts | Custom transformations |
| SSIS Packages | Legacy compatibility |

---

## 4. Data Mapping

### 4.1 Employee Data Mapping

| Source Field (Legacy) | Target Field (DSIF) | Transformation |
|----------------------|---------------------|----------------|
| EMP_ID | EmployeeNumber | Prefix 'EMP' + zero-pad |
| FIRST_NM | FirstName | Trim, Title Case |
| LAST_NM | LastName | Trim, Title Case |
| EMAIL_ADDR | Email | Lowercase |
| HIRE_DT | HireDate | Convert to UTC |
| TERM_DT | TerminationDate | Convert to UTC, null if active |
| STATUS_CD | Status | Map: 'A'→'Active', 'T'→'Terminated' |
| DEPT_ID | DepartmentId | Lookup from Department table |
| MGR_ID | ManagerId | Lookup from Employee table |

### 4.2 Incentive Plan Mapping

| Source Field (Excel) | Target Field (DSIF) | Transformation |
|---------------------|---------------------|----------------|
| Plan Name | Name | Trim |
| Description | Description | Trim |
| Start Date | EffectiveFrom | Parse date |
| End Date | EffectiveTo | Parse date, null if open |
| Plan Type | Type | Map to enum |
| Is Active | IsActive | Parse boolean |

### 4.3 Calculation Mapping

| Source Field (Legacy) | Target Field (DSIF) | Transformation |
|----------------------|---------------------|----------------|
| CALC_ID | LegacyId | Store for reference |
| EMP_ID | EmployeeId | Lookup |
| PERIOD | Period | Format: YYYY-MM |
| GROSS_AMT | GrossAmount | Decimal precision 2 |
| NET_AMT | NetAmount | Decimal precision 2 |
| STATUS | Status | Map to enum |
| CALC_DT | CalculatedAt | Convert to UTC |

### 4.4 Code Mappings

#### Status Codes

| Legacy Code | DSIF Status |
|-------------|-------------|
| A | Active |
| T | Terminated |
| L | OnLeave |
| S | Suspended |

#### Approval Status

| Legacy Code | DSIF Status |
|-------------|-------------|
| P | Pending |
| A | Approved |
| R | Rejected |
| C | Cancelled |

---

## 5. Migration Scripts

### 5.1 Pre-Migration Script

```sql
-- Pre-migration: Create staging tables
-- File: 01_create_staging_tables.sql

-- Staging schema
CREATE SCHEMA staging;
GO

-- Employee staging
CREATE TABLE staging.Employee (
    SourceId NVARCHAR(50) NOT NULL,
    FirstName NVARCHAR(100),
    LastName NVARCHAR(100),
    Email NVARCHAR(255),
    HireDate DATE,
    TerminationDate DATE,
    StatusCode CHAR(1),
    DepartmentCode NVARCHAR(20),
    ManagerSourceId NVARCHAR(50),
    LoadedAt DATETIME2 DEFAULT GETUTCDATE(),
    ProcessedAt DATETIME2,
    ErrorMessage NVARCHAR(MAX),
    IsValid BIT DEFAULT 1
);

-- Add more staging tables...
```

### 5.2 Extract Script

```sql
-- Extract from legacy system
-- File: 02_extract_legacy_data.sql

-- Extract employees
INSERT INTO staging.Employee (
    SourceId, FirstName, LastName, Email,
    HireDate, TerminationDate, StatusCode,
    DepartmentCode, ManagerSourceId
)
SELECT
    EMP_ID,
    LTRIM(RTRIM(FIRST_NM)),
    LTRIM(RTRIM(LAST_NM)),
    LOWER(LTRIM(RTRIM(EMAIL_ADDR))),
    CAST(HIRE_DT AS DATE),
    CAST(TERM_DT AS DATE),
    STATUS_CD,
    DEPT_ID,
    MGR_ID
FROM LegacyDB.dbo.EMPLOYEES
WHERE HIRE_DT >= DATEADD(YEAR, -10, GETDATE()); -- Last 10 years
```

### 5.3 Transform Script

```sql
-- Transform and validate data
-- File: 03_transform_data.sql

-- Validate employees
UPDATE staging.Employee
SET
    IsValid = 0,
    ErrorMessage = CASE
        WHEN FirstName IS NULL OR FirstName = '' THEN 'Missing FirstName'
        WHEN LastName IS NULL OR LastName = '' THEN 'Missing LastName'
        WHEN Email IS NULL OR Email NOT LIKE '%@%.%' THEN 'Invalid Email'
        WHEN HireDate IS NULL THEN 'Missing HireDate'
        WHEN StatusCode NOT IN ('A', 'T', 'L', 'S') THEN 'Invalid Status'
        ELSE NULL
    END
WHERE
    FirstName IS NULL OR FirstName = ''
    OR LastName IS NULL OR LastName = ''
    OR Email IS NULL OR Email NOT LIKE '%@%.%'
    OR HireDate IS NULL
    OR StatusCode NOT IN ('A', 'T', 'L', 'S');

-- Log validation errors
SELECT SourceId, ErrorMessage
FROM staging.Employee
WHERE IsValid = 0;
```

### 5.4 Load Script

```sql
-- Load into target tables
-- File: 04_load_target_tables.sql

-- Disable constraints temporarily
ALTER TABLE dbo.Employee NOCHECK CONSTRAINT ALL;

-- Insert employees
INSERT INTO dbo.Employee (
    Id, EmployeeNumber, FirstName, LastName, Email,
    HireDate, TerminationDate, Status, DepartmentId,
    CreatedAt, CreatedBy
)
SELECT
    NEWID(),
    'EMP' + RIGHT('00000' + SourceId, 5),
    FirstName,
    LastName,
    Email,
    HireDate,
    TerminationDate,
    CASE StatusCode
        WHEN 'A' THEN 'Active'
        WHEN 'T' THEN 'Terminated'
        WHEN 'L' THEN 'OnLeave'
        WHEN 'S' THEN 'Suspended'
    END,
    d.Id,
    GETUTCDATE(),
    'Migration'
FROM staging.Employee s
LEFT JOIN dbo.Department d ON d.Code = s.DepartmentCode
WHERE s.IsValid = 1;

-- Update processed timestamp
UPDATE staging.Employee
SET ProcessedAt = GETUTCDATE()
WHERE IsValid = 1;

-- Re-enable constraints
ALTER TABLE dbo.Employee CHECK CONSTRAINT ALL;
```

### 5.5 Post-Migration Script

```sql
-- Post-migration validation
-- File: 05_validate_migration.sql

-- Count comparison
SELECT
    'Employee' AS Entity,
    (SELECT COUNT(*) FROM staging.Employee WHERE IsValid = 1) AS Staged,
    (SELECT COUNT(*) FROM dbo.Employee WHERE CreatedBy = 'Migration') AS Loaded;

-- Data integrity checks
SELECT 'Orphan Assignments' AS Check, COUNT(*) AS Count
FROM dbo.Assignment a
LEFT JOIN dbo.Employee e ON a.EmployeeId = e.Id
WHERE e.Id IS NULL;

-- Referential integrity
SELECT 'FK Violations' AS Check, COUNT(*) AS Count
FROM dbo.Calculation c
LEFT JOIN dbo.Employee e ON c.EmployeeId = e.Id
WHERE e.Id IS NULL;
```

---

## 6. Validation Rules

### 6.1 Record Count Validation

| Entity | Expected | Tolerance |
|--------|----------|-----------|
| Employees | Source count | 0% |
| Departments | Source count | 0% |
| Calculations | 100,000± | ±0.1% |
| Approvals | 50,000± | ±0.1% |

### 6.2 Data Integrity Validation

```sql
-- Validation queries
-- Run after migration

-- 1. No orphan records
SELECT 'Orphan Calculations' AS Test,
    CASE WHEN COUNT(*) = 0 THEN 'PASS' ELSE 'FAIL' END AS Result
FROM dbo.Calculation c
LEFT JOIN dbo.Employee e ON c.EmployeeId = e.Id
WHERE e.Id IS NULL;

-- 2. No duplicate employees
SELECT 'Duplicate Employees' AS Test,
    CASE WHEN COUNT(*) = 0 THEN 'PASS' ELSE 'FAIL' END AS Result
FROM (
    SELECT EmployeeNumber, COUNT(*) AS cnt
    FROM dbo.Employee
    GROUP BY EmployeeNumber
    HAVING COUNT(*) > 1
) dups;

-- 3. Valid date ranges
SELECT 'Invalid Date Ranges' AS Test,
    CASE WHEN COUNT(*) = 0 THEN 'PASS' ELSE 'FAIL' END AS Result
FROM dbo.IncentivePlan
WHERE EffectiveTo IS NOT NULL AND EffectiveTo < EffectiveFrom;
```

### 6.3 Business Rule Validation

| Rule | Query | Expected |
|------|-------|----------|
| Active employees have no termination date | `WHERE Status='Active' AND TerminationDate IS NOT NULL` | 0 records |
| All calculations have valid periods | `WHERE Period NOT LIKE '[0-9][0-9][0-9][0-9]-[0-9][0-9]'` | 0 records |
| Approved calculations have approver | `WHERE Status='Approved' AND ApprovedById IS NULL` | 0 records |

---

## 7. Execution Plan

### 7.1 Timeline

| Time | Activity | Owner | Duration |
|------|----------|-------|----------|
| T-7 | Deploy migration scripts to staging | DBA | 2 hours |
| T-5 | Run Phase 1: Reference data | DBA | 4 hours |
| T-3 | Validate Phase 1 data | BA | 2 hours |
| T-1 | Run Phase 2: Master data | DBA | 4 hours |
| T-1 | Validate Phase 2 data | BA | 2 hours |
| T-0 06:30 | Final backup of legacy system | DBA | 30 min |
| T-0 07:00 | Run Phase 3: Transactional data | DBA | 2 hours |
| T-0 09:00 | Run Phase 4: Validation | DBA/BA | 1 hour |
| T-0 10:00 | Sign-off on migration | Business | 30 min |

### 7.2 Execution Commands

```bash
# Phase 1: Reference Data
sqlcmd -S sql-dorise-prod-eastus.database.windows.net \
  -d sqldb-dorise-prod \
  -i scripts/phase1_reference_data.sql \
  -o logs/phase1_$(date +%Y%m%d_%H%M%S).log

# Phase 2: Master Data
sqlcmd -S sql-dorise-prod-eastus.database.windows.net \
  -d sqldb-dorise-prod \
  -i scripts/phase2_master_data.sql \
  -o logs/phase2_$(date +%Y%m%d_%H%M%S).log

# Phase 3: Transactional Data
sqlcmd -S sql-dorise-prod-eastus.database.windows.net \
  -d sqldb-dorise-prod \
  -i scripts/phase3_transactional_data.sql \
  -o logs/phase3_$(date +%Y%m%d_%H%M%S).log

# Phase 4: Validation
sqlcmd -S sql-dorise-prod-eastus.database.windows.net \
  -d sqldb-dorise-prod \
  -i scripts/phase4_validation.sql \
  -o logs/phase4_$(date +%Y%m%d_%H%M%S).log
```

---

## 8. Rollback Procedures

### 8.1 Rollback Triggers

- Validation failure rate > 1%
- Data integrity errors detected
- Missing critical records
- Performance issues during migration

### 8.2 Rollback Steps

```sql
-- Rollback migration
-- File: rollback_migration.sql

-- 1. Identify migrated records
SELECT COUNT(*) AS MigratedRecords
FROM dbo.Employee
WHERE CreatedBy = 'Migration';

-- 2. Delete in reverse order (due to FK constraints)
DELETE FROM dbo.AuditLog WHERE CreatedBy = 'Migration';
DELETE FROM dbo.Approval WHERE CreatedBy = 'Migration';
DELETE FROM dbo.Calculation WHERE CreatedBy = 'Migration';
DELETE FROM dbo.Assignment WHERE CreatedBy = 'Migration';
DELETE FROM dbo.Employee WHERE CreatedBy = 'Migration';
DELETE FROM dbo.IncentivePlan WHERE CreatedBy = 'Migration';
DELETE FROM dbo.Role WHERE CreatedBy = 'Migration';
DELETE FROM dbo.Store WHERE CreatedBy = 'Migration';
DELETE FROM dbo.Zone WHERE CreatedBy = 'Migration';
DELETE FROM dbo.Department WHERE CreatedBy = 'Migration';

-- 3. Clean staging tables
TRUNCATE TABLE staging.Employee;
-- Truncate other staging tables...

-- 4. Verify rollback
SELECT 'Employees after rollback' AS Check,
    COUNT(*) AS Count
FROM dbo.Employee
WHERE CreatedBy = 'Migration';
```

### 8.3 Point-in-Time Recovery

```bash
# If complete database restore needed
az sql db restore \
  --resource-group rg-dorise-prod-eastus \
  --server sql-dorise-prod-eastus \
  --name sqldb-dorise-prod \
  --dest-name sqldb-dorise-prod-restored \
  --time "2025-01-15T06:00:00Z"  # Pre-migration time
```

---

## Appendix A: Migration Checklist

### Pre-Migration

- [ ] All scripts tested in staging
- [ ] Source data extracted and validated
- [ ] Staging tables created
- [ ] Backup completed
- [ ] Team notified

### During Migration

- [ ] Phase 1 complete
- [ ] Phase 2 complete
- [ ] Phase 3 complete
- [ ] Phase 4 validation passed

### Post-Migration

- [ ] Record counts verified
- [ ] Data integrity confirmed
- [ ] Business validation complete
- [ ] Documentation updated

---

**Document History:**

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | Jan 2025 | DBA Team | Initial version |

*This document is part of the DSIF Operational Runbook series.*
