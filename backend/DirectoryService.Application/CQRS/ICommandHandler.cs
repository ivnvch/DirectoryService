using CSharpFunctionalExtensions;
using DirectoryService.Shared.Errors;
using MediatR;

namespace DirectoryService.Application.CQRS;

public interface ICommandHandler<TResponse, in TCommand>  where TCommand : ICommand
{
    Task<Result<TResponse, Errors>> Handle(TCommand command, CancellationToken cancellationToken);
}

