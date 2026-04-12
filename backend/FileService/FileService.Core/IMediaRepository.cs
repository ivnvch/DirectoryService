using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FileService.Domain;
using Shared.CommonErrors;

namespace FileService.Core;

public interface IMediaRepository
{
    Task<MediaAsset?> GetByStoragePathAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken);

    Task<Result<MediaAsset, Error>> GetBy(Expression<Func<MediaAsset, bool>> predicate,
        CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
    
    Task AddAsync(MediaAsset asset, CancellationToken cancellationToken);
}
