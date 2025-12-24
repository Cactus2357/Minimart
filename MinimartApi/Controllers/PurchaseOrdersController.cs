
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimartApi.Models;

namespace MinimartApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class PurchaseOrdersController : ControllerBase {
        private readonly AppDbContext context;
        public PurchaseOrdersController(AppDbContext context) {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPurchaseOrders() {
            var purchaseOrders = await context.PurchaseOrders
                .OrderDescending()
                .ToListAsync();

            return Ok(purchaseOrders);
        }

        [HttpGet("{purchaseOrderId}")]
        public async Task<IActionResult> GetPurchaseOrderById(int purchaseOrderId) {
            var purchaseOrder = await context.PurchaseOrders.FindAsync(purchaseOrderId);
            if (purchaseOrder == null)
                return NotFound();
            return Ok(purchaseOrder);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePurchaseOrder([FromBody] PurchaseOrder purchaseOrder) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            purchaseOrder.OrderDate = DateTime.UtcNow;
            await context.PurchaseOrders.AddAsync(purchaseOrder);
            await context.SaveChangesAsync();
            return Ok(purchaseOrder);
        }

        [HttpPost("{purchaseOrderId}/receive")]
        public async Task<IActionResult> ReceivePurchaseOrder(int purchaseOrderId) {
            var purchaseOrder = await context.PurchaseOrders.FindAsync(purchaseOrderId);
            if (purchaseOrder == null)
                return NotFound();
            if (purchaseOrder.Status == "RECEIVED")
                return BadRequest(new { Message = "Purchase order has already been received." });
            purchaseOrder.Status = "RECEIVED";
            purchaseOrder.ReceivedDate = DateTime.UtcNow;
            context.PurchaseOrders.Update(purchaseOrder);
            await context.SaveChangesAsync();
            return Ok(purchaseOrder);
        }

        [HttpPut("{purchaseOrderId}")]
        public async Task<IActionResult> UpdatePurchaseOrder(int purchaseOrderId, [FromBody] PurchaseOrder updatedPurchaseOrder) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var purchaseOrder = await context.PurchaseOrders.FindAsync(purchaseOrderId);
            if (purchaseOrder == null)
                return NotFound();
            purchaseOrder.SupplierId = updatedPurchaseOrder.SupplierId;
            purchaseOrder.TotalAmount = updatedPurchaseOrder.TotalAmount;
            context.PurchaseOrders.Update(purchaseOrder);
            await context.SaveChangesAsync();
            return Ok(purchaseOrder);
        }

        [HttpDelete("{purchaseOrderId}")]
        public async Task<IActionResult> DeletePurchaseOrder(int purchaseOrderId) {
            var purchaseOrder = await context.PurchaseOrders.FindAsync(purchaseOrderId);
            if (purchaseOrder == null)
                return NotFound();
            context.PurchaseOrders.Remove(purchaseOrder);
            await context.SaveChangesAsync();
            return Ok(new { Message = "Purchase order deleted successfully." });
        }


    }
}
