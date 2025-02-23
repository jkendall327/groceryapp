using GroceryApp.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GroceryApp.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ShoppingHistoryController : ControllerBase
{
    private readonly ICosmosService _cosmosService;
    private readonly ILogger<ShoppingHistoryController> _logger;

    public ShoppingHistoryController(ICosmosService cosmosService, ILogger<ShoppingHistoryController> logger)
    {
        _cosmosService = cosmosService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all past purchases for the authenticated user.
    /// </summary>
    /// <returns>List of all purchases grouped by user.</returns>
    [HttpGet]
    public async Task<ActionResult<List<PurchasedItem>>> GetShoppingHistory()
    {
        _logger.LogInformation("GetShoppingHistory called.");

        var userId = User.FindFirst("sub")?.Value; // Assuming 'sub' claim contains user ID
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("GetShoppingHistory: User ID not found.");
            return Unauthorized("User ID not found.");
        }

        _logger.LogInformation("Fetching shopping history for UserID: {UserId}.", userId);
        var shoppingHistory = await _cosmosService.GetAllPurchasesAsync(userId);
        _logger.LogInformation("GetShoppingHistory returning {Count} items.", shoppingHistory.Count);
        return Ok(shoppingHistory);
    }
}
