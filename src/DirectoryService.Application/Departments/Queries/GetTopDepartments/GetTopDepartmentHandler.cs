using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Extensions.Cache;
using DirectoryService.Shared.Departments;
using DirectoryService.Shared.Errors;
using Microsoft.Extensions.Caching.Hybrid;

namespace DirectoryService.Application.Departments.Queries.GetTopDepartments;

public class GetTopDepartmentHandler : IQueryHandler<GetTopDepartmentsDto>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly HybridCache _cache;

    public GetTopDepartmentHandler(IDbConnectionFactory dbConnectionFactory, HybridCache cache)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _cache = cache;
    }

    public async Task<Result<GetTopDepartmentsDto, Errors>> Handle(CancellationToken cancellationToken)
    {
        var cacheKey = "departments".ToCacheKey("topFive");

        var topDepartments = await _cache.GetOrCreateAsync(
            cacheKey,
            async ct =>
            {
                using var connection = await _dbConnectionFactory.CreateConnectionAsync(ct);
        
                var department = await connection.QueryAsync<GetTopDepartmentDto>(
                    """
                        SELECT 
                            d.name,
                            count(DISTINCT dp.position_id) as count_positions
                        FROM departments d
                        JOIN department_position dp ON d.id = dp.department_id
                        group by d.id, d.name
                        ORDER BY count_positions DESC
                        LIMIT 5
                    """, ct);

                return new GetTopDepartmentsDto([..department]);   
            },
            tags: [CacheTags.Departments],
            cancellationToken: cancellationToken);

        return topDepartments;
    }
    
}