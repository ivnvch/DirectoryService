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
        Guid? parentId,
        DepartmentName name,
        DepartmentIdentifier departmentIdentifier,
        DepartmentPath departmentPath,
        int depth,
        IEnumerable<DepartmentLocation> departmentLocations)
    {
        Id = id;
        ParentId = parentId;
        Name = name;
        DepartmentIdentifier = departmentIdentifier;
        DepartmentPath = departmentPath;
        Depth = depth;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        DeletedAt = null;
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
    public DateTime? DeletedAt { get; private set; }
    public IReadOnlyList<DepartmentLocation> Locations => _locations;
    public IReadOnlyList<DepartmentPosition> Positions => _positions;
    public IReadOnlyList<Department> ChildrenDepartments => _childrenDepartments;

    public static Result<Department, Error> CreateParent(
        DepartmentName name,
        DepartmentIdentifier departmentIdentifier,
        IEnumerable<DepartmentLocation> departmentLocations,
        Guid? departmentId = null)
    {
        if (string.IsNullOrWhiteSpace(name.Value))
            return GeneralErrors.ValueIsInvalid($"'{name}'");

        List<DepartmentLocation> locationList = departmentLocations.ToList();

        if (locationList.Count() == 0)
            return GeneralErrors.ValueIsInvalid($"'{locationList}'");

        var path = DepartmentPath.CreateParent(departmentIdentifier).Value;

        return new Department(
            departmentId ?? Guid.NewGuid(),
            null,
            name,
            departmentIdentifier,
            path,
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
        if (departmentLocationsList.Count == 0)
            return Error.Validation("department.location", "Department locations must contain at least one location");

        var path = parent.DepartmentPath.CreateChild(departmentIdentifier);

        return new Department(
            departmentId ?? Guid.NewGuid(),
            parent.Id,
            name,
            departmentIdentifier,
            path.Value,
            parent.Depth + 1,
            departmentLocationsList
        );
    }

    public UnitResult<Error> UpdateLocations(IEnumerable<DepartmentLocation> locationIds)
    {
        var departmentLocations = locationIds.ToList();
        if (departmentLocations.Count == 0)
            return GeneralErrors.ValueIsRequired($"'{departmentLocations}'");

        _locations.Clear();
        _locations.AddRange(departmentLocations);

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> UpdatePathWithoutParent()
    {
        ParentId = null;
        Depth = 0;
        var newPath = DepartmentPath.CreateParent(identifier: DepartmentIdentifier);
        if (newPath.IsFailure)
            return newPath.Error;

        DepartmentPath = newPath.Value;

        return UnitResult.Success<Error>();
    }

    public UnitResult<Error> UpdatePathWithParent(Guid parentId, short parentDepth, DepartmentPath departmentPath)
    {
        ParentId = parentId;
        Depth = (short)(parentDepth + 1);
        var path = DepartmentPath.UpdatePath(DepartmentIdentifier, departmentPath);
        if (path.IsFailure)
            return path.Error;

        DepartmentPath = path.Value;

        return UnitResult.Success<Error>();
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.UtcNow;
        IsActive = false;
    }
}