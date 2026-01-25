using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using MediatR;

namespace DirectoryService.Application.CQRS;

public interface ICommandHandler<TResponse, in TCommand>  where TCommand : ICommand/*IRequestHandler<Result<TResponse, Error>, TCommand>
    where TCommand : ICommand<TResponse>*/
{
    Task<Result<TResponse, Errors>> Handle(TCommand command, CancellationToken token);
}

/*public interface ICommandHandler<in TCommand>  where TCommand : ICommand
{
    Task<UnitResult<Errors>> Handle(TCommand command, CancellationToken token);
}*/
