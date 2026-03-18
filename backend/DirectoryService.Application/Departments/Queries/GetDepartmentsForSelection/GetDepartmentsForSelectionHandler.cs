using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Extensions.Cache;
using DirectoryService.Shared;
using DirectoryService.Shared.Departments;
using DirectoryService.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;

namespace DirectoryService.Application.Departments.Queries.GetDepartmentsForSelection;

public class GetDepartmentsForSelectionHandler : IQueryHandler<List<GetDepartmentForSelectionDto>, GetDepartmentsForSelectionQuery>
{
    private readonly IReadDbContext _readDbContext;
    private readonly HybridCache _cache;

    public GetDepartmentsForSelectionHandler(IReadDbContext readDbContext, HybridCache cache)
    {
        _readDbContext = readDbContext;
        _cache = cache;
    }

    public async Task<Result<List<GetDepartmentForSelectionDto>, Errors>> Handle(
        GetDepartmentsForSelectionQuery query,
        CancellationToken cancellationToken)
    {
        var keys = CacheConstants.SelectedDepartmentForPositions.ToCacheKey();

        return await _cache.GetOrCreateAsync(
            keys,
            factory: async token =>
            {
                var departments = await _readDbContext.DepartmentsRead
                    .Where(d => d.IsActive && d.DeletedAt == null)
                    .OrderBy(d => d.DepartmentPath)
                    .Select(d => new GetDepartmentForSelectionDto(
                        d.Id,
                        d.Name.Value,
                        d.DepartmentPath.Value,
                        d.DepartmentIdentifier.Value))
                    .ToListAsync(token);

                return departments;
            }, 
            tags: CacheTags.SelectedDepartmentForPositions(),
            cancellationToken: cancellationToken);
    }
}
