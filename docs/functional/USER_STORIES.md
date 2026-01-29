# DORISE Sales Incentive Framework
## User Stories

**Document ID:** DOC-REQ-002
**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Created:** January 2025
**Last Updated:** January 2025
**Author:** Claude Code
**Status:** Draft

> *"I choo-choo-choose you!"* - Ralph Wiggum
>
> Just like Ralph chose his Valentine, our users choose the features that matter most to them.

---

## Table of Contents

1. [Document Control](#document-control)
2. [Overview](#overview)
3. [User Personas](#user-personas)
4. [Epic Structure](#epic-structure)
5. [User Stories by Epic](#user-stories-by-epic)
6. [Story Map](#story-map)

---

## 1. Document Control

### Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | January 2025 | Claude Code | Initial user stories document |

### Reviewers

| Role | Name | Review Date | Status |
|------|------|-------------|--------|
| Product Owner | Skanda Prasad | Pending | - |
| Business Analyst | TBD | Pending | - |

---

## 2. Overview

This document contains all user stories for the DSIF project, organized by epic. Each user story follows the standard format:

```
As a [persona],
I want to [action/goal],
So that [benefit/value].
```

### Story Point Scale

| Points | Complexity | Typical Effort |
|--------|------------|----------------|
| 1 | Trivial | Few hours |
| 2 | Simple | ~1 day |
| 3 | Medium | 2-3 days |
| 5 | Complex | ~1 week |
| 8 | Very Complex | 1-2 weeks |
| 13 | Epic-level | Split required |

### Priority Definitions

| Priority | Description |
|----------|-------------|
| **Must Have** | Essential for MVP/Go-live |
| **Should Have** | Important but not blocking |
| **Could Have** | Nice to have |
| **Won't Have** | Out of scope for current release |

---

## 3. User Personas

### P1: System Administrator
**Description:** IT professional responsible for system configuration and maintenance
**Goals:** Keep the system running smoothly, manage users and permissions
**Pain Points:** Manual configuration is error-prone, lack of monitoring tools

### P2: Finance Manager
**Description:** Finance team member responsible for incentive approvals and reporting
**Goals:** Ensure accurate calculations, approve payments on time, maintain compliance
**Pain Points:** Cannot verify calculation accuracy, manual approval process is slow

### P3: Sales Manager
**Description:** Regional or store manager overseeing sales team performance
**Goals:** View team performance, understand incentive calculations
**Pain Points:** No visibility into calculation details, delayed incentive information

### P4: Sales Representative
**Description:** Front-line sales employee earning incentives
**Goals:** Understand earnings, track progress toward targets
**Pain Points:** No self-service view of incentives, unclear calculation methodology

### P5: Business Analyst
**Description:** User responsible for configuring incentive plans and formulas
**Goals:** Modify incentive structures without IT involvement
**Pain Points:** Formula changes require code deployments, no preview capability

### P6: Auditor
**Description:** Internal or external auditor reviewing incentive calculations
**Goals:** Verify calculation accuracy, trace changes, ensure compliance
**Pain Points:** No audit trail, cannot reproduce historical calculations

> *"Me fail English? That's unpossible!"* - Ralph Wiggum
>
> Understanding our users' needs is the first step to building something they'll love.

---

## 4. Epic Structure

| Epic ID | Epic Name | Description | Priority |
|---------|-----------|-------------|----------|
| **EP-001** | Employee Management | Managing sales employees and their assignments | Must Have |
| **EP-002** | Incentive Configuration | Setting up and modifying incentive plans | Must Have |
| **EP-003** | Calculation Engine | Automated incentive calculations | Must Have |
| **EP-004** | Approval Workflow | Multi-level approval process | Must Have |
| **EP-005** | Reporting & Dashboards | Analytics and exports | Must Have |
| **EP-006** | User Management | Authentication and authorization | Must Have |
| **EP-007** | Integration | External system connectivity | Should Have |
| **EP-008** | Audit & Compliance | Audit trail and compliance features | Must Have |

---

## 5. User Stories by Epic

---

### EP-001: Employee Management

#### US-001: View Employee Hierarchy
**As a** Sales Manager,
**I want to** view the organizational hierarchy (Company > Zone > Store > Employee),
**So that** I can understand the structure and locate specific employees.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 3 |
| **Persona** | P3: Sales Manager |
| **Acceptance Criteria** | See AC-001 |

---

#### US-002: Add New Employee
**As a** System Administrator,
**I want to** add new employees with their role, designation, and assignment details,
**So that** they can be included in incentive calculations.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 5 |
| **Persona** | P1: System Administrator |
| **Acceptance Criteria** | See AC-002 |

---

#### US-003: Configure Split Shares
**As a** System Administrator,
**I want to** configure incentive split shares between multiple employees for shared territories,
**So that** incentives are correctly distributed when multiple salespeople share responsibility.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 5 |
| **Persona** | P1: System Administrator |
| **Acceptance Criteria** | See AC-003 |

---

#### US-004: View Assignment History
**As an** Auditor,
**I want to** view the complete assignment history for any employee,
**So that** I can verify historical incentive calculations were based on correct data.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 3 |
| **Persona** | P6: Auditor |
| **Acceptance Criteria** | See AC-004 |

---

#### US-005: Bulk Import Employees
**As a** System Administrator,
**I want to** import employees in bulk via Excel/CSV file,
**So that** I can efficiently load large numbers of employees during initial setup or updates.

| Attribute | Value |
|-----------|-------|
| **Priority** | Should Have |
| **Story Points** | 8 |
| **Persona** | P1: System Administrator |
| **Acceptance Criteria** | See AC-005 |

---

#### US-006: Search Employees
**As a** Finance Manager,
**I want to** search for employees by name, ID, store, or zone,
**So that** I can quickly find specific employees for review or action.

| Attribute | Value |
|-----------|-------|
| **Priority** | Should Have |
| **Story Points** | 3 |
| **Persona** | P2: Finance Manager |
| **Acceptance Criteria** | See AC-006 |

---

#### US-007: Update Employee Status
**As a** System Administrator,
**I want to** change an employee's status (Active, Inactive, Terminated) with an effective date,
**So that** the system correctly handles employees who leave or return.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 3 |
| **Persona** | P1: System Administrator |
| **Acceptance Criteria** | See AC-007 |

---

### EP-002: Incentive Configuration

> *"When I grow up, I want to be a principal or a caterpillar."* - Ralph Wiggum
>
> Our incentive plans can be anything they need to be - the system is that flexible!

---

#### US-008: Create Incentive Plan
**As a** Business Analyst,
**I want to** create new incentive plans with configurable parameters,
**So that** I can define calculation rules for different roles without IT involvement.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 8 |
| **Persona** | P5: Business Analyst |
| **Acceptance Criteria** | See AC-008 |

---

#### US-009: Configure Slabs
**As a** Business Analyst,
**I want to** define threshold-based slabs with percentage or fixed amounts,
**So that** incentives vary based on achievement levels.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 5 |
| **Persona** | P5: Business Analyst |
| **Acceptance Criteria** | See AC-009 |

---

#### US-010: View Plan History
**As an** Auditor,
**I want to** view the complete version history of any incentive plan,
**So that** I can understand what rules were in effect for any historical period.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 3 |
| **Persona** | P6: Auditor |
| **Acceptance Criteria** | See AC-010 |

---

#### US-011: Preview Plan Changes
**As a** Business Analyst,
**I want to** preview the impact of plan changes before activating them,
**So that** I can verify changes produce expected results.

| Attribute | Value |
|-----------|-------|
| **Priority** | Should Have |
| **Story Points** | 8 |
| **Persona** | P5: Business Analyst |
| **Acceptance Criteria** | See AC-011 |

---

#### US-012: Approve Plan Changes
**As a** Finance Manager,
**I want to** approve incentive plan changes before they take effect,
**So that** unauthorized or incorrect changes don't impact calculations.

| Attribute | Value |
|-----------|-------|
| **Priority** | Should Have |
| **Story Points** | 5 |
| **Persona** | P2: Finance Manager |
| **Acceptance Criteria** | See AC-012 |

---

#### US-013: Clone Incentive Plan
**As a** Business Analyst,
**I want to** clone an existing incentive plan as a starting point for a new one,
**So that** I can save time when creating similar plans.

| Attribute | Value |
|-----------|-------|
| **Priority** | Could Have |
| **Story Points** | 3 |
| **Persona** | P5: Business Analyst |
| **Acceptance Criteria** | See AC-013 |

---

### EP-003: Calculation Engine

---

#### US-014: Run Scheduled Calculations
**As a** System Administrator,
**I want** the system to automatically run incentive calculations on a scheduled basis,
**So that** incentives are calculated consistently without manual intervention.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 8 |
| **Persona** | P1: System Administrator |
| **Acceptance Criteria** | See AC-014 |

---

#### US-015: View Calculation Details
**As a** Sales Representative,
**I want to** view the detailed breakdown of my incentive calculation,
**So that** I understand exactly how my incentive was determined.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 5 |
| **Persona** | P4: Sales Representative |
| **Acceptance Criteria** | See AC-015 |

---

#### US-016: Manual Recalculation
**As a** Finance Manager,
**I want to** trigger a manual recalculation for specific employees or periods,
**So that** I can correct calculations when source data is corrected.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 5 |
| **Persona** | P2: Finance Manager |
| **Acceptance Criteria** | See AC-016 |

---

#### US-017: Preview Calculation
**As a** Finance Manager,
**I want to** preview calculation results without committing them,
**So that** I can verify results before they become official.

| Attribute | Value |
|-----------|-------|
| **Priority** | Should Have |
| **Story Points** | 5 |
| **Persona** | P2: Finance Manager |
| **Acceptance Criteria** | See AC-017 |

---

#### US-018: View Calculation History
**As an** Auditor,
**I want to** view all calculation versions including amendments,
**So that** I can trace the complete history of any incentive record.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 5 |
| **Persona** | P6: Auditor |
| **Acceptance Criteria** | See AC-018 |

---

#### US-019: Handle Proration
**As a** Finance Manager,
**I want** the system to automatically prorate incentives for mid-period changes,
**So that** employees are paid correctly when they join, leave, or transfer mid-period.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 8 |
| **Persona** | P2: Finance Manager |
| **Acceptance Criteria** | See AC-019 |

---

### EP-004: Approval Workflow

> *"The doctor said I wouldn't have so many nosebleeds if I kept my finger outta there."* - Ralph Wiggum
>
> Our approval workflow keeps people from poking where they shouldn't!

---

#### US-020: Submit for Approval
**As a** Finance Manager,
**I want to** submit calculated incentives for approval,
**So that** they can be reviewed before payment processing.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 3 |
| **Persona** | P2: Finance Manager |
| **Acceptance Criteria** | See AC-020 |

---

#### US-021: Approve Incentives
**As a** Finance Manager,
**I want to** approve or reject submitted incentives with comments,
**So that** payments can proceed or issues can be flagged for resolution.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 5 |
| **Persona** | P2: Finance Manager |
| **Acceptance Criteria** | See AC-021 |

---

#### US-022: Bulk Approval
**As a** Finance Manager,
**I want to** approve multiple incentive records at once,
**So that** I can efficiently process large batches of approvals.

| Attribute | Value |
|-----------|-------|
| **Priority** | Should Have |
| **Story Points** | 5 |
| **Persona** | P2: Finance Manager |
| **Acceptance Criteria** | See AC-022 |

---

#### US-023: Approval Notifications
**As a** Finance Manager,
**I want to** receive notifications when incentives are pending my approval,
**So that** I can act promptly and avoid delays.

| Attribute | Value |
|-----------|-------|
| **Priority** | Should Have |
| **Story Points** | 5 |
| **Persona** | P2: Finance Manager |
| **Acceptance Criteria** | See AC-023 |

---

#### US-024: Delegate Approval
**As a** Finance Manager,
**I want to** delegate my approval authority to another user temporarily,
**So that** approvals can continue during my absence.

| Attribute | Value |
|-----------|-------|
| **Priority** | Could Have |
| **Story Points** | 5 |
| **Persona** | P2: Finance Manager |
| **Acceptance Criteria** | See AC-024 |

---

#### US-025: View Approval History
**As an** Auditor,
**I want to** view the complete approval chain for any incentive,
**So that** I can verify proper governance was followed.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 3 |
| **Persona** | P6: Auditor |
| **Acceptance Criteria** | See AC-025 |

---

### EP-005: Reporting & Dashboards

---

#### US-026: Executive Dashboard
**As a** Finance Manager,
**I want to** view an executive dashboard with key incentive metrics,
**So that** I can monitor overall incentive spend and trends.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 8 |
| **Persona** | P2: Finance Manager |
| **Acceptance Criteria** | See AC-026 |

---

#### US-027: Export to Excel
**As a** Finance Manager,
**I want to** export incentive data to Excel,
**So that** I can perform additional analysis or share with stakeholders.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 3 |
| **Persona** | P2: Finance Manager |
| **Acceptance Criteria** | See AC-027 |

---

#### US-028: Team Performance Report
**As a** Sales Manager,
**I want to** view a report showing my team's incentive performance,
**So that** I can identify top performers and areas needing attention.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 5 |
| **Persona** | P3: Sales Manager |
| **Acceptance Criteria** | See AC-028 |

---

#### US-029: Audit Trail Report
**As an** Auditor,
**I want to** generate an audit trail report for any time period,
**So that** I can review all system activities during an audit.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 5 |
| **Persona** | P6: Auditor |
| **Acceptance Criteria** | See AC-029 |

---

#### US-030: Personal Incentive View
**As a** Sales Representative,
**I want to** view my personal incentive history and current period status,
**So that** I can track my earnings and progress toward targets.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 5 |
| **Persona** | P4: Sales Representative |
| **Acceptance Criteria** | See AC-030 |

---

### EP-006: User Management

---

#### US-031: Single Sign-On
**As a** User (any persona),
**I want to** log in using my corporate credentials (Azure Entra ID),
**So that** I don't need separate credentials for this system.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 5 |
| **Persona** | All |
| **Acceptance Criteria** | See AC-031 |

---

#### US-032: Manage User Roles
**As a** System Administrator,
**I want to** assign roles to users (Admin, Manager, Approver, Viewer),
**So that** access is properly controlled based on job function.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 5 |
| **Persona** | P1: System Administrator |
| **Acceptance Criteria** | See AC-032 |

---

#### US-033: Scope-Based Access
**As a** System Administrator,
**I want to** restrict user access to specific zones or stores,
**So that** users only see data relevant to their responsibility.

| Attribute | Value |
|-----------|-------|
| **Priority** | Should Have |
| **Story Points** | 8 |
| **Persona** | P1: System Administrator |
| **Acceptance Criteria** | See AC-033 |

---

#### US-034: View User Activity Log
**As a** System Administrator,
**I want to** view user activity logs,
**So that** I can monitor system usage and investigate issues.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 3 |
| **Persona** | P1: System Administrator |
| **Acceptance Criteria** | See AC-034 |

---

### EP-007: Integration

> *"My cat's breath smells like cat food."* - Ralph Wiggum
>
> Our APIs are straightforward - what you put in is what you get out!

---

#### US-035: Export to Payroll
**As a** System Administrator,
**I want** approved incentives to be automatically exported to the payroll system,
**So that** employees receive their payments without manual data entry.

| Attribute | Value |
|-----------|-------|
| **Priority** | Should Have |
| **Story Points** | 8 |
| **Persona** | P1: System Administrator |
| **Acceptance Criteria** | See AC-035 |

---

#### US-036: Import Sales Data
**As a** System Administrator,
**I want** the system to automatically import sales data from the ERP,
**So that** calculations are based on accurate, up-to-date information.

| Attribute | Value |
|-----------|-------|
| **Priority** | Should Have |
| **Story Points** | 8 |
| **Persona** | P1: System Administrator |
| **Acceptance Criteria** | See AC-036 |

---

#### US-037: API Access
**As a** System Administrator,
**I want to** provide API access to external systems,
**So that** third-party applications can interact with incentive data.

| Attribute | Value |
|-----------|-------|
| **Priority** | Should Have |
| **Story Points** | 5 |
| **Persona** | P1: System Administrator |
| **Acceptance Criteria** | See AC-037 |

---

### EP-008: Audit & Compliance

---

#### US-038: Immutable Audit Trail
**As an** Auditor,
**I want** all system changes to be recorded in an immutable audit trail,
**So that** I can trust the integrity of the records.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 5 |
| **Persona** | P6: Auditor |
| **Acceptance Criteria** | See AC-038 |

---

#### US-039: Point-in-Time Query
**As an** Auditor,
**I want to** query data as it existed at any point in time,
**So that** I can reproduce historical calculations exactly.

| Attribute | Value |
|-----------|-------|
| **Priority** | Must Have |
| **Story Points** | 8 |
| **Persona** | P6: Auditor |
| **Acceptance Criteria** | See AC-039 |

---

#### US-040: Data Retention Compliance
**As a** System Administrator,
**I want** the system to automatically archive data per retention policy,
**So that** we remain compliant with data retention requirements.

| Attribute | Value |
|-----------|-------|
| **Priority** | Should Have |
| **Story Points** | 8 |
| **Persona** | P1: System Administrator |
| **Acceptance Criteria** | See AC-040 |

---

## 6. Story Map

### Release 1 (MVP) - Must Have Stories

```
         Employee Mgmt    Incentive Config    Calculations     Approvals       Reporting       User Mgmt       Audit
         ============     ================    ============     =========       =========       =========       =====
Sprint 1  US-001          US-008             US-014           US-020          US-031          US-031          US-038
          US-002          US-009                                                              US-032

Sprint 2  US-003          US-010             US-015           US-021          US-026          US-034          US-039
          US-004                             US-016           US-025          US-027
          US-007                             US-019                           US-028
                                                                              US-029
                                                                              US-030

Sprint 3  US-006          US-011             US-017           US-022          US-033                          US-040
          US-005          US-012             US-018           US-023
                          US-013                              US-024

Sprint 4                                                                      US-035
                                                                              US-036
                                                                              US-037
```

### Story Summary

| Epic | Must Have | Should Have | Could Have | Total Stories |
|------|-----------|-------------|------------|---------------|
| EP-001: Employee Management | 4 | 2 | 0 | 7 |
| EP-002: Incentive Configuration | 3 | 2 | 1 | 6 |
| EP-003: Calculation Engine | 4 | 2 | 0 | 6 |
| EP-004: Approval Workflow | 3 | 2 | 1 | 6 |
| EP-005: Reporting & Dashboards | 5 | 0 | 0 | 5 |
| EP-006: User Management | 3 | 1 | 0 | 4 |
| EP-007: Integration | 0 | 3 | 0 | 3 |
| EP-008: Audit & Compliance | 2 | 1 | 0 | 3 |
| **Total** | **24** | **13** | **2** | **40** |

### Total Story Points

| Priority | Story Points |
|----------|--------------|
| Must Have | 152 |
| Should Have | 89 |
| Could Have | 8 |
| **Total** | **249** |

---

> *"I bent my Wookiee."* - Ralph Wiggum
>
> These user stories are our Wookiee - handle them with care!

---

**Document Approval**

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Product Owner | Skanda Prasad | _____________ | ______ |
| Business Analyst | TBD | _____________ | ______ |

---

*This document is part of the DSIF Quality Gate Framework - QG-1 Deliverable*
