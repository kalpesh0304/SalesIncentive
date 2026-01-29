using Dorise.Incentive.Domain.Common;

namespace Dorise.Incentive.Domain.ValueObjects;

/// <summary>
/// Represents a slab range for tiered incentive calculations.
/// "Principal Skinner, I got carsick in your office." - Don't get range-sick!
/// </summary>
public sealed class SlabRange : ValueObject
{
    public Percentage FromPercentage { get; }
    public Percentage ToPercentage { get; }
    public Percentage PayoutRate { get; }
    public int Order { get; }

    private SlabRange(Percentage fromPercentage, Percentage toPercentage, Percentage payoutRate, int order)
    {
        FromPercentage = fromPercentage;
        ToPercentage = toPercentage;
        PayoutRate = payoutRate;
        Order = order;
    }

    public static SlabRange Create(
        decimal fromPercentage,
        decimal toPercentage,
        decimal payoutRate,
        int order)
    {
        if (toPercentage < fromPercentage)
            throw new ArgumentException("To percentage cannot be less than from percentage");

        if (payoutRate < 0)
            throw new ArgumentException("Payout rate cannot be negative", nameof(payoutRate));

        if (order < 0)
            throw new ArgumentException("Order cannot be negative", nameof(order));

        return new SlabRange(
            Percentage.Create(fromPercentage),
            Percentage.Create(toPercentage),
            Percentage.Create(payoutRate),
            order);
    }

    public bool IsInRange(Percentage achievement)
    {
        return achievement >= FromPercentage && achievement <= ToPercentage;
    }

    public bool IsInRange(decimal achievementValue)
    {
        return IsInRange(Percentage.Create(achievementValue));
    }

    /// <summary>
    /// Calculates the payout for a given achievement within this slab.
    /// </summary>
    public Money CalculatePayout(Money baseAmount, Percentage achievement)
    {
        if (!IsInRange(achievement))
            return Money.Zero(baseAmount.Currency);

        return baseAmount.Multiply(PayoutRate);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return FromPercentage;
        yield return ToPercentage;
        yield return PayoutRate;
        yield return Order;
    }

    public override string ToString() =>
        $"Slab {Order}: {FromPercentage} - {ToPercentage} @ {PayoutRate}";
}
