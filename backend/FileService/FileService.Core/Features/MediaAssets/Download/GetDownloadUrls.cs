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

public sealed class DownloadUrlsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/files/download/urls", async Task<EndpointResult<IEnumerable<string>>> (
                [FromQuery] string[] paths,
                [FromServices] DownloadUrlsHandler handler,
                CancellationToken cancellationToken) =>
            await handler.Handle(new DownloadUrlsCommand(paths), cancellationToken));
    }
}

public class DownloadUrlsValidator : AbstractValidator<DownloadUrlsCommand>
{
    public DownloadUrlsValidator()
    {
        RuleForEach(x => x.Paths)
            .NotEmpty()
            .WithError(GeneralErrors.ValueIsRequired("Path is required"))
            .MustBeValueObject(p => p.ParseStoragePath());
    }
}

public record DownloadUrlsCommand(IEnumerable<string> Paths) : ICommand;

public sealed class DownloadUrlsHandler : ICommandHandler<IEnumerable<string>, DownloadUrlsCommand>
{
    private readonly IS3Provider _s3Provider;
    private readonly IValidator<DownloadUrlsCommand> _validator;
    private readonly ILogger<DownloadUrlsHandler> _logger;

    public DownloadUrlsHandler(
        IS3Provider s3Provider, 
        IValidator<DownloadUrlsCommand> validator, 
        ILogger<DownloadUrlsHandler> logger)
    {
        _s3Provider = s3Provider;
        _validator = validator;
        _logger = logger;
    }

    public async Task<Result<IEnumerable<string>, Errors>> Handle(
        DownloadUrlsCommand command,
        CancellationToken cancellationToken)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);

        if (!validationResult.IsValid)
            return validationResult.ToError().ToErrors();

        List<string> urls = [];

        foreach (string path in command.Paths)
        {
            (string location, string? prefix, string key) = path.ParseStorageKeyParts().Value;

            StorageKey storageKey = StorageKey.Create(location, prefix, key).Value;

            Result<string, Error> getResult = await _s3Provider.GenerateDownloadUrlAsync(storageKey);

            if (getResult.IsFailure)
                return getResult.Error.ToErrors();

            urls.Add(getResult.Value);
        }

        _logger.LogInformation("Download url was generated for paths {Paths}", string.Join(", ", command.Paths));

        return urls;
    }
}