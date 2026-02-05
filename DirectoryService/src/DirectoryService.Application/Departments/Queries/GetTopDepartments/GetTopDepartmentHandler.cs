using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.CQRS;
using DirectoryService.Shared.Departments;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Departments.Queries.GetTopDepartments;

public class GetTopDepartmentHandler : IQueryHandler<GetTopDepartmentsDto>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public GetTopDepartmentHandler(IReadDbContext readDbContext, IDbConnectionFactory dbConnectionFactory)
    {
        _readDbContext = readDbContext;
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<Result<GetTopDepartmentsDto, Errors>> Handle(CancellationToken cancellationToken)
    { 
        using var connection = await _dbConnectionFactory.CreateConnectionAsync(cancellationToken);
        
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
                """, cancellationToken);

        return new GetTopDepartmentsDto([..department]);
    }
}