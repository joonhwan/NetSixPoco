using FileStore.Lib.Contracts;

namespace FileStore.Lib.Services;

public interface IFileStoreClientService : IBaseFileStoreService
{
    Task<FileUploadResult> UploadFilesAsync(string bucketId, List<string> filePaths, CancellationToken ct = default);
}