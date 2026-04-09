using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core.FileStorage;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.Abstractions;
using Shared.CommonErrors;
using Shared.EndpointResults;
using Shared.Validation;

namespace FileService.Core.Features.MediaAssets.Upload;

public sealed class GetChunkUploadUrlEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/files/multipart/url", async Task<EndpointResult<GetChunkUploadUrlResponse>> (
                [FromBody] GetChunkUploadUrlRequest request,
                [FromServices] GetChunkUploadUrlHandler handler,
                CancellationToken cancellationToken) =>
            await handler.Handle(new GetChunkUploadUrlCommand(request), cancellationToken));
    }
}

public class GetChunkUploadUrlValidator : AbstractValidator<GetChunkUploadUrlCommand>
{
    public GetChunkUploadUrlValidator()
    {
        RuleFor(x => x.Request).NotNull();
        
        RuleFor(x => x.Request.PartNumber)
            .GreaterThan(0);

        RuleFor(x => x.Request.MediaAssetId)
            .NotEmpty()
            .NotNull();
        
        RuleFor(x => x.Request.UploadId)
            .NotEmpty()
            .NotNull();
    }
}

public record GetChunkUploadUrlCommand(GetChunkUploadUrlRequest Request) : ICommand;

public sealed class GetChunkUploadUrlHandler : ICommandHandler<GetChunkUploadUrlResponse, GetChunkUploadUrlCommand>
{
    private readonly IValidator<GetChunkUploadUrlCommand> _validator;
    private readonly IS3Provider _s3Provider;
    private readonly IMediaRepository _mediaRepository;
    private readonly ILogger<GetChunkUploadUrlHandler> _logger;

    public GetChunkUploadUrlHandler(
        IValidator<GetChunkUploadUrlCommand> validator, 
        IS3Provider s3Provider, 
        IMediaRepository mediaRepository, 
        ILogger<GetChunkUploadUrlHandler> logger)
    {
        _validator = validator;
        _s3Provider = s3Provider;
        _mediaRepository = mediaRepository;
        _logger = logger;
    }

    public async Task<Result<GetChunkUploadUrlResponse, Errors>> Handle(GetChunkUploadUrlCommand command, CancellationToken cancellationToken)
    {
        ValidationResult? validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
            return validationResult.ToError().ToErrors();

        var mediaAssetResult = await _mediaRepository
            .GetBy(x => x.Id == command.Request.MediaAssetId, cancellationToken);

        if (mediaAssetResult.IsFailure)
            return mediaAssetResult.Error.ToErrors();
        
        var mediaAsset = mediaAssetResult.Value;
        
        Result<string, Error> chunkUploadUrlResult = await _s3Provider
            .GenerateChunkUploadUrlAsync(mediaAsset.Key, command.Request.UploadId, command.Request.PartNumber, cancellationToken);

        if (chunkUploadUrlResult.IsFailure)
            return chunkUploadUrlResult.Error.ToErrors();

        return new GetChunkUploadUrlResponse(
            chunkUploadUrlResult.Value,
            command.Request.PartNumber);
    }
}