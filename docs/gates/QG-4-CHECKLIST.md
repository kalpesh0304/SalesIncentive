# QG-4: Infrastructure Gate Checklist

**Project:** Dorise Sales Incentive Framework (DSIF)
**Gate Owner:** DevOps Engineer
**Status:** 🔒 LOCKED (Requires QG-3)

---

## Purpose

This gate ensures that the infrastructure is properly set up and CI/CD pipelines are functional before starting development.

---

## Prerequisites

- [ ] QG-3 (Technical Gate) must be PASSED

---

## Checklist

### Azure Infrastructure

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG4-I01 | Dev environment provisioned | ⬜ | Azure Portal | |
| QG4-I02 | Staging environment provisioned | ⬜ | Azure Portal | |
| QG4-I03 | Naming conventions followed | ⬜ | Resource names | |
| QG4-I04 | Tagging applied to all resources | ⬜ | Resource tags | |

### Infrastructure as Code

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG4-B01 | main.bicep created and tested | ⬜ | `/infra/bicep/main.bicep` | |
| QG4-B02 | Bicep modules organized | ⬜ | `/infra/bicep/modules/` | |
| QG4-B03 | dev.bicepparam created | ⬜ | `/infra/bicep/parameters/dev.bicepparam` | |
| QG4-B04 | stg.bicepparam created | ⬜ | `/infra/bicep/parameters/stg.bicepparam` | |

### CI/CD Pipelines

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG4-P01 | CI pipeline (ci.yml) created and functional | ⬜ | `/.github/workflows/ci.yml` | |
| QG4-P02 | CD pipeline for dev created | ⬜ | `/.github/workflows/cd-dev.yml` | |
| QG4-P03 | Build passes successfully | ⬜ | GitHub Actions | |
| QG4-P04 | Deploy to dev succeeds | ⬜ | GitHub Actions | |

### Database

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG4-D01 | Dev database provisioned | ⬜ | Azure SQL | |
| QG4-D02 | Initial schema deployed | ⬜ | Migration logs | |
| QG4-D03 | Seed data loaded | ⬜ | Database records | |

---

## Acceptance Criteria

| ID | Criterion | Met? |
|----|-----------|------|
| QG4-AC1 | Dev environment fully functional | ⬜ |
| QG4-AC2 | CI pipeline builds and tests successfully | ⬜ |
| QG4-AC3 | CD pipeline deploys to dev environment | ⬜ |
| QG4-AC4 | Database accessible and seeded | ⬜ |
| QG4-AC5 | All resources follow naming conventions | ⬜ |

---

## Sign-off

| Role | Name | Signature | Date |
|------|------|-----------|------|
| DevOps Engineer | [TBD] | _____________ | ______ |
| Solution Architect | [TBD] | _____________ | ______ |

---

**Gate Review Date:** [TBD]
**Next Gate:** QG-5 (Testing)

*This checklist is part of the DSIF Quality Gate Framework.*
