using Azure.Storage.Blobs;
using GroceryApp.Backend;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Rest;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddCustomServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast",
        () =>
        {
            var forecast = Enumerable.Range(1, 5)
                .Select(index => new WeatherForecast(DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]))
                .ToArray();
            return forecast;
        })
    .WithName("GetWeatherForecast");

// New API Endpoint for Uploading Receipt Images
app.MapPost("/api/upload", async (IFormFile file, IBlobService blobService, IComputerVisionService computerVisionService) =>
{
    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("No file uploaded.");
    }

    var fileName = Path.GetFileName(file.FileName);
    using var stream = file.OpenReadStream();
    var fileUrl = await blobService.UploadReceiptAsync(stream, fileName);

    // Call Azure Computer Vision to perform OCR
    var ocrText = await computerVisionService.AnalyzeReceiptAsync(fileUrl);

    // Optionally, you can store the OCR result in Cosmos DB here using ICosmosService
    // var cosmosService = app.Services.GetRequiredService<ICosmosService>();
    // await cosmosService.StoreOcrResultAsync(new OcrResult { FileUrl = fileUrl, Text = ocrText });

    return Results.Ok(new { Url = fileUrl, OCRText = ocrText });
})
.WithName("UploadReceipt")
.Accepts<IFormFile>("multipart/form-data")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public interface ICosmosService
{
    // Define methods for interacting with Cosmos DB
}

public class CosmosService : ICosmosService
{
    private readonly CosmosClient _cosmosClient;

    public CosmosService(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
        // Initialize Cosmos DB containers, etc.
    }

    // Implement methods for ICosmosService
}

public interface IBlobService
{
    Task<string> UploadReceiptAsync(Stream fileStream, string fileName);
}

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName = "receipts";

    public BlobService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
        InitializeContainer();
    }

    private void InitializeContainer()
    {
        var container = _blobServiceClient.GetBlobContainerClient(_containerName);
        container.CreateIfNotExists();
    }

    public async Task<string> UploadReceiptAsync(Stream fileStream, string fileName)
    {
        var container = _blobServiceClient.GetBlobContainerClient(_containerName);
        var blobClient = container.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, overwrite: true);
        return blobClient.Uri.ToString();
    }
}

public interface IComputerVisionService
{
    Task<string> AnalyzeReceiptAsync(string imageUrl);
}

public class ComputerVisionService : IComputerVisionService
{
    private readonly ComputerVisionClient _computerVisionClient;

    public ComputerVisionService(ComputerVisionClient computerVisionClient)
    {
        _computerVisionClient = computerVisionClient;
        // Initialize Computer Vision client, etc.
    }

    public async Task<string> AnalyzeReceiptAsync(string imageUrl)
    {
        var ocrResult = await _computerVisionClient.RecognizePrintedTextAsync(true, imageUrl);
        
        if (ocrResult == null || ocrResult.Regions == null)
        {
            return "No text detected.";
        }

        var extractedText = string.Empty;

        foreach (var region in ocrResult.Regions)
        {
            foreach (var line in region.Lines)
            {
                foreach (var word in line.Words)
                {
                    extractedText += word.Text + " ";
                }
                extractedText += "\n";
            }
        }

        return extractedText.Trim();
    }
}
