using DirectoryService.Application.Departments.Commands.CreateDepartments;
using DirectoryService.Application.Departments.Commands.UpdateDepartmentLocation;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObject;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObject;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class UpdateDepartmentLocationTest : DirectoryBaseTests
{
    //private readonly DirectoryTestWebFactory _factory;
    public UpdateDepartmentLocationTest(DirectoryTestWebFactory factory) : base(factory)
    {
        //_factory = factory;
    }

    [Fact]
    public async Task UpdateDepartmentLocation_with_valid_data_should_succeed()
    {
        var departmentId = await CreateDepartment();
        var locationIds = await CreateLocation();
        CancellationToken cancellationToken = CancellationToken.None;

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentLocationCommand(
                departmentId, [locationIds]);

            return sut.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async context =>
        {
            var department = await context.Departments
                .Include(d => d.Locations)
                .SingleOrDefaultAsync(d => d.Id == departmentId, cancellationToken);

            Assert.NotNull(department);
            Assert.Equal(result, department.Id);
            Assert.True(result.IsSuccess);
            Assert.True(new HashSet<Guid>(department.Locations.Select(l => l.LocationId)).SetEquals([locationIds]));
        });
    }

    [Fact]
    public async Task UpdateDepartmentLocation_with_NotExistDepartment_should_fail()
    {
        //affect
        var locationId = await CreateLocation();
        CancellationToken cancellationToken = CancellationToken.None;

        //act
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentLocationCommand(Guid.NewGuid(), [locationId]);
            
            return sut.Handle(command,  cancellationToken);
        });

        //assert
        Assert.NotNull(result.Error);
        Assert.False(result.IsSuccess);
    }

    [Fact]
    public async Task UpdateDepartmentLocation_with_empty_locations_should_fail()
    {
        var departmentId = await CreateDepartment();
        var initialLocationIds = await ExecuteInDb(async context =>
        {
            var department = await context.Departments
                .Include(d => d.Locations)
                .SingleOrDefaultAsync(d => d.Id == departmentId);

            return department.Locations.Select(l => l.LocationId).ToArray();
        });
        CancellationToken cancellationToken = CancellationToken.None;

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentLocationCommand(departmentId, Enumerable.Empty<Guid>());
            return sut.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async context =>
        {
            var department = await context.Departments
                .Include(d => d.Locations)
                .SingleOrDefaultAsync(d => d.Id == departmentId, cancellationToken);

            var currentLocationIds = department.Locations.Select(l => l.LocationId).ToArray();

            Assert.NotNull(result.Error);
            Assert.False(result.IsSuccess);
            Assert.True(new HashSet<Guid>(initialLocationIds).SetEquals(currentLocationIds));
        });
    }
    
    private async Task<T> ExecuteHandler<T>(Func<UpdateDepartmentLocationCommandHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<UpdateDepartmentLocationCommandHandler>();

        return await action(sut);
    }

    private async Task<Guid> CreateLocation()
    {
        return await ExecuteInDb(async context =>
        {
            var location = Location.Create(
                LocationName.Create("Минск").Value,
                LocationAddress.Create("длыва", "длфвыо", "двыла", "выдла", "23").Value,
                LocationTimezone.Create("Europe/Minsk").Value);

            context.Add(location.Value);
            await context.SaveChangesAsync();

            return location.Value.Id;
        });
    }

    private async Task<Guid> CreateDepartment()
    {
        return await ExecuteInDb(async context =>
        {
            var locationId = await CreateLocation();
            var anotherLocationId = await CreateLocation();

            var departmentId = Guid.NewGuid();
            var departmentLocation = DepartmentLocation.Create(
                departmentId,
                locationId);
            
            var anotherDepartmentLocation = DepartmentLocation.Create(
                departmentId,
                anotherLocationId);

            var departmentName = DepartmentName.Create("депортамент");
            var departmentIdentifier = DepartmentIdentifier.Create("Identifier");

            var department = Department.CreateParent(
                departmentName.Value,
                departmentIdentifier.Value,
                [departmentLocation, anotherDepartmentLocation],
                departmentId);

            context.Add(department.Value);
            await context.SaveChangesAsync();
            return department.Value.Id;
        });
    }
}