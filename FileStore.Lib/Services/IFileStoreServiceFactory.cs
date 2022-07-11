namespace FileStore.Lib.Services;

public interface IFileStoreServiceFactory
{
    IFileStoreService CreateService(string baseDir);
}