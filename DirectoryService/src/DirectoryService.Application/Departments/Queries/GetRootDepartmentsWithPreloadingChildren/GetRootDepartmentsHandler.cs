using System.Data;
using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.CQRS;
using DirectoryService.Shared.Departments;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Departments.Queries.GetRootDepartmentsWithPreloadingChildren;

public sealed class GetRootDepartmentsHandler : IListQueryHandler<GetRootDepartmentsDto, GetRootDepartmentsQuery>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetRootDepartmentsHandler(IDbConnectionFactory dbConnectionFactory)
    {
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<List<GetRootDepartmentsDto>, Errors>> HandleList(
        GetRootDepartmentsQuery query,
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);

        int limit = query.Pagination.PageSize ?? 20;
        int offset = query.Pagination.Page ?? 1;
        
        IEnumerable<GetRootDepartmentsDto> departmentsWithPreloadingChildren = 
            await connection.QueryAsync<GetRootDepartmentsDto>(
                """
                    WITH roots AS
                    (
                        SELECT 
                            d.id,
                            d.name,
                            d.identifier,
                            d.path,
                            d.parent_id,
                            (EXISTS(SELECT 1 FROM departments ds WHERE d.id = ds.parent_id OFFSET @prefetch LIMIT 1)) as has_more_children
                        FROM departments d 
                        WHERE d.parent_id IS NULL
                        ORDER BY d.created_at
                        OFFSET @rootOffset LIMIT @rootLimit
                    )
                    SELECT * 
                    FROM roots 
                    
                    UNION ALL
                    
                    SELECT ch.*, (EXISTS(SELECT 1 FROM departments ds WHERE ch.id = ds.parent_id)) as has_more_children
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
                    ) ch;
                """,
                new
                {
                    rootLimit = limit,
                    rootOffset = (offset - 1) *  limit,
                    prefetch = query.PrefetchDepth
                });

        ILookup<Guid?, GetRootDepartmentsDto> childrenByParent = departmentsWithPreloadingChildren.ToLookup(d => d.ParentId);

        List<GetRootDepartmentsDto> roots = childrenByParent[null]
            .Select(root => root with { Children = childrenByParent[root.Id].ToList() })
            .ToList();

        return roots;

    }
}