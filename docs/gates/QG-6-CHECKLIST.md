# QG-6: Operations Gate Checklist

**Project:** Dorise Sales Incentive Framework (DSIF)
**Gate Owner:** DevOps Engineer + Product Owner
**Status:** 🔒 LOCKED (Requires QG-5)

---

## Purpose

This gate ensures that the system is ready for production deployment and ongoing operations.

---

## Prerequisites

- [ ] QG-5 (Testing Gate) must be PASSED

---

## Checklist

### Operational Documentation

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG6-D01 | DEPLOYMENT_RUNBOOK.md created and complete | ⬜ | `/docs/runbooks/DEPLOYMENT_RUNBOOK.md` | |
| QG6-D02 | INCIDENT_RESPONSE.md created and complete | ⬜ | `/docs/runbooks/INCIDENT_RESPONSE.md` | |
| QG6-D03 | MAINTENANCE.md created and complete | ⬜ | `/docs/runbooks/MAINTENANCE.md` | |

### Production Infrastructure

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG6-I01 | Production environment provisioned | ⬜ | Azure Portal | |
| QG6-I02 | prod.bicepparam created and tested | ⬜ | `/infra/bicep/parameters/prod.bicepparam` | |
| QG6-I03 | Production database ready | ⬜ | Azure SQL | |
| QG6-I04 | Backup and recovery tested | ⬜ | Test results | |

### CI/CD for Production

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG6-P01 | cd-stg.yml created and tested | ⬜ | `/.github/workflows/cd-stg.yml` | |
| QG6-P02 | cd-prod.yml created and tested | ⬜ | `/.github/workflows/cd-prod.yml` | |
| QG6-P03 | Deployment to staging successful | ⬜ | GitHub Actions | |
| QG6-P04 | Rollback procedure tested | ⬜ | Test results | |

### Monitoring & Alerting

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG6-M01 | Application Insights configured | ⬜ | Azure Portal | |
| QG6-M02 | Log Analytics configured | ⬜ | Azure Portal | |
| QG6-M03 | Alerts configured for critical metrics | ⬜ | Azure Portal | |
| QG6-M04 | Dashboard created | ⬜ | Azure Portal | |

### Security & Compliance

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG6-S01 | Private endpoints configured | ⬜ | Azure Portal | |
| QG6-S02 | WAF enabled | ⬜ | Azure Portal | |
| QG6-S03 | SSL/TLS certificates installed | ⬜ | Azure Portal | |
| QG6-S04 | RBAC configured | ⬜ | Azure Portal | |

### Go-Live Readiness

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG6-G01 | Smoke test on staging passed | ⬜ | Test results | |
| QG6-G02 | Data migration plan ready | ⬜ | Migration plan | |
| QG6-G03 | Support team trained | ⬜ | Training records | |
| QG6-G04 | Communication plan for go-live | ⬜ | Comms plan | |

---

## Acceptance Criteria

| ID | Criterion | Met? |
|----|-----------|------|
| QG6-AC1 | All runbooks complete and reviewed | ⬜ |
| QG6-AC2 | Production environment ready | ⬜ |
| QG6-AC3 | Monitoring and alerting functional | ⬜ |
| QG6-AC4 | Rollback procedure tested | ⬜ |
| QG6-AC5 | Go-live checklist complete | ⬜ |

---

## Sign-off

| Role | Name | Signature | Date |
|------|------|-----------|------|
| DevOps Engineer | [TBD] | _____________ | ______ |
| Product Owner | [TBD] | _____________ | ______ |
| Solution Architect | [TBD] | _____________ | ______ |

---

**Gate Review Date:** [TBD]
**Go-Live Target:** [TBD]

*This checklist is part of the DSIF Quality Gate Framework.*
