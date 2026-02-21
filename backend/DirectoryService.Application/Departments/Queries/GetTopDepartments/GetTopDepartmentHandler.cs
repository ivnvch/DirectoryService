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
        var keys = CacheConstants.Top5Departments.ToCacheKey();
        
        return await _cache.GetOrCreateAsync(
            keys,
            factory: async cancellationTokenFactory =>
            {
                using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationTokenFactory);
        
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
                    """, cancellationTokenFactory);

                return new GetTopDepartmentsDto([..department]);
            },
            tags: CacheTags.Top5Departments(),
            cancellationToken: cancellationToken);
        
        
    }
}