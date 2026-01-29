using Dorise.Incentive.Domain.Common;

namespace Dorise.Incentive.Domain.ValueObjects;

/// <summary>
/// Represents a unique employee identifier code.
/// "My cat's breath smells like cat food." - But employee codes smell like success!
/// </summary>
public sealed class EmployeeCode : ValueObject
{
    public string Value { get; }

    private EmployeeCode(string value)
    {
        Value = value;
    }

    public static EmployeeCode Create(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Employee code cannot be empty", nameof(code));

        var normalized = code.Trim().ToUpperInvariant();

        if (normalized.Length < 3 || normalized.Length > 20)
            throw new ArgumentException("Employee code must be between 3 and 20 characters", nameof(code));

        if (!IsValidFormat(normalized))
            throw new ArgumentException("Employee code contains invalid characters. Only alphanumeric and hyphens allowed.", nameof(code));

        return new EmployeeCode(normalized);
    }

    private static bool IsValidFormat(string code)
    {
        return code.All(c => char.IsLetterOrDigit(c) || c == '-');
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;

    public static implicit operator string(EmployeeCode code) => code.Value;
}
