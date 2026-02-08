using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.CQRS;
using DirectoryService.Shared;
using DirectoryService.Shared.Departments;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Departments.Queries.GetRootDepartmentsWithPreloadingChildren;

public sealed class GetRootDepartmentsHandler : IQueryHandler<PaginationResponse<GetRootDepartmentDto>, GetRootDepartmentsQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetRootDepartmentsHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<PaginationResponse<GetRootDepartmentDto>, Errors>> Handle(
        GetRootDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        int limit = query.Request.Pagination.PageSize;
        int offset = query.Request.Pagination.Page;
        int? totalCount = null;
        IEnumerable<GetRootDepartmentDto> departmentsWithPreloadingChildren = 
            (await connection.QueryAsync<GetRootDepartmentDto, int, GetRootDepartmentDto>(
                """
                    WITH roots AS
                    (
                        SELECT 
                            d.id,
                            d.name,
                            d.identifier,
                            d.path,
                            d.parent_id,
                            COUNT(*) OVER()::int AS totalCount
                        FROM departments d 
                        WHERE d.parent_id IS NULL
                        ORDER BY d.created_at
                        OFFSET @rootOffset LIMIT @rootLimit
                    )
                    SELECT r.*,
                    (EXISTS(SELECT 1 FROM departments ds WHERE r.id = ds.parent_id OFFSET @prefetch LIMIT 1)) as has_more_children
                    FROM roots r
                    
                    UNION ALL
                    
                    SELECT
                         ch.*,
                         0,
                         (EXISTS(SELECT 1 FROM departments ds WHERE ch.id = ds.parent_id)) as has_more_children
                    FROM roots r
                    CROSS JOIN LATERAL
                    (
                        SELECT
                           sd.id,
                           sd.name,
                           sd.identifier,
                           sd.path,
                           sd.parent_id
                        FROM departments sd 
                        WHERE sd.parent_id = r.id
                        LIMIT @prefetch
                    ) ch
                """,
                param: new
                {
                    rootLimit = limit,
                    rootOffset = (offset - 1) *  limit,
                    prefetch = query.Request.Prefetch
                },
                splitOn: "totalCount",
                map: (department, total) =>
                {
                    totalCount ??= total;
                    return department;
                })).ToList();
        
        ILookup<Guid?, GetRootDepartmentDto> childrenByParent = departmentsWithPreloadingChildren.ToLookup(d => d.ParentId);

        List<GetRootDepartmentDto> roots = childrenByParent[null]
            .Select(root => root with { Children = childrenByParent[root.Id].ToList() })
            .ToList();
        
        int totalPages = (totalCount.Value + limit - 1) / limit;

        return new PaginationResponse<GetRootDepartmentDto>(
            [..roots], 
            totalCount.Value,
            offset,
            limit,
            totalPages);
    }
}