using FluentValidation;

namespace Dorise.Incentive.Application.Employees.Commands.CreateEmployee;

/// <summary>
/// Validator for CreateEmployeeCommand.
/// "My cat's breath smells like cat food!" - But our validation smells like roses!
/// </summary>
public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
{
    public CreateEmployeeCommandValidator()
    {
        RuleFor(x => x.EmployeeCode)
            .NotEmpty().WithMessage("Employee code is required.")
            .MinimumLength(3).WithMessage("Employee code must be at least 3 characters.")
            .MaximumLength(20).WithMessage("Employee code cannot exceed 20 characters.")
            .Matches("^[A-Za-z0-9-]+$").WithMessage("Employee code can only contain letters, numbers, and hyphens.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(100).WithMessage("First name cannot exceed 100 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(100).WithMessage("Last name cannot exceed 100 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.");

        RuleFor(x => x.DepartmentId)
            .NotEmpty().WithMessage("Department is required.");

        RuleFor(x => x.DateOfJoining)
            .NotEmpty().WithMessage("Date of joining is required.")
            .LessThanOrEqualTo(DateTime.Today).WithMessage("Date of joining cannot be in the future.");

        RuleFor(x => x.BaseSalary)
            .GreaterThan(0).WithMessage("Base salary must be greater than zero.");

        RuleFor(x => x.Currency)
            .NotEmpty().WithMessage("Currency is required.")
            .Length(3).WithMessage("Currency must be a 3-letter ISO code.");

        RuleFor(x => x.Designation)
            .MaximumLength(100).WithMessage("Designation cannot exceed 100 characters.")
            .When(x => !string.IsNullOrEmpty(x.Designation));
    }
}
