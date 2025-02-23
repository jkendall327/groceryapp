using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;
using GroceryApp.Backend.Models;

namespace GroceryApp.Backend;

public class CosmosService : ICosmosService
{
    private readonly CosmosClient _cosmosClient;
    private readonly Container _container;
    private readonly ILogger<CosmosService> _logger;

    public CosmosService(CosmosClient cosmosClient, ILogger<CosmosService> logger)
    {
        _cosmosClient = cosmosClient;
        _container = _cosmosClient.GetContainer("GroceryAppDatabase", "ItemsContainer");
        _logger = logger;
    }

    public async Task<List<ProductInfo>> GetExpiringProductsAsync(string userId, DateTime currentDate, DateTime endDate)
    {
        _logger.LogInformation("Fetching expiring products for UserID: {UserId} between {CurrentDate} and {EndDate}.", userId, currentDate, endDate);

        var query = new QueryDefinition(
                "SELECT c.id, c.productName, c.nutritionalInfo, c.shelfLife, c.foodCategory, c.unit, c.quantity, c.confidence, c.expirationDate, c.isUsed " +
                "FROM c WHERE c.UserId = @userId AND c.expirationDate >= @currentDate AND c.expirationDate <= @endDate AND c.isUsed = false"
            )
            .WithParameter("@userId", userId)
            .WithParameter("@currentDate", currentDate)
            .WithParameter("@endDate", endDate);

        var iterator = _container.GetItemQueryIterator<ProductInfo>(query);
        var results = new List<ProductInfo>();

        try
        {
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            _logger.LogInformation("Fetched {Count} expiring products for UserID: {UserId}.", results.Count, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching expiring products for UserID: {UserId}.", userId);
            throw;
        }

        return results.OrderByDescending(p => p.ExpirationDate).ToList();
    }

    public async Task MarkProductsAsUsedAsync(string userId, List<string> productIds)
    {
        _logger.LogInformation("Marking {Count} products as used for UserID: {UserId}.", productIds.Count, userId);

        foreach (var id in productIds)
        {
            try
            {
                _logger.LogDebug("Processing ProductID: {ProductId}.", id);
                var response = await _container.ReadItemAsync<ProductInfo>(id, new PartitionKey(id));
                var product = response.Resource;

                if (product.UserId != userId)
                {
                    _logger.LogWarning("Unauthorized attempt to modify ProductID: {ProductId} by UserID: {UserId}.", id, userId);
                    continue;
                }

                product.IsUsed = true;
                await _container.UpsertItemAsync(product, new PartitionKey(product.Id));
                _logger.LogInformation("ProductID: {ProductId} marked as used.", id);
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                _logger.LogWarning("ProductID: {ProductId} not found in Cosmos DB.", id);
                // Handle not found if necessary
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking ProductID: {ProductId} as used for UserID: {UserId}.", id, userId);
                // Optionally, decide whether to continue or rethrow
            }
        }

        _logger.LogInformation("Completed marking products as used for UserID: {UserId}.", userId);
    }

    public async Task<List<PurchasedItem>> GetAllPurchasesAsync(string userId)
    {
        _logger.LogInformation("Fetching all purchases for UserID: {UserId}.", userId);

        var query = new QueryDefinition(
                "SELECT c.id, c.productName, c.nutritionalInfo, c.shelfLife, c.foodCategory, c.unit, c.quantity, c.confidence, c.expirationDate, c.isUsed " +
                "FROM c WHERE c.UserId = @userId"
            )
            .WithParameter("@userId", userId);

        var iterator = _container.GetItemQueryIterator<PurchasedItem>(query);
        var results = new List<PurchasedItem>();

        try
        {
            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            _logger.LogInformation("Fetched {Count} purchased items for UserID: {UserId}.", results.Count, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching shopping history for UserID: {UserId}.", userId);
            throw;
        }

        return results;
    }
}
