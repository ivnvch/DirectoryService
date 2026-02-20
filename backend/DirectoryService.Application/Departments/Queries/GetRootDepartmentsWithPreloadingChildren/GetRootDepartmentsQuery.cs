using DirectoryService.Application.CQRS;
using DirectoryService.Shared;
using DirectoryService.Shared.Departments;

namespace DirectoryService.Application.Departments.Queries.GetRootDepartmentsWithPreloadingChildren;

public record GetRootDepartmentsQuery(GetRootDeparmentsRequest Request) : IQuery;