namespace FileStore.TestClient;

public class DemoV1
{
    private static readonly HttpClient _httpClient = new HttpClient()
    {
        BaseAddress = new Uri("http://localhost:4000"),
        Timeout = TimeSpan.FromMinutes(30)
    };

    public static async Task RunAsync()
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
    
    public static async Task RunImplAsync()
    {
        var progressPrinter = new ConsoleProgressPrinter();
        var filePaths = new string[]
        {
            "/Users/vine/Downloads/jquery-handling-form-events.zip",
            // "/Users/vine/Downloads/KakaoTalk_Photo_2022-05-31-16-33-12 001.jpeg",
            // "/Users/vine/Downloads/KakaoTalk_Photo_2022-05-31-16-33-13 002.jpeg",
            // "/Users/vine/Downloads/KakaoTalk_Photo_2022-05-31-16-33-13 003.jpeg",
            // "/Users/vine/Downloads/KakaoTalk_Photo_2022-05-31-16-35-53.jpeg",
            // "/Users/vine/Downloads/KakaoTalk_Photo_2022-05-31-16-44-08.jpg",
            "/Users/vine/Downloads/Massive.Soundbanks.Big.Collection.2019.dat",
        };

        //var urlPath = "api/filestore/upload";
        var urlPath = "api/streaming/upload";
        var request = new HttpRequestMessage(HttpMethod.Post, urlPath);
        var formData = new MultipartFormDataContent();
        foreach (var filePath in filePaths)
        {
            var fileName = Path.GetFileName(filePath);
            // var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileStream = File.OpenRead(filePath);
            // var content = new ProgressableStreamContent(fileStream, 4*1024, progressPrinter); // FileStream 이 알아서 Dispose됨.
            var content = new StreamContent(fileStream);
            content.Headers.Add("X-Request-ID", Guid.NewGuid().ToString("N"));
            content.Headers.ContentLength = new FileInfo(filePath).Length;
            formData.Add(content, "files", fileName);
        }
        request.Content = formData;
        
        var response = await _httpClient.SendAsync(request);
        
        Console.Write(response);
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine(json);
        }
        
    }
}

public class ConsoleProgressPrinter : IProgress<UploadProgress>
{
    public void Report(UploadProgress value)
    {
        Console.WriteLine(value);
    }
}