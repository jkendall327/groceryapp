using GroceryApp.Backend.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace GroceryApp.Backend.Services;

public interface IReceiptService
{
    Task<IResult> UploadReceiptAsync(IFormFile file, ILogger logger);
}

public class ReceiptService : IReceiptService
{
    private readonly IBlobService _blobService;
    private readonly IComputerVisionService _computerVisionService;
    private readonly ILlmService _llmService;
    private readonly ICosmosService _cosmosService;
    private readonly IConfiguration _configuration;

    public ReceiptService(
        IBlobService blobService,
        IComputerVisionService computerVisionService,
        ILlmService llmService,
        ICosmosService cosmosService,
        IConfiguration configuration)
    {
        _blobService = blobService;
        _computerVisionService = computerVisionService;
        _llmService = llmService;
        _cosmosService = cosmosService;
        _configuration = configuration;
    }

    public async Task<IResult> UploadReceiptAsync(IFormFile file, ILogger logger)
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
        var fileUrl = await _blobService.UploadReceiptAsync(stream, fileName);
        logger.LogInformation("File uploaded to Blob storage at URL: {FileUrl}.", fileUrl);

        // Call Azure Computer Vision to perform OCR
        logger.LogInformation("Analyzing receipt using Computer Vision.");
        var ocrText = await _computerVisionService.AnalyzeReceiptAsync(fileUrl);
        logger.LogInformation("OCR Text extracted: {OcrTextLength} characters.", ocrText.Length);

        // Call LLM service to extract structured data
        logger.LogInformation("Extracting product information using LLM service.");
        var receiptData = await _llmService.ExtractProductInfoAsync(ocrText);
        logger.LogInformation("Extracted {ProductCount} products from receipt.", receiptData.Products.Count);

        // Optionally, store the OCR and structured data in Cosmos DB
        /*try
        {
            var userId = "sample-user-id"; // Replace with actual user ID retrieval logic
            foreach (var product in receiptData.Products)
            {
                product.UserId = userId;
                await _cosmosService.AddProductAsync(product);
            }
            logger.LogInformation("Stored extracted products in Cosmos DB for UserID: {UserId}.", userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error storing products in Cosmos DB.");
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }*/

        logger.LogInformation("UploadReceipt endpoint completed successfully.");
        return Results.Ok(new { Url = fileUrl, OCRText = ocrText, ReceiptData = receiptData });
    }
}
