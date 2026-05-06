using CSharpFunctionalExtensions;
using Shared.CommonErrors;

namespace FileService.Core.FileStorage;

public interface IChunkSizeCalculator
{
    Result<(int ChunkSize, int TotalChunks), Error> CalculateChunkSize(
        long fileSiz);
}