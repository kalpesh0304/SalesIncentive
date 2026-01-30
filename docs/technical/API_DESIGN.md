# DORISE Sales Incentive Framework
## REST API Design

**Document ID:** DOC-TECH-001
**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Created:** January 2025
**Last Updated:** January 2025
**Author:** Claude Code
**Status:** Draft

> *"My cat's breath smells like cat food."* - Ralph Wiggum
>
> Our API responses are exactly what you expect - no surprises.

---

## Table of Contents

1. [Overview](#1-overview)
2. [API Conventions](#2-api-conventions)
3. [Authentication](#3-authentication)
4. [Employees API](#4-employees-api)
5. [Incentive Plans API](#5-incentive-plans-api)
6. [Calculations API](#6-calculations-api)
7. [Approvals API](#7-approvals-api)
8. [Reports API](#8-reports-api)
9. [Integration API](#9-integration-api)
10. [Error Handling](#10-error-handling)

---

## 1. Overview

### 1.1 API Information

| Property | Value |
|----------|-------|
| **Base URL (Dev)** | `https://api-dorise-dev.azurewebsites.net/api` |
| **Base URL (Staging)** | `https://api-dorise-stg.azurewebsites.net/api` |
| **Base URL (Prod)** | `https://api-dorise-prod.azurewebsites.net/api` |
| **API Version** | v1 |
| **Protocol** | HTTPS only |
| **Format** | JSON |

### 1.2 OpenAPI Specification

The full OpenAPI 3.0 specification is available at:
- Swagger UI: `{base-url}/swagger`
- OpenAPI JSON: `{base-url}/swagger/v1/swagger.json`

---

## 2. API Conventions

### 2.1 URL Structure

```
https://{host}/api/v{version}/{resource}/{id?}/{sub-resource?}

Examples:
GET  /api/v1/employees
GET  /api/v1/employees/123
GET  /api/v1/employees/123/calculations
POST /api/v1/calculations/batch
```

### 2.2 HTTP Methods

| Method | Usage | Idempotent |
|--------|-------|------------|
| `GET` | Retrieve resource(s) | Yes |
| `POST` | Create resource / Custom action | No |
| `PUT` | Full update of resource | Yes |
| `PATCH` | Partial update of resource | Yes |
| `DELETE` | Remove resource (soft delete) | Yes |

### 2.3 HTTP Status Codes

| Code | Meaning | Usage |
|------|---------|-------|
| `200 OK` | Success | GET, PUT, PATCH |
| `201 Created` | Resource created | POST |
| `204 No Content` | Success, no body | DELETE |
| `400 Bad Request` | Invalid input | Validation errors |
| `401 Unauthorized` | Not authenticated | Missing/invalid token |
| `403 Forbidden` | Not authorized | Insufficient permissions |
| `404 Not Found` | Resource not found | Invalid ID |
| `409 Conflict` | Conflict | Duplicate, version mismatch |
| `422 Unprocessable` | Business rule violation | Domain errors |
| `429 Too Many Requests` | Rate limited | Throttling |
| `500 Internal Error` | Server error | Unexpected errors |

### 2.4 Pagination

```json
// Request
GET /api/v1/employees?page=1&pageSize=20&sortBy=lastName&sortOrder=asc

// Response
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 150,
    "totalPages": 8,
    "hasNextPage": true,
    "hasPreviousPage": false
  }
}
```

### 2.5 Filtering

```
GET /api/v1/employees?status=Active&storeId=123&search=john

// Multiple values
GET /api/v1/calculations?status=Pending,Approved&period=2025-01
```

### 2.6 Request/Response Headers

**Request Headers:**
| Header | Required | Description |
|--------|----------|-------------|
| `Authorization` | Yes | Bearer token |
| `Content-Type` | Yes (POST/PUT) | `application/json` |
| `Accept` | No | `application/json` |
| `X-Correlation-Id` | No | Request tracking |

**Response Headers:**
| Header | Description |
|--------|-------------|
| `X-Correlation-Id` | Request tracking ID |
| `X-RateLimit-Limit` | Rate limit ceiling |
| `X-RateLimit-Remaining` | Requests remaining |
| `X-RateLimit-Reset` | Reset timestamp |

---

## 3. Authentication

### 3.1 OAuth 2.0 Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      AUTHENTICATION FLOW                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  1. Client requests token from Azure AD                                     │
│                                                                              │
│     POST https://login.microsoftonline.com/{tenant}/oauth2/v2.0/token       │
│     Content-Type: application/x-www-form-urlencoded                         │
│                                                                              │
│     client_id={client_id}                                                   │
│     &scope=api://dsif-api/.default                                          │
│     &client_secret={client_secret}                                          │
│     &grant_type=client_credentials                                          │
│                                                                              │
│  2. Azure AD returns access token                                           │
│                                                                              │
│     {                                                                        │
│       "access_token": "eyJ0eXAiOiJKV1Q...",                                 │
│       "token_type": "Bearer",                                               │
│       "expires_in": 3600                                                    │
│     }                                                                        │
│                                                                              │
│  3. Client includes token in API requests                                   │
│                                                                              │
│     GET /api/v1/employees                                                   │
│     Authorization: Bearer eyJ0eXAiOiJKV1Q...                                │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Token Claims

| Claim | Description |
|-------|-------------|
| `sub` | User principal ID |
| `name` | Display name |
| `email` | Email address |
| `roles` | Application roles |
| `scope` | Authorized scopes (zones/stores) |
| `aud` | API audience |
| `iss` | Token issuer |
| `exp` | Expiration time |

---

## 4. Employees API

### 4.1 Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/employees` | List employees |
| `GET` | `/employees/{id}` | Get employee details |
| `POST` | `/employees` | Create employee |
| `PUT` | `/employees/{id}` | Update employee |
| `DELETE` | `/employees/{id}` | Deactivate employee |
| `GET` | `/employees/{id}/assignments` | Get assignments |
| `POST` | `/employees/{id}/assignments` | Create assignment |
| `GET` | `/employees/{id}/calculations` | Get calculations |

### 4.2 List Employees

```http
GET /api/v1/employees?status=Active&storeId=123&page=1&pageSize=20

Authorization: Bearer {token}
```

**Response: 200 OK**
```json
{
  "data": [
    {
      "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "employeeNumber": "EMP001",
      "firstName": "John",
      "lastName": "Doe",
      "email": "john.doe@dorise.com",
      "status": "Active",
      "hireDate": "2020-03-15",
      "currentAssignment": {
        "storeId": "store-123",
        "storeName": "Downtown Store",
        "roleId": "role-sales-rep",
        "roleName": "Sales Representative"
      }
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 150,
    "totalPages": 8
  }
}
```

### 4.3 Get Employee

```http
GET /api/v1/employees/3fa85f64-5717-4562-b3fc-2c963f66afa6

Authorization: Bearer {token}
```

**Response: 200 OK**
```json
{
  "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "employeeNumber": "EMP001",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@dorise.com",
  "status": "Active",
  "hireDate": "2020-03-15",
  "terminationDate": null,
  "currentAssignment": {
    "assignmentId": "assign-001",
    "storeId": "store-123",
    "storeName": "Downtown Store",
    "zoneId": "zone-east",
    "zoneName": "East Zone",
    "roleId": "role-sales-rep",
    "roleName": "Sales Representative",
    "effectiveFrom": "2023-01-01",
    "effectiveTo": null,
    "isPrimary": true
  },
  "splitShares": [
    {
      "shareId": "share-001",
      "sharePercentage": 100.00,
      "effectiveFrom": "2023-01-01",
      "effectiveTo": null
    }
  ],
  "createdAt": "2020-03-15T09:00:00Z",
  "modifiedAt": "2024-06-01T14:30:00Z"
}
```

### 4.4 Create Employee

```http
POST /api/v1/employees

Authorization: Bearer {token}
Content-Type: application/json

{
  "employeeNumber": "EMP002",
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@dorise.com",
  "hireDate": "2025-02-01",
  "initialAssignment": {
    "storeId": "store-456",
    "roleId": "role-sales-rep",
    "effectiveFrom": "2025-02-01"
  }
}
```

**Response: 201 Created**
```json
{
  "employeeId": "7fa85f64-5717-4562-b3fc-2c963f66afa9",
  "employeeNumber": "EMP002",
  "firstName": "Jane",
  "lastName": "Smith",
  "status": "Active",
  "createdAt": "2025-01-29T10:00:00Z"
}
```

> *"I choo-choo-choose you!"* - Ralph Wiggum
>
> The API carefully validates each employee before creation.

---

## 5. Incentive Plans API

### 5.1 Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/plans` | List plans |
| `GET` | `/plans/{id}` | Get plan details |
| `POST` | `/plans` | Create plan |
| `PUT` | `/plans/{id}` | Update plan |
| `POST` | `/plans/{id}/versions` | Create new version |
| `GET` | `/plans/{id}/versions` | Get version history |
| `POST` | `/plans/{id}/activate` | Activate plan |

### 5.2 List Plans

```http
GET /api/v1/plans?status=Active&roleId=role-sales-rep

Authorization: Bearer {token}
```

**Response: 200 OK**
```json
{
  "data": [
    {
      "planId": "plan-001",
      "planCode": "SALES-2025",
      "planName": "Sales Rep Incentive Plan 2025",
      "description": "Standard incentive plan for sales representatives",
      "isActive": true,
      "currentVersion": {
        "versionId": "ver-003",
        "versionNumber": 3,
        "effectiveFrom": "2025-01-01",
        "effectiveTo": null,
        "status": "Active"
      },
      "applicableRoles": [
        {
          "roleId": "role-sales-rep",
          "roleName": "Sales Representative"
        }
      ]
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 5,
    "totalPages": 1
  }
}
```

### 5.3 Get Plan with Slabs

```http
GET /api/v1/plans/plan-001?includeSlabs=true

Authorization: Bearer {token}
```

**Response: 200 OK**
```json
{
  "planId": "plan-001",
  "planCode": "SALES-2025",
  "planName": "Sales Rep Incentive Plan 2025",
  "description": "Standard incentive plan for sales representatives",
  "isActive": true,
  "currentVersion": {
    "versionId": "ver-003",
    "versionNumber": 3,
    "effectiveFrom": "2025-01-01",
    "effectiveTo": null,
    "status": "Active",
    "slabs": [
      {
        "slabId": "slab-001",
        "slabOrder": 1,
        "slabName": "Bronze",
        "minThreshold": 0,
        "maxThreshold": 50000,
        "rateType": "Percentage",
        "rateValue": 0.02,
        "capAmount": null
      },
      {
        "slabId": "slab-002",
        "slabOrder": 2,
        "slabName": "Silver",
        "minThreshold": 50000.01,
        "maxThreshold": 100000,
        "rateType": "Percentage",
        "rateValue": 0.03,
        "capAmount": null
      },
      {
        "slabId": "slab-003",
        "slabOrder": 3,
        "slabName": "Gold",
        "minThreshold": 100000.01,
        "maxThreshold": null,
        "rateType": "Percentage",
        "rateValue": 0.05,
        "capAmount": 10000
      }
    ]
  },
  "applicableRoles": [
    {
      "roleId": "role-sales-rep",
      "roleName": "Sales Representative"
    }
  ],
  "createdAt": "2024-11-01T09:00:00Z",
  "createdBy": "admin@dorise.com"
}
```

### 5.4 Create Plan Version

```http
POST /api/v1/plans/plan-001/versions

Authorization: Bearer {token}
Content-Type: application/json

{
  "effectiveFrom": "2025-04-01",
  "slabs": [
    {
      "slabOrder": 1,
      "slabName": "Bronze",
      "minThreshold": 0,
      "maxThreshold": 60000,
      "rateType": "Percentage",
      "rateValue": 0.025
    },
    {
      "slabOrder": 2,
      "slabName": "Silver",
      "minThreshold": 60000.01,
      "maxThreshold": 120000,
      "rateType": "Percentage",
      "rateValue": 0.035
    },
    {
      "slabOrder": 3,
      "slabName": "Gold",
      "minThreshold": 120000.01,
      "maxThreshold": null,
      "rateType": "Percentage",
      "rateValue": 0.05,
      "capAmount": 12000
    }
  ],
  "changeReason": "Annual slab threshold adjustment"
}
```

**Response: 201 Created**
```json
{
  "versionId": "ver-004",
  "versionNumber": 4,
  "status": "Draft",
  "effectiveFrom": "2025-04-01",
  "createdAt": "2025-01-29T10:00:00Z"
}
```

---

## 6. Calculations API

### 6.1 Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/calculations` | List calculations |
| `GET` | `/calculations/{id}` | Get calculation details |
| `POST` | `/calculations/batch` | Run batch calculation |
| `POST` | `/calculations/preview` | Preview calculation |
| `POST` | `/calculations/{id}/recalculate` | Recalculate |
| `GET` | `/calculations/{id}/details` | Get line items |

### 6.2 Run Batch Calculation

```http
POST /api/v1/calculations/batch

Authorization: Bearer {token}
Content-Type: application/json

{
  "period": "2025-01",
  "storeIds": ["store-123", "store-456"],
  "executeAsync": true
}
```

**Response: 202 Accepted**
```json
{
  "batchId": "batch-2025-01-001",
  "status": "Queued",
  "period": "2025-01",
  "employeeCount": 150,
  "estimatedCompletionTime": "2025-01-29T10:30:00Z",
  "statusUrl": "/api/v1/calculations/batch/batch-2025-01-001/status"
}
```

### 6.3 Get Calculation Details

```http
GET /api/v1/calculations/calc-001?includeDetails=true

Authorization: Bearer {token}
```

**Response: 200 OK**
```json
{
  "calculationId": "calc-001",
  "employee": {
    "employeeId": "emp-001",
    "employeeNumber": "EMP001",
    "firstName": "John",
    "lastName": "Doe"
  },
  "period": "2025-01",
  "plan": {
    "planId": "plan-001",
    "planName": "Sales Rep Incentive Plan 2025",
    "versionNumber": 3
  },
  "grossAmount": 2500.00,
  "netAmount": 2500.00,
  "status": "Approved",
  "isAmendment": false,
  "calculatedAt": "2025-01-15T08:00:00Z",
  "batchId": "batch-2025-01-001",
  "details": [
    {
      "detailId": "det-001",
      "metricType": "SalesVolume",
      "metricValue": 85000.00,
      "slabApplied": "Silver",
      "rateApplied": 0.03,
      "baseAmount": 2550.00,
      "prorationFactor": 1.0,
      "splitFactor": 1.0,
      "calculatedAmount": 2550.00,
      "description": "Sales volume: $85,000 @ 3% (Silver tier)"
    },
    {
      "detailId": "det-002",
      "metricType": "Adjustment",
      "metricValue": -50.00,
      "slabApplied": null,
      "rateApplied": null,
      "baseAmount": -50.00,
      "prorationFactor": 1.0,
      "splitFactor": 1.0,
      "calculatedAmount": -50.00,
      "description": "Prior period adjustment"
    }
  ],
  "approvals": [
    {
      "approvalId": "appr-001",
      "level": 1,
      "status": "Approved",
      "approvedBy": "finance.manager@dorise.com",
      "approvedAt": "2025-01-18T14:30:00Z",
      "comments": "Verified and approved"
    }
  ]
}
```

### 6.4 Preview Calculation

```http
POST /api/v1/calculations/preview

Authorization: Bearer {token}
Content-Type: application/json

{
  "employeeId": "emp-001",
  "period": "2025-02",
  "overrideMetrics": {
    "salesVolume": 95000.00
  }
}
```

**Response: 200 OK**
```json
{
  "preview": true,
  "employee": {
    "employeeId": "emp-001",
    "employeeName": "John Doe"
  },
  "period": "2025-02",
  "estimatedGrossAmount": 2850.00,
  "details": [
    {
      "metricType": "SalesVolume",
      "metricValue": 95000.00,
      "slabApplied": "Silver",
      "rateApplied": 0.03,
      "calculatedAmount": 2850.00
    }
  ],
  "warnings": []
}
```

> *"Me fail English? That's unpossible!"* - Ralph Wiggum
>
> Calculation errors in our system are also unpossible - we validate everything.

---

## 7. Approvals API

### 7.1 Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/approvals/pending` | Get pending approvals |
| `POST` | `/approvals/{id}/approve` | Approve |
| `POST` | `/approvals/{id}/reject` | Reject |
| `POST` | `/approvals/bulk-approve` | Bulk approve |
| `GET` | `/approvals/{id}/history` | Get approval history |
| `POST` | `/approvals/delegate` | Set up delegation |

### 7.2 Get Pending Approvals

```http
GET /api/v1/approvals/pending?page=1&pageSize=20

Authorization: Bearer {token}
```

**Response: 200 OK**
```json
{
  "data": [
    {
      "approvalId": "appr-002",
      "calculation": {
        "calculationId": "calc-002",
        "employeeName": "Jane Smith",
        "period": "2025-01",
        "amount": 3200.00
      },
      "level": 1,
      "status": "Pending",
      "submittedBy": "payroll@dorise.com",
      "submittedAt": "2025-01-18T09:00:00Z",
      "daysWaiting": 2
    }
  ],
  "summary": {
    "totalPending": 25,
    "totalAmount": 75000.00,
    "oldestPendingDays": 5
  },
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalItems": 25,
    "totalPages": 2
  }
}
```

### 7.3 Approve Calculation

```http
POST /api/v1/approvals/appr-002/approve

Authorization: Bearer {token}
Content-Type: application/json

{
  "comments": "Verified and approved for payment"
}
```

**Response: 200 OK**
```json
{
  "approvalId": "appr-002",
  "status": "Approved",
  "approvedBy": "finance.manager@dorise.com",
  "approvedAt": "2025-01-29T10:00:00Z",
  "nextAction": null
}
```

### 7.4 Reject Calculation

```http
POST /api/v1/approvals/appr-003/reject

Authorization: Bearer {token}
Content-Type: application/json

{
  "reason": "Sales data appears incorrect - please verify with ERP system"
}
```

**Response: 200 OK**
```json
{
  "approvalId": "appr-003",
  "status": "Rejected",
  "rejectedBy": "finance.manager@dorise.com",
  "rejectedAt": "2025-01-29T10:05:00Z",
  "reason": "Sales data appears incorrect - please verify with ERP system"
}
```

### 7.5 Bulk Approve

```http
POST /api/v1/approvals/bulk-approve

Authorization: Bearer {token}
Content-Type: application/json

{
  "approvalIds": ["appr-004", "appr-005", "appr-006"],
  "comments": "Batch approved after verification"
}
```

**Response: 200 OK**
```json
{
  "processed": 3,
  "approved": 2,
  "skipped": 1,
  "results": [
    {
      "approvalId": "appr-004",
      "status": "Approved"
    },
    {
      "approvalId": "appr-005",
      "status": "Approved"
    },
    {
      "approvalId": "appr-006",
      "status": "Skipped",
      "reason": "Cannot approve own incentive"
    }
  ]
}
```

---

## 8. Reports API

### 8.1 Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/reports/dashboard` | Dashboard metrics |
| `GET` | `/reports/team-performance` | Team report |
| `GET` | `/reports/incentive-summary` | Summary report |
| `GET` | `/reports/audit-trail` | Audit report |
| `POST` | `/reports/export` | Export to file |

### 8.2 Dashboard Metrics

```http
GET /api/v1/reports/dashboard?period=2025-01

Authorization: Bearer {token}
```

**Response: 200 OK**
```json
{
  "period": "2025-01",
  "metrics": {
    "totalIncentivesPaid": 125000.00,
    "totalEmployees": 150,
    "averageIncentive": 833.33,
    "pendingApprovals": 25,
    "pendingAmount": 15000.00,
    "exportedThistPeriod": 110000.00
  },
  "trends": {
    "vsLastPeriod": {
      "totalIncentives": 0.05,
      "averageIncentive": 0.02
    },
    "vsLastYear": {
      "totalIncentives": 0.12,
      "averageIncentive": 0.08
    }
  },
  "topPerformers": [
    {
      "employeeId": "emp-001",
      "employeeName": "John Doe",
      "store": "Downtown Store",
      "incentiveAmount": 5200.00,
      "rank": 1
    }
  ],
  "byStatus": {
    "approved": 120,
    "pending": 25,
    "rejected": 5,
    "exported": 110
  }
}
```

### 8.3 Export Report

```http
POST /api/v1/reports/export

Authorization: Bearer {token}
Content-Type: application/json

{
  "reportType": "IncentiveSummary",
  "format": "Excel",
  "parameters": {
    "period": "2025-01",
    "storeIds": ["store-123"],
    "includeDetails": true
  }
}
```

**Response: 202 Accepted**
```json
{
  "exportId": "export-001",
  "status": "Processing",
  "downloadUrl": null,
  "expiresAt": null,
  "statusUrl": "/api/v1/reports/export/export-001/status"
}
```

**Status Check Response:**
```json
{
  "exportId": "export-001",
  "status": "Completed",
  "downloadUrl": "/api/v1/reports/export/export-001/download",
  "expiresAt": "2025-01-30T10:00:00Z",
  "fileSize": 245678,
  "fileName": "IncentiveSummary_2025-01.xlsx"
}
```

---

## 9. Integration API

### 9.1 Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/integration/import/sales` | Import sales data |
| `GET` | `/integration/import/{id}/status` | Import status |
| `POST` | `/integration/export/payroll` | Export to payroll |
| `GET` | `/integration/export/{id}/status` | Export status |
| `GET` | `/integration/webhooks` | List webhooks |
| `POST` | `/integration/webhooks` | Create webhook |

### 9.2 Import Sales Data

```http
POST /api/v1/integration/import/sales

Authorization: Bearer {token}
Content-Type: application/json

{
  "source": "ERP",
  "period": "2025-01",
  "records": [
    {
      "employeeNumber": "EMP001",
      "salesVolume": 85000.00,
      "unitsSOld": 150,
      "productionValue": 78000.00,
      "sellThroughRate": 0.85
    },
    {
      "employeeNumber": "EMP002",
      "salesVolume": 92000.00,
      "unitsSold": 180,
      "productionValue": 85000.00,
      "sellThroughRate": 0.88
    }
  ]
}
```

**Response: 202 Accepted**
```json
{
  "importId": "import-001",
  "status": "Processing",
  "recordsReceived": 2,
  "statusUrl": "/api/v1/integration/import/import-001/status"
}
```

### 9.3 Export to Payroll

```http
POST /api/v1/integration/export/payroll

Authorization: Bearer {token}
Content-Type: application/json

{
  "period": "2025-01",
  "includeApprovedOnly": true
}
```

**Response: 202 Accepted**
```json
{
  "exportId": "payroll-export-001",
  "status": "Queued",
  "recordsQueued": 110,
  "totalAmount": 110000.00,
  "statusUrl": "/api/v1/integration/export/payroll-export-001/status"
}
```

---

## 10. Error Handling

### 10.1 Error Response Format

```json
{
  "type": "https://api.dorise.com/errors/validation-error",
  "title": "Validation Error",
  "status": 400,
  "detail": "One or more validation errors occurred.",
  "instance": "/api/v1/employees",
  "correlationId": "abc-123-def",
  "timestamp": "2025-01-29T10:00:00Z",
  "errors": [
    {
      "field": "email",
      "code": "InvalidEmail",
      "message": "Email must be from @dorise.com domain"
    },
    {
      "field": "hireDate",
      "code": "FutureDate",
      "message": "Hire date cannot be in the future"
    }
  ]
}
```

### 10.2 Error Codes

| Code | HTTP Status | Description |
|------|-------------|-------------|
| `ValidationError` | 400 | Input validation failed |
| `InvalidCredentials` | 401 | Authentication failed |
| `InsufficientPermissions` | 403 | Not authorized |
| `ResourceNotFound` | 404 | Entity not found |
| `DuplicateResource` | 409 | Already exists |
| `ConcurrencyConflict` | 409 | Version mismatch |
| `BusinessRuleViolation` | 422 | Domain rule violation |
| `RateLimitExceeded` | 429 | Too many requests |
| `InternalError` | 500 | Server error |

### 10.3 Retry Guidelines

| Error Type | Retry | Backoff |
|------------|-------|---------|
| 4xx Client Errors | No | N/A |
| 429 Rate Limited | Yes | Exponential (1s, 2s, 4s) |
| 500 Server Error | Yes | Exponential (1s, 2s, 4s) |
| Network Timeout | Yes | Linear (1s, 2s, 3s) |

---

## Appendix A: Rate Limits

| Endpoint Type | Limit | Window |
|---------------|-------|--------|
| Read (GET) | 1000 | Per minute |
| Write (POST/PUT) | 100 | Per minute |
| Batch Operations | 10 | Per minute |
| Export/Import | 5 | Per minute |

---

## Appendix B: Webhooks

### Webhook Events

| Event | Trigger |
|-------|---------|
| `calculation.completed` | Batch calculation finished |
| `approval.required` | New approval pending |
| `approval.completed` | Approval decision made |
| `export.completed` | Payroll export finished |
| `import.completed` | Data import finished |

### Webhook Payload

```json
{
  "eventType": "calculation.completed",
  "timestamp": "2025-01-29T10:00:00Z",
  "data": {
    "batchId": "batch-001",
    "period": "2025-01",
    "recordsProcessed": 150,
    "totalAmount": 125000.00
  },
  "signature": "sha256=..."
}
```

---

> *"Go Banana!"* - Ralph Wiggum
>
> With this API, external systems can integrate smoothly with DSIF!

---

**Document Approval**

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Solution Architect | Skanda Prasad | _____________ | ______ |
| Lead Developer | Claude Code | _____________ | ______ |

---

*This document is part of the DSIF Quality Gate Framework - QG-2 Deliverable*
