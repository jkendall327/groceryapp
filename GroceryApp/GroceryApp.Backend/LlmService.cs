using GroceryApp.Backend.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GroceryApp.Backend;

public class LlmService : ILlmService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    public LlmService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    public async Task<ReceiptData> ExtractProductInfoAsync(string ocrText)
    {
        var apiKey = _configuration["OpenAI:ApiKey"];
        var apiUrl = _configuration["OpenAI:ApiUrl"]; // e.g., "https://api.openai.com/v1/completions"

        var prompt = GeneratePrompt(ocrText);

        var requestData = new
        {
            model = "text-davinci-003",
            prompt = prompt,
            max_tokens = 500,
            temperature = 0.5
        };

        var content = new StringContent(JsonSerializer.Serialize(requestData), Encoding.UTF8, "application/json");
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

        var response = await _httpClient.PostAsync(apiUrl, content);

        if (!response.IsSuccessStatusCode)
        {
            // Handle error accordingly
            return new ReceiptData
            {
                Products = new List<ProductInfo>(),
                LowConfidence = true
            };
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        var oaiResponse = JsonSerializer.Deserialize<OpenAIResponse>(responseContent);

        var extractedText = oaiResponse?.Choices?[0]?.Text?.Trim();

        if (string.IsNullOrEmpty(extractedText))
        {
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

            // Detect low confidence if any product has Confidence below threshold
            bool lowConfidence = products.Exists(p => p.Confidence < 0.7);

            return new ReceiptData
            {
                Products = products,
                LowConfidence = lowConfidence
            };
        }
        catch
        {
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
