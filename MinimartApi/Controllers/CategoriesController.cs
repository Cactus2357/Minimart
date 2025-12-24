using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimartApi.Dtos;
using MinimartApi.Models;

namespace MinimartApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase {
        private readonly AppDbContext context;
        public CategoriesController(AppDbContext context) {
            this.context = context;
        }

        /***
         * TODO: fix all controllers: Admin, Customers, Products, PurchaseOrders, Reports, Roles, Sales, Stocks, Suppliers, Categories
         ***/

        [HttpGet]
        public async Task<IActionResult> GetAllCategoriesAdmin(bool includeDeleted = false) {
            var q = context.Categories.AsQueryable();

            var isAdmin = User.IsInRole(Role.Admin);

            if (includeDeleted && isAdmin)
                q = q.IgnoreQueryFilters();

            var categories = await q.ToListAsync();

            return Ok(categories);
        }

        //[HttpGet]
        //public async Task<IActionResult> GetAllCategories() {
        //    var categories = await context.Categories.ToListAsync();
        //    return Ok(categories);
        //}

        [HttpGet("{id}")] 
        public async Task<IActionResult> GetCategoryById(int id) {
            var category = await context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            return Ok(category);
        }

        [HttpPost]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var categoryName = request.Name.Trim();

            if (await context.Categories.AnyAsync(c => c.Name.ToLower() == categoryName.ToLower())) {
                ModelState.AddModelError(nameof(request.Name), "A category with the same name already exists.");
                return BadRequest(ModelState);
            }

            var category = new Category {
                Name = categoryName,
                Description = request.Description?.Trim() ?? string.Empty
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoryById), new { id = category.CategoryId }, category);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request) {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);
            var category = await context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();
            var categoryName = request.Name.Trim();

            if (await context.Categories.AnyAsync(c => c.CategoryId != id && c.Name.ToLower() == categoryName.ToLower())) {
                ModelState.AddModelError(nameof(request.Name), "A category with the same name already exists.");
                return BadRequest(ModelState);
            }

            category.Name = categoryName;
            category.Description = request.Description?.Trim() ?? string.Empty;

            context.Categories.Update(category);
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = Role.Admin)]
        public async Task<IActionResult> DeleteCategory(int id) {
            var category = await context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            context.Categories.Remove(category);
            await context.SaveChangesAsync();
            return NoContent();
        }

        //[HttpPatch("{id}/restore")]
        //[Authorize(Roles = Role.Admin)]
        //public async Task<IActionResult> RestoreCategory(int id) {
        //    var category = await context.Categories.IgnoreQueryFilters().FirstOrDefaultAsync(c => c.CategoryId == id);
        //    if (category == null)
        //        return NotFound();

        //    if (!category.IsDeleted) {
        //        return BadRequest(new { Message = "Category is not deleted." });
        //    }

        //    var existCategory = await context.Categories
        //        .FirstOrDefaultAsync(c => c.Name.ToLower() == category.Name.ToLower());

        //    if (existCategory != null) {
        //        return Ok(existCategory);
        //    }

        //    category.IsDeleted = false;
        //    category.DeletedAt = null;

        //    context.Categories.Update(category);
        //    await context.SaveChangesAsync();

        //    return Ok(category);
        //}
    }
}
