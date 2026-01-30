namespace Dorise.Incentive.Domain.Enums;

/// <summary>
/// Frequency at which incentives are calculated and paid.
/// </summary>
public enum PaymentFrequency
{
    /// <summary>Calculated and paid monthly</summary>
    Monthly = 1,

    /// <summary>Calculated and paid quarterly</summary>
    Quarterly = 2,

    /// <summary>Calculated twice a year</summary>
    SemiAnnual = 3,

    /// <summary>Calculated annually</summary>
    Annual = 4,

    /// <summary>One-time payment</summary>
    OneTime = 5,

    /// <summary>Bi-weekly payment cycle</summary>
    BiWeekly = 6
}
