using CSharpFunctionalExtensions;
using FileService.Core.Extensions;
using FileService.Core.FileStorage;
using FileService.Domain;
using FileService.Domain.ValueObjects;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shared.Abstractions;
using Shared.EndpointResults;
using Shared.CommonErrors;
using Shared.Validation;

namespace FileService.Core.Features.MediaAssets.Download;

public sealed class DownloadUrlEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/files/download-url/{fileId:guid}", async Task<EndpointResult<string>> (
                [FromRoute] Guid fileId,
                [FromServices] DownloadUrlHandler handler,
                CancellationToken cancellationToken) =>
            await handler.Handle(new DownloadUrlCommand(fileId), cancellationToken));
    }
}

public record DownloadUrlCommand(Guid FileId) : ICommand;

public class DownloadUrlValidator : AbstractValidator<DownloadUrlCommand>
{
    public DownloadUrlValidator()
    {
        //RuleFor(x => x.Path).ValidStoragePath();
        RuleFor(x => x.FileId)
            .NotEmpty()
            .NotNull();
    }
}

public sealed class DownloadUrlHandler : ICommandHandler<string, DownloadUrlCommand>
{
    private readonly IS3Provider _s3Provider;
    private readonly IValidator<DownloadUrlCommand> _validator;
    private readonly IReadDbContext _readDbContext;
    private readonly ILogger<DownloadUrlHandler> _logger;

    public DownloadUrlHandler(
        IS3Provider s3Provider, 
        IValidator<DownloadUrlCommand> validator, 
        IReadDbContext readDbContext,
        ILogger<DownloadUrlHandler> logger)
    {
        _s3Provider = s3Provider;
        _validator = validator;
        _readDbContext = readDbContext;
        _logger = logger;
    }

    public async Task<Result<string, Errors>> Handle(DownloadUrlCommand command, CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            return validationResult.ToError().ToErrors();
        }

        MediaAsset? mediaAssetResult = await _readDbContext.MediaAssetsRead
                .FirstOrDefaultAsync(m => m.Id == command.FileId, cancellationToken);

        if (mediaAssetResult == null)
            return Error.NotFound("file.not_found", "File not found").ToErrors();
        
        StorageKey  storageKeyResult = mediaAssetResult.Key;

        Result<string, Error> result = await _s3Provider.GenerateDownloadUrlAsync(storageKeyResult);
        
        if (result.IsFailure)
            return result.Error.ToErrors();

        _logger.LogInformation("Download url was generated for fileId {fileId}", command.FileId);

        return result.Value;
    }
}