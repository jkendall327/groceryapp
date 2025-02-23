using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Extensions.Logging;

namespace GroceryApp.Backend;

public class ComputerVisionService : IComputerVisionService
{
    private readonly ComputerVisionClient _computerVisionClient;
    private readonly ILogger<ComputerVisionService> _logger;

    public ComputerVisionService(ComputerVisionClient computerVisionClient, ILogger<ComputerVisionService> logger)
    {
        _computerVisionClient = computerVisionClient;
        _logger = logger;
    }

    public async Task<string> AnalyzeReceiptAsync(string imageUrl)
    {
        _logger.LogInformation("Analyzing receipt at URL: {ImageUrl}.", imageUrl);
        try
        {
            var ocrResult = await _computerVisionClient.RecognizePrintedTextAsync(true, imageUrl);

            if (ocrResult == null || ocrResult.Regions == null)
            {
                _logger.LogWarning("No text detected in the image: {ImageUrl}.", imageUrl);
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

            _logger.LogInformation("OCR extraction completed with {Length} characters.", extractedText.Length);
            return extractedText.Trim();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while analyzing receipt at URL: {ImageUrl}.", imageUrl);
            return "Error during OCR processing.";
        }
    }
}
