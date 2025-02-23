using GroceryApp.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GroceryApp.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiptsController : ControllerBase
    {
        private readonly ICosmosService _cosmosService;

        public ReceiptsController(ICosmosService cosmosService)
        {
            _cosmosService = cosmosService;
        }

        /// <summary>
        /// Retrieves products expiring within the current week.
        /// </summary>
        /// <returns>List of expiring products.</returns>
        [HttpGet("expiring")]
        public async Task<ActionResult<List<ProductInfo>>> GetExpiringItems()
        {
            var today = DateTime.UtcNow.Date;
            var endOfWeek = today.AddDays(7);

            var expiringProducts = await _cosmosService.GetExpiringProductsAsync(today, endOfWeek);

            return Ok(expiringProducts);
        }

        /// <summary>
        /// Marks selected products as used.
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

            await _cosmosService.MarkProductsAsUsedAsync(productIds);

            return Ok("Selected products have been marked as used.");
        }
    }
}
