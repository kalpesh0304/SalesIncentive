namespace Dorise.Incentive.Domain.Enums;

/// <summary>
/// Type of incentive plan calculation method.
/// </summary>
public enum PlanType
{
    /// <summary>Fixed amount based on achievement</summary>
    Fixed = 1,

    /// <summary>Percentage of base salary</summary>
    PercentageOfSalary = 2,

    /// <summary>Tiered slabs with different rates</summary>
    SlabBased = 3,

    /// <summary>Commission on revenue/sales</summary>
    Commission = 4,

    /// <summary>Bonus pool shared among team</summary>
    PoolBased = 5,

    /// <summary>Mix of multiple calculation types</summary>
    Hybrid = 6,

    /// <summary>MBO (Management by Objectives) based</summary>
    MBO = 7,

    /// <summary>Spot bonus for one-time achievements</summary>
    SpotBonus = 8
}
