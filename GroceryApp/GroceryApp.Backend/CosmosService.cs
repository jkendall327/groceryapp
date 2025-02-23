using Microsoft.Azure.Cosmos;
using GroceryApp.Backend.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GroceryApp.Backend
{
    public class CosmosService : ICosmosService
    {
        private readonly CosmosClient _cosmosClient;
        private readonly Container _container;

        public CosmosService(CosmosClient cosmosClient)
        {
            _cosmosClient = cosmosClient;
            _container = _cosmosClient.GetContainer("GroceryAppDatabase", "ItemsContainer");
        }

        public async Task<List<ProductInfo>> GetExpiringProductsAsync(string userId, DateTime currentDate, DateTime endDate)
        {
            var query = new QueryDefinition(
                    "SELECT c.id, c.productName, c.nutritionalInfo, c.shelfLife, c.foodCategory, c.unit, c.quantity, c.confidence, c.expirationDate, c.isUsed " +
                    "FROM c WHERE c.UserId = @userId AND c.expirationDate >= @currentDate AND c.expirationDate <= @endDate AND c.isUsed = false"
                )
                .WithParameter("@userId", userId)
                .WithParameter("@currentDate", currentDate)
                .WithParameter("@endDate", endDate);

            var iterator = _container.GetItemQueryIterator<ProductInfo>(query);
            var results = new List<ProductInfo>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results.OrderByDescending(p => p.ExpirationDate).ToList();
        }

        public async Task MarkProductsAsUsedAsync(string userId, List<string> productIds)
        {
            foreach (var id in productIds)
            {
                try
                {
                    var response = await _container.ReadItemAsync<ProductInfo>(id, new PartitionKey(id));
                    var product = response.Resource;

                    if (product.UserId != userId)
                    {
                        // Optionally, handle unauthorized access attempts
                        continue;
                    }

                    product.IsUsed = true;
                    await _container.UpsertItemAsync(product, new PartitionKey(product.Id));
                }
                catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Handle not found if necessary
                }
            }
        }

        public async Task<List<PurchasedItem>> GetAllPurchasesAsync(string userId)
        {
            var query = new QueryDefinition(
                    "SELECT c.id, c.productName, c.nutritionalInfo, c.shelfLife, c.foodCategory, c.unit, c.quantity, c.confidence, c.expirationDate, c.isUsed " +
                    "FROM c WHERE c.UserId = @userId"
                )
                .WithParameter("@userId", userId);

            var iterator = _container.GetItemQueryIterator<PurchasedItem>(query);
            var results = new List<PurchasedItem>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            return results;
        }
    }
}
