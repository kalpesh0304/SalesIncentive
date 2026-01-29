# DORISE Sales Incentive Framework
## Design Decisions (ADR Log)

**Document ID:** DOC-013
**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Created:** January 2025
**Last Updated:** January 2025
**Status:** Active

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [Decision Template](#2-decision-template)
3. [Decisions Log](#3-decisions-log)

---

## 1. Introduction

### 1.1 Purpose

This document records all significant architectural and design decisions made during the DSIF project. It provides context, rationale, and consequences for each decision to ensure team alignment and historical reference.

### 1.2 When to Record a Decision

Record a decision when:
- It impacts the architecture or system design
- It involves technology selection
- It affects multiple components or teams
- It has long-term implications
- It cannot be easily reversed

### 1.3 Decision Status

| Status | Description |
|--------|-------------|
| **Proposed** | Decision is under consideration |
| **Approved** | Decision has been approved and should be followed |
| **Deprecated** | Decision is no longer recommended |
| **Superseded** | Decision has been replaced by another decision |

---

## 2. Decision Template

Use this template for recording new decisions:

```markdown
## DD-XXX: [Decision Title]

| Attribute | Value |
|-----------|-------|
| **Date** | YYYY-MM-DD |
| **Decision Maker** | [Name/Role] |
| **Status** | Proposed / Approved / Deprecated / Superseded |
| **Superseded By** | DD-XXX (if applicable) |

### Context
[Why this decision was needed - the problem or requirement that prompted it]

### Options Considered

#### Option A: [Name]
- **Description:** [Brief description]
- **Pros:** [Benefits]
- **Cons:** [Drawbacks]

#### Option B: [Name]
- **Description:** [Brief description]
- **Pros:** [Benefits]
- **Cons:** [Drawbacks]

#### Option C: [Name]
- **Description:** [Brief description]
- **Pros:** [Benefits]
- **Cons:** [Drawbacks]

### Decision
[Which option was chosen and a brief statement of the decision]

### Rationale
[Why this option was selected over the alternatives]

### Consequences
[Impact of this decision - what changes, what new constraints, what trade-offs]

### Related Decisions
- DD-XXX: [Related decision title]
```

---

## 3. Decisions Log

### DD-001: Use Azure SQL Database for Data Storage

| Attribute | Value |
|-----------|-------|
| **Date** | January 2025 |
| **Decision Maker** | Solution Architect |
| **Status** | Approved |

#### Context
The DSIF application requires a relational database to store employee data, incentive plans, calculations, and audit trails. The database must support complex queries, transactions, and meet compliance requirements for financial data.

#### Options Considered

##### Option A: Azure SQL Database
- **Description:** Managed relational database service on Azure
- **Pros:** Native Azure integration, managed service, built-in HA, compliance certifications, familiar SQL syntax
- **Cons:** Cost at scale, some advanced features require higher tiers

##### Option B: Azure Cosmos DB
- **Description:** NoSQL document database on Azure
- **Pros:** Global distribution, elastic scaling, flexible schema
- **Cons:** More complex for relational data, requires denormalization, higher learning curve for financial calculations

##### Option C: PostgreSQL on Azure
- **Description:** Open-source relational database on Azure
- **Pros:** Open source, lower licensing cost, advanced features
- **Cons:** Less native Azure integration, more operational overhead

#### Decision
Use **Azure SQL Database** for all persistent data storage.

#### Rationale
- Relational data model fits the business domain (employees, hierarchies, calculations)
- Complex reporting queries are better served by SQL
- Audit trail requirements benefit from ACID transactions
- Team familiarity with SQL Server reduces development risk
- Native Azure integration simplifies operations

#### Consequences
- All entities will be designed for relational storage
- Entity Framework Core will be used as ORM
- Point-in-time restore available for audit compliance
- Cost monitoring required for DTU/vCore usage

---

### DD-002: Use Blazor Server for Frontend

| Attribute | Value |
|-----------|-------|
| **Date** | January 2025 |
| **Decision Maker** | Solution Architect |
| **Status** | Approved |

#### Context
The DSIF application requires a web frontend for business users to view and manage incentive calculations, approvals, and reports.

#### Options Considered

##### Option A: Blazor Server
- **Description:** Server-side rendered Blazor with SignalR connection
- **Pros:** .NET ecosystem alignment, real-time updates, simpler deployment, full .NET API access
- **Cons:** Requires persistent connection, server memory per user, latency sensitivity

##### Option B: Blazor WebAssembly
- **Description:** Client-side Blazor running in browser
- **Pros:** Offline capability, reduced server load, faster after initial load
- **Cons:** Larger download size, API required for all data, limited debugging

##### Option C: React SPA
- **Description:** JavaScript-based single page application
- **Pros:** Large ecosystem, mature tooling, wide developer availability
- **Cons:** Different tech stack from backend, context switching for developers

#### Decision
Use **Blazor Server** for the web frontend.

#### Rationale
- Maintains .NET ecosystem consistency
- Real-time dashboard updates via SignalR are valuable
- Team expertise is primarily .NET
- Simpler deployment model for enterprise environment
- Internal application doesn't require offline capability

#### Consequences
- SignalR hub required for client connections
- Azure SignalR Service recommended for scaling
- Session state management considerations
- Connection resilience handling required

---

### DD-003: Use Azure Functions for Background Processing

| Attribute | Value |
|-----------|-------|
| **Date** | January 2025 |
| **Decision Maker** | Solution Architect |
| **Status** | Approved |

#### Context
The DSIF application requires background processing for scheduled incentive calculations (daily, weekly, monthly), batch operations, and asynchronous processing.

#### Options Considered

##### Option A: Azure Functions (Isolated)
- **Description:** Serverless compute for event-driven background jobs
- **Pros:** Auto-scaling, cost-effective (pay per execution), timer triggers, isolated process model
- **Cons:** Cold start latency, execution time limits, debugging complexity

##### Option B: Azure WebJobs
- **Description:** Background jobs running with App Service
- **Pros:** Simple deployment with API, continuous running option
- **Cons:** Tied to App Service scaling, less flexible triggers

##### Option C: Hangfire
- **Description:** Self-hosted background job library
- **Pros:** Rich dashboard, mature library, full control
- **Cons:** Requires hosting infrastructure, additional complexity

#### Decision
Use **Azure Functions (Isolated Worker Model)** for all background processing.

#### Rationale
- Serverless model provides automatic scaling for batch calculation peaks
- Timer triggers ideal for scheduled calculations
- Cost-effective for intermittent workloads
- Isolated process model provides better dependency isolation
- Native Azure integration for monitoring and alerts

#### Consequences
- Functions project required in solution
- Durable Functions may be needed for long-running workflows
- Function orchestration for complex calculation batches
- Premium plan may be required to avoid cold starts

---

### DD-004: Implement Quality Gate Framework

| Attribute | Value |
|-----------|-------|
| **Date** | January 2025 |
| **Decision Maker** | Project Manager |
| **Status** | Approved |

#### Context
To ensure proper project governance and prevent premature development, a structured approach to phase transitions is needed.

#### Options Considered

##### Option A: Quality Gates (Mandatory Checkpoints)
- **Description:** Formal gates between phases requiring sign-off
- **Pros:** Ensures completeness, reduces rework, clear accountability
- **Cons:** May slow velocity, requires discipline, overhead for reviews

##### Option B: Continuous Flow (No Gates)
- **Description:** Work flows continuously without formal checkpoints
- **Pros:** Maximum velocity, flexibility, agile approach
- **Cons:** Risk of incomplete foundations, technical debt, rework

##### Option C: Informal Reviews
- **Description:** Reviews happen but are not mandatory
- **Pros:** Balance of governance and flexibility
- **Cons:** Inconsistent application, unclear accountability

#### Decision
Implement **mandatory Quality Gates** between all project phases.

#### Rationale
- Financial application requires strong governance
- Audit trail requirements demand proper documentation
- Prevents costly rework from incomplete requirements
- Establishes clear accountability for deliverables
- Aligns with enterprise project management standards

#### Consequences
- Seven quality gates (QG-0 through QG-6) defined
- Work blocked until gate passage
- Gate checklists and sign-off required
- GATE_STATUS.md tracks current status
- Exception process defined for urgent situations

---

## Appendix A: Decision Index

| ID | Title | Status | Date |
|----|-------|--------|------|
| DD-001 | Use Azure SQL Database for Data Storage | Approved | January 2025 |
| DD-002 | Use Blazor Server for Frontend | Approved | January 2025 |
| DD-003 | Use Azure Functions for Background Processing | Approved | January 2025 |
| DD-004 | Implement Quality Gate Framework | Approved | January 2025 |

---

## Document History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | January 2025 | [Author] | Initial document with foundational decisions |

---

**Document Owner:** Solution Architect
**Review Cycle:** As decisions are made
**Location:** `/docs/design/DESIGN_DECISIONS.md`

*This document is part of the Dorise Sales Incentive Framework project documentation.*
