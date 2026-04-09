using CSharpFunctionalExtensions;
using Shared.CommonErrors;

namespace Shared.Abstractions;

public interface ICommandHandler<TResponse, in TCommand>  where TCommand : ICommand
{
    Task<Result<TResponse, Errors>> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    Task<UnitResult<Error>> Handle(TCommand command, CancellationToken cancellationToken);
}



