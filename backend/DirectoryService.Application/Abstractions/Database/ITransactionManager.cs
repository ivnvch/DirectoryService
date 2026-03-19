using System.Data;
using CSharpFunctionalExtensions;
using Shared.Errors;

namespace DirectoryService.Application.Abstractions.Database;

public interface ITransactionManager
{
    Task<Result<ITransactionScope, Error>> BeginTransactionAsync(
        CancellationToken cancellationToken = default, IsolationLevel? level = null);
    Task<UnitResult<Error>> SaveChangesAsync(CancellationToken cancellationToken);
}