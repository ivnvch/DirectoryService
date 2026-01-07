namespace DirectoryService.Domain.DepartmentLocations;

public sealed class DepartmentLocation
{
    private DepartmentLocation() { }

    public DepartmentLocation(Guid id, Guid departmentId, Guid locationId)
    {
        Id = id;
        DepartmentId = departmentId;
        LocationId = locationId;
    }
    
    public Guid Id { get; private set; }
    public Guid DepartmentId { get; private set; }
    public Guid LocationId { get; private set; }
}