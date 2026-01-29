# Technical Documentation

**Project:** Dorise Sales Incentive Framework (DSIF)

> *"My cat's breath smells like cat food."* - Ralph Wiggum
>
> Our technical specs are exactly what you expect - clear, consistent, and well-documented.

---

## Purpose

This folder contains all technical specifications and API documentation for the DSIF project, defining the technical implementation details.

---

## Contents

| Document | Description | Status | Lines |
|----------|-------------|--------|-------|
| [API_DESIGN.md](./API_DESIGN.md) | REST API specifications | ✅ Complete | 900+ |
| [CALCULATION_ENGINE_SPEC.md](./CALCULATION_ENGINE_SPEC.md) | Calculation engine technical spec | ✅ Complete | 1200+ |
| [INTEGRATION_SPEC.md](./INTEGRATION_SPEC.md) | External integration specifications | ✅ Complete | 1100+ |

---

## Documentation Summary

### API_DESIGN.md
- **10 Sections** covering complete REST API design
- **API Conventions:** URL structure, HTTP methods, status codes
- **Pagination & Filtering:** Standard patterns for list endpoints
- **Authentication:** OAuth 2.0 with Azure AD
- **Employees API:** 8 endpoints for employee management
- **Incentive Plans API:** 7 endpoints for plan configuration
- **Calculations API:** 6 endpoints for calculation operations
- **Approvals API:** 6 endpoints for approval workflow
- **Reports API:** 5 endpoints for reporting and exports
- **Integration API:** 6 endpoints for external system integration
- **Error Handling:** Standard error response format, retry guidelines

### API Highlights

- **50+ Endpoints** documented with request/response examples
- **OpenAPI 3.0** compliant specification
- **Rate Limiting:** 1000 read, 100 write requests/minute
- **Webhooks:** 5 event types for async notifications
- **Batch Operations:** Support for bulk approve, import, export

### CALCULATION_ENGINE_SPEC.md
- **12 Sections** covering complete calculation engine design
- **Calculation Concepts:** Slab-based incentives, commission rates, thresholds
- **Process Flow:** End-to-end calculation workflow
- **Slab Configuration:** Selection algorithm, boundary handling
- **Point-in-Time Capture:** Temporal data snapshot logic
- **Proration Logic:** Mid-period join/leave calculations
- **Split Share:** Multi-employee contribution allocation
- **Amendments:** Post-calculation adjustments with audit trail
- **Formula Patterns:** Slab, tiered, flat, pro-rata formulas
- **Batch Processing:** Parallel calculation orchestration
- **Error Handling:** Recovery strategies, reconciliation

### INTEGRATION_SPEC.md
- **10 Sections** covering external system integrations
- **Integration Architecture:** Patterns, retry policies, circuit breakers
- **Inbound Integrations:** ERP (sales data), HRMS (employee sync), Azure AD
- **Outbound Integrations:** Payroll export, email notifications
- **Authentication:** OAuth 2.0, API keys, managed identities
- **Data Formats:** JSON, CSV schemas with validation rules
- **Error Handling:** Retry policies, dead letter queues
- **Monitoring:** Health checks, integration dashboards
- **Testing:** Integration test patterns, mock services
- **Operational Procedures:** Runbooks, troubleshooting guides

---

## Quality Gate

These documents are required for **QG-2: Architecture Gate** (API_DESIGN.md) and **QG-3: Technical Gate** (remaining documents).

### Gate Status: ✅ QG-3 DOCUMENTATION COMPLETE

| Document | Gate | Status |
|----------|------|--------|
| API_DESIGN.md | QG-2 | ✅ Complete |
| CALCULATION_ENGINE_SPEC.md | QG-3 | ✅ Complete |
| INTEGRATION_SPEC.md | QG-3 | ✅ Complete |

---

## API Quick Reference

### Base URLs

| Environment | URL |
|-------------|-----|
| Development | `https://api-dorise-dev.azurewebsites.net/api` |
| Staging | `https://api-dorise-stg.azurewebsites.net/api` |
| Production | `https://api-dorise-prod.azurewebsites.net/api` |

### Endpoint Categories

| Category | Endpoints | Description |
|----------|-----------|-------------|
| `/employees` | 8 | Employee and assignment management |
| `/plans` | 7 | Incentive plan configuration |
| `/calculations` | 6 | Calculation operations |
| `/approvals` | 6 | Approval workflow |
| `/reports` | 5 | Reporting and exports |
| `/integration` | 6 | External system integration |

### Authentication

```
Authorization: Bearer {access_token}
```

OAuth 2.0 client credentials flow with Azure AD.

---

## Related Documents

- `/docs/architecture/SOLUTION_ARCHITECTURE.md` - Solution architecture
- `/docs/architecture/DATA_ARCHITECTURE.md` - Data model
- `/docs/architecture/SECURITY_ARCHITECTURE.md` - Security controls
- `/docs/functional/REQUIREMENTS.md` - Functional requirements

---

## QG-3 Deliverables Summary

> *"The doctor said I wouldn't have so many nose bleeds if I kept my finger outta there."* - Ralph Wiggum
>
> Keep your code clean and well-documented - it's the best way to avoid technical debt!

All QG-3 technical documentation deliverables have been completed:

| Deliverable | Location | Status |
|-------------|----------|--------|
| CLAUDE.md | `/CLAUDE.md` | ✅ Complete (1600+ lines) |
| CALCULATION_ENGINE_SPEC.md | `/docs/technical/` | ✅ Complete (1200+ lines) |
| INTEGRATION_SPEC.md | `/docs/technical/` | ✅ Complete (1100+ lines) |
| Database Schema Scripts | `/src/database/migrations/` | ✅ Complete (500+ lines) |

### Database Schema

The database schema is implemented in:
- `/src/database/migrations/V001__Initial_Schema.sql`
- 20+ tables with temporal support
- Stored procedures for point-in-time queries
- Comprehensive indexing strategy

---

## Next Steps

1. ✅ Technical documentation complete
2. ✅ Technical review completed (QG3-AC4)
3. ✅ **QG-3 PASSED** - Proceed to Infrastructure phase (QG-4)

---

*This folder is part of the Dorise Sales Incentive Framework project documentation.*
