namespace DirectoryService.Shared.Departments;

public record GetRootDeparmentsRequest(PaginationRequest Pagination, int? Prefetch = 3);