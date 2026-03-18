using DirectoryService.Application.CQRS;

namespace DirectoryService.Application.Positions.Queries.GetPositions;

public record GetPositionQuery(
    Guid[]? DepartmentIds,
    string? Search,
    bool? IsActive,
    int Page,
    int PageSize,
    string? SortBy,
    string? SortDirection) : IQuery;