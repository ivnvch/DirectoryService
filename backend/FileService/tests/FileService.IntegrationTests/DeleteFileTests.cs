using System.Net;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.MediaAssets.DTOs;
using FileService.Contracts.MediaAssets.Multipart.Responses;
using FileService.Domain;
using FileService.Domain.Enums;
using FileService.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Shared.CommonErrors;
using Shared.HttpCommunication;

namespace FileService.IntegrationTests;

public class DeleteFileTests : FileBaseTests
{
    public DeleteFileTests(FileTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task DeleteFile_ShouldReturnSuccess()
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        
        FileInfo fileInfo = new(Path.Combine(AppContext.BaseDirectory, "Resources", TEST_FILE_NAME));
        
        var multipartUpload = new MultipartUploadTestHelper(AppHttpClient, HttpClient);
        StartMultipartUploadResponse startResponse = await multipartUpload.StartAsync(fileInfo, cancellationToken);
        IReadOnlyList<PartETagDto> partEtags = await multipartUpload.UploadChunksAsync(
            fileInfo,
            startResponse,
            cancellationToken);
        UnitResult<Errors> completeResult = await multipartUpload.CompleteAsync(
            startResponse,
            partEtags,
            cancellationToken);
        
        Assert.True(completeResult.IsSuccess);

        HttpResponseMessage deleteResponse = await AppHttpClient.DeleteAsync($"/files/delete/videos/{startResponse.MediaAssetId}", cancellationToken);
        
        Result<string, Errors> deleteResult = await deleteResponse.HandleResponseAsync<string>(cancellationToken);
        
        Assert.True(deleteResult.IsSuccess);

        await ExecuteInDb(async dbContext =>
        {
            MediaAsset? mediaAsset = await dbContext.MediaAssets
                .FirstOrDefaultAsync(m => m.Id ==startResponse.MediaAssetId, cancellationToken);
            
            Assert.NotNull(mediaAsset);
            Assert.Equal(MediaStatus.Deleted, mediaAsset.Status);
        });
    }
    
    [Fact]
    public async Task DeleteFile_ShouldReturnError_WhenFileNotFound()
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;
        
        HttpResponseMessage deleteResponse = await AppHttpClient.DeleteAsync("/files/delete/videos/1234567890", cancellationToken);

        string responseBody = await deleteResponse.Content.ReadAsStringAsync(cancellationToken);
        
        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
        Assert.Contains("OBJECT_NOT_FOUND", responseBody);
    }
}