using CSharpFunctionalExtensions;
using FileService.Core.Extensions;
using FileService.Core.FileStorage;
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

namespace FileService.Core.Features.MediaAssets.Download;

public sealed class DownloadUrlEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/files/download/url/{path}", async Task<EndpointResult<string>> (
                [FromRoute] string path,
                [FromServices] DownloadUrlHandler handler,
                CancellationToken cancellationToken) =>
            await handler.Handle(new DownloadUrlCommand(path), cancellationToken));
    }
}

public record DownloadUrlCommand(string Path) : ICommand;

public class DownloadUrlValidator : AbstractValidator<DownloadUrlCommand>
{
    public DownloadUrlValidator()
    {
        RuleFor(x => x.Path).ValidStoragePath();
    }
}

public sealed class DownloadUrlHandler : ICommandHandler<string, DownloadUrlCommand>
{
    private readonly IS3Provider _s3Provider;
    private readonly IValidator<DownloadUrlCommand> _validator;
    private readonly ILogger<DownloadUrlHandler> _logger;

    public DownloadUrlHandler(
        IS3Provider s3Provider, 
        IValidator<DownloadUrlCommand> validator, 
        ILogger<DownloadUrlHandler> logger)
    {
        _s3Provider = s3Provider;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<string, Errors>> Handle(DownloadUrlCommand command, CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);
        
        if (!validationResult.IsValid)
        {
            return validationResult.ToError().ToErrors();
        }

        Result<StorageKey, Error> storageKeyResult = StorageKey.FromStoragePath(command.Path);
        
        if (storageKeyResult.IsFailure)
            return storageKeyResult.Error.ToErrors();

        Result<string, Error> result = await _s3Provider.GenerateDownloadUrlAsync(storageKeyResult.Value);
        
        if (result.IsFailure)
            return result.Error.ToErrors();

        _logger.LogInformation("Download url was generated for path {Path}", command.Path);

        return result.Value;
    }
}