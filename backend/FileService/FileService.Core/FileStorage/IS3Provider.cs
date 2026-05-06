using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Contracts.MediaAssets.DTOs;
using FileService.Core.Models;
using FileService.Domain;
using FileService.Domain.ValueObjects;
using Shared.CommonErrors;

namespace FileService.Core.FileStorage;

public interface IS3Provider
{
    Task<UnitResult<Error>> UploadFileAsync(StorageKey storageKey, Stream stream, MediaData mediaData, CancellationToken cancellationToken);
    
    Task<Result<string, Error>> DownloadFileAsync(StorageKey storageKey, string tempPath, CancellationToken cancellationToken);
    
    Task<Result<string, Error>> GenerateUploadUrlAsync(StorageKey storageKey, MediaData mediaData, CancellationToken cancellationToken);
    
    Task<Result<IReadOnlyList<MediaUrl>, Errors>> GenerateDownloadUrlsAsync(
        IEnumerable<StorageKey> storageKeys,
        CancellationToken cancellationToken);
    
    Task<Result<string, Error>> StartMultipartUploadAsync(
        StorageKey storageKey,
        MediaData mediaData,
        CancellationToken cancellationToken);

    Task<Result<IReadOnlyList<ChunkUploadUrl>, Error>> GenerateAllChunkUploadUrlsAsync(
        StorageKey storageKey,
        string uploadId,
        int totalChunks,
        CancellationToken cancellationToken);
    
    Task<Result<string, Error>> GenerateChunkUploadUrlAsync(
        StorageKey storageKey,
        string uploadId,
        int partNumber,
        CancellationToken cancellationToken);

    Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey storageKey);

    Task<Result<string, Error>> CompleteMultipartUploadAsync(
        StorageKey storageKey,
        string uploadId,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken);
    
    Task<Result<string, Error>> DeleteFileAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken);
    
    Task<UnitResult<Error>> AbortMultipartUploadAsync(
        StorageKey storageKey,
        string uploadId,
        CancellationToken cancellationToken);
    
    /*Task<Result<ListMultipartUploadsResponse, Errors>> ListMultipartUploadsAsync(
        string bucketName,
        CancellationToken cancellationToken);*/

    void Dispose();
}