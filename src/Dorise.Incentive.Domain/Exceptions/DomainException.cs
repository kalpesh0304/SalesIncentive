namespace Dorise.Incentive.Domain.Exceptions;

/// <summary>
/// Base exception for domain-specific errors.
/// "My worm went in my mouth and then I ate it. Can I have another one?" - Don't eat exceptions!
/// </summary>
public class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string message, string code = "DOMAIN_ERROR")
        : base(message)
    {
        Code = code;
    }

    public DomainException(string message, Exception innerException, string code = "DOMAIN_ERROR")
        : base(message, innerException)
    {
        Code = code;
    }
}

/// <summary>
/// Exception thrown when an entity is not found.
/// </summary>
public class EntityNotFoundException : DomainException
{
    public string EntityType { get; }
    public object EntityId { get; }

    public EntityNotFoundException(string entityType, object entityId)
        : base($"{entityType} with ID '{entityId}' was not found.", "ENTITY_NOT_FOUND")
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}

/// <summary>
/// Exception thrown when a duplicate entity is detected.
/// </summary>
public class DuplicateEntityException : DomainException
{
    public string EntityType { get; }
    public string DuplicateField { get; }
    public object DuplicateValue { get; }

    public DuplicateEntityException(string entityType, string duplicateField, object duplicateValue)
        : base($"{entityType} with {duplicateField} '{duplicateValue}' already exists.", "DUPLICATE_ENTITY")
    {
        EntityType = entityType;
        DuplicateField = duplicateField;
        DuplicateValue = duplicateValue;
    }
}

/// <summary>
/// Exception thrown when a business rule is violated.
/// </summary>
public class BusinessRuleException : DomainException
{
    public string RuleName { get; }

    public BusinessRuleException(string ruleName, string message)
        : base(message, $"RULE_VIOLATION_{ruleName.ToUpperInvariant()}")
    {
        RuleName = ruleName;
    }
}

/// <summary>
/// Exception thrown when an invalid state transition is attempted.
/// </summary>
public class InvalidStateTransitionException : DomainException
{
    public string EntityType { get; }
    public string CurrentState { get; }
    public string AttemptedState { get; }

    public InvalidStateTransitionException(string entityType, string currentState, string attemptedState)
        : base($"Cannot transition {entityType} from '{currentState}' to '{attemptedState}'.", "INVALID_STATE_TRANSITION")
    {
        EntityType = entityType;
        CurrentState = currentState;
        AttemptedState = attemptedState;
    }
}

/// <summary>
/// Exception thrown when calculation fails.
/// </summary>
public class CalculationException : DomainException
{
    public Guid? EmployeeId { get; }
    public Guid? PlanId { get; }

    public CalculationException(string message, Guid? employeeId = null, Guid? planId = null)
        : base(message, "CALCULATION_ERROR")
    {
        EmployeeId = employeeId;
        PlanId = planId;
    }
}

/// <summary>
/// Exception thrown when validation fails.
/// </summary>
public class ValidationException : DomainException
{
    public IDictionary<string, string[]> Errors { get; }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("One or more validation errors occurred.", "VALIDATION_ERROR")
    {
        Errors = errors;
    }

    public ValidationException(string field, string error)
        : base($"Validation error for {field}: {error}", "VALIDATION_ERROR")
    {
        Errors = new Dictionary<string, string[]> { { field, new[] { error } } };
    }
}
