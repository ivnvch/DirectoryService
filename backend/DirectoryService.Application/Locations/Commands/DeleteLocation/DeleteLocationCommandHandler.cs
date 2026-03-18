using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Locations.Repositories;
using DirectoryService.Shared.Errors;

namespace DirectoryService.Application.Locations.Commands.DeleteLocation;

public class DeleteLocationCommandHandler : ICommandHandler<Guid, DeleteLocationCommand>
{
    private readonly ILocationRepository _locationRepository;
    private readonly ITransactionManager _transactionManager;

    public DeleteLocationCommandHandler(
        ILocationRepository locationRepository,
        ITransactionManager transactionManager)
    {
        _locationRepository = locationRepository;
        _transactionManager = transactionManager;
    }

    public async Task<Result<Guid, Errors>> Handle(DeleteLocationCommand command, CancellationToken cancellationToken)
    {
        var transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();

        using var transactionScope = transactionScopeResult.Value;

        var locationResult = await _locationRepository.GetByIdAsync(command.LocationId, cancellationToken);
        if (locationResult.IsFailure)
        {
            transactionScope.Rollback();
            return locationResult.Error.ToErrors();
        }

        locationResult.Value.SoftDelete();

        var saveResult = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveResult.IsFailure)
        {
            transactionScope.Rollback();
            return saveResult.Error.ToErrors();
        }

        var commitResult = transactionScope.Commit();
        if (commitResult.IsFailure)
            return commitResult.Error.ToErrors();

        return command.LocationId;
    }
}
