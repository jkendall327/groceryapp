using Azure.Storage.Blobs;
using GroceryApp.Backend;
using Microsoft.Azure.Cosmos;

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

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.Run();
