using CSharpFunctionalExtensions;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Departments.Repositories;

public interface IDepartmentRepository
{
    Task<Result<Guid, Error>> Add(Department department,  CancellationToken cancellationToken = default);
    Task<Result<Department, Error>> GetById(Guid id, CancellationToken cancellationToken = default);
}