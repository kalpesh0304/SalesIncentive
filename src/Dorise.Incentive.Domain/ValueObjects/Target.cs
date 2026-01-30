using Dorise.Incentive.Domain.Common;
using Dorise.Incentive.Domain.Enums;

namespace Dorise.Incentive.Domain.ValueObjects;

/// <summary>
/// Represents a performance target with its achievement metrics.
/// "I'm learnding!" - And we're tracking achievements!
/// </summary>
public sealed class Target : ValueObject
{
    public decimal TargetValue { get; }
    public decimal MinimumThreshold { get; }
    public AchievementType AchievementType { get; }
    public string? MetricUnit { get; }

    private Target(decimal targetValue, decimal minimumThreshold, AchievementType achievementType, string? metricUnit)
    {
        TargetValue = targetValue;
        MinimumThreshold = minimumThreshold;
        AchievementType = achievementType;
        MetricUnit = metricUnit;
    }

    public static Target Create(
        decimal targetValue,
        decimal minimumThreshold,
        AchievementType achievementType,
        string? metricUnit = null)
    {
        if (targetValue <= 0)
            throw new ArgumentException("Target value must be positive", nameof(targetValue));

        if (minimumThreshold < 0)
            throw new ArgumentException("Minimum threshold cannot be negative", nameof(minimumThreshold));

        if (minimumThreshold > targetValue)
            throw new ArgumentException("Minimum threshold cannot exceed target value", nameof(minimumThreshold));

        return new Target(targetValue, minimumThreshold, achievementType, metricUnit);
    }

    public Percentage CalculateAchievement(decimal actualValue)
    {
        if (actualValue < 0)
            throw new ArgumentException("Actual value cannot be negative", nameof(actualValue));

        return Percentage.Calculate(actualValue, TargetValue);
    }

    public bool MeetsMinimumThreshold(decimal actualValue)
    {
        return actualValue >= MinimumThreshold;
    }

    public decimal GetThresholdPercentage()
    {
        return MinimumThreshold / TargetValue * 100;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return TargetValue;
        yield return MinimumThreshold;
        yield return AchievementType;
        yield return MetricUnit ?? string.Empty;
    }

    public override string ToString() =>
        $"Target: {TargetValue:N2} {MetricUnit ?? ""} (Min: {MinimumThreshold:N2})";
}
