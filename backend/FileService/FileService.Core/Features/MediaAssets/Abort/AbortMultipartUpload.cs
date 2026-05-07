using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.MediaAssets.Multipart.Requests;
using FileService.Core.FileStorage;
using FileService.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.Abstractions;
using Shared.CommonErrors;
using Shared.Database;
using Shared.EndpointResults;

namespace FileService.Core.Features.MediaAssets.Abort;

public sealed class AbortMultipartUpload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/files/multipart/abort", async Task<EndpointResult> (
            [FromBody] AbortMultipartUploadRequest request,
            [FromServices] AbortMultipartUploadHandler handler,
            CancellationToken cancellationToken) => await handler.Handle(new AbortMultipartUploadCommand(request), cancellationToken));
    }
}

public record AbortMultipartUploadCommand(AbortMultipartUploadRequest Request) : ICommand;


public sealed class AbortMultipartUploadHandler : ICommandHandler<AbortMultipartUploadCommand>
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaRepository _mediaRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<AbortMultipartUploadHandler> _logger;

    public AbortMultipartUploadHandler(
        IS3Provider s3Provider, 
        IMediaRepository mediaRepository, 
        ITransactionManager transactionManager,
        ILogger<AbortMultipartUploadHandler> logger) 
    {
        _s3Provider = s3Provider;
        _mediaRepository = mediaRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<UnitResult<Error>> Handle(AbortMultipartUploadCommand command, CancellationToken cancellationToken)
    {
       Result<MediaAsset, Error> mediaAssetResult = await _mediaRepository.GetBy(x => x.Id == command.Request.MediaAssetId, cancellationToken);
       if (mediaAssetResult.IsFailure)
       {
           return mediaAssetResult.Error;
       }
       
       MediaAsset mediaAsset = mediaAssetResult.Value;
       UnitResult<Error> abortResult = await _s3Provider.AbortMultipartUploadAsync(mediaAsset.Key, command.Request.UploadId, cancellationToken);
       if (abortResult.IsFailure)
       {
           return abortResult.Error;
       }

       UnitResult<Error> result =  mediaAsset.MarkFailed(DateTime.UtcNow);
       if (result.IsFailure)
           return result.Error;
       
       UnitResult<Error> commitedResult = await _transactionManager.SaveChangesAsync(cancellationToken);

       if (commitedResult.IsFailure)
           return commitedResult.Error;

       return UnitResult.Success<Error>();
    }
}