using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Locations.ValueObject;

namespace DirectoryService.Domain.Locations;

public class Location
{
    private readonly List<DepartmentLocation> _locations = [];
    public Guid Id { get; private set; }
    public string Name { get; private set; }
    public LocationAddress Address { get; private set; }
    public LocationTimezone Timezone {get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public IReadOnlyList<DepartmentLocation> Locations => _locations;
}