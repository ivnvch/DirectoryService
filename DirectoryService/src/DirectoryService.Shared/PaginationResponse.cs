namespace DirectoryService.Shared;

public record PaginationResponse<T>(
    IReadOnlyList<T> Results,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages);