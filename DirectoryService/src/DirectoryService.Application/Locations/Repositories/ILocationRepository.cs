using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;

namespace DirectoryService.Application.Locations.Repositories;

public interface ILocationRepository
{
    Task<Result<Guid>> AddAsync(Location location, CancellationToken cancellation = default);
}