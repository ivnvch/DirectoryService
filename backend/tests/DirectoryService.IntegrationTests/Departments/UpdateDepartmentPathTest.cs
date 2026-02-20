using DirectoryService.Application.Departments.Commands.UpdateDepartmentPath;
using DirectoryService.Domain.Departments;
using DirectoryService.IntegrationTests.Infrastructure;
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
        var department = await CreateDepartmentAsync();
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
        var department = await CreateChildDepartmentAsync(departmentWithDescendant);
        
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
        var department = await CreateDepartmentAsync();
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

    private async Task<Department> CreateDepartmentWithDescendant()
    {
        var parentDepartment = await CreateDepartmentAsync();
        return await CreateChildDepartmentAsync(parentDepartment);
    }
}