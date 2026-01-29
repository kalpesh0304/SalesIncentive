# QG-4: Infrastructure Gate Checklist

**Project:** Dorise Sales Incentive Framework (DSIF)
**Gate Owner:** DevOps Engineer
**Status:** 🟡 IN PROGRESS

> *"I choo-choo-choose you!"* - Ralph Wiggum
>
> We choo-choo-choose Azure for our cloud infrastructure - reliable, scalable, and enterprise-ready!

---

## Purpose

This gate ensures that the infrastructure is properly set up and CI/CD pipelines are functional before starting development. All Azure resources must be provisioned following the naming conventions and security standards defined in the governance documentation.

---

## Prerequisites

- [x] QG-3 (Technical Gate) must be PASSED ✅

---

## Checklist

### Azure Infrastructure

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG4-I01 | Resource Group (dev) created | ⬜ | `rg-dorise-dev-eastus` | |
| QG4-I02 | Resource Group (stg) created | ⬜ | `rg-dorise-stg-eastus` | |
| QG4-I03 | App Service Plan provisioned | ⬜ | `asp-dorise-dev` | |
| QG4-I04 | App Service (API) provisioned | ⬜ | `app-dorise-api-dev` | |
| QG4-I05 | Azure SQL Server provisioned | ⬜ | `sql-dorise-dev` | |
| QG4-I06 | Azure SQL Database provisioned | ⬜ | `sqldb-dorise-dev` | |
| QG4-I07 | Key Vault provisioned | ⬜ | `kv-dorise-dev` | |
| QG4-I08 | Application Insights provisioned | ⬜ | `appi-dorise-dev` | |
| QG4-I09 | Log Analytics Workspace provisioned | ⬜ | `log-dorise-dev` | |
| QG4-I10 | Storage Account provisioned | ⬜ | `stdorisedev` | |
| QG4-I11 | All resources tagged correctly | ⬜ | Tag verification | |
| QG4-I12 | Naming conventions verified | ⬜ | Compliance check | |

### Infrastructure as Code (Bicep)

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG4-B01 | main.bicep created | ✅ | `/infra/main.bicep` | Main orchestration template |
| QG4-B02 | appService.bicep module | ✅ | `/infra/modules/appService.bicep` | App Service resources |
| QG4-B03 | sqlDatabase.bicep module | ✅ | `/infra/modules/sqlDatabase.bicep` | Azure SQL resources |
| QG4-B04 | keyVault.bicep module | ✅ | `/infra/modules/keyVault.bicep` | Key Vault resources |
| QG4-B05 | monitoring.bicep module | ✅ | `/infra/modules/monitoring.bicep` | Monitoring resources |
| QG4-B06 | storage.bicep module | ✅ | `/infra/modules/storage.bicep` | Storage Account resources |
| QG4-B07 | dev.bicepparam created | ✅ | `/infra/parameters/dev.bicepparam` | Dev environment parameters |
| QG4-B08 | stg.bicepparam created | ✅ | `/infra/parameters/stg.bicepparam` | Staging environment parameters |
| QG4-B09 | prod.bicepparam created | ✅ | `/infra/parameters/prod.bicepparam` | Production environment parameters |
| QG4-B10 | Bicep templates validated | ⬜ | `az bicep build` | |

### CI/CD Pipelines (GitHub Actions)

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG4-P01 | CI pipeline created | ✅ | `/.github/workflows/ci.yml` | Build, test, lint |
| QG4-P02 | CD Dev pipeline created | ✅ | `/.github/workflows/cd-dev.yml` | Deploy to dev |
| QG4-P03 | CD Staging pipeline created | ✅ | `/.github/workflows/cd-stg.yml` | Deploy to staging |
| QG4-P04 | Infrastructure pipeline created | ✅ | `/.github/workflows/infra.yml` | Bicep deployment |
| QG4-P05 | CI pipeline passing | ⬜ | GitHub Actions | |
| QG4-P06 | CD pipeline deploying successfully | ⬜ | GitHub Actions | |

### Database Deployment

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG4-D01 | Dev database provisioned | ⬜ | Azure Portal | |
| QG4-D02 | Initial schema deployed | ⬜ | Migration logs | V001__Initial_Schema.sql |
| QG4-D03 | Seed data loaded | ⬜ | Database records | |
| QG4-D04 | Connection string in Key Vault | ⬜ | Key Vault secrets | |

