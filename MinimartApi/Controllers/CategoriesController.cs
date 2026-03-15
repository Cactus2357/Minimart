using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MinimartApi.Db.Models;
using MinimartApi.Dtos.Category;
using MinimartApi.Enums;
using System.Collections;

namespace MinimartApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext context;

        public CategoriesController(AppDbContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCategories()
        {
            var categories = await context.Categories
                .AsNoTracking()
                .Select(c => new CategoryResponse
                {
                    CategoryId = c.CategoryId,
                    Name = c.Name,
                    ParentCategoryId = c.ParentCategoryId
                })
                .ToListAsync();
            return Ok(categories);
        }

        [HttpGet("{categoryId}")]
        public async Task<IActionResult> GetCategoryById(int categoryId)
        {
            var categories = await context.Categories
                .AsNoTracking()
                .Where(c => c.CategoryId == categoryId || c.ParentCategoryId == categoryId)
                .ToListAsync();

            var category = categories.FirstOrDefault(c => c.CategoryId == categoryId);

            if (category == null)
            {
                return NotFound(new { Message = "Category not found." });
            }

            var response = new
            {
                CategoryId = categoryId,
                Name = category.Name,
                Children = categories
                    .Where(c => c.ParentCategoryId == category.CategoryId)
                    .Select(c => new
                    {
                        CategoryId = c.CategoryId,
                        Name = c.Name
                    })
                    .ToList()
            };

            return Ok(response);
        }

        [HttpPost]
        [Authorize(Roles = $"{Const.ROLE_ADMIN}, {Const.ROLE_STAFF}")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateRequest request)
        {
            if (request.ParentCategoryId.HasValue && await context.Categories.FindAsync(request.ParentCategoryId.Value) == null)
            {
                ModelState.AddModelError(nameof(request.ParentCategoryId), "Parent category does not exist.");
                return BadRequest(ModelState);
            }

            var category = new Category
            {
                Name = request.Name.Trim(),
                ParentCategoryId = request.ParentCategoryId
            };

            context.Categories.Add(category);
            await context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCategoryById), new { categoryId = category.CategoryId }, new
            {
                category.CategoryId,
                category.Name,
                category.ParentCategoryId
            });
        }

        [HttpPut("{categoryId}")]
        [Authorize(Roles = $"{Const.ROLE_ADMIN}, {Const.ROLE_STAFF}")]
        public async Task<IActionResult> UpdateCategory(int categoryId, [FromBody] CategoryUpdateRequest request)
        {
            var category = await context.Categories.FindAsync(categoryId);
            if (category == null)
            {
                return NotFound(new { Message = "Category not found." });
            }
            if (request.ParentCategoryId.HasValue && await context.Categories.FindAsync(request.ParentCategoryId.Value) == null)
            {
                ModelState.AddModelError(nameof(request.ParentCategoryId), "Parent category does not exist.");
                return BadRequest(ModelState);
            }
            category.Name = request.Name.Trim();
            category.ParentCategoryId = request.ParentCategoryId;
            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{categoryId}")]
        [Authorize(Roles = $"{Const.ROLE_ADMIN}, {Const.ROLE_STAFF}")]
        public async Task<IActionResult> DeleteCategory(int categoryId)
        {
            var category = await context.Categories
                .Include(c => c.Children)
                .Include(c => c.ProductCategories)
                .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

            if (category == null)
            {
                return NotFound(new { Message = "Category not found." });
            }

            category.IsDeleted = true;
            category.UpdatedAt = DateTime.UtcNow;

            foreach (var child in category.Children)
            {
                child.IsDeleted = true;
                child.UpdatedAt = DateTime.UtcNow;
            }

            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("tree")]
        [ResponseCache(Duration = 3600)]
        public async Task<IActionResult> GetCategoryTree()
        {
            var categories = await context.Categories
                .AsNoTracking()
                .ToListAsync();

            var categoryDict = categories.ToDictionary(c => c.CategoryId, c => new SingleCategoryResponse
            {
                CategoryId = c.CategoryId,
                Name = c.Name,
                Children = new ArrayList()
            });

            foreach (var category in categories)
            {
                if (category.ParentCategoryId.HasValue && categoryDict.ContainsKey(category.ParentCategoryId.Value))
                {
                    categoryDict[category.ParentCategoryId.Value].Children.Add(categoryDict[category.CategoryId]);
                }
            }
            var rootCategories = categoryDict.Values
                .Where(c => !categories.First(cat => cat.CategoryId == c.CategoryId).ParentCategoryId.HasValue)
                .ToList();

            return Ok(rootCategories);
        }
    }
}
