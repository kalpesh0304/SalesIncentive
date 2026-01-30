using Dorise.Incentive.Domain.Common;

namespace Dorise.Incentive.Domain.ValueObjects;

/// <summary>
/// Represents a date range with start and end dates.
/// "I bent my Wookiee!" - Don't bend your date ranges though.
/// </summary>
public sealed class DateRange : ValueObject
{
    public DateTime StartDate { get; }
    public DateTime EndDate { get; }

    private DateRange(DateTime startDate, DateTime endDate)
    {
        StartDate = startDate.Date;
        EndDate = endDate.Date;
    }

    public static DateRange Create(DateTime startDate, DateTime endDate)
    {
        if (endDate < startDate)
            throw new ArgumentException("End date cannot be before start date");

        return new DateRange(startDate, endDate);
    }

    public static DateRange ForMonth(int year, int month)
    {
        var start = new DateTime(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        return new DateRange(start, end);
    }

    public static DateRange ForQuarter(int year, int quarter)
    {
        if (quarter < 1 || quarter > 4)
            throw new ArgumentException("Quarter must be between 1 and 4", nameof(quarter));

        var startMonth = (quarter - 1) * 3 + 1;
        var start = new DateTime(year, startMonth, 1);
        var end = start.AddMonths(3).AddDays(-1);
        return new DateRange(start, end);
    }

    public static DateRange ForYear(int year)
    {
        var start = new DateTime(year, 1, 1);
        var end = new DateTime(year, 12, 31);
        return new DateRange(start, end);
    }

    public static DateRange ForFinancialYear(int startYear)
    {
        // Indian financial year: April 1 to March 31
        var start = new DateTime(startYear, 4, 1);
        var end = new DateTime(startYear + 1, 3, 31);
        return new DateRange(start, end);
    }

    public int TotalDays => (int)(EndDate - StartDate).TotalDays + 1;

    public bool Contains(DateTime date)
    {
        var dateOnly = date.Date;
        return dateOnly >= StartDate && dateOnly <= EndDate;
    }

    public bool Overlaps(DateRange other)
    {
        return StartDate <= other.EndDate && EndDate >= other.StartDate;
    }

    public bool IsWithin(DateRange other)
    {
        return StartDate >= other.StartDate && EndDate <= other.EndDate;
    }

    public Percentage CalculateOverlapPercentage(DateRange other)
    {
        if (!Overlaps(other))
            return Percentage.Zero();

        var overlapStart = StartDate > other.StartDate ? StartDate : other.StartDate;
        var overlapEnd = EndDate < other.EndDate ? EndDate : other.EndDate;
        var overlapDays = (int)(overlapEnd - overlapStart).TotalDays + 1;

        return Percentage.Create((decimal)overlapDays / TotalDays * 100);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return StartDate;
        yield return EndDate;
    }

    public override string ToString() => $"{StartDate:yyyy-MM-dd} to {EndDate:yyyy-MM-dd}";
}
