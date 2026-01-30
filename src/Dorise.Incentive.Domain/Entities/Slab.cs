using Dorise.Incentive.Domain.Common;
using Dorise.Incentive.Domain.ValueObjects;

namespace Dorise.Incentive.Domain.Entities;

/// <summary>
/// Represents a payout slab/tier within an incentive plan.
/// "When I grow up, I want to be a principal or a caterpillar." - Slabs help you grow!
/// </summary>
public class Slab : AuditableEntity
{
    public Guid IncentivePlanId { get; private set; }
    public decimal FromPercentage { get; private set; }
    public decimal ToPercentage { get; private set; }
    public decimal PayoutRate { get; private set; }
    public int Order { get; private set; }
    public string? Description { get; private set; }

    private Slab() { } // EF Core constructor

    internal static Slab Create(
        Guid incentivePlanId,
        decimal fromPercentage,
        decimal toPercentage,
        decimal payoutRate,
        int order,
        string? description = null)
    {
        if (toPercentage < fromPercentage)
            throw new ArgumentException("To percentage cannot be less than from percentage");

        if (payoutRate < 0)
            throw new ArgumentException("Payout rate cannot be negative", nameof(payoutRate));

        return new Slab
        {
            Id = Guid.NewGuid(),
            IncentivePlanId = incentivePlanId,
            FromPercentage = fromPercentage,
            ToPercentage = toPercentage,
            PayoutRate = payoutRate,
            Order = order,
            Description = description?.Trim()
        };
    }

    public bool IsInRange(Percentage achievement)
    {
        return achievement.Value >= FromPercentage && achievement.Value <= ToPercentage;
    }

    public bool IsInRange(decimal achievementValue)
    {
        return achievementValue >= FromPercentage && achievementValue <= ToPercentage;
    }

    public Money CalculatePayout(Money baseAmount)
    {
        return baseAmount.Multiply(Percentage.Create(PayoutRate));
    }

    public void SetOrder(int order)
    {
        Order = order;
    }

    public void UpdateDetails(decimal fromPercentage, decimal toPercentage, decimal payoutRate, string? description)
    {
        if (toPercentage < fromPercentage)
            throw new ArgumentException("To percentage cannot be less than from percentage");

        if (payoutRate < 0)
            throw new ArgumentException("Payout rate cannot be negative", nameof(payoutRate));

        FromPercentage = fromPercentage;
        ToPercentage = toPercentage;
        PayoutRate = payoutRate;
        Description = description?.Trim();
    }
}
