using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObject;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObject;
using DirectoryService.Infrastructure;

namespace DirectoryService.IntegrationTests.Infrastructure;

public static class TestDataFactory
{
    private const string DefaultLocationName = "Minsk";
    private const string DefaultStreet = "Street";
    private const string DefaultCity = "City";
    private const string DefaultHouse = "House";
    private const string DefaultCountry = "Country";
    private const string DefaultApartment = "23";
    private const string DefaultTimezone = "Europe/Minsk";
    private const string DefaultDepartmentName = "Department";
    private const string DefaultDepartmentIdentifier = "Identifier";

    public static async Task<Guid> CreateLocationAsync(
        DirectoryDbContext context,
        string name = DefaultLocationName,
        string street = DefaultStreet,
        string city = DefaultCity,
        string house = DefaultHouse,
        string country = DefaultCountry,
        string apartment = DefaultApartment,
        string timezone = DefaultTimezone)
    {
        var location = CreateLocationEntity(name, street, city, house, country, apartment, timezone);

        context.Add(location);
        await context.SaveChangesAsync();

        return location.Id;
    }

    public static async Task<Department> CreateDepartmentAsync(
        DirectoryDbContext context,
        int locationsCount = 1,
        Guid? departmentId = null,
        string name = DefaultDepartmentName,
        string identifier = DefaultDepartmentIdentifier)
    {
        if (locationsCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(locationsCount), "locationsCount must be greater than zero.");

        var actualDepartmentId = departmentId ?? Guid.NewGuid();
        var locations = new List<Location>(locationsCount);

        for (var i = 0; i < locationsCount; i++)
        {
            var location = CreateLocationEntity();
            locations.Add(location);
            context.Add(location);
        }

        var departmentLocations = locations
            .Select(location => DepartmentLocation.Create(actualDepartmentId, location.Id))
            .ToList();

        var department = Department.CreateParent(
            DepartmentName.Create(name).Value,
            DepartmentIdentifier.Create(identifier).Value,
            departmentLocations,
            actualDepartmentId).Value;

        context.Add(department);
        await context.SaveChangesAsync();

        return department;
    }

    public static async Task<Guid> CreateDepartmentIdAsync(
        DirectoryDbContext context,
        int locationsCount = 1,
        Guid? departmentId = null,
        string name = DefaultDepartmentName,
        string identifier = DefaultDepartmentIdentifier)
    {
        var department = await CreateDepartmentAsync(context, locationsCount, departmentId, name, identifier);
        return department.Id;
    }

    public static async Task<Department> CreateChildDepartmentAsync(
        DirectoryDbContext context,
        Department parent,
        int locationsCount = 1,
        Guid? departmentId = null,
        string name = DefaultDepartmentName,
        string identifier = DefaultDepartmentIdentifier)
    {
        if (locationsCount <= 0)
            throw new ArgumentOutOfRangeException(nameof(locationsCount), "locationsCount must be greater than zero.");

        var actualDepartmentId = departmentId ?? Guid.NewGuid();
        var locations = new List<Location>(locationsCount);

        for (var i = 0; i < locationsCount; i++)
        {
            var location = CreateLocationEntity();
            locations.Add(location);
            context.Add(location);
        }

        var departmentLocations = locations
            .Select(location => DepartmentLocation.Create(actualDepartmentId, location.Id))
            .ToList();

        var department = Department.CreateChild(
            DepartmentName.Create(name).Value,
            DepartmentIdentifier.Create(identifier).Value,
            parent,
            departmentLocations,
            actualDepartmentId).Value;

        context.Add(department);
        await context.SaveChangesAsync();

        return department;
    }

    private static Location CreateLocationEntity(
        string name = DefaultLocationName,
        string street = DefaultStreet,
        string city = DefaultCity,
        string house = DefaultHouse,
        string country = DefaultCountry,
        string apartment = DefaultApartment,
        string timezone = DefaultTimezone)
    {
        return Location.Create(
            LocationName.Create(name).Value,
            LocationAddress.Create(street, city, house, country, apartment).Value,
            LocationTimezone.Create(timezone).Value).Value;
    }
}
