using FileStore.Api.Utils;
using FileStore.Lib;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var config = builder.Configuration;
var services = builder.Services;
{
    services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    services.AddEndpointsApiExplorer();
    services.AddSwaggerGen(o => { o.DocumentFilter<AddStreamingFormRequestModificationDocumentFilter>(); });
    services.Configure<KestrelServerOptions>(o => { o.Limits.MaxRequestBodySize = 4L * 1024 * 1024 * 1024; });
    services.Configure<FormOptions>(o =>
    {
        o.ValueLengthLimit = int.MaxValue;
        o.MultipartBodyLengthLimit = int.MaxValue;
        o.MultipartHeadersLengthLimit = int.MaxValue;
    });
    services
        .AddFileStoreServices(options =>
        {
            options.FallbackBaseDir = Path.Combine(builder.Environment.ContentRootPath, "Store");
        })
        .UseDummyNotifier()
        ;
}

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();
// app.MapFallback(context =>
// {
//     context.Response.Redirect("/swagger");
//     return Task.CompletedTask;
// });
app.UseStaticFiles();

app.Run();