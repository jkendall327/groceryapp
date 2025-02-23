using GroceryApp.Backend.Models;

namespace GroceryApp.Backend;

public interface ILlmService
{
    Task<ReceiptData> ExtractProductInfoAsync(string ocrText);
}