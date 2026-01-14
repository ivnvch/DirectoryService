using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Locations.Repositories;

public interface ILocationRepository
{
    Task<Result<Guid, Error>> Add(Location location, CancellationToken cancellation = default);
}