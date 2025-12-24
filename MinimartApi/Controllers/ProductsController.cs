using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimartApi.Models;

namespace MinimartApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase {
        private readonly AppDbContext context;

        public ProductsController(AppDbContext context) {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts(bool includeDeleted = false) {
            var q = context.Products.AsQueryable();

            if (includeDeleted && User.IsInRole(Role.Admin))
                q = q.IgnoreQueryFilters();

            var products = await q.ToListAsync();

            return Ok(products);
        }

        [HttpGet("productId")]
        public async Task<IActionResult> GetProductById(int productId) {
            var product = await context.Products.FindAsync(productId);
            if (product == null)
                return NotFound();
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] Product product) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            await context.Products.AddAsync(product);
            await context.SaveChangesAsync();
            return Ok(product);
        }

        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] Product updatedProduct) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var product = await context.Products.FindAsync(productId);
            if (product == null)
                return NotFound();
            product.Name = updatedProduct.Name;
            product.Description = updatedProduct.Description;
            product.Price = updatedProduct.Price;
            product.Stock = updatedProduct.Stock;
            product.CategoryId = updatedProduct.CategoryId;
            context.Products.Update(product);
            await context.SaveChangesAsync();
            return Ok(product);
        }

        [HttpDelete("{productId}")]
        public async Task<IActionResult> DeleteProduct(int productId) {
            var product = await context.Products.FindAsync(productId);
            if (product == null)
                return NotFound();
            context.Products.Remove(product);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string query) {
            if (string.IsNullOrWhiteSpace(query))
                return BadRequest(new { Message = "Query parameter cannot be empty." });
            var products = await context.Products
                .Where(p => p.Name.Contains(query) || p.Description.Contains(query))
                .ToListAsync();
            return Ok(products);
        }

        [HttpGet("category/{categoryId}")]
        public async Task<IActionResult> GetProductsByCategory(int categoryId) {
            var products = await context.Products
                .Where(p => p.CategoryId == categoryId)
                .ToListAsync();
            return Ok(products);
        }

        [HttpGet("low-stock")]
        public async Task<IActionResult> GetLowStockProducts([FromQuery] int threshold = 10) {
            var products = await context.Products
                .Include(p => p.Stock)
                .Where(p => p.Stock != null && p.Stock.Quantity <= threshold)
                .ToListAsync();
            return Ok(products);
        }

    }
}
