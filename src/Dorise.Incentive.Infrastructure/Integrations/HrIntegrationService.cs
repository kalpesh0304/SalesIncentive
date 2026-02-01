using Dorise.Incentive.Application.Integrations.DTOs;
using Dorise.Incentive.Application.Integrations.Services;
using Dorise.Incentive.Domain.Entities;
using Dorise.Incentive.Domain.Enums;
using Dorise.Incentive.Domain.Interfaces;
using Dorise.Incentive.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Dorise.Incentive.Infrastructure.Integrations;

/// <summary>
/// Implementation of HR integration service for employee sync.
/// "I'm a unitard!" - And I unite HR and Incentive systems!
/// </summary>
public class HrIntegrationService : IHrIntegrationService
{
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly ILogger<HrIntegrationService> _logger;

    private DateTime? _lastSyncAt;
    private string? _lastSyncStatus;
    private int? _lastSyncRecordCount;
    private string? _lastSyncError;

    public HrIntegrationService(
        IEmployeeRepository employeeRepository,
        IDepartmentRepository departmentRepository,
        IUnitOfWork unitOfWork,
        IConfiguration configuration,
        ILogger<HrIntegrationService> logger)
    {
        _employeeRepository = employeeRepository;
        _departmentRepository = departmentRepository;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<HrSyncResultDto> SyncEmployeesAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting full employee sync from HR system");

        try
        {
            var employees = await FetchEmployeesAsync(cancellationToken);
            return await SyncEmployeesAsync(employees, cancellationToken);
        }
        catch (Exception ex)
        {
            _lastSyncStatus = "Failed";
            _lastSyncError = ex.Message;
            _logger.LogError(ex, "Failed to sync employees from HR");
            throw;
        }
    }

