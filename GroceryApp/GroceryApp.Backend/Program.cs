using Azure.Storage.Blobs;
using GroceryApp.Backend;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Rest;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Configure Cosmos DB
builder.Services.AddSingleton<CosmosClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var cosmosConfig = configuration.GetSection("CosmosDb");
    return new CosmosClient(cosmosConfig["Account"], cosmosConfig["Key"]);
});

builder.Services.AddSingleton<ICosmosService, CosmosService>();

// Configure Azure Blob Storage
builder.Services.AddSingleton<BlobServiceClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var blobConfig = configuration.GetSection("AzureBlobStorage");
    return new BlobServiceClient(blobConfig["ConnectionString"]);
});

builder.Services.AddSingleton<IBlobService, BlobService>();

// Configure Azure Computer Vision
builder.Services.AddSingleton<ComputerVisionClient>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var visionConfig = configuration.GetSection("ComputerVision");
    return new ComputerVisionClient(new ApiKeyServiceClientCredentials(visionConfig["SubscriptionKey"]))
    {
        Endpoint = visionConfig["Endpoint"]
    };
});

builder.Services.AddSingleton<IComputerVisionService, ComputerVisionService>();

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
    // Define methods for interacting with Azure Blob Storage
}

public class BlobService : IBlobService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobService(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
        // Initialize Blob containers, etc.
    }

    // Implement methods for IBlobService
}

public interface IComputerVisionService
{
    // Define methods for interacting with Azure Computer Vision
}

public class ComputerVisionService : IComputerVisionService
{
    private readonly ComputerVisionClient _computerVisionClient;

    public ComputerVisionService(ComputerVisionClient computerVisionClient)
    {
        _computerVisionClient = computerVisionClient;
        // Initialize Computer Vision client, etc.
    }

    // Implement methods for IComputerVisionService
}
