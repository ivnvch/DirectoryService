using DirectoryService.Domain.Locations;

namespace DirectoryService.Application.Locations.Repositories;

public interface ILocationRepository
{
    Task<Guid> CreateLocation(Location location, CancellationToken cancellation);
}