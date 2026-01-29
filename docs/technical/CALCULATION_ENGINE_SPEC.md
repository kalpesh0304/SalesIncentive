# DORISE Sales Incentive Framework
## Calculation Engine Specification

**Document ID:** DOC-TECH-002
**Project:** Dorise Sales Incentive Framework (DSIF)
**Version:** 1.0
**Created:** January 2025
**Last Updated:** January 2025
**Author:** Claude Code
**Status:** Active

> *"I choo-choo-choose you!"* - Ralph Wiggum
>
> Our calculation engine carefully chooses the right formula for each employee's situation.

---

## Table of Contents

1. [Overview](#1-overview)
2. [Calculation Concepts](#2-calculation-concepts)
3. [Calculation Process](#3-calculation-process)
4. [Slab Configuration](#4-slab-configuration)
5. [Point-in-Time Capture](#5-point-in-time-capture)
6. [Proration Logic](#6-proration-logic)
7. [Split Share Calculations](#7-split-share-calculations)
8. [Amendment Processing](#8-amendment-processing)
9. [Calculation Formulas](#9-calculation-formulas)
10. [Batch Processing](#10-batch-processing)
11. [Error Handling](#11-error-handling)
12. [Testing Specifications](#12-testing-specifications)

---

## 1. Overview

### 1.1 Purpose

This document provides the complete technical specification for the DSIF Calculation Engine, which is responsible for computing sales incentives based on configurable plans, slabs, and employee performance data.

### 1.2 Scope

The Calculation Engine handles:
- Automated batch calculations (daily, weekly, monthly)
- Manual calculation triggers
- Point-in-time value capture
- Slab-based incentive computation
- Split share allocation
- Proration for partial periods
- Amendment and adjustment processing

### 1.3 Key Design Principles

| Principle | Description |
|-----------|-------------|
| **Immutability** | Calculation records cannot be modified after creation |
| **Auditability** | All inputs and outputs are captured for audit purposes |
| **Reproducibility** | Same inputs must always produce same outputs |
| **Point-in-Time** | All data captured as of calculation date |
| **Configurability** | Business rules driven by configuration, not code |

---

## 2. Calculation Concepts

### 2.1 Terminology

| Term | Definition |
|------|------------|
| **Calculation Period** | Time range for incentive calculation (usually monthly: YYYY-MM) |
| **Effective Period** | Date range when an assignment/plan is active |
| **Slab** | A threshold-based tier that determines incentive rate |
| **Achievement** | The metric value used to determine slab selection |
| **Base Amount** | The calculated incentive before adjustments |
| **Gross Amount** | Base amount after slab application |
| **Net Amount** | Final amount after all adjustments |
| **Proration Factor** | Decimal representing partial period (0.0 to 1.0) |
| **Split Share** | Percentage of incentive allocated to each employee |

### 2.2 Calculation Hierarchy

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         CALCULATION HIERARCHY                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────┐                                                        │
│  │ Calculation     │ One per employee per period                            │
│  │ Batch           │ Contains all calculations for a period                 │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           │ 1:N                                                              │
│           ▼                                                                  │
│  ┌─────────────────┐                                                        │
│  │ Calculation     │ One per employee-assignment per period                 │
│  │ Record          │ Header with gross/net amounts                          │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           │ 1:N                                                              │
│           ▼                                                                  │
│  ┌─────────────────┐                                                        │
│  │ Calculation     │ Line items showing calculation breakdown               │
│  │ Detail          │ Metrics, slabs applied, rates, amounts                 │
│  └────────┬────────┘                                                        │
│           │                                                                  │
│           │ 0:N                                                              │
│           ▼                                                                  │
│  ┌─────────────────┐                                                        │
│  │ Adjustment      │ Proration, caps, clawbacks                             │
│  │ Record          │ Modifications to base calculation                       │
│  └─────────────────┘                                                        │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 2.3 Calculation States

```
┌──────────┐     ┌───────────┐     ┌──────────┐     ┌──────────┐
│ Created  │────▶│ Calculated│────▶│ Pending  │────▶│ Approved │
└──────────┘     └───────────┘     │ Approval │     └──────────┘
                                   └──────────┘           │
                                        │                 │
                                        │                 ▼
                                        │           ┌──────────┐
                                        └──────────▶│ Rejected │
                                                    └──────────┘
                                                          │
                                                          │
                                                          ▼
                                                    ┌──────────┐
                                                    │ Amended  │
                                                    └──────────┘
```

---

## 3. Calculation Process

### 3.1 Process Flow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      CALCULATION PROCESS FLOW                                │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ STEP 1: INITIALIZATION                                               │    │
│  │ • Create batch record                                                │    │
│  │ • Validate period format                                             │    │
│  │ • Capture batch start timestamp                                      │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                      │                                       │
│                                      ▼                                       │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ STEP 2: EMPLOYEE IDENTIFICATION                                      │    │
│  │ • Get active employees for period                                    │    │
│  │ • Filter by calculation criteria                                     │    │
│  │ • Capture employee snapshot (point-in-time)                          │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                      │                                       │
│                                      ▼                                       │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ STEP 3: DATA COLLECTION (per employee)                               │    │
│  │ • Get active assignments for period                                  │    │
│  │ • Get applicable incentive plan                                      │    │
│  │ • Get performance metrics from source                                │    │
│  │ • Capture all values as point-in-time snapshot                       │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                      │                                       │
│                                      ▼                                       │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ STEP 4: PRORATION CALCULATION                                        │    │
│  │ • Calculate days active in period                                    │    │
│  │ • Determine proration factor                                         │    │
│  │ • Handle multiple assignments                                        │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                      │                                       │
│                                      ▼                                       │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ STEP 5: SLAB DETERMINATION                                           │    │
│  │ • Evaluate achievement against thresholds                            │    │
│  │ • Select applicable slab                                             │    │
│  │ • Get rate/amount from slab                                          │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                      │                                       │
│                                      ▼                                       │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ STEP 6: INCENTIVE CALCULATION                                        │    │
│  │ • Apply formula based on plan configuration                          │    │
│  │ • Calculate base incentive amount                                    │    │
│  │ • Apply proration factor                                             │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                      │                                       │
│                                      ▼                                       │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ STEP 7: SPLIT SHARE ALLOCATION                                       │    │
│  │ • Identify split share configuration                                 │    │
│  │ • Allocate amount based on percentages                               │    │
│  │ • Create records for each beneficiary                                │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                      │                                       │
│                                      ▼                                       │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ STEP 8: ADJUSTMENTS                                                  │    │
│  │ • Apply caps (if configured)                                         │    │
│  │ • Apply minimum thresholds                                           │    │
│  │ • Process clawbacks (if any)                                         │    │
│  │ • Round to 2 decimal places                                          │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                      │                                       │
│                                      ▼                                       │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ STEP 9: PERSISTENCE                                                  │    │
│  │ • Create calculation record (immutable)                              │    │
│  │ • Create detail records                                              │    │
│  │ • Create adjustment records                                          │    │
│  │ • Generate audit log                                                 │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                      │                                       │
│                                      ▼                                       │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ STEP 10: FINALIZATION                                                │    │
│  │ • Update batch status                                                │    │
│  │ • Generate batch summary                                             │    │
│  │ • Trigger approval workflow                                          │    │
│  │ • Send notifications                                                 │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 3.2 Process Implementation

```csharp
public class CalculationEngine : ICalculationEngine
{
    public async Task<CalculationBatch> ExecuteBatchAsync(
        string period,
        CalculationOptions options,
        CancellationToken cancellationToken)
    {
        // Step 1: Initialize batch
        var batch = CalculationBatch.Create(period, options);
        _logger.LogInformation("Starting calculation batch {BatchId} for period {Period}",
            batch.Id, period);

        try
        {
            // Step 2: Get eligible employees
            var employees = await _employeeRepository
                .GetActiveForPeriodAsync(period, cancellationToken);

            _logger.LogInformation("Found {Count} eligible employees for calculation", employees.Count());

            // Process each employee
            foreach (var employee in employees)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var calculation = await CalculateForEmployeeAsync(
                    employee, period, batch.Id, cancellationToken);

                if (calculation is not null)
                {
                    batch.AddCalculation(calculation);
                }
            }

            // Finalize batch
            batch.Complete();

            // Persist
            await _batchRepository.AddAsync(batch, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "Completed calculation batch {BatchId}. Processed: {Processed}, Succeeded: {Succeeded}, Failed: {Failed}",
                batch.Id, batch.TotalCount, batch.SuccessCount, batch.FailureCount);

            return batch;
        }
        catch (Exception ex)
        {
            batch.Fail(ex.Message);
            await _batchRepository.UpdateAsync(batch, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            throw;
        }
    }
}
```

---

## 4. Slab Configuration

### 4.1 Slab Structure

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                          SLAB CONFIGURATION                                  │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  INCENTIVE PLAN: Sales Representative Plan (2025)                           │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ SLAB 1: Bronze                                                       │    │
│  │ Threshold: 0 - 50,000                                                │    │
│  │ Rate Type: Percentage                                                │    │
│  │ Rate: 2.0%                                                           │    │
│  │ Cap: None                                                            │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ SLAB 2: Silver                                                       │    │
│  │ Threshold: 50,001 - 100,000                                          │    │
│  │ Rate Type: Percentage                                                │    │
│  │ Rate: 3.0%                                                           │    │
│  │ Cap: None                                                            │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ SLAB 3: Gold                                                         │    │
│  │ Threshold: 100,001 - 200,000                                         │    │
│  │ Rate Type: Percentage                                                │    │
│  │ Rate: 4.0%                                                           │    │
│  │ Cap: None                                                            │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
│  ┌─────────────────────────────────────────────────────────────────────┐    │
│  │ SLAB 4: Platinum                                                     │    │
│  │ Threshold: 200,001+                                                  │    │
│  │ Rate Type: Percentage                                                │    │
│  │ Rate: 5.0%                                                           │    │
│  │ Cap: $15,000                                                         │    │
│  └─────────────────────────────────────────────────────────────────────┘    │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Slab Selection Algorithm

```csharp
public class SlabSelector : ISlabSelector
{
    /// <summary>
    /// Selects the applicable slab based on achievement value.
    /// Uses the HIGHEST qualifying slab (achievement >= MinThreshold).
    /// </summary>
    public PlanSlab SelectSlab(IEnumerable<PlanSlab> slabs, decimal achievement)
    {
        if (!slabs.Any())
            throw new DomainException("No slabs configured for plan.");

        // Order slabs by threshold descending
        var orderedSlabs = slabs.OrderByDescending(s => s.MinThreshold).ToList();

        // Find first slab where achievement >= MinThreshold
        var selectedSlab = orderedSlabs.FirstOrDefault(s => achievement >= s.MinThreshold);

        if (selectedSlab is null)
        {
            // Achievement below minimum threshold - use lowest slab
            selectedSlab = orderedSlabs.Last();
        }

        return selectedSlab;
    }
}

// Slab entity
public class PlanSlab : Entity
{
    public PlanVersionId PlanVersionId { get; private set; }
    public int SlabOrder { get; private set; }
    public string SlabName { get; private set; }
    public decimal MinThreshold { get; private set; }
    public decimal? MaxThreshold { get; private set; }
    public RateType RateType { get; private set; }
    public decimal RateValue { get; private set; }
    public decimal? CapAmount { get; private set; }

    public bool IsInRange(decimal value)
    {
        return value >= MinThreshold &&
               (MaxThreshold is null || value <= MaxThreshold);
    }

    public decimal CalculateIncentive(decimal baseValue)
    {
        var incentive = RateType switch
        {
            RateType.Percentage => baseValue * (RateValue / 100),
            RateType.Fixed => RateValue,
            RateType.PerUnit => baseValue * RateValue,
            _ => throw new DomainException($"Unknown rate type: {RateType}")
        };

        // Apply cap if configured
        if (CapAmount.HasValue && incentive > CapAmount.Value)
        {
            incentive = CapAmount.Value;
        }

        return Math.Round(incentive, 2);
    }
}
```

### 4.3 Tiered Calculation

For tiered (cumulative) calculations, incentive is calculated for each tier:

```csharp
public class TieredCalculator : ITieredCalculator
{
    public CalculationBreakdown CalculateTiered(
        IEnumerable<PlanSlab> slabs,
        decimal achievement)
    {
        var breakdown = new CalculationBreakdown();
        var orderedSlabs = slabs.OrderBy(s => s.MinThreshold).ToList();
        var remainingValue = achievement;

        foreach (var slab in orderedSlabs)
        {
            if (remainingValue <= 0)
                break;

            var slabRange = (slab.MaxThreshold ?? decimal.MaxValue) - slab.MinThreshold;
            var applicableValue = Math.Min(remainingValue, slabRange);

            if (applicableValue > 0)
            {
                var slabIncentive = slab.CalculateIncentive(applicableValue);
                breakdown.AddLine(new CalculationLine
                {
                    SlabName = slab.SlabName,
                    BaseValue = applicableValue,
                    Rate = slab.RateValue,
                    RateType = slab.RateType,
                    Amount = slabIncentive
                });

                remainingValue -= applicableValue;
            }
        }

        return breakdown;
    }
}
```

> *"Me fail English? That's unpossible!"* - Ralph Wiggum
>
> With properly configured slabs, calculation errors are unpossible!

---

## 5. Point-in-Time Capture

### 5.1 Snapshot Requirements

All input data must be captured as it existed at the **calculation date** to ensure reproducibility.

| Data Element | Capture Method | Storage |
|--------------|----------------|---------|
| Employee Details | Temporal query | CalculationSnapshot.Employee |
| Assignment | Temporal query | CalculationSnapshot.Assignment |
| Role | Temporal query | CalculationSnapshot.Role |
| Plan Version | Version ID | CalculationRecord.PlanVersionId |
| Slab Configuration | Snapshot JSON | CalculationSnapshot.PlanConfig |
| Sales Metrics | Staging table | CalculationDetail.Metrics |
| Exchange Rates | Snapshot | CalculationSnapshot.ExchangeRates |

### 5.2 Temporal Query Implementation

```csharp
public class PointInTimeRepository : IPointInTimeRepository
{
    private readonly IncentiveDbContext _context;

    public async Task<Employee?> GetEmployeeAsOfAsync(
        EmployeeId employeeId,
        DateTime asOfDate,
        CancellationToken cancellationToken)
    {
        // Use SQL Server temporal table FOR SYSTEM_TIME AS OF
        return await _context.Employees
            .TemporalAsOf(asOfDate)
            .Include(e => e.Assignments.Where(a =>
                a.EffectiveFrom <= asOfDate &&
                (a.EffectiveTo == null || a.EffectiveTo >= asOfDate)))
            .FirstOrDefaultAsync(e => e.Id == employeeId, cancellationToken);
    }

    public async Task<IEnumerable<Assignment>> GetAssignmentsAsOfAsync(
        EmployeeId employeeId,
        DateOnly periodStart,
        DateOnly periodEnd,
        CancellationToken cancellationToken)
    {
        // Get all assignments that overlapped with the period
        return await _context.Assignments
            .TemporalBetween(periodStart.ToDateTime(TimeOnly.MinValue),
                            periodEnd.ToDateTime(TimeOnly.MaxValue))
            .Where(a => a.EmployeeId == employeeId)
            .Where(a => a.EffectiveFrom <= periodEnd.ToDateTime(TimeOnly.MinValue))
            .Where(a => a.EffectiveTo == null ||
                       a.EffectiveTo >= periodStart.ToDateTime(TimeOnly.MinValue))
            .ToListAsync(cancellationToken);
    }
}
```

### 5.3 Calculation Snapshot Entity

```csharp
public class CalculationSnapshot : ValueObject
{
    public string EmployeeNumber { get; }
    public string EmployeeName { get; }
    public string RoleName { get; }
    public string StoreName { get; }
    public string ZoneName { get; }
    public string PlanName { get; }
    public int PlanVersion { get; }
    public DateTime SnapshotTimestamp { get; }
    public string PlanConfigJson { get; }  // Full plan config as JSON

    // For audit reconstruction
    public static CalculationSnapshot Capture(
        Employee employee,
        Assignment assignment,
        IncentivePlan plan)
    {
        return new CalculationSnapshot
        {
            EmployeeNumber = employee.EmployeeNumber,
            EmployeeName = employee.FullName,
            RoleName = assignment.Role.Name,
            StoreName = assignment.Store.Name,
            ZoneName = assignment.Store.Zone.Name,
            PlanName = plan.Name,
            PlanVersion = plan.CurrentVersion.VersionNumber,
            SnapshotTimestamp = DateTime.UtcNow,
            PlanConfigJson = JsonSerializer.Serialize(plan.CurrentVersion)
        };
    }
}
```

---

## 6. Proration Logic

### 6.1 Proration Scenarios

| Scenario | Proration Method |
|----------|------------------|
| Full period active | Factor = 1.0 |
| Mid-month hire | Days active / Days in period |
| Mid-month termination | Days active / Days in period |
| Role change mid-month | Calculate for each role period |
| Store transfer | Calculate for each store period |
| Leave of absence | Exclude leave days |

### 6.2 Proration Calculator

```csharp
public class ProrationCalculator : IProrationCalculator
{
    /// <summary>
    /// Calculates the proration factor for a partial period.
    /// </summary>
    public decimal CalculateProrationFactor(
        DateOnly periodStart,
        DateOnly periodEnd,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo)
    {
        // Determine actual active period within calculation period
        var activeFrom = effectiveFrom > periodStart ? effectiveFrom : periodStart;
        var activeTo = effectiveTo.HasValue && effectiveTo.Value < periodEnd
            ? effectiveTo.Value
            : periodEnd;

        // Handle case where assignment doesn't overlap period
        if (activeFrom > activeTo)
            return 0m;

        var daysInPeriod = periodEnd.DayNumber - periodStart.DayNumber + 1;
        var daysActive = activeTo.DayNumber - activeFrom.DayNumber + 1;

        var factor = (decimal)daysActive / daysInPeriod;

        return Math.Round(factor, 4);
    }

    /// <summary>
    /// Calculates proration for multiple assignment periods within a calculation period.
    /// </summary>
    public IEnumerable<ProratedPeriod> CalculateProratedPeriods(
        IEnumerable<Assignment> assignments,
        DateOnly periodStart,
        DateOnly periodEnd)
    {
        var periods = new List<ProratedPeriod>();

        foreach (var assignment in assignments.OrderBy(a => a.EffectiveFrom))
        {
            var factor = CalculateProrationFactor(
                periodStart, periodEnd,
                DateOnly.FromDateTime(assignment.EffectiveFrom),
                assignment.EffectiveTo.HasValue
                    ? DateOnly.FromDateTime(assignment.EffectiveTo.Value)
                    : null);

            if (factor > 0)
            {
                periods.Add(new ProratedPeriod
                {
                    Assignment = assignment,
                    ProrationFactor = factor,
                    ActiveFrom = DateOnly.FromDateTime(assignment.EffectiveFrom) > periodStart
                        ? DateOnly.FromDateTime(assignment.EffectiveFrom)
                        : periodStart,
                    ActiveTo = assignment.EffectiveTo.HasValue &&
                              DateOnly.FromDateTime(assignment.EffectiveTo.Value) < periodEnd
                        ? DateOnly.FromDateTime(assignment.EffectiveTo.Value)
                        : periodEnd
                });
            }
        }

        return periods;
    }
}

// Proration example
// Period: January 2025 (31 days)
// Employee hired: January 15, 2025
// Active days: 17 (Jan 15-31)
// Proration factor: 17/31 = 0.5484
```

### 6.3 Proration Application

```csharp
public class IncentiveCalculator : IIncentiveCalculator
{
    public CalculationResult Calculate(
        Employee employee,
        Assignment assignment,
        SalesMetrics metrics,
        IncentivePlan plan,
        string period)
    {
        var periodDates = ParsePeriod(period);

        // Calculate proration factor
        var prorationFactor = _prorationCalculator.CalculateProrationFactor(
            periodDates.Start,
            periodDates.End,
            DateOnly.FromDateTime(assignment.EffectiveFrom),
            assignment.EffectiveTo.HasValue
                ? DateOnly.FromDateTime(assignment.EffectiveTo.Value)
                : null);

        // Select slab based on achievement
        var slab = _slabSelector.SelectSlab(
            plan.CurrentVersion.Slabs,
            metrics.SalesAmount);

        // Calculate base incentive
        var baseIncentive = slab.CalculateIncentive(metrics.SalesAmount);

        // Apply proration
        var proratedIncentive = baseIncentive * prorationFactor;

        return new CalculationResult
        {
            EmployeeId = employee.Id,
            Period = period,
            GrossAmount = Math.Round(proratedIncentive, 2),
            ProrationFactor = prorationFactor,
            SlabApplied = slab.SlabName,
            RateApplied = slab.RateValue,
            Details = new[]
            {
                new CalculationDetail
                {
                    MetricType = "SalesAmount",
                    MetricValue = metrics.SalesAmount,
                    BaseAmount = baseIncentive,
                    ProrationFactor = prorationFactor,
                    ProratedAmount = proratedIncentive
                }
            }
        };
    }
}
```

---

## 7. Split Share Calculations

### 7.1 Split Share Concept

Split shares allow incentive allocation when:
- Multiple employees share a territory
- Manager/subordinate split arrangements
- Team-based incentive pools

### 7.2 Split Configuration

```json
{
  "assignmentId": "a1b2c3d4-...",
  "storeId": "store-001",
  "splits": [
    {
      "employeeId": "emp-001",
      "sharePercentage": 60.0,
      "role": "Primary"
    },
    {
      "employeeId": "emp-002",
      "sharePercentage": 40.0,
      "role": "Secondary"
    }
  ],
  "effectiveFrom": "2025-01-01",
  "effectiveTo": null
}
```

### 7.3 Split Calculation Logic

```csharp
public class SplitShareCalculator : ISplitShareCalculator
{
    /// <summary>
    /// Allocates calculated incentive among split share participants.
    /// </summary>
    public IEnumerable<SplitAllocation> AllocateSplitShares(
        decimal totalIncentive,
        IEnumerable<SplitShare> splits)
    {
        // Validate total percentage
        var totalPercentage = splits.Sum(s => s.SharePercentage);
        if (Math.Abs(totalPercentage - 100m) > 0.01m)
        {
            throw new DomainException(
                $"Split percentages must sum to 100%. Current total: {totalPercentage}%");
        }

        var allocations = new List<SplitAllocation>();
        var allocated = 0m;

        // Allocate to each participant
        var sortedSplits = splits.OrderByDescending(s => s.SharePercentage).ToList();

        for (int i = 0; i < sortedSplits.Count; i++)
        {
            var split = sortedSplits[i];
            decimal amount;

            if (i == sortedSplits.Count - 1)
            {
                // Last allocation gets remainder to avoid rounding issues
                amount = totalIncentive - allocated;
            }
            else
            {
                amount = Math.Round(totalIncentive * (split.SharePercentage / 100m), 2);
            }

            allocations.Add(new SplitAllocation
            {
                EmployeeId = split.EmployeeId,
                SharePercentage = split.SharePercentage,
                Amount = amount,
                Role = split.Role
            });

            allocated += amount;
        }

        return allocations;
    }
}

// Example:
// Total Incentive: $1,000.00
// Employee A: 60% = $600.00
// Employee B: 40% = $400.00
```

### 7.4 Split Share Validation Rules

| Rule | Validation |
|------|------------|
| Percentage Sum | Must equal 100% |
| Minimum Share | >= 1% per participant |
| Maximum Participants | <= 5 per assignment |
| Overlap Check | No duplicate employees |
| Effective Date | Splits must have valid date ranges |

---

## 8. Amendment Processing

### 8.1 Amendment Scenarios

| Scenario | Amendment Type |
|----------|----------------|
| Calculation error correction | Adjustment |
| Retroactive data change | Recalculation |
| Clawback (overpayment) | Negative adjustment |
| Make-good (underpayment) | Positive adjustment |
| Metrics correction | Full recalculation |

### 8.2 Amendment Workflow

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         AMENDMENT WORKFLOW                                   │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  ┌───────────────┐                                                          │
│  │ Amendment     │ User initiates amendment request                         │
│  │ Requested     │                                                          │
│  └───────┬───────┘                                                          │
│          │                                                                   │
│          ▼                                                                   │
│  ┌───────────────┐                                                          │
│  │ Reason        │ Mandatory reason with supporting documentation           │
│  │ Documented    │                                                          │
│  └───────┬───────┘                                                          │
│          │                                                                   │
│          ▼                                                                   │
│  ┌───────────────┐                                                          │
│  │ Original      │ Link to original calculation (immutable)                 │
│  │ Referenced    │                                                          │
│  └───────┬───────┘                                                          │
│          │                                                                   │
│          ▼                                                                   │
│  ┌───────────────┐                                                          │
│  │ New           │ Create new calculation record with IsAmendment = true    │
│  │ Calculation   │                                                          │
│  └───────┬───────┘                                                          │
│          │                                                                   │
│          ▼                                                                   │
│  ┌───────────────┐                                                          │
│  │ Approval      │ Amendment requires re-approval                           │
│  │ Required      │                                                          │
│  └───────┬───────┘                                                          │
│          │                                                                   │
│          ▼                                                                   │
│  ┌───────────────┐                                                          │
│  │ Audit Trail   │ Full audit record linking original and amendment         │
│  │ Created       │                                                          │
│  └───────────────┘                                                          │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 8.3 Amendment Implementation

```csharp
public class AmendmentService : IAmendmentService
{
    public async Task<Calculation> CreateAmendmentAsync(
        CalculationId originalCalculationId,
        AmendmentRequest request,
        CancellationToken cancellationToken)
    {
        // Get original calculation
        var original = await _calculationRepository
            .GetByIdAsync(originalCalculationId, cancellationToken)
            ?? throw new NotFoundException("Calculation", originalCalculationId);

        // Validate original can be amended
        if (original.Status == CalculationStatus.Exported)
        {
            throw new DomainException(
                "Cannot amend calculation that has been exported to payroll. " +
                "Use clawback process instead.");
        }

        // Calculate adjustment
        var adjustment = request.NewAmount - original.NetAmount;

        // Create amendment record
        var amendment = Calculation.CreateAmendment(
            employee: original.Employee,
            originalCalculation: original,
            reason: request.Reason,
            adjustment: adjustment,
            newAmount: request.NewAmount,
            requestedBy: request.RequestedBy);

        // Original remains unchanged (immutable)
        // But we mark it as superseded
        original.MarkSuperseded(amendment.Id);

        await _calculationRepository.AddAsync(amendment, cancellationToken);
        await _calculationRepository.UpdateAsync(original, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Trigger approval workflow
        await _mediator.Publish(new AmendmentCreatedEvent(amendment.Id, original.Id));

        _logger.LogInformation(
            "Created amendment {AmendmentId} for original calculation {OriginalId}. " +
            "Adjustment: {Adjustment:C}",
            amendment.Id, original.Id, adjustment);

        return amendment;
    }
}

// Amendment entity properties
public class Calculation
{
    public bool IsAmendment { get; private set; }
    public CalculationId? OriginalCalculationId { get; private set; }
    public string? AmendmentReason { get; private set; }
    public decimal? AdjustmentAmount { get; private set; }
    public CalculationId? SupersededByCalculationId { get; private set; }
}
```

### 8.4 Clawback Processing

```csharp
public class ClawbackService : IClawbackService
{
    public async Task<Clawback> ProcessClawbackAsync(
        EmployeeId employeeId,
        ClawbackRequest request,
        CancellationToken cancellationToken)
    {
        // Get previously paid amounts
        var paidCalculations = await _calculationRepository
            .GetExportedByEmployeeAsync(employeeId, request.Period, cancellationToken);

        var totalPaid = paidCalculations.Sum(c => c.NetAmount);

        // Validate clawback doesn't exceed paid amount
        if (request.ClawbackAmount > totalPaid)
        {
            throw new DomainException(
                $"Clawback amount ({request.ClawbackAmount:C}) cannot exceed " +
                $"total paid amount ({totalPaid:C}).");
        }

        // Create clawback record
        var clawback = Clawback.Create(
            employeeId,
            request.Period,
            request.ClawbackAmount,
            request.Reason,
            request.SourceCalculationIds);

        await _clawbackRepository.AddAsync(clawback, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return clawback;
    }
}
```

---

## 9. Calculation Formulas

### 9.1 Formula Types

| Formula Type | Description | Example |
|--------------|-------------|---------|
| **Percentage** | Rate as percentage of metric | `Amount = Metric * (Rate / 100)` |
| **Fixed** | Fixed amount per threshold | `Amount = FixedAmount` |
| **Per Unit** | Amount per unit sold | `Amount = Units * RatePerUnit` |
| **Tiered** | Cumulative tiers | Sum of each tier calculation |
| **Composite** | Multiple metrics combined | `Amount = (Sales * 0.02) + (Units * 5)` |

### 9.2 Formula Engine

```csharp
public interface IFormulaEngine
{
    decimal Evaluate(Formula formula, Dictionary<string, decimal> variables);
}

public class FormulaEngine : IFormulaEngine
{
    /// <summary>
    /// Evaluates a formula expression with provided variables.
    /// </summary>
    public decimal Evaluate(Formula formula, Dictionary<string, decimal> variables)
    {
        // Validate all required variables are provided
        foreach (var variable in formula.RequiredVariables)
        {
            if (!variables.ContainsKey(variable))
            {
                throw new DomainException(
                    $"Missing required variable '{variable}' for formula '{formula.Name}'.");
            }
        }

        // Parse and evaluate expression
        var expression = new Expression(formula.Expression);

        foreach (var (key, value) in variables)
        {
            expression.Parameters[key] = (double)value;
        }

        var result = expression.Evaluate();

        return Math.Round(Convert.ToDecimal(result), 2);
    }
}

// Formula configuration example
public class Formula : Entity
{
    public string Name { get; private set; }
    public string Expression { get; private set; }  // e.g., "[Sales] * ([Rate] / 100)"
    public string[] RequiredVariables { get; private set; }  // e.g., ["Sales", "Rate"]

    // Predefined formulas
    public static Formula Percentage(string metricName, string rateName)
        => new($"{metricName}Percentage", $"[{metricName}] * ([{rateName}] / 100)",
            new[] { metricName, rateName });

    public static Formula PerUnit(string unitsName, string rateName)
        => new($"{unitsName}PerUnit", $"[{unitsName}] * [{rateName}]",
            new[] { unitsName, rateName });
}
```

### 9.3 Sample Calculations

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                      CALCULATION EXAMPLE                                     │
├─────────────────────────────────────────────────────────────────────────────┤
│                                                                              │
│  EMPLOYEE: John Doe (EMP001)                                                │
│  PERIOD: January 2025                                                        │
│  PLAN: Sales Representative Plan v2                                          │
│                                                                              │
│  INPUT METRICS:                                                              │
│  ├─ Sales Amount: $85,000.00                                                │
│  ├─ Units Sold: 150                                                         │
│  └─ Sell-Through Rate: 85%                                                  │
│                                                                              │
│  SLAB SELECTION:                                                             │
│  ├─ Achievement: $85,000.00                                                 │
│  ├─ Selected Slab: Silver (50,001 - 100,000)                                │
│  └─ Rate: 3.0%                                                              │
│                                                                              │
│  CALCULATION:                                                                │
│  ├─ Base Incentive: $85,000 × 3.0% = $2,550.00                              │
│  ├─ Proration Factor: 1.0 (full month)                                      │
│  ├─ Prorated Amount: $2,550.00 × 1.0 = $2,550.00                            │
│  ├─ Split Share: 100% (no split)                                            │
│  └─ Gross Amount: $2,550.00                                                 │
│                                                                              │
│  ADJUSTMENTS:                                                                │
│  ├─ Minimum Threshold: Met                                                  │
│  ├─ Cap: None applied                                                       │
│  └─ Net Amount: $2,550.00                                                   │
│                                                                              │
│  RESULT:                                                                     │
│  └─ Final Incentive: $2,550.00                                              │
│                                                                              │
└─────────────────────────────────────────────────────────────────────────────┘
```

> *"Go Banana!"* - Ralph Wiggum
>
> When all inputs are correct, calculations are as simple as going banana!

---

## 10. Batch Processing

### 10.1 Batch Configuration

```csharp
public class BatchConfiguration
{
    public int MaxParallelism { get; set; } = 10;
    public int BatchSize { get; set; } = 100;
    public TimeSpan Timeout { get; set; } = TimeSpan.FromHours(2);
    public int MaxRetries { get; set; } = 3;
    public RetryPolicy RetryPolicy { get; set; } = RetryPolicy.ExponentialBackoff;
}
```

### 10.2 Batch Scheduler

```csharp
// Azure Function trigger for scheduled calculations
public class CalculationTriggerFunction
{
    private readonly ICalculationEngine _calculationEngine;
    private readonly ILogger<CalculationTriggerFunction> _logger;

    [Function("DailyCalculationTrigger")]
    public async Task RunDaily(
        [TimerTrigger("0 0 2 * * *")] TimerInfo timer,  // 2 AM daily
        FunctionContext context)
    {
        var period = DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd");
        _logger.LogInformation("Starting daily calculation for {Period}", period);

        await _calculationEngine.ExecuteBatchAsync(
            period,
            new CalculationOptions
            {
                CalculationType = CalculationType.Daily,
                IncludeInactive = false
            },
            context.CancellationToken);
    }

    [Function("MonthlyCalculationTrigger")]
    public async Task RunMonthly(
        [TimerTrigger("0 0 3 1 * *")] TimerInfo timer,  // 3 AM on 1st of month
        FunctionContext context)
    {
        var period = DateTime.UtcNow.AddMonths(-1).ToString("yyyy-MM");
        _logger.LogInformation("Starting monthly calculation for {Period}", period);

        await _calculationEngine.ExecuteBatchAsync(
            period,
            new CalculationOptions
            {
                CalculationType = CalculationType.Monthly,
                IncludeInactive = false
            },
            context.CancellationToken);
    }
}
```

### 10.3 Parallel Processing

```csharp
public class ParallelCalculationProcessor : ICalculationProcessor
{
    public async Task<BatchResult> ProcessBatchAsync(
        IEnumerable<Employee> employees,
        string period,
        BatchConfiguration config,
        CancellationToken cancellationToken)
    {
        var results = new ConcurrentBag<CalculationResult>();
        var errors = new ConcurrentBag<CalculationError>();

        var semaphore = new SemaphoreSlim(config.MaxParallelism);
        var batches = employees.Chunk(config.BatchSize);

        var tasks = batches.Select(async batch =>
        {
            foreach (var employee in batch)
            {
                await semaphore.WaitAsync(cancellationToken);
                try
                {
                    var result = await ProcessEmployeeWithRetryAsync(
                        employee, period, config, cancellationToken);
                    results.Add(result);
                }
                catch (Exception ex)
                {
                    errors.Add(new CalculationError(employee.Id, ex.Message));
                }
                finally
                {
                    semaphore.Release();
                }
            }
        });

        await Task.WhenAll(tasks);

        return new BatchResult
        {
            TotalProcessed = results.Count + errors.Count,
            Succeeded = results.Count,
            Failed = errors.Count,
            Results = results.ToList(),
            Errors = errors.ToList()
        };
    }

    private async Task<CalculationResult> ProcessEmployeeWithRetryAsync(
        Employee employee,
        string period,
        BatchConfiguration config,
        CancellationToken cancellationToken)
    {
        var policy = Policy
            .Handle<TransientException>()
            .WaitAndRetryAsync(
                config.MaxRetries,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception,
                        "Retry {RetryCount} for employee {EmployeeId} after {Delay}s",
                        retryCount, employee.Id, timeSpan.TotalSeconds);
                });

        return await policy.ExecuteAsync(async () =>
            await CalculateForEmployeeAsync(employee, period, cancellationToken));
    }
}
```

---

## 11. Error Handling

### 11.1 Error Categories

| Category | Example | Handling |
|----------|---------|----------|
| **Validation** | Missing required data | Log warning, skip employee |
| **Configuration** | No plan for role | Log error, skip employee |
| **Data** | Invalid metrics | Log error, skip employee |
| **System** | Database timeout | Retry with backoff |
| **Fatal** | Out of memory | Abort batch, alert |

### 11.2 Error Handling Strategy

```csharp
public class CalculationErrorHandler : ICalculationErrorHandler
{
    public async Task<ErrorAction> HandleErrorAsync(
        Exception exception,
        CalculationContext context,
        CancellationToken cancellationToken)
    {
        return exception switch
        {
            ValidationException validationEx =>
                await HandleValidationErrorAsync(validationEx, context, cancellationToken),

            NotFoundException notFoundEx =>
                await HandleNotFoundErrorAsync(notFoundEx, context, cancellationToken),

            TransientException transientEx =>
                await HandleTransientErrorAsync(transientEx, context, cancellationToken),

            DomainException domainEx =>
                await HandleDomainErrorAsync(domainEx, context, cancellationToken),

            _ => await HandleUnexpectedErrorAsync(exception, context, cancellationToken)
        };
    }

    private async Task<ErrorAction> HandleValidationErrorAsync(
        ValidationException ex,
        CalculationContext context,
        CancellationToken cancellationToken)
    {
        _logger.LogWarning(
            "Validation error for employee {EmployeeId}: {Errors}",
            context.EmployeeId,
            string.Join(", ", ex.Errors.SelectMany(e => e.Value)));

        // Record error but continue batch
        await _errorRepository.RecordErrorAsync(new CalculationError
        {
            BatchId = context.BatchId,
            EmployeeId = context.EmployeeId,
            ErrorType = ErrorType.Validation,
            Message = ex.Message,
            Details = JsonSerializer.Serialize(ex.Errors)
        }, cancellationToken);

        return ErrorAction.SkipAndContinue;
    }

    private async Task<ErrorAction> HandleTransientErrorAsync(
        TransientException ex,
        CalculationContext context,
        CancellationToken cancellationToken)
    {
        if (context.RetryCount < _config.MaxRetries)
        {
            _logger.LogWarning(
                "Transient error for employee {EmployeeId}, retry {RetryCount}",
                context.EmployeeId, context.RetryCount + 1);

            return ErrorAction.Retry;
        }

        return await HandlePermanentErrorAsync(ex, context, cancellationToken);
    }
}

public enum ErrorAction
{
    SkipAndContinue,
    Retry,
    AbortBatch,
    AlertAndContinue
}
```

### 11.3 Error Recording

```csharp
public class CalculationError : Entity
{
    public Guid BatchId { get; set; }
    public Guid? EmployeeId { get; set; }
    public ErrorType ErrorType { get; set; }
    public string Message { get; set; }
    public string? Details { get; set; }
    public string? StackTrace { get; set; }
    public DateTime OccurredAt { get; set; }
    public bool Resolved { get; set; }
    public string? ResolutionNotes { get; set; }
}
```

---

## 12. Testing Specifications

### 12.1 Test Categories

| Category | Focus | Coverage Target |
|----------|-------|-----------------|
| **Unit Tests** | Individual calculators | 90% |
| **Integration Tests** | Full calculation pipeline | 80% |
| **Scenario Tests** | Business scenarios | All documented scenarios |
| **Performance Tests** | Batch throughput | 10,000 employees/hour |
| **Regression Tests** | Historical calculations | Previous period data |

### 12.2 Test Scenarios

```csharp
public class CalculationEngineTests
{
    [Theory]
    [InlineData(0, 0)]              // Below minimum
    [InlineData(25000, 500)]        // Bronze slab: 25000 * 2%
    [InlineData(50000, 1000)]       // Bronze max: 50000 * 2%
    [InlineData(75000, 2250)]       // Silver: 75000 * 3%
    [InlineData(100000, 3000)]      // Silver max: 100000 * 3%
    [InlineData(150000, 6000)]      // Gold: 150000 * 4%
    [InlineData(250000, 12500)]     // Platinum: 250000 * 5%
    [InlineData(350000, 15000)]     // Platinum capped at $15,000
    public async Task Calculate_WithVariousSalesAmounts_ReturnsExpectedIncentive(
        decimal salesAmount,
        decimal expectedIncentive)
    {
        // Arrange
        var employee = CreateTestEmployee();
        var plan = CreateStandardPlan();
        var metrics = new SalesMetrics { SalesAmount = salesAmount };

        // Act
        var result = await _sut.CalculateAsync(employee, metrics, plan, "2025-01");

        // Assert
        result.GrossAmount.Should().Be(expectedIncentive);
    }

    [Fact]
    public async Task Calculate_WithMidMonthHire_AppliesCorrectProration()
    {
        // Arrange
        var employee = CreateTestEmployee(hireDate: new DateTime(2025, 1, 16));
        var metrics = new SalesMetrics { SalesAmount = 50000 };

        // Act
        var result = await _sut.CalculateAsync(employee, metrics, "2025-01");

        // Assert
        // Proration: 16 days / 31 days = 0.5161
        // Base: 50000 * 2% = 1000
        // Prorated: 1000 * 0.5161 = 516.13
        result.GrossAmount.Should().BeApproximately(516.13m, 0.01m);
        result.ProrationFactor.Should().BeApproximately(0.5161m, 0.001m);
    }

    [Fact]
    public async Task Calculate_WithSplitShare_AllocatesCorrectly()
    {
        // Arrange
        var totalIncentive = 1000m;
        var splits = new[]
        {
            new SplitShare { EmployeeId = Guid.NewGuid(), SharePercentage = 60 },
            new SplitShare { EmployeeId = Guid.NewGuid(), SharePercentage = 40 }
        };

        // Act
        var allocations = _splitCalculator.AllocateSplitShares(totalIncentive, splits);

        // Assert
        allocations.Should().HaveCount(2);
        allocations.First().Amount.Should().Be(600m);
        allocations.Last().Amount.Should().Be(400m);
        allocations.Sum(a => a.Amount).Should().Be(totalIncentive);
    }

    [Fact]
    public async Task Calculate_WithAmendment_CreatesNewImmutableRecord()
    {
        // Arrange
        var original = await CreateAndSaveCalculation(1000m);

        // Act
        var amendment = await _amendmentService.CreateAmendmentAsync(
            original.Id,
            new AmendmentRequest
            {
                NewAmount = 1200m,
                Reason = "Metrics correction"
            },
            CancellationToken.None);

        // Assert
        amendment.IsAmendment.Should().BeTrue();
        amendment.OriginalCalculationId.Should().Be(original.Id);
        amendment.AdjustmentAmount.Should().Be(200m);

        // Original should be unchanged
        var reloadedOriginal = await _repository.GetByIdAsync(original.Id);
        reloadedOriginal.GrossAmount.Should().Be(1000m);
        reloadedOriginal.SupersededByCalculationId.Should().Be(amendment.Id);
    }
}
```

### 12.3 Performance Benchmarks

```csharp
[MemoryDiagnoser]
public class CalculationBenchmarks
{
    [Benchmark]
    public async Task CalculateSingleEmployee()
    {
        await _engine.CalculateForEmployeeAsync(_testEmployee, "2025-01");
    }

    [Benchmark]
    public async Task CalculateBatch100Employees()
    {
        await _engine.ExecuteBatchAsync("2025-01", _batch100);
    }

    [Benchmark]
    public async Task CalculateBatch1000Employees()
    {
        await _engine.ExecuteBatchAsync("2025-01", _batch1000);
    }
}

// Target benchmarks:
// | Method                      | Mean      | Allocated |
// |---------------------------- |----------:|----------:|
// | CalculateSingleEmployee     |   5.2 ms  |    45 KB  |
// | CalculateBatch100Employees  |  150 ms   |   2.5 MB  |
// | CalculateBatch1000Employees | 1,200 ms  |    25 MB  |
```

---

## Appendix A: Calculation Record Schema

```json
{
  "calculationId": "7fa85f64-5717-4562-b3fc-2c963f66afa9",
  "batchId": "b1a2c3d4-5678-90ab-cdef-1234567890ab",
  "employeeId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "period": "2025-01",
  "planVersionId": "p1a2b3c4-5678-90ab-cdef-1234567890ab",
  "assignmentId": "a1b2c3d4-5678-90ab-cdef-1234567890ab",
  "calculatedAt": "2025-02-01T02:15:30Z",
  "status": "Pending",
  "grossAmount": 2550.00,
  "netAmount": 2550.00,
  "isAmendment": false,
  "originalCalculationId": null,
  "snapshot": {
    "employeeNumber": "EMP001",
    "employeeName": "John Doe",
    "roleName": "Sales Representative",
    "storeName": "Downtown Store",
    "planName": "Sales Rep Plan 2025",
    "planVersion": 2
  },
  "details": [
    {
      "detailId": "d1e2f3g4-5678-90ab-cdef-1234567890ab",
      "metricType": "SalesAmount",
      "metricValue": 85000.00,
      "slabApplied": "Silver",
      "rateType": "Percentage",
      "rateApplied": 3.0,
      "baseAmount": 2550.00,
      "prorationFactor": 1.0,
      "proratedAmount": 2550.00
    }
  ],
  "adjustments": []
}
```

---

> *"The doctor said I wouldn't have so many nosebleeds if I kept my finger outta there."* - Ralph Wiggum
>
> Our calculation engine catches every edge case - no unexpected results!

---

**Document Owner:** Lead Developer (Claude Code)
**Review Cycle:** Quarterly
**Last Review:** January 2025

---

*This document is part of the DSIF Quality Gate Framework - QG-3 Deliverable*
