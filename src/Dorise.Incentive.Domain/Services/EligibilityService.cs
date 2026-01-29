using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.ValueObjects;

namespace Dorise.Incentive.Domain.Services;

/// <summary>
/// Domain service for determining employee eligibility for incentive plans.
/// "Mrs. Krabappel and Principal Skinner were in the closet making babies
/// and I saw one of the babies and the baby looked at me!" - But eligibility rules are more straightforward!
/// </summary>
public class EligibilityService : IEligibilityService
{
    private const int DefaultMinTenureDays = 90;

    public EligibilityCheckResult CheckEligibility(Employee employee, IncentivePlan plan, DateTime asOfDate)
    {
        var criteriaMet = new List<EligibilityCriterion>();
        var criteriaNotMet = new List<EligibilityCriterion>();

        // Criterion 1: Employee Status
        var statusCriterion = CheckStatusCriterion(employee);
        if (statusCriterion.IsMet)
            criteriaMet.Add(statusCriterion);
        else
            criteriaNotMet.Add(statusCriterion);

        // Criterion 2: Plan is Active
        var planStatusCriterion = CheckPlanStatusCriterion(plan);
        if (planStatusCriterion.IsMet)
            criteriaMet.Add(planStatusCriterion);
        else
            criteriaNotMet.Add(planStatusCriterion);

        // Criterion 3: Plan is Effective
        var planEffectiveCriterion = CheckPlanEffectiveCriterion(plan, asOfDate);
        if (planEffectiveCriterion.IsMet)
            criteriaMet.Add(planEffectiveCriterion);
        else
            criteriaNotMet.Add(planEffectiveCriterion);

        // Criterion 4: Minimum Tenure
        var tenureCriterion = CheckTenureCriterion(employee, plan, asOfDate);
        if (tenureCriterion.IsMet)
            criteriaMet.Add(tenureCriterion);
        else
            criteriaNotMet.Add(tenureCriterion);

        // Criterion 5: Employee joined before period end
        var joinDateCriterion = CheckJoinDateCriterion(employee, plan, asOfDate);
        if (joinDateCriterion.IsMet)
            criteriaMet.Add(joinDateCriterion);
        else
            criteriaNotMet.Add(joinDateCriterion);

        // Determine overall eligibility
        if (criteriaNotMet.Count > 0)
        {
            var primaryReason = criteriaNotMet[0].Description;
            return EligibilityCheckResult.NotEligible(primaryReason, criteriaMet, criteriaNotMet);
        }

        // Calculate prorata factor
        var prorataFactor = CalculateProrataFactor(employee, plan.EffectivePeriod);

        return EligibilityCheckResult.Eligible(criteriaMet, prorataFactor);
    }

    public bool MeetsTenureRequirement(Employee employee, IncentivePlan plan, DateTime asOfDate)
    {
        var minTenureDays = plan.MinimumTenureDays ?? DefaultMinTenureDays;
        var tenureDays = employee.GetTenureInDays(asOfDate);
        return tenureDays >= minTenureDays;
    }

    public bool HasEligibleStatus(Employee employee)
    {
        return employee.Status == EmployeeStatus.Active ||
               employee.Status == EmployeeStatus.Probation;
    }

    public Percentage CalculateProrataFactor(Employee employee, DateRange period)
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

        // If adjusted period is invalid
        if (periodStart > periodEnd)
        {
            return Percentage.Zero();
        }

        var eligibleDays = (int)(periodEnd - periodStart).TotalDays + 1;
        var totalDays = period.TotalDays;

        if (eligibleDays >= totalDays)
        {
            return Percentage.Full();
        }

        return Percentage.Create((decimal)eligibleDays / totalDays * 100);
    }

    private EligibilityCriterion CheckStatusCriterion(Employee employee)
    {
        var isEligible = HasEligibleStatus(employee);
        return new EligibilityCriterion(
            "Employee Status",
            isEligible
                ? $"Employee status is {employee.Status}"
                : $"Employee status {employee.Status} is not eligible for incentives",
            isEligible);
    }

    private EligibilityCriterion CheckPlanStatusCriterion(IncentivePlan plan)
    {
        var isActive = plan.Status == PlanStatus.Active;
        return new EligibilityCriterion(
            "Plan Status",
            isActive
                ? "Plan is active"
                : $"Plan status is {plan.Status}, must be Active",
            isActive);
    }

    private EligibilityCriterion CheckPlanEffectiveCriterion(IncentivePlan plan, DateTime asOfDate)
    {
        var isEffective = plan.IsEffective(asOfDate);
        return new EligibilityCriterion(
            "Plan Effective Period",
            isEffective
                ? $"Plan is effective on {asOfDate:yyyy-MM-dd}"
                : $"Plan is not effective on {asOfDate:yyyy-MM-dd}. Effective period: {plan.EffectivePeriod}",
            isEffective);
    }

    private EligibilityCriterion CheckTenureCriterion(Employee employee, IncentivePlan plan, DateTime asOfDate)
    {
        var minTenureDays = plan.MinimumTenureDays ?? DefaultMinTenureDays;
        var tenureDays = employee.GetTenureInDays(asOfDate);
        var meetsTenure = tenureDays >= minTenureDays;

        return new EligibilityCriterion(
            "Minimum Tenure",
            meetsTenure
                ? $"Employee tenure ({tenureDays} days) meets minimum requirement ({minTenureDays} days)"
                : $"Employee tenure ({tenureDays} days) does not meet minimum requirement ({minTenureDays} days)",
            meetsTenure);
    }

    private EligibilityCriterion CheckJoinDateCriterion(Employee employee, IncentivePlan plan, DateTime asOfDate)
    {
        var joinedBeforePeriodEnd = employee.DateOfJoining <= plan.EffectivePeriod.EndDate;
        return new EligibilityCriterion(
            "Join Date",
            joinedBeforePeriodEnd
                ? $"Employee joined ({employee.DateOfJoining:yyyy-MM-dd}) before period end"
                : $"Employee joined ({employee.DateOfJoining:yyyy-MM-dd}) after period end ({plan.EffectivePeriod.EndDate:yyyy-MM-dd})",
            joinedBeforePeriodEnd);
    }
}
