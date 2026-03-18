using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions.Database;
using DirectoryService.Application.CQRS;
using DirectoryService.Application.Extensions.Cache;
using DirectoryService.Application.Positions.Repositories;
using DirectoryService.Shared.Errors;
using Microsoft.Extensions.Caching.Hybrid;

namespace DirectoryService.Application.Positions.Commands.SoftDeletePosition;

public class SoftDeletePositionCommandHandler : ICommandHandler<Guid, SoftDeletePositionCommand>
{
    private readonly IPositionRepository _positionRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly HybridCache _cache;

    public SoftDeletePositionCommandHandler(
        IPositionRepository positionRepository, 
        ITransactionManager transactionManager, 
        HybridCache cache)
    {
        _positionRepository = positionRepository;
        _transactionManager = transactionManager;
        _cache = cache;
    }

    public async Task<Result<Guid, Errors>> Handle(SoftDeletePositionCommand command, CancellationToken cancellationToken)
    {
        Result<ITransactionScope, Error> transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error.ToErrors();
        
        using ITransactionScope? transactionScope = transactionScopeResult.Value;
        
        var result = await _positionRepository.GetPositionById(command.PositionId, cancellationToken);

        if (result.IsFailure)
        {
            transactionScope.Rollback();
            return result.Error.ToErrors();
        }
        
        result.Value.SoftDeleted();
        
        var saveChanges = await _transactionManager.SaveChangesAsync(cancellationToken);
        if (saveChanges.IsFailure)
        {
            transactionScope.Rollback();
            return saveChanges.Error.ToErrors();
        }

        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
            return commitedResult.Error.ToErrors();
        
        await _cache.RemoveByTagAsync(CacheTags.Positions, cancellationToken);
        
        return command.PositionId;
    }
}