### Security Configuration

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG4-S01 | Managed Identity enabled | ⬜ | App Service | |
| QG4-S02 | Key Vault access policies configured | ⬜ | Key Vault | |
| QG4-S03 | SQL firewall rules configured | ⬜ | Azure SQL | |
| QG4-S04 | TLS 1.2 enforced | ⬜ | All resources | |
| QG4-S05 | Diagnostic logging enabled | ⬜ | Log Analytics | |

---

## Acceptance Criteria

| ID | Criterion | Met? |
|----|-----------|------|
| QG4-AC1 | Dev environment fully operational | ⬜ |
| QG4-AC2 | CI pipeline builds and tests successfully | ⬜ |
| QG4-AC3 | CD pipeline deploys to dev environment | ⬜ |
| QG4-AC4 | Database accessible and schema deployed | ⬜ |
| QG4-AC5 | All resources follow naming conventions | ⬜ |
| QG4-AC6 | Infrastructure as Code reviewed | ⬜ |

---

## Progress Summary

| Category | Completed | Total | Percentage |
|----------|-----------|-------|------------|
| Azure Infrastructure | 0 | 12 | 0% |
| Infrastructure as Code | 10 | 10 | 100% |
| CI/CD Pipelines | 4 | 6 | 67% |
| Database Deployment | 0 | 4 | 0% |
| Security Configuration | 0 | 5 | 0% |
| Acceptance Criteria | 0 | 6 | 0% |
| **Overall** | **14** | **43** | **33%** |

---

## Deliverables Summary

### Infrastructure as Code Documents

| Document | Location | Description |
|----------|----------|-------------|
| main.bicep | `/infra/main.bicep` | Main orchestration template |
| Bicep Modules | `/infra/modules/` | Modular resource definitions |
| Parameters | `/infra/parameters/` | Environment-specific configs |
| README | `/infra/README.md` | Infrastructure documentation |

### CI/CD Pipelines

| Pipeline | Location | Triggers |
|----------|----------|----------|
| CI | `/.github/workflows/ci.yml` | Push to main, PRs |
| CD Dev | `/.github/workflows/cd-dev.yml` | Merge to main |
| CD Staging | `/.github/workflows/cd-stg.yml` | Manual, tag |
| Infrastructure | `/.github/workflows/infra.yml` | Manual |

---

## Azure Resource Summary

> *"My cat's breath smells like cat food."* - Ralph Wiggum
>
> Our Azure resources are configured exactly as expected - no surprises, just well-defined infrastructure!

### Development Environment

| Resource Type | Resource Name | SKU |
|---------------|---------------|-----|
| Resource Group | rg-dorise-dev-eastus | - |
| App Service Plan | asp-dorise-dev | B1 |
| App Service | app-dorise-api-dev | - |
| SQL Server | sql-dorise-dev | - |
| SQL Database | sqldb-dorise-dev | Basic (5 DTU) |
| Key Vault | kv-dorise-dev | Standard |
| App Insights | appi-dorise-dev | - |
| Log Analytics | log-dorise-dev | PerGB2018 |
| Storage Account | stdorisedev | Standard_LRS |

### Tagging Standard

All resources must include these tags:

```json
{
  "Project": "Dorise-Incentive",
  "Environment": "dev",
  "Owner": "DSIF-Team",
  "CostCenter": "CC-12345",
  "Application": "DSIF"
}
```

---

## Next Steps

1. ✅ Create Bicep templates
2. ✅ Create CI/CD pipelines
3. ⏳ Provision Azure resources
4. ⏳ Deploy initial database schema
5. ⏳ Verify CI/CD pipeline execution
6. ⏳ Complete security configuration

---

## Sign-off

| Role | Name | Signature | Date |
|------|------|-----------|------|
| DevOps Engineer | Claude Code | _____________ | ______ |
| Solution Architect | Skanda Prasad | _____________ | ______ |

---

**Gate Review Date:** [TBD]
**Gate Status:** 🟡 IN PROGRESS
**Next Gate:** QG-5 (Testing)

*This checklist is part of the DSIF Quality Gate Framework.*
