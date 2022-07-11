namespace FileStore.Lib.Contracts;

public class FileUploadResult
{
    public List<FileUploadEntry> Entries { get; set; } = new List<FileUploadEntry>();
}