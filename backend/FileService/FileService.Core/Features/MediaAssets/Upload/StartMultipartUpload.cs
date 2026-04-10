using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core.FileStorage;
using FileService.Domain;
using FileService.Domain.Extensions;
using FileService.Domain.ValueObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.Abstractions;
using Shared.CommonErrors;
using Shared.Database;
using Shared.EndpointResults;

namespace FileService.Core.Features.MediaAssets.Upload;

public class StartMultipartUpload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/files/multipart/start", async Task<EndpointResult<StartMultipartUploadResponse>> (
                [FromBody] StartMultipartUploadRequest request,
                [FromServices] StartMultipartUploadHandler handler,
                CancellationToken cancellationToken) =>
            await handler.Handle(new StartMultipartUploadCommand(request), cancellationToken));
    }
}

public record StartMultipartUploadCommand(StartMultipartUploadRequest Request) : ICommand;

public sealed class StartMultipartUploadHandler : ICommandHandler<StartMultipartUploadResponse, StartMultipartUploadCommand>
{
    private readonly IChunkSizeCalculator _chunkSizeCalculator;
    private readonly IS3Provider _s3Provider;
    private readonly IMediaRepository _mediaRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<StartMultipartUploadHandler> _logger;

    public StartMultipartUploadHandler(
        IS3Provider s3Provider, 
        IChunkSizeCalculator chunkSizeCalculator,
        IMediaRepository mediaRepository,
        ITransactionManager transactionManager,
        ILogger<StartMultipartUploadHandler> logger)
    {
        _chunkSizeCalculator = chunkSizeCalculator;
        _s3Provider = s3Provider;
        _mediaRepository = mediaRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<StartMultipartUploadResponse, Errors>> Handle(StartMultipartUploadCommand command, CancellationToken cancellationToken)
    {
        Result<FileName, Error> fileNameResult = FileName.Create(command.Request.FileName);
        if (fileNameResult.IsFailure)
            return fileNameResult.Error.ToErrors();

        Result<ContentType, Error> contentTypeResult = ContentType.Create(command.Request.ContentType);
        if (contentTypeResult.IsFailure)
            return contentTypeResult.Error.ToErrors();

        Result<(long ChunkSize, int TotalChunks), Error> chunkCalculationResult = _chunkSizeCalculator.CalculateChunkSize(command.Request.Size);
        
        var mediaData = MediaData.Create(
            fileNameResult.Value,
            contentTypeResult.Value,
            command.Request.Size,
            chunkCalculationResult.Value.TotalChunks);
        
        Guid mediaAssetId = Guid.NewGuid();
        
        var owner = MediaOwner.Create(command.Request.Context, command.Request.ContextId);
        if (owner.IsFailure)
            return owner.Error.ToErrors();
        
        var mediaAssetResult = MediaAsset.CreateForUpload(
            mediaAssetId,
            mediaData.Value,
            command.Request.AssetType.ToAssetType(),
            owner.Value);

        Result<ITransactionScope, Error> transactionScopeResult = await _transactionManager.BeginTransactionAsync(cancellationToken);
        using ITransactionScope? transactionScope = transactionScopeResult.Value;
        
        await _mediaRepository.AddAsync(mediaAssetResult.Value, cancellationToken);
        
        Result<string, Error> startUploadResult = await _s3Provider.StartMultipartUploadAsync(
            mediaAssetResult.Value.Key,
            mediaAssetResult.Value.MediaData,
            cancellationToken);

        if (startUploadResult.IsFailure)
        {
            transactionScope.Rollback();
            return startUploadResult.Error.ToErrors();
        }
        
        Result<IReadOnlyList<ChunkUploadUrl>, Error> chunkUploadUrlsResult = await _s3Provider.GenerateAllChunkUploadUrlsAsync(
            mediaAssetResult.Value.Key,
            startUploadResult.Value,
            chunkCalculationResult.Value.TotalChunks,
            cancellationToken);

        if (chunkUploadUrlsResult.IsFailure)
        {
            transactionScope.Rollback();
            return chunkUploadUrlsResult.Error.ToErrors();
        }

        await _transactionManager.SaveChangesAsync(cancellationToken);
        
        var commitedResult = transactionScope.Commit();
        if (commitedResult.IsFailure)
            return commitedResult.Error.ToErrors();
        
        _logger.LogInformation("Started multipart upload for media asset {MediaAssetId}", mediaAssetId);
        
        return new StartMultipartUploadResponse(
            mediaAssetId,
            startUploadResult.Value,
            chunkUploadUrlsResult.Value,
            chunkCalculationResult.Value.ChunkSize);
        
    }
}