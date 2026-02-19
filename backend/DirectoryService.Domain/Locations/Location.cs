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
        LocationName name, 
        LocationAddress address,  
        LocationTimezone timezone)
    {
        Id = id;
        Name = name;
        Address = address;
        Timezone = timezone;
        IsActive = true;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = CreatedAt;
        DeletedAt =  null;
    }
    private readonly List<DepartmentLocation> _departments = [];
    public Guid Id { get; private set; }
    public LocationName Name { get; private set; }
    public LocationAddress Address { get; private set; }
    public LocationTimezone Timezone {get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }
    public IReadOnlyList<DepartmentLocation> Departments => _departments;

    public static Result<Location, Error> Create(
        LocationName name, 
        LocationAddress address, 
        LocationTimezone timezone)
    {
        return new Location(Guid.NewGuid(), name, address, timezone);
    }
}