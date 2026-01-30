# DORISE Sales Incentive Framework
## Requirements Specification

**Document ID:** DOC-REQ-001
**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Created:** January 2025
**Last Updated:** January 2025
**Author:** Claude Code
**Status:** Draft

> *"I'm learnding!"* - Ralph Wiggum
>
> Like Ralph, we're learning what our users truly need. This document captures those learnings.

---

## Table of Contents

1. [Document Control](#document-control)
2. [Executive Summary](#executive-summary)
3. [Scope](#scope)
4. [Functional Requirements](#functional-requirements)
5. [Non-Functional Requirements](#non-functional-requirements)
6. [Business Rules](#business-rules)
7. [Data Requirements](#data-requirements)
8. [Integration Requirements](#integration-requirements)
9. [Assumptions and Constraints](#assumptions-and-constraints)
10. [Glossary](#glossary)

---

## 1. Document Control

### Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | January 2025 | Claude Code | Initial requirements document |

### Reviewers

| Role | Name | Review Date | Status |
|------|------|-------------|--------|
| Product Owner | Skanda Prasad | Pending | - |
| Solution Architect | Skanda Prasad | Pending | - |
| Project Sponsor | Farhan Mubashir | Pending | - |

---

## 2. Executive Summary

The Dorise Sales Incentive Framework (DSIF) is designed to automate and manage sales incentive calculations for the organization. The current manual process is error-prone, lacks audit trails, and creates compliance risks.

### Business Objectives

| ID | Objective | Success Metric |
|----|-----------|----------------|
| BO-1 | Automate incentive calculations | 100% automated calculation coverage |
| BO-2 | Establish audit compliance | Complete audit trail for all transactions |
| BO-3 | Reduce calculation errors | <0.1% error rate |
| BO-4 | Enable self-service configuration | Business users can modify plans without IT |
| BO-5 | Integrate with external systems | API-based integration with payroll/ERP |

### Key Business Drivers

1. **Compliance Risk** - Auditors require immutable calculation records
2. **Operational Efficiency** - Manual processes consume 40+ hours per pay period
3. **Data Integrity** - Formula replication across systems causes inconsistencies
4. **Scalability** - Current Excel-based system cannot scale with business growth

---

## 3. Scope

### In Scope

| Category | Items |
|----------|-------|
| **Employee Management** | Hierarchical structure, role assignments, split shares |
| **Incentive Configuration** | Plans, slabs, thresholds, formulas |
| **Calculation Engine** | Automated batch processing, point-in-time snapshots |
| **Approval Workflow** | Multi-level approvals, status tracking |
| **Reporting** | Dashboards, exports, historical views |
| **Integration** | REST APIs for external systems |
| **Audit Trail** | Immutable records for all operations |

### Out of Scope

| Category | Reason |
|----------|--------|
| Payroll Processing | Handled by external payroll system |
| Tax Calculations | Performed by payroll system post-integration |
| Performance Reviews | Separate HR system responsibility |
| Recruitment | Not related to incentive calculations |

> *"When I grow up, I want to be a principal or a caterpillar."* - Ralph Wiggum
>
> Our scope is intentionally focused - we're incentives specialists, not everything specialists.

---

## 4. Functional Requirements

### 4.1 Employee Management (FR-EMP)

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| FR-EMP-001 | System shall maintain hierarchical employee structure (Company > Zone > Store > Employee) | P1 - Critical | Foundation for incentive allocation |
| FR-EMP-002 | System shall track employee role and designation changes with effective dates | P1 - Critical | Required for historical accuracy |
| FR-EMP-003 | System shall support split incentive shares between multiple employees | P1 - Critical | Common scenario for shared territories |
| FR-EMP-004 | System shall maintain assignment history with point-in-time retrieval | P1 - Critical | Audit requirement |
| FR-EMP-005 | System shall support employee status tracking (Active, Inactive, Terminated) | P2 - High | Payroll integration requirement |
| FR-EMP-006 | System shall allow bulk employee import via Excel/CSV | P2 - High | Initial data load and updates |
| FR-EMP-007 | System shall validate employee data before saving | P2 - High | Data integrity |
| FR-EMP-008 | System shall support employee search by name, ID, store, zone | P3 - Medium | Usability requirement |

### 4.2 Incentive Configuration (FR-INC)

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| FR-INC-001 | System shall support configurable incentive plans by role | P1 - Critical | Core functionality |
| FR-INC-002 | System shall support threshold-based slabs (sales volume, production, sell-through rate) | P1 - Critical | Business calculation requirement |
| FR-INC-003 | System shall allow formula modifications through configuration (no code changes) | P1 - Critical | Self-service requirement |
| FR-INC-004 | System shall maintain version history for all plan changes | P1 - Critical | Audit and rollback capability |
| FR-INC-005 | System shall support effective date ranges for plan versions | P1 - Critical | Historical accuracy |
| FR-INC-006 | System shall validate plan configurations before activation | P2 - High | Prevent calculation errors |
| FR-INC-007 | System shall support plan templates for common structures | P3 - Medium | Usability improvement |
| FR-INC-008 | System shall allow plan comparison between versions | P3 - Medium | Change management support |
| FR-INC-009 | System shall support plan approval workflow before activation | P2 - High | Change control |

### 4.3 Calculation Engine (FR-CALC)

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| FR-CALC-001 | System shall perform automated batch calculations (daily/weekly/monthly) | P1 - Critical | Core automation requirement |
| FR-CALC-002 | System shall capture point-in-time values for all calculation inputs | P1 - Critical | Audit and accuracy requirement |
| FR-CALC-003 | System shall create immutable calculation records | P1 - Critical | Compliance requirement |
| FR-CALC-004 | System shall support manual recalculation with amendment tracking | P1 - Critical | Error correction capability |
| FR-CALC-005 | System shall calculate incentives based on configured slab thresholds | P1 - Critical | Core business logic |
| FR-CALC-006 | System shall handle split shares according to configured percentages | P1 - Critical | Multi-employee scenarios |
| FR-CALC-007 | System shall log all calculation steps for debugging | P2 - High | Troubleshooting support |
| FR-CALC-008 | System shall support calculation preview without committing | P2 - High | Verification capability |
| FR-CALC-009 | System shall handle proration for mid-period changes | P2 - High | Employee change scenarios |
| FR-CALC-010 | System shall support clawback calculations for adjustments | P2 - High | Business requirement |

> *"I choo-choo-choose you!"* - Ralph Wiggum
>
> Our calculation engine carefully chooses the right formula for each employee's situation.

### 4.4 Approval Workflow (FR-APPR)

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| FR-APPR-001 | System shall support multi-level approval workflow | P1 - Critical | Governance requirement |
| FR-APPR-002 | System shall track status (Pending, Approved, Rejected, On Hold) | P1 - Critical | Process tracking |
| FR-APPR-003 | System shall capture approver details and timestamps | P1 - Critical | Audit trail |
| FR-APPR-004 | System shall support approval delegation | P2 - High | Vacation/absence scenarios |
| FR-APPR-005 | System shall send notifications for pending approvals | P2 - High | Process efficiency |
| FR-APPR-006 | System shall support bulk approval operations | P2 - High | Efficiency improvement |
| FR-APPR-007 | System shall allow rejection with mandatory comments | P1 - Critical | Process documentation |
| FR-APPR-008 | System shall support escalation after configurable timeout | P3 - Medium | SLA management |
| FR-APPR-009 | System shall maintain approval chain history | P1 - Critical | Audit requirement |

### 4.5 Reporting (FR-RPT)

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| FR-RPT-001 | System shall provide executive dashboards with key metrics | P1 - Critical | Management visibility |
| FR-RPT-002 | System shall support historical incentive record viewing | P1 - Critical | Audit and analysis |
| FR-RPT-003 | System shall support export functionality (Excel, PDF) | P1 - Critical | External consumption |
| FR-RPT-004 | System shall support drill-down from summary to detail | P2 - High | Analysis capability |
| FR-RPT-005 | System shall support custom date range filtering | P2 - High | Flexible analysis |
| FR-RPT-006 | System shall provide calculation breakdown reports | P2 - High | Transparency requirement |
| FR-RPT-007 | System shall support scheduled report generation | P3 - Medium | Automation |
| FR-RPT-008 | System shall provide audit trail reports | P1 - Critical | Compliance requirement |
| FR-RPT-009 | System shall support report templates | P3 - Medium | Usability |

### 4.6 User Management (FR-USER)

| ID | Requirement | Priority | Notes |
|----|-------------|----------|-------|
| FR-USER-001 | System shall integrate with Azure Entra ID for authentication | P1 - Critical | Enterprise SSO |
| FR-USER-002 | System shall support role-based access control (RBAC) | P1 - Critical | Security requirement |
| FR-USER-003 | System shall define user roles (Admin, Manager, Approver, Viewer) | P1 - Critical | Access control |
| FR-USER-004 | System shall log user activities for audit | P1 - Critical | Security audit |
| FR-USER-005 | System shall support scope-based access (by zone, store) | P2 - High | Data isolation |
| FR-USER-006 | System shall support session management | P2 - High | Security requirement |

---

## 5. Non-Functional Requirements

### 5.1 Performance (NFR-PERF)

| ID | Requirement | Target | Notes |
|----|-------------|--------|-------|
| NFR-PERF-001 | Page load time | < 3 seconds | 90th percentile |
| NFR-PERF-002 | API response time | < 500ms | Average for CRUD operations |
| NFR-PERF-003 | Batch calculation throughput | 10,000 employees/hour | Monthly calculation batch |
| NFR-PERF-004 | Report generation time | < 30 seconds | Large reports (1 year data) |
| NFR-PERF-005 | Concurrent users supported | 100 simultaneous users | Peak usage scenario |
| NFR-PERF-006 | Database query time | < 200ms | 95th percentile |

### 5.2 Availability (NFR-AVAIL)

| ID | Requirement | Target | Notes |
|----|-------------|--------|-------|
| NFR-AVAIL-001 | System uptime | 99.9% | Excluding planned maintenance |
| NFR-AVAIL-002 | Planned maintenance window | < 4 hours/month | Weekend window preferred |
| NFR-AVAIL-003 | Recovery Time Objective (RTO) | < 4 hours | Disaster recovery |
| NFR-AVAIL-004 | Recovery Point Objective (RPO) | < 1 hour | Data loss tolerance |
| NFR-AVAIL-005 | Backup frequency | Daily full, hourly incremental | Data protection |

### 5.3 Security (NFR-SEC)

| ID | Requirement | Target | Notes |
|----|-------------|--------|-------|
| NFR-SEC-001 | Authentication | Azure Entra ID + MFA | Enterprise standard |
| NFR-SEC-002 | Data encryption at rest | AES-256 | Azure SQL TDE |
| NFR-SEC-003 | Data encryption in transit | TLS 1.3 | All communications |
| NFR-SEC-004 | Secrets management | Azure Key Vault | No hardcoded secrets |
| NFR-SEC-005 | Audit logging retention | 7 years | Compliance requirement |
| NFR-SEC-006 | Vulnerability scanning | Monthly | OWASP Top 10 coverage |
| NFR-SEC-007 | Penetration testing | Annual | Third-party assessment |
| NFR-SEC-008 | Access review | Quarterly | RBAC validation |

> *"Me fail English? That's unpossible!"* - Ralph Wiggum
>
> Security failures in our system are also unpossible - we follow enterprise-grade security standards.

### 5.4 Scalability (NFR-SCALE)

| ID | Requirement | Target | Notes |
|----|-------------|--------|-------|
| NFR-SCALE-001 | Employee capacity | 50,000 employees | Initial design capacity |
| NFR-SCALE-002 | Transaction history | 5 years online | Older data archived |
| NFR-SCALE-003 | Horizontal scaling | Auto-scale on demand | Azure App Service |
| NFR-SCALE-004 | Database growth | 100GB/year | Capacity planning |

### 5.5 Maintainability (NFR-MAINT)

| ID | Requirement | Target | Notes |
|----|-------------|--------|-------|
| NFR-MAINT-001 | Code test coverage | >= 80% | Business logic |
| NFR-MAINT-002 | Documentation | 100% public API coverage | Swagger/OpenAPI |
| NFR-MAINT-003 | Deployment frequency | Weekly capable | CI/CD pipeline |
| NFR-MAINT-004 | Deployment rollback | < 15 minutes | Blue-green deployment |
| NFR-MAINT-005 | Log aggregation | Centralized logging | Application Insights |
| NFR-MAINT-006 | Monitoring alerts | < 5 min detection | Critical issues |

### 5.6 Compliance (NFR-COMP)

| ID | Requirement | Target | Notes |
|----|-------------|--------|-------|
| NFR-COMP-001 | Data residency | Region-specific | Azure region selection |
| NFR-COMP-002 | Audit trail immutability | Tamper-proof records | Legal requirement |
| NFR-COMP-003 | Data retention | 7 years | Financial records |
| NFR-COMP-004 | GDPR compliance | Full compliance | EU data subjects |
| NFR-COMP-005 | SOC 2 readiness | Type II capable | Customer requirement |

---

## 6. Business Rules

### 6.1 Calculation Rules (BR-CALC)

| ID | Rule | Description |
|----|------|-------------|
| BR-CALC-001 | Slab Selection | System shall select the highest qualifying slab based on achievement |
| BR-CALC-002 | Proration | Mid-period changes shall be prorated based on days active |
| BR-CALC-003 | Split Shares | Split percentages must sum to 100% for shared territories |
| BR-CALC-004 | Minimum Threshold | Employees below minimum threshold receive zero incentive |
| BR-CALC-005 | Cap Application | Maximum incentive cap shall be applied after calculation |
| BR-CALC-006 | Rounding | All currency values rounded to 2 decimal places |
| BR-CALC-007 | Negative Adjustments | Clawbacks cannot exceed previously paid amounts |

### 6.2 Approval Rules (BR-APPR)

| ID | Rule | Description |
|----|------|-------------|
| BR-APPR-001 | Amount Threshold | Amounts > $10,000 require senior manager approval |
| BR-APPR-002 | Self-Approval | Users cannot approve their own incentives |
| BR-APPR-003 | Rejection Reason | Rejections must include a reason (minimum 10 characters) |
| BR-APPR-004 | Amendment Approval | All amendments require re-approval |
| BR-APPR-005 | Timeout | Pending approvals escalate after 7 business days |

### 6.3 Data Rules (BR-DATA)

| ID | Rule | Description |
|----|------|-------------|
| BR-DATA-001 | Effective Date | Future-dated records allowed; past-dated records require approval |
| BR-DATA-002 | Deletion | Soft delete only; no physical deletions |
| BR-DATA-003 | Duplicate Prevention | Employee assignments cannot overlap for same period |
| BR-DATA-004 | Historical Integrity | Committed calculations cannot be modified, only amended |
| BR-DATA-005 | Data Quality | Required fields must be populated before save |

---

## 7. Data Requirements

### 7.1 Data Entities

| Entity | Description | Volume Estimate |
|--------|-------------|-----------------|
| Employee | Sales personnel records | 10,000 active |
| IncentivePlan | Calculation configurations | 50 active plans |
| PlanSlab | Threshold-based slabs | 500 slabs |
| Calculation | Monthly calculation results | 120,000/year |
| CalculationDetail | Line-item breakdowns | 1.2M/year |
| Approval | Approval workflow records | 120,000/year |
| AuditLog | All system activities | 5M/year |

### 7.2 Data Retention

| Data Type | Retention Period | Archive Strategy |
|-----------|------------------|------------------|
| Active employee data | Indefinite | N/A |
| Calculation records | 7 years online | Archive to cold storage |
| Audit logs | 7 years online | Archive to cold storage |
| System logs | 90 days | Delete after period |
| Session data | 24 hours | Delete after expiry |

### 7.3 Data Quality Requirements

| Requirement | Description |
|-------------|-------------|
| Completeness | All required fields must be populated |
| Accuracy | Data validated against business rules |
| Consistency | Cross-entity references must be valid |
| Timeliness | Source data must be < 24 hours old for calculations |
| Uniqueness | No duplicate employee records |

---

## 8. Integration Requirements

### 8.1 External System Integrations

| System | Direction | Method | Frequency | Data |
|--------|-----------|--------|-----------|------|
| Payroll System | Outbound | REST API | Per pay period | Incentive amounts |
| ERP System | Inbound | REST API | Daily | Sales data |
| HR System | Inbound | REST API | Real-time | Employee changes |
| AD/Entra ID | Bidirectional | OIDC/SCIM | Real-time | Authentication |

### 8.2 API Requirements

| ID | Requirement | Notes |
|----|-------------|-------|
| INT-API-001 | RESTful API design | OpenAPI 3.0 specification |
| INT-API-002 | API versioning | URL-based (v1, v2) |
| INT-API-003 | Rate limiting | 1000 requests/minute per client |
| INT-API-004 | API authentication | OAuth 2.0 / JWT tokens |
| INT-API-005 | Error handling | Standard HTTP status codes + error details |
| INT-API-006 | Pagination | Offset-based with configurable page size |
| INT-API-007 | Bulk operations | Support batch create/update |

> *"Hi, Super Nintendo Chalmers!"* - Ralph Wiggum
>
> Our APIs are designed to be friendly and approachable - even to systems that get names wrong.

---

## 9. Assumptions and Constraints

### 9.1 Assumptions

| ID | Assumption | Impact if Invalid |
|----|------------|-------------------|
| ASM-001 | Azure subscription will be available | Project timeline delay |
| ASM-002 | Source data quality is acceptable | Additional cleansing effort |
| ASM-003 | Stakeholders available for requirements validation | Requirement gaps |
| ASM-004 | Current calculation logic is documented | Additional discovery needed |
| ASM-005 | Users have modern browsers | UI compatibility issues |
| ASM-006 | Network connectivity to Azure is stable | Performance degradation |

### 9.2 Constraints

| ID | Constraint | Mitigation |
|----|------------|------------|
| CON-001 | Budget limit of $X per month for Azure resources | Cost optimization design |
| CON-002 | Go-live deadline of August 2025 | Phased delivery approach |
| CON-003 | Must integrate with existing payroll system | API-based integration |
| CON-004 | Data must remain in specific region | Azure region selection |
| CON-005 | Must support existing calculation formulas | Configuration-based formulas |

### 9.3 Dependencies

| ID | Dependency | Owner | Risk |
|----|------------|-------|------|
| DEP-001 | Azure subscription provisioning | IT Operations | Medium |
| DEP-002 | Entra ID app registration | Security Team | Low |
| DEP-003 | Source system API availability | IT Operations | High |
| DEP-004 | User acceptance testing resources | Business Team | Medium |
| DEP-005 | Production deployment approval | Change Board | Low |

---

## 10. Glossary

| Term | Definition |
|------|------------|
| **Incentive Plan** | A configured set of rules defining how incentives are calculated for a role |
| **Slab** | A threshold-based tier that determines incentive percentage or amount |
| **Split Share** | The percentage allocation of an incentive between multiple employees |
| **Proration** | Adjustment of incentive based on partial period participation |
| **Clawback** | Recovery of previously paid incentive due to adjustments |
| **Point-in-Time** | Capturing data values as they existed at a specific moment |
| **Immutable Record** | A record that cannot be modified after creation |
| **Amendment** | A new record that adjusts a previous calculation |
| **Sell-Through Rate** | Ratio of units sold to units available |
| **Achievement** | Employee performance metric used for slab selection |

---

## Appendix A: Priority Definitions

| Priority | Definition | SLA |
|----------|------------|-----|
| **P1 - Critical** | Must have for go-live; system unusable without it | Phase 1 delivery |
| **P2 - High** | Should have; significant impact on user experience | Phase 2 delivery |
| **P3 - Medium** | Nice to have; enhances user experience | Post go-live |
| **P4 - Low** | Future consideration | Backlog |

---

## Appendix B: Requirement Traceability

This section will be completed during the design phase to map requirements to design elements and test cases.

| Requirement ID | Design Element | Test Case(s) |
|----------------|----------------|--------------|
| FR-EMP-001 | TBD | TBD |
| FR-INC-001 | TBD | TBD |
| ... | ... | ... |

---

**Document Approval**

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Product Owner | Skanda Prasad | _____________ | ______ |
| Solution Architect | Skanda Prasad | _____________ | ______ |
| Project Sponsor | Farhan Mubashir | _____________ | ______ |

---

> *"Go Banana!"* - Ralph Wiggum
>
> With these requirements defined, we're ready to go forward with architecture design!

---

*This document is part of the DSIF Quality Gate Framework - QG-1 Deliverable*
