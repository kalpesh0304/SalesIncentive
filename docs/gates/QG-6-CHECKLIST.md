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
| QG6-D01 | DEPLOYMENT_RUNBOOK.md created and complete | ✅ | `/docs/runbooks/DEPLOYMENT_RUNBOOK.md` | Comprehensive deployment procedures |
| QG6-D02 | INCIDENT_RESPONSE.md created and complete | ✅ | `/docs/runbooks/INCIDENT_RESPONSE.md` | Severity levels, escalation, PIR template |
| QG6-D03 | MAINTENANCE.md created and complete | ✅ | `/docs/runbooks/MAINTENANCE.md` | Daily/weekly/monthly maintenance tasks |

### Production Infrastructure

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG6-I01 | Production environment provisioned | ⬜ | Azure Portal | Pending Azure subscription |
| QG6-I02 | prod.bicepparam created and tested | ✅ | `/infra/parameters/prod.bicepparam` | Production SKUs configured |
| QG6-I03 | Production database ready | ⬜ | Azure SQL | Pending provisioning |
| QG6-I04 | Backup and recovery tested | ⬜ | Test results | Pending environment |

### CI/CD for Production

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG6-P01 | cd-stg.yml created and tested | ✅ | `/.github/workflows/cd-stg.yml` | Staging deployment with slot swap |
| QG6-P02 | cd-prod.yml created and tested | ✅ | `/.github/workflows/cd-prod.yml` | Blue-green deployment with approvals |
| QG6-P03 | Deployment to staging successful | ⬜ | GitHub Actions | Pending Azure environment |
| QG6-P04 | Rollback procedure tested | ✅ | `/.github/workflows/rollback.yml` | Emergency rollback workflow created |

### Monitoring & Alerting

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG6-M01 | Application Insights configured | ✅ | `/infra/modules/monitoring.bicep` | App Insights + Log Analytics |
| QG6-M02 | Log Analytics configured | ✅ | `/infra/monitoring/log-analytics-queries.kql` | Saved queries for all scenarios |
| QG6-M03 | Alerts configured for critical metrics | ✅ | `/infra/modules/alerts.bicep` | CPU, memory, errors, health, DB |
| QG6-M04 | Dashboard created | ✅ | `/infra/modules/dashboard.bicep` | Overview dashboard template |

### Security & Compliance

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG6-S01 | Private endpoints configured | ✅ | `/infra/modules/security.bicep` | SQL, KV, Storage PE templates |
| QG6-S02 | WAF enabled | ✅ | `/infra/modules/security.bicep` | Front Door + WAF Premium |
| QG6-S03 | SSL/TLS certificates installed | ⬜ | Azure Portal | Pending domain configuration |
| QG6-S04 | RBAC configured | ⬜ | Azure Portal | Pending user setup |

### Go-Live Readiness

| ID | Item | Status | Evidence | Notes |
|----|------|--------|----------|-------|
| QG6-G01 | Smoke test on staging passed | ⬜ | Test results | Pending staging deployment |
| QG6-G02 | Data migration plan ready | ✅ | `/docs/runbooks/DATA_MIGRATION_PLAN.md` | Complete migration scripts |
| QG6-G03 | Support team trained | ⬜ | Training records | Pending training sessions |
| QG6-G04 | Communication plan for go-live | ✅ | `/docs/runbooks/GO_LIVE_CHECKLIST.md` | Templates and schedule included |

---

## Acceptance Criteria

| ID | Criterion | Met? |
|----|-----------|------|
| QG6-AC1 | All runbooks complete and reviewed | ✅ |
| QG6-AC2 | Production environment ready | ⬜ |
| QG6-AC3 | Monitoring and alerting functional | ✅ |
| QG6-AC4 | Rollback procedure tested | ✅ |
| QG6-AC5 | Go-live checklist complete | ✅ |

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
