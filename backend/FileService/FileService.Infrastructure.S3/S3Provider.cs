using Amazon.S3;
using Amazon.S3.Model;
using CSharpFunctionalExtensions;
using FileService.Contracts;
using FileService.Core;
using FileService.Domain;
using FileService.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Errors;

namespace FileService.Infrastructure.S3;

public class S3Provider : IS3Provider, IDisposable
{
    private readonly IAmazonS3 _s3Client;
    private readonly S3Options _s3Options;
    private readonly ILogger<S3Provider> _logger;

    private readonly SemaphoreSlim _requestSemaphore;

    public S3Provider(
        IAmazonS3 s3Client,
        IOptions<S3Options> s3Options,
        ILogger<S3Provider> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
        _s3Options = s3Options.Value;
        _requestSemaphore = new SemaphoreSlim(_s3Options.MaxConcurrentRequests);
    }

    public async Task<UnitResult<Error>> UploadFileAsync(
        StorageKey storageKey,
        Stream stream,
        MediaData mediaData,
        CancellationToken cancellationToken)
    {
        try
        {
            if (_s3Options.RequiredBuckets.Count > 0 &&
                _s3Options.RequiredBuckets.Contains(storageKey.Location, StringComparer.OrdinalIgnoreCase) == false)
            {
                var bucketError = FileErrors.BucketNotFound(storageKey.Location);
                _logger.LogWarning(
                    "S3 upload rejected. Bucket '{BucketName}' is not configured in S3Options.RequiredBuckets.",
                    storageKey.Location);
                return bucketError;
            }

            if (stream.CanSeek)
                stream.Position = 0;

            var request = new PutObjectRequest
            {
                BucketName = storageKey.Location,
                Key = storageKey.Key,
                InputStream = stream,
                ContentType = mediaData.ContentType.Value,
                AutoCloseStream = false
            };

            await _s3Client.PutObjectAsync(request, cancellationToken);

            return UnitResult.Success<Error>();
        }
        catch (Exception ex)
        {
            Error mappedError = S3ErrorMapper.ToError(ex);

            _logger.LogError(
                ex,
                "S3 upload failed for bucket '{BucketName}', key '{Key}'. S3Code: {S3Code}. MappedCode: {MappedCode}.",
                storageKey.Location,
                storageKey.Key,
                (ex as AmazonS3Exception)?.ErrorCode ?? "n/a",
                mappedError.Messages.FirstOrDefault()?.Code ?? "unknown");

            return mappedError;
        }
    }

    public async Task<Result<string, Error>> DownloadFileAsync(
        StorageKey storageKey,
        string tempPath,
        CancellationToken cancellationToken = default)
    {
        string directoryPath = Path.Combine(Directory.GetCurrentDirectory(), tempPath);

        try
        {
            Directory.CreateDirectory(directoryPath);

            var request = new GetObjectRequest
            {
                BucketName = storageKey.Location,
                Key = storageKey.Key
            };

            using GetObjectResponse response = await _s3Client.GetObjectAsync(request, cancellationToken);

            string fileName = Path.GetFileName(storageKey.Key);
            if (string.IsNullOrWhiteSpace(fileName))
                fileName = Guid.NewGuid().ToString("N");

            string fullPath = Path.Combine(directoryPath, fileName);

            await using var fileStream = new FileStream(
                fullPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None);

            await response.ResponseStream.CopyToAsync(fileStream, cancellationToken);

            return Path.Combine(tempPath, fileName);
        }
        catch (Exception ex)
        {
            Error mappedError = S3ErrorMapper.ToError(ex);

            _logger.LogError(
                ex,
                "S3 download failed for bucket '{BucketName}', key '{Key}', tempPath '{TempPath}'. S3Code: {S3Code}. MappedCode: {MappedCode}.",
                storageKey.Location,
                storageKey.Key,
                tempPath,
                (ex as AmazonS3Exception)?.ErrorCode ?? "n/a",
                mappedError.Messages.FirstOrDefault()?.Code ?? "unknown");

            return mappedError;
        }
    }

