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
| CALCULATION_ENGINE_SPEC.md | Calculation engine technical spec | Pending (QG-3) | - |
| INTEGRATION_SPEC.md | External integration specifications | Pending (QG-3) | - |

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

---

## Quality Gate

These documents are required for **QG-2: Architecture Gate** (API_DESIGN.md) and **QG-3: Technical Gate** (remaining documents).

### Gate Status

| Document | Gate | Status |
|----------|------|--------|
| API_DESIGN.md | QG-2 | ✅ Complete |
| CALCULATION_ENGINE_SPEC.md | QG-3 | ⬜ Pending |
| INTEGRATION_SPEC.md | QG-3 | ⬜ Pending |

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

## Next Steps (QG-3)

1. Create CALCULATION_ENGINE_SPEC.md
2. Create INTEGRATION_SPEC.md
3. Define database schema scripts
4. Document coding standards in CLAUDE.md

---

*This folder is part of the Dorise Sales Incentive Framework project documentation.*
