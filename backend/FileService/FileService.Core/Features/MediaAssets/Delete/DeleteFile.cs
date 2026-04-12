using CSharpFunctionalExtensions;
using FileService.Core.Extensions;
using FileService.Core.FileStorage;
using FileService.Domain;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Shared.Abstractions;
using Shared.EndpointResults;
using Shared.CommonErrors;

namespace FileService.Core.Features.MediaAssets.Delete;

public sealed class DeleteFileEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/files/delete/{**path}", async Task<EndpointResult<string>> (
                [FromRoute] string path,
                [FromServices] DeleteFileHandler handler,
                CancellationToken cancellationToken) =>
            await handler.Handle(new DeleteFileCommand(path), cancellationToken)).DisableAntiforgery();
    }
}

public record DeleteFileCommand(string Path) : ICommand;

public sealed class DeleteFileCommandValidator : AbstractValidator<DeleteFileCommand>
{
    public DeleteFileCommandValidator()
    {
        RuleFor(x => x.Path).ValidStoragePath();
    }
}

public sealed class DeleteFileHandler : ICommandHandler<string, DeleteFileCommand>
{
    private readonly IS3Provider _s3Provider;
    private readonly IMediaRepository _mediaRepository;
    private readonly ILogger<DeleteFileHandler> _logger;

    public DeleteFileHandler(
        IS3Provider s3Provider, 
        IMediaRepository mediaRepository,
        ILogger<DeleteFileHandler> logger)
    {
        _s3Provider = s3Provider;
        _mediaRepository = mediaRepository;
        _logger = logger;
    }

    public async Task<Result<string, Errors>> Handle(DeleteFileCommand command, CancellationToken cancellationToken)
    {
        Result<(string BucketName, string ObjectKey), Error> parseResult = command.Path.ParseStoragePath();
        if (parseResult.IsFailure)
            return parseResult.Error.ToErrors();

        MediaAsset? mediaAsset = await _mediaRepository.GetByStoragePathAsync(
            parseResult.Value.BucketName,
            parseResult.Value.ObjectKey,
            cancellationToken);

        if (mediaAsset is null)
            return FileErrors.ObjectNotFound($"{parseResult.Value.BucketName}/{parseResult.Value.ObjectKey}").ToErrors();

        Result<string, Error> deleteResult = await _s3Provider.DeleteFileAsync(
            parseResult.Value.BucketName,
            parseResult.Value.ObjectKey,
            cancellationToken);

        if (deleteResult.IsFailure)
            return deleteResult.Error.ToErrors();
        
        UnitResult<Error> markDeletedResult = mediaAsset.MarkDelete(DateTime.UtcNow);
        if (markDeletedResult.IsFailure)
            return markDeletedResult.Error.ToErrors();
        
        await _mediaRepository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Delete file command succeeded for bucket '{BucketName}' with key '{ObjectKey}'",
            parseResult.Value.BucketName,
            parseResult.Value.ObjectKey);

        return $"{parseResult.Value.BucketName}/{parseResult.Value.ObjectKey}";
    }
}
