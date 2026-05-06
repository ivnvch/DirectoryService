using CSharpFunctionalExtensions;
using Dapper;
using DirectoryService.Application.Abstractions.Database;
using Shared.Abstractions;
using DirectoryService.Shared.Departments;
using Shared.CommonErrors;
using DirectoryService.Shared.Positions;

namespace DirectoryService.Application.Positions.Queries.GetPositionDetails;

public class GetPositionDetailsHandler : IQueryHandler<GetPositionDetailsDto, GetPositionDetailsQuery>
{
    private readonly IDbConnectionFactory _connectionFactory;

    public GetPositionDetailsHandler(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Result<GetPositionDetailsDto, Errors>> Handle(GetPositionDetailsQuery query, CancellationToken cancellationToken)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        List<GetShortDepartmentDto> departments = [];
        var result = await connection.QueryAsync<GetPositionDto, (int SplitMarker, string? Name, string? Identifier, string? Path), GetPositionDto>(
            """
            SELECT p."Id", p.name, p.description, p.is_active, p.created_at, p.updated_at,
                   0 AS "SplitMarker",
                   d.name, d.identifier, d.path
            FROM positions p
            LEFT JOIN department_position dp ON p."Id" = dp.position_id
            LEFT JOIN departments d ON dp.department_id = d.id
            WHERE p."Id" = @id AND p.deleted_at IS NULL
            """,
            param: new { id = query.Id },
            splitOn: "SplitMarker",
            map: (p, dept) =>
            {
                if (dept.Name is not null)
                    departments.Add(new GetShortDepartmentDto(dept.Name, dept.Identifier!, dept.Path!));
                return p;
            });

        GetPositionDto? positionDto = result.FirstOrDefault();
        if (positionDto is null)
            return Error.NotFound("position.not.found", $"Позиция с id {query.Id} не найдена.").ToErrors();

        return new GetPositionDetailsDto(positionDto, departments);
    }
}