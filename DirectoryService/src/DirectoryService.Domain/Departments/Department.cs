using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments.ValueObject;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Domain.Departments;

public sealed class Department
{
    private Department()
    {
        
    }
    private Department(
        Guid id,
        DepartmentName name,
        DepartmentIdentifier departmentIdentifier,
        DepartmentPath departmentPath,
        int depth,
        IEnumerable<DepartmentLocation> departmentLocations)
    {
        Id = id;
        Name = name;
        DepartmentIdentifier = departmentIdentifier;
        DepartmentPath = departmentPath;
        Depth = depth;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;

        /*var depLocations = locations.Select(l =>
            new DepartmentLocation(Guid.NewGuid(), id, l));*/
        _locations = departmentLocations.ToList();
    }

    private readonly List<DepartmentPosition> _positions = [];
    private readonly List<DepartmentLocation> _locations = [];
    private readonly List<Department> _childrenDepartments = [];

    public Guid Id { get; private set; }
    public DepartmentName Name { get; private set; } = null!;
    public DepartmentIdentifier DepartmentIdentifier { get; private set; } = null!;
    public Guid? ParentId { get; private set; }
    public DepartmentPath DepartmentPath { get; private set; } = null!;
    public int Depth { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public IReadOnlyList<DepartmentLocation> Locations => _locations;
    public IReadOnlyList<DepartmentPosition> Positions => _positions;
    public IReadOnlyList<Department> ChildrenDepartments => _childrenDepartments;


    public static Result<Department, Error> CreateParent(
        DepartmentName name,
        DepartmentIdentifier departmentIdentifier,
        DepartmentPath departmentPath,
        IEnumerable<DepartmentLocation> departmentLocations)
    {
        if (string.IsNullOrWhiteSpace(name.Value))
          return GeneralErrors.ValueIsInvalid($"'{name}'");
        
        List<DepartmentLocation> locationList = departmentLocations.ToList();
        
        if(locationList.Count() == 0)
            return GeneralErrors.ValueIsInvalid($"'{locationList}'");
        

        return new Department(
            Guid.NewGuid(),
            name,
            departmentIdentifier, 
            departmentPath,
            0,
            locationList);
    }

    public static Result<Department, Error> CreateChild(
        DepartmentName name,
        DepartmentIdentifier departmentIdentifier,
        Department parent,
        IEnumerable<DepartmentLocation> departmentLocations,
        Guid? departmentId = null)
    {
        var departmentLocationsList = departmentLocations.ToList();
        if(departmentLocationsList.Count == 0)
            return Error.Validation("department.location", "Department locations must contain at least one location");

        var path = parent.DepartmentPath.CreateChild(departmentIdentifier);

        return new Department(
            departmentId ?? Guid.NewGuid(),
            name,
            departmentIdentifier,
            path.Value,
            parent.Depth + 1,
            departmentLocationsList
            );
    }
}