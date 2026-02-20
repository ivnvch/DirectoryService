using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Commands.SoftDeleteDepartment;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments.ValueObject;
using DirectoryService.IntegrationTests.Infrastructure;
using DirectoryService.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.IntegrationTests.Departments;

public class SoftDeleteDepartmentTest : DirectoryBaseTests
{
    public SoftDeleteDepartmentTest(DirectoryTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Handle_WhenRootDepartmentHasChildAndExclusivePosition_ShouldSoftDeleteRelatedEntities()
    {
        var department = await CreateDepartmentAsync();
        var childDepartment = await CreateChildDepartmentAsync(department);
        var position = await CreatePositionAsync([department.Id]);

        var result = await ExecuteSoftDeleteAsync(department.Id);

        await ExecuteInDb(async context =>
        {
            var expectedDeletedPath = DepartmentPath
                .Create(department.DepartmentPath.Value)
                .MarkAsDeleted();
            var expectedDeletedChildPath = DepartmentPath.Create(
                $"{expectedDeletedPath.Value}.{childDepartment.DepartmentIdentifier.Value}");

            var deletedDepartment =
                await context.Departments
                    .SingleOrDefaultAsync(x => x.Id == department.Id, CancellationToken.None);
            var deletedChildDepartment = await context.Departments
                .SingleOrDefaultAsync(x => x.Id == childDepartment.Id, CancellationToken.None);
            var deletedPosition = await context.Positions
                .SingleOrDefaultAsync(x => x.Id == position.Id, CancellationToken.None);
            
            Assert.True(result.IsSuccess);
            Assert.Equal(department.Id, result.Value);

            Assert.NotNull(deletedDepartment);
            Assert.Equal(department.Id, deletedDepartment.Id);
            Assert.Equal(expectedDeletedPath, deletedDepartment.DepartmentPath);
            Assert.False(deletedDepartment.IsActive);
            Assert.NotNull(deletedDepartment.DeletedAt);

            Assert.NotNull(deletedChildDepartment);
            Assert.Equal(expectedDeletedChildPath, deletedChildDepartment!.DepartmentPath);

            Assert.NotNull(deletedPosition);
            Assert.False(deletedPosition!.IsActive);
            Assert.NotNull(deletedPosition.DeletedAt);
        });
    }
    
    [Fact]
    public async Task Handle_WhenPositionBelongsOnlyToDeletedDepartment_ShouldSoftDeletePosition()
    {
        var department = await CreateDepartmentAsync(locationsCount: 3);

        var position = await CreatePositionAsync([department.Id]);

        var result = await ExecuteSoftDeleteAsync(department.Id);

        await ExecuteInDb(async context =>
        {
            var deletedPosition =
                await context.Positions
                    .SingleOrDefaultAsync(x => x.Id == position.Id, CancellationToken.None);
            
            Assert.True(result.IsSuccess);
            Assert.Equal(department.Id, result.Value);

            Assert.NotNull(deletedPosition);
            Assert.False(deletedPosition!.IsActive);
            Assert.NotNull(deletedPosition.DeletedAt);
        });
    }
    
    [Fact]
    public async Task Handle_WhenPositionIsSharedWithAnotherDepartment_ShouldKeepPositionActive()
    {
        var department = await CreateDepartmentAsync();
        var department2 = await CreateDepartmentAsync();

        var position = await CreatePositionAsync([department.Id, department2.Id]);

        var result = await ExecuteSoftDeleteAsync(department.Id);

        await ExecuteInDb(async context =>
        {
            var existingPosition =
                await context.Positions
                    .SingleOrDefaultAsync(x => x.Id == position.Id, CancellationToken.None);
            
            Assert.True(result.IsSuccess);
            Assert.Equal(department.Id, result.Value);

            Assert.NotNull(existingPosition);
            Assert.True(existingPosition!.IsActive);
            Assert.Null(existingPosition.DeletedAt);
        });
    }
    
    [Fact]
    public async Task Handle_WhenLocationIsSharedWithAnotherDepartment_ShouldKeepLocationActive()
    {
        var department1 = await CreateDepartmentAsync(locationsCount: 1);
        var sharedLocationId = department1.Locations.Single().LocationId;

        var department2 = await CreateDepartmentAsync(locationsCount: 1);

        await ExecuteInDb(async context =>
        {
            context.DepartmentLocations.Add(
                DepartmentLocation.Create(department2.Id, sharedLocationId));

            await context.SaveChangesAsync(CancellationToken.None);
        });

        var result = await ExecuteSoftDeleteAsync(department1.Id);

        await ExecuteInDb(async context =>
        {
            var existingLocation = await context.Locations
                .SingleOrDefaultAsync(x => x.Id == sharedLocationId, CancellationToken.None);

            Assert.True(result.IsSuccess);
            Assert.Equal(department1.Id, result.Value);

            Assert.NotNull(existingLocation);
            Assert.True(existingLocation!.IsActive);
            Assert.Null(existingLocation.DeletedAt);
        });
    }
    
    private async Task<T> ExecuteHandler<T>(Func<SoftDeleteDepartmentHandler, Task<T>> action)
    {
        await using var scope = Services.CreateAsyncScope();

        var sut = scope.ServiceProvider.GetRequiredService<SoftDeleteDepartmentHandler>();

        return await action(sut);
    }

    private Task<Result<Guid, Errors>> ExecuteSoftDeleteAsync(Guid departmentId)
    {
        return ExecuteHandler(sut => sut.Handle(
            new SoftDeleteDepartmentCommand(departmentId),
            CancellationToken.None));
    }
}