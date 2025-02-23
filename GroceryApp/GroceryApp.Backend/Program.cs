using GroceryApp.Backend;
using Microsoft.Extensions.Logging;

var builder = WebApplication.CreateBuilder(args);

// Configure logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Add services to the container.
builder.Services.AddCustomServices(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.Logger.LogInformation("Application is running in Development mode.");
}
else
{
    app.Logger.LogInformation("Application is running in Production mode.");
}

app.UseHttpsRedirection();

// Existing ReceiptsController endpoints
app.MapPost("/api/upload", async (IFormFile file, IBlobService blobService, IComputerVisionService computerVisionService, ILlmService llmService, ILogger<Program> logger) =>
{
    logger.LogInformation("UploadReceipt endpoint called.");

    if (file == null || file.Length == 0)
    {
        logger.LogWarning("No file uploaded.");
        return Results.BadRequest("No file uploaded.");
    }

    var fileName = Path.GetFileName(file.FileName);
    logger.LogInformation("Uploading file: {FileName}.", fileName);
    await using var stream = file.OpenReadStream();
    var fileUrl = await blobService.UploadReceiptAsync(stream, fileName);
    logger.LogInformation("File uploaded to Blob storage at URL: {FileUrl}.", fileUrl);

    // Call Azure Computer Vision to perform OCR
    logger.LogInformation("Analyzing receipt using Computer Vision.");
    var ocrText = await computerVisionService.AnalyzeReceiptAsync(fileUrl);
    logger.LogInformation("OCR Text extracted: {OcrTextLength} characters.", ocrText.Length);

    // Call LLM service to extract structured data
    logger.LogInformation("Extracting product information using LLM service.");
    var receiptData = await llmService.ExtractProductInfoAsync(ocrText);
    logger.LogInformation("Extracted {ProductCount} products from receipt.", receiptData.Products.Count);

    // Optionally, you can store the OCR and structured data in Cosmos DB here using ICosmosService
    // var cosmosService = app.Services.GetRequiredService<ICosmosService>();
    // await cosmosService.StoreReceiptDataAsync(new ReceiptData { ... });

    logger.LogInformation("UploadReceipt endpoint completed successfully.");
    return Results.Ok(new { Url = fileUrl, OCRText = ocrText, ReceiptData = receiptData });
})
.WithName("UploadReceipt")
.Accepts<IFormFile>("multipart/form-data")
.Produces(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest);

app.Run();
