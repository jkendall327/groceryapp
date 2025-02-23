using GroceryApp.Backend;

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

// New API Endpoint for Uploading Receipt Images
app.MapPost("/api/upload", async (IFormFile file, IBlobService blobService, IComputerVisionService computerVisionService, ILlmService llmService) =>
{
    if (file == null || file.Length == 0)
    {
        return Results.BadRequest("No file uploaded.");
    }

    var fileName = Path.GetFileName(file.FileName);
    await using var stream = file.OpenReadStream();
    var fileUrl = await blobService.UploadReceiptAsync(stream, fileName);

    // Call Azure Computer Vision to perform OCR
    var ocrText = await computerVisionService.AnalyzeReceiptAsync(fileUrl);

    // Call LLM service to extract structured data
    var receiptData = await llmService.ExtractProductInfoAsync(ocrText);

    // Optionally, you can store the OCR and structured data in Cosmos DB here using ICosmosService
    // var cosmosService = app.Services.GetRequiredService<ICosmosService>();
    // await cosmosService.StoreReceiptDataAsync(new ReceiptData { ... });

    return Results.Ok(new { Url = fileUrl, OCRText = ocrText, ReceiptData = receiptData });
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
    Task<List<ProductInfo>> GetExpiringProductsAsync(DateTime currentDate, DateTime endDate);
    Task MarkProductsAsUsedAsync(List<string> productIds);
    // Define other methods for interacting with Cosmos DB
}

public class CosmosService : ICosmosService
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;

    public CosmosService(CosmosClient cosmosClient)
    {
        _cosmosClient = cosmosClient;
        _container = _cosmosClient.GetContainer("GroceryAppDatabase", "ItemsContainer");
    }

    public async Task<List<ProductInfo>> GetExpiringProductsAsync(DateTime currentDate, DateTime endDate)
    {
        var query = new QueryDefinition(
            "SELECT c.id, c.productName, c.nutritionalInfo, c.shelfLife, c.foodCategory, c.unit, c.quantity, c.confidence, c.expirationDate, c.isUsed " +
            "FROM c WHERE c.expirationDate >= @currentDate AND c.expirationDate <= @endDate AND c.isUsed = false"
        )
        .WithParameter("@currentDate", currentDate)
        .WithParameter("@endDate", endDate);

        var iterator = _container.GetItemQueryIterator<ProductInfo>(query);
        var results = new List<ProductInfo>();

        while (iterator.HasMoreResults)
        {
            var response = await iterator.ReadNextAsync();
            results.AddRange(response.ToList());
        }

        return results.OrderByDescending(p => p.ExpirationDate).ToList();
    }

    public async Task MarkProductsAsUsedAsync(List<string> productIds)
    {
        foreach (var id in productIds)
        {
            try
            {
                var response = await _container.ReadItemAsync<ProductInfo>(id, new PartitionKey(id));
                var product = response.Resource;
                product.IsUsed = true;
                await _container.UpsertItemAsync(product, new PartitionKey(product.Id));
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // Handle not found if necessary
            }
        }
    }

    // Implement other methods for ICosmosService as needed
}
