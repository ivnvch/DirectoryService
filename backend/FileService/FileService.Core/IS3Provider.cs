using CSharpFunctionalExtensions;
using FileService.Contracts;
using Shared.Errors;

namespace FileService.Core;

public interface IS3Provider
{
    Task<Result<string, Error>> StartMultipartUploadAsync(
        string bucketName,
        string key,
        string contentType,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<string>, Error>> GenerateAllChunkUploadUrlsAsync(
        string bucketName,
        string key,
        string updoadId,
        int totalChunks,
        CancellationToken cancellationToken);

    Task<Result<string, Error>> GenerateDownloadUrlAsync(string bucketName, string key);

    Task<Result<string, Error>> CompleteMultipartUploadAsync(
        string bucketName,
        string key,
        string updoadId,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken);

    void Dispose();
}