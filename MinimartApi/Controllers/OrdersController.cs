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

        //[HttpGet]
        //public async Task<IActionResult> GetAllOrders()
        //{
        //    var orders = await context.Orders
        //        .AsNoTracking()
        //        .ToListAsync();
        //    return Ok(orders);
        //}

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
                    CurrentStatus = Const.ORDER_STATUS_PENDING,
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
                    OldStatus = Const.ORDER_STATUS_PENDING,
                    NewStatus = Const.ORDER_STATUS_PENDING,
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

        [HttpGet]
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

        [HttpPost("{orderId}/cancel")]
        [Authorize(Roles = Const.ROLE_CUSTOMER)]
        public async Task<IActionResult> CancelOrder(int orderId)
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.NewGuid().ToString()
            );
            var order = await context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.OrderId == orderId && o.CustomerId == userId);
            if (order == null)
                return NotFound(new { Message = "Order not found." });
            if (order.CurrentStatus != Const.ORDER_STATUS_PENDING)
                return BadRequest(new { Message = "Only pending orders can be canceled." });
            order.CurrentStatus = Const.ORDER_STATUS_CANCELLED;
            context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                Order = order,
                OldStatus = Const.ORDER_STATUS_PENDING,
                NewStatus = Const.ORDER_STATUS_CANCELLED,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = userId
            });
            foreach (var item in order.Items)
            {
                var variant = await context.ProductVariants.FindAsync(item.VariantId);
                if (variant != null)
                {
                    variant.Stock += item.Quantity;
                }
            }
            await context.SaveChangesAsync();
            return Ok(new { Message = "Order canceled successfully." });

        }

        [HttpPut("{orderId}/status")]
        [Authorize(Roles = Const.ROLE_STAFF)]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, [FromBody] OrderStatusUpdateRequest request)
        {
            var userId = Guid.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? Guid.NewGuid().ToString()
            );
            //var newStatus = 
            var order = await context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);
            if (order == null)
                return NotFound(new { Message = "Order not found." });
            var oldStatus = order.CurrentStatus;
            order.CurrentStatus = request.NewStatus;
            context.OrderStatusHistories.Add(new OrderStatusHistory
            {
                Order = order,
                OldStatus = oldStatus,
                NewStatus = request.NewStatus,
                ChangedAt = DateTime.UtcNow,
                ChangedBy = userId
            });
            await context.SaveChangesAsync();
            return Ok(new { Message = "Order status updated successfully." });
        }



        ////
    }
}
