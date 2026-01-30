# DORISE Sales Incentive Framework
## Integration Specification

**Document ID:** DOC-TECH-003
**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Created:** January 2025
**Last Updated:** January 2025
**Author:** Claude Code
**Status:** Active

> *"Hi, Super Nintendo Chalmers!"* - Ralph Wiggum
>
> Our APIs are designed to be friendly and approachable - even when names get mixed up!

---

## Table of Contents

1. [Overview](#1-overview)
2. [Integration Architecture](#2-integration-architecture)
3. [Inbound Integrations](#3-inbound-integrations)
4. [Outbound Integrations](#4-outbound-integrations)
5. [Authentication & Security](#5-authentication--security)
6. [Data Formats & Contracts](#6-data-formats--contracts)
7. [Error Handling](#7-error-handling)
8. [Monitoring & Alerting](#8-monitoring--alerting)
9. [Testing Strategy](#9-testing-strategy)
10. [Operational Procedures](#10-operational-procedures)

---

## 1. Overview

### 1.1 Purpose

This document defines all external system integrations for DSIF, including data flows, contracts, security requirements, and operational procedures.

### 1.2 Integration Summary

| System | Direction | Method | Frequency | Purpose |
|--------|-----------|--------|-----------|---------|
| **ERP System** | Inbound | REST API | Daily | Sales metrics import |
| **HR System** | Inbound | REST API / Webhook | Real-time | Employee data sync |
| **Payroll System** | Outbound | REST API / File | Per pay period | Incentive export |
| **Azure Entra ID** | Bidirectional | OIDC/SCIM | Real-time | Authentication |
| **Email Service** | Outbound | SMTP/SendGrid | As needed | Notifications |

### 1.3 Integration Principles

| Principle | Description |
|-----------|-------------|
| **Loose Coupling** | Systems communicate via well-defined contracts |
| **Idempotency** | All operations safe to retry |
| **Resilience** | Graceful handling of failures |
| **Observability** | Full tracing and logging |
| **Security** | Zero trust, encrypted communications |

---

## 2. Integration Architecture

### 2.1 Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        INTEGRATION ARCHITECTURE                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  EXTERNAL SYSTEMS                     DSIF                    EXTERNAL      │
│  (INBOUND)                                                    (OUTBOUND)    │
│                                                                              │
│  ┌─────────────┐                ┌─────────────┐               ┌──────────┐  │
│  │ ERP System  │                │             │               │ Payroll  │  │
│  │             │═══════════════▶│             │═══════════════▶│ System   │  │
│  │ Sales Data  │   REST API     │   DSIF      │   REST API    │          │  │
│  └─────────────┘   (Pull/Push)  │   Core      │   (Push)      └──────────┘  │
│                                 │             │                             │
│  ┌─────────────┐                │   ┌─────┐   │               ┌──────────┐  │
│  │ HR System   │                │   │ API │   │               │  Email   │  │
│  │             │═══════════════▶│   │Gate │   │═══════════════▶│ Service  │  │
│  │ Employee    │   Webhook      │   │way  │   │   SMTP/API    │          │  │
│  │ Changes     │                │   └─────┘   │               └──────────┘  │
│  └─────────────┘                │             │                             │
│                                 │   ┌─────┐   │                             │
│  ┌─────────────┐                │   │Azure│   │                             │
│  │ Azure AD    │◀══════════════▶│   │Func │   │                             │
│  │ (Entra ID)  │   OIDC/SCIM    │   │tions│   │                             │
│  │             │                │   └─────┘   │                             │
│  └─────────────┘                │             │                             │
│                                 └─────────────┘                             │
│                                                                              │
│  ═════════════ Synchronous (REST API)                                       │
│  ─────────────▶ Asynchronous (Webhook/Queue)                                │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Integration Patterns

| Pattern | Use Case | Implementation |
|---------|----------|----------------|
| **Request/Response** | Real-time queries | REST API with timeout |
| **Polling** | Scheduled data import | Azure Function timer trigger |
| **Webhook** | Event notifications | HTTP POST with retry |
| **File Transfer** | Batch export | Blob storage + SFTP |
| **Pub/Sub** | Internal events | Azure Service Bus |

### 2.3 Message Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         MESSAGE FLOW PATTERNS                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  PATTERN 1: Synchronous Import (ERP)                                        │
│  ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐                   │
│  │  Timer  │───▶│ Function│───▶│  ERP    │───▶│ Staging │                   │
│  │ Trigger │    │ (Pull)  │    │  API    │    │  Table  │                   │
│  └─────────┘    └─────────┘    └─────────┘    └─────────┘                   │
│                                                                              │
│  PATTERN 2: Webhook (HR System)                                             │
│  ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐                   │
│  │   HR    │───▶│ Webhook │───▶│ Service │───▶│ Employee│                   │
│  │ System  │    │Endpoint │    │  Bus    │    │  Table  │                   │
│  └─────────┘    └─────────┘    └─────────┘    └─────────┘                   │
│                                                                              │
│  PATTERN 3: Scheduled Export (Payroll)                                      │
│  ┌─────────┐    ┌─────────┐    ┌─────────┐    ┌─────────┐                   │
│  │  Timer  │───▶│ Function│───▶│  Blob   │───▶│ Payroll │                   │
│  │ Trigger │    │(Export) │    │ Storage │    │  SFTP   │                   │
│  └─────────┘    └─────────┘    └─────────┘    └─────────┘                   │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 3. Inbound Integrations

### 3.1 ERP System - Sales Data Import

#### 3.1.1 Overview

| Attribute | Value |
|-----------|-------|
| **System** | Enterprise ERP (SAP/Oracle/Custom) |
| **Direction** | Inbound |
| **Method** | REST API (Pull) |
| **Frequency** | Daily at 1:00 AM UTC |
| **Data Volume** | ~10,000 records/day |
| **SLA** | < 30 minutes processing |

#### 3.1.2 Data Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       ERP SALES DATA IMPORT FLOW                             │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ STEP 1: Trigger                                                        │  │
│  │ Azure Function timer trigger fires at 1:00 AM UTC                      │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                      │                                       │
│                                      ▼                                       │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ STEP 2: Authentication                                                 │  │
│  │ Obtain OAuth token from ERP identity provider                          │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                      │                                       │
│                                      ▼                                       │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ STEP 3: Data Request                                                   │  │
│  │ GET /api/sales?date={yesterday}&pageSize=1000                          │  │
│  │ Paginate through all results                                           │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                      │                                       │
│                                      ▼                                       │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ STEP 4: Validation                                                     │  │
│  │ • Schema validation                                                    │  │
│  │ • Business rule validation                                             │  │
│  │ • Duplicate detection                                                  │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                      │                                       │
│                                      ▼                                       │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ STEP 5: Staging                                                        │  │
│  │ Insert into StagingSalesData table with batch ID                       │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                      │                                       │
│                                      ▼                                       │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ STEP 6: Processing                                                     │  │
│  │ • Map employee IDs                                                     │  │
│  │ • Aggregate metrics by employee/period                                 │  │
│  │ • Move to production SalesMetrics table                                │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                      │                                       │
│                                      ▼                                       │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ STEP 7: Notification                                                   │  │
│  │ • Update import log                                                    │  │
│  │ • Send success/failure notification                                    │  │
│  │ • Trigger calculation if configured                                    │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

#### 3.1.3 API Contract

**Request:**
```http
GET /api/v1/sales
Host: erp-api.company.com
Authorization: Bearer {access_token}
Content-Type: application/json

Query Parameters:
  date: 2025-01-15 (required)
  storeId: STORE001 (optional)
  page: 1 (default)
  pageSize: 1000 (max 5000)
```

**Response:**
```json
{
  "data": [
    {
      "transactionId": "TXN-2025-001234",
      "transactionDate": "2025-01-15T14:30:00Z",
      "employeeId": "EMP001",
      "storeId": "STORE001",
      "salesAmount": 1500.00,
      "unitsSold": 5,
      "productCategory": "Electronics",
      "productSku": "SKU-12345"
    }
  ],
  "pagination": {
    "page": 1,
    "pageSize": 1000,
    "totalRecords": 8500,
    "totalPages": 9
  },
  "metadata": {
    "extractedAt": "2025-01-16T01:00:00Z",
    "dataAsOf": "2025-01-15T23:59:59Z"
  }
}
```

#### 3.1.4 Implementation

```csharp
public class ErpSalesImportFunction
{
    private readonly IErpClient _erpClient;
    private readonly ISalesDataRepository _repository;
    private readonly ILogger<ErpSalesImportFunction> _logger;

    [Function("ImportErpSalesData")]
    public async Task Run(
        [TimerTrigger("0 0 1 * * *")] TimerInfo timer,
        FunctionContext context)
    {
        var importDate = DateTime.UtcNow.AddDays(-1).Date;
        var batchId = Guid.NewGuid();

        _logger.LogInformation(
            "Starting ERP sales import for {Date}, BatchId: {BatchId}",
            importDate, batchId);

        try
        {
            var totalRecords = 0;
            var page = 1;

            do
            {
                var response = await _erpClient.GetSalesDataAsync(
                    importDate, page, pageSize: 1000, context.CancellationToken);

                // Validate and transform
                var validRecords = response.Data
                    .Where(ValidateSalesRecord)
                    .Select(r => MapToStagingRecord(r, batchId))
                    .ToList();

                // Insert to staging
                await _repository.BulkInsertStagingAsync(validRecords, context.CancellationToken);

                totalRecords += validRecords.Count;
                page++;

            } while (page <= response.Pagination.TotalPages);

            // Process staging to production
            await _repository.ProcessStagingBatchAsync(batchId, context.CancellationToken);

            _logger.LogInformation(
                "ERP sales import completed. Records processed: {Count}",
                totalRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "ERP sales import failed for {Date}", importDate);
            throw;
        }
    }
}
```

### 3.2 HR System - Employee Sync

#### 3.2.1 Overview

| Attribute | Value |
|-----------|-------|
| **System** | HR Management System (Workday/SAP HCM) |
| **Direction** | Inbound |
| **Method** | Webhook (Push) + REST API (Pull) |
| **Frequency** | Real-time (webhook) + Daily full sync |
| **Data Volume** | ~100 events/day, 10,000 employees full sync |

#### 3.2.2 Webhook Events

| Event | Trigger | Action |
|-------|---------|--------|
| `employee.created` | New hire | Create employee record |
| `employee.updated` | Profile change | Update employee record |
| `employee.terminated` | Termination | Mark employee terminated |
| `assignment.created` | New assignment | Create assignment record |
| `assignment.updated` | Role/store change | Update assignment |
| `assignment.ended` | Assignment end | Close assignment |

#### 3.2.3 Webhook Contract

**Webhook Request:**
```http
POST /api/v1/webhooks/hr
Host: api.dorise-incentive.com
Authorization: Bearer {webhook_secret}
Content-Type: application/json
X-Webhook-Signature: sha256=abc123...
X-Event-Type: employee.updated
X-Delivery-Id: d1e2f3g4-5678-90ab-cdef

{
  "eventId": "evt-2025-001234",
  "eventType": "employee.updated",
  "timestamp": "2025-01-15T14:30:00Z",
  "data": {
    "employeeId": "EMP001",
    "changes": [
      {
        "field": "email",
        "oldValue": "john.old@company.com",
        "newValue": "john.new@company.com"
      }
    ],
    "effectiveDate": "2025-01-15",
    "modifiedBy": "HR_ADMIN"
  }
}
```

**Webhook Response:**
```json
{
  "received": true,
  "processedAt": "2025-01-15T14:30:01Z",
  "correlationId": "d1e2f3g4-5678-90ab-cdef"
}
```

#### 3.2.4 Webhook Handler Implementation

```csharp
[ApiController]
[Route("api/v1/webhooks")]
public class WebhookController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IWebhookValidator _validator;
    private readonly ILogger<WebhookController> _logger;

    [HttpPost("hr")]
    public async Task<IActionResult> HandleHrWebhook(
        [FromBody] HrWebhookPayload payload,
        [FromHeader(Name = "X-Webhook-Signature")] string signature,
        [FromHeader(Name = "X-Event-Type")] string eventType,
        [FromHeader(Name = "X-Delivery-Id")] string deliveryId)
    {
        // Validate signature
        if (!_validator.ValidateSignature(Request.Body, signature))
        {
            _logger.LogWarning("Invalid webhook signature for delivery {DeliveryId}", deliveryId);
            return Unauthorized();
        }

        // Check for duplicate delivery (idempotency)
        if (await _validator.IsDuplicateDeliveryAsync(deliveryId))
        {
            _logger.LogInformation("Duplicate webhook delivery {DeliveryId}", deliveryId);
            return Ok(new { received = true, duplicate = true });
        }

        try
        {
            // Route to appropriate handler
            IRequest command = eventType switch
            {
                "employee.created" => new CreateEmployeeFromHrCommand(payload.Data),
                "employee.updated" => new UpdateEmployeeFromHrCommand(payload.Data),
                "employee.terminated" => new TerminateEmployeeCommand(payload.Data),
                "assignment.created" => new CreateAssignmentFromHrCommand(payload.Data),
                "assignment.updated" => new UpdateAssignmentFromHrCommand(payload.Data),
                "assignment.ended" => new EndAssignmentCommand(payload.Data),
                _ => throw new NotSupportedException($"Unknown event type: {eventType}")
            };

            await _mediator.Send(command);

            // Mark delivery as processed
            await _validator.MarkDeliveryProcessedAsync(deliveryId);

            return Ok(new
            {
                received = true,
                processedAt = DateTime.UtcNow,
                correlationId = deliveryId
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process HR webhook {EventType}", eventType);
            return StatusCode(500, new { error = "Processing failed", retry = true });
        }
    }
}
```

### 3.3 Azure Entra ID - Identity

#### 3.3.1 Overview

| Attribute | Value |
|-----------|-------|
| **System** | Azure Entra ID (Azure AD) |
| **Direction** | Bidirectional |
| **Method** | OIDC (Auth) + Microsoft Graph API |
| **Frequency** | Real-time |

#### 3.3.2 Authentication Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    AZURE ENTRA ID AUTHENTICATION FLOW                        │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌────────┐     ┌────────┐     ┌────────┐     ┌────────┐     ┌────────┐    │
│  │  User  │────▶│  DSIF  │────▶│ Entra  │────▶│  DSIF  │────▶│  User  │    │
│  │Browser │     │  Web   │     │   ID   │     │  API   │     │  Home  │    │
│  └────────┘     └────────┘     └────────┘     └────────┘     └────────┘    │
│      │              │              │              │              │           │
│      │  1. Access   │              │              │              │           │
│      │─────────────▶│              │              │              │           │
│      │              │  2. Redirect │              │              │           │
│      │              │─────────────▶│              │              │           │
│      │              │              │  3. Login    │              │           │
│      │◀─────────────┼──────────────│              │              │           │
│      │  4. Auth     │              │              │              │           │
│      │─────────────▶│              │              │              │           │
│      │              │  5. Exchange │              │              │           │
│      │              │─────────────▶│              │              │           │
│      │              │  6. Tokens   │              │              │           │
│      │              │◀─────────────│              │              │           │
│      │              │              │  7. API Call │              │           │
│      │              │              │─────────────▶│              │           │
│      │              │              │  8. Validate │              │           │
│      │              │              │◀─────────────│              │           │
│      │              │              │              │  9. Response │           │
│      │◀─────────────┼──────────────┼──────────────┼──────────────│           │
│      │              │              │              │              │           │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

#### 3.3.3 Configuration

```csharp
// Program.cs
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAuthenticatedUser",
        policy => policy.RequireAuthenticatedUser());

    options.AddPolicy("RequireAdmin",
        policy => policy.RequireRole("DSIF.Admin"));

    options.AddPolicy("RequireApprover",
        policy => policy.RequireRole("DSIF.Admin", "DSIF.Approver"));
});

// appsettings.json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "TenantId": "{tenant-id}",
    "ClientId": "{client-id}",
    "Audience": "api://dsif-api",
    "Scopes": "Employees.Read Employees.Write Calculations.Read Approvals.Write"
  }
}
```

> *"My cat's breath smells like cat food."* - Ralph Wiggum
>
> Our integrations are straightforward - data flows exactly where you'd expect!

---

## 4. Outbound Integrations

### 4.1 Payroll System - Incentive Export

#### 4.1.1 Overview

| Attribute | Value |
|-----------|-------|
| **System** | Payroll System (ADP/Ceridian/Custom) |
| **Direction** | Outbound |
| **Method** | REST API + File Export (SFTP) |
| **Frequency** | Per pay period (bi-weekly/monthly) |
| **Data Volume** | ~10,000 records per export |

#### 4.1.2 Export Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                     PAYROLL EXPORT PROCESS FLOW                              │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ STEP 1: Trigger                                                        │  │
│  │ Manual trigger by Finance or scheduled after approval deadline         │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                      │                                       │
│                                      ▼                                       │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ STEP 2: Validation                                                     │  │
│  │ • All calculations for period approved                                 │  │
│  │ • No pending amendments                                                │  │
│  │ • Export not already completed for period                              │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                      │                                       │
│                                      ▼                                       │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ STEP 3: Data Extraction                                                │  │
│  │ • Get approved calculations for period                                 │  │
│  │ • Aggregate by employee (if multiple calculations)                     │  │
│  │ • Apply payroll-specific transformations                               │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                      │                                       │
│                                      ▼                                       │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ STEP 4: Format Generation                                              │  │
│  │ • Generate JSON payload for API                                        │  │
│  │ • Generate CSV file for SFTP backup                                    │  │
│  │ • Calculate control totals                                             │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                      │                                       │
│                                      ▼                                       │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ STEP 5: Transmission                                                   │  │
│  │ • POST to Payroll API                                                  │  │
│  │ • Upload CSV to SFTP (backup)                                          │  │
│  │ • Store copy in Blob Storage (archive)                                 │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                      │                                       │
│                                      ▼                                       │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ STEP 6: Confirmation                                                   │  │
│  │ • Receive acknowledgment from Payroll                                  │  │
│  │ • Update calculation status to "Exported"                              │  │
│  │ • Create export audit record                                           │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                      │                                       │
│                                      ▼                                       │
│  ┌───────────────────────────────────────────────────────────────────────┐  │
│  │ STEP 7: Notification                                                   │  │
│  │ • Notify Finance team of successful export                             │  │
│  │ • Send summary report with control totals                              │  │
│  └───────────────────────────────────────────────────────────────────────┘  │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

#### 4.1.3 API Contract

**Request:**
```http
POST /api/v1/incentives/batch
Host: payroll-api.company.com
Authorization: Bearer {access_token}
Content-Type: application/json
X-Idempotency-Key: export-2025-01-batch-001

{
  "batchId": "exp-2025-01-001",
  "payPeriod": "2025-01",
  "exportDate": "2025-02-01T10:00:00Z",
  "currency": "USD",
  "controlTotals": {
    "recordCount": 8500,
    "totalAmount": 2150000.00
  },
  "records": [
    {
      "employeeId": "EMP001",
      "employeeNumber": "100001",
      "firstName": "John",
      "lastName": "Doe",
      "incentiveAmount": 2550.00,
      "earningsCode": "INCENT",
      "calculationIds": ["calc-001", "calc-002"],
      "effectiveDate": "2025-02-15"
    }
  ]
}
```

**Response:**
```json
{
  "batchId": "exp-2025-01-001",
  "status": "accepted",
  "payrollBatchId": "PAY-2025-001234",
  "receivedAt": "2025-02-01T10:00:05Z",
  "recordsAccepted": 8500,
  "recordsRejected": 0,
  "rejectedRecords": [],
  "controlTotals": {
    "recordCount": 8500,
    "totalAmount": 2150000.00
  }
}
```

#### 4.1.4 CSV File Format

```csv
EmployeeNumber,FirstName,LastName,IncentiveAmount,EarningsCode,PayPeriod,CalculationIds,ExportDate
100001,John,Doe,2550.00,INCENT,2025-01,"calc-001,calc-002",2025-02-01
100002,Jane,Smith,1875.50,INCENT,2025-01,"calc-003",2025-02-01
100003,Bob,Johnson,3200.00,INCENT,2025-01,"calc-004,calc-005",2025-02-01
```

#### 4.1.5 Implementation

```csharp
public class PayrollExportService : IPayrollExportService
{
    private readonly ICalculationRepository _calculationRepository;
    private readonly IPayrollClient _payrollClient;
    private readonly IBlobStorageService _blobStorage;
    private readonly ISftpClient _sftpClient;
    private readonly ILogger<PayrollExportService> _logger;

    public async Task<ExportResult> ExportToPayrollAsync(
        string period,
        CancellationToken cancellationToken)
    {
        var exportId = $"exp-{period}-{Guid.NewGuid():N}";

        _logger.LogInformation(
            "Starting payroll export {ExportId} for period {Period}",
            exportId, period);

        // Get approved calculations
        var calculations = await _calculationRepository
            .GetApprovedByPeriodAsync(period, cancellationToken);

        if (!calculations.Any())
        {
            throw new DomainException($"No approved calculations found for period {period}.");
        }

        // Check for pending items
        var pendingCount = await _calculationRepository
            .CountPendingByPeriodAsync(period, cancellationToken);

        if (pendingCount > 0)
        {
            throw new DomainException(
                $"Cannot export: {pendingCount} calculations pending approval.");
        }

        // Aggregate by employee
        var exportRecords = calculations
            .GroupBy(c => c.EmployeeId)
            .Select(g => new PayrollExportRecord
            {
                EmployeeId = g.Key,
                EmployeeNumber = g.First().Snapshot.EmployeeNumber,
                FirstName = g.First().Snapshot.FirstName,
                LastName = g.First().Snapshot.LastName,
                IncentiveAmount = g.Sum(c => c.NetAmount),
                CalculationIds = g.Select(c => c.Id.ToString()).ToList()
            })
            .ToList();

        // Build payload
        var payload = new PayrollBatchPayload
        {
            BatchId = exportId,
            PayPeriod = period,
            ExportDate = DateTime.UtcNow,
            Currency = "USD",
            ControlTotals = new ControlTotals
            {
                RecordCount = exportRecords.Count,
                TotalAmount = exportRecords.Sum(r => r.IncentiveAmount)
            },
            Records = exportRecords
        };

        // Send to Payroll API
        var apiResult = await _payrollClient.SendBatchAsync(payload, cancellationToken);

        // Archive to Blob Storage
        var csvContent = GenerateCsv(exportRecords);
        await _blobStorage.UploadAsync(
            $"exports/{period}/{exportId}.csv",
            csvContent,
            cancellationToken);

        // Upload to SFTP (backup)
        await _sftpClient.UploadAsync(
            $"/incoming/dsif/{exportId}.csv",
            csvContent,
            cancellationToken);

        // Update calculation status
        foreach (var calc in calculations)
        {
            calc.MarkExported(exportId, DateTime.UtcNow);
        }
        await _calculationRepository.UpdateRangeAsync(calculations, cancellationToken);

        _logger.LogInformation(
            "Payroll export {ExportId} completed. Records: {Count}, Amount: {Amount:C}",
            exportId, exportRecords.Count, payload.ControlTotals.TotalAmount);

        return new ExportResult
        {
            ExportId = exportId,
            PayrollBatchId = apiResult.PayrollBatchId,
            RecordCount = exportRecords.Count,
            TotalAmount = payload.ControlTotals.TotalAmount,
            Status = ExportStatus.Completed
        };
    }
}
```

### 4.2 Email Notifications

#### 4.2.1 Overview

| Attribute | Value |
|-----------|-------|
| **System** | SendGrid / Azure Communication Services |
| **Direction** | Outbound |
| **Method** | REST API |
| **Frequency** | Event-driven |

#### 4.2.2 Notification Types

| Event | Recipients | Template |
|-------|------------|----------|
| `calculation.completed` | Employee | calculation-summary |
| `approval.required` | Approvers | approval-request |
| `approval.approved` | Employee, Submitter | approval-confirmation |
| `approval.rejected` | Employee, Submitter | rejection-notice |
| `export.completed` | Finance Team | export-summary |
| `system.alert` | Admins | system-alert |

#### 4.2.3 Implementation

```csharp
public class EmailNotificationService : INotificationService
{
    private readonly ISendGridClient _sendGridClient;
    private readonly ITemplateRenderer _templateRenderer;
    private readonly ILogger<EmailNotificationService> _logger;

    public async Task SendCalculationCompletedNotificationAsync(
        EmployeeId employeeId,
        string period,
        Money amount,
        CancellationToken cancellationToken)
    {
        var employee = await _employeeRepository.GetByIdAsync(employeeId, cancellationToken);

        var templateData = new
        {
            EmployeeName = employee.FullName,
            Period = FormatPeriod(period),
            Amount = amount.ToString("C"),
            DashboardUrl = $"{_config.BaseUrl}/calculations/{period}"
        };

        var htmlContent = await _templateRenderer.RenderAsync(
            "calculation-summary", templateData);

        var message = new SendGridMessage
        {
            From = new EmailAddress("noreply@dorise.com", "DSIF Notifications"),
            Subject = $"Your Incentive Calculation for {FormatPeriod(period)}",
            HtmlContent = htmlContent
        };
        message.AddTo(employee.Email, employee.FullName);

        var response = await _sendGridClient.SendEmailAsync(message, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning(
                "Failed to send email to {Email}. Status: {Status}",
                employee.Email, response.StatusCode);
        }
    }
}
```

---

## 5. Authentication & Security

### 5.1 Service-to-Service Authentication

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    SERVICE AUTHENTICATION PATTERNS                           │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  PATTERN 1: OAuth 2.0 Client Credentials (API to API)                       │
│  ┌────────┐    ┌────────┐    ┌────────┐    ┌────────┐                       │
│  │  DSIF  │───▶│ Entra  │───▶│ Token  │───▶│ Target │                       │
│  │  API   │    │   ID   │    │        │    │  API   │                       │
│  └────────┘    └────────┘    └────────┘    └────────┘                       │
│      │                           │              │                            │
│      │ 1. Request token          │              │                            │
│      │   (client_id + secret)    │              │                            │
│      │──────────────────────────▶│              │                            │
│      │                           │ 2. Return    │                            │
│      │◀──────────────────────────│    token     │                            │
│      │ 3. API call               │              │                            │
│      │   (Bearer token)          │              │                            │
│      │──────────────────────────────────────────▶│                            │
│      │                           │              │ 4. Validate               │
│      │                           │◀─────────────│    token                  │
│      │ 5. Response               │              │                            │
│      │◀──────────────────────────────────────────│                            │
│                                                                              │
│  PATTERN 2: Managed Identity (Azure Services)                               │
│  ┌────────┐    ┌────────┐    ┌────────┐                                     │
│  │ Azure  │───▶│ Azure  │───▶│ Azure  │                                     │
│  │Function│    │  IMDS  │    │  SQL   │                                     │
│  └────────┘    └────────┘    └────────┘                                     │
│      │              │              │                                         │
│      │ 1. Request   │              │                                         │
│      │    token     │              │                                         │
│      │─────────────▶│              │                                         │
│      │ 2. Token     │              │                                         │
│      │◀─────────────│              │                                         │
│      │ 3. Connect (token)         │                                         │
│      │────────────────────────────▶│                                         │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5.2 API Security Headers

```csharp
public class SecurityHeadersMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Security headers
        context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Add("X-Frame-Options", "DENY");
        context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
        context.Response.Headers.Add("Content-Security-Policy",
            "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'");

        // CORS handling
        if (context.Request.Method == "OPTIONS")
        {
            context.Response.StatusCode = 204;
            return;
        }

        await next(context);
    }
}
```

### 5.3 Webhook Signature Validation

```csharp
public class WebhookSignatureValidator : IWebhookValidator
{
    private readonly byte[] _secretKey;

    public bool ValidateSignature(Stream requestBody, string signature)
    {
        if (string.IsNullOrEmpty(signature))
            return false;

        // Parse signature (format: sha256=<hex>)
        var parts = signature.Split('=');
        if (parts.Length != 2 || parts[0] != "sha256")
            return false;

        var expectedHash = parts[1];

        // Compute hash
        using var hmac = new HMACSHA256(_secretKey);
        using var reader = new StreamReader(requestBody, leaveOpen: true);
        requestBody.Position = 0;
        var body = reader.ReadToEnd();
        requestBody.Position = 0;

        var computedHash = Convert.ToHexString(
            hmac.ComputeHash(Encoding.UTF8.GetBytes(body))).ToLowerInvariant();

        // Constant-time comparison
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expectedHash),
            Encoding.UTF8.GetBytes(computedHash));
    }
}
```

---

## 6. Data Formats & Contracts

### 6.1 Standard Request Format

```json
{
  "requestId": "req-12345",
  "timestamp": "2025-01-15T10:30:00Z",
  "version": "1.0",
  "data": {
    // Request-specific data
  }
}
```

### 6.2 Standard Response Format

```json
{
  "requestId": "req-12345",
  "timestamp": "2025-01-15T10:30:01Z",
  "status": "success",
  "data": {
    // Response-specific data
  },
  "metadata": {
    "processingTime": "150ms",
    "serverVersion": "1.2.3"
  }
}
```

### 6.3 Error Response Format

```json
{
  "requestId": "req-12345",
  "timestamp": "2025-01-15T10:30:01Z",
  "status": "error",
  "error": {
    "code": "VALIDATION_ERROR",
    "message": "One or more validation errors occurred.",
    "details": [
      {
        "field": "employeeId",
        "message": "Employee ID is required."
      }
    ]
  }
}
```

### 6.4 Data Type Mappings

| DSIF Type | JSON Type | Format/Example |
|-----------|-----------|----------------|
| `Guid` | string | `"3fa85f64-5717-4562-b3fc-2c963f66afa6"` |
| `DateTime` | string | ISO 8601: `"2025-01-15T10:30:00Z"` |
| `DateOnly` | string | `"2025-01-15"` |
| `decimal` | number | `1234.56` (2 decimal places for money) |
| `Money` | object | `{"amount": 1234.56, "currency": "USD"}` |
| `enum` | string | `"Active"`, `"Pending"` |

---

## 7. Error Handling

### 7.1 Error Categories

| Category | HTTP Status | Retry | Example |
|----------|-------------|-------|---------|
| **Client Error** | 400-499 | No | Validation failure |
| **Authentication** | 401, 403 | No (refresh token) | Invalid/expired token |
| **Not Found** | 404 | No | Resource doesn't exist |
| **Rate Limited** | 429 | Yes (with backoff) | Too many requests |
| **Server Error** | 500-599 | Yes | Internal error |
| **Timeout** | 504 | Yes | Gateway timeout |

### 7.2 Retry Strategy

```csharp
public class RetryPolicyFactory
{
    public static IAsyncPolicy<HttpResponseMessage> CreateRetryPolicy()
    {
        return Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r =>
                r.StatusCode == HttpStatusCode.TooManyRequests ||
                r.StatusCode >= HttpStatusCode.InternalServerError)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: (retryAttempt, response, context) =>
                {
                    // Check for Retry-After header
                    if (response.Result?.Headers.RetryAfter?.Delta != null)
                    {
                        return response.Result.Headers.RetryAfter.Delta.Value;
                    }

                    // Exponential backoff: 2s, 4s, 8s
                    return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                },
                onRetryAsync: async (outcome, timespan, retryAttempt, context) =>
                {
                    Log.Warning(
                        "Retry {RetryAttempt} after {Delay}s. Status: {Status}",
                        retryAttempt, timespan.TotalSeconds, outcome.Result?.StatusCode);
                });
    }

    public static IAsyncPolicy<HttpResponseMessage> CreateCircuitBreakerPolicy()
    {
        return Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => !r.IsSuccessStatusCode)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (outcome, breakDelay) =>
                {
                    Log.Warning("Circuit breaker opened for {Delay}s", breakDelay.TotalSeconds);
                },
                onReset: () =>
                {
                    Log.Information("Circuit breaker reset");
                });
    }
}
```

### 7.3 Dead Letter Handling

```csharp
public class DeadLetterProcessor
{
    [Function("ProcessDeadLetters")]
    public async Task ProcessDeadLetter(
        [ServiceBusTrigger("dsif-queue/$deadletterqueue")] ServiceBusReceivedMessage message,
        FunctionContext context)
    {
        var deadLetterReason = message.DeadLetterReason;
        var deadLetterDescription = message.DeadLetterErrorDescription;

        _logger.LogWarning(
            "Processing dead letter. Reason: {Reason}, Description: {Description}",
            deadLetterReason, deadLetterDescription);

        // Store for manual review
        await _deadLetterRepository.SaveAsync(new DeadLetterRecord
        {
            MessageId = message.MessageId,
            Body = message.Body.ToString(),
            Reason = deadLetterReason,
            Description = deadLetterDescription,
            OriginalEnqueueTime = message.EnqueuedTime,
            ReceivedAt = DateTime.UtcNow
        }, context.CancellationToken);

        // Alert operations team
        await _alertService.SendDeadLetterAlertAsync(message.MessageId);
    }
}
```

---

## 8. Monitoring & Alerting

### 8.1 Integration Health Metrics

| Metric | Description | Alert Threshold |
|--------|-------------|-----------------|
| `integration.latency` | Response time | > 5 seconds |
| `integration.error_rate` | Error percentage | > 5% |
| `integration.throughput` | Records/minute | < expected |
| `integration.queue_depth` | Pending messages | > 1000 |
| `integration.circuit_open` | Circuit breaker state | Open |

### 8.2 Application Insights Integration

```csharp
public class IntegrationTelemetryMiddleware
{
    private readonly TelemetryClient _telemetry;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var stopwatch = Stopwatch.StartNew();
        var integrationName = DetermineIntegrationName(context.Request.Path);

        try
        {
            await next(context);

            stopwatch.Stop();

            _telemetry.TrackDependency(
                "HTTP",
                integrationName,
                context.Request.Path,
                DateTimeOffset.UtcNow.AddMilliseconds(-stopwatch.ElapsedMilliseconds),
                stopwatch.Elapsed,
                context.Response.StatusCode < 400);

            _telemetry.GetMetric("integration.latency", "integration")
                .TrackValue(stopwatch.ElapsedMilliseconds, integrationName);
        }
        catch (Exception ex)
        {
            _telemetry.TrackException(ex, new Dictionary<string, string>
            {
                ["Integration"] = integrationName,
                ["Path"] = context.Request.Path
            });

            _telemetry.GetMetric("integration.errors", "integration")
                .TrackValue(1, integrationName);

            throw;
        }
    }
}
```

### 8.3 Alert Configuration

```json
{
  "alerts": [
    {
      "name": "ERP Import Failure",
      "condition": "integration.error_rate{integration='erp'} > 5%",
      "severity": "High",
      "notification": ["ops-team@company.com", "pagerduty"]
    },
    {
      "name": "Payroll Export Delayed",
      "condition": "time_since_last_export > 25h",
      "severity": "Critical",
      "notification": ["finance@company.com", "ops-team@company.com"]
    },
    {
      "name": "HR Webhook Failures",
      "condition": "webhook.failures{source='hr'} > 10 in 1h",
      "severity": "Medium",
      "notification": ["ops-team@company.com"]
    }
  ]
}
```

---

## 9. Testing Strategy

### 9.1 Integration Test Types

| Type | Purpose | Tools |
|------|---------|-------|
| **Contract Tests** | Verify API contracts | Pact |
| **Component Tests** | Test integration logic | xUnit + WireMock |
| **End-to-End Tests** | Full flow verification | Postman/Newman |
| **Load Tests** | Performance validation | k6, Artillery |
| **Chaos Tests** | Resilience validation | Chaos Monkey |

### 9.2 Mock Server Setup

```csharp
public class ErpClientTests : IClassFixture<WireMockFixture>
{
    private readonly WireMockServer _mockServer;
    private readonly IErpClient _client;

    public ErpClientTests(WireMockFixture fixture)
    {
        _mockServer = fixture.Server;
        _client = new ErpClient(new HttpClient
        {
            BaseAddress = new Uri(_mockServer.Url)
        });
    }

    [Fact]
    public async Task GetSalesData_ReturnsData_WhenApiSucceeds()
    {
        // Arrange
        _mockServer
            .Given(Request.Create()
                .WithPath("/api/v1/sales")
                .WithParam("date", "2025-01-15")
                .UsingGet())
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithHeader("Content-Type", "application/json")
                .WithBody(@"{
                    ""data"": [{
                        ""transactionId"": ""TXN-001"",
                        ""salesAmount"": 1500.00
                    }],
                    ""pagination"": {
                        ""page"": 1,
                        ""totalPages"": 1
                    }
                }"));

        // Act
        var result = await _client.GetSalesDataAsync(
            new DateTime(2025, 1, 15), 1, 1000, CancellationToken.None);

        // Assert
        result.Data.Should().HaveCount(1);
        result.Data.First().SalesAmount.Should().Be(1500m);
    }

    [Fact]
    public async Task GetSalesData_RetriesOnTransientError()
    {
        // Arrange - First two calls fail, third succeeds
        _mockServer
            .Given(Request.Create()
                .WithPath("/api/v1/sales")
                .UsingGet())
            .InScenario("retry-test")
            .WillSetStateTo("first-failure")
            .RespondWith(Response.Create()
                .WithStatusCode(503));

        _mockServer
            .Given(Request.Create()
                .WithPath("/api/v1/sales")
                .UsingGet())
            .InScenario("retry-test")
            .WhenStateIs("first-failure")
            .WillSetStateTo("second-failure")
            .RespondWith(Response.Create()
                .WithStatusCode(503));

        _mockServer
            .Given(Request.Create()
                .WithPath("/api/v1/sales")
                .UsingGet())
            .InScenario("retry-test")
            .WhenStateIs("second-failure")
            .RespondWith(Response.Create()
                .WithStatusCode(200)
                .WithBody(@"{ ""data"": [], ""pagination"": {} }"));

        // Act
        var result = await _client.GetSalesDataAsync(
            DateTime.Today, 1, 1000, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockServer.LogEntries.Should().HaveCount(3);
    }
}
```

---

## 10. Operational Procedures

### 10.1 Integration Runbook

#### ERP Import Failure

```markdown
## Symptoms
- No new sales data for current date
- Import job shows "Failed" status
- Alert: "ERP Import Failure"

## Investigation Steps
1. Check Azure Function logs for ImportErpSalesData
2. Verify ERP API health: GET /api/health
3. Check authentication: Validate token refresh
4. Review error details in staging table

## Resolution
- If auth failure: Rotate client secret
- If API down: Wait for ERP team, document outage
- If data issue: Contact ERP team for data fix
- If transient: Manually trigger re-run

## Manual Re-run
```powershell
az functionapp function invoke \
  --name func-dorise-prod \
  --function-name ImportErpSalesData \
  --data '{"date": "2025-01-15"}'
```
```

#### Payroll Export Issue

```markdown
## Symptoms
- Export job failed or stuck
- Payroll team reports missing data
- Alert: "Payroll Export Delayed"

## Investigation Steps
1. Check calculation approval status
2. Verify no pending amendments
3. Check Payroll API health
4. Review export audit log

## Resolution
- If pending approvals: Notify approvers
- If API failure: Contact Payroll team
- If partial failure: Generate delta export

## Manual Export
```powershell
# Trigger export for specific period
POST /api/v1/admin/exports/trigger
{
  "period": "2025-01",
  "force": true
}
```
```

### 10.2 Health Check Endpoints

```csharp
public class IntegrationHealthCheck : IHealthCheck
{
    private readonly IErpClient _erpClient;
    private readonly IPayrollClient _payrollClient;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken)
    {
        var results = new Dictionary<string, object>();

        // Check ERP
        try
        {
            var erpHealth = await _erpClient.CheckHealthAsync(cancellationToken);
            results["erp"] = erpHealth ? "healthy" : "unhealthy";
        }
        catch (Exception ex)
        {
            results["erp"] = $"error: {ex.Message}";
        }

        // Check Payroll
        try
        {
            var payrollHealth = await _payrollClient.CheckHealthAsync(cancellationToken);
            results["payroll"] = payrollHealth ? "healthy" : "unhealthy";
        }
        catch (Exception ex)
        {
            results["payroll"] = $"error: {ex.Message}";
        }

        var allHealthy = results.Values.All(v => v.ToString() == "healthy");

        return allHealthy
            ? HealthCheckResult.Healthy("All integrations healthy", results)
            : HealthCheckResult.Degraded("Some integrations unhealthy", data: results);
    }
}
```

---

## Appendix A: Integration Contacts

| System | Team | Contact | Escalation |
|--------|------|---------|------------|
| ERP | IT Operations | erp-support@company.com | CTO |
| HR | HR Tech | hrtech@company.com | CHRO |
| Payroll | Finance Tech | payroll-tech@company.com | CFO |
| Azure | Cloud Ops | cloud-ops@company.com | CIO |

---

> *"I bent my Wookie."* - Ralph Wiggum
>
> Even when integrations have issues, our error handling keeps everything running smoothly!

---

**Document Owner:** Lead Developer (Claude Code)
**Review Cycle:** Quarterly
**Last Review:** January 2025

---

*This document is part of the DSIF Quality Gate Framework - QG-3 Deliverable*
