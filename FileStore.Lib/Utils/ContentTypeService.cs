using Microsoft.AspNetCore.StaticFiles;

namespace FileStore.Lib.Utils;

public interface IContentTypeService
{
    string Map(string fileName);
}

public class ContentTypeService : IContentTypeService
{
    private readonly IContentTypeProvider _contentTypeProvider;

    public ContentTypeService(IContentTypeProvider contentTypeProvider)
    {
        _contentTypeProvider = contentTypeProvider;
    }

    public string Map(string fileName)
    {
        if (!_contentTypeProvider.TryGetContentType(fileName, out var contentType))
        {
            contentType = "application/octet-stream";
        }

        return contentType;
    }
}