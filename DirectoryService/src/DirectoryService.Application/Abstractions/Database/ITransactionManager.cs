using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Abstractions.Database;

public interface ITransactionManager
{
    Task<UnitResult<Errors>> SaveChangesAsync(CancellationToken cancellationToken = default);
}