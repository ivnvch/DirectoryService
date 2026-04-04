using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core.Extensions;
using FileService.Domain;
using FileService.Domain.Enums;
using FileService.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.Abstractions;
using Shared.Database;
using Shared.EndpointResults;
using Shared.Errors;
using Shared.Validation;

namespace FileService.Core.Features.MediaAssets.Upload;

public class UploadFileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/files/upload", async Task<EndpointResult<Guid>> (
            [FromForm] UploadFileRequest request,
            [FromServices] UploadFileHandler handler, 
                CancellationToken cancellationToken) =>
            await handler.Handle(new UploadFileCommand(request), cancellationToken)).DisableAntiforgery();
    }
}

public class UploadFileValidator : AbstractValidator<UploadFileCommand>
{
    public UploadFileValidator()
    {
        RuleFor(x => x.UploadFileRequest).NotNull();
        When(x => x.UploadFileRequest is not null, () =>
        {
            RuleFor(x => x.UploadFileRequest.AssetType)
                .Must(CheckConvertToAssetType.BeSupportedAssetType)
                .WithError(GeneralErrors.ValueIsInvalid("assetType"));
            RuleFor(x => x.UploadFileRequest.Context)
                .NotEmpty()
                .MaximumLength(50)
                .WithError(GeneralErrors.ValueIsInvalid("context"));
            RuleFor(x => x.UploadFileRequest.File).NotNull();
            When(x => x.UploadFileRequest.File is not null, () =>
            {
                RuleFor(x => x.UploadFileRequest.File.FileName)
                    .NotEmpty()
                    .Must(name => name.LastIndexOf('.') > 0)
                    .WithError(GeneralErrors.ValueIsInvalid("fileName"));
                RuleFor(x => x.UploadFileRequest.File.ContentType)
                    .NotEmpty()
                    .WithError(GeneralErrors.ValueIsInvalid("contentType"));
                RuleFor(x => x.UploadFileRequest.File.Length)
                    .GreaterThan(0)
                    .WithError(GeneralErrors.ValueIsInvalid("fileLength"));
            });
        });
    }
}

public record UploadFileCommand(UploadFileRequest  UploadFileRequest) : ICommand;

public sealed class UploadFileHandler : ICommandHandler<Guid, UploadFileCommand>
{
    private readonly IS3Provider _s3Provider;
    private readonly IValidator<UploadFileCommand> _validator;
    private readonly IMediaRepository _mediaRepository;
    private readonly ITransactionManager _transactionManager;
    private readonly ILogger<UploadFileHandler> _logger;

    public UploadFileHandler(
        IS3Provider s3Provider, 
        IValidator<UploadFileCommand> validator, 
        IMediaRepository mediaRepository,
        ITransactionManager transactionManager, 
        ILogger<UploadFileHandler> logger)
    {
        _s3Provider = s3Provider;
        _validator = validator;
        _mediaRepository = mediaRepository;
        _transactionManager = transactionManager;
        _logger = logger;
    }

    public async Task<Result<Guid, Errors>> Handle(UploadFileCommand command, CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError().ToErrors();
        
        AssetType assetType = command.UploadFileRequest.AssetType.ToAssetType();

        Result<FileName, Error> fileName = FileName.Create(command.UploadFileRequest.File.FileName);
        Result<ContentType, Error> contentType = ContentType.Create(command.UploadFileRequest.File.ContentType);
        
        long size = command.UploadFileRequest.File.Length;

        Result<MediaData, Error> mediaData = MediaData.Create(fileName.Value, contentType.Value, size, 1);

        var mediaAssetId = Guid.CreateVersion7();
        var owner = MediaOwner.Create(command.UploadFileRequest.Context, mediaAssetId);


        Result<MediaAsset, Error> mediaAssetResult =
            MediaAsset.CreateForUpload(mediaAssetId, mediaData.Value, assetType, owner.Value);
        
        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error.ToErrors();
        
        MediaAsset mediaAsset = mediaAssetResult.Value;

        await _mediaRepository.AddAsync(mediaAsset, cancellationToken);
        
        UnitResult<Error> commitedResult = await _transactionManager.SaveChangesAsync(cancellationToken);

        if (commitedResult.IsFailure)
            return commitedResult.Error.ToErrors();

        UnitResult<Error> uploadResult = await _s3Provider.UploadFileAsync(mediaAsset.Key,
            command.UploadFileRequest.File.OpenReadStream(), mediaData.Value, cancellationToken);

        if (uploadResult.IsFailure)
        {
            mediaAsset.MarkFailed(DateTime.UtcNow);

            UnitResult<Error> commitedFailedUpload = await _transactionManager.SaveChangesAsync(cancellationToken);

            if (commitedFailedUpload.IsFailure)
            {
                return commitedFailedUpload.Error.ToErrors();
            }
            
            return uploadResult.Error.ToErrors();
        }
        
        mediaAsset.MarkUploaded(DateTime.UtcNow);

        UnitResult<Error> commitedSuccessUpload = await
            _transactionManager.SaveChangesAsync(cancellationToken);

        if (commitedSuccessUpload.IsFailure)
            return commitedSuccessUpload.Error.ToErrors();
        
        _logger.LogInformation("File {FileName} was successfully uploaded", command.UploadFileRequest.File.FileName);

        return mediaAssetId;

    }
}