    public async Task<HrSyncResultDto> SyncEmployeesAsync(
        IEnumerable<HrEmployeeDto> employees,
        CancellationToken cancellationToken = default)
    {
        var employeeList = employees.ToList();

        _logger.LogInformation("Processing {Count} employees for sync", employeeList.Count);

        var errors = new List<SyncErrorDto>();
        var newCount = 0;
        var updatedCount = 0;
        var deactivatedCount = 0;
        var failedCount = 0;

        foreach (var hrEmployee in employeeList)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Employee? existingEmployee = null;
            try
            {
                var employeeCode = EmployeeCode.Create(hrEmployee.EmployeeCode);
                existingEmployee = await _employeeRepository.GetByCodeAsync(
                    employeeCode, cancellationToken);

                if (existingEmployee == null)
                {
                    // Create new employee
                    await CreateEmployeeFromHrAsync(hrEmployee, cancellationToken);
                    newCount++;
                    _logger.LogDebug("Created new employee: {EmployeeCode}", hrEmployee.EmployeeCode);
                }
                else
                {
                    // Update existing employee
                    var wasUpdated = await UpdateEmployeeFromHrAsync(
                        existingEmployee, hrEmployee, cancellationToken);

                    if (wasUpdated)
                    {
                        updatedCount++;
                        _logger.LogDebug("Updated employee: {EmployeeCode}", hrEmployee.EmployeeCode);
                    }

                    // Check for deactivation
                    if (hrEmployee.DateOfLeaving.HasValue &&
                        existingEmployee.Status != EmployeeStatus.Inactive &&
                        existingEmployee.Status != EmployeeStatus.Terminated)
                    {
                        deactivatedCount++;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error syncing employee {EmployeeCode}", hrEmployee.EmployeeCode);
                errors.Add(new SyncErrorDto
                {
                    EmployeeCode = hrEmployee.EmployeeCode,
                    ErrorMessage = ex.Message,
                    Operation = existingEmployee == null ? "Create" : "Update"
                });
                failedCount++;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _lastSyncAt = DateTime.UtcNow;
        _lastSyncStatus = failedCount == 0 ? "Success" : "PartialSuccess";
        _lastSyncRecordCount = newCount + updatedCount;

        var result = new HrSyncResultDto
        {
            TotalRecords = employeeList.Count,
            NewEmployees = newCount,
            UpdatedEmployees = updatedCount,
            DeactivatedEmployees = deactivatedCount,
            FailedRecords = failedCount,
            Errors = errors,
            SyncedAt = DateTime.UtcNow,
            SyncId = Guid.NewGuid().ToString()
        };

        _logger.LogInformation(
            "HR sync completed. New: {New}, Updated: {Updated}, Deactivated: {Deactivated}, Failed: {Failed}",
            newCount, updatedCount, deactivatedCount, failedCount);

        return result;
    }

    public Task<IReadOnlyList<HrEmployeeDto>> FetchEmployeesAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching employees from HR system");

        // In a real implementation, this would call an external HR API
        // For now, return sample data for demonstration
        var sampleData = new List<HrEmployeeDto>
        {
            new HrEmployeeDto
            {
                EmployeeCode = "EMP001",
                FirstName = "John",
                LastName = "Smith",
                Email = "john.smith@dorise.com",
                Department = "Sales",
                Designation = "Sales Executive",
                ManagerCode = "MGR001",
                DateOfJoining = DateTime.UtcNow.AddYears(-2),
                Status = "Active",
                AzureAdObjectId = Guid.NewGuid().ToString()
            },
            new HrEmployeeDto
            {
                EmployeeCode = "EMP002",
                FirstName = "Jane",
                LastName = "Doe",
                Email = "jane.doe@dorise.com",
                Department = "Sales",
                Designation = "Senior Sales Executive",
                ManagerCode = "MGR001",
                DateOfJoining = DateTime.UtcNow.AddYears(-3),
                Status = "Active",
                AzureAdObjectId = Guid.NewGuid().ToString()
            },
            new HrEmployeeDto
            {
                EmployeeCode = "MGR001",
                FirstName = "Mike",
                LastName = "Manager",
                Email = "mike.manager@dorise.com",
                Department = "Sales",
                Designation = "Sales Manager",
                DateOfJoining = DateTime.UtcNow.AddYears(-5),
                Status = "Active",
                AzureAdObjectId = Guid.NewGuid().ToString()
            }
        };

        return Task.FromResult<IReadOnlyList<HrEmployeeDto>>(sampleData);
    }

    public async Task<HrEmployeeDto?> FetchEmployeeAsync(
        string employeeCode,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching employee {EmployeeCode} from HR system", employeeCode);

        var allEmployees = await FetchEmployeesAsync(cancellationToken);
        return allEmployees.FirstOrDefault(e =>
            e.EmployeeCode.Equals(employeeCode, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IReadOnlyList<HrEmployeeDto>> FetchModifiedEmployeesAsync(
        DateTime since,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching employees modified since {Since}", since);

        // In a real implementation, this would filter by modification date
        // For demo, return all employees
        return await FetchEmployeesAsync(cancellationToken);
    }

    public async Task<HrSyncResultDto> SyncEmployeeAsync(
        string employeeCode,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Syncing single employee {EmployeeCode}", employeeCode);

        var hrEmployee = await FetchEmployeeAsync(employeeCode, cancellationToken);
        if (hrEmployee == null)
        {
            return new HrSyncResultDto
            {
                TotalRecords = 0,
                FailedRecords = 1,
                Errors = new List<SyncErrorDto>
                {
                    new SyncErrorDto
                    {
                        EmployeeCode = employeeCode,
                        ErrorMessage = "Employee not found in HR system",
                        Operation = "Fetch"
                    }
                },
                SyncedAt = DateTime.UtcNow,
                SyncId = Guid.NewGuid().ToString()
            };
        }

        return await SyncEmployeesAsync(new[] { hrEmployee }, cancellationToken);
    }

    public Task<IReadOnlyList<string>> GetDepartmentsAsync(
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching departments from HR system");

        // In a real implementation, this would call HR API
        var departments = new List<string>
        {
            "Sales",
            "Marketing",
            "Operations",
            "Finance",
            "Human Resources",
            "IT"
        };

        return Task.FromResult<IReadOnlyList<string>>(departments);
    }

    public Task<IntegrationSyncStatusDto> GetSyncStatusAsync(
        CancellationToken cancellationToken = default)
    {
        var config = _configuration.GetSection("Integrations:Hr");

        return Task.FromResult(new IntegrationSyncStatusDto
        {
            IntegrationType = "HR",
            LastSyncAt = _lastSyncAt,
            LastSyncStatus = _lastSyncStatus,
            LastSyncRecordCount = _lastSyncRecordCount,
            LastSyncError = _lastSyncError,
            NextScheduledSync = _lastSyncAt?.AddMinutes(
                config.GetValue<int>("SyncIntervalMinutes", 60)),
            IsEnabled = config.GetValue<bool>("Enabled", true)
        });
    }

    public Task<bool> TestConnectionAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Testing HR connection");

        var endpoint = _configuration["Integrations:Hr:Endpoint"];
        var isConfigured = !string.IsNullOrEmpty(endpoint);

        _logger.LogInformation("HR connection test result: {Result}",
            isConfigured ? "Success" : "NotConfigured");

        return Task.FromResult(isConfigured);
    }

    private async Task CreateEmployeeFromHrAsync(
        HrEmployeeDto hrEmployee,
        CancellationToken cancellationToken)
    {
        // Find or create department
        Guid departmentId = Guid.Empty;
        if (!string.IsNullOrWhiteSpace(hrEmployee.Department))
        {
            var department = await _departmentRepository.GetByNameAsync(
                hrEmployee.Department, cancellationToken);
            departmentId = department?.Id ?? Guid.Empty;
        }

        // Find manager if specified
        Guid? managerId = null;
        if (!string.IsNullOrWhiteSpace(hrEmployee.ManagerCode))
        {
            var manager = await _employeeRepository.GetByCodeAsync(
                EmployeeCode.Create(hrEmployee.ManagerCode), cancellationToken);
            managerId = manager?.Id;
        }

        var employee = Employee.Create(
            hrEmployee.EmployeeCode,
            hrEmployee.FirstName,
            hrEmployee.LastName,
            hrEmployee.Email,
            departmentId,
            hrEmployee.DateOfJoining,
            0m, // Base salary will be updated separately
            "INR",
            hrEmployee.Designation,
            managerId);

        if (!string.IsNullOrWhiteSpace(hrEmployee.AzureAdObjectId))
        {
            employee.SetAzureAdObjectId(hrEmployee.AzureAdObjectId);
        }

        await _employeeRepository.AddAsync(employee, cancellationToken);
    }

    private async Task<bool> UpdateEmployeeFromHrAsync(
        Employee employee,
        HrEmployeeDto hrEmployee,
        CancellationToken cancellationToken)
    {
        var wasUpdated = false;

        // Update basic info using UpdateProfile
        if (employee.FirstName != hrEmployee.FirstName ||
            employee.LastName != hrEmployee.LastName ||
            employee.Email != hrEmployee.Email)
        {
            employee.UpdateProfile(
                hrEmployee.FirstName,
                hrEmployee.LastName,
                hrEmployee.Email,
                hrEmployee.Designation);
            wasUpdated = true;
        }

        // Update department using TransferToDepartment
        if (!string.IsNullOrWhiteSpace(hrEmployee.Department))
        {
            var department = await _departmentRepository.GetByNameAsync(
                hrEmployee.Department, cancellationToken);

            if (department != null && employee.DepartmentId != department.Id)
            {
                // Find manager if specified
                Guid? managerId = null;
                if (!string.IsNullOrWhiteSpace(hrEmployee.ManagerCode))
                {
                    var manager = await _employeeRepository.GetByCodeAsync(
                        EmployeeCode.Create(hrEmployee.ManagerCode), cancellationToken);
                    managerId = manager?.Id;
                }

                employee.TransferToDepartment(department.Id, managerId);
                wasUpdated = true;
            }
        }

        // Update manager separately if department didn't change
        if (!wasUpdated && !string.IsNullOrWhiteSpace(hrEmployee.ManagerCode))
        {
            var manager = await _employeeRepository.GetByCodeAsync(
                EmployeeCode.Create(hrEmployee.ManagerCode), cancellationToken);

            if (manager != null && employee.ManagerId != manager.Id)
            {
                employee.SetManager(manager.Id);
                wasUpdated = true;
            }
        }

        // Update Azure AD ID
        if (!string.IsNullOrWhiteSpace(hrEmployee.AzureAdObjectId) &&
            employee.AzureAdObjectId != hrEmployee.AzureAdObjectId)
        {
            employee.SetAzureAdObjectId(hrEmployee.AzureAdObjectId);
            wasUpdated = true;
        }

        // Update status based on HR status (handles termination too)
        if (!string.IsNullOrWhiteSpace(hrEmployee.Status))
        {
            var newStatus = MapHrStatus(hrEmployee.Status);
            if (employee.Status != newStatus)
            {
                employee.ChangeStatus(newStatus);
                wasUpdated = true;
            }
        }

        if (wasUpdated)
        {
            _employeeRepository.Update(employee);
        }

        return wasUpdated;
    }

    private static EmployeeStatus MapHrStatus(string hrStatus)
    {
        return hrStatus.ToLowerInvariant() switch
        {
            "active" => EmployeeStatus.Active,
            "probation" => EmployeeStatus.Probation,
            "inactive" => EmployeeStatus.Inactive,
            "terminated" or "resigned" or "left" => EmployeeStatus.Terminated,
            "suspended" => EmployeeStatus.OnLeave, // Suspended maps to OnLeave as closest equivalent
            _ => EmployeeStatus.Active
        };
    }
}
