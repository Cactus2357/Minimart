using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimartApi.Models;

namespace MinimartApi.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase {
        private readonly AppDbContext context;
        public CategoriesController(AppDbContext context) {
            this.context = context;
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllCategories() {
            var categories = await context.Categories.ToListAsync();
            return Ok(categories);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCategoryById(int id) {
            var category = await context.Categories.FindAsync(id);
            if (category == null)
                return NotFound();

            return Ok(category);
        }
    }
}
