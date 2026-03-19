using CSharpFunctionalExtensions;

namespace Shared.Abstractions;

public interface ICommandHandler<TResponse, in TCommand>  where TCommand : ICommand
{
    Task<Result<TResponse, Shared.Errors.Errors>> Handle(TCommand command, CancellationToken cancellationToken);
}

