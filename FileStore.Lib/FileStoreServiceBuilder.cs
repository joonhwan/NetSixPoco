using FileStore.Lib.Uploader;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FileStore.Lib;

public class FileStoreServiceBuilder
{
    private readonly IServiceCollection _services;

    public FileStoreServiceBuilder(IServiceCollection services)
    {
        _services = services;
    }

    public FileStoreServiceBuilder UseDummyNotifier()
    {
        _services.TryAddSingleton<IProgressEventNotifier>(new DummyProgressEventNotifier());
        return this;
    }

    // public FileStoreServiceBuilder UseWebSocketProgressEventNotifier(Action<WebSocketProgressEventOption>? options = null)
    // {
    //     // TODO    
    //     
    //     return this;
    // }
}