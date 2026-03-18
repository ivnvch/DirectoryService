namespace DirectoryService.API.Models.RequestModels.Positions;

public record GetPositionsRequest(
    Guid[]? DepartmentIds,
    string? Search,
    bool? IsActive,
    int Page,
    int PageSize,
    string? SortBy,
    string? SortDirection);