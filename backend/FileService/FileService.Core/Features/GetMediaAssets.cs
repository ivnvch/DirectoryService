using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.MediaAssets.DTOs;
using FileService.Contracts.MediaAssets.Queries.Requests;
using FileService.Contracts.MediaAssets.Queries.Responses;
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

public sealed class GetMediaAssetsEndpoint : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost("/files/batch", async Task<EndpointResult<GetMediaAssetsResponse>> (
            [FromBody] GetMediaAssetsRequest request,
            [FromServices] GetMediaAssetsHandler handler,
            CancellationToken cancellationToken) => await handler.Handle(new GetMediaAssetCommand(request), cancellationToken));
    }
}

public record GetMediaAssetCommand(GetMediaAssetsRequest Request) : ICommand;

public sealed class GetMediaAssetsHandler : ICommandHandler<GetMediaAssetsResponse, GetMediaAssetCommand>
{
    private readonly IReadDbContext _readDbContext;
    private readonly IS3Provider _s3Provider;

    public GetMediaAssetsHandler(IReadDbContext readDbContext, IS3Provider s3Provider)
    {
        _readDbContext = readDbContext;
        _s3Provider = s3Provider;
    }

    public async Task<Result<GetMediaAssetsResponse, Errors>> Handle(GetMediaAssetCommand command, CancellationToken cancellationToken)
    {
        if (!command.Request.MediaAssetIds.Any())
            return new GetMediaAssetsResponse([]);
        
        List<MediaAsset> mediaAssets = await _readDbContext.MediaAssetsRead
            .Where(x => command.Request.MediaAssetIds.Contains(x.Id) && x.Status != MediaStatus.Deleted).ToListAsync(cancellationToken);

        var readyMediaAssets = mediaAssets.Where(m => m.Status == MediaStatus.Ready).ToList();
        
        List<StorageKey> keys = readyMediaAssets.Select(x => x.Key).ToList();
        
        Result<IReadOnlyList<MediaUrl>, Errors> urlsResult = await _s3Provider.GenerateDownloadUrlsAsync(keys, cancellationToken);
        if (urlsResult.IsFailure)
            return urlsResult.Error;
        
        IReadOnlyList<MediaUrl>? urls = urlsResult.Value;

        var urlsDict = urls.ToDictionary(url => url.StorageKey, url => url.PresignedUrl);
        
        var result = new List<GetMediaAssetDto>();
        foreach (MediaAsset mediaAsset in mediaAssets)
        {
            urlsDict.TryGetValue(mediaAsset.Key, out string? url);
            
            var mediaAssetDto = new GetMediaAssetDto(
                mediaAsset.Id,
                mediaAsset.Status.ToString().ToLowerInvariant(),
                mediaAsset.AssetType.ToString().ToLowerInvariant(),
                url);
            
            result.Add(mediaAssetDto);
        }
        
        return new GetMediaAssetsResponse(result);
    }
}