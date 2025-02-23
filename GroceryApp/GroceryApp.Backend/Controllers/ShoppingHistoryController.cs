using GroceryApp.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GroceryApp.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShoppingHistoryController : ControllerBase
{
    private readonly ICosmosService _cosmosService;

    public ShoppingHistoryController(ICosmosService cosmosService)
    {
        _cosmosService = cosmosService;
    }

    /// <summary>
    /// Retrieves all past purchases.
    /// </summary>
    /// <returns>List of all purchases grouped by user.</returns>
    [HttpGet]
    public async Task<ActionResult<List<PurchasedItem>>> GetShoppingHistory()
    {
        var shoppingHistory = await _cosmosService.GetAllPurchasesAsync();
        return Ok(shoppingHistory);
    }
}
