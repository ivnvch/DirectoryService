using DirectoryService.Application.Departments.Commands.UpdateDepartmentLocation;
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
        var departmentId = await CreateDepartmentIdAsync(locationsCount: 2);
        var locationIds = await CreateLocationAsync();
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
        var locationId = await CreateLocationAsync();
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
        var departmentId = await CreateDepartmentIdAsync(locationsCount: 2);
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

}