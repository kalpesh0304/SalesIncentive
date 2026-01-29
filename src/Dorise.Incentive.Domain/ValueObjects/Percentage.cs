using Dorise.Incentive.Domain.Common;

namespace Dorise.Incentive.Domain.ValueObjects;

/// <summary>
/// Represents a percentage value (0-100 or beyond for achievements > 100%).
/// "The doctor said I wouldn't have so many nosebleeds if I kept my finger outta there."
/// </summary>
public sealed class Percentage : ValueObject
{
    public decimal Value { get; }

    private Percentage(decimal value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a percentage from a value (e.g., 50 for 50%).
    /// </summary>
    public static Percentage Create(decimal value)
    {
        if (value < 0)
            throw new ArgumentException("Percentage cannot be negative", nameof(value));

        return new Percentage(Math.Round(value, 4));
    }

    /// <summary>
    /// Creates a percentage from a decimal fraction (e.g., 0.5 for 50%).
    /// </summary>
    public static Percentage FromFraction(decimal fraction)
    {
        if (fraction < 0)
            throw new ArgumentException("Fraction cannot be negative", nameof(fraction));

        return new Percentage(Math.Round(fraction * 100, 4));
    }

    /// <summary>
    /// Calculates percentage of one value relative to another.
    /// </summary>
    public static Percentage Calculate(decimal actual, decimal target)
    {
        if (target == 0)
            return actual == 0 ? Zero() : Create(100);

        return Create(actual / target * 100);
    }

    public static Percentage Zero() => new(0);
    public static Percentage Full() => new(100);

    public decimal ToFraction() => Value / 100m;

    public bool IsZero() => Value == 0;
    public bool IsFull() => Value == 100;
    public bool ExceedsTarget() => Value > 100;

    public Percentage Cap(decimal maxValue)
    {
        return Value > maxValue ? Create(maxValue) : this;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => $"{Value:N2}%";

    public static bool operator >(Percentage left, Percentage right) => left.Value > right.Value;
    public static bool operator <(Percentage left, Percentage right) => left.Value < right.Value;
    public static bool operator >=(Percentage left, Percentage right) => left.Value >= right.Value;
    public static bool operator <=(Percentage left, Percentage right) => left.Value <= right.Value;
}
