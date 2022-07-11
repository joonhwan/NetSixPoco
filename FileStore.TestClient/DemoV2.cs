using FileStore.Lib.Services;

namespace FileStore.TestClient;

public class DemoV2
{
    private readonly IFileStoreClientServiceFactory _factory;

    public DemoV2(IFileStoreClientServiceFactory factory)
    {
        _factory = factory;
    }
    
    public async Task RunAsync()
    {
        try
        {
            await RunImplAsync();  
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    public async Task RunImplAsync()
    {
        var service = _factory.CreateService("/api/v1/image-files");
        
        var bucketId = "first_upload";
        
        var buckets = await service.GetBucketsAsync();
        Console.WriteLine("Buckets : {0}", buckets.ToString());

        var files = await service.GetFilesAsync(bucketId);
        Console.WriteLine("Bucket(={0}) Files : ", bucketId);
        foreach (var file in files.StoredFileNames)
        {
            Console.WriteLine("   {0}", file);
        }
        
        var filePaths = new List<string>
        {
            "/Users/vine/Downloads/jquery-handling-form-events.zip",
            // "/Users/vine/Downloads/KakaoTalk_Photo_2022-05-31-16-33-12 001.jpeg",
            // "/Users/vine/Downloads/KakaoTalk_Photo_2022-05-31-16-33-13 002.jpeg",
            // "/Users/vine/Downloads/KakaoTalk_Photo_2022-05-31-16-33-13 003.jpeg",
            // "/Users/vine/Downloads/KakaoTalk_Photo_2022-05-31-16-35-53.jpeg",
            // "/Users/vine/Downloads/KakaoTalk_Photo_2022-05-31-16-44-08.jpg",
            "/Users/vine/Downloads/Massive.Soundbanks.Big.Collection.2019.dat",
        };

        bucketId = $"{DateTime.Now:yyyyMMdd_HHmmSS}";
        var result = await service.UploadFilesAsync(bucketId, filePaths);
        Console.WriteLine("Result : {0}", result);

        
    }
}