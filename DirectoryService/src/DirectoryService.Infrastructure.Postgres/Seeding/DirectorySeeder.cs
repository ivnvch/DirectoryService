using System.Reflection;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.DepartmentPositions;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObject;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObject;
using DirectoryService.Domain.Positions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.Seeding;

public class DirectorySeeder : ISeeder
{
    private const int LocationCount = 35;
    private const int ParentDepartmentCount = 8;
    private const int ChildDepartmentsPerParent = 15;
    private const int DepartmentLocationsPerDepartment = 7;
    private const int PositionCount = 20;
    private const int DepartmentsPerPosition = 20;

    private static readonly string[] Timezones =
    [
        "Europe/Moscow",
        "Europe/London",
        "America/New_York",
        "America/Chicago",
        "Asia/Tokyo",
        "Asia/Dubai"
    ];

    private static readonly string[] Countries = ["USA", "UK", "Japan", "UAE", "Germany", "Canada"];
    private static readonly string[] Cities = ["New York", "London", "Tokyo", "Dubai", "Berlin", "Toronto"];
    private static readonly string[] Streets = ["Main St", "High St", "Oak Ave", "Maple Rd", "Sunset Blvd"];

    private readonly DirectoryDbContext _context;
    private readonly ILogger<DirectorySeeder> _logger;

    public DirectorySeeder(DirectoryDbContext context, ILogger<DirectorySeeder> logger)
    {
        _context = context;
        _logger = logger;
    }
    
    public async Task SeedAsync()
    {
        _logger.LogInformation("Starting Seeding directory");
            
        try
        {
            await SeedData();
            
            _logger.LogInformation("Finished Seeding directory");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding directory");
        }
    }

    private async Task SeedData()
    {
        await using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            await ClearDatabaseAsync();

            var random = new Random();

            var locations = CreateLocations(random);
            _context.Locations.AddRange(locations);

            var departments = CreateDepartments(random, locations);
            _context.Departments.AddRange(departments);

            var positions = CreatePositions(random, departments);
            _context.Positions.AddRange(positions);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private async Task ClearDatabaseAsync()
    {
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM department_position");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM department_locations");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM departments");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM positions");
        await _context.Database.ExecuteSqlRawAsync("DELETE FROM locations");
    }

    private static List<Location> CreateLocations(Random random)
    {
        var locations = new List<Location>(LocationCount);

        for (int i = 0; i < LocationCount; i++)
        {
            var nameResult = LocationName.Create($"Location {i + 1}");
            var addressResult = LocationAddress.Create(
                country: Countries[i % Countries.Length],
                city: Cities[i % Cities.Length],
                street: Streets[i % Streets.Length],
                house: (i + 10).ToString(),
                apartment: (i % 2 == 0) ? (i + 100).ToString() : null);
            var timezoneResult = LocationTimezone.Create(Timezones[random.Next(Timezones.Length)]);

            if (nameResult.IsFailure || addressResult.IsFailure || timezoneResult.IsFailure)
                throw new InvalidOperationException("Invalid location seed data.");

            var locationResult = Location.Create(nameResult.Value, addressResult.Value, timezoneResult.Value);
            if (locationResult.IsFailure)
                throw new InvalidOperationException("Failed to create location seed data.");

            locations.Add(locationResult.Value);
        }

        return locations;
    }

    private static List<Department> CreateDepartments(Random random, IReadOnlyList<Location> locations)
    {
        var departments = new List<Department>(ParentDepartmentCount * (ChildDepartmentsPerParent + 1));

        for (int parentIndex = 0; parentIndex < ParentDepartmentCount; parentIndex++)
        {
            var parentId = Guid.NewGuid();
            var parentName = DepartmentName.Create($"Department {parentIndex + 1}");
            var parentIdentifier = DepartmentIdentifier.Create($"dept-{GetAlphabeticCode(parentIndex)}");
            var parentLocations = CreateDepartmentLocations(parentId, locations, random);

            if (parentName.IsFailure || parentIdentifier.IsFailure)
                throw new InvalidOperationException("Invalid department seed data.");

            var parentResult = Department.CreateParent(
                parentName.Value,
                parentIdentifier.Value,
                parentLocations,
                parentId);

            if (parentResult.IsFailure)
                throw new InvalidOperationException("Failed to create parent department seed data.");

            var parent = parentResult.Value;
            departments.Add(parent);

            for (int childIndex = 0; childIndex < ChildDepartmentsPerParent; childIndex++)
            {
                var childId = Guid.NewGuid();
                var childName = DepartmentName.Create($"Department {parentIndex + 1}.{childIndex + 1}");
                var childIdentifier = DepartmentIdentifier.Create(
                    $"dept-{GetAlphabeticCode(parentIndex)}-{GetAlphabeticCode(childIndex)}");
                var childLocations = CreateDepartmentLocations(childId, locations, random);

                if (childName.IsFailure || childIdentifier.IsFailure)
                    throw new InvalidOperationException("Invalid department seed data.");

                var childResult = Department.CreateChild(
                    childName.Value,
                    childIdentifier.Value,
                    parent,
                    childLocations,
                    childId);

                if (childResult.IsFailure)
                    throw new InvalidOperationException("Failed to create child department seed data.");

                departments.Add(childResult.Value);
            }
        }

        return departments;
    }

    private static IEnumerable<DepartmentLocation> CreateDepartmentLocations(
        Guid departmentId,
        IReadOnlyList<Location> locations,
        Random random)
    {
        int count = Math.Min(DepartmentLocationsPerDepartment, locations.Count);
        var selectedLocations = locations
            .OrderBy(_ => random.Next())
            .Take(count)
            .Select(l => l.Id)
            .ToList();

        return selectedLocations.Select(locationId => DepartmentLocation.Create(departmentId, locationId));
    }

    private static List<Position> CreatePositions(Random random, IReadOnlyList<Department> departments)
    {
        var positions = new List<Position>(PositionCount);

        for (int i = 0; i < PositionCount; i++)
        {
            int departmentsCount = Math.Min(DepartmentsPerPosition, departments.Count);
            var departmentIds = departments
                .OrderBy(_ => random.Next())
                .Take(departmentsCount)
                .Select(d => d.Id)
                .ToList();

            var positionId = Guid.NewGuid();
            var departmentPositions = departmentIds
                .Select(d => new DepartmentPosition(Guid.NewGuid(), d, positionId))
                .ToList();

            var positionResult = Position.Create(
                $"Position {i + 1}",
                $"Position description {i + 1}",
                departmentPositions);

            if (positionResult.IsFailure)
                throw new InvalidOperationException("Failed to create position seed data.");

            var position = positionResult.Value;
            SetPositionId(position, positionId);

            positions.Add(position);
        }

        return positions;
    }

    private static void SetPositionId(Position position, Guid positionId)
    {
        var idProperty = typeof(Position).GetProperty(
            nameof(Position.Id),
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (idProperty is null)
            throw new InvalidOperationException("Position Id property not found for seeding.");

        idProperty.SetValue(position, positionId);
    }

    private static string GetAlphabeticCode(int index)
    {
        const int alphabetLength = 26;
        index = Math.Abs(index);

        var chars = new List<char>();
        do
        {
            chars.Add((char)('a' + (index % alphabetLength)));
            index = (index / alphabetLength) - 1;
        } while (index >= 0);

        chars.Reverse();
        return new string(chars.ToArray());
    }
}