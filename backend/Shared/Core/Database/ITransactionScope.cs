using CSharpFunctionalExtensions;
using Shared.CommonErrors;

namespace Shared.Database;

public interface ITransactionScope : IDisposable
{
    UnitResult<Error> Commit();
    UnitResult<Error> Rollback();
}