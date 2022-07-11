using FileStore.Lib.Contracts;

namespace FileStore.Lib.Services;

public interface IBaseFileStoreService
{
    Task<BucketList> GetBucketsAsync();
    Task<FileList> GetFilesAsync(string bucketId);
    Task<FileDownloadInfo> GetFileInfoAsync(string bucketId, string filePath);
}