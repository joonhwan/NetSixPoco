namespace FileStore.Lib.Services;

public interface IFileStoreClientServiceFactory
{
    IFileStoreClientService CreateService(string baseUrlPath);
}