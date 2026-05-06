using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using FileService.Contracts.MediaAssets.DTOs;
using FileService.Contracts.MediaAssets.Multipart.Requests;
using FileService.Contracts.MediaAssets.Multipart.Responses;
using Shared.CommonErrors;
using Shared.HttpCommunication;

namespace FileService.IntegrationTests.Infrastructure;

public sealed class MultipartUploadTestHelper
{
    private readonly HttpClient _appHttpClient;
    private readonly HttpClient _storageHttpClient;

    public MultipartUploadTestHelper(HttpClient appHttpClient, HttpClient storageHttpClient)
    {
        _appHttpClient = appHttpClient;
        _storageHttpClient = storageHttpClient;
    }

    public async Task<StartMultipartUploadResponse> StartAsync(
        FileInfo fileInfo,
        CancellationToken cancellationToken)
    {
        var request = new StartMultipartUploadRequest(
            fileInfo.Name,
            "video",
            "video/mp4",
            fileInfo.Length,
            "lesson",
            Guid.Parse("26053671-bfab-4043-9488-fee2e4a6258d")
        );

        HttpResponseMessage response = await _appHttpClient.PostAsJsonAsync(
            "/files/multipart/start",
            request,
            cancellationToken);

        Result<StartMultipartUploadResponse, Errors> result = await response
            .HandleResponseAsync<StartMultipartUploadResponse>(cancellationToken);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value.UploadId);

        return result.Value;
    }

    public async Task<IReadOnlyList<PartETagDto>> UploadChunksAsync(
        FileInfo fileInfo,
        StartMultipartUploadResponse startMultipartUploadResponse,
        CancellationToken cancellationToken)
    {
        await using var stream = fileInfo.OpenRead();

        var parts = new List<PartETagDto>();

        foreach (ChunkUploadUrl chunkUploadUrl in startMultipartUploadResponse.ChunkUploadUrls.OrderBy(c => c.PartNumber))
        {
            byte[] chunk = new byte[startMultipartUploadResponse.ChunkSize];
            int bytesRead = await stream.ReadAsync(
                chunk.AsMemory(0, startMultipartUploadResponse.ChunkSize),
                cancellationToken);

            if (bytesRead == 0)
                break;

            var content = new ByteArrayContent(chunk);

            HttpResponseMessage response = await _storageHttpClient.PutAsync(
                chunkUploadUrl.UploadUrl,
                content,
                cancellationToken);

            response.EnsureSuccessStatusCode();

            string? eTag = response.Headers.ETag?.Tag.Trim('"');

            parts.Add(new PartETagDto(chunkUploadUrl.PartNumber, eTag!));
        }

        return parts;
    }

    public async Task<UnitResult<Errors>> CompleteAsync(
        StartMultipartUploadResponse startMultipartUploadResponse,
        IEnumerable<PartETagDto> partEtags,
        CancellationToken cancellationToken)
    {
        var request = new CompleteMultipartUploadDtoRequest(
            startMultipartUploadResponse.MediaAssetId,
            startMultipartUploadResponse.UploadId,
            partEtags.ToList()
        );

        HttpResponseMessage response = await _appHttpClient.PostAsJsonAsync(
            "/files/multipart/end",
            request,
            cancellationToken);

        return await response.HandleResponseAsync(cancellationToken);
    }
}
