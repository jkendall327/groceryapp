using GroceryApp.Backend.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GroceryApp.Backend
{
    public interface ICosmosService
    {
        Task<List<ProductInfo>> GetExpiringProductsAsync(DateTime currentDate, DateTime endDate);
        Task MarkProductsAsUsedAsync(List<string> productIds);
        // Define other methods for interacting with Cosmos DB
    }
}
