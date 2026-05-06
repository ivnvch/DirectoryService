using CSharpFunctionalExtensions;
using DirectoryService.Domain.Positions;
using Shared.CommonErrors;

namespace DirectoryService.Application.Positions.Repositories;

public interface IPositionRepository
{
    Task<Result<Guid, Error>> Add(Position position, CancellationToken cancellationToken);
    Task<UnitResult<Error>> ExistsActiveWithName(string name, CancellationToken cancellationToken);

    Task<UnitResult<Error>> GetPositionsExclusiveToDepartment(Guid departmentId,
        CancellationToken cancellationToken = default);
    
    Task<Result<Position, Error>> GetPositionById(Guid positionId, CancellationToken cancellationToken);
}