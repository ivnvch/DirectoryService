using DirectoryService.Application.Departments.Commands.CreateDepartments;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObject;
using DirectoryService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class CreateDepartmentTest : DirectoryBaseTests
{
    //private readonly DirectoryTestWebFactory _factory;
    
    public CreateDepartmentTest(DirectoryTestWebFactory factory) : base(factory)
    {
       // _factory = factory;
    }
    
    [Fact]
    public async Task CreateDepartment_with_valid_data_should_succeed()
    {
        //arrange
        var locationId = await CreateLocation();

        var cancellationToken = CancellationToken.None;
        
        //act
        var result = await ExecuteHandler((sut) =>
        {

            var command = new CreateDepartmentCommand("Department",
                "Identifier",
                null,
                [locationId]);
            
            return sut.Handle(command, cancellationToken);
        });
            
        
        //assert
        await ExecuteInDb(async context =>
        {
            var department = await context.Departments.FirstAsync(x => x.Id == result.Value, cancellationToken);

            Assert.NotNull(department);
            Assert.Equal(department.Id, result.Value);

            Assert.True(result.IsSuccess);
            Assert.NotEqual(Guid.Empty, result.Value);
        });
    }

    [Fact]
    public async Task CreateDepartment_with_invalid_data_should_fail()
    {
        //arrange
        var locationId = await CreateLocation();
        
        var cancellationToken = CancellationToken.None;
        //act
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand("", "", Guid.NewGuid(), [locationId]);
            
            return sut.Handle(command, cancellationToken);
        });
        //assert
        Assert.False(result.IsSuccess);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public async Task CreateDepartment_with_list_locationId_should_succeed()
    {
        //arrange
        var locationId = await CreateLocation();
        var anotherLocationId = await CreateLocation();
        
        var listLocationId = new List<Guid> { locationId, anotherLocationId };

        var cancellationToken = CancellationToken.None;
        //act
        var result = await ExecuteHandler(sut =>
        {
            var command = new CreateDepartmentCommand("DepartmentCommandName", "DepartmentIDentifier",
                null, listLocationId.ToArray());
            
            return sut.Handle(command, cancellationToken);
        });
        
        //assert
        await ExecuteInDb(async context =>
        {
            var locations = await context.Locations
                .AsNoTracking()
                .Select(l => l.Id)
                .ToListAsync(cancellationToken);
            var department = await context.Departments.FirstAsync(x => x.Id == result.Value, cancellationToken);
            
            Assert.NotNull(department);
            Assert.Equal(department.Id, result.Value);
            Assert.True(result.IsSuccess);
            Assert.NotNull(locations);
            Assert.True(new HashSet<Guid>(listLocationId).SetEquals(locations));
        });
    }

    private async Task<Guid> CreateLocation()
    {
      return  await ExecuteInDb(async context =>
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

    private async Task<T> ExecuteHandler<T>(Func<CreateDepartmentCommandHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<CreateDepartmentCommandHandler>();

        return await action(sut);
        
    }

    
}