using FileStore.Lib.Services;
using FileStore.Lib.Uploader;
using FileStore.Lib.Utils;
using Microsoft.AspNetCore.Mvc;

namespace FileStore.Api.Controllers;

// /api/v1/image-files
// /api/v1/image-files
[Route("/api/v1/image-files")]
public class ImageFileStoreController : BaseFileStoreController
{
    public ImageFileStoreController(IHostEnvironment env, IFileStoreServiceFactory fileStoreServiceFactory)
    {
        var baseDir = Path.Combine(env.ContentRootPath, "Store/ImageFiles");
        FileStoreService = fileStoreServiceFactory.CreateService(baseDir);
    }

    protected sealed override IFileStoreService FileStoreService { get; set; }
}