    public async Task<Result<string, Error>> GenerateUploadUrlAsync(StorageKey storageKey, MediaData mediaData, CancellationToken cancellationToken)
    {
        GetPreSignedUrlRequest request = new()
        {
            BucketName = storageKey.Location,
            Key = storageKey.Key,
            Verb = HttpVerb.PUT,
            ContentType = mediaData.ContentType.Value,
            Expires = DateTime.UtcNow.AddHours(_s3Options.UploadUrlExpirationHours),
            Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP
        };

        try
        {
            return await _s3Client.GetPreSignedURLAsync(request);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<IReadOnlyList<string>, Errors>> GenerateDownloadUrlsAsync(IEnumerable<StorageKey> storageKeys, CancellationToken cancellationToken)
    {
        IEnumerable<Task<Result<string, Error>>> tasks = storageKeys.Select(async key =>
        {
            await _requestSemaphore.WaitAsync(cancellationToken);

            try
            {
                return await GenerateDownloadUrlAsync(key);
            }
            finally
            {
                _requestSemaphore.Release();
            }
        });

        Result<string, Error>[] downloadUrlsResult = await Task.WhenAll(tasks);
        
        Error[] errors = downloadUrlsResult
            .Where(x => x.IsFailure)
            .Select(x => x.Error)
            .ToArray();

        if (errors.Any())
            return new Errors(errors);
        
        return downloadUrlsResult.Select(x => x.Value).ToList();
    }

    public async Task<Result<string, Error>> StartMultipartUploadAsync(
        string bucketName,
        string key,
        string contentType,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new InitiateMultipartUploadRequest()
            {
                BucketName = bucketName, Key = key, ContentType = contentType
            };

            InitiateMultipartUploadResponse result =
                await _s3Client.InitiateMultipartUploadAsync(request, cancellationToken);

            return result.UploadId;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting multipart upload");
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<IReadOnlyList<string>, Error>> GenerateAllChunkUploadUrlsAsync(
        string bucketName,
        string key,
        string updoadId,
        int totalChunks,
        CancellationToken cancellationToken)
    {
        try
        {
            IEnumerable<Task<string>> tasks = Enumerable.Range(1, totalChunks)
                .Select(async partNumber =>
                {
                    await _requestSemaphore.WaitAsync(cancellationToken);

                    try
                    {
                        var request = new GetPreSignedUrlRequest
                        {
                            BucketName = bucketName, Key = key, Verb = HttpVerb.PUT,
                            UploadId = updoadId,
                            PartNumber = partNumber,
                            Expires = DateTime.UtcNow.AddHours(_s3Options.UploadUrlExpirationHours),
                            Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP
                        };

                        string? url = await _s3Client.GetPreSignedURLAsync(request);

                        return url;
                    }
                    finally
                    {
                        _requestSemaphore.Release();
                    }
                });

            string[] result = await Task.WhenAll(tasks);

            return result;
        }
        catch (Exception ex)
        {
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> GenerateDownloadUrlAsync(StorageKey storageKey)
    {
        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = storageKey.Location, Key = storageKey.Key, Verb = HttpVerb.GET,
                Expires = DateTime.UtcNow.AddHours(_s3Options.DownloadUrlExpirationHours),
                Protocol = _s3Options.WithSsl ? Protocol.HTTPS : Protocol.HTTP
            };

            string? response = await _s3Client.GetPreSignedURLAsync(request);

            return response;
        }
        catch (Exception ex)
        {
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> CompleteMultipartUploadAsync(
        string bucketName,
        string key,
        string updoadId,
        IReadOnlyList<PartETagDto> partETags,
        CancellationToken cancellationToken)
    {
        try
        {
            var request = new CompleteMultipartUploadRequest()
            {
                BucketName = bucketName,
                Key = key,
                UploadId = updoadId,
                PartETags = partETags.Select(p => new PartETag
                {
                    ETag = p.ETag, PartNumber = p.PartNumber 
                    
                }).ToList(),
            };

            CompleteMultipartUploadResponse response =
                await _s3Client.CompleteMultipartUploadAsync(request, cancellationToken);

            return response.Key;
        }
        catch (Exception ex)
        {
            return S3ErrorMapper.ToError(ex);
        }
    }

    public async Task<Result<string, Error>> DeleteFileAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken)
    {
        try
        {
           var result = await _s3Client.DeleteObjectAsync(
               bucketName, objectKey, cancellationToken);
           
           _logger.LogInformation(
               "Deleted file from bucket '{BucketName}' with key '{ObjectKey}'",
               bucketName,
               objectKey);

           return result.DeleteMarker;
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error deleting file from bucket '{BucketName}' with key '{ObjectKey}'",
                bucketName,
                objectKey);
            return S3ErrorMapper.ToError(ex);
        }
    }

    public void Dispose()
    {
        /*_s3Client.Dispose();*/
        _requestSemaphore.Release();
        _requestSemaphore.Dispose();
    }
}