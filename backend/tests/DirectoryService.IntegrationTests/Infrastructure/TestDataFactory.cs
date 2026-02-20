using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObject;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObject;
using DirectoryService.Domain.Positions;
using DirectoryService.Infrastructure;
using DirectoryService.IntegrationTests.Infrastructure.Constants;

namespace DirectoryService.IntegrationTests.Infrastructure;

public static class TestDataFactory
{
    public static async Task<Guid> CreateLocationAsync(
        DirectoryDbContext context,
        string name = LocationTestConstants.DefaultName,
        string street = LocationTestConstants.DefaultStreet,
        string city = LocationTestConstants.DefaultCity,
        string house = LocationTestConstants.DefaultHouse,
        string country = LocationTestConstants.DefaultCountry,
        string apartment = LocationTestConstants.DefaultApartment,
        string timezone = LocationTestConstants.DefaultTimezone,
        CancellationToken cancellationToken = default)
    {
        var location = CreateLocationEntity(
            name, 
            street, 
            city, 
            house, 
            country, 
            apartment, 
            timezone);

        context.Add(location);
        await context.SaveChangesAsync(cancellationToken);

        return location.Id;
    }

    public static async Task<Position> CreatePositionAsync(
        DirectoryDbContext context,
        string positionName = PositionTestConstants.DefaultName,
        string? positionDescription = PositionTestConstants.DefaultDescription,
        IEnumerable<Guid>? departmentIds = null,
        CancellationToken cancellationToken = default)
    {
        Position position = CreatePositionEntity(
            positionName, 
            positionDescription, 
            departmentIds);
        
        context.Add(position);
        await context.SaveChangesAsync(cancellationToken);
        
        return position;
    }

    public static async Task<Department> CreateDepartmentAsync(
        DirectoryDbContext context,
        int locationsCount = DepartmentTestConstants.DefaultLocationsCount,
        Guid? departmentId = null,
        string name = DepartmentTestConstants.DefaultName,
        string identifier = DepartmentTestConstants.DefaultIdentifier,
        CancellationToken cancellationToken = default)
    {
        return await CreateDepartmentCoreAsync(
            context,
            locationsCount,
            departmentId,
            name,
            identifier,
            parent: null,
            cancellationToken);
    }

    public static async Task<Guid> CreateDepartmentIdAsync(
        DirectoryDbContext context,
        int locationsCount = DepartmentTestConstants.DefaultLocationsCount,
        Guid? departmentId = null,
        string name = DepartmentTestConstants.DefaultName,
        string identifier = DepartmentTestConstants.DefaultIdentifier,
        CancellationToken cancellationToken = default)
    {
        var department = await CreateDepartmentAsync(
            context,
            locationsCount,
            departmentId,
            name,
            identifier,
            cancellationToken);
        return department.Id;
    }

    public static async Task<Department> CreateChildDepartmentAsync(
        DirectoryDbContext context,
        Department parent,
        int locationsCount = DepartmentTestConstants.DefaultLocationsCount,
        Guid? departmentId = null,
        string name = DepartmentTestConstants.DefaultName,
        string identifier = DepartmentTestConstants.DefaultIdentifier,
        CancellationToken cancellationToken = default)
    {
        return await CreateDepartmentCoreAsync(
            context,
            locationsCount,
            departmentId,
            name,
            identifier,
            parent,
            cancellationToken);
    }

    private static Location CreateLocationEntity(
        string name = LocationTestConstants.DefaultName,
        string street = LocationTestConstants.DefaultStreet,
        string city = LocationTestConstants.DefaultCity,
        string house = LocationTestConstants.DefaultHouse,
        string country = LocationTestConstants.DefaultCountry,
        string apartment = LocationTestConstants.DefaultApartment,
        string timezone = LocationTestConstants.DefaultTimezone)
    {
        return Location.Create(
            LocationName.Create(name).Value,
            LocationAddress.Create(street, city, house, country, apartment).Value,
            LocationTimezone.Create(timezone).Value).Value;
    }

    public static Position CreatePositionEntity(
        string name,
        string? description,
        IEnumerable<Guid>? departmentIds = null)
    {
        var positionId = Guid.NewGuid();
        var positions = departmentIds?
            .Select(departmentId => new DepartmentPosition(
                Guid.NewGuid(),
                departmentId,
                positionId))
            .ToList() ?? [];

        return Position.Create(
            name,
            description,
            positions).Value;
    }

    private static async Task<Department> CreateDepartmentCoreAsync(
        DirectoryDbContext context,
        int locationsCount,
        Guid? departmentId,
        string name,
        string identifier,
        Department? parent,
        CancellationToken cancellationToken)
    {
        ValidateLocationsCount(locationsCount);

        var actualDepartmentId = departmentId ?? Guid.NewGuid();
        var locations = CreateLocations(locationsCount);

        context.AddRange(locations);

        var departmentLocations = locations
            .Select(location => DepartmentLocation.Create(actualDepartmentId, location.Id))
            .ToList();

        var department = parent is null
            ? Department.CreateParent(
                DepartmentName.Create(name).Value,
                DepartmentIdentifier.Create(identifier).Value,
                departmentLocations,
                actualDepartmentId).Value
            : Department.CreateChild(
                DepartmentName.Create(name).Value,
                DepartmentIdentifier.Create(identifier).Value,
                parent,
                departmentLocations,
                actualDepartmentId).Value;

        context.Add(department);
        await context.SaveChangesAsync(cancellationToken);

        return department;
    }

    private static List<Location> CreateLocations(int locationsCount)
    {
        var locations = new List<Location>(locationsCount);
        for (var i = 0; i < locationsCount; i++)
        {
            locations.Add(CreateLocationEntity());
        }

        return locations;
    }

    private static void ValidateLocationsCount(int locationsCount)
    {
        if (locationsCount < DepartmentTestConstants.MinLocationsCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(locationsCount),
                $"locationsCount must be greater than or equal to {DepartmentTestConstants.MinLocationsCount}.");
        }
    }
}
