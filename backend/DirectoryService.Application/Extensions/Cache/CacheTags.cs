using DirectoryService.Shared.Constants;

namespace DirectoryService.Application.Extensions.Cache;

public static class CacheTags
{
    public const string Departments = "departments";
    public const string Locations = "locations";
    public const string Positions = "positions";

    public static string[] Top5Departments() => WithGroup(Departments, "top5departments");
    public static string[] ForDepartment(Guid id) => WithGroup(Departments, $"department:{id}");
    public static string[] RootDepartments() => WithGroup(Departments, "root-departments");

    public static string[] ForLocation(Guid id) => WithGroup(Locations, $"location:{id}");
    public static string[] ForPosition(Guid id) => WithGroup(Positions, $"position:{id}");

    private static string[] WithGroup(string group, string specific) => [group, specific];
}