using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Exceptions;
using Dorise.Incentive.Domain.ValueObjects;

namespace Dorise.Incentive.Domain.Services;

/// <summary>
/// Domain service for incentive calculations.
/// "Fun toys are not allowed." - But fun calculations ARE allowed!
/// </summary>
public class IncentiveCalculationService : IIncentiveCalculationService
{
    public CalculationResult Calculate(
        Employee employee,
        IncentivePlan plan,
        decimal actualValue,
        DateRange period)
    {
        // Check eligibility first
        var eligibility = CheckEligibility(employee, plan, period);
        if (!eligibility.IsEligible)
        {
            return new CalculationResult(
                false,
                Money.Zero(employee.BaseSalary.Currency),
                Money.Zero(employee.BaseSalary.Currency),
                Percentage.Zero(),
                null,
                eligibility.Reason);
        }

        // Calculate achievement percentage
        var achievement = Percentage.Calculate(actualValue, plan.Target.TargetValue);

        // Check minimum threshold
        if (!plan.Target.MeetsMinimumThreshold(actualValue))
        {
            return new CalculationResult(
                true,
                Money.Zero(employee.BaseSalary.Currency),
                Money.Zero(employee.BaseSalary.Currency),
                achievement,
                null,
                "Below minimum threshold");
        }

        // Calculate based on plan type
        var grossIncentive = CalculateByPlanType(employee, plan, achievement);
        var netIncentive = grossIncentive;
        Slab? appliedSlab = null;

        // For slab-based plans, get the applied slab
        if (plan.PlanType == PlanType.SlabBased)
        {
            appliedSlab = DetermineApplicableSlab(plan, achievement);
        }

        // Apply prorata if needed
        if (eligibility.ProrataFactor.Value < 100)
        {
            netIncentive = grossIncentive.Multiply(eligibility.ProrataFactor);
        }

        // Apply caps
        if (plan.MaximumPayout != null && netIncentive > plan.MaximumPayout)
        {
            netIncentive = plan.MaximumPayout;
        }

        if (plan.MinimumPayout != null && netIncentive < plan.MinimumPayout && netIncentive.IsPositive())
        {
            netIncentive = plan.MinimumPayout;
        }

        return new CalculationResult(
            true,
            grossIncentive,
            netIncentive,
            achievement,
            appliedSlab,
            null);
    }

    public CalculationResult ApplyProrata(
        Calculation calculation,
        int eligibleDays,
        int totalDays)
    {
        if (eligibleDays <= 0 || totalDays <= 0)
            throw new CalculationException("Invalid prorata days", calculation.EmployeeId, calculation.IncentivePlanId);

        if (eligibleDays > totalDays)
            throw new CalculationException("Eligible days cannot exceed total days", calculation.EmployeeId, calculation.IncentivePlanId);

        var prorataFactor = Percentage.Create((decimal)eligibleDays / totalDays * 100);
        var proratedAmount = calculation.GrossIncentive.Multiply(prorataFactor);

        return new CalculationResult(
            true,
            calculation.GrossIncentive,
            proratedAmount,
            calculation.AchievementPercentage,
            calculation.AppliedSlab,
            $"Prorated for {eligibleDays}/{totalDays} days");
    }

    public Slab? DetermineApplicableSlab(IncentivePlan plan, Percentage achievement)
    {
        if (plan.PlanType != PlanType.SlabBased)
            return null;

        return plan.GetApplicableSlab(achievement);
    }

    public EligibilityResult CheckEligibility(
        Employee employee,
        IncentivePlan plan,
        DateRange period)
    {
        // Check employee status
        if (!employee.IsActive && employee.DateOfLeaving.HasValue &&
            employee.DateOfLeaving.Value < period.StartDate)
        {
            return new EligibilityResult(false, Percentage.Zero(), "Employee left before period started");
        }

        // Check if employee joined after period ended
        if (employee.DateOfJoining > period.EndDate)
        {
            return new EligibilityResult(false, Percentage.Zero(), "Employee joined after period ended");
        }

        // Check plan effectiveness
        if (!plan.IsEffective(period.StartDate) && !plan.IsEffective(period.EndDate))
        {
            return new EligibilityResult(false, Percentage.Zero(), "Plan not effective for period");
        }

        // Calculate prorata factor
        var prorataFactor = CalculateProrataFactor(employee, period);

        return new EligibilityResult(true, prorataFactor, null);
    }

