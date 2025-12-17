using System.Runtime.InteropServices;
using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Locations.ValueObject;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Domain.Locations;

public sealed class Location
{
    private Location()
    {
        
    }

    public Location(
        Guid id,
        string name, 
        LocationAddress address, 
        LocationTimezone timezone, 
        bool isActive, 
        DateTime createdAt)
    {
        Id = id;
        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = isActive;
        CreatedAt = createdAt;
        UpdatedAt = createdAt;
    }
    private readonly List<DepartmentLocation> _departments = [];
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public LocationAddress Address { get; private set; }
    public LocationTimezone Timezone {get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public IReadOnlyList<DepartmentLocation> Departments => _departments;

    public static Result<Location, Error> Create(string name, LocationAddress address, LocationTimezone timezone, bool isActive, DateTime createdAt)
    {
        if(string.IsNullOrWhiteSpace(name))
            return GeneralErrors.ValueIsInvalid("Name");
        
        return new Location(Guid.NewGuid(), name, address, timezone, isActive, createdAt);
    }
}