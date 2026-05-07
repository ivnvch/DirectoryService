using CSharpFunctionalExtensions;
using FileService.Contracts.MediaAssets.Queries.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Shared.Abstractions;
using Shared.CommonErrors;
using Shared.EndpointResults;

namespace FileService.Core.Features.MediaAssets;

public class CheckMediaAssetExists : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapGet("/files/{mediaAssetId:guid}/exists", async Task<EndpointResult<CheckMediaAssetExistsResponse>> (
            [FromRoute] Guid mediaAssetId,
            [FromServices] CheckMediaAssetExistsHandler handler,
            CancellationToken cancellationToken) => await handler.Handle(new CheckMediaAssetExistsCommand(mediaAssetId), cancellationToken));
    }
}

public record CheckMediaAssetExistsCommand(Guid MediaAssetId) : ICommand;
public sealed class CheckMediaAssetExistsHandler : ICommandHandler<CheckMediaAssetExistsResponse, CheckMediaAssetExistsCommand>
{
    private readonly IReadDbContext _readDbContext;

    public CheckMediaAssetExistsHandler(IReadDbContext readDbContext)
    {
        _readDbContext = readDbContext;
    }

    public async Task<Result<CheckMediaAssetExistsResponse, Errors>> Handle(CheckMediaAssetExistsCommand command, CancellationToken cancellationToken)
    {
        bool exists = await _readDbContext.MediaAssetsRead
            .AnyAsync(x => x.Id == command.MediaAssetId, cancellationToken);
        
        return new CheckMediaAssetExistsResponse(exists);
    }
}