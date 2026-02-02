using CSharpFunctionalExtensions;
using DirectoryService.Domain.DepartmentLocations;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.ValueObject;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Departments.Repositories;

public interface IDepartmentRepository
{
    Task<Result<Guid, Error>> Add(Department department,  CancellationToken cancellationToken = default);
    Task<Result<Department, Error>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Result<Department, Error>> GetByIdWithLockAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<bool, Errors>> AllDepartmentsExistAsync(IReadOnlyCollection<Guid> departmentIds,
        CancellationToken cancellationToken = default);
    
    Task<UnitResult<Error>> ExistDepartmentAsync(Guid departmentId, CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> DeleteLocationsAsync(Guid departmentId, CancellationToken cancellationToken = default);
    
    Task<UnitResult<Error>> LockDescendants(DepartmentPath path, CancellationToken cancellationToken = default);
    Task<bool> IsDescendantsAsync(Guid oldDepartmentId, Guid newDepartmentId, CancellationToken cancellationToken = default);
    Task<UnitResult<Errors>> UpdateDepartmentsHierarchyAsync(Department department, DepartmentPath oldPath, CancellationToken cancellationToken = default);
}