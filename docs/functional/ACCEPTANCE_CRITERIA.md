# DORISE Sales Incentive Framework
## Acceptance Criteria

**Document ID:** DOC-REQ-003
**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Created:** January 2025
**Last Updated:** January 2025
**Author:** Claude Code
**Status:** Draft

> *"I'm a unitard!"* - Ralph Wiggum
>
> Our acceptance criteria are unified and standardized - a single source of truth for what "done" means.

---

## Table of Contents

1. [Document Control](#document-control)
2. [Overview](#overview)
3. [Acceptance Criteria Format](#acceptance-criteria-format)
4. [EP-001: Employee Management](#ep-001-employee-management)
5. [EP-002: Incentive Configuration](#ep-002-incentive-configuration)
6. [EP-003: Calculation Engine](#ep-003-calculation-engine)
7. [EP-004: Approval Workflow](#ep-004-approval-workflow)
8. [EP-005: Reporting & Dashboards](#ep-005-reporting--dashboards)
9. [EP-006: User Management](#ep-006-user-management)
10. [EP-007: Integration](#ep-007-integration)
11. [EP-008: Audit & Compliance](#ep-008-audit--compliance)

---

## 1. Document Control

### Version History

| Version | Date | Author | Changes |
|---------|------|--------|---------|
| 1.0 | January 2025 | Claude Code | Initial acceptance criteria document |

### Reviewers

| Role | Name | Review Date | Status |
|------|------|-------------|--------|
| Product Owner | Skanda Prasad | Pending | - |
| QA Lead | Claude Code | Pending | - |

---

## 2. Overview

This document defines the detailed acceptance criteria for all user stories in the DSIF project. Acceptance criteria are the specific conditions that must be met for a user story to be considered complete.

### Purpose

- Define unambiguous conditions for story completion
- Provide basis for test case development
- Ensure shared understanding between development and business
- Enable objective acceptance testing

---

## 3. Acceptance Criteria Format

Each acceptance criterion follows the Given-When-Then format:

```
GIVEN [precondition/context]
WHEN [action is performed]
THEN [expected outcome]
```

Additional attributes:
- **Priority**: Critical / Important / Nice to Have
- **Testable**: Yes / No
- **Automated**: Yes / No / Partial

---

## EP-001: Employee Management

### AC-001: View Employee Hierarchy (US-001)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-001-01 | **GIVEN** I am logged in as a Sales Manager<br>**WHEN** I navigate to the Employee section<br>**THEN** I see a hierarchical tree view showing Company > Zone > Store > Employee | Critical |
| AC-001-02 | **GIVEN** I am viewing the hierarchy<br>**WHEN** I expand a node (Zone or Store)<br>**THEN** I see its child nodes with employee counts | Critical |
| AC-001-03 | **GIVEN** I am viewing the hierarchy<br>**WHEN** I click on an employee<br>**THEN** I see their profile summary in a side panel | Important |
| AC-001-04 | **GIVEN** I have scope-based access to specific zones<br>**WHEN** I view the hierarchy<br>**THEN** I only see zones within my access scope | Critical |
| AC-001-05 | **GIVEN** the hierarchy has more than 100 nodes<br>**WHEN** I expand all nodes<br>**THEN** the page remains responsive (load time < 3 seconds) | Important |

---

### AC-002: Add New Employee (US-002)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-002-01 | **GIVEN** I am logged in as a System Administrator<br>**WHEN** I click "Add Employee"<br>**THEN** I see a form with fields: Employee ID, Name, Email, Role, Designation, Zone, Store, Start Date | Critical |
| AC-002-02 | **GIVEN** I am on the Add Employee form<br>**WHEN** I submit with all required fields completed<br>**THEN** the employee is created and appears in the hierarchy | Critical |
| AC-002-03 | **GIVEN** I am on the Add Employee form<br>**WHEN** I submit with missing required fields<br>**THEN** I see validation errors for each missing field | Critical |
| AC-002-04 | **GIVEN** I enter an Employee ID that already exists<br>**WHEN** I submit the form<br>**THEN** I see an error "Employee ID already exists" | Critical |
| AC-002-05 | **GIVEN** I successfully add an employee<br>**WHEN** I check the audit log<br>**THEN** I see an entry showing who created the employee and when | Critical |
| AC-002-06 | **GIVEN** I enter a Start Date in the past<br>**WHEN** I submit the form<br>**THEN** I am prompted for approval (past-dated records require approval) | Important |

---

### AC-003: Configure Split Shares (US-003)

> *"Go Banana!"* - Ralph Wiggum
>
> Splitting incentives is like sharing a banana - everyone should get their fair share!

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-003-01 | **GIVEN** I am on an employee's assignment page<br>**WHEN** I click "Configure Split"<br>**THEN** I can add other employees to share the incentive | Critical |
| AC-003-02 | **GIVEN** I am configuring a split<br>**WHEN** I assign percentages that total 100%<br>**THEN** I can save the configuration | Critical |
| AC-003-03 | **GIVEN** I am configuring a split<br>**WHEN** I assign percentages that do NOT total 100%<br>**THEN** I see an error "Split percentages must total 100%" | Critical |
| AC-003-04 | **GIVEN** a split is configured with effective dates<br>**WHEN** a calculation runs for that period<br>**THEN** each employee receives their configured percentage | Critical |
| AC-003-05 | **GIVEN** a split has overlapping effective dates with another split<br>**WHEN** I try to save<br>**THEN** I see an error "Overlapping split configurations detected" | Critical |

---

### AC-004: View Assignment History (US-004)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-004-01 | **GIVEN** I am viewing an employee profile<br>**WHEN** I click "Assignment History"<br>**THEN** I see a timeline of all assignments with effective dates | Critical |
| AC-004-02 | **GIVEN** I am viewing assignment history<br>**WHEN** I click on a specific assignment<br>**THEN** I see the full details: Zone, Store, Role, Split %, Effective From/To | Critical |
| AC-004-03 | **GIVEN** an employee has been transferred multiple times<br>**WHEN** I view their assignment history<br>**THEN** I see all transfers in chronological order | Critical |
| AC-004-04 | **GIVEN** I am viewing assignment history<br>**WHEN** I export the history<br>**THEN** I receive an Excel file with all assignment records | Important |
| AC-004-05 | **GIVEN** I query a point-in-time date<br>**WHEN** the system retrieves the assignment<br>**THEN** it returns the assignment that was active on that exact date | Critical |

---

### AC-005: Bulk Import Employees (US-005)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-005-01 | **GIVEN** I am on the Employee Management page<br>**WHEN** I click "Import"<br>**THEN** I can upload an Excel or CSV file | Critical |
| AC-005-02 | **GIVEN** I upload a file with the correct template<br>**WHEN** the system processes it<br>**THEN** valid rows are imported and a summary shows success/failure counts | Critical |
| AC-005-03 | **GIVEN** I upload a file with some invalid rows<br>**WHEN** the system processes it<br>**THEN** valid rows are imported and errors are listed for each failed row | Critical |
| AC-005-04 | **GIVEN** I need the import template<br>**WHEN** I click "Download Template"<br>**THEN** I receive an Excel file with correct headers and sample data | Important |
| AC-005-05 | **GIVEN** I upload a file with 1000+ rows<br>**WHEN** the system processes it<br>**THEN** the import completes within 5 minutes | Important |
| AC-005-06 | **GIVEN** an import is in progress<br>**WHEN** I check the status<br>**THEN** I see a progress indicator showing rows processed | Nice to Have |

---

### AC-006: Search Employees (US-006)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-006-01 | **GIVEN** I am on the Employee page<br>**WHEN** I enter a partial name in the search box<br>**THEN** I see matching employees as I type (autocomplete) | Critical |
| AC-006-02 | **GIVEN** I search by Employee ID<br>**WHEN** I enter the exact ID<br>**THEN** I see the matching employee immediately | Critical |
| AC-006-03 | **GIVEN** I want to filter by Store<br>**WHEN** I select a Store from the filter dropdown<br>**THEN** I see only employees assigned to that store | Important |
| AC-006-04 | **GIVEN** I apply multiple filters (Zone AND Role)<br>**WHEN** I search<br>**THEN** results match ALL applied filters | Important |
| AC-006-05 | **GIVEN** no results match my search<br>**WHEN** I view the results<br>**THEN** I see "No employees found" with suggestions to modify my search | Important |

---

### AC-007: Update Employee Status (US-007)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-007-01 | **GIVEN** I am viewing an employee profile<br>**WHEN** I change their status to "Inactive" with an effective date<br>**THEN** the status is updated and calculations exclude them from that date | Critical |
| AC-007-02 | **GIVEN** I terminate an employee<br>**WHEN** the effective date passes<br>**THEN** they no longer appear in active employee lists | Critical |
| AC-007-03 | **GIVEN** I re-activate a previously terminated employee<br>**WHEN** I set status to "Active"<br>**THEN** they are included in calculations from the effective date | Critical |
| AC-007-04 | **GIVEN** I change an employee's status<br>**WHEN** I check the audit log<br>**THEN** I see the status change with who made it and when | Critical |
| AC-007-05 | **GIVEN** an employee has pending incentives when terminated<br>**WHEN** I terminate them<br>**THEN** I am warned that pending incentives exist | Important |

---

## EP-002: Incentive Configuration

### AC-008: Create Incentive Plan (US-008)

> *"Even my boogers are spicy!"* - Ralph Wiggum
>
> Like Ralph's boogers, our incentive plans can have many flavors - each one uniquely configured!

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-008-01 | **GIVEN** I am on the Incentive Plans page<br>**WHEN** I click "Create New Plan"<br>**THEN** I see a wizard to configure: Plan Name, Description, Applicable Roles, Effective Dates | Critical |
| AC-008-02 | **GIVEN** I am creating a plan<br>**WHEN** I save the plan without slabs<br>**THEN** I see an error "At least one slab is required" | Critical |
| AC-008-03 | **GIVEN** I create a plan with all required fields<br>**WHEN** I save<br>**THEN** the plan is created in "Draft" status | Critical |
| AC-008-04 | **GIVEN** I create a plan<br>**WHEN** I check the audit log<br>**THEN** I see who created the plan and when | Critical |
| AC-008-05 | **GIVEN** I am creating a plan<br>**WHEN** I select a formula type (Percentage, Fixed, Tiered)<br>**THEN** the appropriate configuration options are displayed | Critical |
| AC-008-06 | **GIVEN** a plan with the same name already exists<br>**WHEN** I try to create another with that name<br>**THEN** I see an error "Plan name already exists" | Important |

---

### AC-009: Configure Slabs (US-009)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-009-01 | **GIVEN** I am editing an incentive plan<br>**WHEN** I click "Add Slab"<br>**THEN** I can enter: Minimum Threshold, Maximum Threshold, Incentive Rate/Amount | Critical |
| AC-009-02 | **GIVEN** I configure multiple slabs<br>**WHEN** slab thresholds overlap<br>**THEN** I see an error "Slab thresholds cannot overlap" | Critical |
| AC-009-03 | **GIVEN** I configure multiple slabs<br>**WHEN** there are gaps between slab thresholds<br>**THEN** I see a warning "Gap detected between slabs - verify this is intentional" | Important |
| AC-009-04 | **GIVEN** a slab is configured as percentage-based<br>**WHEN** a calculation runs<br>**THEN** the incentive is calculated as (Sales Amount * Percentage Rate) | Critical |
| AC-009-05 | **GIVEN** a slab is configured as fixed amount<br>**WHEN** a calculation runs and employee qualifies<br>**THEN** the employee receives the exact fixed amount | Critical |
| AC-009-06 | **GIVEN** I reorder slabs via drag-and-drop<br>**WHEN** I save<br>**THEN** the order is preserved and validated for threshold consistency | Nice to Have |

---

### AC-010: View Plan History (US-010)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-010-01 | **GIVEN** I am viewing an incentive plan<br>**WHEN** I click "Version History"<br>**THEN** I see all versions with version number, effective date, and change summary | Critical |
| AC-010-02 | **GIVEN** I am viewing version history<br>**WHEN** I click on a specific version<br>**THEN** I see the complete plan configuration as it was in that version | Critical |
| AC-010-03 | **GIVEN** I am viewing two versions<br>**WHEN** I select "Compare"<br>**THEN** I see a side-by-side diff highlighting changes | Important |
| AC-010-04 | **GIVEN** a historical version exists<br>**WHEN** I try to edit it<br>**THEN** I cannot modify it (read-only) | Critical |
| AC-010-05 | **GIVEN** I need to restore a previous version<br>**WHEN** I click "Restore Version"<br>**THEN** a new version is created with the old configuration | Important |

---

### AC-011: Preview Plan Changes (US-011)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-011-01 | **GIVEN** I have modified a plan's slabs<br>**WHEN** I click "Preview Impact"<br>**THEN** I see a simulation showing how the changes would affect sample calculations | Critical |
| AC-011-02 | **GIVEN** I am previewing changes<br>**WHEN** the preview calculates<br>**THEN** I see before/after comparison for affected employees | Critical |
| AC-011-03 | **GIVEN** I am previewing changes<br>**WHEN** I select a specific period<br>**THEN** the preview uses data from that period | Important |
| AC-011-04 | **GIVEN** the preview shows significant impact<br>**WHEN** I review results<br>**THEN** I see metrics: Total Impact, Affected Employees, Average Change | Important |
| AC-011-05 | **GIVEN** I am satisfied with the preview<br>**WHEN** I click "Apply Changes"<br>**THEN** the plan is updated (subject to approval workflow) | Critical |

---

### AC-012: Approve Plan Changes (US-012)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-012-01 | **GIVEN** a plan change is submitted for approval<br>**WHEN** I view my approval queue<br>**THEN** I see the pending plan change with summary of changes | Critical |
| AC-012-02 | **GIVEN** I am reviewing a plan change<br>**WHEN** I approve<br>**THEN** the plan is activated and effective from the specified date | Critical |
| AC-012-03 | **GIVEN** I am reviewing a plan change<br>**WHEN** I reject with comments<br>**THEN** the submitter is notified with my rejection reason | Critical |
| AC-012-04 | **GIVEN** I reject a plan change<br>**WHEN** the submitter modifies and resubmits<br>**THEN** a new approval request appears in my queue | Important |
| AC-012-05 | **GIVEN** a plan change is approved<br>**WHEN** I check the audit log<br>**THEN** I see the approval with approver name and timestamp | Critical |

---

### AC-013: Clone Incentive Plan (US-013)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-013-01 | **GIVEN** I am viewing an existing plan<br>**WHEN** I click "Clone"<br>**THEN** a new plan is created with the same configuration and a suffix "(Copy)" | Critical |
| AC-013-02 | **GIVEN** I clone a plan<br>**WHEN** the clone is created<br>**THEN** it is in "Draft" status regardless of source plan status | Critical |
| AC-013-03 | **GIVEN** I clone a plan<br>**WHEN** I modify the clone<br>**THEN** the original plan is unaffected | Critical |

---

## EP-003: Calculation Engine

### AC-014: Run Scheduled Calculations (US-014)

> *"That's where I saw the leprechaun. He tells me to burn things."* - Ralph Wiggum
>
> Our scheduled calculations run like clockwork - no leprechauns required!

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-014-01 | **GIVEN** a schedule is configured (daily/weekly/monthly)<br>**WHEN** the scheduled time arrives<br>**THEN** the calculation batch starts automatically | Critical |
| AC-014-02 | **GIVEN** a calculation batch is running<br>**WHEN** I check the status<br>**THEN** I see progress: employees processed, errors encountered | Critical |
| AC-014-03 | **GIVEN** a calculation batch completes<br>**WHEN** I view the results<br>**THEN** I see summary: total calculated, total amount, success/failure counts | Critical |
| AC-014-04 | **GIVEN** a calculation fails for some employees<br>**WHEN** the batch completes<br>**THEN** successful calculations are saved; failures are logged for review | Critical |
| AC-014-05 | **GIVEN** a previous batch exists for the same period<br>**WHEN** a new batch runs<br>**THEN** the new batch creates amendments, not overwrites | Critical |
| AC-014-06 | **GIVEN** a batch is running<br>**WHEN** an administrator cancels it<br>**THEN** the batch stops gracefully; completed calculations are preserved | Important |

---

### AC-015: View Calculation Details (US-015)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-015-01 | **GIVEN** I am a Sales Rep viewing my incentives<br>**WHEN** I click on a calculation<br>**THEN** I see: Period, Plan Used, Input Values, Slab Selected, Calculated Amount | Critical |
| AC-015-02 | **GIVEN** I am viewing calculation details<br>**WHEN** I see input values<br>**THEN** I see the exact values used (sales, production, etc.) at calculation time | Critical |
| AC-015-03 | **GIVEN** my incentive includes a split<br>**WHEN** I view details<br>**THEN** I see the split percentage and other participants | Critical |
| AC-015-04 | **GIVEN** my incentive was amended<br>**WHEN** I view details<br>**THEN** I see the original calculation and all amendments | Critical |
| AC-015-05 | **GIVEN** I am viewing calculation details<br>**WHEN** I click "Export"<br>**THEN** I receive a PDF with the full calculation breakdown | Important |

---

### AC-016: Manual Recalculation (US-016)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-016-01 | **GIVEN** I am a Finance Manager viewing a calculation<br>**WHEN** I click "Recalculate"<br>**THEN** I am prompted to confirm and provide a reason | Critical |
| AC-016-02 | **GIVEN** I confirm a recalculation<br>**WHEN** the recalculation runs<br>**THEN** a new calculation record is created as an amendment | Critical |
| AC-016-03 | **GIVEN** I recalculate an already-approved incentive<br>**WHEN** the recalculation completes<br>**THEN** the amendment requires re-approval | Critical |
| AC-016-04 | **GIVEN** a manual recalculation is triggered<br>**WHEN** I check the audit log<br>**THEN** I see who triggered it, when, and the reason | Critical |
| AC-016-05 | **GIVEN** I need to recalculate multiple employees<br>**WHEN** I select employees and click "Bulk Recalculate"<br>**THEN** all selected employees are recalculated | Important |

---

### AC-017: Preview Calculation (US-017)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-017-01 | **GIVEN** I am on the calculation page<br>**WHEN** I click "Preview"<br>**THEN** calculations run without committing to the database | Critical |
| AC-017-02 | **GIVEN** I am previewing calculations<br>**WHEN** the preview completes<br>**THEN** I see results identical to what would be committed | Critical |
| AC-017-03 | **GIVEN** I have previewed calculations<br>**WHEN** I click "Commit"<br>**THEN** the previewed results are saved as official records | Critical |
| AC-017-04 | **GIVEN** I have previewed calculations<br>**WHEN** I click "Discard"<br>**THEN** no records are created and I return to the calculation page | Critical |

---

### AC-018: View Calculation History (US-018)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-018-01 | **GIVEN** I am an Auditor viewing an employee<br>**WHEN** I access their calculation history<br>**THEN** I see all calculations in chronological order | Critical |
| AC-018-02 | **GIVEN** I am viewing calculation history<br>**WHEN** amendments exist<br>**THEN** I see the chain: Original > Amendment 1 > Amendment 2 > etc. | Critical |
| AC-018-03 | **GIVEN** I am viewing a specific calculation<br>**WHEN** I click "Reproduce"<br>**THEN** the system shows the exact inputs and formula that produced that result | Critical |
| AC-018-04 | **GIVEN** I am viewing calculation history<br>**WHEN** I export<br>**THEN** I receive an Excel file with all calculation records | Important |

---

### AC-019: Handle Proration (US-019)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-019-01 | **GIVEN** an employee joins mid-period<br>**WHEN** the calculation runs<br>**THEN** their incentive is prorated based on days active | Critical |
| AC-019-02 | **GIVEN** an employee transfers mid-period<br>**WHEN** the calculation runs<br>**THEN** incentives are calculated separately for each assignment, prorated | Critical |
| AC-019-03 | **GIVEN** an employee terminates mid-period<br>**WHEN** the calculation runs<br>**THEN** their incentive is prorated to their termination date | Critical |
| AC-019-04 | **GIVEN** proration is applied<br>**WHEN** I view calculation details<br>**THEN** I see the proration factor and calculation | Critical |
| AC-019-05 | **GIVEN** an employee is inactive for entire period<br>**WHEN** the calculation runs<br>**THEN** they receive zero incentive | Critical |

---

## EP-004: Approval Workflow

### AC-020: Submit for Approval (US-020)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-020-01 | **GIVEN** I have completed calculations for a period<br>**WHEN** I click "Submit for Approval"<br>**THEN** the calculations move to "Pending Approval" status | Critical |
| AC-020-02 | **GIVEN** I submit for approval<br>**WHEN** the submission succeeds<br>**THEN** the designated approvers are notified | Critical |
| AC-020-03 | **GIVEN** calculations are pending approval<br>**WHEN** I try to modify them<br>**THEN** I cannot edit (locked for review) | Critical |
| AC-020-04 | **GIVEN** I need to withdraw a submission<br>**WHEN** I click "Withdraw"<br>**THEN** the calculations return to draft status | Important |

---

### AC-021: Approve Incentives (US-021)

> *"I found a moon rock in my nose!"* - Ralph Wiggum
>
> Finding issues during approval is like finding moon rocks - unexpected but exciting!

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-021-01 | **GIVEN** I have items pending my approval<br>**WHEN** I view my approval queue<br>**THEN** I see: Employee, Period, Amount, Submitted By, Date Submitted | Critical |
| AC-021-02 | **GIVEN** I am reviewing an item<br>**WHEN** I click "Approve"<br>**THEN** the status changes to "Approved" and timestamp is recorded | Critical |
| AC-021-03 | **GIVEN** I am reviewing an item<br>**WHEN** I click "Reject"<br>**THEN** I must enter a reason (minimum 10 characters) | Critical |
| AC-021-04 | **GIVEN** I reject an item<br>**WHEN** the rejection is saved<br>**THEN** the submitter is notified with my rejection reason | Critical |
| AC-021-05 | **GIVEN** an amount exceeds $10,000<br>**WHEN** I approve<br>**THEN** it escalates to senior manager approval | Critical |
| AC-021-06 | **GIVEN** the item is my own incentive<br>**WHEN** I try to approve<br>**THEN** I see an error "Cannot approve your own incentive" | Critical |

---

### AC-022: Bulk Approval (US-022)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-022-01 | **GIVEN** I have multiple items in my queue<br>**WHEN** I select multiple items and click "Bulk Approve"<br>**THEN** all selected items are approved | Critical |
| AC-022-02 | **GIVEN** I am performing bulk approval<br>**WHEN** some items cannot be approved (e.g., my own)<br>**THEN** those items are skipped and I see a summary | Important |
| AC-022-03 | **GIVEN** I am performing bulk approval<br>**WHEN** I confirm<br>**THEN** I see a count: "X items approved, Y items skipped" | Important |
| AC-022-04 | **GIVEN** I want to reject in bulk<br>**WHEN** I select items and click "Bulk Reject"<br>**THEN** I must enter a reason that applies to all items | Important |

---

### AC-023: Approval Notifications (US-023)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-023-01 | **GIVEN** an item is submitted for my approval<br>**WHEN** the submission occurs<br>**THEN** I receive an email notification within 5 minutes | Critical |
| AC-023-02 | **GIVEN** an item has been pending for 3 days<br>**WHEN** the reminder is triggered<br>**THEN** I receive a reminder email | Important |
| AC-023-03 | **GIVEN** I am the submitter<br>**WHEN** my item is approved/rejected<br>**THEN** I receive an email notification with the decision | Critical |
| AC-023-04 | **GIVEN** I want to configure notifications<br>**WHEN** I access my preferences<br>**THEN** I can enable/disable notification types | Nice to Have |

---

### AC-024: Delegate Approval (US-024)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-024-01 | **GIVEN** I am an approver<br>**WHEN** I access delegation settings<br>**THEN** I can designate another user as my delegate with date range | Critical |
| AC-024-02 | **GIVEN** I have delegated to another user<br>**WHEN** items arrive in my queue<br>**THEN** my delegate can approve them on my behalf | Critical |
| AC-024-03 | **GIVEN** my delegate approves an item<br>**WHEN** I check the audit trail<br>**THEN** it shows "Approved by [Delegate] on behalf of [Me]" | Critical |
| AC-024-04 | **GIVEN** my delegation period ends<br>**WHEN** new items arrive<br>**THEN** only I can approve them (delegation expired) | Critical |

---

### AC-025: View Approval History (US-025)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-025-01 | **GIVEN** I am viewing an approved incentive<br>**WHEN** I click "Approval History"<br>**THEN** I see the complete chain: Submitted > Approved/Rejected with names and times | Critical |
| AC-025-02 | **GIVEN** multiple approval levels were required<br>**WHEN** I view history<br>**THEN** I see each level: Level 1 Approved by X, Level 2 Approved by Y | Critical |
| AC-025-03 | **GIVEN** an item was rejected and resubmitted<br>**WHEN** I view history<br>**THEN** I see the rejection, resubmission, and final approval | Critical |
| AC-025-04 | **GIVEN** I am exporting approval history<br>**WHEN** I click "Export"<br>**THEN** I receive a PDF with full approval chain | Important |

---

## EP-005: Reporting & Dashboards

### AC-026: Executive Dashboard (US-026)

> *"I sleep in a drawer!"* - Ralph Wiggum
>
> Our dashboard fits all your metrics in one place - like Ralph in his drawer!

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-026-01 | **GIVEN** I am a Finance Manager<br>**WHEN** I access the dashboard<br>**THEN** I see KPIs: Total Incentives Paid, Pending Approvals, YTD Spend, vs Budget | Critical |
| AC-026-02 | **GIVEN** I am viewing the dashboard<br>**WHEN** I change the date range<br>**THEN** all metrics update to reflect the selected period | Critical |
| AC-026-03 | **GIVEN** I am viewing a metric<br>**WHEN** I click on it<br>**THEN** I drill down to the underlying data | Important |
| AC-026-04 | **GIVEN** the dashboard is loading<br>**WHEN** data is retrieved<br>**THEN** it displays within 3 seconds | Critical |
| AC-026-05 | **GIVEN** I am viewing the dashboard<br>**WHEN** I click "Refresh"<br>**THEN** data is updated to the latest available | Important |

---

### AC-027: Export to Excel (US-027)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-027-01 | **GIVEN** I am viewing any data table<br>**WHEN** I click "Export to Excel"<br>**THEN** an Excel file downloads with all displayed data | Critical |
| AC-027-02 | **GIVEN** I have applied filters<br>**WHEN** I export<br>**THEN** only filtered data is exported | Critical |
| AC-027-03 | **GIVEN** the data set is large (>10,000 rows)<br>**WHEN** I export<br>**THEN** the export completes within 30 seconds | Important |
| AC-027-04 | **GIVEN** the export contains currency<br>**WHEN** I open the Excel file<br>**THEN** numbers are formatted as currency with proper decimal places | Important |

---

### AC-028: Team Performance Report (US-028)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-028-01 | **GIVEN** I am a Sales Manager<br>**WHEN** I access Team Performance<br>**THEN** I see my team's incentive summary by employee | Critical |
| AC-028-02 | **GIVEN** I am viewing team performance<br>**WHEN** I sort by Total Incentive<br>**THEN** top performers appear at the top | Important |
| AC-028-03 | **GIVEN** I am viewing team performance<br>**WHEN** I compare to previous period<br>**THEN** I see trend indicators (up/down arrows) | Important |
| AC-028-04 | **GIVEN** I only have access to certain stores<br>**WHEN** I view team performance<br>**THEN** I only see employees within my scope | Critical |

---

### AC-029: Audit Trail Report (US-029)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-029-01 | **GIVEN** I am an Auditor<br>**WHEN** I access Audit Trail Report<br>**THEN** I can filter by: Date Range, User, Action Type, Entity | Critical |
| AC-029-02 | **GIVEN** I generate an audit report<br>**WHEN** results display<br>**THEN** I see: Timestamp, User, Action, Entity, Old Value, New Value | Critical |
| AC-029-03 | **GIVEN** I need to verify a specific change<br>**WHEN** I search by criteria<br>**THEN** I find the exact audit record | Critical |
| AC-029-04 | **GIVEN** I export the audit report<br>**WHEN** the export completes<br>**THEN** I receive a signed/timestamped PDF for audit purposes | Important |

---

### AC-030: Personal Incentive View (US-030)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-030-01 | **GIVEN** I am a Sales Rep<br>**WHEN** I access "My Incentives"<br>**THEN** I see my incentive history with: Period, Plan, Amount, Status | Critical |
| AC-030-02 | **GIVEN** I am viewing my incentives<br>**WHEN** I click on a period<br>**THEN** I see the full calculation breakdown | Critical |
| AC-030-03 | **GIVEN** I have current period progress<br>**WHEN** I view my dashboard<br>**THEN** I see a progress indicator toward target (if applicable) | Important |
| AC-030-04 | **GIVEN** I want to download my records<br>**WHEN** I click "Download Statement"<br>**THEN** I receive a PDF statement of my incentives | Important |

---

## EP-006: User Management

### AC-031: Single Sign-On (US-031)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-031-01 | **GIVEN** I navigate to the application<br>**WHEN** I am not logged in<br>**THEN** I am redirected to Azure Entra ID login | Critical |
| AC-031-02 | **GIVEN** I have valid corporate credentials<br>**WHEN** I authenticate<br>**THEN** I am logged into the application with my identity | Critical |
| AC-031-03 | **GIVEN** my session expires<br>**WHEN** I take an action<br>**THEN** I am prompted to re-authenticate seamlessly | Critical |
| AC-031-04 | **GIVEN** MFA is required<br>**WHEN** I log in<br>**THEN** I must complete MFA before accessing the application | Critical |

---

### AC-032: Manage User Roles (US-032)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-032-01 | **GIVEN** I am a System Administrator<br>**WHEN** I access User Management<br>**THEN** I see a list of users with their assigned roles | Critical |
| AC-032-02 | **GIVEN** I am editing a user<br>**WHEN** I change their role<br>**THEN** the change takes effect on their next login | Critical |
| AC-032-03 | **GIVEN** I assign a role<br>**WHEN** the assignment is saved<br>**THEN** the audit log records who made the change | Critical |
| AC-032-04 | **GIVEN** I try to remove my own Admin role<br>**WHEN** I save<br>**THEN** I see an error "Cannot remove your own Admin role" | Critical |

---

### AC-033: Scope-Based Access (US-033)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-033-01 | **GIVEN** I am assigning scope to a user<br>**WHEN** I select specific zones/stores<br>**THEN** the user only sees data from those areas | Critical |
| AC-033-02 | **GIVEN** I have scope-limited access<br>**WHEN** I search for employees<br>**THEN** I only see employees within my scope | Critical |
| AC-033-03 | **GIVEN** I have scope-limited access<br>**WHEN** I view reports<br>**THEN** data is filtered to my scope | Critical |
| AC-033-04 | **GIVEN** I try to access data outside my scope via URL<br>**WHEN** the request is processed<br>**THEN** I see "Access Denied" | Critical |

---

### AC-034: View User Activity Log (US-034)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-034-01 | **GIVEN** I am a System Administrator<br>**WHEN** I access User Activity Log<br>**THEN** I see: User, Action, Timestamp, IP Address, Status | Critical |
| AC-034-02 | **GIVEN** I filter by user<br>**WHEN** results display<br>**THEN** I see all actions by that specific user | Critical |
| AC-034-03 | **GIVEN** a failed login attempt occurs<br>**WHEN** I check the log<br>**THEN** I see the failed attempt with reason | Critical |
| AC-034-04 | **GIVEN** I export the activity log<br>**WHEN** the export completes<br>**THEN** I receive an Excel file with full log data | Important |

---

## EP-007: Integration

### AC-035: Export to Payroll (US-035)

> *"Eww, Daddy, this tastes like Gramma!"* - Ralph Wiggum
>
> Our payroll export is clean and properly formatted - no unexpected surprises!

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-035-01 | **GIVEN** incentives are approved<br>**WHEN** the export is triggered<br>**THEN** data is sent to the payroll system via API | Critical |
| AC-035-02 | **GIVEN** an export is triggered<br>**WHEN** the export succeeds<br>**THEN** incentive status changes to "Exported" | Critical |
| AC-035-03 | **GIVEN** an export fails<br>**WHEN** error occurs<br>**THEN** the error is logged and administrators are notified | Critical |
| AC-035-04 | **GIVEN** an export succeeds<br>**WHEN** I check the audit log<br>**THEN** I see the export timestamp and payroll batch ID | Critical |
| AC-035-05 | **GIVEN** I need to re-export<br>**WHEN** I trigger a re-export<br>**THEN** I am warned "Already exported - are you sure?" | Important |

---

### AC-036: Import Sales Data (US-036)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-036-01 | **GIVEN** the daily import schedule<br>**WHEN** the scheduled time arrives<br>**THEN** sales data is fetched from the ERP system | Critical |
| AC-036-02 | **GIVEN** import succeeds<br>**WHEN** data is loaded<br>**THEN** new records are added and existing records are updated | Critical |
| AC-036-03 | **GIVEN** import fails<br>**WHEN** error occurs<br>**THEN** administrators are notified and last successful import is retained | Critical |
| AC-036-04 | **GIVEN** I need to check import status<br>**WHEN** I access the import log<br>**THEN** I see: Last Import Time, Records Imported, Errors | Important |
| AC-036-05 | **GIVEN** I need to trigger manual import<br>**WHEN** I click "Import Now"<br>**THEN** the import runs immediately | Important |

---

### AC-037: API Access (US-037)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-037-01 | **GIVEN** I am configuring API access<br>**WHEN** I create API credentials<br>**THEN** a client ID and secret are generated | Critical |
| AC-037-02 | **GIVEN** a client has valid credentials<br>**WHEN** they request a token<br>**THEN** they receive a JWT valid for 1 hour | Critical |
| AC-037-03 | **GIVEN** a client makes an API request<br>**WHEN** the token is valid<br>**THEN** they receive the requested data | Critical |
| AC-037-04 | **GIVEN** a client exceeds rate limits<br>**WHEN** they make a request<br>**THEN** they receive a 429 "Too Many Requests" response | Critical |
| AC-037-05 | **GIVEN** API activity occurs<br>**WHEN** I check the API log<br>**THEN** I see: Client, Endpoint, Timestamp, Response Code | Important |

---

## EP-008: Audit & Compliance

### AC-038: Immutable Audit Trail (US-038)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-038-01 | **GIVEN** any data change occurs<br>**WHEN** the change is saved<br>**THEN** an audit record is created automatically | Critical |
| AC-038-02 | **GIVEN** an audit record exists<br>**WHEN** any user tries to modify or delete it<br>**THEN** the operation is rejected | Critical |
| AC-038-03 | **GIVEN** I query the audit trail<br>**WHEN** I search by entity<br>**THEN** I see all changes with before/after values | Critical |
| AC-038-04 | **GIVEN** audit data is stored<br>**WHEN** 7 years pass<br>**THEN** the data is archived but still retrievable | Critical |

---

### AC-039: Point-in-Time Query (US-039)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-039-01 | **GIVEN** I need to query historical data<br>**WHEN** I specify a point-in-time date<br>**THEN** I see data exactly as it existed on that date | Critical |
| AC-039-02 | **GIVEN** I query an employee's assignment on a past date<br>**WHEN** results display<br>**THEN** I see their Zone, Store, Role as of that date | Critical |
| AC-039-03 | **GIVEN** I reproduce a historical calculation<br>**WHEN** I use point-in-time data<br>**THEN** the result matches the original calculation exactly | Critical |
| AC-039-04 | **GIVEN** I query a date before the system existed<br>**WHEN** results display<br>**THEN** I see "No data available for this date" | Important |

---

### AC-040: Data Retention Compliance (US-040)

| ID | Criterion | Priority |
|----|-----------|----------|
| AC-040-01 | **GIVEN** data retention policy is 7 years<br>**WHEN** data reaches retention age<br>**THEN** it is archived to cold storage | Critical |
| AC-040-02 | **GIVEN** data is archived<br>**WHEN** I need to access it<br>**THEN** I can request retrieval (with delay) | Important |
| AC-040-03 | **GIVEN** a data deletion request<br>**WHEN** I process it<br>**THEN** personal data is anonymized but financial records are retained | Critical |
| AC-040-04 | **GIVEN** retention jobs run<br>**WHEN** completed<br>**THEN** a summary report is generated showing actions taken | Important |

---

## Summary Statistics

| Epic | Acceptance Criteria Count |
|------|---------------------------|
| EP-001: Employee Management | 33 |
| EP-002: Incentive Configuration | 28 |
| EP-003: Calculation Engine | 29 |
| EP-004: Approval Workflow | 25 |
| EP-005: Reporting & Dashboards | 21 |
| EP-006: User Management | 15 |
| EP-007: Integration | 15 |
| EP-008: Audit & Compliance | 13 |
| **Total** | **179** |

---

> *"And when the doctor said I didn't have worms anymore, that was the happiest day of my life."* - Ralph Wiggum
>
> When all acceptance criteria pass, that will be the happiest day of this project's life!

---

**Document Approval**

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Product Owner | Skanda Prasad | _____________ | ______ |
| QA Lead | Claude Code | _____________ | ______ |

---

*This document is part of the DSIF Quality Gate Framework - QG-1 Deliverable*
