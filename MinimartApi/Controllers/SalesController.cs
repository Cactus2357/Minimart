using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimartApi.Models;

namespace MinimartApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class SalesController : ControllerBase {
        private readonly AppDbContext context;
        public SalesController(AppDbContext context) {
            this.context = context;
        }

        [HttpPost]
        public async Task<IActionResult> CreateSale([FromBody] Sale sale) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            sale.SaleDate = DateTime.UtcNow;
            await context.Sales.AddAsync(sale);
            await context.SaveChangesAsync();
            return Ok(sale);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSales() {
            var sales = await context.Sales.ToListAsync();
            return Ok(sales);
        }

        [HttpGet("{saleId}")]
        public async Task<IActionResult> GetSaleById(int saleId) {
            var sale = await context.Sales.FindAsync(saleId);
            if (sale == null)
                return NotFound();
            return Ok(sale);
        }

        [HttpPost("{saleId}/refund")]
        public async Task<IActionResult> RefundSale(int saleId) {
            var sale = await context.Sales.FindAsync(saleId);
            if (sale == null)
                return NotFound();
            if (sale.Status == "REFUNDED")
                return BadRequest(new { Message = "Sale has already been refunded." });
            sale.Status = "REFUNDED";
            sale.RefundDate = DateTime.UtcNow;
            context.Sales.Update(sale);
            await context.SaveChangesAsync();
            return Ok(sale);
        }


    }
}
