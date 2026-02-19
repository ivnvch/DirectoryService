using DirectoryService.Application.CQRS;
using DirectoryService.Shared;

namespace DirectoryService.Application.Departments.Queries.GetDescendantsDepartments;

public record GetDescendantsDepartmentQuery(
    Guid Id,
    PaginationRequest Pagination) : IQuery;