    private Money CalculateByPlanType(Employee employee, IncentivePlan plan, Percentage achievement)
    {
        return plan.PlanType switch
        {
            PlanType.Fixed => CalculateFixed(employee, plan, achievement),
            PlanType.PercentageOfSalary => CalculatePercentageOfSalary(employee, plan, achievement),
            PlanType.SlabBased => CalculateSlabBased(employee, plan, achievement),
            PlanType.Commission => CalculateCommission(employee, plan, achievement),
            PlanType.MBO => CalculateMBO(employee, plan, achievement),
            _ => throw new CalculationException($"Unsupported plan type: {plan.PlanType}")
        };
    }

    private Money CalculateFixed(Employee employee, IncentivePlan plan, Percentage achievement)
    {
        // Fixed amount based on achievement threshold
        if (achievement >= Percentage.Full())
        {
            // Return target value as incentive (assumed fixed amount configured elsewhere)
            return employee.BaseSalary.Multiply(Percentage.Create(10)); // Example: 10% of salary
        }

        return Money.Zero(employee.BaseSalary.Currency);
    }

    private Money CalculatePercentageOfSalary(Employee employee, IncentivePlan plan, Percentage achievement)
    {
        // Simple percentage of base salary based on achievement
        var incentivePercentage = achievement.Value > 100
            ? Percentage.Create(achievement.Value / 100 * 10) // Scale factor for over-achievement
            : Percentage.Create(achievement.Value / 100 * 10);

        return employee.BaseSalary.Multiply(incentivePercentage);
    }

    private Money CalculateSlabBased(Employee employee, IncentivePlan plan, Percentage achievement)
    {
        var slab = plan.GetApplicableSlab(achievement);
        if (slab == null)
        {
            return Money.Zero(employee.BaseSalary.Currency);
        }

        return slab.CalculatePayout(employee.BaseSalary);
    }

    private Money CalculateCommission(Employee employee, IncentivePlan plan, Percentage achievement)
    {
        // Commission based on actual value achieved
        var commissionRate = Percentage.Create(5); // Example: 5% commission
        var actualAmount = Money.Create(plan.Target.TargetValue * achievement.ToFraction(), employee.BaseSalary.Currency);
        return actualAmount.Multiply(commissionRate);
    }

    private Money CalculateMBO(Employee employee, IncentivePlan plan, Percentage achievement)
    {
        // MBO: percentage of salary based on objective achievement
        var mboPercentage = achievement.Cap(150); // Cap at 150%
        var targetBonus = employee.BaseSalary.Multiply(Percentage.Create(15)); // 15% target bonus
        return targetBonus.Multiply(Percentage.Create(mboPercentage.Value / 100 * 100));
    }

    private Percentage CalculateProrataFactor(Employee employee, DateRange period)
    {
        var periodStart = period.StartDate;
        var periodEnd = period.EndDate;

        // Adjust for join date
        if (employee.DateOfJoining > periodStart)
        {
            periodStart = employee.DateOfJoining;
        }

        // Adjust for leave date
        if (employee.DateOfLeaving.HasValue && employee.DateOfLeaving.Value < periodEnd)
        {
            periodEnd = employee.DateOfLeaving.Value;
        }

        var eligibleDays = (int)(periodEnd - periodStart).TotalDays + 1;
        var totalDays = period.TotalDays;

        if (eligibleDays <= 0)
            return Percentage.Zero();

        if (eligibleDays >= totalDays)
            return Percentage.Full();

        return Percentage.Create((decimal)eligibleDays / totalDays * 100);
    }
}
