using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPostitions;
using DirectoryService.Domain.Departments.ValueObject;

namespace DirectoryService.Domain.Departments;

public class Department
{
    private Department(
        string name,
        DepartmentIdentifier departmentIdentifier,
        Guid? parentId, 
        DepartmentPath departmentPath,
        short depth,
        bool isActive,
        DateTime created)
    {
        Id = Guid.NewGuid();
        Name = name;
        DepartmentIdentifier = departmentIdentifier;
        ParentId = parentId;
        DepartmentPath = departmentPath;
        Depth = depth;
        IsActive = isActive;
        CreatedAt = created;
    }

    private readonly List<DepartmentPosition> _positions = [];
    private readonly List<DepartmentLocation> _locations = [];

    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public DepartmentIdentifier DepartmentIdentifier { get; private set; }
    public Guid? ParentId { get; private set; }
    public DepartmentPath DepartmentPath { get; private set; }
    public short Depth { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public IReadOnlyList<DepartmentLocation> Locations => _locations;

    public IReadOnlyList<DepartmentPosition> Positions => _positions;


    public static Result<Department> Create(
        string name,
        DepartmentIdentifier departmentIdentifier,
        Guid? parentId, 
        DepartmentPath departmentPath,
        short depth,
        bool isActive,
        DateTime created)
    {
        if (string.IsNullOrWhiteSpace(name))
          return Result.Failure<Department>($"The name '{name}' is invalid");

        return new Department(name, departmentIdentifier, parentId, departmentPath, depth, isActive, created);
    }
}