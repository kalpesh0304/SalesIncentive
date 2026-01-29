# DORISE Sales Incentive Framework
## Security Architecture

**Document ID:** DOC-ARCH-003
**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Created:** January 2025
**Last Updated:** January 2025
**Author:** Claude Code
**Status:** Draft

> *"That's where I saw the leprechaun. He tells me to burn things."* - Ralph Wiggum
>
> Unlike Ralph's leprechaun, our security measures are very real - and they protect everything.

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Security Principles](#2-security-principles)
3. [Identity & Access Management](#3-identity--access-management)
4. [Data Protection](#4-data-protection)
5. [Network Security](#5-network-security)
6. [Application Security](#6-application-security)
7. [Infrastructure Security](#7-infrastructure-security)
8. [Monitoring & Detection](#8-monitoring--detection)
9. [Compliance](#9-compliance)
10. [Incident Response](#10-incident-response)

---

## 1. Executive Summary

### 1.1 Purpose

This document defines the security architecture for DSIF, ensuring:
- Protection of sensitive financial and employee data
- Compliance with regulatory requirements
- Defense against common attack vectors
- Audit trail integrity

### 1.2 Security Objectives

| Objective | Description | Priority |
|-----------|-------------|----------|
| **Confidentiality** | Protect PII and financial data | Critical |
| **Integrity** | Ensure calculation accuracy and audit immutability | Critical |
| **Availability** | Maintain 99.9% uptime | High |
| **Accountability** | Track all actions to individuals | Critical |
| **Non-repudiation** | Prevent denial of actions | High |

### 1.3 Threat Landscape

| Threat Category | Risk Level | Primary Controls |
|-----------------|------------|------------------|
| Unauthorized Access | High | Azure AD, MFA, RBAC |
| Data Breach | High | Encryption, Network isolation |
| Insider Threat | Medium | Audit logging, least privilege |
| SQL Injection | Medium | Parameterized queries, WAF |
| Data Manipulation | High | Immutable records, checksums |

---

## 2. Security Principles

### 2.1 Core Security Principles

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        SECURITY PRINCIPLES                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐             │
│  │  ZERO TRUST     │  │  DEFENSE IN     │  │  LEAST          │             │
│  │                 │  │  DEPTH          │  │  PRIVILEGE      │             │
│  │  Never trust,   │  │  Multiple       │  │  Minimum        │             │
│  │  always verify  │  │  security       │  │  access         │             │
│  │                 │  │  layers         │  │  required       │             │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘             │
│                                                                              │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐             │
│  │  SECURE BY      │  │  FAIL           │  │  SEPARATION     │             │
│  │  DEFAULT        │  │  SECURE         │  │  OF DUTIES      │             │
│  │                 │  │                 │  │                 │             │
│  │  Security on,   │  │  Deny on        │  │  No single      │             │
│  │  not opt-in     │  │  error          │  │  point of       │             │
│  │                 │  │                 │  │  compromise     │             │
│  └─────────────────┘  └─────────────────┘  └─────────────────┘             │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2.2 Security Controls Framework

| Layer | Controls |
|-------|----------|
| **Perimeter** | WAF, DDoS protection, IP filtering |
| **Network** | VNet isolation, NSGs, Private endpoints |
| **Identity** | Azure AD, MFA, Conditional Access |
| **Application** | Input validation, CSRF protection, secure headers |
| **Data** | Encryption at rest/transit, masking, tokenization |
| **Endpoint** | Secure development, dependency scanning |

---

## 3. Identity & Access Management

### 3.1 Authentication Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      AUTHENTICATION FLOW                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌──────────┐      ┌──────────────┐      ┌──────────────┐                  │
│  │   User   │─────▶│  DSIF App    │─────▶│  Azure AD    │                  │
│  │ Browser  │      │  (Redirect)  │      │  (Entra ID)  │                  │
│  └──────────┘      └──────────────┘      └──────────────┘                  │
│       │                                         │                           │
│       │                                         │ 1. Authentication         │
│       │                                         ▼                           │
│       │                                  ┌──────────────┐                   │
│       │                                  │     MFA      │                   │
│       │                                  │   Required   │                   │
│       │                                  └──────────────┘                   │
│       │                                         │                           │
│       │                                         │ 2. Token issued           │
│       │◀────────────────────────────────────────┘                           │
│       │         (ID Token + Access Token)                                   │
│       │                                                                      │
│       │         3. Access application                                        │
│       ▼                                                                      │
│  ┌──────────────────────────────────────────────────────────────────────┐   │
│  │                          DSIF Application                            │   │
│  │                                                                       │   │
│  │  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐              │   │
│  │  │   Validate  │───▶│   Extract   │───▶│   Apply     │              │   │
│  │  │   Token     │    │   Claims    │    │   RBAC      │              │   │
│  │  └─────────────┘    └─────────────┘    └─────────────┘              │   │
│  │                                                                       │   │
│  └──────────────────────────────────────────────────────────────────────┘   │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Azure AD Configuration

| Setting | Value | Purpose |
|---------|-------|---------|
| **App Registration** | DSIF-Prod-App | OAuth2/OIDC |
| **Redirect URIs** | https://app-dorise-*.azurewebsites.net | Auth callback |
| **Token Lifetime** | 1 hour | Session duration |
| **Refresh Token** | 14 days (sliding) | Session refresh |
| **MFA Policy** | Required for all users | Strong auth |

### 3.3 Role-Based Access Control (RBAC)

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          RBAC MATRIX                                         │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ROLE HIERARCHY                                                              │
│                                                                              │
│  ┌───────────────────┐                                                      │
│  │  System Admin     │ ◀─── Full system access                              │
│  └─────────┬─────────┘                                                      │
│            │                                                                 │
│            ├────────────────┬────────────────┐                              │
│            ▼                ▼                ▼                              │
│  ┌─────────────────┐ ┌─────────────────┐ ┌─────────────────┐               │
│  │ Finance Manager │ │ Business Analyst│ │  Sales Manager  │               │
│  │ (Approvals)     │ │ (Configuration) │ │ (View Team)     │               │
│  └────────┬────────┘ └─────────────────┘ └────────┬────────┘               │
│           │                                        │                        │
│           └─────────────────┬──────────────────────┘                        │
│                             ▼                                               │
│                    ┌─────────────────┐                                      │
│                    │ Sales Rep       │                                      │
│                    │ (View Own)      │                                      │
│                    └─────────────────┘                                      │
│                             │                                               │
│                             ▼                                               │
│                    ┌─────────────────┐                                      │
│                    │ Auditor         │ ◀─── Read-only, all data            │
│                    │ (Read All)      │                                      │
│                    └─────────────────┘                                      │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.4 Permission Matrix

| Permission | Admin | Finance Mgr | Business Analyst | Sales Mgr | Sales Rep | Auditor |
|------------|-------|-------------|------------------|-----------|-----------|---------|
| View All Employees | ✅ | ✅ | ✅ | Scope | Own | ✅ |
| Create Employee | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| View All Plans | ✅ | ✅ | ✅ | ✅ | ❌ | ✅ |
| Create/Edit Plans | ✅ | ❌ | ✅ | ❌ | ❌ | ❌ |
| Run Calculations | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |
| Approve Incentives | ❌ | ✅ | ❌ | ❌ | ❌ | ❌ |
| View All Calculations | ✅ | ✅ | ✅ | Scope | Own | ✅ |
| View Audit Logs | ✅ | ❌ | ❌ | ❌ | ❌ | ✅ |
| Manage Users | ✅ | ❌ | ❌ | ❌ | ❌ | ❌ |
| Export to Payroll | ✅ | ✅ | ❌ | ❌ | ❌ | ❌ |

### 3.5 Scope-Based Access

```csharp
// Scope-based authorization
public class ScopeRequirement : IAuthorizationRequirement
{
    public ScopeType Type { get; }
    public string[] AllowedScopes { get; }
}

public class ScopeHandler : AuthorizationHandler<ScopeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ScopeRequirement requirement)
    {
        var userScopes = context.User.FindAll("scope")
            .Select(c => c.Value);

        if (userScopes.Intersect(requirement.AllowedScopes).Any())
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
```

> *"Hi, Super Nintendo Chalmers!"* - Ralph Wiggum
>
> Our RBAC system knows exactly who you are - even if you get names mixed up.

---

## 4. Data Protection

### 4.1 Encryption Strategy

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                        DATA ENCRYPTION LAYERS                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                    IN TRANSIT                                        │    │
│  │  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐              │    │
│  │  │   TLS 1.3   │    │  HTTPS Only │    │  Cert Mgmt  │              │    │
│  │  │   Minimum   │    │  HSTS       │    │  (Azure)    │              │    │
│  │  └─────────────┘    └─────────────┘    └─────────────┘              │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                    AT REST                                           │    │
│  │  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐              │    │
│  │  │  Azure SQL  │    │ Blob Storage│    │ Redis Cache │              │    │
│  │  │  TDE (AES-  │    │ SSE (AES-   │    │ TLS + At    │              │    │
│  │  │  256)       │    │ 256)        │    │ Rest Enc.   │              │    │
│  │  └─────────────┘    └─────────────┘    └─────────────┘              │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │                    APPLICATION LEVEL                                 │    │
│  │  ┌─────────────┐    ┌─────────────┐    ┌─────────────┐              │    │
│  │  │  Column     │    │   Key       │    │  Sensitive  │              │    │
│  │  │  Encryption │    │  Vault      │    │  Data       │              │    │
│  │  │  (Optional) │    │  Managed    │    │  Masking    │              │    │
│  │  └─────────────┘    └─────────────┘    └─────────────┘              │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Data Classification

| Classification | Description | Examples | Controls |
|----------------|-------------|----------|----------|
| **Confidential** | Highly sensitive | SSN, Bank accounts | Column encryption, restricted access |
| **Internal** | Business sensitive | Salary, calculations | Role-based access |
| **Private** | Personal data | Names, emails | Audit logging |
| **Public** | Non-sensitive | Plan names | Standard protection |

### 4.3 Sensitive Data Handling

```sql
-- Column-level encryption for sensitive fields (if required)
CREATE MASTER KEY ENCRYPTION BY PASSWORD = 'StrongPassword!';

CREATE CERTIFICATE SensitiveDataCert
WITH SUBJECT = 'Sensitive Data Encryption';

CREATE SYMMETRIC KEY SensitiveDataKey
WITH ALGORITHM = AES_256
ENCRYPTION BY CERTIFICATE SensitiveDataCert;

-- Encrypted column example
ALTER TABLE Employee
ADD SSN_Encrypted VARBINARY(256);
```

### 4.4 Data Masking

| Field | Mask Type | Original | Masked |
|-------|-----------|----------|--------|
| Email | Partial | john.doe@example.com | j***@example.com |
| Phone | Partial | 555-123-4567 | ***-***-4567 |
| Bank Account | Full | 1234567890 | ********** |
| SSN | Partial | 123-45-6789 | ***-**-6789 |

```csharp
// Dynamic data masking in EF Core
public class MaskedEmployeeDto
{
    public string EmployeeId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    [DataMask(MaskType.Email)]
    public string Email { get; set; }

    [DataMask(MaskType.Full)]
    public string BankAccount { get; set; }
}
```

---

## 5. Network Security

### 5.1 Network Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                       NETWORK SECURITY ARCHITECTURE                          │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│                            INTERNET                                          │
│                               │                                              │
│                               ▼                                              │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                     AZURE FRONT DOOR + WAF                             │ │
│  │  • DDoS Protection (Standard)                                          │ │
│  │  • Web Application Firewall (OWASP 3.2)                               │ │
│  │  • Geographic Filtering                                                │ │
│  │  • Rate Limiting                                                       │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│                               │                                              │
│                               ▼                                              │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                   VIRTUAL NETWORK (10.0.0.0/16)                        │ │
│  │  ┌──────────────────────────────────────────────────────────────────┐ │ │
│  │  │              PUBLIC SUBNET (10.0.1.0/24)                         │ │ │
│  │  │  ┌─────────────────┐                                             │ │ │
│  │  │  │   Application   │                                             │ │ │
│  │  │  │   Gateway       │ ◀── NSG: Allow 443 from Front Door         │ │ │
│  │  │  └─────────────────┘                                             │ │ │
│  │  └──────────────────────────────────────────────────────────────────┘ │ │
│  │                               │                                        │ │
│  │  ┌──────────────────────────────────────────────────────────────────┐ │ │
│  │  │              APP SUBNET (10.0.2.0/24)                            │ │ │
│  │  │  ┌─────────────────┐    ┌─────────────────┐                     │ │ │
│  │  │  │   App Service   │    │ Azure Functions │                     │ │ │
│  │  │  │   (VNet Int.)   │    │  (VNet Int.)    │                     │ │ │
│  │  │  └─────────────────┘    └─────────────────┘                     │ │ │
│  │  │                                                                   │ │ │
│  │  │  NSG: Allow from App Gateway only                                │ │ │
│  │  └──────────────────────────────────────────────────────────────────┘ │ │
│  │                               │                                        │ │
│  │  ┌──────────────────────────────────────────────────────────────────┐ │ │
│  │  │              DATA SUBNET (10.0.3.0/24)                           │ │ │
│  │  │  ┌─────────────────┐    ┌─────────────────┐    ┌─────────────┐  │ │ │
│  │  │  │   Azure SQL     │    │   Redis Cache   │    │ Key Vault   │  │ │ │
│  │  │  │  (Pvt Endpoint) │    │  (Pvt Endpoint) │    │(Pvt Endpoint│  │ │ │
│  │  │  └─────────────────┘    └─────────────────┘    └─────────────┘  │ │ │
│  │  │                                                                   │ │ │
│  │  │  NSG: Allow from App Subnet only, Deny Internet                  │ │ │
│  │  └──────────────────────────────────────────────────────────────────┘ │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 5.2 Network Security Groups (NSG)

| NSG | Inbound Rules | Outbound Rules |
|-----|---------------|----------------|
| **nsg-public** | 443 from Front Door | Any to VNet |
| **nsg-app** | 443 from public subnet | 1433, 6380 to data subnet |
| **nsg-data** | 1433, 6380 from app subnet | Deny Internet |

### 5.3 Private Endpoints

| Service | Private Endpoint | DNS Zone |
|---------|------------------|----------|
| Azure SQL | pep-sql-dorise | privatelink.database.windows.net |
| Redis Cache | pep-redis-dorise | privatelink.redis.cache.windows.net |
| Key Vault | pep-kv-dorise | privatelink.vaultcore.azure.net |
| Blob Storage | pep-blob-dorise | privatelink.blob.core.windows.net |

---

## 6. Application Security

### 6.1 OWASP Top 10 Mitigations

| Vulnerability | Mitigation | Implementation |
|---------------|------------|----------------|
| **A01: Broken Access Control** | RBAC, Scope validation | Custom authorization handlers |
| **A02: Cryptographic Failures** | TLS 1.3, AES-256 | Azure managed certificates |
| **A03: Injection** | Parameterized queries | EF Core ORM |
| **A04: Insecure Design** | Threat modeling | Security reviews |
| **A05: Security Misconfiguration** | Secure defaults | IaC with Bicep |
| **A06: Vulnerable Components** | Dependency scanning | Dependabot, Snyk |
| **A07: Auth Failures** | Azure AD, MFA | Microsoft.Identity.Web |
| **A08: Data Integrity Failures** | Signed artifacts | CI/CD pipeline |
| **A09: Logging Failures** | Comprehensive logging | Application Insights |
| **A10: SSRF** | URL validation | Allowlist approach |

### 6.2 Secure Headers

```csharp
// Security headers middleware
app.Use(async (context, next) =>
{
    context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Add("X-Frame-Options", "DENY");
    context.Response.Headers.Add("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Add("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Add("Content-Security-Policy",
        "default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'");
    context.Response.Headers.Add("Permissions-Policy",
        "geolocation=(), microphone=(), camera=()");

    await next();
});

// HSTS
app.UseHsts();
```

### 6.3 Input Validation

```csharp
// FluentValidation example
public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeValidator()
    {
        RuleFor(x => x.EmployeeNumber)
            .NotEmpty()
            .MaximumLength(20)
            .Matches(@"^[A-Z0-9]+$")
            .WithMessage("Employee number must be alphanumeric");

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .Must(BeValidDomain)
            .WithMessage("Email must be from @dorise.com domain");

        RuleFor(x => x.HireDate)
            .NotEmpty()
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("Hire date cannot be in the future");
    }

    private bool BeValidDomain(string email)
    {
        return email?.EndsWith("@dorise.com", StringComparison.OrdinalIgnoreCase) ?? false;
    }
}
```

### 6.4 API Security

| Control | Implementation |
|---------|----------------|
| **Rate Limiting** | 1000 req/min per client |
| **Request Size** | Max 4MB |
| **Timeout** | 30 seconds |
| **CORS** | Allowlist of domains |
| **API Versioning** | URL-based (v1, v2) |
| **Request Validation** | FluentValidation |

> *"I bent my Wookiee."* - Ralph Wiggum
>
> Our input validation ensures no one can bend our data in unexpected ways.

---

## 7. Infrastructure Security

### 7.1 Secrets Management

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      SECRETS MANAGEMENT                                      │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌────────────────────────────────────────────────────────────────────┐     │
│  │                     AZURE KEY VAULT                                 │     │
│  │                                                                      │     │
│  │  Secrets:                                                            │     │
│  │  ├── SqlConnectionString                                            │     │
│  │  ├── RedisConnectionString                                          │     │
│  │  ├── PayrollApiKey                                                  │     │
│  │  ├── ErpApiKey                                                      │     │
│  │  └── JwtSigningKey                                                  │     │
│  │                                                                      │     │
│  │  Certificates:                                                       │     │
│  │  ├── app-dorise-cert (SSL/TLS)                                      │     │
│  │  └── signing-cert (Token signing)                                   │     │
│  │                                                                      │     │
│  │  Access Policies:                                                    │     │
│  │  ├── App Service Managed Identity: Get, List                        │     │
│  │  ├── Functions Managed Identity: Get, List                          │     │
│  │  └── Admin Group: All operations                                    │     │
│  └────────────────────────────────────────────────────────────────────┘     │
│                                                                              │
│  ┌────────────────────────────────────────────────────────────────────┐     │
│  │                   MANAGED IDENTITIES                                │     │
│  │                                                                      │     │
│  │  ┌─────────────────┐    ┌─────────────────┐                        │     │
│  │  │   App Service   │    │ Azure Functions │                        │     │
│  │  │   Identity      │    │   Identity      │                        │     │
│  │  └────────┬────────┘    └────────┬────────┘                        │     │
│  │           │                      │                                  │     │
│  │           └──────────┬───────────┘                                  │     │
│  │                      ▼                                              │     │
│  │  ┌─────────────────────────────────────────────────────────────┐   │     │
│  │  │  RBAC Assignments:                                          │   │     │
│  │  │  ├── Key Vault Secrets User (Key Vault)                     │   │     │
│  │  │  ├── SQL DB Contributor (Azure SQL)                         │   │     │
│  │  │  ├── Storage Blob Data Contributor (Blob)                   │   │     │
│  │  │  └── Redis Cache Contributor (Redis)                        │   │     │
│  │  └─────────────────────────────────────────────────────────────┘   │     │
│  └────────────────────────────────────────────────────────────────────┘     │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 7.2 Secret Rotation

| Secret Type | Rotation Period | Method |
|-------------|-----------------|--------|
| Database Connection | 90 days | Key Vault + Auto rotation |
| API Keys | 90 days | Manual with notification |
| Certificates | 1 year | Azure managed |
| Encryption Keys | 2 years | Key Vault managed |

### 7.3 Managed Identity Configuration

```csharp
// Using managed identity for Azure SQL
builder.Services.AddDbContext<IncentiveDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        sqlOptions.UseAzureSqlDefaults();
    });
});

// Configure token-based authentication
builder.Services.AddSingleton<SqlAuthenticationProvider, AzureSqlAuthProvider>();

// Key Vault configuration
builder.Configuration.AddAzureKeyVault(
    new Uri($"https://kv-dorise-{environment}.vault.azure.net/"),
    new DefaultAzureCredential());
```

---

## 8. Monitoring & Detection

### 8.1 Security Monitoring Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    SECURITY MONITORING                                       │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  DATA SOURCES                                                                │
│  ┌─────────────┐ ┌─────────────┐ ┌─────────────┐ ┌─────────────┐           │
│  │  App Logs   │ │  AAD Logs   │ │ NSG Flows   │ │  SQL Audit  │           │
│  └──────┬──────┘ └──────┬──────┘ └──────┬──────┘ └──────┬──────┘           │
│         │               │               │               │                   │
│         └───────────────┴───────────────┴───────────────┘                   │
│                                 │                                            │
│                                 ▼                                            │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                    LOG ANALYTICS WORKSPACE                             │ │
│  │                                                                         │ │
│  │  ┌─────────────────────────────────────────────────────────────────┐   │ │
│  │  │  KUSTO QUERIES (KQL)                                            │   │ │
│  │  │                                                                  │   │ │
│  │  │  // Failed login attempts                                        │   │ │
│  │  │  SigninLogs                                                      │   │ │
│  │  │  | where ResultType != 0                                         │   │ │
│  │  │  | summarize count() by UserPrincipalName, bin(TimeGenerated, 1h)│   │ │
│  │  │                                                                  │   │ │
│  │  │  // Unusual data access                                          │   │ │
│  │  │  AuditLogs                                                       │   │ │
│  │  │  | where OperationName contains "Read"                           │   │ │
│  │  │  | where TimeGenerated > ago(24h)                                │   │ │
│  │  │  | summarize AccessCount=count() by UserId                       │   │ │
│  │  │  | where AccessCount > 1000                                      │   │ │
│  │  └─────────────────────────────────────────────────────────────────┘   │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│                                 │                                            │
│                                 ▼                                            │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                    MICROSOFT SENTINEL (SIEM)                           │ │
│  │                                                                         │ │
│  │  Detection Rules:                    Playbooks:                        │ │
│  │  ├── Brute force detection          ├── Block IP                      │ │
│  │  ├── Impossible travel               ├── Disable user                  │ │
│  │  ├── Privilege escalation            ├── Alert team                    │ │
│  │  ├── Data exfiltration               └── Create incident              │ │
│  │  └── Anomalous behavior                                                │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│                                 │                                            │
│                                 ▼                                            │
│  ┌────────────────────────────────────────────────────────────────────────┐ │
│  │                    AZURE MONITOR ALERTS                                │ │
│  │                                                                         │ │
│  │  ┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐    │ │
│  │  │  Email Alert    │    │  Teams Channel  │    │  PagerDuty      │    │ │
│  │  │  (Security Team)│    │  (#security)    │    │  (On-call)      │    │ │
│  │  └─────────────────┘    └─────────────────┘    └─────────────────┘    │ │
│  └────────────────────────────────────────────────────────────────────────┘ │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 8.2 Security Alerts

| Alert | Threshold | Severity | Response |
|-------|-----------|----------|----------|
| Failed logins | >5 in 5 min | High | Block IP temporarily |
| Unusual data access | >1000 records/hour | Medium | Review and notify |
| Privilege escalation | Any attempt | Critical | Immediate investigation |
| After-hours access | Outside 6AM-10PM | Low | Log for review |
| Admin actions | Any | Medium | Audit trail review |

### 8.3 Audit Logging Requirements

| Event Category | Logged Data | Retention |
|----------------|-------------|-----------|
| Authentication | User, IP, result, timestamp | 2 years |
| Authorization | Resource, action, decision | 2 years |
| Data Access | Entity, operation, user | 7 years |
| Admin Actions | Action, target, user | 7 years |
| System Events | Error, warning, info | 90 days |

---

## 9. Compliance

### 9.1 Compliance Framework

| Framework | Applicability | Status |
|-----------|---------------|--------|
| **SOC 2 Type II** | Service organization | Target |
| **GDPR** | EU data subjects | Required |
| **ISO 27001** | Information security | Target |
| **PCI DSS** | N/A (no payment card data) | N/A |

### 9.2 GDPR Compliance Controls

| GDPR Article | Requirement | Implementation |
|--------------|-------------|----------------|
| Art. 5 | Data minimization | Only collect required data |
| Art. 6 | Lawful basis | Employment contract |
| Art. 15 | Right of access | Data export API |
| Art. 16 | Right to rectification | Self-service update |
| Art. 17 | Right to erasure | Anonymization process |
| Art. 20 | Data portability | JSON/CSV export |
| Art. 25 | Privacy by design | Data classification |
| Art. 32 | Security measures | This document |
| Art. 33 | Breach notification | Incident response plan |

### 9.3 Security Assessments

| Assessment | Frequency | Scope |
|------------|-----------|-------|
| Vulnerability scan | Monthly | All systems |
| Penetration test | Annual | External + Internal |
| Code review | Per PR | Security-sensitive code |
| Access review | Quarterly | User permissions |
| Third-party audit | Annual | SOC 2 |

---

## 10. Incident Response

### 10.1 Incident Response Plan

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                    INCIDENT RESPONSE WORKFLOW                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  1. DETECTION                                                                │
│     └─▶ Automated alert OR Manual report                                    │
│                │                                                             │
│                ▼                                                             │
│  2. TRIAGE (< 15 minutes)                                                   │
│     └─▶ Classify severity: Critical / High / Medium / Low                   │
│                │                                                             │
│                ▼                                                             │
│  3. CONTAINMENT (< 1 hour for Critical)                                     │
│     ├─▶ Isolate affected systems                                            │
│     ├─▶ Preserve evidence                                                   │
│     └─▶ Notify stakeholders                                                 │
│                │                                                             │
│                ▼                                                             │
│  4. INVESTIGATION                                                            │
│     ├─▶ Determine scope and impact                                          │
│     ├─▶ Identify root cause                                                 │
│     └─▶ Document findings                                                   │
│                │                                                             │
│                ▼                                                             │
│  5. REMEDIATION                                                              │
│     ├─▶ Apply fixes                                                         │
│     ├─▶ Verify resolution                                                   │
│     └─▶ Restore services                                                    │
│                │                                                             │
│                ▼                                                             │
│  6. POST-INCIDENT                                                            │
│     ├─▶ Post-mortem review                                                  │
│     ├─▶ Update runbooks                                                     │
│     └─▶ Implement preventive measures                                       │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 10.2 Severity Classification

| Severity | Definition | Response Time | Examples |
|----------|------------|---------------|----------|
| **Critical** | Active breach, data loss | 15 minutes | Unauthorized access, ransomware |
| **High** | Potential breach | 1 hour | Vulnerability exploitation attempt |
| **Medium** | Security anomaly | 4 hours | Failed brute force, suspicious access |
| **Low** | Minor issue | 24 hours | Policy violation, misconfiguration |

### 10.3 Communication Matrix

| Audience | Critical | High | Medium | Low |
|----------|----------|------|--------|-----|
| Security Team | Immediate | 1 hour | 4 hours | Daily digest |
| IT Management | 1 hour | 4 hours | Daily | Weekly |
| Executive | 2 hours | 24 hours | As needed | Monthly |
| Legal/Privacy | 2 hours | 24 hours | As needed | As needed |
| Customers | As required | As required | N/A | N/A |

---

## Appendix A: Security Checklist

### Pre-Production Security Review

- [ ] Threat model reviewed
- [ ] Penetration test completed
- [ ] Vulnerability scan passed
- [ ] Security headers configured
- [ ] CORS policy configured
- [ ] Rate limiting enabled
- [ ] Input validation tested
- [ ] Authentication tested
- [ ] Authorization tested
- [ ] Audit logging verified
- [ ] Encryption verified
- [ ] Key rotation configured
- [ ] Backup tested
- [ ] Incident response tested

---

> *"Go Banana!"* - Ralph Wiggum
>
> With this security architecture, we're ready to go live - safely and securely!

---

**Document Approval**

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Solution Architect | Skanda Prasad | _____________ | ______ |
| Security Lead | TBD | _____________ | ______ |

---

*This document is part of the DSIF Quality Gate Framework - QG-2 Deliverable*
