using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Extensions.Cache;
using DirectoryService.Application.Extensions.Validation;
using DirectoryService.Domain.Departments;
using DirectoryService.Shared.Departments;
using DirectoryService.Shared.Errors;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Caching.Hybrid;

namespace DirectoryService.Application.Departments.Queries.GetDescendantsDepartments;

public sealed class GetDescendantsDepartmentHandler : IListQueryHandler<GetDescendantsDepartmentDto, GetDescendantsDepartmentQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IValidator<GetDescendantsDepartmentQuery> _validator;
    private readonly HybridCache _cache;

    public GetDescendantsDepartmentHandler(IDbConnectionFactory dbConnectionFactory,
        IValidator<GetDescendantsDepartmentQuery> validator,
        HybridCache cache)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _validator = validator;
        _cache = cache;
    }

    public async Task<Result<List<GetDescendantsDepartmentDto>, Errors>> HandleList(GetDescendantsDepartmentQuery query,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError().ToErrors();
        
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        bool isExists = await connection.ExecuteScalarAsync<bool>(
            """
               SELECT EXISTS(SELECT 1 FROM departments
                WHERE id = @id)
            """, 
            new{ id = query.Id});
        
        if (!isExists)
            return Error.NotFound("parent.department.not.found", $"Department with so Id: {query.Id} does not exists").ToErrors();

        var keys = "getDescendantDepartment".ToCacheKey(id: query.Id, page: query.Pagination.Page, pageSize: query.Pagination.PageSize);

        return await _cache.GetOrCreateAsync(
            keys,
            factory: async ct => await GetDescendantsDepartments(query, connection, ct),
            tags: CacheTags.ForDepartment(query.Id),
            cancellationToken: cancellationToken);
    }

    private async Task<Result<List<GetDescendantsDepartmentDto>, Errors>> GetDescendantsDepartments(
        GetDescendantsDepartmentQuery query, IDbConnection connection, CancellationToken cancellationToken)
    {
        
        IEnumerable<GetDescendantsDepartmentDto> descendantsDepartment = await connection.QueryAsync<GetDescendantsDepartmentDto>(
            """
                 SELECT 
                    d.id,
                    d.name,
                    d.identifier,
                    d.path,
                    d.parent_id, 
                    (EXISTS(SELECT 1 FROM departments ds WHERE d.id = ds.parent_id LIMIT 1)) as has_more_children
                 FROM departments d
                 WHERE d.parent_id = @id
                 OFFSET @offset LIMIT @limit
            """,
            new
            {
                id = query.Id,
                offset = (query.Pagination.Page - 1) * query.Pagination.PageSize,
                limit = query.Pagination.PageSize
            });

        return descendantsDepartment.ToList();
    }
}