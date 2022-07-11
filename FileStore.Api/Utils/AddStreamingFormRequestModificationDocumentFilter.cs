using Microsoft.OpenApi.Extensions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace FileStore.Api.Utils;

public class AddStreamingFormRequestModificationDocumentFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        var fileUpload = swaggerDoc.Paths.FirstOrDefault(x => x.Key.ToLower() == "/api/filestore/upload").Value;
        var streamUpload = swaggerDoc.Paths.FirstOrDefault(x => x.Key.ToLower() == "/api/streaming/upload").Value;
        
        {
            streamUpload.Operations[OperationType.Post] = fileUpload.Operations[OperationType.Post];
        }
        // var operation = streamUpload.Value.Operations[OperationType.Post];
        // operation.RequestBody = new OpenApiRequestBody
        // {
        //     Content = new Dictionary<string, OpenApiMediaType>
        //     {
        //         ["multipart/form-data"] = new OpenApiMediaType
        //         {
        //             Encoding = new Dictionary<string, OpenApiEncoding>
        //             {
        //                 ["files"] = new OpenApiEncoding
        //                 {
        //                     ContentType = "*/*"
        //                 }
        //             },
        //             Schema = new OpenApiSchema()
        //             {
        //                 Properties = new Dictionary<string, OpenApiSchema>
        //                 {
        //                     ["files"] = new OpenApiSchema()
        //                 }
        //             }
        //         }
        //     },
        //     
        // };
        // var paths = swaggerDoc.Paths.ToDictionary(x => x.Key, x => x.Value);
        // var newPaths = new OpenApiPaths();
        // Console.WriteLine(newPaths?.Count);
    }
}