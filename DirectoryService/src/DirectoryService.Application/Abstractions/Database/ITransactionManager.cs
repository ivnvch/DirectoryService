using CSharpFunctionalExtensions;
using DirectoryService.Application.Database;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Abstractions.Database;

public interface ITransactionManager
{
    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(CancellationToken cancellationToken);
    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);
}