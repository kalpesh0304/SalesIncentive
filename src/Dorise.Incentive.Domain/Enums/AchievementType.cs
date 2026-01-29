namespace Dorise.Incentive.Domain.Enums;

/// <summary>
/// Type of metric used to measure achievement.
/// </summary>
public enum AchievementType
{
    /// <summary>Revenue/sales amount</summary>
    Revenue = 1,

    /// <summary>Number of units sold</summary>
    UnitsSold = 2,

    /// <summary>Number of new customers acquired</summary>
    NewCustomers = 3,

    /// <summary>Customer retention rate</summary>
    RetentionRate = 4,

    /// <summary>Profit margin percentage</summary>
    ProfitMargin = 5,

    /// <summary>Collection amount</summary>
    Collections = 6,

    /// <summary>Number of deals closed</summary>
    DealsCount = 7,

    /// <summary>Custom KPI metric</summary>
    CustomKPI = 8,

    /// <summary>Composite score from multiple metrics</summary>
    CompositeScore = 9
}
