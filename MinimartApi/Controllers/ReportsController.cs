using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MinimartApi.Models;

namespace MinimartApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase {
        private readonly AppDbContext context;
        public ReportsController(AppDbContext context) {
            this.context = context;
        }

        [HttpGet("daily-sales")]
        public IActionResult GetDailySalesReport() {
            // Placeholder for daily sales report logic
            var report = new {
                Date = DateTime.UtcNow.Date,
                TotalSales = 1000.00,
                TotalTransactions = 50
            };
            return Ok(report);
        }

        [HttpGet("monthly-sales")]
        public IActionResult GetMonthlySalesReport() {
            // Placeholder for monthly sales report logic
            var report = new {
                Month = DateTime.UtcNow.Month,
                Year = DateTime.UtcNow.Year,
                TotalSales = 30000.00,
                TotalTransactions = 1500
            };
            return Ok(report);
        }

        [HttpGet("top-products")]
        public IActionResult GetTopSellingProductsReport() {
            // Placeholder for top-selling products report logic
            var report = new[] {
                new { ProductId = 1, ProductName = "Product A", UnitsSold = 150 },
                new { ProductId = 2, ProductName = "Product B", UnitsSold = 120 },
                new { ProductId = 3, ProductName = "Product C", UnitsSold = 100 }
            };
            return Ok(report);
        }

        [HttpGet("inventory-value")]
        public IActionResult GetInventoryValueReport() {
            // Placeholder for inventory value report logic
            var report = new {
                TotalInventoryValue = 50000.00,
                AverageInventoryValue = 250.00
            };
            return Ok(report);
        }

        [HttpGet("profit-margins")]
        public IActionResult GetProfitMarginsReport() {
            // Placeholder for profit margins report logic
            var report = new {
                TotalRevenue = 100000.00,
                TotalCost = 70000.00,
                ProfitMargin = 30.00
            };
            return Ok(report);
        }

        [HttpGet("inventory-status")]
        public IActionResult GetInventoryStatusReport() {
            // Placeholder for inventory status report logic
            var report = new {
                TotalProducts = 500,
                LowStockProducts = 25,
                OutOfStockProducts = 5
            };
            return Ok(report);
        }

        [HttpGet("customer-activity")]
        public IActionResult GetCustomerActivityReport() {
            // Placeholder for customer activity report logic
            var report = new {
                TotalCustomers = 2000,
                ActiveCustomers = 1500,
                NewCustomersThisMonth = 100
            };
            return Ok(report);
        }

        [HttpGet("purchase-order-status")]
        public IActionResult GetPurchaseOrderStatusReport() {
            // Placeholder for purchase order status report logic
            var report = new {
                TotalPurchaseOrders = 300,
                PendingOrders = 20,
                ReceivedOrders = 270,
                CancelledOrders = 10
            };
            return Ok(report);
        }

        [HttpGet("sales-by-product")]
        public IActionResult GetSalesByProductReport() {
            // Placeholder for sales by product report logic
            var report = new[] {
                new { ProductId = 1, ProductName = "Product A", TotalSales = 5000.00, UnitsSold = 100 },
                new { ProductId = 2, ProductName = "Product B", TotalSales = 3000.00, UnitsSold = 75 },
                new { ProductId = 3, ProductName = "Product C", TotalSales = 2000.00, UnitsSold = 50 }
            };
            return Ok(report);
        }

    }
}
