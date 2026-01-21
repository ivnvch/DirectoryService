using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Positions.Repositories;

public interface IPositionRepository
{
    Task<Result<Guid, Error>> Add(Position position, CancellationToken cancellationToken);
    Task<UnitResult<Error>> ExistsActiveWithName(string name, CancellationToken cancellationToken);
}