using GroceryApp.Backend.Models;

namespace GroceryApp.Backend;

public interface ICosmosService
{
    Task<List<ProductInfo>> GetExpiringProductsAsync(string userId, DateTime currentDate, DateTime endDate);
    Task MarkProductsAsUsedAsync(string userId, List<string> productIds);
    Task<List<PurchasedItem>> GetAllPurchasesAsync(string userId);
    // Define other methods for interacting with Cosmos DB
}
