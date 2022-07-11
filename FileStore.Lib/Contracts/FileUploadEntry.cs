namespace FileStore.Lib.Contracts;

public class FileUploadEntry
{
    public FileUploadEntry(string requestedFileName, string storedFileName, long fileSizeBytes, string storedFilePath)
    {
        RequestedFileName = requestedFileName;
        StoredFilePath = storedFilePath;
        FileSizeBytes = fileSizeBytes;
        StoredFileName = storedFileName;
    }

    public string RequestedFileName { get; set; }
    public string StoredFileName { get; set; }
    public long FileSizeBytes { get; set; }

    // Just For Debugging
    public string StoredFilePath { get; set; }
}