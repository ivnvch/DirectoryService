using DirectoryService.Application.CQRS;
using DirectoryService.Shared;

namespace DirectoryService.Application.Departments.Queries.GetRootDepartmentsWithPreloadingChildren;

public record GetRootDepartmentsQuery(
    PaginationRequest Pagination,
    int? PrefetchDepth) : IQuery;