using GroceryApp.Backend.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GroceryApp.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ShoppingHistoryController : ControllerBase
    {
        private readonly ICosmosService _cosmosService;

        public ShoppingHistoryController(ICosmosService cosmosService)
        {
            _cosmosService = cosmosService;
        }

        /// <summary>
        /// Retrieves all past purchases for the authenticated user.
        /// </summary>
        /// <returns>List of all purchases grouped by user.</returns>
        [HttpGet]
        public async Task<ActionResult<List<PurchasedItem>>> GetShoppingHistory()
        {
            var userId = User.FindFirst("sub")?.Value; // Assuming 'sub' claim contains user ID
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found.");
            }

            var shoppingHistory = await _cosmosService.GetAllPurchasesAsync(userId);
            return Ok(shoppingHistory);
        }
    }
}
