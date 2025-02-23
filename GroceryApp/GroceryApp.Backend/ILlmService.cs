using GroceryApp.Backend.Models;
using System.Threading.Tasks;

namespace GroceryApp.Backend;

public interface ILlmService
{
    Task<ReceiptData> ExtractProductInfoAsync(string ocrText);
}