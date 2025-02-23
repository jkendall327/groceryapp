using GroceryApp.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GroceryApp.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceiptsController : ControllerBase
{
    private readonly ICosmosService _cosmosService;
    private readonly ILogger<ReceiptsController> _logger;

    public ReceiptsController(ICosmosService cosmosService, ILogger<ReceiptsController> logger)
    {
        _cosmosService = cosmosService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves products expiring within the current week for the authenticated user.
    /// </summary>
    /// <returns>List of expiring products.</returns>
    [HttpGet("expiring")]
    public async Task<ActionResult<List<ProductInfo>>> GetExpiringItems()
    {
        _logger.LogInformation("GetExpiringItems called.");

        var userId = User.FindFirst("sub")?.Value; // Assuming 'sub' claim contains user ID
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("GetExpiringItems: User ID not found.");
            return Unauthorized("User ID not found.");
        }

        var today = DateTime.UtcNow.Date;
        var endOfWeek = today.AddDays(7);

        _logger.LogInformation("Fetching expiring products for UserID: {UserId} from {Today} to {EndOfWeek}.", userId, today, endOfWeek);
        var expiringProducts = await _cosmosService.GetExpiringProductsAsync(userId, today, endOfWeek);

        _logger.LogInformation("GetExpiringItems returning {Count} items.", expiringProducts.Count);
        return Ok(expiringProducts);
    }

    /// <summary>
    /// Marks selected products as used for the authenticated user.
    /// </summary>
    /// <param name="productIds">List of product IDs to mark as used.</param>
    /// <returns>Status of the operation.</returns>
    [HttpPost("mark-used")]
    public async Task<IActionResult> MarkItemsAsUsed([FromBody] List<string> productIds)
    {
        _logger.LogInformation("MarkItemsAsUsed called with {Count} product IDs.", productIds?.Count ?? 0);

        if (productIds == null || productIds.Count == 0)
        {
            _logger.LogWarning("MarkItemsAsUsed: No product IDs provided.");
            return BadRequest("No product IDs provided.");
        }

        var userId = User.FindFirst("sub")?.Value; // Assuming 'sub' claim contains user ID
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("MarkItemsAsUsed: User ID not found.");
            return Unauthorized("User ID not found.");
        }

        _logger.LogInformation("Marking {Count} products as used for UserID: {UserId}.", productIds.Count, userId);
        await _cosmosService.MarkProductsAsUsedAsync(userId, productIds);

        _logger.LogInformation("MarkItemsAsUsed: Successfully marked items as used.");
        return Ok("Selected products have been marked as used.");
    }
}
