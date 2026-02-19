using System.Data.Common;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.Extensions.Cache;
using DirectoryService.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.Logging;

namespace DirectoryService.Infrastructure.BackgroundServices;

public sealed class DeleteDepartmentService
{
    private readonly DirectoryDbContext _dbContext;
    private readonly ITransactionManager _transactionManager;
    private readonly HybridCache _cache;
    private readonly ILogger<DeleteDepartmentService> _logger;
    
    public DeleteDepartmentService(
        DirectoryDbContext dbContext, 
        ITransactionManager transactionManager,
        HybridCache cache,
        ILogger<DeleteDepartmentService> logger)
    {
        _dbContext = dbContext;
        _transactionManager = transactionManager;
        _cache = cache;
        _logger = logger;
    }

    public async Task<UnitResult<Errors>> DeleteDepartment(DateTime period, CancellationToken cancellationToken = default)
    {
        DbConnection dbConnection = _dbContext.Database.GetDbConnection();

        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return UnitResult.Failure(transactionScopeResult.Error.ToErrors());

        using var transactionScope = transactionScopeResult.Value;

        try
        {
            await dbConnection.ExecuteAsync(
                """
                WITH dept_ids AS (
                    SELECT d.id
                    FROM departments d
                    WHERE d.is_active = false
                      AND d.deleted_at IS NOT NULL
                      AND d.deleted_at <= @period
                ),
                targets AS (
                    SELECT d.id, d.parent_id, d.path, d.depth,
                           p.path AS parent_path, p.depth AS parent_depth
                    FROM departments d
                    LEFT JOIN departments p ON p.id = d.parent_id
                    WHERE d.id IN (SELECT id FROM dept_ids)
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
                ),
                update_parent AS (
                    UPDATE departments d
                    SET parent_id = t.parent_id,
                        updated_at = now()
                    FROM targets t
                    WHERE d.parent_id = t.id
                ),
                delete_locations AS (
                    DELETE FROM department_locations
                    WHERE department_id IN (SELECT id FROM dept_ids)
                ),
                delete_positions AS (
                    DELETE FROM department_position
                    WHERE department_id IN (SELECT id FROM dept_ids)
                )
                DELETE FROM departments
                WHERE id IN (SELECT id FROM dept_ids)
                """, new { period });
        }
        catch (Exception ex)
        {
            transactionScope.Rollback();
            _logger.LogError(ex, "Failed to delete departments");
            return UnitResult.Failure(Error.Failure("failed.delete.departments", ex.Message).ToErrors());
        }

        var committedResult = transactionScope.Commit();
        if (committedResult.IsFailure)
        {
            _logger.LogError("Failed to commit transaction during delete departments");
            return UnitResult.Failure<Errors>(committedResult.Error);
        }

        await _cache.RemoveByTagAsync(CacheTags.Departments, cancellationToken);

        _logger.LogInformation("Departments deleted successfully");

        return UnitResult.Success<Errors>();
    }
}
