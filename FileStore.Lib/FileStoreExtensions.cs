using System.Net.Mime;
using FileStore.Lib.Services;
using FileStore.Lib.Uploader;
using FileStore.Lib.Utils;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.DependencyInjection;

namespace FileStore.Lib;

public static class FileStoreExtensions
{
    public static FileStoreServiceBuilder AddFileStoreServices(this IServiceCollection services,
        Action<FileStoreServiceOptions>? options = null)
    {
        services.Configure(options);
        services.AddHttpContextAccessor();
        services.AddScoped<IMultiPartFormFileStreamingUploader, MultiPartFormFileStreamingUploader>();
        services.AddScoped<IContentTypeProvider, FileExtensionContentTypeProvider>();
        services.AddScoped<IContentTypeService, ContentTypeService>();
        services.AddScoped<IFileStoreServiceFactory, FileStoreServiceFactory>();
        return new FileStoreServiceBuilder(services);
    }

    public static IServiceCollection AddFileStoreClientServices(this IServiceCollection services)
    {
        services.AddHttpClient<IFileStoreClientServiceFactory, FileStoreClientServiceFactory>(client =>
        {
            client.BaseAddress = new Uri("http://localhost:4000");
            client.Timeout = TimeSpan.FromMinutes(60);
        });
        return services;
    }
}