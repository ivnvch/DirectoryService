using System.Net;
using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.MediaAssets.DTOs;
using FileService.Contracts.MediaAssets.Multipart.Requests;
using FileService.Contracts.MediaAssets.Multipart.Responses;
using FileService.Domain;
using FileService.Domain.Enums;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.CommonErrors;

namespace FileService.IntegrationTests;

public class StartMultipartUploadTests : FileBaseTests
{
    private readonly MultipartUploadTestHelper _multipartUpload;

    public StartMultipartUploadTests(FileTestWebFactory factory) : base(factory)
    {
        _multipartUpload = new MultipartUploadTestHelper(AppHttpClient, HttpClient);
    }
    
    [Fact]
    public async Task StartMultipartUpload_ShouldReturnSuccess()
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));
        
        StartMultipartUploadResponse startMultipartUploadResponse = await _multipartUpload.StartAsync(fileInfo, cancellationToken);

        await ExecuteInDb(async dbContext =>
        {
            MediaAsset? mediaAsset =
                await dbContext.MediaAssets
                    .FirstOrDefaultAsync(x => x.Id == startMultipartUploadResponse.MediaAssetId, cancellationToken);
            
            Assert.Equal(MediaStatus.Uploading, mediaAsset?.Status);
            Assert.NotNull(mediaAsset);
        });
        
        IReadOnlyList<PartETagDto> partEtags = await _multipartUpload.UploadChunksAsync(fileInfo, startMultipartUploadResponse, cancellationToken);
        
        UnitResult<Errors> result = await _multipartUpload.CompleteAsync(startMultipartUploadResponse, partEtags, cancellationToken);
        
        Assert.True(result.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            MediaAsset? mediaAsset =
                await dbContext.MediaAssets
                    .FirstOrDefaultAsync(x => x.Id == startMultipartUploadResponse.MediaAssetId, cancellationToken);

            Assert.NotNull(mediaAsset);
            Assert.Equal(MediaStatus.Uploaded, mediaAsset.Status);
        });
    }
    
    [Fact]
    public async Task StartMultipartUpload_ShouldReturnError_WhenFileNameIsInvalid()
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        var request = new StartMultipartUploadRequest(
            "not_found",
            "video",
            "video/mp4",
            1024,
            "lesson",
            Guid.Parse("26053671-bfab-4043-9488-fee2e4a6258d")
        );
        
        HttpResponseMessage startMultipartResponse = await AppHttpClient.PostAsJsonAsync("/files/multipart/start", request, cancellationToken);
        
        string responseBody = await startMultipartResponse.Content.ReadAsStringAsync(cancellationToken);
        
        Assert.Equal(HttpStatusCode.BadRequest, startMultipartResponse.StatusCode);
        Assert.Contains("value.is.invalid", responseBody);
    }

    [Fact]
    public async Task StartMultipartUpload_ShouldReturnError_WhenFileSizeIsInvalid()
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        var request = new StartMultipartUploadRequest(
            "test.mp4",
            "video",
            "video/mp4",
            0,
            "lesson",
            Guid.Parse("26053671-bfab-4043-9488-fee2e4a6258d")
        );

        HttpResponseMessage response = await AppHttpClient.PostAsJsonAsync(
            "/files/multipart/start",
            request,
            cancellationToken);
        
        string responseBody = await response.Content.ReadAsStringAsync(cancellationToken);
        
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("value.is.invalid", responseBody);
    }

}