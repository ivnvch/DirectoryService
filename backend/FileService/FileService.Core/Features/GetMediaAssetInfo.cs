using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core.FileStorage;
using FileService.Core.Models;
using FileService.Domain;
using FileService.Domain.Enums;
using FileService.Domain.ValueObjects;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using Shared.CommonErrors;
using Shared.EndpointResults;

namespace FileService.Core.Features;

public sealed class GetMediaAssetInfo : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/files/{mediaAssetId:guid}", async Task<EndpointResult<GetMediaAssetDto>> (
            Guid mediaAssetId,
            [FromServices] GetMediaAssetInfoHandler handler,
            CancellationToken cancellationToken) => await handler.Handle(new GetMediaAssetInfoCommand(mediaAssetId), cancellationToken));
    }
}

public record GetMediaAssetInfoCommand(Guid MediaAssetId) : ICommand;
public sealed class GetMediaAssetInfoHandler : ICommandHandler<GetMediaAssetDto?, GetMediaAssetInfoCommand>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IS3Provider _s3Provider;

    public GetMediaAssetInfoHandler(IReadDbContext readDbContext, IS3Provider s3Provider)
    {
        _readDbContext = readDbContext;
        _s3Provider = s3Provider;
    }

    public async Task<Result<GetMediaAssetDto?, Errors>> Handle(GetMediaAssetInfoCommand command, CancellationToken cancellationToken)
    {
        MediaAsset? mediaAsset = await _readDbContext.MediaAssetsRead
            .FirstOrDefaultAsync(x => x.Id == command.MediaAssetId && x.Status != MediaStatus.Deleted, cancellationToken);

        if (mediaAsset == null)
            return Result.Success<GetMediaAssetDto?, Errors>(null);
        
        Result<string, Error> urlsResult = await _s3Provider.GenerateDownloadUrlAsync(mediaAsset.Key);
        if (urlsResult.IsFailure)
            return urlsResult.Error.ToErrors();

        string? url = null;
        if (mediaAsset.Status == MediaStatus.Ready)
        {
            var (_, isFailure, presignedUrl, error) = await _s3Provider.GenerateDownloadUrlAsync(mediaAsset.Key);
            
            if (isFailure)
                return error.ToErrors();

            url = presignedUrl;
        }

        GetMediaAssetDto mediaAssetDto = new GetMediaAssetDto(
            mediaAsset.Id,
            mediaAsset.Status.ToString().ToLowerInvariant(),
            mediaAsset.AssetType.ToString().ToLowerInvariant(),
            url);

        return mediaAssetDto;
    }
}

