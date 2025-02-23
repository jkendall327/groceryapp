using GroceryApp.Backend.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace GroceryApp.Backend;

public class LlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<LlmService> _logger;

    public LlmService(HttpClient httpClient, IConfiguration configuration, ILogger<LlmService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ReceiptData> ExtractProductInfoAsync(string ocrText)
    {
        _logger.LogInformation("ExtractProductInfoAsync called.");

        var apiKey = _configuration["OpenAI:ApiKey"];
        var apiUrl = _configuration["OpenAI:ApiUrl"]; // e.g., "https://api.openai.com/v1/completions"

        if (string.IsNullOrEmpty(apiKey) || string.IsNullOrEmpty(apiUrl))
        {
            _logger.LogError("OpenAI API configuration is missing.");
            throw new InvalidOperationException("OpenAI API configuration is missing.");
        }

        var prompt = GeneratePrompt(ocrText);
        _logger.LogDebug("Generated prompt for LLM: {Prompt}", prompt);

        var requestData = new
        {
            model = "text-davinci-003",
            prompt = prompt,
            max_tokens = 500,
            temperature = 0.5
        };

        var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        try
        {
            _logger.LogInformation("Sending request to OpenAI API.");
            var response = await _httpClient.PostAsync(apiUrl, content);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("OpenAI API returned non-success status code: {StatusCode}.", response.StatusCode);
                return new ReceiptData
                {
                    Products = new List<ProductInfo>(),
                    LowConfidence = true
                };
            }

            var responseContent = await response.Content.ReadAsStringAsync();
            _logger.LogDebug("Received response from OpenAI API: {ResponseContent}", responseContent);

            var oaiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

            var extractedText = oaiResponse?.Choices?[0]?.Text?.Trim();
            _logger.LogInformation("Extracted text from OpenAI API: {ExtractedTextLength} characters.", extractedText?.Length ?? 0);

            if (string.IsNullOrEmpty(extractedText))
            {
                _logger.LogWarning("No text extracted from OpenAI API response.");
                return new ReceiptData
                {
                    Products = new List<ProductInfo>(),
                    LowConfidence = true
                };
            }

            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            try
            {
                var products = JsonSerializer.Deserialize<List<ProductInfo>>(extractedText, options);
                _logger.LogInformation("Deserialized {Count} products from extracted text.", products?.Count ?? 0);

                // Detect low confidence if any product has Confidence below threshold
                bool lowConfidence = products.Exists(p => p.Confidence < 0.7);
                if (lowConfidence)
                {
                    _logger.LogWarning("Low confidence detected in some product entries.");
                }

                return new ReceiptData
                {
                    Products = products,
                    LowConfidence = lowConfidence
                };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "JSON deserialization failed for extracted text.");
                return new ReceiptData
                {
                    Products = new List<ProductInfo>(),
                    LowConfidence = true
                };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while extracting product info using LLM.");
            return new ReceiptData
            {
                Products = new List<ProductInfo>(),
                LowConfidence = true
            };
        }
    }

    private string GeneratePrompt(string ocrText)
    {
        return $"Extract structured product information from the following receipt text. Return the data in JSON format with fields: ProductName, NutritionalInfo, ShelfLife, FoodCategory, Unit, Quantity, Confidence (0 to 1), ExpirationDate.\n\nReceipt Text:\n{ocrText}";
    }

    private class OpenAIResponse
    {
        public List<Choice> Choices { get; set; }
    }

    private class Choice
    {
        public string Text { get; set; }
    }
}
