using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.MediaAssets.Upload.Requests;
using FileService.Core.FileStorage;
using FileService.Domain;
using FileService.Domain.Enums;
using FileService.Domain.Extensions;
using FileService.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.Abstractions;
using Shared.EndpointResults;
using Shared.CommonErrors;
using Shared.Validation;

namespace FileService.Core.Features.MediaAssets.Upload;

public sealed class UploadUrlEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPut("/files/upload/url", async Task<EndpointResult<string>> (
                [FromBody] GetUploadUrlRequest request,
                [FromServices] UploadUrlHandler handler,
                CancellationToken cancellationToken) =>
            await handler.Handle(new UploadUrlCommand(request), cancellationToken)).DisableAntiforgery();
    }
}

public class UploadUrlCommandValidator : AbstractValidator<UploadUrlCommand>
{
    public UploadUrlCommandValidator()
    {
       
    }
}
public record UploadUrlCommand(GetUploadUrlRequest Request) : ICommand;

public sealed class UploadUrlHandler : ICommandHandler<string, UploadUrlCommand>
{
    private readonly IS3Provider _s3Provider;
    private readonly IValidator<UploadUrlCommand> _validator;
    private readonly ILogger<UploadUrlHandler> _logger;

    public UploadUrlHandler(
        IS3Provider s3Provider, 
        IValidator<UploadUrlCommand> validator, 
        ILogger<UploadUrlHandler> logger)
    {
        _s3Provider = s3Provider;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<string, Errors>> Handle(UploadUrlCommand command, CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            return validationResult.ToError().ToErrors();
        
        AssetType assetType = command.Request.AssetType.ToAssetType();

        Result<FileName, Error> fileName = FileName.Create(command.Request.FileName);
        Result<ContentType, Error> contentType = ContentType.Create(command.Request.ContentType);
        
        long size = command.Request.FileSize;

        Result<MediaData, Error> mediaData = MediaData.Create(fileName.Value, contentType.Value, size, 1);

        var mediaAssetId = Guid.CreateVersion7();
        var owner = MediaOwner.Create(command.Request.Context, mediaAssetId);


        Result<MediaAsset, Error> mediaAssetResult =
            MediaAsset.CreateForUpload(mediaAssetId, mediaData.Value, assetType, owner.Value);
        
        
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error.ToErrors();

        MediaAsset mediaAsset = mediaAssetResult.Value;

        Result<string, Error> generateResult = await _s3Provider.GenerateUploadUrlAsync(mediaAsset.Key, mediaData.Value, cancellationToken);

        if (generateResult.IsFailure)
            return generateResult.Error.ToErrors();

        _logger.LogInformation("Upload url was generated for file {FileName}", command.Request.FileName);

        return generateResult.Value;
    }
}