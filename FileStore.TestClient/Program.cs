// See https://aka.ms/new-console-template for more information

//Console.WriteLine("Hello, World!");

using FileStore.Lib;
using FileStore.Lib.Services;
using FileStore.TestClient;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
services.AddFileStoreClientServices();
var provider = services.BuildServiceProvider();


// await DemoV1.RunAsync();
var factory = provider.GetRequiredService<IFileStoreClientServiceFactory>();
var demo = new DemoV2(factory);
await demo.RunAsync();