using System.Data.Common;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Departments.Repositories;
using DirectoryService.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.BackgroundServices;

public sealed class DeleteDepartmentService
{
    private readonly DirectoryDbContext _dbContext;
    private readonly ITransactionManager _transactionManager;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly ILogger<DeleteDepartmentService> _logger;
    
    public DeleteDepartmentService(
        DirectoryDbContext dbContext, 
        ITransactionManager transactionManager, 
        IDepartmentRepository departmentRepository, 
        ILogger<DeleteDepartmentService> logger)
    {
        _dbContext = dbContext;
        _transactionManager = transactionManager;
        _departmentRepository = departmentRepository;
        _logger = logger;
    }

    public async Task<UnitResult<Errors>> DeleteDepartment(DateTime period, CancellationToken cancellationToken = default)
    {
        var departmentIds = await _departmentRepository.GetIdsForDeleteAsync(period, cancellationToken);
        if (departmentIds.Count == 0)
        {
            _logger.LogInformation("No departments found for delete");
            return UnitResult.Success<Errors>();
        }
        
        var transactionScopeResult =  await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
        {
            return UnitResult.Failure(transactionScopeResult.Error.ToErrors());
        }

        using var transactionScope = transactionScopeResult.Value;

        DbConnection dbConnection = _dbContext.Database.GetDbConnection();

        try
        {
            var updatePathInHierarchyBeforeDelete = await dbConnection.ExecuteAsync(
                """
                    WITH targets AS (
                    SELECT d.id, d.parent_id, d.path, d.depth,
                           p.path AS parent_path, p.depth AS parent_depth
                    FROM departments d
                    LEFT JOIN departments p ON p.id = d.parent_id
                    WHERE d.id = ANY(@descendant_ids)
                ),
                picked AS (
                    SELECT DISTINCT ON (ch.id)
                        ch.id AS child_id,
                        t.path AS target_path,
                        t.depth AS target_depth,
                        t.parent_id,
                        t.parent_path,
                        t.parent_depth
                    FROM departments ch
                    JOIN targets t
                      ON ch.path <@ t.path
                     AND ch.id != t.id
                    ORDER BY ch.id, nlevel(t.path) DESC
                ),
                update_subtree AS (
                    UPDATE departments d
                    SET path = CASE
                            WHEN p.parent_id IS NULL THEN subpath(d.path, 1)
                            ELSE p.parent_path::ltree || subpath(d.path, nlevel(p.target_path))
                        END,
                        depth = CASE
                            WHEN p.parent_id IS NULL THEN d.depth - 1
                            ELSE p.parent_depth + (d.depth - p.target_depth)
                        END,
                        updated_at = now()
                    FROM picked p
                    WHERE d.id = p.child_id
                    RETURNING d.id
                )
                UPDATE departments d
                SET parent_id = t.parent_id,
                    updated_at = now()
                FROM targets t
                WHERE d.parent_id = t.id; 
                """, new
                {
                    descendant_ids = departmentIds
                });
        }
        catch (Exception ex)
        {
            transactionScope.Rollback();
                
            _logger.LogError(
                "Failed to update hierarchy before delete, departmentIds: {@DepartmentIds}",
                departmentIds);

            return UnitResult.Failure(Error.Failure("failed.update.hierarchy.before.delete", ex.Message).ToErrors());
        }

       var deleteDepartments = await dbConnection.ExecuteAsync(
           """
           DELETE FROM department_locations
           WHERE department_id = ANY(@departmentIds);

           DELETE FROM department_position
           WHERE department_id = ANY(@departmentIds);

           DELETE FROM departments
           WHERE id = ANY(@departmentIds);
           """,
           new { departmentIds });

       if (deleteDepartments == 0)
       {
           transactionScope.Rollback();
           _logger.LogError("Failed to delete departments, departmentIds: {@DepartmentIds}",
               departmentIds);
       }
       
       var saveChanges = await _transactionManager.SaveChangesAsync(cancellationToken);
       if (saveChanges.IsFailure)
       {
           transactionScope.Rollback();
           _logger.LogError("Failed to save changes during delete, departmentIds: {@DepartmentIds}",
               departmentIds);
       }

       var committedResult = transactionScope.Commit();
       if (committedResult.IsFailure)
       {
           _logger.LogError("Failed to commit transaction during delete, departmentIds: {@DepartmentIds}",
               departmentIds);
       }

       _logger.LogInformation("Department deleted successfully, departmentId: {@DepartmentIds}",
           departmentIds);
       
       _logger.LogInformation("Delete completed, processed departments count: {Count}", departmentIds.Count);
       
       return UnitResult.Success<Errors>();
    }
}