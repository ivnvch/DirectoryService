using DirectoryService.Application.CQRS;

namespace DirectoryService.Application.Departments.Queries.GetRootDepartmentsWithPreloadingChildren;

public record GetRootDepartmentsQuery(int Page, int Size, int PrefetchDepth) : IQuery;
