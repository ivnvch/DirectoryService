using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Locations.Repositories;

public interface ILocationRepository
{
    Task<Result<Guid, Error>> Add(Location location, CancellationToken cancellation = default);

    Task<Result<bool, Errors>> AllExistAsync(IReadOnlyCollection<Guid> locationIds,
        CancellationToken cancellation = default);

    Task<UnitResult<Error>> GetLocationsExclusiveToDepartment(Guid departmentId,
        CancellationToken cancellationToken = default);
}