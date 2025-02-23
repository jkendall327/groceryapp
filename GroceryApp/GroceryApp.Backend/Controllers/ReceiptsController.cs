using GroceryApp.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GroceryApp.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReceiptsController : ControllerBase
{
    private readonly ICosmosService _cosmosService;

    public ReceiptsController(ICosmosService cosmosService)
    {
        _cosmosService = cosmosService;
    }

    /// <summary>
    /// Retrieves products expiring within the current week for the authenticated user.
    /// </summary>
    /// <returns>List of expiring products.</returns>
    [HttpGet("expiring")]
    public async Task<ActionResult<List<ProductInfo>>> GetExpiringItems()
    {
        var userId = User.FindFirst("sub")?.Value; // Assuming 'sub' claim contains user ID
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found.");
        }

        var today = DateTime.UtcNow.Date;
        var endOfWeek = today.AddDays(7);

        var expiringProducts = await _cosmosService.GetExpiringProductsAsync(userId, today, endOfWeek);

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
        if (productIds == null || productIds.Count == 0)
        {
            return BadRequest("No product IDs provided.");
        }

        var userId = User.FindFirst("sub")?.Value; // Assuming 'sub' claim contains user ID
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized("User ID not found.");
        }

        await _cosmosService.MarkProductsAsUsedAsync(userId, productIds);

        return Ok("Selected products have been marked as used.");
    }
}