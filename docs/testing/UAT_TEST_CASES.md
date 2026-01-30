# User Acceptance Test Cases

**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Last Updated:** January 2025
**Total Scenarios:** 85

> *"I don't even believe in Jebus!"* - Ralph Wiggum
>
> We believe in thorough UAT - every scenario tested by real users!

---

## Table of Contents

1. [Overview](#overview)
2. [UAT Preparation](#uat-preparation)
3. [Employee Management Scenarios](#employee-management-scenarios)
4. [Incentive Plan Scenarios](#incentive-plan-scenarios)
5. [Calculation Scenarios](#calculation-scenarios)
6. [Approval Workflow Scenarios](#approval-workflow-scenarios)
7. [Reporting Scenarios](#reporting-scenarios)
8. [Administration Scenarios](#administration-scenarios)
9. [Edge Cases and Error Handling](#edge-cases-and-error-handling)
10. [UAT Sign-off](#uat-sign-off)

---

## Overview

### UAT Objectives

> *"The doctor said I wouldn't have so many nose bleeds if I kept my finger outta there."* - Ralph Wiggum
>
> Our UAT objectives are much more refined - we probe for quality, not nosebleeds!

1. Validate business requirements are met
2. Verify user workflows are intuitive
3. Confirm calculations match expected results
4. Ensure reporting meets business needs
5. Validate system performance from user perspective

### Scenario Summary

| Category | Scenarios | Critical | High | Medium |
|----------|-----------|----------|------|--------|
| Employee Management | 15 | 5 | 7 | 3 |
| Incentive Plans | 12 | 4 | 5 | 3 |
| Calculations | 20 | 10 | 7 | 3 |
| Approval Workflows | 15 | 5 | 7 | 3 |
| Reporting | 10 | 3 | 4 | 3 |
| Administration | 8 | 2 | 4 | 2 |
| Edge Cases | 5 | 2 | 2 | 1 |
| **Total** | **85** | **31** | **36** | **18** |

### Test Environment

| Item | Value |
|------|-------|
| Environment | UAT (https://uat.dsif.dorise.com) |
| Test Data | Production-masked data |
| Test Users | 10 configured users |
| Browser | Chrome (latest), Edge (latest) |

---

## UAT Preparation

### Prerequisites

- [ ] UAT environment deployed and accessible
- [ ] Test users created with appropriate roles
- [ ] Test data loaded and verified
- [ ] User training completed
- [ ] Test schedule communicated

### Test User Roles

| User | Role | Department | Purpose |
|------|------|------------|---------|
| UAT_Admin | System Administrator | IT | Admin scenarios |
| UAT_Manager1 | Sales Manager | Sales | Manager workflows |
| UAT_Manager2 | Finance Manager | Finance | Finance approvals |
| UAT_User1 | Sales Executive | Sales | Employee view |
| UAT_User2 | Sales Representative | Sales | Employee view |
| UAT_HR | HR Administrator | HR | HR integrations |
| UAT_Finance | Finance Analyst | Finance | Reporting |
| UAT_Viewer | Report Viewer | Operations | Read-only access |

---

## Employee Management Scenarios

### UAT-EMP-001: View Employee Dashboard

| Field | Value |
|-------|-------|
| **ID** | UAT-EMP-001 |
| **Title** | Employee views personal incentive dashboard |
| **Priority** | Critical |
| **User Role** | Sales Executive |
| **Business Value** | Employees can track their incentive progress |

**Preconditions:**
- User logged in as UAT_User1
- User has active incentive plan assignment

**Test Steps:**
1. Navigate to Dashboard
2. View current period incentive summary
3. View achievement progress
4. View pending payouts
5. View historical earnings

**Expected Results:**
- Dashboard loads within 3 seconds
- Current incentive amount displayed
- Achievement percentage shown
- Historical data accurate

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

**Sign-off:** _______________ Date: _______________

---

### UAT-EMP-002: View Detailed Incentive Breakdown

| Field | Value |
|-------|-------|
| **ID** | UAT-EMP-002 |
| **Title** | Employee views calculation breakdown |
| **Priority** | High |
| **User Role** | Sales Executive |

**Preconditions:**
- User has completed calculations

**Test Steps:**
1. Navigate to Incentive History
2. Select a calculation period
3. View calculation breakdown
4. View slab application details
5. Download calculation statement

**Expected Results:**
- Step-by-step calculation visible
- Slab breakdown shown
- PDF statement downloadable
- All amounts correct

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-EMP-003: Search and Filter Employees

| Field | Value |
|-------|-------|
| **ID** | UAT-EMP-003 |
| **Title** | Manager searches for team members |
| **Priority** | High |
| **User Role** | Sales Manager |

**Test Steps:**
1. Navigate to Employee List
2. Search by name
3. Filter by department
4. Filter by status
5. Sort results

**Expected Results:**
- Search returns matching employees
- Filters work correctly
- Results paginated properly
- Export to Excel works

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-EMP-004: View Team Incentive Summary

| Field | Value |
|-------|-------|
| **ID** | UAT-EMP-004 |
| **Title** | Manager views team incentive summary |
| **Priority** | Critical |
| **User Role** | Sales Manager |

**Test Steps:**
1. Navigate to Team Dashboard
2. View team summary metrics
3. View individual team member performance
4. Compare against targets
5. Identify top performers

**Expected Results:**
- Team total incentive shown
- Individual breakdowns visible
- Target vs achievement comparison
- Ranking list accurate

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-EMP-005: Update Employee Information

| Field | Value |
|-------|-------|
| **ID** | UAT-EMP-005 |
| **Title** | HR updates employee details |
| **Priority** | High |
| **User Role** | HR Administrator |

**Test Steps:**
1. Search for employee
2. Edit employee details
3. Change department
4. Update manager
5. Save changes

**Expected Results:**
- Changes saved successfully
- Audit trail created
- Eligibility recalculated if needed
- Notifications sent

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-EMP-006: Add New Employee

| Field | Value |
|-------|-------|
| **ID** | UAT-EMP-006 |
| **Title** | HR adds new employee manually |
| **Priority** | High |
| **User Role** | HR Administrator |

**Test Steps:**
1. Navigate to Add Employee
2. Fill in required fields
3. Select department and manager
4. Set join date
5. Save employee

**Expected Results:**
- Employee created successfully
- Employee code generated
- Eligibility determined
- Welcome notification sent

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-EMP-007: Bulk Import Employees

| Field | Value |
|-------|-------|
| **ID** | UAT-EMP-007 |
| **Title** | HR imports employees from file |
| **Priority** | Medium |
| **User Role** | HR Administrator |

**Test Steps:**
1. Download template file
2. Fill template with data
3. Upload file
4. Review validation results
5. Confirm import

**Expected Results:**
- Template downloads correctly
- Validation errors clearly shown
- Valid records imported
- Summary report generated

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-EMP-008: Process Employee Termination

| Field | Value |
|-------|-------|
| **ID** | UAT-EMP-008 |
| **Title** | HR processes employee termination |
| **Priority** | Critical |
| **User Role** | HR Administrator |

**Test Steps:**
1. Select employee
2. Initiate termination process
3. Enter termination date
4. Select reason
5. Request final settlement

**Expected Results:**
- Termination recorded
- Prorated calculation triggered
- Access removed
- Final settlement calculated

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-EMP-009: Process Employee Transfer

| Field | Value |
|-------|-------|
| **ID** | UAT-EMP-009 |
| **Title** | HR processes department transfer |
| **Priority** | High |
| **User Role** | HR Administrator |

**Test Steps:**
1. Select employee
2. Initiate transfer
3. Select new department
4. Set effective date
5. Assign new manager

**Expected Results:**
- Transfer recorded
- Old plan prorated
- New plan assigned
- Manager notified

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-EMP-010: Assign Employee to Plan

| Field | Value |
|-------|-------|
| **ID** | UAT-EMP-010 |
| **Title** | Manager assigns employee to incentive plan |
| **Priority** | Critical |
| **User Role** | Sales Manager |

**Test Steps:**
1. Select employee
2. View eligible plans
3. Select plan
4. Set target amount
5. Confirm assignment

**Expected Results:**
- Employee assigned to plan
- Target recorded
- Employee notified
- Assignment visible in history

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-EMP-011: View Employee Audit Trail

| Field | Value |
|-------|-------|
| **ID** | UAT-EMP-011 |
| **Title** | Admin views employee change history |
| **Priority** | Medium |
| **User Role** | System Administrator |

**Test Steps:**
1. Select employee
2. View audit log
3. Filter by date range
4. Filter by change type
5. Export audit log

**Expected Results:**
- Complete history shown
- Changes clearly described
- User who made change visible
- Export works correctly

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-EMP-012: Employee Self-Service Profile

| Field | Value |
|-------|-------|
| **ID** | UAT-EMP-012 |
| **Title** | Employee views and updates profile |
| **Priority** | Medium |
| **User Role** | Sales Executive |

**Test Steps:**
1. Navigate to Profile
2. View personal details
3. Update contact info
4. Change notification preferences
5. Save changes

**Expected Results:**
- Profile displays correctly
- Editable fields identified
- Changes saved
- Confirmation shown

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-EMP-013: Manager Views Direct Reports

| Field | Value |
|-------|-------|
| **ID** | UAT-EMP-013 |
| **Title** | Manager views organizational hierarchy |
| **Priority** | High |
| **User Role** | Sales Manager |

**Test Steps:**
1. Navigate to My Team
2. View direct reports
3. View team hierarchy
4. Access team member details
5. View team performance

**Expected Results:**
- All direct reports visible
- Hierarchy displayed correctly
- Can drill down to individuals
- Performance metrics shown

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-EMP-014: Employee Eligibility Check

| Field | Value |
|-------|-------|
| **ID** | UAT-EMP-014 |
| **Title** | System determines plan eligibility |
| **Priority** | High |
| **User Role** | HR Administrator |

**Test Steps:**
1. Add employee with 2 months tenure
2. Check eligibility for plan requiring 3 months
3. Advance date to 3 months
4. Recheck eligibility
5. Verify automatic notification

**Expected Results:**
- Initially ineligible
- Eligibility changes at threshold
- Automatic notification sent
- Eligibility reason displayed

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-EMP-015: Employee Data Export

| Field | Value |
|-------|-------|
| **ID** | UAT-EMP-015 |
| **Title** | Export employee data for reporting |
| **Priority** | Medium |
| **User Role** | Finance Analyst |

**Test Steps:**
1. Navigate to Employee Reports
2. Select fields to export
3. Apply filters
4. Choose export format
5. Download file

**Expected Results:**
- All selected fields exported
- Filters applied correctly
- File downloads successfully
- Data matches screen

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

## Incentive Plan Scenarios

### UAT-PLN-001: Create New Incentive Plan

| Field | Value |
|-------|-------|
| **ID** | UAT-PLN-001 |
| **Title** | Admin creates new incentive plan |
| **Priority** | Critical |
| **User Role** | System Administrator |

> *"Prinskipper Skipple... Prin... Principal Skinner!"* - Ralph Wiggum
>
> Creating plans is much easier than pronouncing Principal Skinner's name!

**Test Steps:**
1. Navigate to Plan Management
2. Click Create New Plan
3. Enter plan details
4. Configure slabs
5. Add business rules
6. Save as draft

**Expected Results:**
- Plan created in draft status
- Slabs validated
- Rules applied
- Ready for activation

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-PLN-002: Configure Incentive Slabs

| Field | Value |
|-------|-------|
| **ID** | UAT-PLN-002 |
| **Title** | Admin configures slab structure |
| **Priority** | Critical |
| **User Role** | System Administrator |

**Test Steps:**
1. Open plan in edit mode
2. Add slab (0-80%: 0%)
3. Add slab (80-100%: 5%)
4. Add slab (100-120%: 8%)
5. Add slab (120%+: 12%)
6. Validate slabs

**Expected Results:**
- No overlap validation passes
- No gap validation passes
- Preview calculation works
- Slabs saved correctly

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-PLN-003: Activate Incentive Plan

| Field | Value |
|-------|-------|
| **ID** | UAT-PLN-003 |
| **Title** | Admin activates draft plan |
| **Priority** | Critical |
| **User Role** | System Administrator |

**Test Steps:**
1. Select draft plan
2. Run validation check
3. Review warnings
4. Click Activate
5. Confirm activation

**Expected Results:**
- Validation passes
- Plan status changes to Active
- Audit entry created
- Plan available for assignment

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-PLN-004: Clone Existing Plan

| Field | Value |
|-------|-------|
| **ID** | UAT-PLN-004 |
| **Title** | Admin clones plan for new period |
| **Priority** | High |
| **User Role** | System Administrator |

**Test Steps:**
1. Select existing plan
2. Click Clone
3. Enter new plan name
4. Set new effective dates
5. Adjust parameters if needed
6. Save cloned plan

**Expected Results:**
- New plan created
- Slabs copied
- Rules copied
- New dates applied

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-PLN-005: View Plan Details

| Field | Value |
|-------|-------|
| **ID** | UAT-PLN-005 |
| **Title** | Manager views plan configuration |
| **Priority** | High |
| **User Role** | Sales Manager |

**Test Steps:**
1. Navigate to Plans
2. Select active plan
3. View plan summary
4. View slab structure
5. View assigned employees

**Expected Results:**
- Plan details displayed
- Slab structure clear
- Assigned employees listed
- Cannot edit (read-only for manager)

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-PLN-006: Test Plan Calculator

| Field | Value |
|-------|-------|
| **ID** | UAT-PLN-006 |
| **Title** | User tests plan with sample values |
| **Priority** | High |
| **User Role** | Sales Executive |

**Test Steps:**
1. Open plan calculator
2. Enter target amount
3. Enter achievement
4. Click Calculate
5. View projected incentive

**Expected Results:**
- Calculator easy to use
- Slab application shown
- Projected amount correct
- Can save scenarios

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-PLN-007: Expire Incentive Plan

| Field | Value |
|-------|-------|
| **ID** | UAT-PLN-007 |
| **Title** | Admin expires old plan |
| **Priority** | Medium |
| **User Role** | System Administrator |

**Test Steps:**
1. Select active plan
2. Click Expire
3. Confirm expiration
4. Verify final calculations processed
5. Verify employees reassigned

**Expected Results:**
- Plan status changes to Expired
- Pending calculations completed
- Employees notified
- Cannot be modified

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-PLN-008: Add Business Rules to Plan

| Field | Value |
|-------|-------|
| **ID** | UAT-PLN-008 |
| **Title** | Admin adds eligibility rules |
| **Priority** | High |
| **User Role** | System Administrator |

**Test Steps:**
1. Open plan in edit mode
2. Navigate to Rules section
3. Add minimum tenure rule
4. Add department rule
5. Save and validate

**Expected Results:**
- Rules added successfully
- Validation logic correct
- Employees filtered correctly
- Rules displayed clearly

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-PLN-009: Plan Performance Dashboard

| Field | Value |
|-------|-------|
| **ID** | UAT-PLN-009 |
| **Title** | View plan performance metrics |
| **Priority** | Medium |
| **User Role** | Finance Manager |

**Test Steps:**
1. Navigate to Plan Analytics
2. Select plan
3. View participation rate
4. View payout totals
5. View distribution charts

**Expected Results:**
- Participation rate accurate
- Payout totals correct
- Charts render properly
- Can drill down to details

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-PLN-010: Compare Plans

| Field | Value |
|-------|-------|
| **ID** | UAT-PLN-010 |
| **Title** | Compare multiple plans side by side |
| **Priority** | Medium |
| **User Role** | System Administrator |

**Test Steps:**
1. Select first plan
2. Add second plan for comparison
3. View side-by-side comparison
4. Compare slabs
5. Compare performance

**Expected Results:**
- Plans displayed side by side
- Differences highlighted
- Performance comparison clear
- Can export comparison

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-PLN-011: Import Plan Configuration

| Field | Value |
|-------|-------|
| **ID** | UAT-PLN-011 |
| **Title** | Import plan from template |
| **Priority** | Low |
| **User Role** | System Administrator |

**Test Steps:**
1. Download plan template
2. Fill template
3. Upload template
4. Review imported plan
5. Activate plan

**Expected Results:**
- Template format correct
- Import validates data
- Plan created correctly
- Ready for activation

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-PLN-012: Plan Audit Trail

| Field | Value |
|-------|-------|
| **ID** | UAT-PLN-012 |
| **Title** | View plan change history |
| **Priority** | Medium |
| **User Role** | System Administrator |

**Test Steps:**
1. Select plan
2. View audit log
3. See creation entry
4. See modification entries
5. See activation entry

**Expected Results:**
- Complete history shown
- Changes detailed
- User information visible
- Timestamps accurate

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

## Calculation Scenarios

### UAT-CALC-001: Calculate Single Employee Incentive

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-001 |
| **Title** | Calculate incentive for individual employee |
| **Priority** | Critical |
| **User Role** | Sales Manager |

> *"I ated the purple berries... they taste like burning!"* - Ralph Wiggum
>
> Our calculations never burn - they're accurate to the penny!

**Test Steps:**
1. Navigate to Calculations
2. Select employee
3. Select period
4. Enter achievement data
5. Click Calculate
6. Review results

**Expected Results:**
- Calculation completes
- Slab application correct
- Amount accurate
- Breakdown available

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-002: Batch Calculate All Employees

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-002 |
| **Title** | Run batch calculation for period |
| **Priority** | Critical |
| **User Role** | Finance Manager |

**Test Steps:**
1. Navigate to Batch Calculations
2. Select plan
3. Select period
4. Click Run Batch
5. Monitor progress
6. Review results

**Expected Results:**
- All eligible employees processed
- Progress shown in real-time
- No errors or exceptions
- Summary report generated

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-003: Calculate with Threshold Validation

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-003 |
| **Title** | Validate minimum achievement threshold |
| **Priority** | Critical |
| **User Role** | Sales Manager |

**Test Steps:**
1. Employee with 70% achievement
2. Plan threshold at 80%
3. Calculate incentive
4. Verify zero payout
5. Display threshold message

**Expected Results:**
- Calculation shows zero
- Clear message about threshold
- Achievement still recorded
- Status marked appropriately

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-004: Calculate Across Multiple Slabs

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-004 |
| **Title** | Achievement spanning multiple slabs |
| **Priority** | Critical |
| **User Role** | Sales Manager |

**Test Steps:**
1. Employee with 115% achievement
2. Plan has 4 slabs
3. Calculate incentive
4. View slab breakdown
5. Verify each slab contribution

**Expected Results:**
- Each slab calculated separately
- Combined total correct
- Breakdown clearly shows:
  - 0-80%: 0% rate
  - 80-100%: 5% rate
  - 100-115%: 8% rate

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-005: Calculate with Maximum Cap

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-005 |
| **Title** | Apply maximum incentive cap |
| **Priority** | High |
| **User Role** | Sales Manager |

**Test Steps:**
1. Employee with 200% achievement
2. Plan has max cap of 50,000
3. Calculate incentive
4. Verify cap applied
5. Display cap message

**Expected Results:**
- Cap applied at 50,000
- Clear message about cap
- Uncapped amount shown
- Cap amount shown

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-006: Calculate Prorated New Joiner

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-006 |
| **Title** | Prorate incentive for mid-period joiner |
| **Priority** | Critical |
| **User Role** | Sales Manager |

**Test Steps:**
1. Employee joined Jan 15
2. Period is full January
3. Achievement 100%
4. Calculate incentive
5. Verify proration

**Expected Results:**
- Prorated to 17/31 days
- Proration clearly shown
- Full and prorated amounts displayed
- Proration reason documented

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-007: Calculate Prorated Leaver

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-007 |
| **Title** | Prorate incentive for mid-period leaver |
| **Priority** | Critical |
| **User Role** | HR Administrator |

**Test Steps:**
1. Employee leaving Jan 20
2. Period is full January
3. Achievement 100%
4. Calculate final settlement
5. Verify proration

**Expected Results:**
- Prorated to 20/31 days
- Final settlement calculated
- Part of termination process
- Audit trail created

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-008: Recalculate with Adjustment

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-008 |
| **Title** | Recalculate after achievement adjustment |
| **Priority** | High |
| **User Role** | Finance Manager |

**Test Steps:**
1. Find approved calculation
2. Request adjustment
3. Enter adjustment reason
4. Enter new achievement
5. Recalculate
6. Submit for re-approval

**Expected Results:**
- Original preserved
- Adjustment recorded
- Difference calculated
- New approval required

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-009: Calculate Multiple Metrics

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-009 |
| **Title** | Calculate with weighted metrics |
| **Priority** | High |
| **User Role** | Sales Manager |

**Test Steps:**
1. Plan has Revenue (70%) and New Customers (30%)
2. Enter Revenue achievement: 100%
3. Enter New Customers achievement: 120%
4. Calculate weighted result
5. Verify calculation

**Expected Results:**
- Weighted average: 106%
- Each metric contribution shown
- Final incentive correct
- Breakdown available

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-010: Void Calculation

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-010 |
| **Title** | Void erroneous calculation |
| **Priority** | High |
| **User Role** | Finance Manager |

**Test Steps:**
1. Find calculation with error
2. Click Void
3. Enter void reason
4. Confirm void
5. Verify calculation status

**Expected Results:**
- Calculation voided
- Not included in reports
- Audit trail created
- Can recalculate

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-011: View Calculation History

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-011 |
| **Title** | View historical calculations |
| **Priority** | High |
| **User Role** | Sales Executive |

**Test Steps:**
1. Navigate to My Incentives
2. View calculation history
3. Filter by period
4. View details of past calculation
5. Download statement

**Expected Results:**
- Complete history shown
- Filters work correctly
- Details accessible
- Statements downloadable

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-012: Zero Achievement Calculation

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-012 |
| **Title** | Handle zero achievement |
| **Priority** | High |
| **User Role** | Sales Manager |

**Test Steps:**
1. Employee with 0% achievement
2. Calculate incentive
3. Verify zero result
4. No negative amounts
5. Record still created

**Expected Results:**
- Calculation completes
- Result is zero
- No errors
- Record for audit

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-013: Negative Adjustment Calculation

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-013 |
| **Title** | Process negative adjustment |
| **Priority** | High |
| **User Role** | Finance Manager |

**Test Steps:**
1. Original calculation: ₹50,000
2. Adjustment: -₹5,000 (returned order)
3. Calculate adjustment
4. Verify negative handled
5. Net amount correct

**Expected Results:**
- Negative adjustment recorded
- Recovery amount calculated
- Net payable updated
- Approval required

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-014: Calculation Error Handling

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-014 |
| **Title** | Handle calculation errors gracefully |
| **Priority** | Medium |
| **User Role** | Finance Manager |

**Test Steps:**
1. Trigger calculation error (e.g., missing data)
2. View error message
3. Check error details
4. Resolve issue
5. Retry calculation

**Expected Results:**
- Clear error message
- Actionable guidance
- Other calculations continue
- Retry option available

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-015: Bulk Upload Achievements

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-015 |
| **Title** | Upload achievements via file |
| **Priority** | High |
| **User Role** | Finance Analyst |

**Test Steps:**
1. Download achievement template
2. Fill template with data
3. Upload file
4. Review validation
5. Process calculations

**Expected Results:**
- Template clear and usable
- Validation catches errors
- Valid records processed
- Summary report generated

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-016: Currency Conversion

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-016 |
| **Title** | Calculate with currency conversion |
| **Priority** | Medium |
| **User Role** | Sales Manager |

**Test Steps:**
1. Achievement in USD
2. Payout in INR
3. Calculate with period exchange rate
4. View conversion details
5. Verify converted amount

**Expected Results:**
- Exchange rate applied
- Conversion transparent
- Both currencies shown
- Rate source documented

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-017: Retrospective Calculation

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-017 |
| **Title** | Calculate for past period |
| **Priority** | Medium |
| **User Role** | Finance Manager |

**Test Steps:**
1. Select past period
2. Run calculation
3. Use historical data
4. Apply period rules
5. Mark as retrospective

**Expected Results:**
- Historical data used
- Period rules applied
- Clearly marked retrospective
- Special approval needed

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-018: Calculate Team Bonus

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-018 |
| **Title** | Calculate team-based bonus |
| **Priority** | High |
| **User Role** | Sales Manager |

**Test Steps:**
1. Select team plan
2. Enter team achievement
3. Calculate team bonus
4. Distribute to members
5. Review distribution

**Expected Results:**
- Team total calculated
- Distribution formula applied
- Individual amounts correct
- Can adjust distribution

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-019: Preview Before Calculation

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-019 |
| **Title** | Preview calculation before save |
| **Priority** | High |
| **User Role** | Sales Manager |

**Test Steps:**
1. Enter achievement data
2. Click Preview
3. Review projected result
4. Make adjustments if needed
5. Confirm and save

**Expected Results:**
- Preview shows expected result
- Can modify before save
- Clear "Preview" indicator
- Easy to confirm or cancel

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-CALC-020: Decimal Precision

| Field | Value |
|-------|-------|
| **ID** | UAT-CALC-020 |
| **Title** | Verify decimal handling |
| **Priority** | Critical |
| **User Role** | Finance Analyst |

**Test Steps:**
1. Achievement: 99.5%
2. Target: ₹100,000.50
3. Rate: 5.25%
4. Calculate incentive
5. Verify precision

**Expected Results:**
- Decimals handled correctly
- Rounding rules applied
- Final amount precise
- No floating-point errors

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

## Approval Workflow Scenarios

### UAT-APR-001: Submit for Approval

| Field | Value |
|-------|-------|
| **ID** | UAT-APR-001 |
| **Title** | Submit calculations for approval |
| **Priority** | Critical |
| **User Role** | Sales Manager |

> *"Daddy, I'm scared. Too scared to even wet my pants."* - Ralph Wiggum
>
> Don't be scared of approval workflows - they're designed to be smooth!

**Test Steps:**
1. Select completed calculations
2. Click Submit for Approval
3. Add submission comments
4. Confirm submission
5. Verify notifications sent

**Expected Results:**
- Calculations submitted
- Status changes to Pending Approval
- Approver notified
- Submitter receives confirmation

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-APR-002: Approve Single Calculation

| Field | Value |
|-------|-------|
| **ID** | UAT-APR-002 |
| **Title** | Approve individual calculation |
| **Priority** | Critical |
| **User Role** | Finance Manager |

**Test Steps:**
1. View pending approvals
2. Select calculation
3. Review details
4. Click Approve
5. Add comments

**Expected Results:**
- Calculation approved
- Status changes to Approved
- Submitter notified
- Audit trail updated

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-APR-003: Reject Calculation

| Field | Value |
|-------|-------|
| **ID** | UAT-APR-003 |
| **Title** | Reject calculation with reason |
| **Priority** | High |
| **User Role** | Finance Manager |

**Test Steps:**
1. Select pending calculation
2. Click Reject
3. Enter rejection reason
4. Confirm rejection
5. Verify notification

**Expected Results:**
- Calculation rejected
- Reason recorded
- Submitter notified
- Can be resubmitted

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-APR-004: Bulk Approve

| Field | Value |
|-------|-------|
| **ID** | UAT-APR-004 |
| **Title** | Approve multiple calculations at once |
| **Priority** | High |
| **User Role** | Finance Manager |

**Test Steps:**
1. View pending approvals
2. Select multiple items
3. Click Bulk Approve
4. Add comments
5. Confirm

**Expected Results:**
- All selected items approved
- Single audit entry for batch
- Submitters notified
- Summary displayed

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-APR-005: Multi-Level Approval

| Field | Value |
|-------|-------|
| **ID** | UAT-APR-005 |
| **Title** | Navigate multi-level approval chain |
| **Priority** | Critical |
| **User Role** | Sales Manager, Finance Manager |

**Test Steps:**
1. Submit calculation > ₹50,000
2. First level manager approves
3. Auto-route to finance
4. Finance manager approves
5. Final status updated

**Expected Results:**
- Correct routing based on amount
- Each level tracked
- Final approval completes flow
- All approvers logged

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-APR-006: Delegate Approval

| Field | Value |
|-------|-------|
| **ID** | UAT-APR-006 |
| **Title** | Delegate approval to another user |
| **Priority** | High |
| **User Role** | Finance Manager |

**Test Steps:**
1. Open pending approval
2. Click Delegate
3. Select delegate
4. Enter reason
5. Confirm delegation

**Expected Results:**
- Approval delegated
- Delegate receives notification
- Original approver logged
- Delegation reason recorded

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-APR-007: Request More Information

| Field | Value |
|-------|-------|
| **ID** | UAT-APR-007 |
| **Title** | Request additional info from submitter |
| **Priority** | Medium |
| **User Role** | Finance Manager |

**Test Steps:**
1. Open pending approval
2. Click Request Info
3. Enter questions
4. Submit request
5. Verify notification

**Expected Results:**
- Request sent to submitter
- Status changes to Info Requested
- Submitter can respond
- Approval paused

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-APR-008: View Approval History

| Field | Value |
|-------|-------|
| **ID** | UAT-APR-008 |
| **Title** | View complete approval trail |
| **Priority** | High |
| **User Role** | System Administrator |

**Test Steps:**
1. Select approved calculation
2. View approval history
3. See all approval steps
4. See comments at each step
5. See timestamps

**Expected Results:**
- Complete history visible
- All approvers shown
- Comments displayed
- Timeline accurate

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-APR-009: Approval Dashboard

| Field | Value |
|-------|-------|
| **ID** | UAT-APR-009 |
| **Title** | View approval dashboard |
| **Priority** | High |
| **User Role** | Finance Manager |

**Test Steps:**
1. Navigate to Approvals
2. View pending count
3. View overdue items
4. Filter by status
5. Sort by date

**Expected Results:**
- Clear dashboard view
- Pending count accurate
- Overdue highlighted
- Filters work correctly

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-APR-010: Recall Submission

| Field | Value |
|-------|-------|
| **ID** | UAT-APR-010 |
| **Title** | Recall submitted calculation |
| **Priority** | Medium |
| **User Role** | Sales Manager |

**Test Steps:**
1. View my submissions
2. Find pending submission
3. Click Recall
4. Enter reason
5. Confirm recall

**Expected Results:**
- Submission recalled
- Status reset to draft
- Approver notified
- Can edit and resubmit

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-APR-011: Approval Reminders

| Field | Value |
|-------|-------|
| **ID** | UAT-APR-011 |
| **Title** | Receive approval reminders |
| **Priority** | Medium |
| **User Role** | Finance Manager |

**Test Steps:**
1. Item pending for 2 days
2. Verify reminder sent
3. Item pending for 5 days
4. Verify escalation
5. Check notification settings

**Expected Results:**
- Reminder at 2 days
- Escalation at 5 days
- Email notifications work
- In-app notifications work

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-APR-012: Partial Approval

| Field | Value |
|-------|-------|
| **ID** | UAT-APR-012 |
| **Title** | Approve some, reject others |
| **Priority** | High |
| **User Role** | Finance Manager |

**Test Steps:**
1. View batch submission
2. Approve items 1-3
3. Reject item 4
4. Request info for item 5
5. Complete actions

**Expected Results:**
- Each item handled separately
- Mixed status batch
- Appropriate notifications
- Clear tracking

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-APR-013: Approval Thresholds

| Field | Value |
|-------|-------|
| **ID** | UAT-APR-013 |
| **Title** | Verify threshold-based routing |
| **Priority** | High |
| **User Role** | System Administrator |

**Test Steps:**
1. Submit item < ₹25,000 (single level)
2. Submit item ₹25-50,000 (two levels)
3. Submit item > ₹50,000 (three levels)
4. Verify correct routing
5. Check threshold configuration

**Expected Results:**
- Routing matches thresholds
- Correct approvers assigned
- Threshold rules visible
- Can modify thresholds

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-APR-014: Out-of-Office Handling

| Field | Value |
|-------|-------|
| **ID** | UAT-APR-014 |
| **Title** | Handle approver out of office |
| **Priority** | Medium |
| **User Role** | Finance Manager |

**Test Steps:**
1. Set out-of-office delegate
2. Submit item requiring approval
3. Verify auto-delegation
4. Return from OOO
5. Verify normal routing resumes

**Expected Results:**
- Delegate receives approval
- Original approver informed
- OOO period tracked
- Normal flow resumes after

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-APR-015: Approval SLA Tracking

| Field | Value |
|-------|-------|
| **ID** | UAT-APR-015 |
| **Title** | Track approval SLA compliance |
| **Priority** | Medium |
| **User Role** | System Administrator |

**Test Steps:**
1. View approval metrics
2. Check average approval time
3. View SLA breaches
4. Identify bottlenecks
5. Export SLA report

**Expected Results:**
- Average time calculated
- SLA breaches highlighted
- Bottleneck identification
- Report exportable

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

## Reporting Scenarios

### UAT-RPT-001: Generate Payout Report

| Field | Value |
|-------|-------|
| **ID** | UAT-RPT-001 |
| **Title** | Generate monthly payout report |
| **Priority** | Critical |
| **User Role** | Finance Analyst |

> *"Grandma had hair like that when she went to sleep in her forever box."* - Ralph Wiggum
>
> Our reports live forever - always accessible and always accurate!

**Test Steps:**
1. Navigate to Reports
2. Select Payout Report
3. Choose period
4. Apply filters
5. Generate report
6. Export to Excel

**Expected Results:**
- Report generates quickly
- Data matches calculations
- Export works correctly
- Formatting professional

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-RPT-002: Generate Achievement Summary

| Field | Value |
|-------|-------|
| **ID** | UAT-RPT-002 |
| **Title** | Generate achievement summary report |
| **Priority** | High |
| **User Role** | Sales Manager |

**Test Steps:**
1. Select Achievement Summary
2. Choose team
3. Select period
4. Generate report
5. View visualizations

**Expected Results:**
- Team performance visible
- Charts render correctly
- Can drill down
- Export options available

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-RPT-003: Generate Variance Report

| Field | Value |
|-------|-------|
| **ID** | UAT-RPT-003 |
| **Title** | Compare budget vs actual payouts |
| **Priority** | High |
| **User Role** | Finance Manager |

**Test Steps:**
1. Select Variance Report
2. Choose plan
3. Select period
4. View variance analysis
5. Drill into anomalies

**Expected Results:**
- Budget vs actual clear
- Variance highlighted
- Root cause visible
- Exportable

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-RPT-004: Schedule Automated Report

| Field | Value |
|-------|-------|
| **ID** | UAT-RPT-004 |
| **Title** | Schedule report for automatic delivery |
| **Priority** | Medium |
| **User Role** | Finance Analyst |

**Test Steps:**
1. Create report
2. Click Schedule
3. Set frequency (daily/weekly/monthly)
4. Add recipients
5. Save schedule

**Expected Results:**
- Schedule saved
- Report delivered on time
- Recipients receive email
- Can modify/cancel schedule

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-RPT-005: Custom Report Builder

| Field | Value |
|-------|-------|
| **ID** | UAT-RPT-005 |
| **Title** | Build custom ad-hoc report |
| **Priority** | Medium |
| **User Role** | Finance Analyst |

**Test Steps:**
1. Open Report Builder
2. Select data fields
3. Add filters
4. Add grouping
5. Preview and save

**Expected Results:**
- Field selection intuitive
- Filters work correctly
- Preview accurate
- Can save for reuse

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-RPT-006: Dashboard Overview

| Field | Value |
|-------|-------|
| **ID** | UAT-RPT-006 |
| **Title** | View executive dashboard |
| **Priority** | High |
| **User Role** | Finance Manager |

**Test Steps:**
1. Navigate to Dashboard
2. View KPI widgets
3. Check YTD metrics
4. View trend charts
5. Drill into details

**Expected Results:**
- Dashboard loads quickly
- KPIs accurate
- Charts interactive
- Drill-down works

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-RPT-007: Employee Incentive Statement

| Field | Value |
|-------|-------|
| **ID** | UAT-RPT-007 |
| **Title** | Generate individual employee statement |
| **Priority** | High |
| **User Role** | Sales Executive |

**Test Steps:**
1. Navigate to My Incentives
2. Select period
3. Click Download Statement
4. View PDF statement
5. Verify all details

**Expected Results:**
- Professional format
- All calculations shown
- Can print or save
- Company branding visible

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-RPT-008: Audit Report

| Field | Value |
|-------|-------|
| **ID** | UAT-RPT-008 |
| **Title** | Generate audit trail report |
| **Priority** | Medium |
| **User Role** | System Administrator |

**Test Steps:**
1. Select Audit Report
2. Choose date range
3. Filter by entity type
4. Filter by action
5. Generate report

**Expected Results:**
- Complete audit trail
- All actions captured
- User details shown
- Timestamps accurate

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-RPT-009: Plan Performance Report

| Field | Value |
|-------|-------|
| **ID** | UAT-RPT-009 |
| **Title** | Analyze plan effectiveness |
| **Priority** | Medium |
| **User Role** | System Administrator |

**Test Steps:**
1. Select Plan Performance
2. Choose plans to compare
3. View participation metrics
4. View payout distribution
5. Analyze ROI

**Expected Results:**
- Clear plan comparison
- Distribution charts accurate
- ROI calculated
- Recommendations shown

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-RPT-010: Export to Payroll

| Field | Value |
|-------|-------|
| **ID** | UAT-RPT-010 |
| **Title** | Export payout file for payroll |
| **Priority** | Critical |
| **User Role** | Finance Analyst |

**Test Steps:**
1. Select approved payouts
2. Choose payroll format
3. Preview file
4. Generate export
5. Verify file integrity

**Expected Results:**
- Format matches payroll system
- All required fields included
- File validates successfully
- Can redownload if needed

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

## Administration Scenarios

### UAT-ADM-001: User Management

| Field | Value |
|-------|-------|
| **ID** | UAT-ADM-001 |
| **Title** | Create and manage system users |
| **Priority** | Critical |
| **User Role** | System Administrator |

> *"That's my swingset. I have to go now."* - Ralph Wiggum
>
> Our admins don't have to go - user management is a breeze!

**Test Steps:**
1. Navigate to User Management
2. Create new user
3. Assign roles
4. Set permissions
5. Save user

**Expected Results:**
- User created successfully
- Roles assigned
- Login works
- Access correct

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-ADM-002: Role Configuration

| Field | Value |
|-------|-------|
| **ID** | UAT-ADM-002 |
| **Title** | Configure role permissions |
| **Priority** | High |
| **User Role** | System Administrator |

**Test Steps:**
1. Navigate to Roles
2. Select role
3. Modify permissions
4. Save changes
5. Verify user access

**Expected Results:**
- Permissions updated
- Changes immediate
- Users affected correctly
- Audit logged

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-ADM-003: System Configuration

| Field | Value |
|-------|-------|
| **ID** | UAT-ADM-003 |
| **Title** | Configure system settings |
| **Priority** | High |
| **User Role** | System Administrator |

**Test Steps:**
1. Navigate to Settings
2. Configure approval thresholds
3. Set notification preferences
4. Configure calculation parameters
5. Save settings

**Expected Results:**
- Settings saved
- Changes effective
- Validation applied
- History tracked

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-ADM-004: View System Logs

| Field | Value |
|-------|-------|
| **ID** | UAT-ADM-004 |
| **Title** | View and analyze system logs |
| **Priority** | Medium |
| **User Role** | System Administrator |

**Test Steps:**
1. Navigate to System Logs
2. Filter by severity
3. Search by keyword
4. View log details
5. Export logs

**Expected Results:**
- Logs accessible
- Filters work
- Details clear
- Export works

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-ADM-005: Data Import Configuration

| Field | Value |
|-------|-------|
| **ID** | UAT-ADM-005 |
| **Title** | Configure data import mappings |
| **Priority** | Medium |
| **User Role** | System Administrator |

**Test Steps:**
1. Navigate to Import Config
2. Map source fields
3. Set validation rules
4. Test mapping
5. Save configuration

**Expected Results:**
- Mapping saved
- Validation works
- Test import succeeds
- Ready for use

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-ADM-006: Backup and Restore

| Field | Value |
|-------|-------|
| **ID** | UAT-ADM-006 |
| **Title** | Verify backup and restore |
| **Priority** | High |
| **User Role** | System Administrator |

**Test Steps:**
1. Initiate manual backup
2. Verify backup created
3. Restore from backup
4. Verify data integrity
5. Check scheduled backups

**Expected Results:**
- Backup completes
- Restore works
- Data intact
- Schedules active

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-ADM-007: Notification Templates

| Field | Value |
|-------|-------|
| **ID** | UAT-ADM-007 |
| **Title** | Configure notification templates |
| **Priority** | Medium |
| **User Role** | System Administrator |

**Test Steps:**
1. Navigate to Templates
2. Edit email template
3. Use placeholders
4. Preview template
5. Save and test

**Expected Results:**
- Template editable
- Placeholders work
- Preview accurate
- Test email received

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-ADM-008: Integration Monitoring

| Field | Value |
|-------|-------|
| **ID** | UAT-ADM-008 |
| **Title** | Monitor external integrations |
| **Priority** | Medium |
| **User Role** | System Administrator |

**Test Steps:**
1. Navigate to Integrations
2. View integration status
3. Check last sync times
4. View error logs
5. Retry failed syncs

**Expected Results:**
- Status visible
- Last sync shown
- Errors detailed
- Retry works

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

## Edge Cases and Error Handling

### UAT-EDGE-001: Session Timeout

| Field | Value |
|-------|-------|
| **ID** | UAT-EDGE-001 |
| **Title** | Handle session timeout gracefully |
| **Priority** | High |
| **User Role** | Any |

> *"My daddy's name is Homer and he snores like a buffalo."* - Ralph Wiggum
>
> Our session handling is more graceful than Homer's snoring!

**Test Steps:**
1. Login to system
2. Wait for session timeout (30 min)
3. Attempt action
4. Verify redirect to login
5. Verify data not lost

**Expected Results:**
- Clear timeout message
- Redirected to login
- Unsaved work warned
- Can resume after login

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-EDGE-002: Concurrent Editing

| Field | Value |
|-------|-------|
| **ID** | UAT-EDGE-002 |
| **Title** | Handle concurrent edit conflict |
| **Priority** | High |
| **User Role** | Multiple |

**Test Steps:**
1. User A opens employee record
2. User B opens same record
3. User A saves changes
4. User B tries to save
5. Verify conflict handling

**Expected Results:**
- Conflict detected
- User B notified
- Options to refresh or merge
- No data loss

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-EDGE-003: Large Data Set Performance

| Field | Value |
|-------|-------|
| **ID** | UAT-EDGE-003 |
| **Title** | Handle large data volumes |
| **Priority** | Critical |
| **User Role** | Finance Analyst |

**Test Steps:**
1. Select all employees (1000+)
2. Run batch calculation
3. Generate full report
4. Export large dataset
5. Verify performance

**Expected Results:**
- No timeout errors
- Progress indicator shown
- Results accurate
- Export completes

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-EDGE-004: Network Interruption

| Field | Value |
|-------|-------|
| **ID** | UAT-EDGE-004 |
| **Title** | Recover from network failure |
| **Priority** | Medium |
| **User Role** | Any |

**Test Steps:**
1. Start saving data
2. Disconnect network
3. Observe error handling
4. Reconnect network
5. Retry operation

**Expected Results:**
- Clear error message
- Retry option shown
- Data preserved
- Successful retry

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

### UAT-EDGE-005: Invalid Data Input

| Field | Value |
|-------|-------|
| **ID** | UAT-EDGE-005 |
| **Title** | Validate all input fields |
| **Priority** | High |
| **User Role** | Any |

**Test Steps:**
1. Enter special characters
2. Enter very long text
3. Enter negative numbers
4. Enter invalid dates
5. Submit forms

**Expected Results:**
- Validation catches errors
- Clear error messages
- No crashes
- XSS prevented

**Actual Results:** _______________

**Status:** ⬜ Pass / ⬜ Fail / ⬜ Blocked

---

## UAT Sign-off

### Summary

| Category | Total | Passed | Failed | Blocked |
|----------|-------|--------|--------|---------|
| Employee Management | 15 | ___ | ___ | ___ |
| Incentive Plans | 12 | ___ | ___ | ___ |
| Calculations | 20 | ___ | ___ | ___ |
| Approval Workflows | 15 | ___ | ___ | ___ |
| Reporting | 10 | ___ | ___ | ___ |
| Administration | 8 | ___ | ___ | ___ |
| Edge Cases | 5 | ___ | ___ | ___ |
| **Total** | **85** | **___** | **___** | **___** |

### Pass Rate: ____%

### Outstanding Issues

| ID | Severity | Description | Status |
|----|----------|-------------|--------|
| | | | |
| | | | |
| | | | |

### Final Sign-off

> *"Wheee! I'm doing things!"* - Ralph Wiggum
>
> We've done all the things - UAT is complete!

| Role | Name | Signature | Date |
|------|------|-----------|------|
| Product Owner | _____________ | _____________ | _______ |
| QA Lead | _____________ | _____________ | _______ |
| Business Analyst | _____________ | _____________ | _______ |
| Project Manager | _____________ | _____________ | _______ |

### Acceptance Statement

I hereby confirm that User Acceptance Testing has been completed satisfactorily and the Dorise Sales Incentive Framework (DSIF) is approved for production deployment.

**Product Owner Signature:** ___________________________

**Date:** _______________

---

*This document is part of the DSIF Quality Gate Framework - QG-5 (Testing Gate)*
