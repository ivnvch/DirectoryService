using System.Runtime.InteropServices;

namespace DirectoryService.Domain.DepartmentLocations;

public sealed class DepartmentLocation
{
    private DepartmentLocation() { }

    private DepartmentLocation(Guid departmentId, Guid locationId)
    {
        DepartmentId = departmentId;
        LocationId = locationId;
    }
    
    public Guid Id { get; private set; }
    public Guid DepartmentId { get; private set; }
    public Guid LocationId { get; private set; }

    public static DepartmentLocation Create(Guid departmentId, Guid locationId)
    {
        return new DepartmentLocation(departmentId, locationId);
    }
}