# Integration Test Cases

**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Last Updated:** January 2025
**Total Test Cases:** 120

> *"I'm a furniture!"* - Ralph Wiggum
>
> Our integrations are rock-solid furniture - everything fits together perfectly!

---

## Table of Contents

1. [Overview](#overview)
2. [API Integration Tests](#api-integration-tests)
3. [Database Integration Tests](#database-integration-tests)
4. [External Service Integration Tests](#external-service-integration-tests)
5. [Event/Message Integration Tests](#eventmessage-integration-tests)
6. [End-to-End Workflow Tests](#end-to-end-workflow-tests)
7. [Test Execution Summary](#test-execution-summary)

---

## Overview

### Test Coverage Summary

| Category | Test Cases | Priority |
|----------|------------|----------|
| API Integration | 65 | High |
| Database Integration | 25 | High |
| External Services | 15 | Medium |
| Events/Messages | 10 | Medium |
| E2E Workflows | 5 | Critical |
| **Total** | **120** | - |

### Environment Requirements

- Integration test database (SQL Server)
- Mock external services (WireMock)
- Message queue (Azure Service Bus emulator)
- Test API server (WebApplicationFactory)

---

## API Integration Tests

### Employee API Tests (INT-EMP-001 to INT-EMP-020)

> *"I sleep in a drawer!"* - Ralph Wiggum
>
> Our employee data is well-organized - unlike Ralph's sleeping arrangements!

#### INT-EMP-001: Create Employee - Valid Request

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-001 |
| **Title** | Create new employee with valid data |
| **Priority** | High |
| **API Endpoint** | `POST /api/employees` |
| **Prerequisites** | None |

**Request:**
```json
{
  "employeeCode": "EMP001",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@company.com",
  "department": "Sales",
  "designation": "Sales Executive",
  "joinDate": "2024-01-15",
  "managerId": null
}
```

**Expected Response:**
- Status: `201 Created`
- Body contains employee ID
- Employee persisted in database

---

#### INT-EMP-002: Create Employee - Duplicate Employee Code

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-002 |
| **Title** | Reject duplicate employee code |
| **Priority** | High |
| **API Endpoint** | `POST /api/employees` |
| **Prerequisites** | Employee EMP001 exists |

**Request:**
```json
{
  "employeeCode": "EMP001",
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane.smith@company.com"
}
```

**Expected Response:**
- Status: `409 Conflict`
- Error message: "Employee code already exists"

---

#### INT-EMP-003: Create Employee - Invalid Email Format

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-003 |
| **Title** | Reject invalid email format |
| **Priority** | Medium |
| **API Endpoint** | `POST /api/employees` |

**Request:**
```json
{
  "employeeCode": "EMP002",
  "firstName": "Invalid",
  "lastName": "Email",
  "email": "not-an-email"
}
```

**Expected Response:**
- Status: `400 Bad Request`
- Validation error for email field

---

#### INT-EMP-004: Get Employee - Valid ID

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-004 |
| **Title** | Retrieve employee by valid ID |
| **Priority** | High |
| **API Endpoint** | `GET /api/employees/{id}` |
| **Prerequisites** | Employee exists in database |

**Expected Response:**
- Status: `200 OK`
- Complete employee details returned

---

#### INT-EMP-005: Get Employee - Not Found

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-005 |
| **Title** | Return 404 for non-existent employee |
| **Priority** | Medium |
| **API Endpoint** | `GET /api/employees/{id}` |

**Expected Response:**
- Status: `404 Not Found`
- Error message: "Employee not found"

---

#### INT-EMP-006: Update Employee - Valid Update

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-006 |
| **Title** | Update employee details successfully |
| **Priority** | High |
| **API Endpoint** | `PUT /api/employees/{id}` |
| **Prerequisites** | Employee exists |

**Request:**
```json
{
  "firstName": "John",
  "lastName": "Doe Updated",
  "department": "Enterprise Sales"
}
```

**Expected Response:**
- Status: `200 OK`
- Updated employee returned
- Database reflects changes

---

#### INT-EMP-007: Update Employee - Concurrent Modification

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-007 |
| **Title** | Handle concurrent update conflict |
| **Priority** | Medium |
| **API Endpoint** | `PUT /api/employees/{id}` |
| **Prerequisites** | Employee exists, outdated ETag |

**Expected Response:**
- Status: `409 Conflict`
- Error: "Resource has been modified"

---

#### INT-EMP-008: Delete Employee - Soft Delete

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-008 |
| **Title** | Soft delete employee |
| **Priority** | High |
| **API Endpoint** | `DELETE /api/employees/{id}` |
| **Prerequisites** | Employee exists, no pending calculations |

**Expected Response:**
- Status: `204 No Content`
- Employee marked as deleted, not removed

---

#### INT-EMP-009: Delete Employee - With Pending Calculations

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-009 |
| **Title** | Prevent delete with pending calculations |
| **Priority** | High |
| **API Endpoint** | `DELETE /api/employees/{id}` |
| **Prerequisites** | Employee has pending incentive calculations |

**Expected Response:**
- Status: `400 Bad Request`
- Error: "Cannot delete employee with pending calculations"

---

#### INT-EMP-010: List Employees - Pagination

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-010 |
| **Title** | List employees with pagination |
| **Priority** | Medium |
| **API Endpoint** | `GET /api/employees?page=1&pageSize=10` |
| **Prerequisites** | 50 employees in database |

**Expected Response:**
- Status: `200 OK`
- 10 employees returned
- Total count: 50
- Pagination metadata included

---

#### INT-EMP-011: List Employees - Filter by Department

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-011 |
| **Title** | Filter employees by department |
| **Priority** | Medium |
| **API Endpoint** | `GET /api/employees?department=Sales` |

**Expected Response:**
- Status: `200 OK`
- Only Sales department employees returned

---

#### INT-EMP-012: List Employees - Search by Name

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-012 |
| **Title** | Search employees by name |
| **Priority** | Medium |
| **API Endpoint** | `GET /api/employees?search=john` |

**Expected Response:**
- Status: `200 OK`
- Employees matching "john" returned

---

#### INT-EMP-013: Bulk Import Employees

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-013 |
| **Title** | Bulk import employees from CSV |
| **Priority** | High |
| **API Endpoint** | `POST /api/employees/bulk-import` |

**Request:** Multipart form with CSV file

**Expected Response:**
- Status: `202 Accepted`
- Import job ID returned
- Employees created asynchronously

---

#### INT-EMP-014: Bulk Import - Validation Errors

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-014 |
| **Title** | Handle bulk import validation errors |
| **Priority** | Medium |
| **API Endpoint** | `POST /api/employees/bulk-import` |

**Request:** CSV with invalid rows

**Expected Response:**
- Status: `200 OK`
- Success count and error details returned
- Valid rows processed, invalid rows reported

---

#### INT-EMP-015: Get Employee Eligibility

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-015 |
| **Title** | Get employee plan eligibility |
| **Priority** | High |
| **API Endpoint** | `GET /api/employees/{id}/eligibility` |

**Expected Response:**
- Status: `200 OK`
- List of eligible incentive plans

---

#### INT-EMP-016: Assign Employee to Plan

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-016 |
| **Title** | Assign employee to incentive plan |
| **Priority** | High |
| **API Endpoint** | `POST /api/employees/{id}/plans` |

**Request:**
```json
{
  "planId": "PLAN-001",
  "effectiveDate": "2024-02-01",
  "targetAmount": 100000
}
```

**Expected Response:**
- Status: `201 Created`
- Assignment created

---

#### INT-EMP-017: Get Employee History

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-017 |
| **Title** | Retrieve employee change history |
| **Priority** | Low |
| **API Endpoint** | `GET /api/employees/{id}/history` |

**Expected Response:**
- Status: `200 OK`
- Audit trail of employee changes

---

#### INT-EMP-018: Transfer Employee

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-018 |
| **Title** | Transfer employee to new department |
| **Priority** | Medium |
| **API Endpoint** | `POST /api/employees/{id}/transfer` |

**Request:**
```json
{
  "newDepartment": "Enterprise Sales",
  "newManager": "MGR-002",
  "effectiveDate": "2024-03-01"
}
```

**Expected Response:**
- Status: `200 OK`
- Transfer recorded, plan adjustments calculated

---

#### INT-EMP-019: Terminate Employee

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-019 |
| **Title** | Terminate employee with proration |
| **Priority** | High |
| **API Endpoint** | `POST /api/employees/{id}/terminate` |

**Request:**
```json
{
  "terminationDate": "2024-06-15",
  "reason": "Resignation",
  "finalSettlement": true
}
```

**Expected Response:**
- Status: `200 OK`
- Prorated calculations triggered
- Employee status updated

---

#### INT-EMP-020: Get Employee Dashboard

| Field | Value |
|-------|-------|
| **ID** | INT-EMP-020 |
| **Title** | Get employee incentive dashboard |
| **Priority** | Medium |
| **API Endpoint** | `GET /api/employees/{id}/dashboard` |

**Expected Response:**
- Status: `200 OK`
- YTD earnings, pending calculations, plan details

---

### Incentive Plan API Tests (INT-PLN-001 to INT-PLN-015)

> *"I bent my wookiee!"* - Ralph Wiggum
>
> Our incentive plans don't bend - they're rigorously tested!

#### INT-PLN-001: Create Incentive Plan

| Field | Value |
|-------|-------|
| **ID** | INT-PLN-001 |
| **Title** | Create new incentive plan |
| **Priority** | Critical |
| **API Endpoint** | `POST /api/plans` |

**Request:**
```json
{
  "name": "Q1 2024 Sales Plan",
  "description": "Quarterly sales incentive",
  "startDate": "2024-01-01",
  "endDate": "2024-03-31",
  "planType": "Commission",
  "slabs": [
    { "minAchievement": 0, "maxAchievement": 80, "rate": 0 },
    { "minAchievement": 80, "maxAchievement": 100, "rate": 5 },
    { "minAchievement": 100, "maxAchievement": 120, "rate": 8 },
    { "minAchievement": 120, "maxAchievement": null, "rate": 12 }
  ]
}
```

**Expected Response:**
- Status: `201 Created`
- Plan ID returned
- Slabs validated and stored

---

#### INT-PLN-002: Create Plan - Overlapping Slabs

| Field | Value |
|-------|-------|
| **ID** | INT-PLN-002 |
| **Title** | Reject overlapping slab definitions |
| **Priority** | High |
| **API Endpoint** | `POST /api/plans` |

**Request:** Plan with overlapping slab ranges

**Expected Response:**
- Status: `400 Bad Request`
- Validation error: "Slab ranges must not overlap"

---

#### INT-PLN-003: Create Plan - Gap in Slabs

| Field | Value |
|-------|-------|
| **ID** | INT-PLN-003 |
| **Title** | Reject gaps in slab coverage |
| **Priority** | High |
| **API Endpoint** | `POST /api/plans` |

**Request:** Plan with gaps in slab ranges

**Expected Response:**
- Status: `400 Bad Request`
- Validation error: "Slab ranges must be continuous"

---

#### INT-PLN-004: Get Plan Details

| Field | Value |
|-------|-------|
| **ID** | INT-PLN-004 |
| **Title** | Retrieve plan with slabs |
| **Priority** | High |
| **API Endpoint** | `GET /api/plans/{id}` |

**Expected Response:**
- Status: `200 OK`
- Complete plan with slabs returned

---

#### INT-PLN-005: Update Plan - Draft Status

| Field | Value |
|-------|-------|
| **ID** | INT-PLN-005 |
| **Title** | Update plan in draft status |
| **Priority** | High |
| **API Endpoint** | `PUT /api/plans/{id}` |
| **Prerequisites** | Plan in draft status |

**Expected Response:**
- Status: `200 OK`
- Plan updated successfully

---

#### INT-PLN-006: Update Plan - Active Status

| Field | Value |
|-------|-------|
| **ID** | INT-PLN-006 |
| **Title** | Reject update to active plan |
| **Priority** | High |
| **API Endpoint** | `PUT /api/plans/{id}` |
| **Prerequisites** | Plan in active status |

**Expected Response:**
- Status: `400 Bad Request`
- Error: "Cannot modify active plan"

---

#### INT-PLN-007: Activate Plan

| Field | Value |
|-------|-------|
| **ID** | INT-PLN-007 |
| **Title** | Activate draft plan |
| **Priority** | Critical |
| **API Endpoint** | `POST /api/plans/{id}/activate` |

**Expected Response:**
- Status: `200 OK`
- Plan status changed to Active

---

#### INT-PLN-008: Activate Plan - Missing Configuration

| Field | Value |
|-------|-------|
| **ID** | INT-PLN-008 |
| **Title** | Reject activation of incomplete plan |
| **Priority** | High |
| **API Endpoint** | `POST /api/plans/{id}/activate` |
| **Prerequisites** | Plan missing required slabs |

**Expected Response:**
- Status: `400 Bad Request`
- Validation errors listing missing items

---

#### INT-PLN-009: Clone Plan

| Field | Value |
|-------|-------|
| **ID** | INT-PLN-009 |
| **Title** | Clone existing plan |
| **Priority** | Medium |
| **API Endpoint** | `POST /api/plans/{id}/clone` |

**Request:**
```json
{
  "newName": "Q2 2024 Sales Plan",
  "startDate": "2024-04-01",
  "endDate": "2024-06-30"
}
```

**Expected Response:**
- Status: `201 Created`
- New plan created with copied slabs

---

#### INT-PLN-010: Expire Plan

| Field | Value |
|-------|-------|
| **ID** | INT-PLN-010 |
| **Title** | Expire active plan |
| **Priority** | Medium |
| **API Endpoint** | `POST /api/plans/{id}/expire` |

**Expected Response:**
- Status: `200 OK`
- Plan status changed to Expired

---

#### INT-PLN-011: List Plans with Filters

| Field | Value |
|-------|-------|
| **ID** | INT-PLN-011 |
| **Title** | List plans with status filter |
| **Priority** | Medium |
| **API Endpoint** | `GET /api/plans?status=Active` |

**Expected Response:**
- Status: `200 OK`
- Only active plans returned

---

#### INT-PLN-012: Get Plan Participants

| Field | Value |
|-------|-------|
| **ID** | INT-PLN-012 |
| **Title** | Get employees assigned to plan |
| **Priority** | Medium |
| **API Endpoint** | `GET /api/plans/{id}/participants` |

**Expected Response:**
- Status: `200 OK`
- List of assigned employees

---

#### INT-PLN-013: Add Plan Rule

| Field | Value |
|-------|-------|
| **ID** | INT-PLN-013 |
| **Title** | Add business rule to plan |
| **Priority** | High |
| **API Endpoint** | `POST /api/plans/{id}/rules` |

**Request:**
```json
{
  "ruleType": "MinimumQualification",
  "condition": "MonthsOfService >= 3",
  "action": "Qualify"
}
```

**Expected Response:**
- Status: `201 Created`
- Rule added to plan

---

#### INT-PLN-014: Validate Plan Integrity

| Field | Value |
|-------|-------|
| **ID** | INT-PLN-014 |
| **Title** | Validate plan configuration |
| **Priority** | Medium |
| **API Endpoint** | `POST /api/plans/{id}/validate` |

**Expected Response:**
- Status: `200 OK`
- Validation results with warnings/errors

---

#### INT-PLN-015: Get Plan Metrics

| Field | Value |
|-------|-------|
| **ID** | INT-PLN-015 |
| **Title** | Get plan performance metrics |
| **Priority** | Low |
| **API Endpoint** | `GET /api/plans/{id}/metrics` |

**Expected Response:**
- Status: `200 OK`
- Participant count, total payout, avg achievement

---

### Calculation API Tests (INT-CALC-001 to INT-CALC-015)

> *"Me fail English? That's unpossible!"* - Ralph Wiggum
>
> Our calculations never fail - they're mathematically impeccable!

#### INT-CALC-001: Calculate Single Employee

| Field | Value |
|-------|-------|
| **ID** | INT-CALC-001 |
| **Title** | Calculate incentive for single employee |
| **Priority** | Critical |
| **API Endpoint** | `POST /api/calculations` |

**Request:**
```json
{
  "employeeId": "EMP-001",
  "planId": "PLAN-001",
  "periodStart": "2024-01-01",
  "periodEnd": "2024-01-31",
  "achievements": [
    { "metricId": "REVENUE", "value": 95000 }
  ]
}
```

**Expected Response:**
- Status: `201 Created`
- Calculation ID returned
- Slab applied, amount calculated

---

#### INT-CALC-002: Calculate - Multiple Slabs

| Field | Value |
|-------|-------|
| **ID** | INT-CALC-002 |
| **Title** | Calculate with achievement spanning slabs |
| **Priority** | Critical |
| **API Endpoint** | `POST /api/calculations` |

**Request:** Achievement of 115% (spans 100-120 and 120+ slabs)

**Expected Response:**
- Status: `201 Created`
- Correct amount with slab breakdown

---

#### INT-CALC-003: Calculate - Zero Achievement

| Field | Value |
|-------|-------|
| **ID** | INT-CALC-003 |
| **Title** | Handle zero achievement |
| **Priority** | High |
| **API Endpoint** | `POST /api/calculations` |

**Request:** Achievement of 0%

**Expected Response:**
- Status: `201 Created`
- Incentive amount: 0

---

#### INT-CALC-004: Calculate - Below Threshold

| Field | Value |
|-------|-------|
| **ID** | INT-CALC-004 |
| **Title** | Achievement below minimum threshold |
| **Priority** | High |
| **API Endpoint** | `POST /api/calculations` |

**Request:** Achievement of 50% (below 80% threshold)

**Expected Response:**
- Status: `201 Created`
- Incentive amount: 0 (not qualified)

---

#### INT-CALC-005: Calculate - Maximum Cap

| Field | Value |
|-------|-------|
| **ID** | INT-CALC-005 |
| **Title** | Apply maximum incentive cap |
| **Priority** | High |
| **API Endpoint** | `POST /api/calculations` |

**Request:** Achievement of 200% with capped plan

**Expected Response:**
- Status: `201 Created`
- Amount capped at maximum limit

---

#### INT-CALC-006: Batch Calculation

| Field | Value |
|-------|-------|
| **ID** | INT-CALC-006 |
| **Title** | Batch calculation for all employees |
| **Priority** | Critical |
| **API Endpoint** | `POST /api/calculations/batch` |

**Request:**
```json
{
  "planId": "PLAN-001",
  "periodStart": "2024-01-01",
  "periodEnd": "2024-01-31"
}
```

**Expected Response:**
- Status: `202 Accepted`
- Batch job ID returned
- Processing asynchronously

---

#### INT-CALC-007: Get Calculation Status

| Field | Value |
|-------|-------|
| **ID** | INT-CALC-007 |
| **Title** | Get batch calculation status |
| **Priority** | High |
| **API Endpoint** | `GET /api/calculations/batch/{batchId}` |

**Expected Response:**
- Status: `200 OK`
- Progress, completed count, errors

---

#### INT-CALC-008: Recalculate with Adjustment

| Field | Value |
|-------|-------|
| **ID** | INT-CALC-008 |
| **Title** | Recalculate with achievement adjustment |
| **Priority** | High |
| **API Endpoint** | `POST /api/calculations/{id}/recalculate` |

**Request:**
```json
{
  "adjustments": [
    { "metricId": "REVENUE", "value": 5000, "reason": "Late order" }
  ]
}
```

**Expected Response:**
- Status: `200 OK`
- New calculation with adjustment applied

---

#### INT-CALC-009: Calculate - Prorated New Joiner

| Field | Value |
|-------|-------|
| **ID** | INT-CALC-009 |
| **Title** | Calculate prorated for mid-period joiner |
| **Priority** | High |
| **API Endpoint** | `POST /api/calculations` |

**Request:** Employee joined Jan 15, period is full month

**Expected Response:**
- Status: `201 Created`
- Amount prorated for 17 days

---

#### INT-CALC-010: Calculate - Prorated Leaver

| Field | Value |
|-------|-------|
| **ID** | INT-CALC-010 |
| **Title** | Calculate prorated for mid-period leaver |
| **Priority** | High |
| **API Endpoint** | `POST /api/calculations` |

**Request:** Employee left Jan 20, period is full month

**Expected Response:**
- Status: `201 Created`
- Amount prorated for 20 days

---

#### INT-CALC-011: Get Calculation History

| Field | Value |
|-------|-------|
| **ID** | INT-CALC-011 |
| **Title** | Get employee calculation history |
| **Priority** | Medium |
| **API Endpoint** | `GET /api/employees/{id}/calculations` |

**Expected Response:**
- Status: `200 OK`
- List of past calculations

---

#### INT-CALC-012: Void Calculation

| Field | Value |
|-------|-------|
| **ID** | INT-CALC-012 |
| **Title** | Void erroneous calculation |
| **Priority** | Medium |
| **API Endpoint** | `POST /api/calculations/{id}/void` |

**Request:**
```json
{
  "reason": "Incorrect achievement data"
}
```

**Expected Response:**
- Status: `200 OK`
- Calculation voided, audit logged

---

#### INT-CALC-013: Calculate - Multiple Metrics

| Field | Value |
|-------|-------|
| **ID** | INT-CALC-013 |
| **Title** | Calculate with multiple achievement metrics |
| **Priority** | High |
| **API Endpoint** | `POST /api/calculations` |

**Request:**
```json
{
  "achievements": [
    { "metricId": "REVENUE", "value": 100000, "weight": 0.7 },
    { "metricId": "NEWCUSTOMERS", "value": 15, "weight": 0.3 }
  ]
}
```

**Expected Response:**
- Status: `201 Created`
- Weighted calculation applied

---

#### INT-CALC-014: Calculate - Currency Conversion

| Field | Value |
|-------|-------|
| **ID** | INT-CALC-014 |
| **Title** | Calculate with currency conversion |
| **Priority** | Medium |
| **API Endpoint** | `POST /api/calculations` |

**Request:** Achievement in USD, payout in INR

**Expected Response:**
- Status: `201 Created`
- Converted amount at period exchange rate

---

#### INT-CALC-015: Get Calculation Breakdown

| Field | Value |
|-------|-------|
| **ID** | INT-CALC-015 |
| **Title** | Get detailed calculation breakdown |
| **Priority** | Medium |
| **API Endpoint** | `GET /api/calculations/{id}/breakdown` |

**Expected Response:**
- Status: `200 OK`
- Step-by-step calculation details

---

### Approval Workflow API Tests (INT-APR-001 to INT-APR-015)

> *"I'm pedaling backwards!"* - Ralph Wiggum
>
> Our approval workflows only move forward - towards sign-off!

#### INT-APR-001: Submit for Approval

| Field | Value |
|-------|-------|
| **ID** | INT-APR-001 |
| **Title** | Submit calculation for approval |
| **Priority** | High |
| **API Endpoint** | `POST /api/approvals` |

**Request:**
```json
{
  "calculationIds": ["CALC-001", "CALC-002"],
  "comments": "Monthly incentive submission"
}
```

**Expected Response:**
- Status: `201 Created`
- Approval request created
- Notifications sent

---

#### INT-APR-002: Approve Single Calculation

| Field | Value |
|-------|-------|
| **ID** | INT-APR-002 |
| **Title** | Approve single calculation |
| **Priority** | High |
| **API Endpoint** | `POST /api/approvals/{id}/approve` |

**Request:**
```json
{
  "comments": "Verified and approved"
}
```

**Expected Response:**
- Status: `200 OK`
- Status changed to Approved

---

#### INT-APR-003: Reject Calculation

| Field | Value |
|-------|-------|
| **ID** | INT-APR-003 |
| **Title** | Reject calculation with reason |
| **Priority** | High |
| **API Endpoint** | `POST /api/approvals/{id}/reject` |

**Request:**
```json
{
  "reason": "Incorrect achievement data",
  "comments": "Please verify with sales records"
}
```

**Expected Response:**
- Status: `200 OK`
- Status changed to Rejected
- Notification sent to submitter

---

#### INT-APR-004: Bulk Approve

| Field | Value |
|-------|-------|
| **ID** | INT-APR-004 |
| **Title** | Bulk approve multiple calculations |
| **Priority** | High |
| **API Endpoint** | `POST /api/approvals/bulk-approve` |

**Request:**
```json
{
  "approvalIds": ["APR-001", "APR-002", "APR-003"],
  "comments": "Batch approved"
}
```

**Expected Response:**
- Status: `200 OK`
- All approvals processed

---

#### INT-APR-005: Delegate Approval

| Field | Value |
|-------|-------|
| **ID** | INT-APR-005 |
| **Title** | Delegate approval to another user |
| **Priority** | Medium |
| **API Endpoint** | `POST /api/approvals/{id}/delegate` |

**Request:**
```json
{
  "delegateTo": "USER-002",
  "reason": "Out of office"
}
```

**Expected Response:**
- Status: `200 OK`
- Approval delegated

---

#### INT-APR-006: Escalate Overdue Approval

| Field | Value |
|-------|-------|
| **ID** | INT-APR-006 |
| **Title** | Auto-escalate overdue approvals |
| **Priority** | Medium |
| **API Endpoint** | `POST /api/approvals/{id}/escalate` |

**Expected Response:**
- Status: `200 OK`
- Escalated to next level

---

#### INT-APR-007: Get Pending Approvals

| Field | Value |
|-------|-------|
| **ID** | INT-APR-007 |
| **Title** | Get user's pending approvals |
| **Priority** | High |
| **API Endpoint** | `GET /api/approvals/pending` |

**Expected Response:**
- Status: `200 OK`
- List of pending approvals

---

#### INT-APR-008: Get Approval History

| Field | Value |
|-------|-------|
| **ID** | INT-APR-008 |
| **Title** | Get approval audit trail |
| **Priority** | Medium |
| **API Endpoint** | `GET /api/approvals/{id}/history` |

**Expected Response:**
- Status: `200 OK`
- Complete approval history

---

#### INT-APR-009: Multi-Level Approval

| Field | Value |
|-------|-------|
| **ID** | INT-APR-009 |
| **Title** | Process multi-level approval chain |
| **Priority** | High |
| **API Endpoint** | `POST /api/approvals/{id}/approve` |
| **Prerequisites** | Level 1 approval needed |

**Expected Response:**
- Status: `200 OK`
- Moved to Level 2 approval

---

#### INT-APR-010: Recall Submission

| Field | Value |
|-------|-------|
| **ID** | INT-APR-010 |
| **Title** | Recall submitted approval |
| **Priority** | Medium |
| **API Endpoint** | `POST /api/approvals/{id}/recall` |

**Expected Response:**
- Status: `200 OK`
- Approval recalled, status reset

---

#### INT-APR-011: Request More Information

| Field | Value |
|-------|-------|
| **ID** | INT-APR-011 |
| **Title** | Request additional info from submitter |
| **Priority** | Medium |
| **API Endpoint** | `POST /api/approvals/{id}/request-info` |

**Request:**
```json
{
  "questions": "Please provide sales order references"
}
```

**Expected Response:**
- Status: `200 OK`
- Request sent to submitter

---

#### INT-APR-012: Respond to Info Request

| Field | Value |
|-------|-------|
| **ID** | INT-APR-012 |
| **Title** | Respond to information request |
| **Priority** | Medium |
| **API Endpoint** | `POST /api/approvals/{id}/provide-info` |

**Request:**
```json
{
  "response": "Order refs: SO-001, SO-002",
  "attachments": ["evidence.pdf"]
}
```

**Expected Response:**
- Status: `200 OK`
- Info added, approval resumed

---

#### INT-APR-013: Get Approval Statistics

| Field | Value |
|-------|-------|
| **ID** | INT-APR-013 |
| **Title** | Get approval metrics |
| **Priority** | Low |
| **API Endpoint** | `GET /api/approvals/statistics` |

**Expected Response:**
- Status: `200 OK`
- Pending, approved, rejected counts

---

#### INT-APR-014: Set Approval Rules

| Field | Value |
|-------|-------|
| **ID** | INT-APR-014 |
| **Title** | Configure approval rules |
| **Priority** | Medium |
| **API Endpoint** | `POST /api/approvals/rules` |

**Request:**
```json
{
  "amountThreshold": 50000,
  "requireLevel2": true
}
```

**Expected Response:**
- Status: `201 Created`
- Rules configured

---

#### INT-APR-015: Approval Timeout Handling

| Field | Value |
|-------|-------|
| **ID** | INT-APR-015 |
| **Title** | Test approval timeout behavior |
| **Priority** | Medium |
| **API Endpoint** | Background job |

**Expected Behavior:**
- After 3 days, reminder sent
- After 5 days, escalation triggered

---

## Database Integration Tests

### Repository Tests (INT-DB-001 to INT-DB-015)

> *"I ate my crayons!"* - Ralph Wiggum
>
> Our database operations don't eat data - they preserve it perfectly!

#### INT-DB-001: Employee Repository - Create

| Field | Value |
|-------|-------|
| **ID** | INT-DB-001 |
| **Title** | Create employee in database |
| **Priority** | High |

**Test Steps:**
1. Create employee entity
2. Save via repository
3. Verify ID generated
4. Verify all fields persisted

---

#### INT-DB-002: Employee Repository - Query

| Field | Value |
|-------|-------|
| **ID** | INT-DB-002 |
| **Title** | Query employees with specifications |
| **Priority** | High |

**Test Steps:**
1. Seed test employees
2. Query with filter specification
3. Verify correct results returned
4. Verify query efficiency

---

#### INT-DB-003: Plan Repository - Transaction

| Field | Value |
|-------|-------|
| **ID** | INT-DB-003 |
| **Title** | Plan creation with slabs in transaction |
| **Priority** | Critical |

**Test Steps:**
1. Create plan with slabs
2. Verify transaction commits
3. Simulate failure
4. Verify rollback works

---

#### INT-DB-004: Calculation Repository - Bulk Insert

| Field | Value |
|-------|-------|
| **ID** | INT-DB-004 |
| **Title** | Bulk insert calculations |
| **Priority** | High |

**Test Steps:**
1. Generate 1000 calculations
2. Bulk insert
3. Verify performance < 5 seconds
4. Verify all records inserted

---

#### INT-DB-005: Audit Repository - Event Sourcing

| Field | Value |
|-------|-------|
| **ID** | INT-DB-005 |
| **Title** | Store and replay audit events |
| **Priority** | High |

**Test Steps:**
1. Perform multiple operations
2. Verify audit events stored
3. Replay events
4. Verify state reconstruction

---

#### INT-DB-006: Concurrency - Optimistic Locking

| Field | Value |
|-------|-------|
| **ID** | INT-DB-006 |
| **Title** | Handle concurrent updates |
| **Priority** | High |

**Test Steps:**
1. Read entity twice
2. Update first copy
3. Update second copy
4. Verify concurrency exception

---

#### INT-DB-007: Migration - Up/Down

| Field | Value |
|-------|-------|
| **ID** | INT-DB-007 |
| **Title** | Verify migration up/down |
| **Priority** | Medium |

**Test Steps:**
1. Apply all migrations
2. Verify schema correct
3. Rollback migration
4. Verify schema reverted

---

#### INT-DB-008: Index Performance

| Field | Value |
|-------|-------|
| **ID** | INT-DB-008 |
| **Title** | Verify query uses indexes |
| **Priority** | Medium |

**Test Steps:**
1. Insert 100,000 records
2. Run common queries
3. Check execution plans
4. Verify index usage

---

#### INT-DB-009: Soft Delete

| Field | Value |
|-------|-------|
| **ID** | INT-DB-009 |
| **Title** | Verify soft delete filtering |
| **Priority** | High |

**Test Steps:**
1. Soft delete entity
2. Query without filter
3. Verify deleted excluded
4. Query with filter
5. Verify deleted included

---

#### INT-DB-010: Full-Text Search

| Field | Value |
|-------|-------|
| **ID** | INT-DB-010 |
| **Title** | Test full-text search |
| **Priority** | Medium |

**Test Steps:**
1. Index employee names
2. Search partial name
3. Verify results ranked

---

#### INT-DB-011: Connection Resilience

| Field | Value |
|-------|-------|
| **ID** | INT-DB-011 |
| **Title** | Test connection retry |
| **Priority** | Medium |

**Test Steps:**
1. Execute query
2. Simulate connection failure
3. Verify automatic retry
4. Verify eventual success

---

#### INT-DB-012: Stored Procedure Call

| Field | Value |
|-------|-------|
| **ID** | INT-DB-012 |
| **Title** | Call stored procedure |
| **Priority** | Medium |

**Test Steps:**
1. Call calculation SP
2. Pass parameters
3. Verify output
4. Verify performance

---

#### INT-DB-013: JSON Column Operations

| Field | Value |
|-------|-------|
| **ID** | INT-DB-013 |
| **Title** | Query JSON columns |
| **Priority** | Medium |

**Test Steps:**
1. Store JSON data
2. Query JSON path
3. Verify extraction
4. Update JSON property

---

#### INT-DB-014: Temporal Table Query

| Field | Value |
|-------|-------|
| **ID** | INT-DB-014 |
| **Title** | Query temporal history |
| **Priority** | Medium |

**Test Steps:**
1. Update entity multiple times
2. Query as of past time
3. Verify historical values

---

#### INT-DB-015: Database Backup/Restore

| Field | Value |
|-------|-------|
| **ID** | INT-DB-015 |
| **Title** | Verify backup integrity |
| **Priority** | Low |

**Test Steps:**
1. Create backup
2. Modify data
3. Restore backup
4. Verify data restored

---

## External Service Integration Tests

### ERP Integration (INT-ERP-001 to INT-ERP-005)

> *"I'm a star! I'm a great big shining star!"* - Ralph Wiggum
>
> Our ERP integration shines - connecting systems seamlessly!

#### INT-ERP-001: Fetch Sales Orders

| Field | Value |
|-------|-------|
| **ID** | INT-ERP-001 |
| **Title** | Fetch sales orders from ERP |
| **Priority** | High |

**Test Steps:**
1. Configure ERP mock
2. Request sales orders
3. Verify data mapped correctly
4. Handle pagination

---

#### INT-ERP-002: Sync Customer Data

| Field | Value |
|-------|-------|
| **ID** | INT-ERP-002 |
| **Title** | Sync customer master data |
| **Priority** | Medium |

**Test Steps:**
1. Trigger sync
2. Verify customers created
3. Verify updates applied
4. Handle duplicates

---

#### INT-ERP-003: ERP Connection Failure

| Field | Value |
|-------|-------|
| **ID** | INT-ERP-003 |
| **Title** | Handle ERP connection failure |
| **Priority** | High |

**Test Steps:**
1. Simulate ERP down
2. Attempt connection
3. Verify retry logic
4. Verify graceful degradation

---

#### INT-ERP-004: ERP Rate Limiting

| Field | Value |
|-------|-------|
| **ID** | INT-ERP-004 |
| **Title** | Handle ERP rate limits |
| **Priority** | Medium |

**Test Steps:**
1. Exceed rate limit
2. Verify backoff applied
3. Verify retry succeeds

---

#### INT-ERP-005: ERP Data Validation

| Field | Value |
|-------|-------|
| **ID** | INT-ERP-005 |
| **Title** | Validate ERP data quality |
| **Priority** | Medium |

**Test Steps:**
1. Receive invalid data
2. Verify validation errors
3. Verify error reporting
4. Continue with valid data

---

### HR System Integration (INT-HR-001 to INT-HR-005)

#### INT-HR-001: Employee Onboarding Sync

| Field | Value |
|-------|-------|
| **ID** | INT-HR-001 |
| **Title** | Sync new employees from HR |
| **Priority** | High |

**Test Steps:**
1. HR creates employee
2. DSIF receives webhook
3. Employee created in DSIF
4. Eligibility determined

---

#### INT-HR-002: Employee Termination Sync

| Field | Value |
|-------|-------|
| **ID** | INT-HR-002 |
| **Title** | Process employee termination |
| **Priority** | High |

**Test Steps:**
1. HR terminates employee
2. DSIF receives notification
3. Pending calculations prorated
4. Future plans cancelled

---

#### INT-HR-003: Department Transfer

| Field | Value |
|-------|-------|
| **ID** | INT-HR-003 |
| **Title** | Process department transfer |
| **Priority** | Medium |

**Test Steps:**
1. HR transfers employee
2. DSIF receives update
3. Plan eligibility recalculated
4. Prorations applied

---

#### INT-HR-004: Role Change

| Field | Value |
|-------|-------|
| **ID** | INT-HR-004 |
| **Title** | Process role change |
| **Priority** | Medium |

**Test Steps:**
1. HR changes role
2. DSIF receives update
3. Target amount updated
4. New plans assigned

---

#### INT-HR-005: HR System Unavailable

| Field | Value |
|-------|-------|
| **ID** | INT-HR-005 |
| **Title** | Handle HR system outage |
| **Priority** | High |

**Test Steps:**
1. Simulate HR down
2. Queue pending syncs
3. Retry when available
4. Verify data consistency

---

### Payroll Integration (INT-PAY-001 to INT-PAY-005)

#### INT-PAY-001: Submit Payout Batch

| Field | Value |
|-------|-------|
| **ID** | INT-PAY-001 |
| **Title** | Submit approved payouts to payroll |
| **Priority** | Critical |

**Test Steps:**
1. Batch approved calculations
2. Generate payout file
3. Submit to payroll
4. Verify confirmation

---

#### INT-PAY-002: Payout Confirmation

| Field | Value |
|-------|-------|
| **ID** | INT-PAY-002 |
| **Title** | Process payout confirmation |
| **Priority** | High |

**Test Steps:**
1. Payroll confirms payment
2. DSIF receives confirmation
3. Status updated to Paid
4. Records updated

---

#### INT-PAY-003: Payout Rejection

| Field | Value |
|-------|-------|
| **ID** | INT-PAY-003 |
| **Title** | Handle payout rejection |
| **Priority** | High |

**Test Steps:**
1. Payroll rejects payout
2. DSIF receives rejection
3. Status updated to Rejected
4. Alert generated

---

#### INT-PAY-004: Payout Reconciliation

| Field | Value |
|-------|-------|
| **ID** | INT-PAY-004 |
| **Title** | Reconcile payout totals |
| **Priority** | Medium |

**Test Steps:**
1. Generate reconciliation
2. Compare DSIF totals
3. Compare payroll totals
4. Report discrepancies

---

#### INT-PAY-005: Payroll System Timeout

| Field | Value |
|-------|-------|
| **ID** | INT-PAY-005 |
| **Title** | Handle payroll timeout |
| **Priority** | High |

**Test Steps:**
1. Submit batch
2. Simulate timeout
3. Verify retry logic
4. Prevent duplicate submissions

---

## Event/Message Integration Tests

### Event Publishing (INT-EVT-001 to INT-EVT-005)

> *"I found a mushroom in my pocket!"* - Ralph Wiggum
>
> Our events are more predictable - they always go where they should!

#### INT-EVT-001: Publish Calculation Created Event

| Field | Value |
|-------|-------|
| **ID** | INT-EVT-001 |
| **Title** | Publish event on calculation creation |
| **Priority** | High |

**Test Steps:**
1. Create calculation
2. Verify event published
3. Verify event payload
4. Verify message properties

---

#### INT-EVT-002: Publish Approval Completed Event

| Field | Value |
|-------|-------|
| **ID** | INT-EVT-002 |
| **Title** | Publish event on approval completion |
| **Priority** | High |

**Test Steps:**
1. Complete approval
2. Verify event published
3. Downstream receives event

---

#### INT-EVT-003: Event Retry on Failure

| Field | Value |
|-------|-------|
| **ID** | INT-EVT-003 |
| **Title** | Retry failed event publish |
| **Priority** | Medium |

**Test Steps:**
1. Simulate message broker failure
2. Verify retry attempts
3. Verify eventual success
4. Verify no duplicates

---

#### INT-EVT-004: Dead Letter Queue

| Field | Value |
|-------|-------|
| **ID** | INT-EVT-004 |
| **Title** | Route failed events to DLQ |
| **Priority** | Medium |

**Test Steps:**
1. Poison message scenario
2. Verify max retries
3. Verify moved to DLQ
4. Alert generated

---

#### INT-EVT-005: Event Ordering

| Field | Value |
|-------|-------|
| **ID** | INT-EVT-005 |
| **Title** | Maintain event order |
| **Priority** | High |

**Test Steps:**
1. Publish ordered events
2. Verify session ID
3. Verify order preserved

---

### Event Consumption (INT-EVT-006 to INT-EVT-010)

#### INT-EVT-006: Consume HR Event

| Field | Value |
|-------|-------|
| **ID** | INT-EVT-006 |
| **Title** | Process HR system event |
| **Priority** | High |

**Test Steps:**
1. Receive employee event
2. Process event
3. Update employee
4. Acknowledge message

---

#### INT-EVT-007: Idempotent Processing

| Field | Value |
|-------|-------|
| **ID** | INT-EVT-007 |
| **Title** | Handle duplicate events |
| **Priority** | High |

**Test Steps:**
1. Process event
2. Replay same event
3. Verify idempotent
4. No duplicate effects

---

#### INT-EVT-008: Event Schema Validation

| Field | Value |
|-------|-------|
| **ID** | INT-EVT-008 |
| **Title** | Validate incoming event schema |
| **Priority** | Medium |

**Test Steps:**
1. Receive invalid event
2. Verify validation error
3. Route to error handling
4. Log details

---

#### INT-EVT-009: Event Handler Error

| Field | Value |
|-------|-------|
| **ID** | INT-EVT-009 |
| **Title** | Handle processing error |
| **Priority** | Medium |

**Test Steps:**
1. Simulate handler error
2. Verify message not lost
3. Verify retry occurs
4. Verify error logged

---

#### INT-EVT-010: Batch Event Processing

| Field | Value |
|-------|-------|
| **ID** | INT-EVT-010 |
| **Title** | Process batch of events |
| **Priority** | Medium |

**Test Steps:**
1. Receive batch of events
2. Process efficiently
3. Verify all processed
4. Verify performance

---

## End-to-End Workflow Tests

### Complete Workflows (INT-E2E-001 to INT-E2E-005)

> *"When I grow up, I'm going to Bovine University!"* - Ralph Wiggum
>
> Our end-to-end tests cover the full journey - from start to graduation!

#### INT-E2E-001: New Employee Incentive Cycle

| Field | Value |
|-------|-------|
| **ID** | INT-E2E-001 |
| **Title** | Complete new employee incentive cycle |
| **Priority** | Critical |

**Workflow:**
1. HR creates new employee
2. DSIF receives notification
3. Employee assigned to plan
4. Achievement recorded
5. Calculation executed
6. Approval workflow
7. Payout processed

**Verification:**
- All steps complete successfully
- Data consistent across systems
- Audit trail complete

---

#### INT-E2E-002: Monthly Batch Processing

| Field | Value |
|-------|-------|
| **ID** | INT-E2E-002 |
| **Title** | Complete monthly batch cycle |
| **Priority** | Critical |

**Workflow:**
1. Month-end triggers batch
2. All eligible employees calculated
3. Results submitted for approval
4. Manager approves batch
5. Finance approves
6. Payout file generated
7. Payroll confirmed

**Verification:**
- All employees processed
- No calculation errors
- Totals match

---

#### INT-E2E-003: Mid-Period Employee Transfer

| Field | Value |
|-------|-------|
| **ID** | INT-E2E-003 |
| **Title** | Handle employee transfer mid-period |
| **Priority** | High |

**Workflow:**
1. Employee in Plan A
2. HR transfers employee
3. Plan A prorated
4. Plan B assigned
5. Both periods calculated
6. Combined approval

**Verification:**
- Proration accurate
- Both plans calculated
- Total incentive correct

---

#### INT-E2E-004: Calculation Adjustment Cycle

| Field | Value |
|-------|-------|
| **ID** | INT-E2E-004 |
| **Title** | Process post-approval adjustment |
| **Priority** | High |

**Workflow:**
1. Original calculation approved
2. Adjustment requested
3. Recalculation performed
4. Difference calculated
5. Adjustment approved
6. Additional payout processed

**Verification:**
- Original preserved
- Adjustment accurate
- Audit complete

---

#### INT-E2E-005: Annual Plan Rollover

| Field | Value |
|-------|-------|
| **ID** | INT-E2E-005 |
| **Title** | Annual plan expiry and renewal |
| **Priority** | High |

**Workflow:**
1. Current plan expires
2. Final calculations processed
3. New plan activated
4. Employees reassigned
5. New targets set

**Verification:**
- All final calcs complete
- Smooth transition
- No gaps in coverage

---

## Test Execution Summary

### Test Results Template

```
Integration Test Execution Report
================================
Date: ____________
Environment: ____________
Build: ____________

Category                    Total   Passed  Failed  Blocked
------------------------------------------------------------
Employee API               20      20      0       0
Incentive Plan API         15      15      0       0
Calculation API            15      15      0       0
Approval Workflow API      15      15      0       0
Database Integration       15      15      0       0
External Services          15      15      0       0
Event/Message Integration  10      10      0       0
E2E Workflows              5       5       0       0
------------------------------------------------------------
TOTAL                      120     120     0       0

Pass Rate: 100%
Execution Time: 45 minutes

Sign-off: ____________
Date: ____________
```

---

## Appendix: Test Data Requirements

### Required Test Data

| Data Set | Records | Purpose |
|----------|---------|---------|
| Test Employees | 100 | Employee API tests |
| Test Plans | 10 | Plan API tests |
| Test Calculations | 500 | Calculation tests |
| Test Approvals | 200 | Approval tests |
| Historical Data | 1000 | Performance tests |

### Data Location

All test data files are located in `/tests/data/`

---

*This document is part of the DSIF Quality Gate Framework - QG-5 (Testing Gate)*
