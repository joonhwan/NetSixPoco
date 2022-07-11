using Microsoft.AspNetCore.Http.Features;

namespace FileStore.Lib.Uploader;

public class FileStoreServiceOptions
{
    public FileStoreServiceOptions()
    {
        var defaultFormOptions = new FormOptions();
        MultipartBoundaryLengthLimit = defaultFormOptions.MultipartBoundaryLengthLimit;
        FallbackBaseDir = Environment.CurrentDirectory;
    }
    public string FallbackBaseDir { get; set; }
    public int MultipartBoundaryLengthLimit { get; set; }
    // public int ValueLengthLimit { get; set; } = int.MaxValue;
    // public int MultipartBodyLengthLimit { get; set; } = int.MaxValue;
    // public int MultipartHeadersLengthLimit { get; set; } = int.MaxValue;
}