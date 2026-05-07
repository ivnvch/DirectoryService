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

namespace FileService.Core.Features.MediaAssets.Upload;

public sealed class CompleteMultipartUpload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/files/multipart/end", async Task<EndpointResult<CompleteMultipartUploadResponse>> (
            [FromBody] CompleteMultipartUploadDtoRequest dtoRequest,
            [FromServices] CompleteMultipartUploadHandler handler,
            CancellationToken cancellationToken) => await handler.Handle(new CompleteMultipartUploadCommand(dtoRequest), cancellationToken));
    }
}

public record CompleteMultipartUploadResponse(Guid MediaAssetId);
public record CompleteMultipartUploadCommand(CompleteMultipartUploadDtoRequest DtoRequest) : ICommand;

public sealed class CompleteMultipartUploadHandler : ICommandHandler<CompleteMultipartUploadResponse, CompleteMultipartUploadCommand>
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaRepository _mediaRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<CompleteMultipartUploadHandler> _logger;

    public CompleteMultipartUploadHandler(
        IS3Provider s3Provider, 
        IMediaRepository mediaRepository,
        ITransactionManager transactionManager,
        ILogger<CompleteMultipartUploadHandler> logger)
    {
        _s3Provider = s3Provider;
        _mediaRepository = mediaRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<CompleteMultipartUploadResponse, Errors>> Handle(CompleteMultipartUploadCommand command,
        CancellationToken cancellationToken)
    {
         (_, bool isFailure,  MediaAsset? mediaAsset, Error? error) = await _mediaRepository.GetBy(m => m.Id == command.DtoRequest.MediaAssetId, cancellationToken);
         if (isFailure)
             return error.ToErrors();

         Result<ITransactionScope, Error> transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
         using ITransactionScope? transactionResult = transactionScopeResult.Value;

         if (mediaAsset.MediaData.ExpectedChunksCount != command.DtoRequest.PartETags.Count)
         {
             transactionResult.Rollback();
             return GeneralErrors.Failure("Количество eTag не соответствует количеству чанков").ToErrors();
         }
         
         Result<string, Error> completeResult = await _s3Provider.CompleteMultipartUploadAsync(
             mediaAsset.Key,
             command.DtoRequest.UploadId,
             command.DtoRequest.PartETags,
             cancellationToken);

         if (completeResult.IsFailure)
         {
             mediaAsset.MarkFailed(DateTime.UtcNow);
             
             await _transactionManager.SaveChangesAsync(cancellationToken);
             return completeResult.Error.ToErrors();
         }

         mediaAsset.MarkUploaded(DateTime.UtcNow);
         
         await  _transactionManager.SaveChangesAsync(cancellationToken);
         
         var commitedResult = transactionResult.Commit();
         if (commitedResult.IsFailure)
         {
             _logger.LogError("Error while committing transaction");
             return commitedResult.Error.ToErrors();
         }
         
         _logger.LogInformation("File upload successfully. MediaAssetId: {MediaAssetId}", mediaAsset.Id);
                 
         return new CompleteMultipartUploadResponse(mediaAsset.Id);
    }
}