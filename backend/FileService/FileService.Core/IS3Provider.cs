using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Domain;
using FileService.Domain.ValueObjects;
using Shared.Errors;

namespace FileService.Core;

public interface IS3Provider
{
    Task<UnitResult<Error>> UploadFileAsync(StorageKey storageKey, Stream stream, MediaData mediaData, CancellationToken cancellationToken);
    
    Task<Result<string, Error>> DownloadFileAsync(StorageKey storageKey, string tempPath, CancellationToken cancellationToken);
    
    Task<Result<string, Error>> GenerateUploadUrlAsync(StorageKey storageKey, MediaData mediaData, CancellationToken cancellationToken);
    
    Task<Result<IReadOnlyList<string>, Errors>> GenerateDownloadUrlsAsync(
        IEnumerable<StorageKey> storageKeys,
        CancellationToken cancellationToken);
    
    Task<Result<string, Error>> StartMultipartUploadAsync(
        string bucketName,
        string key,
        string contentType,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<string>, Error>> GenerateAllChunkUploadUrlsAsync(
        string bucketName,
        string key,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken);

    Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey storageKey);

    Task<Result<string, Error>> CompleteMultipartUploadAsync(
        string bucketName,
        string key,
        string uploadId,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken);
    
    Task<Result<string, Error>> DeleteFileAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken);

    void Dispose();
}