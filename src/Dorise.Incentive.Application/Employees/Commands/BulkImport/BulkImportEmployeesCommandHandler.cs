using Dorise.Incentive.Application.Common.Interfaces;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Application.Employees.Commands.BulkImport;

/// <summary>
/// Handler for BulkImportEmployeesCommand.
/// "I'm Idaho!" - And we're importing employees from all over!
/// </summary>
public class BulkImportEmployeesCommandHandler : ICommandHandler<BulkImportEmployeesCommand, BulkImportResultDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<BulkImportEmployeesCommandHandler> _logger;

    public BulkImportEmployeesCommandHandler(
        IUnitOfWork unitOfWork,
        ILogger<BulkImportEmployeesCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<BulkImportResultDto>> Handle(
        BulkImportEmployeesCommand request,
        CancellationToken cancellationToken)
    {
        var successes = new List<ImportSuccessDto>();
        var errors = new List<ImportErrorDto>();
        var warnings = new List<ImportWarningDto>();
        var updatedCount = 0;

        // Pre-fetch all departments for validation
        var departments = await _unitOfWork.Departments.GetAllAsync(cancellationToken);
        var departmentLookup = departments.ToDictionary(d => d.Code.ToUpperInvariant(), d => d.Id);

        // Pre-fetch existing employees for duplicate check
        var employeeCodes = request.Employees.Select(e => e.EmployeeCode.ToUpperInvariant()).ToHashSet();
        var existingEmployees = await _unitOfWork.Employees.GetByCodesAsync(employeeCodes, cancellationToken);
        var existingLookup = existingEmployees.ToDictionary(e => e.EmployeeCode.Value.ToUpperInvariant());

        foreach (var importDto in request.Employees)
        {
            try
            {
                // Validate department
                var deptCode = importDto.DepartmentCode.ToUpperInvariant();
                if (!departmentLookup.TryGetValue(deptCode, out var departmentId))
                {
                    errors.Add(new ImportErrorDto
                    {
                        RowNumber = importDto.RowNumber,
                        EmployeeCode = importDto.EmployeeCode,
                        ErrorMessage = $"Department '{importDto.DepartmentCode}' not found",
                        Field = "DepartmentCode"
                    });
                    continue;
                }

                // Check for existing employee
                var empCode = importDto.EmployeeCode.ToUpperInvariant();
                if (existingLookup.TryGetValue(empCode, out var existingEmployee))
                {
                    if (request.UpdateExisting)
                    {
                        if (!request.ValidateOnly)
                        {
                            existingEmployee.UpdateProfile(
                                importDto.FirstName,
                                importDto.LastName,
                                importDto.Email,
                                importDto.Designation);

                            existingEmployee.UpdateSalary(importDto.BaseSalary, importDto.Currency);

                            if (existingEmployee.DepartmentId != departmentId)
                            {
                                existingEmployee.TransferToDepartment(departmentId);
                            }
                        }

                        successes.Add(new ImportSuccessDto
                        {
                            RowNumber = importDto.RowNumber,
                            EmployeeCode = importDto.EmployeeCode,
                            EmployeeId = existingEmployee.Id,
                            Action = "Updated"
                        });
                        updatedCount++;
                    }
                    else
                    {
                        warnings.Add(new ImportWarningDto
                        {
                            RowNumber = importDto.RowNumber,
                            EmployeeCode = importDto.EmployeeCode,
                            WarningMessage = "Employee already exists. Skipped. Set UpdateExisting=true to update."
                        });
                    }
                    continue;
                }

                // Validate required fields
                var validationErrors = ValidateImportDto(importDto);
                if (validationErrors.Count > 0)
                {
                    foreach (var error in validationErrors)
                    {
                        errors.Add(new ImportErrorDto
                        {
                            RowNumber = importDto.RowNumber,
                            EmployeeCode = importDto.EmployeeCode,
                            ErrorMessage = error.Message,
                            Field = error.Field
                        });
                    }
                    continue;
                }

                // Create new employee
                if (!request.ValidateOnly)
                {
                    var employee = Employee.Create(
                        importDto.EmployeeCode,
                        importDto.FirstName,
                        importDto.LastName,
                        importDto.Email,
                        departmentId,
                        importDto.DateOfJoining,
                        importDto.BaseSalary,
                        importDto.Currency,
                        importDto.Designation);

                    await _unitOfWork.Employees.AddAsync(employee, cancellationToken);

                    successes.Add(new ImportSuccessDto
                    {
                        RowNumber = importDto.RowNumber,
                        EmployeeCode = importDto.EmployeeCode,
                        EmployeeId = employee.Id,
                        Action = "Created"
                    });
                }
                else
                {
                    successes.Add(new ImportSuccessDto
                    {
                        RowNumber = importDto.RowNumber,
                        EmployeeCode = importDto.EmployeeCode,
                        EmployeeId = Guid.Empty,
                        Action = "WouldCreate"
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error importing row {RowNumber}", importDto.RowNumber);
                errors.Add(new ImportErrorDto
                {
                    RowNumber = importDto.RowNumber,
                    EmployeeCode = importDto.EmployeeCode,
                    ErrorMessage = ex.Message
                });
            }
        }

        // Save all changes
        if (!request.ValidateOnly && successes.Count > 0)
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        _logger.LogInformation(
            "Bulk import completed: {Success} succeeded, {Failed} failed, {Warnings} warnings",
            successes.Count, errors.Count, warnings.Count);

        return Result<BulkImportResultDto>.Success(new BulkImportResultDto
        {
            TotalRows = request.Employees.Count,
            SuccessCount = successes.Count,
            FailedCount = errors.Count,
            SkippedCount = warnings.Count,
            UpdatedCount = updatedCount,
            WasValidationOnly = request.ValidateOnly,
            Successes = successes,
            Errors = errors,
            Warnings = warnings
        });
    }

    private List<(string Field, string Message)> ValidateImportDto(EmployeeImportDto dto)
    {
        var errors = new List<(string Field, string Message)>();

        if (string.IsNullOrWhiteSpace(dto.EmployeeCode))
            errors.Add(("EmployeeCode", "Employee code is required"));

        if (string.IsNullOrWhiteSpace(dto.FirstName))
            errors.Add(("FirstName", "First name is required"));

        if (string.IsNullOrWhiteSpace(dto.LastName))
            errors.Add(("LastName", "Last name is required"));

        if (string.IsNullOrWhiteSpace(dto.Email))
            errors.Add(("Email", "Email is required"));
        else if (!dto.Email.Contains('@'))
            errors.Add(("Email", "Invalid email format"));

        if (dto.DateOfJoining > DateTime.Today)
            errors.Add(("DateOfJoining", "Date of joining cannot be in the future"));

        if (dto.BaseSalary <= 0)
            errors.Add(("BaseSalary", "Base salary must be greater than zero"));

        return errors;
    }
}
