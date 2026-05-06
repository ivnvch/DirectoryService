using System.Net.Http.Headers;
using CSharpFunctionalExtensions;
using FileService.Contracts.MediaAssets.Upload.Requests;
using FileService.IntegrationTests.Infrastructure;
using Shared.CommonErrors;
using Shared.HttpCommunication;

namespace FileService.IntegrationTests;

public class GetDownloadUrlTests : FileBaseTests
{
    public GetDownloadUrlTests(FileTestWebFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task GetDownloadUrl_ShouldReturnSuccess()
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        // 1. Upload a file - returns MediaAssetId (Guid)
        Guid mediaAssetId = await UploadTestFile(cancellationToken);

        // 2. Call the download URL endpoint by file id
        var result = await GetDownloadUrl(mediaAssetId, cancellationToken);

        // 3. Assert success and a valid presigned URL is returned
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Contains("://", result.Value); // presigned URL must contain a protocol
    }

    [Fact]
    public async Task GetDownloadUrl_ShouldReturnError_WhenPathIsInvalid()
    {
        CancellationToken cancellationToken = new CancellationTokenSource().Token;

        Guid missingFileId = Guid.NewGuid();

        var result = await GetDownloadUrl(missingFileId, cancellationToken);

        Assert.True(result.IsFailure);
    }

    private async Task<Guid> UploadTestFile(CancellationToken cancellationToken)
    {
        FileInfo fileInfo = new(Path.Combine(
            AppContext.BaseDirectory,
            "Resources",
            TEST_FILE_NAME));

        await using var stream = fileInfo.OpenRead();

        using var content = new MultipartFormDataContent();

        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");

        content.Add(fileContent, "file", TEST_FILE_NAME);
        content.Add(new StringContent("video"), "AssetType");
        content.Add(new StringContent("lesson"), "Context");

        var uploadFileResponse = await AppHttpClient.PostAsync(
            "/files/upload",
            content,
            cancellationToken);
        
        return await ReadGuidFromResponse(uploadFileResponse, cancellationToken);
    }

    private async Task<Result<string>> GetDownloadUrl(Guid fileId, CancellationToken cancellationToken)
    {
         var getDownloadUrlResponse = await AppHttpClient.GetAsync(
             $"/files/download-url/{fileId}",
             cancellationToken);

         var downloadFileResult = await getDownloadUrlResponse.HandleResponseAsync<string>(cancellationToken);

         if (downloadFileResult.IsFailure)
             return Result.Failure<string>("Download url request failed");

         return downloadFileResult.Value;
    }

    private async Task<Result<Guid>> UploadFile(
        UploadFileRequest request,
        CancellationToken cancellationToken)
    {
        var fileContent = new StreamContent(request.File.OpenReadStream());
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(request.File.ContentType);
        var content = new MultipartFormDataContent
        {
            { fileContent, "file", request.File.FileName },
            { new StringContent(request.AssetType), "assetType" },
            { new StringContent(request.Context), "context" }
        };
        
        var uploadFileResponse = await AppHttpClient.PostAsync(
            "/files/upload",
            content,
            cancellationToken);

        var uploadFileResult = await uploadFileResponse.HandleResponseAsync<string>(cancellationToken);
        if (uploadFileResult.IsFailure)
            return Result.Failure<Guid>("Upload file request failed");

        if (!Guid.TryParse(uploadFileResult.Value, out Guid guid))
            return Result.Failure<Guid>("Upload endpoint returned invalid Guid");

        return guid;
    }

    private static async Task<Guid> ReadGuidFromResponse(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var uploadFileResult = await response.HandleResponseAsync<string>(cancellationToken);
        if (uploadFileResult.IsFailure)
            throw new InvalidOperationException("Upload file request failed.");

        if (!Guid.TryParse(uploadFileResult.Value, out Guid guid))
            throw new InvalidOperationException($"Upload endpoint did not return a valid Guid. Body: {uploadFileResult.Value}");

        return guid;
    }
}