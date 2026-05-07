using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using FileService.Contracts.MediaAssets.Queries.Requests;
using FileService.Contracts.MediaAssets.Queries.Responses;
using Microsoft.Extensions.Logging;
using Shared.CommonErrors;
using Shared.HttpCommunication;

namespace FileService.Contracts.HttpCommunication;

public class FileHttpClient : IFileCommunicationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<FileHttpClient> _logger;
    
    public FileHttpClient(HttpClient httpClient, ILogger<FileHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }
    public async Task<Result<GetMediaAssetsResponse, Errors>> GetMediaAssets(GetMediaAssetsRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            HttpResponseMessage response = await _httpClient.PostAsJsonAsync("/files/batch", request, cancellationToken);
            return await response.HandleResponseAsync<GetMediaAssetsResponse>(cancellationToken);

        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media assets for {MeidaAssetIds}", request.MediaAssetIds);
            
            return Error.Failure("server.internal", "Failed to request media assets info").ToErrors();
        }
    }

    public async Task<Result<GetMediaAssetsResponse, Errors>> GetMediaAssets
        (Guid mediaAssetId, CancellationToken cancellationToken)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/files/{mediaAssetId}", cancellationToken);
            return await response.HandleResponseAsync<GetMediaAssetsResponse>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting media asset for {MediaAssetId}",  mediaAssetId);
            return Error.Failure("server.error", "Failed to get media asset").ToErrors();
        }
    }

    public async Task<Result<CheckMediaAssetExistsResponse, Errors>> CheckMediaAssetExists(Guid mediaAssetId, CancellationToken cancellationToken)
    {
        try
        {
           var response = await _httpClient.GetAsync($"/files/{mediaAssetId}/exists", cancellationToken);
           return await response.HandleResponseAsync<CheckMediaAssetExistsResponse>(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking media asset exists for {MediaAssetId}", mediaAssetId);
            return Error.Failure("server.error", "Failed to check media asset exists").ToErrors();
        }
    }
}