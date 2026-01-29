# DORISE Sales Incentive Framework
## RACI Matrix

**Document ID:** DOC-013  
**Version:** 1.0  
**Created:** January 2025  
**Last Updated:** January 2025  
**Status:** Draft  
**Quality Gate:** QG-0 (Foundation)

---

## Table of Contents

1. [Introduction](#1-introduction)
2. [RACI Legend](#2-raci-legend)
3. [Team Roles & Responsibilities](#3-team-roles--responsibilities)
4. [Project Phase RACI Matrix](#4-project-phase-raci-matrix)
5. [Activity-Level RACI](#5-activity-level-raci)
6. [Quality Gate RACI](#6-quality-gate-raci)
7. [Escalation Matrix](#7-escalation-matrix)
8. [Document Approval](#8-document-approval)

---

## 1. Introduction

### 1.1 Purpose

This document defines the Responsibility Assignment Matrix (RACI) for the Dorise Sales Incentive Framework (DSIF) project. It establishes clear accountability for all project activities, deliverables, and decisions.

### 1.2 Scope

This RACI matrix covers:
- All project phases (Phase 0-6)
- Quality gate certifications
- Technical deliverables
- Operational activities
- Decision-making authority

### 1.3 How to Use This Document

- **Before starting any work:** Check who is Responsible (R) and Accountable (A)
- **When making decisions:** Consult (C) the appropriate stakeholders
- **After completing work:** Inform (I) relevant parties
- **For escalations:** Use the Escalation Matrix in Section 7

---

## 2. RACI Legend

| Symbol | Role | Definition |
|--------|------|------------|
| **R** | Responsible | Does the work. May be multiple people. |
| **A** | Accountable | Final decision maker. Only ONE person per activity. |
| **C** | Consulted | Provides input before work/decision. Two-way communication. |
| **I** | Informed | Notified after work/decision. One-way communication. |
| **-** | Not Involved | No participation required. |

### 2.1 RACI Rules

1. **Every activity must have exactly ONE Accountable (A)** - No shared accountability
2. **Every activity must have at least ONE Responsible (R)** - Someone must do the work
3. **Accountable (A) can also be Responsible (R)** - Same person can own and do
4. **Minimize Consulted (C)** - Too many slows decisions
5. **Informed (I) is one-way** - No response expected

---

## 3. Team Roles & Responsibilities

### 3.1 Core Team

| Role | Abbreviation | Primary Responsibilities |
|------|--------------|-------------------------|
| **Project Manager** | PM | Project coordination, timeline, risk management, stakeholder communication |
| **Product Owner** | PO | Business requirements, UAT sign-off, priority decisions, stakeholder liaison |
| **Solution Architect** | SA | Technical design, Azure architecture, security design, integration patterns |
| **Lead Developer** | LD | Code implementation, code reviews, technical standards, team guidance |
| **DevOps Engineer** | DE | CI/CD pipelines, infrastructure deployment, monitoring, production operations |
| **QA Lead** | QA | Test strategy, test execution, quality assurance, defect management |
| **Database Administrator** | DBA | Database design, performance tuning, data migration, backup strategy |

### 3.2 Extended Team

| Role | Abbreviation | Primary Responsibilities |
|------|--------------|-------------------------|
| **Business Analyst** | BA | Requirements elicitation, process documentation, gap analysis |
| **UI/UX Designer** | UX | User interface design, user experience, wireframes |
| **Security Engineer** | SE | Security reviews, vulnerability assessment, compliance |
| **Release Manager** | RM | Release coordination, change management, deployment scheduling |

### 3.3 Stakeholders

| Role | Abbreviation | Primary Responsibilities |
|------|--------------|-------------------------|
| **Executive Sponsor** | ES | Strategic direction, budget approval, executive escalation |
| **Finance Controller** | FC | Financial requirements, payout approval, audit compliance |
| **HR Manager** | HR | Employee data requirements, organizational hierarchy |
| **Sales Director** | SD | Sales process requirements, incentive plan validation |

---

## 4. Project Phase RACI Matrix

### 4.1 Phase Overview

| Phase | Activity | PM | PO | SA | LD | DE | QA | DBA |
|-------|----------|:--:|:--:|:--:|:--:|:--:|:--:|:---:|
| **Phase 0** | Project Foundation | A/R | C | C | I | I | I | I |
| | Governance Framework | A/R | C | C | I | I | I | I |
| | Team Onboarding | A/R | I | C | C | C | C | C |
| | Tool Setup | R | I | C | R | A/R | I | I |
| **Phase 1** | Requirements Gathering | C | A/R | C | C | I | C | C |
| | User Story Creation | C | A/R | C | C | I | C | I |
| | Acceptance Criteria | C | A/R | C | C | I | C | I |
| | Requirements Sign-off | C | A | C | I | I | I | I |
| **Phase 2** | Solution Architecture | C | C | A/R | C | C | I | C |
| | Data Architecture | C | C | A/R | C | I | I | A/R |
| | Security Architecture | C | I | A/R | C | C | I | C |
| | API Design | I | C | A/R | R | I | C | I |
| | Architecture Review | C | C | A | R | C | C | C |
| **Phase 3** | Technical Specifications | I | C | C | A/R | C | C | C |
| | CLAUDE.md Creation | I | I | C | A/R | I | I | I |
| | Calculation Engine Spec | I | C | C | A/R | I | C | I |
| | Integration Specifications | I | C | A | R | C | I | I |
| **Phase 4** | Infrastructure Setup | I | I | C | C | A/R | I | C |
| | Database Deployment | I | I | C | I | R | I | A/R |
| | CI/CD Pipelines | I | I | C | C | A/R | C | I |
| | Environment Configuration | I | I | C | C | A/R | I | I |
| **Phase 5** | Test Strategy | I | C | C | C | I | A/R | I |
| | Unit Testing | I | I | I | A/R | I | C | I |
| | Integration Testing | I | I | C | R | C | A/R | C |
| | UAT Coordination | C | A/R | I | C | I | R | I |
| | Performance Testing | I | I | C | C | R | A/R | C |
| **Phase 6** | Deployment Planning | C | C | C | C | A/R | C | C |
| | Production Deployment | I | A | C | C | R | C | R |
| | Monitoring Setup | I | I | C | C | A/R | I | I |
| | Runbook Creation | I | I | C | C | A/R | C | C |
| | Go-Live | C | A | C | C | R | C | C |

---

## 5. Activity-Level RACI

### 5.1 Requirements & Documentation

| Activity | PM | PO | SA | LD | DE | QA | DBA |
|----------|:--:|:--:|:--:|:--:|:--:|:--:|:---:|
| Business Requirements Document | C | A/R | C | I | I | I | I |
| Functional Requirements (FR-XXX) | C | A/R | C | C | I | C | C |
| Non-Functional Requirements | C | A | R | C | C | C | C |
| User Stories (US-XXX) | I | A/R | C | C | I | C | I |
| Acceptance Criteria | I | A/R | I | C | I | R | I |
| Process Flow Diagrams | C | A | R | C | I | I | I |
| Data Dictionary | I | C | R | C | I | I | A/R |
| API Specifications | I | C | A/R | R | I | C | I |

### 5.2 Architecture & Design

| Activity | PM | PO | SA | LD | DE | QA | DBA |
|----------|:--:|:--:|:--:|:--:|:--:|:--:|:---:|
| Solution Architecture Document | I | C | A/R | C | C | I | C |
| Component Diagrams | I | I | A/R | C | I | I | I |
| Data Flow Diagrams | I | C | A/R | C | I | I | C |
| Database Schema Design | I | C | C | C | I | I | A/R |
| Entity Relationship Diagrams | I | I | C | C | I | I | A/R |
| Security Architecture | I | I | A/R | C | C | I | C |
| Network Architecture | I | I | A/R | I | R | I | I |
| Integration Design | I | C | A/R | R | C | I | I |
| Design Decisions (ADRs) | I | C | A/R | C | C | I | C |

### 5.3 Development

| Activity | PM | PO | SA | LD | DE | QA | DBA |
|----------|:--:|:--:|:--:|:--:|:--:|:--:|:---:|
| Coding Standards Definition | I | I | C | A/R | I | C | I |
| Entity/Domain Classes | I | I | C | A/R | I | I | C |
| Repository Layer | I | I | C | A/R | I | I | C |
| Service Layer | I | I | C | A/R | I | I | I |
| API Controllers | I | I | C | A/R | I | C | I |
| Azure Functions | I | I | C | A/R | C | I | I |
| Frontend Development | I | C | C | A/R | I | C | I |
| Code Reviews | I | I | C | A/R | I | I | I |
| Technical Debt Management | I | I | C | A/R | C | I | I |

### 5.4 Database

| Activity | PM | PO | SA | LD | DE | QA | DBA |
|----------|:--:|:--:|:--:|:--:|:--:|:--:|:---:|
| Database Design | I | C | C | C | I | I | A/R |
| Table Creation Scripts | I | I | I | C | I | I | A/R |
| Index Strategy | I | I | C | C | I | I | A/R |
| Stored Procedures | I | I | I | C | I | I | A/R |
| Data Migration Scripts | I | C | C | C | C | C | A/R |
| Query Optimization | I | I | I | C | I | I | A/R |
| Backup Strategy | I | I | C | I | C | I | A/R |
| Database Monitoring | I | I | I | I | R | I | A/R |

### 5.5 Infrastructure & DevOps

| Activity | PM | PO | SA | LD | DE | QA | DBA |
|----------|:--:|:--:|:--:|:--:|:--:|:--:|:---:|
| Azure Resource Provisioning | I | I | C | I | A/R | I | I |
| Bicep/IaC Templates | I | I | C | C | A/R | I | I |
| CI Pipeline (Build) | I | I | I | C | A/R | C | I |
| CD Pipeline (Deploy) | I | I | C | C | A/R | C | I |
| Environment Management | I | I | C | I | A/R | I | I |
| Secret Management (Key Vault) | I | I | C | I | A/R | I | I |
| Monitoring & Alerting | I | I | C | C | A/R | I | C |
| Log Analytics Setup | I | I | C | C | A/R | I | I |
| Disaster Recovery Planning | C | I | A | C | R | I | R |

### 5.6 Testing

| Activity | PM | PO | SA | LD | DE | QA | DBA |
|----------|:--:|:--:|:--:|:--:|:--:|:--:|:---:|
| Test Strategy Document | I | C | C | C | I | A/R | I |
| Test Case Design | I | C | I | C | I | A/R | I |
| Unit Test Implementation | I | I | I | A/R | I | C | I |
| Integration Test Implementation | I | I | C | R | C | A/R | C |
| API Testing | I | I | I | C | I | A/R | I |
| Performance Test Execution | I | I | C | C | R | A/R | C |
| Security Testing | I | I | C | C | C | A/R | I |
| UAT Test Case Preparation | C | A | I | C | I | R | I |
| UAT Execution | C | A/R | I | C | I | C | I |
| Defect Triage | C | A | I | R | I | R | I |
| Test Automation | I | I | I | C | C | A/R | I |

### 5.7 Deployment & Operations

| Activity | PM | PO | SA | LD | DE | QA | DBA |
|----------|:--:|:--:|:--:|:--:|:--:|:--:|:---:|
| Deployment Runbook | I | I | C | C | A/R | C | C |
| Release Planning | C | A | C | C | R | C | C |
| Change Request Submission | C | A | C | C | R | I | I |
| Production Deployment | I | A | C | C | R | C | R |
| Smoke Testing (Post-Deploy) | I | I | I | C | C | A/R | I |
| Rollback Execution | I | A | C | C | R | I | R |
| Incident Response | I | I | C | R | A/R | I | R |
| Post-Incident Review | C | C | C | R | A/R | I | C |
| Operational Monitoring | I | I | I | I | A/R | I | C |

---

## 6. Quality Gate RACI

### 6.1 Gate Certification Matrix

| Gate | Certification Owner | PM | PO | SA | LD | DE | QA | DBA |
|------|---------------------|:--:|:--:|:--:|:--:|:--:|:--:|:---:|
| **QG-0: Foundation** | Project Manager | A | C | C | I | I | I | I |
| **QG-1: Requirements** | Product Owner | C | A | C | I | I | C | I |
| **QG-2: Architecture** | Solution Architect | C | C | A | C | C | I | C |
| **QG-3: Technical** | Lead Developer | I | C | C | A | C | C | C |
| **QG-4: Infrastructure** | DevOps Engineer | I | I | C | C | A | C | C |
| **QG-5: Testing** | QA Lead | I | C | I | C | C | A | I |
| **QG-6: Operations** | DevOps + PO | C | A | C | C | A | C | C |

### 6.2 Gate Review Participation

| Activity | PM | PO | SA | LD | DE | QA | DBA |
|----------|:--:|:--:|:--:|:--:|:--:|:--:|:---:|
| Gate Checklist Completion | R | R | R | R | R | R | R |
| Evidence Collection | R | R | R | R | R | R | R |
| Gate Review Meeting | A/R | R | R | R | R | R | R |
| Gate Certification Sign-off | C | C | C | C | C | C | C |
| Gate Status Update | A/R | I | I | I | I | I | I |
| Exception Request | R | R | R | R | R | R | R |
| Exception Approval | - | A* | A* | A* | A* | A* | - |

*Exception approval by the Gate Owner only

---

## 7. Escalation Matrix

### 7.1 Escalation Levels

| Level | Trigger | Escalation Path | Response Time |
|-------|---------|-----------------|---------------|
| **L1** | Team-level blockers | Lead Developer / QA Lead | 4 hours |
| **L2** | Cross-team conflicts | Solution Architect / Project Manager | 8 hours |
| **L3** | Architecture/Design disputes | Solution Architect + Product Owner | 24 hours |
| **L4** | Project-level risks | Project Manager + Executive Sponsor | 48 hours |
| **L5** | Strategic/Budget issues | Executive Sponsor | 72 hours |

### 7.2 Escalation by Category

| Category | L1 Contact | L2 Contact | L3 Contact |
|----------|------------|------------|------------|
| **Technical Issues** | Lead Developer | Solution Architect | Executive Sponsor |
| **Requirements Issues** | Business Analyst | Product Owner | Executive Sponsor |
| **Infrastructure Issues** | DevOps Engineer | Solution Architect | Executive Sponsor |
| **Quality Issues** | QA Lead | Project Manager | Executive Sponsor |
| **Resource Issues** | Project Manager | Executive Sponsor | - |
| **Schedule Issues** | Project Manager | Product Owner | Executive Sponsor |

### 7.3 Escalation Process

```
┌─────────────────────────────────────────────────────────────────────────┐
│                        ESCALATION PROCESS                               │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  1. IDENTIFY    → Document the issue clearly                           │
│                   - What is the problem?                                │
│                   - What is the impact?                                 │
│                   - What has been tried?                                │
│                                                                         │
│  2. ATTEMPT     → Try to resolve at current level                      │
│                   - Discuss with immediate team                         │
│                   - Document attempted solutions                        │
│                                                                         │
│  3. ESCALATE    → If unresolved within response time                   │
│                   - Notify next level via Teams/Email                   │
│                   - Include all documentation                           │
│                   - Set up meeting if needed                            │
│                                                                         │
│  4. RESOLVE     → Work with escalation contact                         │
│                   - Implement solution                                  │
│                   - Document resolution                                 │
│                                                                         │
│  5. CLOSE       → Update stakeholders                                  │
│                   - Inform all impacted parties                         │
│                   - Update project tracking                             │
│                   - Capture lessons learned                             │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## 8. Document Approval

### 8.1 Approval Signatures

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Project Manager | Claude Code | AI Agent | January 2025 |
| Product Owner | Skanda Prasad | _____________ | ______ |
| Solution Architect | Skanda Prasad | _____________ | ______ |

### 8.2 Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | January 2025 | Skanda Prasad | Initial document creation |
| 1.1 | January 2025 | Claude Code | Updated team assignments and contact directory |

### 8.3 Review Schedule

This document should be reviewed:
- At the start of each project phase
- When team composition changes
- When significant scope changes occur
- Quarterly at minimum

---

## Appendix A: RACI Quick Reference Card

```
┌─────────────────────────────────────────────────────────────────────────┐
│                     RACI QUICK REFERENCE                                │
├─────────────────────────────────────────────────────────────────────────┤
│                                                                         │
│  REQUIREMENTS      → Product Owner (A)                                 │
│  ARCHITECTURE      → Solution Architect (A)                            │
│  DEVELOPMENT       → Lead Developer (A)                                │
│  DATABASE          → DBA (A)                                           │
│  INFRASTRUCTURE    → DevOps Engineer (A)                               │
│  TESTING           → QA Lead (A)                                       │
│  PROJECT           → Project Manager (A)                               │
│                                                                         │
│  ─────────────────────────────────────────────────────────────────     │
│                                                                         │
│  GATE OWNERS:                                                          │
│  QG-0 Foundation     → Project Manager                                 │
│  QG-1 Requirements   → Product Owner                                   │
│  QG-2 Architecture   → Solution Architect                              │
│  QG-3 Technical      → Lead Developer                                  │
│  QG-4 Infrastructure → DevOps Engineer                                 │
│  QG-5 Testing        → QA Lead                                         │
│  QG-6 Operations     → DevOps + Product Owner                          │
│                                                                         │
└─────────────────────────────────────────────────────────────────────────┘
```

---

## Appendix B: Contact Directory

| Role | Name | Email | Teams Handle |
|------|------|-------|--------------|
| Project Manager | Claude Code | AI Agent | - |
| Product Owner | Skanda Prasad | skanda.prasad@diligentglobal.com | @Skanda Prasad |
| Solution Architect | Skanda Prasad | skanda.prasad@diligentglobal.com | @Skanda Prasad |
| Lead Developer | Claude Code | AI Agent | - |
| DevOps Engineer | Claude Code | AI Agent | - |
| QA Lead | Claude Code | AI Agent | - |
| DBA | Claude Code | AI Agent | - |
| Executive Sponsor | Farhan Mubashir | farhan.mubashir@diligentglobal.com | @Farhan Mubashir |

---

**Document ID:** DOC-013  
**Classification:** Internal  
**Quality Gate:** QG-0 (Foundation)  
**Location:** `/docs/design/RACI.md`

*This document is part of the Dorise Sales Incentive Framework project documentation.*
