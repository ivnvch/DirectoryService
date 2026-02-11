using System.Data.Common;
using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using DirectoryService.Application.Departments.Repositories;
using DirectoryService.Domain.Departments;
using DirectoryService.Domain.Departments.Errors;
using DirectoryService.Domain.Departments.ValueObject;
using DirectoryService.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Dapper;

namespace DirectoryService.Infrastructure.Departments.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly DirectoryDbContext _context;
    private readonly ILogger<DepartmentRepository> _logger;

    public DepartmentRepository(DirectoryDbContext context, ILogger<DepartmentRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result<Guid, Error>> Add(Department department, CancellationToken cancellationToken = default)
    {
        try
        {
            _context.Departments.Add(department);
            _logger.LogInformation($"Department: {department.Name} with Department ID: {department.Id} has been added");

            return department.Id;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex, "Operation cancelled while creating department: {department}", department);

            return DepartmentError.OperationCancelled();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while creating department: {department}", department);

            return DepartmentError.DatabaseError();
        }
    }

    public async Task<Result<Department, Error>> GetByAsync(Expression<Func<Department, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        var department = await _context.Departments
            .Where(x => x.IsActive)
            .FirstOrDefaultAsync(predicate, cancellationToken);

        if (department is null)
            return GeneralErrors.NotFound(null, "department");

        return department;
    }

    public async Task<Result<Department, Error>> GetByIdWithLockAsync(Guid id,
        CancellationToken cancellationToken = default)
    {
        var department = await _context.Departments
            .FromSql($"SELECT * FROM departments WHERE id = {id} FOR UPDATE")
            .Where(x => x.IsActive)
            .FirstOrDefaultAsync(cancellationToken);

        if (department is null)
            return GeneralErrors.NotFound(id);

        return department;
    }

    public async Task<Result<bool, Errors>> AllDepartmentsExistAsync(IReadOnlyCollection<Guid> departmentIds,
        CancellationToken cancellationToken = default)
    {
        if (departmentIds.Count == 0)
            return false;

        var existingCount = await _context.Departments
            .Where(d => departmentIds.Contains(d.Id))
            .CountAsync(cancellationToken);

        return existingCount == departmentIds.Count;
    }

    public async Task<UnitResult<Error>> ExistDepartmentAsync(Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        bool result = await _context.Departments
            .AnyAsync(x => x.Id == departmentId && x.IsActive, cancellationToken: cancellationToken);

        return result
            ? UnitResult.Success<Error>()
            : GeneralErrors.NotFound(departmentId);
    }

    public async Task<UnitResult<Error>> DeleteLocationsAsync(Guid departmentId,
        CancellationToken cancellationToken = default)
    {
        await _context.DepartmentLocations
            .Where(d => d.DepartmentId == departmentId)
            .ExecuteDeleteAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }

    public async Task<UnitResult<Error>> LockDescendants(DepartmentPath path,
        CancellationToken cancellationToken = default)
    {
        await _context.Database.ExecuteSqlInterpolatedAsync(
            $"""
             SELECT * FROM departments 
             WHERE path <@{path.Value}::ltree AND path != {path.Value}::ltree
             FOR UPDATE
             """, cancellationToken);

        return UnitResult.Success<Error>();
    }

    public async Task<bool> IsDescendantsAsync(Guid oldDepartmentId, Guid newDepartmentId,
        CancellationToken cancellationToken = default)
    {
        var dbConnection = _context.Database.GetDbConnection();

        var isDescendant = await dbConnection.ExecuteScalarAsync<bool>(
            """
            SELECT EXISTS (
                SELECT 1 
                FROM departments nd
                JOIN departments od ON od.id = @oldDepartmentId
                WHERE nd.id = @newDepartmentId
                AND nd.path <@ od.path
            ) 
            """, new
            {
                oldDepartmentId,
                newDepartmentId
            });

        return isDescendant;
    }

    public async Task<UnitResult<Errors>> UpdateDepartmentsHierarchyAsync(Department department, DepartmentPath oldPath,
        CancellationToken cancellationToken = default)
    {
        DbConnection dbConnection = _context.Database.GetDbConnection();

        var updatePath = await dbConnection.ExecuteAsync(
            """
                  UPDATE departments
                  SET path = (@departmentPath::ltree || subpath(path, nlevel(@oldPath::ltree))),
                  depth = @departmentDepth + (depth - nlevel(@oldPath::ltree) + 1),
                  updated_at = now()
                  WHERE path <@ @oldPath::ltree
                    AND path != @oldPath::ltree
            """, new
            {
                departmentPath = department.DepartmentPath.Value,
                departmentDepth = department.Depth,
                oldPath = oldPath.Value
            });

        return UnitResult.Success<Errors>();
    }

    public async Task<UnitResult<Error>> UpdatePathAfterSoftDeleted(
        DepartmentPath oldPath,
        DepartmentPath newPath,
        CancellationToken cancellationToken = default)
    {
        DbConnection dbConnection = _context.Database.GetDbConnection();

        var updatePath = await dbConnection.ExecuteAsync(
            """
                    UPDATE departments
                    SET path = CASE
                        WHEN path = @oldPath::ltree THEN @newPath::ltree
                        ELSE (@newPath::ltree || subpath(path, nlevel(@oldPath::ltree)))
                    END,
                    updated_at = now()
                    WHERE path <@ @oldPath::ltree
                """,
            new
            {
                oldPath = oldPath.Value,
                newPath = newPath.Value
            });

        return UnitResult.Success<Error>();
    }
}