using CSharpFunctionalExtensions;
using Shared.Errors;

namespace Shared.Database;

public interface ITransactionScope : IDisposable
{
    UnitResult<Error> Commit();
    UnitResult<Error> Rollback();
}