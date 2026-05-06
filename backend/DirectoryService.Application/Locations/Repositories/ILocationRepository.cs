using CSharpFunctionalExtensions;
using DirectoryService.Domain.Locations;
using Shared.CommonErrors;

namespace DirectoryService.Application.Locations.Repositories;

public interface ILocationRepository
{
    Task<Result<Guid, Error>> Add(Location location, CancellationToken cancellation = default);

    Task<Result<bool, Errors>> AllExistAsync(IReadOnlyCollection<Guid> locationIds,
        CancellationToken cancellation = default);

    Task<UnitResult<Error>> GetLocationsExclusiveToDepartment(Guid departmentId,
        CancellationToken cancellationToken = default);

    Task<Result<Location, Error>> GetByIdAsync(Guid locationId, CancellationToken cancellationToken = default);

    Task<UnitResult<Error>> UpdateAsync(Location location, CancellationToken cancellationToken = default);
}