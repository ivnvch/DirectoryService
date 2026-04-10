using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core.FileStorage;
using FileService.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Abstractions;
using Shared.CommonErrors;
using Shared.Database;
using Shared.EndpointResults;

namespace FileService.Core.Features.MediaAssets.Cancel;

public sealed class CancelMultipartUploadEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/files/multipart/cancel", async Task<EndpointResult> (
                [FromBody] CancelMultipartUploadRequest request,
                [FromServices] CancelMultipartUploadHandler handler,
                CancellationToken cancellationToken) =>
            await handler.Handle(new CancelMultipartUploadCommand(request), cancellationToken));
    }
}

public record CancelMultipartUploadCommand(CancelMultipartUploadRequest Request) : ICommand;

public sealed class CancelMultipartUploadHandler : ICommandHandler<CancelMultipartUploadCommand>
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaRepository _mediaRepository;
    private readonly ITransactionManager _transactionManager;

    public CancelMultipartUploadHandler(
        IS3Provider s3Provider, 
        IMediaRepository mediaRepository, 
        ITransactionManager transactionManager)
    {
        _s3Provider = s3Provider;
        _mediaRepository = mediaRepository;
        _transactionManager = transactionManager;
    }

    public async Task<UnitResult<Error>> Handle(CancelMultipartUploadCommand command, CancellationToken cancellationToken)
    {
        Result<ITransactionScope, Error> transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        if (transactionScopeResult.IsFailure)
            return transactionScopeResult.Error;
        
        using ITransactionScope? transactionScope = transactionScopeResult.Value;
        
        Result<MediaAsset, Error> mediaAssetResult = await _mediaRepository.GetBy(m => m.Id == command.Request.MediaAssetId, cancellationToken);
        if (mediaAssetResult.IsFailure)
        {
            transactionScope.Rollback();
            return mediaAssetResult.Error;
        }
        
        MediaAsset? mediaAsset = mediaAssetResult.Value;
        
        UnitResult<Error> cancelResult = await _s3Provider.AbortMultipartUploadAsync(mediaAsset.Key, command.Request.UploadId, cancellationToken);
        if (cancelResult.IsFailure)
            return cancelResult.Error;

        mediaAsset.MarkDelete(DateTime.UtcNow);

        await _transactionManager.SaveChangesAsync(cancellationToken);  
        
        UnitResult<Error> transactionCommitedResult =  transactionScope.Commit();
        if (transactionCommitedResult.IsFailure)
        {
            transactionScope.Rollback();
            return transactionCommitedResult.Error;
        }
        
        return UnitResult.Success<Error>();
    }
}