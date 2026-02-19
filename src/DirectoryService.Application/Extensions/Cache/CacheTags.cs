namespace DirectoryService.Application.Extensions.Cache;

public static class CacheTags
{
    public const string Departments = "departments";
    public const string Positions = "positions";
    public const string Locations = "locations";
    
    public static string ForDepartment(Guid departmentId)
        => $"department:{departmentId}";
    
    public static string ForPosition(Guid positionId)
        => $"position:{positionId}";
    
    public static string ForLocation(Guid locationId)
        => $"location:{locationId}";
}