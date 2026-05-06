using Shared.Abstractions;

namespace DirectoryService.Application.Locations.Queries.GetLocations;

public record GetLocationsQuery(
    Guid[]? DepartmentIds,
    string? Search,
    bool? IsActive,
    int Page,
    int PageSize,
    string? SortBy,
    string? SortDirection) : IQuery;