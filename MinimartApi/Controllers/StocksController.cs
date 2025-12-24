using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimartApi.Models;

namespace MinimartApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Role.Admin)]
    public class StocksController : ControllerBase {
        private readonly AppDbContext context;
        public StocksController(AppDbContext context) {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllStocks() {
            var stocks = await context.Stocks.ToListAsync();
            return Ok(stocks);
        }

        [HttpGet("{stockId}")]
        public async Task<IActionResult> GetStockById(int stockId) {
            var stock = await context.Stocks.FindAsync(stockId);
            if (stock == null)
                return NotFound();
            return Ok(stock);
        }

        [HttpPost("adjust")]
        public async Task<IActionResult> AdjustStock([FromBody] Stock adjustment) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var product = await context.Products
                .Include(p => p.Stock)
                .FirstOrDefaultAsync(p => p.ProductId == adjustment.ProductId);
            if (product == null)
                return NotFound(new { Message = "Product not found." });
            if (product.Stock == null) {
                product.Stock = new Stock {
                    ProductId = product.ProductId,
                    Quantity = 0
                };
                context.Stocks.Add(product.Stock);
            }
            product.Stock.Quantity += adjustment.Quantity;
            context.Products.Update(product);
            await context.SaveChangesAsync();
            return Ok(product);
        }

    }
}
