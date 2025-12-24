using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimartApi.Models;
using System.Security.AccessControl;

namespace MinimartApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase {
        private readonly AppDbContext context;
        public AdminController(AppDbContext context) {
            this.context = context;
        }

        //[HttpGet("audit-logs")]
        //public IActionResult GetAuditLogs() {
        //    var auditLogs = context.AuditLogs
        //        .OrderByDescending(log => log.Timestamp)
        //        .ToList();
        //    return Ok(auditLogs);
        //}

        [HttpGet("deleted-records")]
        public async Task<IActionResult> GetDeletedRecords() {
            var deletedCustomers = await context.Customers
                .IgnoreQueryFilters()
                .Where(c => c.IsDeleted)
                .ToListAsync();
            var deletedProducts = await context.Products
                .Where(p => p.IsDeleted)
                .ToListAsync();
            var result = new {
                DeletedCustomers = deletedCustomers,
                DeletedProducts = deletedProducts,
            };
            return Ok(result);
        }

        [HttpGet("stats")]
        public IActionResult GetStats() {
            var totalCustomers = context.Customers.Count();
            var totalProducts = context.Products.Count();
            var totalSales = context.Sales.Count();
            var totalPurchaseOrders = context.PurchaseOrders.Count();
            var stats = new {
                TotalCustomers = totalCustomers,
                TotalProducts = totalProducts,
                TotalSales = totalSales,
                TotalPurchaseOrders = totalPurchaseOrders
            };
            return Ok(stats);
        }
    }
}
