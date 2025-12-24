using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimartApi.Models;

namespace MinimartApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = Role.Admin)]
    public class SuppliersController : ControllerBase {
        private readonly AppDbContext context;
        public SuppliersController(AppDbContext context) {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSuppliers() {
            var suppliers = await context.Suppliers.ToListAsync();
            return Ok(suppliers);
        }

        [HttpGet("{supplierId}")]
        public async Task<IActionResult> GetSupplierById(int supplierId) {
            var supplier = await context.Suppliers.FindAsync(supplierId);
            if (supplier == null)
                return NotFound();
            return Ok(supplier);
        }

        [HttpPost]
        public async Task<IActionResult> CreateSupplier([FromBody] Supplier supplier) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            await context.Suppliers.AddAsync(supplier);
            await context.SaveChangesAsync();
            return Ok(supplier);
        }

        [HttpPut("{supplierId}")]
        public async Task<IActionResult> UpdateSupplier(int supplierId, [FromBody] Supplier updatedSupplier) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var supplier = await context.Suppliers.FindAsync(supplierId);
            if (supplier == null)
                return NotFound();

            supplier.Name = updatedSupplier.Name;
            supplier.Phone = updatedSupplier.Phone;
            supplier.Email = updatedSupplier.Email;
            supplier.Address = updatedSupplier.Address;

            context.Suppliers.Update(supplier);
            await context.SaveChangesAsync();
            return Ok(supplier);
        }

        [HttpDelete("{supplierId}")]
        public async Task<IActionResult> DeleteSupplier(int supplierId) {
            var supplier = await context.Suppliers.FindAsync(supplierId);
            if (supplier == null)
                return NotFound();
            context.Suppliers.Remove(supplier);
            await context.SaveChangesAsync();
            return NoContent();
        }

    }
}
