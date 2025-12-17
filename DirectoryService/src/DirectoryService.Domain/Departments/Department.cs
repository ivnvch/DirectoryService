using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments.ValueObject;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Domain.Departments;

public sealed class Department
{
    private Department(
        Guid id,
        string name,
        DepartmentIdentifier departmentIdentifier,
        Guid? parentId, 
        DepartmentPath departmentPath,
        short depth,
        bool isActive,
        DateTime created,
        IEnumerable<Guid> locations)
    {
        Id = id;
        Name = name;
        DepartmentIdentifier = departmentIdentifier;
        ParentId = parentId;
        DepartmentPath = departmentPath;
        Depth = depth;
        IsActive = isActive;
        CreatedAt = created;

        var depLocations = locations.Select(l =>
            new DepartmentLocation(Guid.NewGuid(), id, l));
        _locations = depLocations.ToList();
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


    public static Result<Department, Error> Create(
        string name,
        DepartmentIdentifier departmentIdentifier,
        Guid? parentId, 
        DepartmentPath departmentPath,
        short depth,
        bool isActive,
        DateTime created,
        IEnumerable<Guid> departmentLocations)
    {
        if (string.IsNullOrWhiteSpace(name))
          return GeneralErrors.ValueIsInvalid($"'{name}'");
        
        List<Guid> locationList = departmentLocations.ToList() ?? [];
        
        if(departmentLocations.Count() == 0)
            return GeneralErrors.ValueIsInvalid($"'{departmentLocations}'");
        
        if(locationList.Any(id => id == Guid.Empty))
            return GeneralErrors.ValueIsInvalid($"location id");

        return new Department(
            Guid.NewGuid(),
            name,
            departmentIdentifier, 
            parentId, 
            departmentPath, 
            depth, 
            isActive, 
            created,
            locationList);
    }
}