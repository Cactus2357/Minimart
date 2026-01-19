using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimartApi.Db.Models;
using MinimartApi.Dtos.Product;
using MinimartApi.Enums;
using System.Security.Claims;

namespace MinimartApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase {
        private readonly AppDbContext context;
        public ProductsController(AppDbContext context) {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllProducts() {
            var products = await context.Products
                .AsNoTracking()
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Select(p => new ProductResponse {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Categories = p.ProductCategories.Select(pc => pc.Category.Name).ToList()
                })
                .ToListAsync();
            return Ok(products);
        }

        [HttpGet("{productId}")]
        public async Task<IActionResult> GetProductById(int productId) {
            var product = await context.Products
                .AsNoTracking()
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Where(p => p.ProductId == productId)
                .Select(p => new ProductResponse {
                    ProductId = p.ProductId,
                    Name = p.Name,
                    Description = p.Description,
                    Categories = p.ProductCategories.Select(pc => pc.Category.Name).ToList()
                })
                .FirstOrDefaultAsync();
            if (product == null) {
                return NotFound(new { Message = "Product not found." });
            }
            return Ok(product);
        }

        [HttpGet("{productId}/variants")]
        public async Task<IActionResult> GetProductVariants(int productId) {
            var now = DateTime.UtcNow;

            var variants = await context.ProductVariants
                .AsNoTracking()
                .Include(v => v.PriceHistories)
                .Where(v => v.ProductId == productId)
                .Select(v => new {
                    VariantId = v.VariantId,
                    VariantName = v.VariantName,
                    SKU = v.SKU,
                    CurrentPrice = v.PriceHistories
                        .Where(ph => ph.EffectiveFrom <= now && (ph.EffectiveTo == null || ph.EffectiveTo > now))
                        .OrderByDescending(ph => ph.EffectiveFrom)
                        .Select(ph => new {
                            SalePrice = ph.SalePrice,
                            OriginalPrice = ph.OriginalPrice,
                            DiscountPercent = ph.DiscountPercent,
                            EffectiveTo = ph.EffectiveTo
                        })
                        .FirstOrDefault(),
                })
                .ToListAsync();

            return Ok(variants);
        }


        [HttpPost("products")]
        [Authorize(Roles = $"{Const.ROLE_ADMIN}, {Const.ROLE_STAFF}")]
        public async Task<IActionResult> CreateProduct([FromBody] ProductCreateRequest request) {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userId = userIdClaim != null && Guid.TryParse(userIdClaim, out Guid uid) ? uid : (Guid?)null;

            //TODO: status guard

            var product = new Product {
                Name = request.Name,
                Description = request.Description,
                Status = Const.PRODUCT_STATUS_PENDING,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId,
            };
            context.Products.Add(product);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProductById), new { productId = product.ProductId }, new { Message = "Product created successfully.", ProductId = product.ProductId });
        }

        [HttpPut("products/{productId}")]
        [Authorize(Roles = $"{Const.ROLE_ADMIN}, {Const.ROLE_STAFF}")]
        public async Task<IActionResult> UpdateProduct(int productId, [FromBody] ProductUpdateRequest request) {
            var product = await context.Products.FindAsync(productId);
            if (product == null) {
                return NotFound(new { Message = "Product not found." });
            }
            product.Name = request.Name ?? product.Name;
            product.Description = request.Description ?? product.Description;
            product.Status = Const.PRODUCT_STATUS_PENDING;
            product.UpdatedAt = DateTime.UtcNow;
            context.Products.Update(product);
            await context.SaveChangesAsync();
            return Ok(new { Message = "Product updated successfully." });
        }

        [HttpPost("products/{productId}/variants")]
        [Authorize(Roles = $"{Const.ROLE_ADMIN}, {Const.ROLE_STAFF}")]
        public async Task<IActionResult> CreateProductVariant(int productId, [FromBody] ProductVariantCreateRequest request) {
            var product = await context.Products.FindAsync(productId);
            if (product == null) {
                return NotFound(new { Message = "Product not found." });
            }
            var variant = new ProductVariant {
                ProductId = productId,
                VariantName = request.VariantName,
                SKU = request.SKU,
                Stock = request.StockQuantity,
                CreatedAt = DateTime.UtcNow
            };
            context.ProductVariants.Add(variant);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProductVariants), new { productId = productId }, new { Message = "Product variant created successfully.", VariantId = variant.VariantId });
        }

        [HttpPut("variants/{variantId}")]
        [Authorize(Roles = $"{Const.ROLE_ADMIN}, {Const.ROLE_STAFF}")]
        public async Task<IActionResult> UpdateProductVariant(int variantId, [FromBody] ProductVariantUpdateRequest request) {
            var variant = await context.ProductVariants.FindAsync(variantId);
            if (variant == null) {
                return NotFound(new { Message = "Product variant not found." });
            }
            variant.VariantName = request.VariantName ?? variant.VariantName;
            variant.SKU = request.SKU ?? variant.SKU;
            variant.Stock = request.StockQuantity ?? variant.Stock;
            context.ProductVariants.Update(variant);
            await context.SaveChangesAsync();
            return Ok(new { Message = "Product variant updated successfully." });
        }

        [HttpGet("variants/{variantId}/prices")]
        public async Task<IActionResult> GetProductVariantPriceHistory(int variantId) {
            var priceHistories = await context.PriceHistories
                .AsNoTracking()
                .Where(ph => ph.VariantId == variantId)
                .OrderByDescending(ph => ph.EffectiveFrom)
                .Select(ph => new {
                    ph.SalePrice,
                    ph.OriginalPrice,
                    ph.DiscountPercent,
                    ph.EffectiveFrom,
                    ph.EffectiveTo
                })
                .ToListAsync();
            if (priceHistories.Count == 0) {
                return NotFound(new { Message = "No price history found for the specified product variant." });
            }
            return Ok(priceHistories);
        }

        [HttpPost("variants/{variantId}/prices")]
        [Authorize(Roles = $"{Const.ROLE_ADMIN}, {Const.ROLE_STAFF}")]
        public async Task<IActionResult> CreateProductVariantPriceHistory(int variantId, [FromBody] PriceRequest request) {
            if (request.EffectiveTo != null && request.EffectiveTo <= request.EffectiveFrom) {
                ModelState.AddModelError(nameof(request.EffectiveTo), "EffectiveTo must be later than EffectiveFrom.");
                return BadRequest(ModelState);
            }

            var variant = await context.ProductVariants.FindAsync(variantId);
            if (variant == null) {
                return NotFound(new { Message = "Product variant not found." });
            }

            var priceHistory = new PriceHistory {
                VariantId = variantId,
                SalePrice = request.SalePrice,
                DiscountPercent = request.DiscountPercent,
                EffectiveFrom = request.EffectiveFrom,
                EffectiveTo = request.EffectiveTo
            };
            context.PriceHistories.Add(priceHistory);
            await context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetProductVariantPriceHistory), new { variantId = variantId }, new { Message = "Price history created successfully." });
        }

    }
}
