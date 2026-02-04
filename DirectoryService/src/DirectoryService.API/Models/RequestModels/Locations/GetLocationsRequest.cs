namespace DirectoryService.API.Models.RequestModels;

public record GetLocationsRequest(
    Guid[]? DepartmentIds,
    string? Search,
    bool? IsActive,
    string? SortBy,
    string? SortDirection,
    int Page = 1,
    int PageSize = 20);
