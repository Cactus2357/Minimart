using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimartApi.Models;

namespace MinimartApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase {
        private readonly AppDbContext context;
        public CustomersController(AppDbContext context) {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCustomers() {
            var customers = await context.Customers.ToListAsync();
            return Ok(customers);
        }

        [HttpGet("{customerId}")]
        public async Task<IActionResult> GetCustomerById(int customerId) {
            var customer = await context.Customers.FindAsync(customerId);
            if (customer == null)
                return NotFound();
            return Ok(customer);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] Customer customer) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            await context.Customers.AddAsync(customer);
            await context.SaveChangesAsync();
            return Ok(customer);
        }

        [HttpPut("{customerId}")]
        public async Task<IActionResult> UpdateCustomer(int customerId, [FromBody] Customer updatedCustomer) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var customer = await context.Customers.FindAsync(customerId);
            if (customer == null)
                return NotFound();
            customer.Name = updatedCustomer.Name;
            customer.Phone = updatedCustomer.Phone;
            context.Customers.Update(customer);
            await context.SaveChangesAsync();
            return Ok(customer);
        }

    }
}
