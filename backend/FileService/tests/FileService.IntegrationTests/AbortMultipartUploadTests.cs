using System.Net.Http.Json;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.MediaAssets.Multipart.Requests;
using FileService.Contracts.MediaAssets.Multipart.Responses;
using FileService.Domain;
using FileService.Domain.Enums;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.CommonErrors;
using Shared.HttpCommunication;

namespace FileService.IntegrationTests;

public class AbortMultipartUploadTests : FileBaseTests
{
    private readonly MultipartUploadTestHelper _multipartUpload;

    public AbortMultipartUploadTests(FileTestWebFactory factory) : base(factory)
    {
        _multipartUpload = new MultipartUploadTestHelper(AppHttpClient, HttpClient);
    }

    [Fact]
    public async Task AbortMultipartUpload_ShouldReturnSuccess()
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));

        StartMultipartUploadResponse startMultipartUploadResponse = await _multipartUpload.StartAsync(fileInfo, cancellationToken);
        
        var abortRequest = new AbortMultipartUploadRequest(
            startMultipartUploadResponse.MediaAssetId,
            startMultipartUploadResponse.UploadId);
        
        HttpResponseMessage abortResponse = await AppHttpClient.PostAsJsonAsync("/files/multipart/abort", abortRequest, cancellationToken);

        UnitResult<Errors> abortResult = await abortResponse
            .HandleResponseAsync(cancellationToken);
        
        Assert.True(abortResult.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            MediaAsset? mediaAsset = await dbContext.MediaAssets
                .FirstOrDefaultAsync(x => x.Id == abortRequest.MediaAssetId, cancellationToken);
            
            Assert.NotNull(mediaAsset);
            Assert.Equal(MediaStatus.Failed, mediaAsset.Status);
        });
    }

    [Fact]
    public async Task AbortMultipartUpload_ShouldReturnError_WhenMediaAssetNotFound()
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        var abortRequest = new AbortMultipartUploadRequest(
            Guid.NewGuid(),
            "upload_id");

        HttpResponseMessage abortResponse = await AppHttpClient.PostAsJsonAsync("/files/multipart/abort", abortRequest, cancellationToken);

        UnitResult<Errors> abortResult = await abortResponse
            .HandleResponseAsync(cancellationToken);
        
        Assert.True(abortResult.IsFailure);
    }
}