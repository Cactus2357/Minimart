using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimartApi.Db.Models;
using MinimartApi.Dtos.Order;
using MinimartApi.Enums;
using System.Security.Claims;

namespace MinimartApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext context;
        public OrdersController(AppDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllOrders()
        {
            var orders = await context.Orders
                .AsNoTracking()
                .ToListAsync();
            return Ok(orders);
        }

        [HttpPost]
        [Authorize(Roles = Const.ROLE_CUSTOMER)]
        public async Task<IActionResult> CreateOrder([FromBody] OrderCreateRequest request)
        {

            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.NewGuid().ToString()
            );

            await using var transaction = await context.Database.BeginTransactionAsync();

            try
            {
                var variantIds = request.Items.Select(i => i.VariantId).ToList();

                var variants = await context.ProductVariants
                    //.Include(v => v.PriceHistories.Where(p => p.EffectiveTo == null))
                    .Where(v => variantIds.Contains(v.VariantId))
                    .ToListAsync();

                if (variants.Count != request.Items.Count)
                    return BadRequest("Invalid product variant");

                foreach (var item in request.Items)
                {
                    var variant = variants.First(v => v.VariantId == item.VariantId);
                    if (variant.Stock < item.Quantity)
                        //return BadRequest($"Insufficient stock for {variant.SKU}");
                        throw new InvalidOperationException($"Insufficient stock for {variant.SKU}");
                }

                var order = new Order
                {
                    CustomerId = userId,
                    AddressId = request.AddressId,
                    CurrentStatus = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    TotalAmount = 0,
                    Items = new List<OrderItem>()
                };

                decimal totalAmount = 0;

                foreach (var item in request.Items)
                {
                    var variant = variants.First(v => v.VariantId == item.VariantId);
                    var price = variant.PriceHistories.First();

                    var unitPrice = price.SalePrice > 0
                        ? (price.SalePrice ?? price.OriginalPrice)
                        : price.OriginalPrice;

                    totalAmount += unitPrice * item.Quantity;

                    order.Items.Add(new OrderItem
                    {
                        VariantId = item.VariantId,
                        Quantity = item.Quantity,
                        UnitPrice = unitPrice
                    });

                    variant.Stock -= item.Quantity;
                }

                order.TotalAmount = totalAmount;

                context.Orders.Add(order);

                context.OrderStatusHistories.Add(new OrderStatusHistory
                {
                    Order = order,
                    OldStatus = OrderStatus.Pending,
                    NewStatus = OrderStatus.Pending,
                    ChangedAt = DateTime.UtcNow,
                    ChangedBy = userId
                });

                await context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    order.OrderId,
                    order.TotalAmount,
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("me")]
        [Authorize(Roles = Const.ROLE_CUSTOMER)]
        public async Task<IActionResult> GetOrdersByCustomer()
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.NewGuid().ToString()
            );
            var orders = await context.Orders
                .AsNoTracking()
                .Where(o => o.CustomerId == userId)
                .ToListAsync();
            return Ok(orders);
        }

        [HttpGet("{orderId}")]
        [Authorize(Roles = Const.ROLE_CUSTOMER)]
        public async Task<IActionResult> GetOrderById(int orderId)
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.NewGuid().ToString()
            );
            var order = await context.Orders
                .AsNoTracking()
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == userId);
            if (order == null)
                return NotFound(new { Message = "Order not found." });
            return Ok(order);
        }

    }
}
