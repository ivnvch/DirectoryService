using System.Linq.Expressions;
using CSharpFunctionalExtensions;
using FileService.Core;
using FileService.Domain;
using Microsoft.EntityFrameworkCore;
using Shared.CommonErrors;

namespace FileService.Infrastructure.Postgres.Repositiories;

public sealed class MediaRepository : IMediaRepository
{
    private readonly FileServiceDbContext _dbContext;

    public MediaRepository(FileServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<MediaAsset?> GetByStoragePathAsync(
        string bucketName,
        string objectKey,
        CancellationToken cancellationToken)
    {
        return _dbContext.MediaAssets
            .FirstOrDefaultAsync(
                asset =>
                    (asset.Key.Location == bucketName && asset.Key.Key == objectKey) ||
                    (asset.FinalKey.Location == bucketName && asset.FinalKey.Key == objectKey),
                cancellationToken);
    }

    public async Task<Result<MediaAsset, Error>> GetBy(Expression<Func<MediaAsset, bool>> predicate, CancellationToken cancellationToken)
    {
        MediaAsset? asset = await _dbContext.MediaAssets.FirstOrDefaultAsync(predicate, cancellationToken);

        if (asset is null)
            return GeneralErrors.NotFound(null, "mediaAsset");

        return asset;
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
        => _dbContext.SaveChangesAsync(cancellationToken);

    public async  Task AddAsync(MediaAsset asset, CancellationToken cancellationToken)
    {
        await _dbContext.MediaAssets.AddAsync(asset, cancellationToken);
    }
}
