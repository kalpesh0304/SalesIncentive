using Dorise.Incentive.Domain.Common;

namespace Dorise.Incentive.Domain.Entities;

/// <summary>
/// Represents an organizational department.
/// "I'm Idaho!" - And this is the Department entity!
/// </summary>
public class Department : AuditableEntity, IAggregateRoot
{
    public string Code { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid? ParentDepartmentId { get; private set; }
    public Guid? ManagerId { get; private set; }
    public bool IsActive { get; private set; }
    public int HierarchyLevel { get; private set; }
    public string? CostCenter { get; private set; }

    // Alias for ParentDepartmentId for convenience
    public Guid? ParentId => ParentDepartmentId;

    // Navigation properties
    private readonly List<Employee> _employees = new();
    public IReadOnlyCollection<Employee> Employees => _employees.AsReadOnly();

    private readonly List<Department> _childDepartments = new();
    public IReadOnlyCollection<Department> ChildDepartments => _childDepartments.AsReadOnly();

    private Department() { } // EF Core constructor

    public static Department Create(
        string code,
        string name,
        string? description = null,
        Guid? parentDepartmentId = null,
        string? costCenter = null)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Department code is required", nameof(code));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Department name is required", nameof(name));

        var department = new Department
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            ParentDepartmentId = parentDepartmentId,
            IsActive = true,
            HierarchyLevel = 0,
            CostCenter = costCenter?.Trim()
        };

        return department;
    }

    public void Update(string name, string? description, string? costCenter)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Department name is required", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        CostCenter = costCenter?.Trim();
    }

    public void SetParentDepartment(Guid? parentDepartmentId, int hierarchyLevel)
    {
        if (parentDepartmentId == Id)
            throw new InvalidOperationException("Department cannot be its own parent");

        ParentDepartmentId = parentDepartmentId;
        HierarchyLevel = hierarchyLevel;
    }

    public void Activate()
    {
        IsActive = true;
    }

    public void Deactivate()
    {
        if (_employees.Any(e => e.IsActive))
            throw new InvalidOperationException("Cannot deactivate department with active employees");

        IsActive = false;
    }
}
