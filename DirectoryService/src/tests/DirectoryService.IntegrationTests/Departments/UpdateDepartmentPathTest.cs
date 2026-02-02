using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Commands.UpdateDepartmentPath;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObject;
using DirectoryService.Domain.Locations;
using DirectoryService.Domain.Locations.ValueObject;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class UpdateDepartmentPathTest : DirectoryBaseTests
{
    public UpdateDepartmentPathTest(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task UpdateParentDepartmentPath_with_valid_data_should_succeed()
    {
        var department = await CreateDepartment();
        CancellationToken cancellationToken = CancellationToken.None;

        var departmentBefore = await ExecuteInDb(async context =>
        {
            return await context.Departments
                .SingleOrDefaultAsync(x => x.Id == department.Id, cancellationToken);
        });
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentPathCommand(department.Id, null);
            return sut.Handle(command, cancellationToken);
        });

        await ExecuteInDb(async context =>
        {
            var departmentAfter = await context.Departments
                .SingleOrDefaultAsync(x => x.Id == result.Value, cancellationToken);
            
            Assert.NotNull(departmentBefore);
            Assert.NotNull(departmentAfter);
            Assert.Equal(departmentBefore.Id, departmentAfter.Id);
            Assert.Null(departmentAfter.ParentId);
        });
    }

    [Fact]
    public async Task UpdateDepartmentPath_when_newParentId_isDescendant_should_failed()
    {
        var departmentWithDescendant = await CreateDepartmentWithDescendant();
        var department = await CreateDepartment(departmentWithDescendant);
        
        CancellationToken cancellationToken = CancellationToken.None;

        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentPathCommand(departmentWithDescendant.Id, department.Id);
            return sut.Handle(command, cancellationToken);
        });
        Assert.True(result.IsFailure);
    }

    [Fact]
    public async Task UpdateDepartmentPath_with_validParentId_should_succeed()
    {
        var departmentWithDescendant = await CreateDepartmentWithDescendant();
        var department = await CreateDepartment();
        CancellationToken cancellationToken = CancellationToken.None;
        
        var result = await ExecuteHandler(sut =>
        {
            var command = new UpdateDepartmentPathCommand(departmentWithDescendant.Id, department.Id);
            return sut.Handle(command, cancellationToken);
        });
        
        Assert.True(result.IsSuccess);
    }

    private async Task<T> ExecuteHandler<T>(Func<UpdateDepartmentPathCommandHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();
        var sut = scope.ServiceProvider.GetRequiredService<UpdateDepartmentPathCommandHandler>();
        
        return await action(sut);
    }

    private async Task<Department> CreateDepartment(Department? depWith = null)
    {
        return await ExecuteInDb(async context =>
        {
            var departmentId = Guid.NewGuid();
            var locationId = await CreateLocation();
            
            var departmentLocation = DepartmentLocation.Create(
                departmentId,
                locationId);
            
            var departmentName = DepartmentName.Create("департамент");
            var departmentIdentifier = DepartmentIdentifier.Create("Identifier");
            Result<Department, Error> department;
            if (depWith is null)
            {
                 department = Department.CreateParent(
                    departmentName.Value,
                    departmentIdentifier.Value,
                    [departmentLocation],
                    departmentId);
            }
            else
            {
                 department = Department.CreateChild(
                    departmentName.Value,
                    departmentIdentifier.Value,
                    depWith,
                    [departmentLocation],
                    departmentId);
            }
            
            context.Add(department.Value);
            await context.SaveChangesAsync();
            
            return department.Value;
        });
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

    private async Task<Department> CreateDepartmentWithDescendant()
    {
        var parentDepartment = await CreateDepartment();
        return await ExecuteInDb(async context =>
        {
            var departmentId = Guid.NewGuid();
            var locationId = await CreateLocation();
            
            var departmentLocation = DepartmentLocation.Create(
                departmentId,
                locationId);
            
            var departmentName = DepartmentName.Create("департамент");
            var departmentIdentifier = DepartmentIdentifier.Create("Identifier");
            
            var department = Department.CreateChild(
                departmentName.Value,
                departmentIdentifier.Value,
                parentDepartment,
                [departmentLocation],
                departmentId);
            
            context.Add(department.Value);
            await context.SaveChangesAsync();
            
            return department.Value;
        });
    }
}