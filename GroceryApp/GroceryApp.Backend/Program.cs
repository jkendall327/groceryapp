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

// Existing ReceiptsController endpoints
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
